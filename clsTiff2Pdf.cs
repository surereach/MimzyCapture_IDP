using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using PdfSharp.Drawing;

//using Syncfusion.Compression.Zip;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Parsing;
using Syncfusion.OCRProcessor;

using IronOcr;
using IronPdf;

namespace Tiff2PdfConverter
{
    public class clsTiffSplitter
    {
        private string _IronLic;
        private static int iTimeout;
        private static int iDelay;

        public clsTiffSplitter(string pIronLic)
        {
            _IronLic = pIronLic;
            iTimeout = Convert.ToInt32(SRDocScanIDP.mGlobal.GetAppCfg("2PDFTimeout"));
            iDelay = Convert.ToInt32(SRDocScanIDP.mGlobal.GetAppCfg("2PDFDelay"));
        }

        //Retrive PageCount of a multi-page tiff image
        public int getPageCount(String fileName)
        {
            int pageCount = -1;
            try
            {
                Image img = Bitmap.FromFile(fileName);
                pageCount = img.GetFrameCount(FrameDimension.Page);
                img.Dispose();
            }
            catch (Exception ex)
            {
                pageCount = 0;
            }
            return pageCount;
        }

        public int getPageCount(Image img)
        {
            int pageCount = -1;
            try
            {
                pageCount = img.GetFrameCount(FrameDimension.Page);
            }
            catch (Exception ex)
            {
                pageCount = 0;                
            }
            return pageCount;
        }

        //Retrive a specific Page from a multi-page tiff image
        private Image getTiffImage(String sourceFile, int pageNumber)
        {
            Image returnImage = null;

            try
            {
                Image sourceIamge = Bitmap.FromFile(sourceFile);
                returnImage = getTiffImage(sourceIamge, pageNumber);
                sourceIamge.Dispose();
            }
            catch (Exception ex)
            {                
                returnImage = null;                
            }

            //String splittedImageSavePath = "X:\\CJT\\CJT-Docs\\CJT-Images\\result001.tif";
            //returnImage.Save(splittedImageSavePath);

            return returnImage;
        }

        private Image getTiffImage(Image sourceImage, int pageNumber)
        {
            MemoryStream ms = null;
            Image returnImage = null;

            try
            {
                ms = new MemoryStream();
                Guid objGuid = sourceImage.FrameDimensionsList[0];
                FrameDimension objDimension = new FrameDimension(objGuid);
                sourceImage.SelectActiveFrame(objDimension, pageNumber);
                sourceImage.Save(ms, ImageFormat.Tiff);
                returnImage = Image.FromStream(ms);
            }
            catch (Exception ex)
            {
                returnImage = null;
            }
            return returnImage;
        }

        public PdfSharp.Pdf.PdfDocument tiff2PDF(string fileName, PdfSharp.Pdf.PdfDocument doc)
        {
            XGraphics xgr = null;
            try
            {
                int pageCount = this.getPageCount(fileName);

                if (pageCount > 0) //if multiple pages or single page per tif file.
                {
                    PdfSharp.Pdf.PdfPage page = null;

                    //for (int i = 0; i < pageCount; i++)
                    //{
                    page = new PdfSharp.Pdf.PdfPage();

                    //System.Drawing.Image tiffImg = this.getTiffImage(fileName, i);

                    //XImage img = XImage.FromGdiPlusImage(tiffImg);
                    XImage img = XImage.FromFile(fileName);
                        
                    page.Width = img.PointWidth;
                    page.Height = img.PointHeight;
                    doc.Pages.Add(page);

                    xgr = XGraphics.FromPdfPage(page);
                    xgr.DrawImage(img, 0, 0);

                    xgr.Dispose();
                    //}
                }

                return doc;
            }
            catch (Exception ex)
            {
                SRDocScanIDP.mGlobal.Write2Log(fileName);
                throw new Exception(ex.Message);
            }
        }

        public Syncfusion.Pdf.PdfDocument tiff2PDF(string fileName, Syncfusion.Pdf.PdfDocument doc)
        {            
            try
            {
                //Create a new PDF document
                //PdfDocument document = new PdfDocument();
                //Add a page to the document
                PdfPage page = doc.Pages.Add();
                //Create PDF graphics for a page
                PdfGraphics graphics = page.Graphics;
                //Load the image from the disk
                PdfBitmap image = new PdfBitmap(fileName);
                //Draw the image
                graphics.DrawImage(image, 0, 0, page.GetClientSize().Width, page.GetClientSize().Height);

                //Save the document into stream
                using (MemoryStream stream = new MemoryStream())
                {
                    doc.Save(stream);
                }
                //image.Dispose();
                return doc;
            }
            catch (Exception ex)
            {
                SRDocScanIDP.mGlobal.Write2Log(fileName);
                SRDocScanIDP.mGlobal.Write2Log(ex.Message);
                SRDocScanIDP.mGlobal.Write2Log(ex.StackTrace.ToString());
                throw new Exception(ex.Message);
            }
        }

