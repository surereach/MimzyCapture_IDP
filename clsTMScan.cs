using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Dynamsoft.TWAIN.Interface;
using System.Windows.Forms;
using System.Drawing;
using Dynamsoft.Core;

using IronBarCode;
using IronSoftware.Drawing;
using IronPdf;

using ImageInfoJson;

namespace SRDocScanIDP
{
    public class clsTMScan: Form, IAcquireCallback, IDisposable
    {
        Form oForm = null;
        private ImageCore _oImgScan;
        public string sErrMsg;
        private delegate void CrossThreadOperationControl();

        private bool bScanBarcode;
        private string sDocType;
        private System.Drawing.Bitmap oBmp;

        private string sFrontBack;

        public bool ScanBarcodeOnly
        {
            get { return bScanBarcode; }
            set { bScanBarcode = value; }
        }

        public string DocType
        {
            get { return sDocType; }
        }

        public ImageCore ImageScan
        {
            get { return _oImgScan; }
            set { _oImgScan = value; }
        }

        public System.Drawing.Bitmap ScanBmp
        {
            get { return oBmp; }
            set { oBmp = value; }
        }

        public string FrontBack
        {
            get { return sFrontBack; }
            set { sFrontBack = value; }
        }

        public clsTMScan()
        {
            sDocType = "";
            sErrMsg = "";
            this.oForm = null;
            IronBarCode.License.LicenseKey = MDIMain.strIronBcLic;
            IronPdf.License.LicenseKey = MDIMain.strIronPdfLic;
        }

        public clsTMScan(Form pForm)
        {
            sDocType = "";
            sErrMsg = "";
            this.oForm = pForm;
            IronBarCode.License.LicenseKey = MDIMain.strIronBcLic;
            IronPdf.License.LicenseKey = MDIMain.strIronPdfLic;
        }

        #region AcquireImage Callback
        public void OnPostAllTransfers()
        {
            CrossThreadOperationControl crossDelegate = delegate ()
            {
                try
                {
                    //mGlobal.Write2Log("Post all.");
                    return;
                }
                catch (Exception ex)
                {
                    mGlobal.Write2Log(ex.Message);
                }
            };

            oForm.Invoke(crossDelegate);
        }

        public bool OnPostTransfer(Bitmap bit, string info)
        {
            try
            {
                var oImgInfo = clsImgInfo.FromJson(info);

                if (oImgInfo.ExtendedImageInfo.Others.TweiPageside == 2)
                    sFrontBack = "B";
                else
                    sFrontBack = "F";

                oBmp = bit;
                _oImgScan = new ImageCore();

                //mGlobal.Write2Log(info);
                _oImgScan.IO.LoadImage(bit);

                if (bScanBarcode) //Scan barcode only.
                {
                    bool isSep = ReadFromImageBarcode();

                    if (!isSep)
                    {
                        sErrMsg = "Image scanned is not document separator! Barcode not expected.";
                        OnTransferCancelled();
                        return false;
                    }
                }

                this.ImageScan = _oImgScan;

                sErrMsg = "";
                OnTransferCancelled();
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log("Post an image.." + ex.Message);
                return false;
            }

            return true; //Scan only once at a time.
        }

        public void OnPreAllTransfers()
        {
        }

        public bool OnPreTransfer()
        {
            return true;
        }

        public void OnSourceUIClose()
        {
        }

        public void OnTransferCancelled()
        {
            try
            {
                if (sErrMsg.Trim() != "")
                {
                    MessageBox.Show(this, "Scan process stopped with error! " + Environment.NewLine + sErrMsg, "Process Cancel");
                    sErrMsg = "";
                }
                return;
            }
            catch (Exception ex)
            {
            }
        }

        public void OnTransferError()
        {
            MessageBox.Show(this, "Error happen while in scanning..", "Error");
        }

        public bool IfGetImageInfo
        {
            get
            {
                return true;
            }
        }

        public bool IfGetExtImageInfo
        {
            get
            {
                return true;
            }
        }
        #endregion AcquireImage Callback

