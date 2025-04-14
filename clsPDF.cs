using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using PdfSharp.Drawing;
using PdfSharp;

using Dynamsoft.Core;
using Dynamsoft.OCR;

namespace SRDocScanIDP
{
    public class clsPDF
    {
        private Tesseract mTesseract = null;

        public clsPDF()
        {
            mTesseract = new Tesseract(MDIMain.strProductKey);
        }

        public Image getTiffImage(String sourceFile, int pageNumber)
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

            return returnImage;
        }

        public Image getTiffImage(Image sourceImage, int pageNumber)
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

        private PdfSharp.Pdf.PdfDocument tiffToPDF(string fileName, PdfSharp.Pdf.PdfDocument doc)
        {
            XGraphics xgr = null;
            try
            {
                int pageCount = this.getPageCount(fileName);

                if (pageCount > 0) //if multiple pages or single page per tiff file.
                {

                    PdfSharp.Pdf.PdfPage page = null;

                    for (int i = 0; i < pageCount; i++)
                    {
                        page = new PdfSharp.Pdf.PdfPage();

                        System.Drawing.Image pdfImg = this.getTiffImage(fileName, i);

                        XImage img = XImage.FromGdiPlusImage(pdfImg);

                        page.Width = img.PointWidth;
                        page.Height = img.PointHeight;
                        doc.Pages.Add(page);

                        xgr = XGraphics.FromPdfPage(page);
                        xgr.DrawImage(img, 0, 0);

                        xgr.Dispose();
                    }
                }

                return doc;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

        private PdfSharp.Pdf.PdfDocument setPDF(Bitmap oImgCore, PdfSharp.Pdf.PdfDocument doc)
        {
            XGraphics xgr = null;
            try
            {
                int pageCount = 1; //if multiple pages or single page per pdf file.

                if (oImgCore != null) 
                {

                    PdfSharp.Pdf.PdfPage page = null;

                    for (int i = 0; i < pageCount; i++)
                    {
                        page = new PdfSharp.Pdf.PdfPage();

                        System.Drawing.Image pdfImg = (System.Drawing.Image) oImgCore;

                        XImage img = XImage.FromGdiPlusImage(pdfImg);

                        page.Width = img.PointWidth;
                        page.Height = img.PointHeight;
                        doc.Pages.Add(page);

                        xgr = XGraphics.FromPdfPage(page);
                        xgr.DrawImage(img, 0, 0);

                        xgr.Dispose();
                    }
                }

                return doc;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

        public bool savePDF(Bitmap oImgCore, string pFilename)
        {
            bool bSave = true;
            PdfSharp.Pdf.PdfDocument oPDF = null;
            try
            {
                /*//Codes using iTextSharp.
                //System.Drawing.Image image = oImgCore;
                //FileStream fs = new FileStream(pFilename, FileMode.Create, FileAccess.Write, FileShare.None);

                //iTextSharp.text.Image pdfImage = iTextSharp.text.Image.GetInstance(image, System.Drawing.Imaging.ImageFormat.Jpeg);

                //iTextSharp.text.Document doc = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4, 0, 0, 0, 0);
                //PdfWriter pdfWriter = PdfWriter.GetInstance(pdfdoc, fs);
                //doc.Open();
                //doc.Add(pdfImage);
                //doc.Close();               

                //iTextSharp.text.pdf.PdfDocument pdfdoc = new iTextSharp.text.pdf.PdfDocument();                
                //PdfWriter pdfWriter = PdfWriter.GetInstance(pdfdoc, fs);
                //pdfdoc.Open();
                //pdfdoc.Add(pdfImage);
                //pdfdoc.Close();
                //convert2SearchablePDF(pFilename);

                //pdfWriter.Close();
                */

                oPDF = new PdfSharp.Pdf.PdfDocument();
                oPDF = setPDF(oImgCore, oPDF);
                oPDF.Save(pFilename);
                convert2SearchablePDF(pFilename);
                //ocr2SearchablePDF(oImgCore, pFilename); //Return empty content text in this function, not the expected.
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log(ex.Message);
                bSave = false;
                throw new Exception(ex.Message);
            }

            return bSave;
        }

        private void ocr2SearchablePDF(Bitmap oImgCore, string pPDFFilename)
        {

            try
            {
                mTesseract.Language = "UTF-8";
                mTesseract.ResultFormat = Dynamsoft.OCR.Enums.ResultFormat.PDFPlainText;
                byte[] sbytes = null;

                sbytes = mTesseract.Recognize(oImgCore);

                System.IO.File.WriteAllBytes(pPDFFilename, sbytes);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

        //private void ocr2SearchablePDF(string pFilename)
        //{
        //    if (File.Exists(pFilename))
        //    {
        //        try
        //        {
        //            FileStream fs = new FileStream(pFilename, FileMode.Create, FileAccess.Write, FileShare.None);
        //            PdfReader pdfReader = new PdfReader(pFilename);
        //            iTextSharp.text.Document doc = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4, 0, 0, 0, 0);
        //            PdfWriter pdfWriter = PdfWriter.GetInstance(doc, fs);
        //            pdfWriter.SetMargins(0, 0, 0, 0);

        //            iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(fs);

        //            PdfReaderContentParser parser = new PdfReaderContentParser(pdfReader);
        //            parser.ProcessContent(0, listener);

        //            pdfReader.Close();
        //        }
        //        catch (Exception ex)
        //        {
        //            throw new Exception(ex.Message);
        //        }
        //    }
        //}

        private void convert2SearchablePDF(string pFilename)
        {
            if (File.Exists(pFilename))
            {
                try
                {
                    PdfReader pdfReader = new PdfReader(pFilename);
                    for (int page = 1; page <= pdfReader.NumberOfPages; page++)
                    {
                        ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                        string currentText = PdfTextExtractor.GetTextFromPage(pdfReader, page, strategy);

                        currentText = Encoding.UTF8.GetString(ASCIIEncoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(currentText)));
                        
                    }
                    pdfReader.Close();
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
        }
    }
}
