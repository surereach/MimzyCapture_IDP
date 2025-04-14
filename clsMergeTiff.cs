using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Parsing;
using Syncfusion.OCRProcessor;

using IronOcr;

namespace Tif2TiffMerge
{
    public class clsMergeTiff
    {
        private static string _IronLic;

        public clsMergeTiff(string pIronLic)
        {
            _IronLic = pIronLic;
        }

        public int getPageCount(byte[] byImg)
        {
            int pageCount = 0;
            try
            {
                using (MemoryStream mem = new MemoryStream(byImg))
                {
                    Image img = Image.FromStream(mem);
                    pageCount = getPageCount(img);
                    img.Dispose();
                }
            }
            catch (Exception ex)
            {
                pageCount = 0;
            }
            return pageCount;
        }

        public int getPageCount(Image img)
        {
            int pageCount = 0;
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

        public byte[] tif2tiff(string[] tiffFilenames)
        {
            byte[] bytImg = null;
            try
            {
                bytImg = this.getTiffImages(tiffFilenames);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return bytImg;
        }

        private byte[] getTiffImages(string[] sourceFiles)
        {
            byte[] tiffMerge = null;
            try
            {
                var msMerge = new MemoryStream();
                Bitmap pages = null;

                ImageCodecInfo ici = null;
                foreach (ImageCodecInfo ic in ImageCodecInfo.GetImageEncoders())
                {
                    if (ic.MimeType == "image/tiff")
                        ici = ic;
                }

                System.Drawing.Imaging.Encoder enc = System.Drawing.Imaging.Encoder.SaveFlag;
                EncoderParameters ep = new EncoderParameters(1);
                int iframe = 0;
                System.Drawing.Image sourceImage;

                int iSrc = 0;

                while (iSrc < sourceFiles.Length)
                {
                    sourceImage = Image.FromFile(sourceFiles[iSrc].ToString());

                    Guid[] objGuids = sourceImage.FrameDimensionsList;

                    int i = 0;
                    while (i < objGuids.Length)
                    {
                        FrameDimension objDimension = new FrameDimension(objGuids[i]);
                        int noOfPages = sourceImage.GetFrameCount(objDimension); //Gets the total number of frames in the .tiff file 

                        int iPage = 0;
                        while (iPage < noOfPages)
                        {
                            FrameDimension curDimension = new FrameDimension(objGuids[i]);
                            sourceImage.SelectActiveFrame(curDimension, iPage);

                            using (MemoryStream ms = new MemoryStream())
                            {
                                sourceImage.Save(ms, ImageFormat.Tiff);
                                if (iframe == 0)
                                {
                                    //save the first frame
                                    pages = (Bitmap)Image.FromStream(ms);
                                    ep.Param[0] = new EncoderParameter(enc, (long)EncoderValue.MultiFrame);
                                    pages.Save(msMerge, ici, ep);
                                }
                                else
                                {
                                    //save the intermediate frames
                                    ep.Param[0] = new EncoderParameter(enc, (long)EncoderValue.FrameDimensionPage);
                                    pages.SaveAdd((Bitmap)Image.FromStream(ms), ep);
                                }
                            }                            
                            iframe++;

                            iPage += 1;
                        }
                        i += 1;
                    }

                    iSrc += 1;
                }

                if (iframe > 0)
                {
                    //flush and close.
                    ep.Param[0] = new EncoderParameter(enc, (long)EncoderValue.Flush);
                    pages.SaveAdd(ep);
                }

                msMerge.Position = 0;
                tiffMerge = msMerge.ToArray();

                msMerge.Close();
                msMerge.Dispose();
            }
            catch (Exception ex)
            {
                tiffMerge = null;
            }

            return tiffMerge;
        }

        public void OCR2PdfSearchableFromTiff(string PDFfileName, string TIFFfileName, bool bDeleteTiff)
        {
            try
            {
                string sPDFfileName = PDFfileName;
                string sTIFFfileName = TIFFfileName;
                //Load a PDF document
                //Syncfusion pdf.
                //PdfDocument pdfDocument = new PdfDocument();
                //pdfDocument.EnableMemoryOptimization = true;
                //pdfDocument.Compression = PdfCompressionLevel.Best;
                //Tiff2PdfConverter.clsTiffSplitter oTiff = new Tiff2PdfConverter.clsTiffSplitter(this._IronLic);
                ////pdfDocument = oTiff.tiff2PDF(TIFFfileName, pdfDocument);                

                ////Load the multi frame TIFF image from the disk
                //PdfBitmap tiffImage = new PdfBitmap(TIFFfileName);
                ////Get the frame count
                //int frameCount = tiffImage.FrameCount;
                ////Access each frame and draw into the page
                //int i = 0;
                //while (i < frameCount)
                //{                    
                //    tiffImage.ActiveFrame = i;
                //    pdfDocument = oTiff.tiff2PDF(tiffImage, pdfDocument);
                //    ////Add a section to the PDF document
                //    //PdfSection section = pdfDocument.Sections.Add();
                //    ////Set page margins
                //    //section.PageSettings.Margins.All = 0;
                //    ////Create a PDF unit converter instance
                //    //PdfUnitConvertor converter = new PdfUnitConvertor();
                //    ////Convert to point
                //    //SizeF size = converter.ConvertFromPixels(tiffImage.PhysicalDimension, PdfGraphicsUnit.Point);
                //    ////Set page orientation
                //    //section.PageSettings.Orientation = (size.Width > size.Height) ? PdfPageOrientation.Landscape : PdfPageOrientation.Portrait;
                //    ////Set page size
                //    //section.PageSettings.Size = size;
                //    ////Add a page to the section
                //    //PdfPage page = section.Pages.Add();
                //    ////Draw TIFF image into the PDF page
                //    //page.Graphics.DrawImage(tiffImage, PointF.Empty, size);

                //    i += 1;
                //}
                //SRDocScanIDP.mGlobal.Write2Log("tiff pdf completed..");
                //tiffImage.Dispose();
                //MemoryStream oMem = new MemoryStream();
                //pdfDocument.Save(oMem);
                //SRDocScanIDP.mGlobal.Write2Log("pdf save completed..");
                //PdfLoadedDocument lDoc = new PdfLoadedDocument(oMem);

                //string sTesseractPath = System.Windows.Forms.Application.StartupPath + @"\3.02\";
                ////Initialize the OCR processor by providing the path of tesseract binaries(SyncfusionTesseract.dll and liblept168.dll)
                //using (OCRProcessor processor = new OCRProcessor(sTesseractPath))
                //{
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
                //SRDocScanIDP.mGlobal.Write2Log("searchable pdf completed..");

                IronOcr.License.LicenseKey = _IronLic;
                //SRDocScanIDP.mGlobal.Write2Log(IronOcr.License.LicenseKey);
                //SRDocScanIDP.mGlobal.Write2Log(IronOcr.License.IsValidLicense(_IronLic).ToString());

                if (File.Exists(sTIFFfileName) && !File.Exists(sPDFfileName))
                {
                    var ocrTesseract = new IronTesseract();
                    ocrTesseract.Language = OcrLanguage.EnglishBest;
                    ocrTesseract.Configuration.EngineMode = TesseractEngineMode.TesseractOnly;
                    //ocrTesseract.UseCustomTesseractLanguageFile(@"C:\Development\SRDocScanIDP - Demo\bin\Debug\runtimes\win-x86\native\tessdata\tessdata_best\eng.best.traineddata");
                    ocrTesseract.Configuration.RenderSearchablePdfsAndHocr = true;
                    ocrTesseract.Configuration.RenderHocr = true;
                    ocrTesseract.Configuration.RenderSearchablePdf = true;

                    using (var ocrInput = new OcrInput())
                    {
                        ocrInput.LoadImage(sTIFFfileName);

                        ocrInput.Deskew();
                        ocrInput.DeNoise();
                        //var pageindices = ocrInput.Pages.ToArray();

                        OcrResult ocrResult = ocrTesseract.Read(ocrInput);
                        try
                        {
                            if (ocrInput.PageCount() > 0)
                            {
                                if (FileInUse(sPDFfileName) == false && ocrResult != null)
                                {
                                    if (ocrResult.PageCount > 0)
                                        ocrResult.SaveAsSearchablePdf(sPDFfileName);
                                    else
                                        SRDocScanIDP.mGlobal.Write2Log("OCR result is 0 pages.");
                                }
                                else
                                    SRDocScanIDP.mGlobal.Write2Log("PDF file is in used.");
                            }
                            else
                            {
                                ocrResult = ocrTesseract.Read(sTIFFfileName);
                                if (ocrResult != null)
                                {
                                    if (ocrResult.PageCount > 0)
                                    {
                                        ocrResult.SaveAsSearchablePdf(sPDFfileName);
                                    }
                                    else
                                        SRDocScanIDP.mGlobal.Write2Log("OCR result is 0 pages.");
                                }
                                else
                                    SRDocScanIDP.mGlobal.Write2Log("Input image is 0 pages.");
                            }

                        }
                        catch (IOException io) //in case for pdf file already in used by another process.
                        {
                            return;
                        }
                        catch (Exception ex)
                        {
                            return;
                        }                        
                        
                        //if (ocrInput != null) ocrInput.Dispose();
                    }
                    //if (oMem != null)
                    //{
                    //    oMem.Close();
                    //    oMem.Dispose();
                    //}
                    //if (pdfDocument != null)
                    //{
                    //    pdfDocument.Close(); pdfDocument.Dispose();
                    //}
                    //if (lDoc != null)
                    //{
                    //    lDoc.Dispose();
                    //}

                    if (bDeleteTiff)
                    {
                        if (File.Exists(sTIFFfileName))
                            File.Delete(sTIFFfileName);
                    }
                }
            }
            catch (Exception ex)
            {
                SRDocScanIDP.mGlobal.Write2Log(ex.Message);
                SRDocScanIDP.mGlobal.Write2Log(ex.StackTrace.ToString());
                //throw new Exception(ex.Message);
            }
        }

        public void OCR2PdfSearchableFromTiff(string TIFFfileName, string PDFfileName, byte[] TiffBytes)
        {
            try
            {
                //Load a PDF document
                //Syncfusion pdf.
                //PdfDocument pdfDocument = new PdfDocument();
                //pdfDocument.EnableMemoryOptimization = true;
                //pdfDocument.Compression = PdfCompressionLevel.Best;
                //Tiff2PdfConverter.clsTiffSplitter oTiff = new Tiff2PdfConverter.clsTiffSplitter(this._IronLic);
                ////pdfDocument = oTiff.tiff2PDF(TIFFfileName, pdfDocument);
                //MemoryStream oMem = new MemoryStream();
                //oMem.Write(TiffBytes, 0, TiffBytes.Length);

                ////Load the multi frame TIFF image from the disk
                //PdfBitmap tiffImage = new PdfBitmap(oMem);
                ////Get the frame count
                //int frameCount = tiffImage.FrameCount;
                ////Access each frame and draw into the page
                //int i = 0;
                //while (i < frameCount)
                //{
                //    tiffImage.ActiveFrame = i;
                //    pdfDocument = oTiff.tiff2PDF(tiffImage, pdfDocument);
                //    ////Add a section to the PDF document
                //    //PdfSection section = pdfDocument.Sections.Add();
                //    ////Set page margins
                //    //section.PageSettings.Margins.All = 0;
                //    ////Create a PDF unit converter instance
                //    //PdfUnitConvertor converter = new PdfUnitConvertor();
                //    ////Convert to point
                //    //SizeF size = converter.ConvertFromPixels(tiffImage.PhysicalDimension, PdfGraphicsUnit.Point);
                //    ////Set page orientation
                //    //section.PageSettings.Orientation = (size.Width > size.Height) ? PdfPageOrientation.Landscape : PdfPageOrientation.Portrait;
                //    ////Set page size
                //    //section.PageSettings.Size = size;
                //    ////Add a page to the section
                //    //PdfPage page = section.Pages.Add();
                //    ////Draw TIFF image into the PDF page
                //    //page.Graphics.DrawImage(tiffImage, PointF.Empty, size);

                //    i += 1;
                //}
                //SRDocScanIDP.mGlobal.Write2Log("tiff pdf completed..");
                //tiffImage.Dispose();
                //oMem.Close(); oMem.Dispose();
                //oMem = new MemoryStream();
                //pdfDocument.Save(oMem);
                //SRDocScanIDP.mGlobal.Write2Log("pdf save completed..");
                //PdfLoadedDocument lDoc = new PdfLoadedDocument(oMem);

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
                //    processor.Settings.TesseractVersion = Syncfusion.OCRProcessor.TesseractVersion.Version3_02; //Syncfusion.OCRProcessor.TesseractVersion.Version4_0;
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
                //SRDocScanIDP.mGlobal.Write2Log("searchable pdf completed..");

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
                    ocrInput.LoadImage(TiffBytes);

                    ocrInput.Deskew();
                    ocrInput.DeNoise();
                    //var pageindices = ocrInput.Pages.ToArray();                    

                    OcrResult ocrResult = null;
                    if (ocrInput.PageCount() > 0)
                    {
                        ocrResult = ocrTesseract.Read(ocrInput);
                        if (ocrResult != null)
                        {
                            if (ocrResult.PageCount > 0)
                                ocrResult.SaveAsSearchablePdf(PDFfileName);
                            else
                                SRDocScanIDP.mGlobal.Write2Log("OCR result is 0 pages.");
                        }
                    }
                    else
                    {
                        ocrResult = ocrTesseract.Read(TIFFfileName);
                        if (ocrResult != null)
                        {
                            if (ocrResult.PageCount > 0)
                                ocrResult.SaveAsSearchablePdf(PDFfileName);
                            else
                                SRDocScanIDP.mGlobal.Write2Log("OCR result is 0 pages..");
                        }
                        else
                            SRDocScanIDP.mGlobal.Write2Log("Input image is 0 pages.");

                        if (File.Exists(TIFFfileName))
                            File.Delete(TIFFfileName);
                    }                    

                    //if (ocrInput != null) ocrInput.Dispose();
                }

                //if (oMem != null)
                //{
                //    oMem.Close();
                //    oMem.Dispose();
                //}
                //if (pdfDocument != null)
                //{
                //    pdfDocument.Close(); pdfDocument.Dispose();
                //}
                //if (lDoc != null)
                //{
                //    lDoc.Dispose();
                //}
            }
            catch (Exception ex)
            {
                SRDocScanIDP.mGlobal.Write2Log(ex.Message);
                SRDocScanIDP.mGlobal.Write2Log(ex.StackTrace.ToString());
                //throw new Exception(ex.Message);
            }
        }

        static bool FileInUse(string path)
        {
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
                {
                    return !fs.CanWrite;
                }
                //return false;
            }
            catch (IOException ex)
            {
                return true;
            }
        }

    }
}