        public Syncfusion.Pdf.PdfDocument tiff2PDF(PdfBitmap bitImage, Syncfusion.Pdf.PdfDocument doc)
        {
            try
            {
                //Create a new PDF document
                //PdfDocument document = new PdfDocument();
                //Add a page to the document
                PdfPage page = doc.Pages.Add();
                //Create PDF graphics for a page
                PdfGraphics graphics = page.Graphics;
                //Load the image from the disk
                PdfBitmap image = bitImage;
                //Draw the image
                graphics.DrawImage(image, 0, 0, page.GetClientSize().Width, page.GetClientSize().Height);

                //Save the document into stream
                using (MemoryStream stream = new MemoryStream())
                {
                    doc.Save(stream);
                }
                //image.Dispose();
                return doc;
            }
            catch (Exception ex)
            {
                SRDocScanIDP.mGlobal.Write2Log(ex.Message);
                SRDocScanIDP.mGlobal.Write2Log(ex.StackTrace.ToString());
                throw new Exception(ex.Message);
            }
        }

        public IronPdf.PdfDocument tiff2PDF(string fileName, IronPdf.PdfDocument doc)
        {
            //IronPdf.PdfDocument pdf = null;
            try
            {
                var oOption = new ChromePdfRenderOptions();
                oOption.PaperSize = IronPdf.Rendering.PdfPaperSize.Letter;
                oOption.Timeout = iTimeout; //seconds.
                oOption.WaitFor.RenderDelay(iDelay); //miliseconds.
                //oOption.RenderDelay = iDelay;
                oOption.CreatePdfFormsFromHtml = false;

                //pdf = ImageToPdfConverter.ImageToPdf(fileName);
                int i = 1;
                while (i <= 2)
                {
                    try
                    {
                        using (var pdf = ImageToPdfConverter.ImageToPdf(fileName, IronPdf.Imaging.ImageBehavior.FitToPage, oOption))
                        {
                            if (doc == null)
                                doc = new IronPdf.PdfDocument(pdf);
                            else
                                doc.AppendPdf(pdf);

                            //doc = pdf;
                            if (pdf != null) pdf.Dispose();
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        SRDocScanIDP.mGlobal.Write2Log(fileName);
                        if (i == 1)
                            SRDocScanIDP.mGlobal.Write2Log("ToPdf.." + ex.Message);
                        else
                            SRDocScanIDP.mGlobal.Write2Log("Retry " + (i - 1).ToString() + ".." + ex.Message);
                    }

                    i += 1;
                }

                return doc;
            }
            catch (Exception ex)
            {
                SRDocScanIDP.mGlobal.Write2Log(fileName);
                SRDocScanIDP.mGlobal.Write2Log(ex.Message);
                SRDocScanIDP.mGlobal.Write2Log(ex.StackTrace.ToString());
                throw new Exception(ex.Message);
            }
            finally
            {
                //if (pdf != null) pdf.Dispose();
            }
        }

        public void OCR2PdfSearchable(Stream pStream, string pFileName)
        {
            try
            {
                //Load a PDF document  
                //Syncfusion pdf.
                //PdfLoadedDocument lDoc = new PdfLoadedDocument(pStream);

                //string sTesseractPath = System.Windows.Forms.Application.StartupPath + @"\3.02\";
                ////Initialize the OCR processor by providing the path of tesseract binaries(SyncfusionTesseract.dll and liblept168.dll)
                //using (OCRProcessor processor = new OCRProcessor(sTesseractPath))
                //{
                //    //Set OCR language to process
                //    processor.Settings.Language = Languages.English;
                //    //Set custom temp file path location
                //    processor.Settings.TempFolder = @"C:\temp\";
                //    //Set tesseract OCR Engine 
                //    processor.Settings.TesseractVersion = Syncfusion.OCRProcessor.TesseractVersion.Version3_02;
                //    //Set OCR engine mode to process
                //    processor.Settings.OCREngineMode = OCREngineMode.TesseractOnly;
                //    //set the OCR performance
                //    processor.Settings.Performance = Performance.Fast;
                //    //Process OCR by providing the PDF document and Tesseract data
                //    processor.PerformOCR(lDoc, sTesseractPath + @"TessData\", true);
                //    //Save the OCR processed PDF document in the disk
                //    lDoc.Save(pFileName);
                //    //Close the document
                //    lDoc.Close(true);
                //}
                IronOcr.License.LicenseKey = _IronLic;

                var ocrTesseract = new IronTesseract();
                ocrTesseract.Language = OcrLanguage.EnglishBest;
                ocrTesseract.Configuration.EngineMode = TesseractEngineMode.TesseractOnly;
                //ocrTesseract.UseCustomTesseractLanguageFile(@"C:\Development\SRDocScanIDP - Demo\bin\Debug\runtimes\win-x86\native\tessdata\tessdata_best\eng.best.traineddata");
                //ocrTesseract.Configuration.RenderSearchablePdfsAndHocr = true;
                ocrTesseract.Configuration.RenderHocr = false;
                ocrTesseract.Configuration.RenderSearchablePdf = true;

                System.Drawing.Bitmap oBit = (System.Drawing.Bitmap) System.Drawing.Bitmap.FromStream(pStream);
                Byte[] oBytes = null;

                using (MemoryStream oMem = new MemoryStream())
                {
                    oBit.Save(oMem, System.Drawing.Imaging.ImageFormat.Tiff);
                    oBytes = oMem.ToArray();
                }

                using (var ocrInput = new OcrInput())
                {
                    ocrInput.LoadImage(oBytes);
                    //ocrInput.AddImage(oBytes);

                    ocrInput.Deskew();
                    ocrInput.DeNoise();

                    var ocrResult = ocrTesseract.Read(ocrInput);
                    if (ocrInput.PageCount() > 0)
                    {
                        if (ocrResult != null)
                        {
                            if (ocrResult.PageCount > 0)
                                ocrResult.SaveAsSearchablePdf(pFileName);
                            else
                                SRDocScanIDP.mGlobal.Write2Log("OCR result is 0 pages.");
                        }
                    }
                    else
                        SRDocScanIDP.mGlobal.Write2Log("Input pdf is 0 pages.");
                }
            }
            catch (Exception ex)
            {
                SRDocScanIDP.mGlobal.Write2Log(ex.Message);
                SRDocScanIDP.mGlobal.Write2Log(ex.StackTrace.ToString());
                throw new Exception(ex.Message);
            }
        }

        public void OCR2PdfSearchable(string PDFfileName)
        {
            try
            {
                //Load a PDF document
                //Syncfusion pdf.
                //PdfLoadedDocument lDoc = new PdfLoadedDocument(PDFfileName);

                //string sTesseractPath = System.Windows.Forms.Application.StartupPath + @"\3.02\";
                ////Initialize the OCR processor by providing the path of tesseract binaries(SyncfusionTesseract.dll and liblept168.dll)
                //using (OCRProcessor processor = new OCRProcessor(sTesseractPath))
                //{
                //    processor.Settings.IsCompressionEnabled = true;

                //    //Set OCR language to process
                //    processor.Settings.Language = Languages.English;
                //    //Set custom temp file path location
                //    processor.Settings.TempFolder = @"C:\temp\";
                //    //Set tesseract OCR Engine 
                //    processor.Settings.TesseractVersion = Syncfusion.OCRProcessor.TesseractVersion.Version3_02; //Syncfusion.OCRProcessor.TesseractVersion.Version3_02;
                //    //Set OCR engine mode to process
                //    processor.Settings.OCREngineMode = OCREngineMode.TesseractOnly;
                //    //set the OCR performance
                //    processor.Settings.Performance = Performance.Fast;
                //    //Process OCR by providing the PDF document and Tesseract data
                //    processor.PerformOCR(lDoc, sTesseractPath + @"Tessdata\", true);
                //    //Save the OCR processed PDF document in the disk
                //    lDoc.Save(PDFfileName);
                //    //Close the document
                //    lDoc.Close(true);
                //}

                IronOcr.License.LicenseKey = _IronLic;
                //SRDocScanIDP.mGlobal.Write2Log(IronOcr.License.LicenseKey);
                //SRDocScanIDP.mGlobal.Write2Log(IronOcr.License.IsValidLicense(_IronLic).ToString());

                var ocrTesseract = new IronTesseract();
                ocrTesseract.Language = OcrLanguage.EnglishBest;
                ocrTesseract.Configuration.EngineMode = TesseractEngineMode.TesseractOnly;
                //ocrTesseract.UseCustomTesseractLanguageFile(@"C:\Development\SRDocScanIDP - Demo\bin\Debug\runtimes\win-x86\native\tessdata\tessdata_best\eng.best.traineddata");
                ocrTesseract.Configuration.RenderSearchablePdfsAndHocr = true;
                ocrTesseract.Configuration.RenderHocr = true;
                ocrTesseract.Configuration.RenderSearchablePdf = true;

                using (var ocrInput = new OcrInput())
                {
                    ocrInput.LoadPdf(PDFfileName);

                    ocrInput.Deskew();
                    ocrInput.DeNoise();

                    var ocrResult = ocrTesseract.Read(ocrInput);
                    if (ocrInput.PageCount() > 0)
                    {
                        if (ocrResult != null)
                        {
                            if (ocrResult.PageCount > 0)
                                ocrResult.SaveAsSearchablePdf(PDFfileName);
                            else
                                SRDocScanIDP.mGlobal.Write2Log("OCR result is 0 pages.");
                        }
                    }  
                    else
                        SRDocScanIDP.mGlobal.Write2Log("Input pdf is 0 pages.");
                }
            }
            catch (Exception ex)
            {
                SRDocScanIDP.mGlobal.Write2Log(ex.Message);
                SRDocScanIDP.mGlobal.Write2Log(ex.StackTrace.ToString());
                throw new Exception(ex.Message);
            }
        }

        


    }

}