        public new void Dispose()
        {
            if (_oImgScan != null) _oImgScan.Dispose();
            if (oBmp != null) oBmp.Dispose();

            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool ReadFromImageBarcode()
        {
            bool isDocSep = false;

            Bitmap bmp = null;
            try
            {
                if (_oImgScan.ImageBuffer.CurrentImageIndexInBuffer >= 0)
                {
                    bmp = (Bitmap)(_oImgScan.ImageBuffer.GetBitmap(_oImgScan.ImageBuffer.CurrentImageIndexInBuffer));

                    // To set more options and optimization with your Barcode Reading,
                    // Please utilize the BarcodeReaderOptions paramter of read:
                    var codeOptions = new BarcodeReaderOptions
                    {
                        // Choose a speed from: Faster, Balanced, Detailed, ExtremeDetail
                        // There is a tradeoff in performance as more Detail is set
                        Speed = ReadingSpeed.Balanced,

                        // Reader will stop scanning once a barcode is found, unless set to true
                        ExpectMultipleBarcodes = false,

                        // By default, all barcode formats are scanned for.
                        // Specifying one or more, performance will increase.
                        ExpectBarcodeTypes = BarcodeEncoding.AllOneDimensional, //BarcodeEncoding.AllOneDimensional, //BarcodeEncoding.Code128,

                        // Utilizes multiple threads to reads barcodes from multiple images in parallel.
                        Multithreaded = false,

                        // Maximum threads for parallel. Default is 4
                        MaxParallelThreads = 4,

                        // The area of each image frame in which to scan for barcodes.
                        // Will improve performance significantly and avoid unwanted results and avoid noisy parts of the image.
                        CropArea = new IronSoftware.Drawing.Rectangle(),

                        // Special Setting for Code39 Barcodes.
                        // If a Code39 barcode is detected. Try to use extended mode for the full ASCII Character Set
                        UseCode39ExtendedMode = true
                    };

                    System.Threading.Tasks.Task<BarcodeResults> textResults = IronBarCode.BarcodeReader.ReadAsync(bmp, codeOptions);
                    BarcodeResult[] results = textResults.Result.ToArray();

                    if (results.Length > 0)
                    {
                        int i = 0;
                        sDocType = "";

                        while (i < results.Length)
                        {
                            sDocType = results[i].Text;

                            i += 1;
                        }
                        isDocSep = true;
                    }
                    textResults = null;
                    results = null;

                }
            }
            catch (Exception exp)
            {
                mGlobal.Write2Log(exp.Message);
            }
            finally
            {
                if (bmp != null)
                    bmp.Dispose();
            }

            return isDocSep;
        }

        private bool ReadFromPdfBarcode(string pFilename)
        {
            bool isDocSep = false;
            try
            {
                // To set more options and optimization with your Barcode Reading,
                // Please utilize the BarcodeReaderOptions paramter of read:
                var codeOptions = new PdfBarcodeReaderOptions
                {
                    // Choose a speed from: Faster, Balanced, Detailed, ExtremeDetail
                    // There is a tradeoff in performance as more Detail is set
                    Speed = ReadingSpeed.Balanced,

                    // Reader will stop scanning once a barcode is found, unless set to true
                    ExpectMultipleBarcodes = false,

                    // By default, all barcode formats are scanned for.
                    // Specifying one or more, performance will increase.
                    ExpectBarcodeTypes = BarcodeEncoding.AllOneDimensional, //BarcodeEncoding.AllOneDimensional, //BarcodeEncoding.Code128,

                    // Utilizes multiple threads to reads barcodes from multiple images in parallel.
                    Multithreaded = false,

                    // Maximum threads for parallel. Default is 4
                    MaxParallelThreads = 4,

                    // The area of each image frame in which to scan for barcodes.
                    // Will improve performance significantly and avoid unwanted results and avoid noisy parts of the image.
                    CropArea = new IronSoftware.Drawing.Rectangle(),

                    // Special Setting for Code39 Barcodes.
                    // If a Code39 barcode is detected. Try to use extended mode for the full ASCII Character Set
                    UseCode39ExtendedMode = true
                };

                List<string> lPdfs = new List<string>();
                lPdfs.Add(pFilename);

                BarcodeResults textResults = IronBarCode.BarcodeReader.ReadPdfAsync(lPdfs, codeOptions);
                BarcodeResult[] results = textResults.ToArray();

                if (results.Length > 0)
                {
                    int i = 0;
                    sDocType = "";

                    while (i < results.Length)
                    {
                        sDocType = results[i].Text;

                        i += 1;
                    }
                    isDocSep = true;
                }
                textResults = null;
                results = null;
            }
            catch (Exception exp)
            {
                mGlobal.Write2Log(exp.Message);
            }

            return isDocSep;
        }

        public bool ReadFromImageBarcode(string pFilename)
        {
            try
            {
                if (_oImgScan == null) _oImgScan = new ImageCore();

                System.IO.FileInfo oFi = new System.IO.FileInfo(pFilename);
                if (oFi.Extension.ToLower() == ".pdf")
                {
                    Byte[] oBytes = null;
                    List<int> lPages = new List<int>();
                    lPages.Add(0);
                    AnyBitmap[] aBmp = PdfDocument.FromFile(pFilename).ToBitmap(lPages, 300);
                    oBmp = aBmp[0];

                    using (MemoryStream oMem = new MemoryStream())
                    {
                        oBmp.Save(oMem, System.Drawing.Imaging.ImageFormat.Tiff);
                        oBytes = oMem.ToArray();
                    }

                    _oImgScan.IO.LoadImageFromBytes(oBytes, Dynamsoft.Core.Enums.EnumImageFileFormat.WEBTW_TIF);

                    return ReadFromPdfBarcode(pFilename);
                }
                else
                {
                    if (oBmp == null) oBmp = new Bitmap(pFilename);
                    _oImgScan.IO.LoadImage(pFilename);

                    return ReadFromImageBarcode();
                }
            }
            catch (Exception ex)
            {
                sErrMsg = "Read barcode:" + ex.Message;
                mGlobal.Write2Log(sErrMsg);
            }
            return false;
        }


    }


}
