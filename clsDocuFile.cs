using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;
using System.Drawing;

using Dynamsoft.Core;
//using Dynamsoft.PDF;

using IronSoftware.Drawing;
using Dynamsoft.PDF;
using System.Data.Odbc;

namespace SRDocScanIDP
{
    extern alias clsUtilComn;

    public class clsDocument
    {
        public string Set;
        public string DocType;
        public int SetIdx;
        public int TypeIdx;
    }

    public class Folders
    {
        public string Source
        {
            get; private set;
        }
        public string Target
        {
            get; private set;
        }
        public Folders(string source, string target)
        {
            Source = source;
            Target = target;
        }
    }

    public class clsDocuFile
    {
        public bool _isMultiPage;

        //public PDFCreator oPDFCreator;
        public clsPDF oPDFCreator;
        public ImageCore oImgCore;

        public List<_stcOCRAreaInfo> stcOCRAreaList;

        public struct _stcOCRAreaInfo
        {
            public string name;
            public int left;
            public int top;
            public int right;
            public int bottom;
            public int width;
            public int height;
        }

        public clsDocuFile()
        {
            _isMultiPage = false;
        }

        public bool saveImageToFile(string pDir, string pFileName, string pFileExt)
        {
            try
            {
                if (this.oImgCore != null)
                {
                    pFileName = mGlobal.addDirSep(pDir) + pFileName;

                    if (!Directory.Exists(mGlobal.addDirSep(pDir)))
                        Directory.CreateDirectory(mGlobal.addDirSep(pDir));

                    if (pFileExt.Trim().ToUpper() == "JPEG")
                    {
                        this.oImgCore.IO.SaveAsJPEG(pFileName, this.oImgCore.ImageBuffer.CurrentImageIndexInBuffer);
                    }
                    if (pFileExt.Trim().ToUpper() == "BMP")
                    {
                        this.oImgCore.IO.SaveAsBMP(pFileName, this.oImgCore.ImageBuffer.CurrentImageIndexInBuffer);
                    }
                    if (pFileExt.Trim().ToUpper() == "PNG")
                    {
                        this.oImgCore.IO.SaveAsPNG(pFileName, this.oImgCore.ImageBuffer.CurrentImageIndexInBuffer);
                    }
                    if (pFileExt.Trim().ToUpper() == "TIFF")
                    {
                        // Multi page TIFF
                        List<short> tempListIndex = new List<short>();
                        if (_isMultiPage == true)
                        {
                            short sIndex = 0;
                            while (sIndex < this.oImgCore.ImageBuffer.HowManyImagesInBuffer)
                            {
                                tempListIndex.Add(sIndex);

                                sIndex += 1;
                            }
                        }
                        else
                        {
                            tempListIndex.Add(this.oImgCore.ImageBuffer.CurrentImageIndexInBuffer);
                        }
                        this.oImgCore.IO.SaveAsTIFF(pFileName, tempListIndex);

                        //AnyBitmap oBit = this.oImgCore.ImageBuffer.GetBitmap(tempListIndex[0]);
                        //List<AnyBitmap> oBits = new List<AnyBitmap>();
                        //oBits.Add(oBit);
                        //IronSoftware.Drawing.AnyBitmap.CreateMultiFrameTiff(oBits).ExportFile(pFileName, AnyBitmap.ImageFormat.Tiff);
                    }
                    if (pFileExt.Trim().ToUpper() == "PDF")
                    {
                        //Multi page PDF
                        if (this.oPDFCreator != null)
                        {
                            //oPDFCreator.Save(this as ISave, pFileName);
                            Bitmap oImgCore = this.oImgCore.ImageBuffer.GetBitmap(this.oImgCore.ImageBuffer.CurrentImageIndexInBuffer);
                            oPDFCreator.savePDF(oImgCore, pFileName);
                        }                            
                        else
                            throw new Exception("An PDF Creator is required!");
                        
                    }
                }
                else
                {
                    throw new Exception("An image is required!");
                }
            }
            catch (Exception ex)
            {
                return false;
                throw new Exception(ex.Message);
            }

            return true;
        }

        public DataRow getProcessDocBatchDB(string pCurrScanProj, string pAppNum, string pBatchType, string pStatusIn, string pScanStn, string pScanUsr, long pCurrRowId = 0)
        {
            DataRow dtRet = null;
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT TOP 1 * FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan ";
                sSQL += "WHERE scanproj='" + pCurrScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchtype='" + pBatchType.Trim().Replace("'", "") + "' ";
                //sSQL += "AND scanstation='" + pScanStn.Trim().Replace("'", "") + "' ";
                //sSQL += "AND scanuser='" + pScanUsr.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchstatus IN (" + pStatusIn.Trim() + ") ";
                sSQL += "AND fromproc IS NOT NULL ";

                if (pCurrRowId != 0)
                {
                    sSQL += "AND rowid=" + pCurrRowId + " ";
                    sSQL += "ORDER BY batchcode ";
                }
                else
                    sSQL += "ORDER BY batchcode ";

                dtRet = mGlobal.objDB.ReturnSingleRow(sSQL);

            }
            catch (Exception ex)
            {
                dtRet = null;
                throw new Exception(ex.Message);
            }

            return dtRet;
        }

        public DataRow getDocBatchDB(string pCurrScanProj, string pAppNum, string pBatchType, string pStatusIn, string pScanStn, string pScanUsr = "", long pCurrRowId = 0)
        {
            DataRow dtRet = null;
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT TOP 1 * FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan ";
                sSQL += "WHERE scanproj='" + pCurrScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchtype='" + pBatchType.Trim().Replace("'", "") + "' ";
                //sSQL += "AND scanstation='" + pScanStn.Trim().Replace("'", "") + "' ";                
                sSQL += "AND batchstatus IN (" + pStatusIn.Trim() + ") ";

                if (pScanUsr.Trim() != "")
                    sSQL += "AND scanuser='" + pScanUsr.Trim().Replace("'", "") + "' ";

                if (pCurrRowId != 0)
                {
                    sSQL += "AND rowid=" + pCurrRowId + " ";
                    sSQL += "ORDER BY setnum DESC,batchcode ";
                }
                else
                    sSQL += "ORDER BY setnum DESC,batchcode ";

                dtRet = mGlobal.objDB.ReturnSingleRow(sSQL);

            }
            catch (Exception ex)
            {
                dtRet = null;
                throw new Exception(ex.Message);
            }

            return dtRet;
        }

        public DataRowCollection getDocFileBySetNumDB(string pCurrScanProj, string pAppNum, string pSetNum, string pStatusIN, string pStartDate = "")
        {
            DataRowCollection dtRet = null;
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT * FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuScan ";
                sSQL += "WHERE scanproj='" + pCurrScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                //sSQL += "AND setnum='" + pSetNum.Trim().Replace("'", "") + "' ";
                //sSQL += "AND scanstation='" + pScanStn.Trim().Replace("'", "") + "' ";
                //sSQL += "AND scanuser='" + pScanUsr.Trim().Replace("'", "") + "' ";
                sSQL += "AND scanstatus IN (" + pStatusIN.Trim() + ") ";

                if (pStartDate.Trim() != "")
                    sSQL += "AND Convert(datetime,scanstart,103)>='" + pStartDate + "' "; //"yyyy-MM-dd HH:mm:ss"

                sSQL += "ORDER BY batchcode,doctype,imageseq,scanstart ";

                dtRet = mGlobal.objDB.ReturnRows(sSQL);

            }
            catch (Exception ex)
            {
                dtRet = null;
                throw new Exception(ex.Message);
            }

            return dtRet;
        }

        public DataRowCollection getIndexedDocFileBySetNumDB(string pCurrScanProj, string pAppNum, string pSetNum, string pStatusIN)
        {
            DataRowCollection dtRet = null;
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT * FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuIndex ";
                sSQL += "WHERE scanproj='" + pCurrScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND setnum='" + pSetNum.Trim().Replace("'", "") + "' ";
                //sSQL += "AND indexstation='" + pScanStn.Trim().Replace("'", "") + "' ";
                //sSQL += "AND indexuser='" + pScanUsr.Trim().Replace("'", "") + "' ";
                sSQL += "AND indexstatus IN (" + pStatusIN.Trim() + ") ";
                sSQL += "ORDER BY batchcode,doctype,imageseq,indexstart ";

                dtRet = mGlobal.objDB.ReturnRows(sSQL);

            }
            catch (Exception ex)
            {
                dtRet = null;
                throw new Exception(ex.Message);
            }

            return dtRet;
        }


        public DataRowCollection getDocFileByBatchCodeDB(string pCurrScanProj, string pAppNum, string pBatchCode, string pStatusIN)
        {
            DataRowCollection dtRet = null;
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT * FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuScan ";
                sSQL += "WHERE scanproj='" + pCurrScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";
                //sSQL += "AND scanstation='" + pScanStn.Trim().Replace("'", "") + "' ";
                //sSQL += "AND scanuser='" + pScanUsr.Trim().Replace("'", "") + "' ";
                sSQL += "AND scanstatus IN (" + pStatusIN.Trim() + ") ";
                sSQL += "ORDER BY setnum,doctype,imageseq,scanstart ";

                dtRet = mGlobal.objDB.ReturnRows(sSQL);

            }
            catch (Exception ex)
            {
                dtRet = null;
                throw new Exception(ex.Message);
            }

            return dtRet;
        }

        public DataRowCollection getIndexedDocFileByBatchCodeDB(string pCurrScanProj, string pAppNum, string pBatchCode, string pStatusIN)
        {
            DataRowCollection dtRet = null;
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT * FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuIndex ";
                sSQL += "WHERE scanproj='" + pCurrScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";
                //sSQL += "AND indexstation='" + pScanStn.Trim().Replace("'", "") + "' ";
                //sSQL += "AND indexuser='" + pScanUsr.Trim().Replace("'", "") + "' ";
                sSQL += "AND indexstatus IN (" + pStatusIN.Trim() + ") ";
                sSQL += "ORDER BY setnum,doctype,imageseq,indexstart ";

                dtRet = mGlobal.objDB.ReturnRows(sSQL);

            }
            catch (Exception ex)
            {
                dtRet = null;
                throw new Exception(ex.Message);
            }

            return dtRet;
        }

        public DataRowCollection getIndexedFieldValueByBatchCodeDB(string pCurrScanProj, string pAppNum, string pBatchCode, string pSetNum, string pDocDefId, string pPageId = "")
        {
            DataRowCollection dtRet = null;
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT DISTINCT * FROM (";
                sSQL += "SELECT fldvalue,fieldid FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TIndexFieldValue a ";
                sSQL += "INNER JOIN " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuDefiSet b ";
                //sSQL += "ON a.fieldid=b.rowid ";
                sSQL += "ON a.scanpjcode=b.scanpjcode and a.sysappnum=b.sysappnum and a.docdefid=b.docdefid ";
                sSQL += "WHERE a.scanpjcode='" + pCurrScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND a.sysappnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";

                if (pSetNum.Trim() != string.Empty)
                    sSQL += "AND a.setnum='" + pSetNum.Trim().Replace("'", "") + "' ";

                sSQL += "AND a.docdefid='" + pDocDefId.Trim().Replace("'", "") + "' ";

                if (pPageId.Trim() != string.Empty)
                    sSQL += "AND a.pageid='" + pPageId.Trim().Replace("'", "") + "' ";

                sSQL += "GROUP BY flddispseq,fieldid,fldvalue ";
                //sSQL += "ORDER BY b.flddispseq,fieldid ";
                sSQL += ") c ORDER BY fieldid ";

                dtRet = mGlobal.objDB.ReturnRows(sSQL);

            }
            catch (Exception ex)
            {
                dtRet = null;
                throw new Exception(ex.Message);
            }

            return dtRet;
        }

        public string getDocFilenameDB(string pRowid, string pTableName)
        {
            string sRet = "";
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT docimage FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo." + pTableName.Trim().Replace("'", "") + " ";
                sSQL += "WHERE rowid='" + pRowid.Trim().Replace("'", "") + "' ";

                DataRow dr = mGlobal.objDB.ReturnSingleRow(sSQL);

                if (dr != null)
                {
                    sRet = dr[0].ToString();
                }
                dr = null;
            }
            catch (Exception ex)
            {
                sRet = "";
                throw new Exception(ex.Message);                            
            }

            return sRet;
        }

        public string getFormatBatchNum(string pBatchNum, DateTime pCreatedDate, string pFormat)
        {
            string sRet;
            try
            {
                sRet = pBatchNum.Trim() + pCreatedDate.ToString(pFormat);
            }
            catch (Exception ex)
            {
                sRet = "";
                throw new Exception(ex.Message);
            }

            return sRet;
        }

        public string getBatchCodeNum(string pCurrScanProj, string pAppNum, string pBatchCode)
        {
            string iRet = "";
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT TOP 1 batchnum ";
                sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan ";
                sSQL += "WHERE scanproj='" + pCurrScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";

                DataRow dr = mGlobal.objDB.ReturnSingleRow(sSQL);

                if (dr != null)
                {
                    iRet = dr[0].ToString();
                }

            }
            catch (Exception ex)
            {
                iRet = "";
                //throw new Exception(ex.Message);                            
            }

            return iRet;
        }

        public Int32 getNewBatchNum(string pCurrScanProj, string pAppNum, DateTime pCreatedDate, string pBatchType)
        {
            Int32 iRet = -1;
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT IsNull(MAX(Convert(int,batchnum)),0) ";
                sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan ";
                sSQL += "WHERE scanproj='" + pCurrScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchtype='" + pBatchType.Trim().Replace("'", "") + "' ";
                sSQL += "AND Convert(varchar,createddate,103)='" + pCreatedDate.ToString("dd/MM/yyyy") + "' ";

                DataRow dr = mGlobal.objDB.ReturnSingleRow(sSQL);

                if (dr != null)
                {
                    iRet = (Int32)dr[0];
                    iRet = iRet + 1;
                }

            }
            catch (Exception ex)
            {
                iRet = -1;
                throw new Exception(ex.Message);                            
            }

            return iRet;
        }

        public Int32 getLastBatchNum(string pCurrScanProj, string pAppNum, string pBatchType)
        {
            Int32 iRet = -1;
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT IsNull(MAX(Convert(int,batchnum)),0) ";
                sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan ";
                sSQL += "WHERE scanproj='" + pCurrScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchtype='" + pBatchType.Trim().Replace("'", "") + "' ";
                sSQL += "AND Convert(varchar,createddate,103)='" + DateTime.Now.ToString("dd/MM/yyyy") + "' ";

                DataRow dr = mGlobal.objDB.ReturnSingleRow(sSQL);

                if (dr != null)
                {
                    iRet = (Int32)dr[0];
                    if (iRet == 0) iRet += 1;
                }

            }
            catch (Exception ex)
            {
                iRet = -1;
                //throw new Exception(ex.Message);                            
            }

            return iRet;
        }

        public Int32 getLastSetNum(string pCurrScanProj, string pAppNum, string pBatchCode = "")
        {
            Int32 iRet = -1;
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT IsNull(MAX(Convert(int,setnum)),0) ";
                sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan ";
                sSQL += "WHERE scanproj='" + pCurrScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchstatus<>'3' "; //not deleted.

                if (pBatchCode.Trim() != "")
                    sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";

                DataRow dr = mGlobal.objDB.ReturnSingleRow(sSQL);

                if (dr != null)
                {
                    iRet = (Int32)dr[0];
                }

            }
            catch (Exception ex)
            {
                iRet = -1;
                //throw new Exception(ex.Message);                            
            }

            return iRet;
        }

        public Int32 getLastCollaNum(string pCurrScanProj, string pAppNum, string pBatchCode, string pSetNum)
        {
            Int32 iRet = -1;
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT IsNull(MAX(Convert(int,collanum)),0) ";
                sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuScan ";
                sSQL += "WHERE scanproj='" + pCurrScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND setnum='" + pSetNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";
                sSQL += "AND scanstatus<>'D' "; //not deleted.

                DataRow dr = mGlobal.objDB.ReturnSingleRow(sSQL);

                if (dr != null)
                {
                    iRet = (Int32)dr[0];
                }

            }
            catch (Exception ex)
            {
                iRet = -1;
                //throw new Exception(ex.Message);                            
            }

            return iRet;
        }

        public Int32 getLastCollaNumIndex(string pCurrScanProj, string pAppNum, string pBatchCode, string pSetNum)
        {
            Int32 iRet = -1;
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT IsNull(MAX(Convert(int,collanum)),0) ";
                sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuIndex ";
                sSQL += "WHERE scanproj='" + pCurrScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND setnum='" + pSetNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";
                sSQL += "AND indexstatus<>'D' "; //not deleted.

                DataRow dr = mGlobal.objDB.ReturnSingleRow(sSQL);

                if (dr != null)
                {
                    iRet = (Int32)dr[0];
                }

            }
            catch (Exception ex)
            {
                iRet = -1;
                //throw new Exception(ex.Message);                            
            }

            return iRet;
        }

        public Int32 getCurrCollaNum(string pCurrScanProj, string pAppNum, string pBatchCode, string pSetNum, string pDocType)
        {
            Int32 iRet = -1;
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT IsNull(MAX(Convert(int,collanum)),0) ";
                sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuScan ";
                sSQL += "WHERE scanproj='" + pCurrScanProj.Trim().Replace("'","") + "' ";
                sSQL += "AND appnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND setnum='" + pSetNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";
                sSQL += "AND doctype='" + pDocType.Trim().Replace("'", "") + "' ";
                sSQL += "AND scanstatus<>'D' "; //not deleted.

                DataRow dr = mGlobal.objDB.ReturnSingleRow(sSQL);

                if (dr != null)
                {
                    iRet = (Int32)dr[0];
                }
                dr = null;
            }
            catch (Exception ex)
            {
                iRet = -1;
                mGlobal.Write2Log("Curr. Colla#" + ex.Message);
                //throw new Exception(ex.Message);                            
            }

            return iRet;
        }

        public Int32 getCurrCollaNumIndex(string pCurrScanProj, string pAppNum, string pBatchCode, string pSetNum, string pDocType)
        {
            Int32 iRet = -1;
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT IsNull(MAX(Convert(int,collanum)),0) ";
                sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuIndex ";
                sSQL += "WHERE scanproj='" + pCurrScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND setnum='" + pSetNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";
                sSQL += "AND doctype='" + pDocType.Trim().Replace("'", "") + "' ";
                sSQL += "AND indexstatus<>'D' "; //not deleted.

                DataRow dr = mGlobal.objDB.ReturnSingleRow(sSQL);

                if (dr != null)
                {
                    iRet = (Int32)dr[0];
                }
                dr = null;
            }
            catch (Exception ex)
            {
                iRet = -1;
                mGlobal.Write2Log("Curr. Colla#" + ex.Message);
                //throw new Exception(ex.Message);                            
            }

            return iRet;
        }

        public string getLastDocTypeDB(string pCurrScanProj, string pAppNum, string pSetNum, string pBatchCode)
        {
            string iRet = "";
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT TOP 1 IsNull(doctype,'') ";
                sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuScan ";
                sSQL += "WHERE scanproj='" + pCurrScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND setnum='" + pSetNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";
                sSQL += "ORDER BY setnum DESC, createddate DESC ";

                DataRow dr = mGlobal.objDB.ReturnSingleRow(sSQL);

                if (dr != null)
                {
                    iRet = dr[0].ToString();
                }

                dr = null;
            }
            catch (Exception ex)
            {
                iRet = "";
                throw new Exception(ex.Message);
            }

            return iRet;
        }

        public Int32 getLastDocTypeIdxImageSeqDB(string pCurrScanProj, string pAppNum, string pSetNum, string pDocType, string pBatchCode)
        {
            Int32 iRet = 0;
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT IsNull(MAX(imageseq),0) ";
                sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuIndex ";
                sSQL += "WHERE scanproj='" + pCurrScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND setnum='" + pSetNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";
                sSQL += "AND doctype='" + pDocType.Trim().Replace("'", "") + "' ";

                DataRow dr = mGlobal.objDB.ReturnSingleRow(sSQL);

                if (dr != null)
                {
                    iRet = Convert.ToInt32(dr[0].ToString());
                }

                dr = null;
            }
            catch (Exception ex)
            {
                iRet = 0;
                throw new Exception(ex.Message);
            }

            return iRet;
        }

        public Int32 getLastDocTypeImageSeqDB(string pCurrScanProj, string pAppNum, string pSetNum, string pDocType, string pBatchCode)
        {
            Int32 iRet = 0;
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT IsNull(MAX(imageseq),0) ";
                sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuScan ";
                sSQL += "WHERE scanproj='" + pCurrScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND setnum='" + pSetNum.Trim().Replace("'", "") + "' ";                
                sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";
                sSQL += "AND doctype='" + pDocType.Trim().Replace("'", "") + "' ";

                DataRow dr = mGlobal.objDB.ReturnSingleRow(sSQL);

                if (dr != null)
                {
                    iRet = Convert.ToInt32(dr[0].ToString());

                    if (iRet == 0)
                    {
                        mGlobal.LoadAppDBCfg();

                        sSQL = "SELECT IsNull(MAX(imageseq),0) ";
                        sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuIndex ";
                        sSQL += "WHERE scanproj='" + pCurrScanProj.Trim().Replace("'", "") + "' ";
                        sSQL += "AND appnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                        sSQL += "AND setnum='" + pSetNum.Trim().Replace("'", "") + "' ";
                        sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";
                        sSQL += "AND doctype='" + pDocType.Trim().Replace("'", "") + "' ";

                        dr = null;
                        dr = mGlobal.objDB.ReturnSingleRow(sSQL);

                        if (dr != null)
                        {
                            iRet = Convert.ToInt32(dr[0].ToString());
                        }
                    }
                }

                dr = null;
            }
            catch (Exception ex)
            {
                iRet = 0;
                throw new Exception(ex.Message);
            }

            return iRet;
        }

        public Int32 getLastImageSeqDB(string pCurrScanProj, string pAppNum, string pSetNum, string pCollanum, string pBatchCode)
        {
            Int32 iRet = 1;
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT IsNull(MAX(imageseq),1) ";
                sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuScan ";
                sSQL += "WHERE scanproj='" + pCurrScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND setnum='" + pSetNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND collanum='" + pCollanum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";

                DataRow dr = mGlobal.objDB.ReturnSingleRow(sSQL);

                if (dr != null)
                {
                    iRet = Convert.ToInt32(dr[0].ToString());

                    if (iRet == 0)
                    {
                        mGlobal.LoadAppDBCfg();

                        sSQL = "SELECT IsNull(MAX(imageseq),1) ";
                        sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuIndex ";
                        sSQL += "WHERE scanproj='" + pCurrScanProj.Trim().Replace("'", "") + "' ";
                        sSQL += "AND appnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                        sSQL += "AND setnum='" + pSetNum.Trim().Replace("'", "") + "' ";
                        sSQL += "AND collanum='" + pCollanum.Trim().Replace("'", "") + "' ";
                        sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";

                        dr = null;
                        dr = mGlobal.objDB.ReturnSingleRow(sSQL);

                        if (dr != null)
                        {
                            iRet = Convert.ToInt32(dr[0].ToString());
                        }
                    }
                }

                dr = null;
            }
            catch (Exception ex)
            {
                iRet = 1;
                throw new Exception(ex.Message);                            
            }

            return iRet;
        }

        public Int32 getValidImageSeq(string pCurrScanProj, string pAppNum, string pSetNum, string pCollanum, string pBatchCode, int iCurrSeq)
        {
            Int32 iRet = 1;
            try
            {
                iRet = getLastImageSeqDB(pCurrScanProj, pAppNum, pSetNum, pCollanum, pBatchCode);

                if (iRet == iCurrSeq)
                {
                    iRet = iRet + 1; //Set at bottom of the seq.
                }
                else
                {
                    if (iCurrSeq != 1)
                    {
                        iRet = iCurrSeq - 1; //Set at top of the seq.
                    }
                    else
                        iRet = 1; //Set first seq.
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return iRet;
        }

        public void reorderDocTypeScanImageSeq(string pCurrScanProj, string pAppNum, string pSetNum, string pDocType, string pBatchCode, int iCurrSeq)
        {
            int i = 0;
            string sSQL = "";
            try
            {
                int iSeq = 1;
                if (iCurrSeq != 1)
                    iSeq = iCurrSeq - 1;

                sSQL = "SELECT rowid FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuScan ";
                sSQL += "WHERE scanproj='" + pCurrScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";
                sSQL += "AND setnum='" + pSetNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND doctype='" + pDocType.Trim().Replace("'", "") + "' ";                
                sSQL += "AND imageseq>=" + iSeq + " ";
                sSQL += "ORDER BY imageseq,createddate DESC ";

                DataRowCollection drs = mGlobal.objDB.ReturnRows(sSQL);

                if (drs != null)
                {
                    if (drs.Count > 0)
                    {
                        sSQL = ""; string sRowid = "";

                        while (i < drs.Count)
                        {
                            sRowid = drs[i][0].ToString();

                            sSQL = "UPDATE " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuScan ";
                            sSQL += "SET imageseq=" + iSeq + " ";
                            sSQL += "WHERE rowid=" + sRowid + " ";
                            //sSQL += "WHERE scanproj='" + pCurrScanProj.Trim().Replace("'", "") + "' ";
                            //sSQL += "AND appnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                            //sSQL += "AND setnum='" + pSetNum.Trim().Replace("'", "") + "' ";
                            //sSQL += "AND collanum='" + pCollanum.Trim().Replace("'", "") + "' ";
                            //sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";
                            //sSQL += "AND imageseq>=" + iCurrSeq + " ";
                            //sSQL += "ORDER BY imageseq ";

                            mGlobal.objDB.UpdateRows(sSQL);

                            iSeq = iSeq + 1;
                            i += 1;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        public void reorderScanImageSeq(string pCurrScanProj, string pAppNum, string pSetNum, string pCollanum, string pBatchCode, int pCurrSeq, string pDocType = "")
        {
            int i = 0;
            string sSQL = "";
            try
            {
                int iSeq = 1;
                if (pCurrSeq != 1)
                    iSeq = pCurrSeq - 1;

                sSQL = "SELECT rowid FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuScan ";
                sSQL += "WHERE scanproj='" + pCurrScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";
                sSQL += "AND setnum='" + pSetNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND collanum='" + pCollanum.Trim().Replace("'", "") + "' ";                

                if (pDocType.Trim() != string.Empty)
                    sSQL += "AND doctype='" + pDocType.Trim().Replace("'", "") + "' ";

                sSQL += "AND imageseq>=" + iSeq + " ";
                sSQL += "ORDER BY imageseq,createddate DESC ";

                DataRowCollection drs = mGlobal.objDB.ReturnRows(sSQL);

                if (drs != null)
                {
                    if (drs.Count > 0)
                    {
                        sSQL = ""; string sRowid = "";

                        while (i < drs.Count)
                        {                            
                            sRowid = drs[i][0].ToString();

                            sSQL = "UPDATE " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuScan ";
                            sSQL += "SET imageseq=" + iSeq + " ";
                            sSQL += "WHERE rowid=" + sRowid + " ";
                            //sSQL += "WHERE scanproj='" + pCurrScanProj.Trim().Replace("'", "") + "' ";
                            //sSQL += "AND appnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                            //sSQL += "AND setnum='" + pSetNum.Trim().Replace("'", "") + "' ";
                            //sSQL += "AND collanum='" + pCollanum.Trim().Replace("'", "") + "' ";
                            //sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";
                            //sSQL += "AND imageseq>=" + iCurrSeq + " ";
                            //sSQL += "ORDER BY imageseq ";

                            mGlobal.objDB.UpdateRows(sSQL);

                            iSeq = iSeq + 1;
                            i += 1;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        public void reorderIndexImageSeq(string pCurrScanProj, string pAppNum, string pSetNum, string pCollanum, string pBatchCode, int iCurrSeq, string pDocType = "")
        {
            int i = 0;
            string sSQL = "";
            try
            {
                int iSeq = 1;
                if (iCurrSeq != 1)
                    iSeq = iCurrSeq - 1;

                sSQL = "SELECT rowid FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuIndex ";
                sSQL += "WHERE scanproj='" + pCurrScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";
                sSQL += "AND setnum='" + pSetNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND collanum='" + pCollanum.Trim().Replace("'", "") + "' ";                

                if (pDocType.Trim() != string.Empty)
                    sSQL += "AND doctype='" + pDocType.Trim().Replace("'", "") + "' ";

                sSQL += "AND imageseq>=" + iSeq + " ";
                sSQL += "ORDER BY imageseq,createddate DESC ";

                DataRowCollection drs = mGlobal.objDB.ReturnRows(sSQL);

                if (drs != null)
                {
                    if (drs.Count > 0)
                    {
                        sSQL = ""; string sRowid = "";

                        while (i < drs.Count)
                        {
                            sRowid = drs[i][0].ToString();

                            sSQL = "UPDATE " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuIndex ";
                            sSQL += "SET imageseq=" + iSeq + " ";
                            sSQL += "WHERE rowid=" + sRowid + " ";
                            //sSQL += "WHERE scanproj='" + pCurrScanProj.Trim().Replace("'", "") + "' ";
                            //sSQL += "AND appnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                            //sSQL += "AND setnum='" + pSetNum.Trim().Replace("'", "") + "' ";
                            //sSQL += "AND collanum='" + pCollanum.Trim().Replace("'", "") + "' ";
                            //sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";
                            //sSQL += "AND imageseq>=" + iCurrSeq + " ";
                            //sSQL += "ORDER BY imageseq ";

                            mGlobal.objDB.UpdateRows(sSQL);

                            iSeq = iSeq + 1;
                            i += 1;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        public Int32 getBatchCurrRowID(string pCurrScanProj, string pAppNum, string pBatchCode, string pStatusIn)
        {
            Int32 iRet = -1;
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT TOP 1 IsNull(rowid,0) ";
                sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan ";
                sSQL += "WHERE scanproj='" + pCurrScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchstatus IN (" + pStatusIn.Trim().Replace("'", "") + ") ";
                sSQL += "ORDER BY setnum DESC ";

                DataRow dr = mGlobal.objDB.ReturnSingleRow(sSQL);

                if (dr != null)
                {
                    iRet = (Int32)dr[0];
                }

            }
            catch (Exception ex)
            {
                iRet = -1;
                //throw new Exception(ex.Message);                            
            }

            return iRet;
        }

        public Int32 getBatchCurrRowID(string pCurrScanProj, string pAppNum, string pBatchCode, string pSetNum, string pStatusIn)
        {
            Int32 iRet = -1;
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT TOP 1 IsNull(rowid,0) ";
                sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan ";
                sSQL += "WHERE scanproj='" + pCurrScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND setnum='" + pSetNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchstatus IN (" + pStatusIn.Trim().Replace("'", "") + ") ";
                sSQL += "ORDER BY setnum DESC ";

                DataRow dr = mGlobal.objDB.ReturnSingleRow(sSQL);

                if (dr != null)
                {
                    iRet = (Int32)dr[0];
                }

            }
            catch (Exception ex)
            {
                iRet = -1;
                //throw new Exception(ex.Message);                            
            }

            return iRet;
        }

        public Int32 getTotPageCnt(string pCurrScanProj, string pAppNum, string pBatchCode, string pSetNum = "", string pDocType = "")
        {
            Int32 iRet = -1;
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                //sSQL = "SELECT IsNull(MAX(totpagecnt),0) ";
                //sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan ";
                //sSQL += "WHERE scanproj='" + pCurrScanProj.Trim().Replace("'", "") + "' ";
                //sSQL += "AND appnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                //sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";
                sSQL = "SELECT IsNull(COUNT(*),0) ";
                sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuScan ";
                sSQL += "WHERE scanproj='" + pCurrScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";

                if (pSetNum.Trim() != "")
                    sSQL += "AND setnum='" + pSetNum.Trim().Replace("'", "") + "' ";

                if (pDocType.Trim() != "")
                    sSQL += "AND doctype='" + pDocType.Trim().Replace("'", "") + "' ";

                //sSQL += "AND batchstatus<>'3' "; //not deleted.
                sSQL += "AND scanstatus<>'D' "; //not deleted.

                DataRow dr = mGlobal.objDB.ReturnSingleRow(sSQL);

                if (dr != null)
                {
                    iRet = (Int32)dr[0];
                }

            }
            catch (Exception ex)
            {
                iRet = -1;
                //throw new Exception(ex.Message);                            
            }

            return iRet;
        }

        public Int32 getTotPageCntIndex(string pCurrScanProj, string pAppNum, string pBatchCode, string pSetNum = "", string pDocType = "")
        {
            Int32 iRet = -1;
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                //sSQL = "SELECT IsNull(MAX(totpagecnt),0) ";
                //sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan ";
                //sSQL += "WHERE scanproj='" + pCurrScanProj.Trim().Replace("'", "") + "' ";
                //sSQL += "AND appnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                //sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";
                sSQL = "SELECT IsNull(COUNT(*),0) ";
                sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuIndex ";
                sSQL += "WHERE scanproj='" + pCurrScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";

                if (pSetNum.Trim() != "")
                    sSQL += "AND setnum='" + pSetNum.Trim().Replace("'", "") + "' ";

                if (pDocType.Trim() != "")
                    sSQL += "AND doctype='" + pDocType.Trim().Replace("'", "") + "' ";

                //sSQL += "AND batchstatus<>'3' "; //not deleted.
                sSQL += "AND indexstatus<>'D' "; //not deleted.

                DataRow dr = mGlobal.objDB.ReturnSingleRow(sSQL);

                if (dr != null)
                {
                    iRet = (Int32)dr[0];
                }

            }
            catch (Exception ex)
            {
                iRet = -1;
                //throw new Exception(ex.Message);                            
            }

            return iRet;
        }

        public bool updateBatchStatusDB(string pCurrScanProj, string pAppNum, string pBatchCode, string pBatchStatus,
            string pNewBatchStatus, string pScanStn = "", string pScanUsr = "", string pRemarks = "", string pFromProcess = "",
            string pIndexBy = "", string pIndexDate = "", string pVerifyBy = "", string pVerifyDate = "")
        {
            bool bUpdated = true;
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "UPDATE " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan ";
                sSQL += "SET batchstatus='" + pNewBatchStatus.Trim().Replace("'", "") + "' ";
                sSQL += ",fromproc='" + pFromProcess.Trim().Replace("'", "") + "' ";

                if (pBatchStatus == "1" && pNewBatchStatus == "2")
                {
                    sSQL += ",indexby='" + pIndexBy.Trim().Replace("'", "") + "' ";
                    sSQL += ",indexdate='" + pIndexDate.Trim() + "' ";
                }
                if (pBatchStatus == "6" && pNewBatchStatus == "2")
                {
                    sSQL += ",indexby='" + pIndexBy.Trim().Replace("'", "") + "' ";
                    sSQL += ",indexdate='" + pIndexDate.Trim() + "' ";
                }
                if (pBatchStatus == "2" && pNewBatchStatus == "5")
                {
                    sSQL += ",verifyby='" + pVerifyBy.Trim().Replace("'", "") + "' ";
                    sSQL += ",verifydate='" + pVerifyDate.Trim() + "' ";
                }
                if (pBatchStatus == "6" && pNewBatchStatus == "5")
                {
                    sSQL += ",verifyby='" + pVerifyBy.Trim().Replace("'", "") + "' ";
                    sSQL += ",verifydate='" + pVerifyDate.Trim() + "' ";
                }

                if (pRemarks.Trim() != "")
                    sSQL += ",remarks='" + pRemarks.Trim().Replace("'", "''") + "' ";

                sSQL += "WHERE scanproj='" + pCurrScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchstatus='" + pBatchStatus.Trim() + "' ";
                //sSQL += "AND scanstation='" + pScanStn.Trim().Replace("'", "") + "' ";
                //sSQL += "AND scanuser='" + pScanUsr.Trim().Replace("'", "") + "' ";

                if (mGlobal.objDB.UpdateRows(sSQL, false) == false)
                {
                    bUpdated = false;
                }

            }
            catch (Exception ex)
            {
                bUpdated = false;
                //throw new Exception(ex.Message);                            
            }

            return bUpdated;
        }

        public bool deleteRecordDB(int pRowID, string pDBTableName)
        {
            bool bDeleted = true;
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "DELETE FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo." + pDBTableName.Trim().Replace("'", "") + " ";
                sSQL += "WHERE rowid=" + pRowID + " ";
                
                if (mGlobal.objDB.UpdateRows(sSQL) == false)
                {
                    bDeleted = false;
                }

            }
            catch (Exception ex)
            {
                bDeleted = false;
                //throw new Exception(ex.Message);                            
            }

            return bDeleted;
        }

        public bool moveDirectories(string[] pSource, string pDest)
        {
            bool bMove = false;
            try
            {
                var stack = new Stack<Folders>();
                stack.Push(new Folders(pSource[0], pDest));
                while (stack.Count > 0)
                {
                    var folders = stack.Pop();
                    Directory.CreateDirectory(folders.Target);
                    foreach (var file in Directory.GetFiles(folders.Source, "*.*"))
                    {
                        string targetFile = Path.Combine(folders.Target, Path.GetFileName(file));

                        if (file.ToLower() != targetFile.ToLower())
                        {
                            if (File.Exists(targetFile)) File.Delete(targetFile); File.Move(file, targetFile);
                            bMove = true;
                        }
                    }

                    foreach (var folder in Directory.GetDirectories(folders.Source))
                    {
                        stack.Push(new Folders(folder, Path.Combine(folders.Target, Path.GetFileName(folder))));
                    }
                }

                if (bMove)
                    Directory.Delete(pSource[0], true);
            }
            catch (Exception ex)
            {
                bMove = false;
                mGlobal.Write2Log("Move.." + ex.Message);
            }
            return bMove;
        }

        public bool moveImgFile(string pSource, string pDest)
        {
            bool bMove = false;

            try
            {
                FileInfo oFI = new FileInfo(pSource);

                if (oFI != null)
                {
                    if (oFI.Exists)
                    {
                        FileInfo oDFI = oFI.CopyTo(pDest, true);

                        if (oDFI.Exists)
                            File.Delete(pSource);
                    }                    

                    bMove = true;
                }
            }
            catch (Exception ex)
            {
                bMove = false;
                mGlobal.Write2Log(ex.Message);
            }            

            return bMove;
        }

        public bool updateTotPageDB(string pCurrScanProj, string pAppNum, string pBatchCode, int pTotPage, string pModifiedDate = "",
            string pSetNum = "")
        {
            bool bUpdated = true;
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "UPDATE " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan ";
                sSQL += "SET totpagecnt=" + pTotPage.ToString() + " ";

                if (pModifiedDate.Trim() != "")
                    sSQL += ",modifieddate='" + pModifiedDate.Trim().Replace("'", "") + "' ";

                sSQL += "WHERE scanproj='" + pCurrScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";

                if (pSetNum.Trim() != "")
                    sSQL += "AND setnum='" + pSetNum.Trim().Replace("'", "") + "' ";

                bUpdated = mGlobal.objDB.UpdateRows(sSQL);
            }
            catch (Exception ex)
            {
                bUpdated = false;
                //throw new Exception(ex.Message);                            
            }

            return bUpdated;
        }

        public bool updateTotPageDB(string pCurrScanProj, string pAppNum, string pBatchCode, string pBatchStatus,
            int pTotPage, string pNewBatchStatus, string pScanStn = "", string pScanUsr = "", string pModifiedDate = "",
            string pSetNum = "")
        {
            bool bUpdated = true;
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "UPDATE " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan ";
                sSQL += "SET totpagecnt=" + pTotPage.ToString() + ", ";
                sSQL += "batchstatus='" + pNewBatchStatus.Trim().Replace("'", "") + "' "; 

                if (pModifiedDate.Trim() != "")
                    sSQL += ",modifieddate='" + pModifiedDate.Trim().Replace("'", "") + "' ";

                sSQL += "WHERE scanproj='" + pCurrScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchstatus='" + pBatchStatus.Trim() + "' ";

                if (pSetNum.Trim() != "")
                    sSQL += "AND setnum='" + pSetNum.Trim().Replace("'", "") + "' ";

                if (pScanStn.Trim() != "")
                    sSQL += "AND scanstation='" + pScanStn.Trim().Replace("'", "") + "' ";
                if (pScanUsr.Trim() != "")
                    sSQL += "AND scanuser='" + pScanUsr.Trim().Replace("'", "") + "' ";

                if (mGlobal.objDB.UpdateRows(sSQL, true) == false)
                {
                    bUpdated = false;
                }

            }
            catch (Exception ex)
            {
                bUpdated = false;
                //throw new Exception(ex.Message);                            
            }

            return bUpdated;
        }

        public bool checkBatchExist(string pScanProj, string pAppNum, string pBatchCode, string pSetNum, string pBatchType)
        {
            bool bRet = true;
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT IsNull(Count(*),0) FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan ";
                sSQL += "WHERE scanproj='" + pScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND setnum='" + pSetNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchtype='" + pBatchType.Trim().Replace("'", "") + "' ";

                DataRow dr = mGlobal.objDB.ReturnSingleRow(sSQL);
                if (dr != null)
                {
                    if (dr[0].ToString().Trim() == "0") bRet = false;
                }
                else
                    bRet = false;
            }
            catch (Exception ex)
            {
                bRet = false;
            }

            return bRet;
        }

        public bool checkDocuScanExist(string pScanProj, string pAppNum, string pBatchCode, string pSetNum, string pDocType)
        {
            bool bRet = true;
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT IsNull(Count(*),0) FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuScan ";
                sSQL += "WHERE scanproj='" + pScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND setnum='" + pSetNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";
                sSQL += "AND doctype='" + pDocType.Trim().Replace("'", "") + "' ";

                DataRow dr = mGlobal.objDB.ReturnSingleRow(sSQL);
                if (dr != null)
                {
                    if (dr[0].ToString().Trim() == "0") bRet = false;
                }
                else
                    bRet = false;

                dr = null;
            }
            catch (Exception ex)
            {
                bRet = false;
            }

            return bRet;
        }

        public bool checkDocuIndexExist(string pScanProj, string pAppNum, string pBatchCode, string pSetNum, string pDocType)
        {
            bool bRet = true;
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT IsNull(Count(*),0) FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuIndex ";
                sSQL += "WHERE scanproj='" + pScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND setnum='" + pSetNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";
                sSQL += "AND doctype='" + pDocType.Trim().Replace("'", "") + "' ";

                DataRow dr = mGlobal.objDB.ReturnSingleRow(sSQL);
                if (dr != null)
                {
                    if (dr[0].ToString().Trim() == "0") bRet = false;
                }
                else
                    bRet = false;
            }
            catch (Exception ex)
            {
                bRet = false;
            }

            return bRet;
        }

        public bool checkDocuSetIndexExist(string pScanProj, string pAppNum, string pBatchCode, string pSetNum, string pDocType, string pStatus, string pFilename)
        {
            bool bRet = true;
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT IsNull(Count(*),0) FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuIndex ";
                sSQL += "WHERE scanproj='" + pScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND setnum='" + pSetNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";
                sSQL += "AND doctype='" + pDocType.Trim().Replace("'", "") + "' ";
                sSQL += "AND indexstatus='" + pStatus.Trim() + "' ";
                sSQL += "AND docimage='" + pFilename.Trim().Replace("'", "") + "' ";

                DataRow dr = mGlobal.objDB.ReturnSingleRow(sSQL);
                if (dr != null)
                {
                    if (dr[0].ToString().Trim() == "0") bRet = false;
                }
                else
                    bRet = false;
            }
            catch (Exception ex)
            {
                bRet = false;
            }

            return bRet;
        }

        public bool checkDocuSetIndexExistIn(string pScanProj, string pAppNum, string pBatchCode, string pSetNum, string pDocType, string pStatusIn, string pFilename)
        {
            bool bRet = true;
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT IsNull(Count(*),0) FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuIndex ";
                sSQL += "WHERE scanproj='" + pScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND setnum='" + pSetNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";
                sSQL += "AND doctype='" + pDocType.Trim().Replace("'", "") + "' ";
                sSQL += "AND indexstatus IN (" + pStatusIn.Trim() + ") ";
                sSQL += "AND docimage='" + pFilename.Trim().Replace("'", "") + "' ";

                DataRow dr = mGlobal.objDB.ReturnSingleRow(sSQL);
                if (dr != null)
                {
                    if (dr[0].ToString().Trim() == "0") bRet = false;
                }
                else
                    bRet = false;
            }
            catch (Exception ex)
            {
                bRet = false;
            }

            return bRet;
        }

        public bool checkDocuSetIndexFieldExist(string pScanProj, string pAppNum, string pBatchCode, string pSetNum, string pDocDefId)
        {
            bool bRet = true;
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT IsNull(Count(*),0) FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TIndexFieldValue ";
                sSQL += "WHERE scanpjcode='" + pScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND sysappnum='" + pAppNum.Trim().Replace("'", "") + "' ";                
                sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";
                sSQL += "AND setnum='" + pSetNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND docdefid='" + pDocDefId.Trim().Replace("'", "") + "' ";

                DataRow dr = mGlobal.objDB.ReturnSingleRow(sSQL);
                if (dr != null)
                {
                    if (dr[0].ToString().Trim() == "0") bRet = false;
                }
                else
                    bRet = false;
            }
            catch (Exception ex)
            {
                bRet = false;
            }

            return bRet;
        }

        public List<clsDocument> findDuplicateDocType(List<string> pLst)
        {
            List<clsDocument> lDoc = new List<clsDocument>();
            List<clsDocument> lDocDup = new List<clsDocument>();
            clsDocument oDoc = new clsDocument();
            int iSetIdx = 0;
            List<int> setIdx = new List<int>();
            try
            {
                int k = 0;
                //Find Set Separator Indexes.
                while (k < pLst.Count)
                {
                    iSetIdx = pLst.FindIndex(k, a => a.Contains(staMain.stcProjCfg.SeparatorText));
                    if (iSetIdx != -1)
                    {
                        setIdx.Add(iSetIdx);
                        k = iSetIdx;
                    }

                    k += 1;
                }

                //Get all doc type separator index which duplicate in the set.
                k = 0; //Init;
                int cur = 0;
                string sDocType = "";

                while (k < setIdx.Count)
                {
                    cur = setIdx[k] + 1; //Separator + 1 advance one step for doc type.
                    while (pLst[cur].ToUpper().Trim().IndexOf(staMain.stcProjCfg.SeparatorText) == -1) //if find one doc type continue find more doc type.
                    {
                        sDocType = pLst[cur].ToUpper().Trim(); //Assume 2 separator configured.
                        if (pLst.FindIndex(cur, a => a.Contains(sDocType)) != -1) //if duplicate index and is doc type separator.
                        {
                            oDoc.Set = pLst[setIdx[k]] + "_" + (k + 1).ToString("000");
                            oDoc.DocType = sDocType;
                            oDoc.SetIdx = setIdx[k];
                            oDoc.TypeIdx = cur;

                            lDoc.Add(oDoc);
                            oDoc = new clsDocument();
                            //lDup.Add(pLst[cur]);
                        }

                        cur = cur + 1;
                        if (cur >= pLst.Count) break;
                    }

                    k += 1;
                }

                lDocDup = lDoc.GroupBy(x => new { x.SetIdx, x.DocType })
                    .Where(g => g.Count() > 1)
                    .Select(x => x.Last()).ToList();

            }
            catch (Exception ex)
            {
                mGlobal.Write2Log("Duplicate.." + ex.Message);
                mGlobal.Write2Log(ex.StackTrace.ToString());
            }

            return lDocDup;
        }

        public bool checkDocuSetIndexFieldExist(string pScanProj, string pAppNum, string pBatchCode, string pSetNum, 
            string pDocDefId, string pFieldId = "", string pPageId = "")
        {
            bool bRet = true;
            string sSQL = "";
            try
            {
                if (mGlobal.objDB == null)
                {
                    mGlobal.objDB = new clsUtilComn.clsDatabase();
                }

                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT IsNull(Count(*),0) FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TIndexFieldValue ";
                sSQL += "WHERE scanpjcode='" + pScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND sysappnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND docdefid='" + pDocDefId.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";
                sSQL += "AND setnum='" + pSetNum.Trim().Replace("'", "") + "' ";                

                if (pFieldId.Trim() != string.Empty)
                    sSQL += "AND fieldid='" + pFieldId.Trim().Replace("'", "") + "' ";

                if (pPageId.Trim() != string.Empty)
                    sSQL += "AND pageid='" + pPageId.Trim().Replace("'", "") + "' ";

                DataRow dr = mGlobal.objDB.ReturnSingleRow(sSQL);
                if (dr != null)
                {
                    if (dr[0].ToString().Trim() == "0") bRet = false;
                }
                else
                    bRet = false;
            }
            catch (Exception ex)
            {
                bRet = false;
                mGlobal.Write2Log("Index check.." + ex.Message);
                throw new Exception(ex.Message);
            }

            return bRet;
        }

        public bool checkDocuSetKeyFieldExist(string pScanProj, string pAppNum, string pBatchCode, string pSetNum,
            string pDocType, string pPageId = "", string pFieldId = "")
        {
            bool bRet = true;
            string sSQL = "";
            try
            {
                if (mGlobal.objDB == null)
                {
                    mGlobal.objDB = new clsUtilComn.clsDatabase();
                }

                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT IsNull(Count(*),0) FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TKeyFieldValue ";
                sSQL += "WHERE scanpjcode='" + pScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND sysappnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";
                sSQL += "AND setnum='" + pSetNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND doctype='" + pDocType.Trim().Replace("'", "") + "' ";

                if (pPageId.Trim() != string.Empty)
                    sSQL += "AND pageid='" + pPageId.Trim().Replace("'", "") + "' ";

                if (pFieldId.Trim() != string.Empty)
                    sSQL += "AND fieldid='" + pFieldId.Trim().Replace("'", "") + "' ";

                DataRow dr = mGlobal.objDB.ReturnSingleRow(sSQL);
                if (dr != null)
                {
                    if (dr[0].ToString().Trim() == "0") bRet = false;
                }
                else
                    bRet = false;
            }
            catch (Exception ex)
            {
                bRet = false;
                mGlobal.Write2Log("Key field check.." + ex.Message);
                throw new Exception(ex.Message);
            }

            return bRet;
        }

        public int getOCRAreaMainDB(string pScanProj, string pAppNum)
        {
            int iMainId = 0;
            try
            {
                mGlobal.LoadAppDBCfg();

                string sSQL = "SELECT TOP 1 rowid FROM " + mGlobal.objDB.sDBName.Trim().Replace("'", "") + ".dbo.TContentAreaMain ";
                sSQL += "WHERE scanproj='" + pScanProj.ToString().Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + pAppNum.ToString().Trim().Replace("'", "") + "' ";

                DataRow oDr = mGlobal.objDB.ReturnSingleRow(sSQL);

                if (oDr != null)
                {
                    iMainId = Convert.ToInt32(oDr[0].ToString());
                }
            }
            catch (Exception ex)
            {
            }

            return iMainId;
        }

        public void getOCRAreaDB(int iMainid)
        {
            try
            {
                stcOCRAreaList = new List<_stcOCRAreaInfo>();
                _stcOCRAreaInfo stcOCRArea = new _stcOCRAreaInfo();

                mGlobal.LoadAppDBCfg();

                string sSQL = "SELECT DISTINCT * FROM " + mGlobal.objDB.sDBName.Trim().Replace("'", "") + ".dbo.TContentArea ";
                sSQL += "WHERE mainid=" + iMainid.ToString().Trim().Replace("'", "") + " ";
                sSQL += "ORDER BY rowid ";

                DataRowCollection oDrs = mGlobal.objDB.ReturnRows(sSQL);

                if (oDrs != null)
                {
                    int i = 0;
                    while (i < oDrs.Count)
                    {
                        stcOCRArea.name = oDrs[i]["name"].ToString();
                        stcOCRArea.left = Convert.ToInt32(oDrs[i]["aleft"]);
                        stcOCRArea.top = Convert.ToInt32(oDrs[i]["atop"]);
                        stcOCRArea.right = Convert.ToInt32(oDrs[i]["aright"]);
                        stcOCRArea.bottom = Convert.ToInt32(oDrs[i]["abottom"]);
                        stcOCRArea.width = Convert.ToInt32(oDrs[i]["width"]);
                        stcOCRArea.height = Convert.ToInt32(oDrs[i]["height"]);

                        stcOCRAreaList.Add(stcOCRArea);
                        i += 1;
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        public string getBatchBoxNum(string pCurrScanProj, string pAppNum, string pBatchCode)
        {
            string iRet = "";
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT TOP 1 boxnum ";
                sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan ";
                sSQL += "WHERE scanproj='" + pCurrScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";

                DataRow dr = mGlobal.objDB.ReturnSingleRow(sSQL);

                if (dr != null)
                {
                    iRet = dr[0].ToString();
                }

            }
            catch (Exception ex)
            {
                iRet = "";
                //throw new Exception(ex.Message);                            
            }

            return iRet;
        }
        public string getBatchBoxLabel(string pCurrScanProj, string pAppNum, string pBatchCode)
        {
            string iRet = "";
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT TOP 1 boxlabel ";
                sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan ";
                sSQL += "WHERE scanproj='" + pCurrScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";

                DataRow dr = mGlobal.objDB.ReturnSingleRow(sSQL);

                if (dr != null)
                {
                    iRet = dr[0].ToString();
                }
            }
            catch (Exception ex)
            {
                iRet = "";
                //throw new Exception(ex.Message);                            
            }

            return iRet;
        }

        public string getExpFilenameFmt(string pScanProj, string pAppNum, string pBatchCode, string pSetNum, string pDocType, int pRunningNum)
        {
            string sFilename = "";
            string[] sDelims = null;
            string[] sFormats = null;
            string sFormat = "";
            string[] sFldDelims = null;
            string sFldName = "";
            bool bIsDelim = true;
            Dictionary<int, string> dFormat = new Dictionary<int, string>();
            clsDocuFile oDF = new clsDocuFile();
            try
            {
                if (staMain.stcProjCfg.ExpFileFmt != null)
                {
                    sFormat = staMain.stcProjCfg.ExpFileFmt.Trim();
                }

                if (sFormat != "")
                {
                    sDelims = sFormat.Split('_');
                    if (sDelims.Length == 1)
                    {
                        sFldDelims = sFormat.Split(new string[] { "]" }, StringSplitOptions.RemoveEmptyEntries);
                        bIsDelim = false;
                    }

                    int i = 0; int j = 0; int iKey = 0; int iIdx = 0; string sValue = "";
                    if (!bIsDelim)
                    {
                        while (i < sFldDelims.Length)
                        {
                            sFormat = sFldDelims[i];

                            if (sFormat.IndexOf("<") != -1)
                                sFormats = sFormat.Split(new string[] { ">" }, StringSplitOptions.RemoveEmptyEntries);

                            if (sFormats != null)
                            {
                                j = 0;
                                while (j < sFormats.Length)
                                {
                                    sFormat = sFormats[j];
                                    if (sFormat.IndexOf("<") == -1)
                                    {
                                        sFldName = sFormat.Replace("[", "").Replace("]", "");

                                        if (sFldName != string.Empty)
                                        {
                                            iIdx = frmExport.getKeyIndexIdDB(pScanProj, pAppNum, staMain.stcProjCfg.DocDefId, sFldName);
                                            sValue = frmExport.getKeyIndexValueDB(pScanProj, pAppNum, pSetNum, pBatchCode, staMain.stcProjCfg.DocDefId, iIdx);

                                            if (sValue.Trim() != string.Empty)
                                            {
                                                dFormat.Add(iKey, sValue);

                                                iKey += 1;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        dFormat = getExpFilenameSysFmt(sFormat, "", sFldDelims, iKey, dFormat, pScanProj, pAppNum, pBatchCode, pSetNum, pDocType, pRunningNum);
                                        iKey = dFormat.Count;
                                    }

                                    j += 1;
                                }
                            }
                            else
                            {
                                sFldName = sFormat.Replace("[", "").Replace("]", "");

                                if (sFldName != string.Empty)
                                {
                                    iIdx = frmExport.getKeyIndexIdDB(pScanProj, pAppNum, staMain.stcProjCfg.DocDefId, sFldName);
                                    sValue = frmExport.getKeyIndexValueDB(pScanProj, pAppNum, pSetNum, pBatchCode, staMain.stcProjCfg.DocDefId, iIdx);

                                    if (sValue.Trim() != string.Empty)
                                    {
                                        dFormat.Add(iKey, sValue);

                                        iKey += 1;
                                    }
                                }
                            }

                            //sFormat = sFldDelims[i].Trim();

                            //dFormat = getExpFilenameSysFmt(sFormat, "", sFldDelims, iKey, dFormat, pScanProj, pAppNum, pBatchCode, pSetNum, pRunningNum);
                            //iKey = dFormat.Count;

                            sFilename = "";
                            sFormats = null;
                            
                            i += 1;
                        }
                    }
                    else //Has delimiter.
                    {
                        int k = 0;
                        while (i < sDelims.Length) 
                        {
                            sFormat = sDelims[i].Trim();

                            if (sFormat.IndexOf("[") != -1)
                                sFldDelims = sFormat.Split(new string[] { "]" }, StringSplitOptions.RemoveEmptyEntries);

                            if (sFldDelims != null)
                            {
                                j = 0;
                                while (j < sFldDelims.Length)
                                {
                                    sFormat = sFldDelims[j];

                                    if (sFormat.IndexOf("<") != -1)
                                        sFormats = sFormat.Split(new string[] { ">" }, StringSplitOptions.RemoveEmptyEntries);

                                    if (sFormats != null)
                                    {
                                        k = 0;
                                        while (k < sFormats.Length)
                                        {
                                            sFormat = sFormats[k];
                                            if (sFormat.IndexOf("<") == -1)
                                            {
                                                sFldName = sFormat.Replace("[", "").Replace("]", "");

                                                if (sFldName != string.Empty)
                                                {
                                                    iIdx = frmExport.getKeyIndexIdDB(pScanProj, pAppNum, staMain.stcProjCfg.DocDefId, sFldName);
                                                    sValue = frmExport.getKeyIndexValueDB(pScanProj, pAppNum, pSetNum, pBatchCode, staMain.stcProjCfg.DocDefId, iIdx);

                                                    if (sValue.Trim() != string.Empty)
                                                    {
                                                        dFormat.Add(iKey, sValue);

                                                        iKey += 1;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                dFormat = getExpFilenameSysFmt(sFormat, "", sDelims, iKey, dFormat, pScanProj, pAppNum, pBatchCode, pSetNum, pDocType, pRunningNum);
                                                iKey = dFormat.Count;
                                            }

                                            k += 1;
                                        } //End while
                                    }
                                    else
                                    {
                                        sFldName = sFormat.Replace("[", "").Replace("]", "");

                                        if (sFldName != string.Empty)
                                        {
                                            iIdx = frmExport.getKeyIndexIdDB(pScanProj, pAppNum, staMain.stcProjCfg.DocDefId, sFldName);
                                            sValue = frmExport.getKeyIndexValueDB(pScanProj, pAppNum, pSetNum, pBatchCode, staMain.stcProjCfg.DocDefId, iIdx);

                                            if (sValue.Trim() != string.Empty)
                                            {
                                                dFormat.Add(iKey, sValue);

                                                iKey += 1;
                                            }
                                        }
                                    }                                        

                                    j += 1;
                                } //End While
                            }
                            else
                            {
                                if (sFormat.IndexOf("<") != -1)
                                    sFormats = sFormat.Split(new string[] { ">" }, StringSplitOptions.RemoveEmptyEntries);

                                if (sFormats != null)
                                {
                                    k = 0;
                                    while (k < sFormats.Length)
                                    {
                                        sFormat = sFormats[k];

                                        dFormat = getExpFilenameSysFmt(sFormat, "", sDelims, iKey, dFormat, pScanProj, pAppNum, pBatchCode, pSetNum, pDocType, pRunningNum);
                                        iKey = dFormat.Count;

                                        k += 1;
                                    }                                    
                                }                                
                            }

                            if (i != sDelims.Length - 1)
                            {
                                dFormat.Add(iKey, "_");
                                iKey += 1;
                            }                            

                            sFilename = "";
                            sFormats = null;
                            sFldDelims = null;                            
                            i += 1;
                        } //End while
                    }

                    j = 0;
                    while (j < dFormat.Count)
                    {
                        sFilename += dFormat[j];

                        j += 1;
                    }
                    
                }               

            }
            catch (Exception ex)
            {
            }
            
            return sFilename;
        }

        private Dictionary<int, string> getExpFilenameSysFmt(string pFormat, string pDelim, string[] pDelims, int pKey, Dictionary<int, string> pDFormat,
            string pScanProj, string pAppNum, string pBatchCode, string pSetNum, string pDocType, int pRunningNum)
        {
            clsDocuFile oDF = new clsDocuFile();
            string sFilename = "";
            string sBatchNum = "";
            string sBoxNum = "";
            string sRunFmt = "0";
            try
            {
                if (staMain.stcProjCfg.ExpRunDigitsFmt.Trim() != "" && staMain.stcProjCfg.ExpRunDigitsFmt.Trim() != "0")
                {
                    int iCnt = Convert.ToInt32(staMain.stcProjCfg.ExpRunDigitsFmt.Trim());
                    int i = 1;
                    while (i < iCnt)
                    {
                        sRunFmt += "0";

                        i += 1;
                    }
                }


                if (pFormat.IndexOf("<") == -1) pFormat = "<" + pFormat.Trim();
                if (pFormat.IndexOf(">") == -1) pFormat = pFormat.Trim() + ">";

                if (pDelim == string.Empty)
                {
                    if (pFormat.Contains("<scan proj code>"))
                    {
                        pDFormat.Add(pKey, pScanProj);
                        pKey += 1;
                    }
                    else if (pFormat.Contains("<sys app no>"))
                    {
                        pDFormat.Add(pKey, pAppNum);
                        pKey += 1;
                    }
                    else if (pFormat.Contains("<batch running no>"))
                    {
                        sBatchNum = oDF.getBatchCodeNum(pScanProj, pAppNum, pBatchCode);
                        pDFormat.Add(pKey, sBatchNum);
                        pKey += 1;
                    }
                    else if (pFormat.Contains("<set running no>"))
                    {
                        pDFormat.Add(pKey, pSetNum);
                        pKey += 1;
                    }
                    else if (pFormat.Contains("<document type>"))
                    {
                        pDFormat.Add(pKey, pDocType);
                        pKey += 1;
                    }
                    else if (pFormat == "<yyyyMMdd>")
                    {
                        pDFormat.Add(pKey, oDF.getFormatBatchNum(sFilename, DateTime.Now, "yyyyMMdd"));
                        pKey += 1;
                    }
                    else if (pFormat == "<ddMMyyyy>")
                    {
                        pDFormat.Add(pKey, oDF.getFormatBatchNum(sFilename, DateTime.Now, "ddMMyyyy"));
                        pKey += 1;
                    }
                    else if (pFormat == "<hhmmssttt>")
                    {
                        pDFormat.Add(pKey, oDF.getFormatBatchNum(sFilename, DateTime.Now, "hhmmssfff"));
                        pKey += 1;
                    }
                    else if (pFormat == "<tttssmmhh>")
                    {
                        pDFormat.Add(pKey, oDF.getFormatBatchNum(sFilename, DateTime.Now, "fffssmmhh"));
                        pKey += 1;
                    }
                    else if (pFormat == "<box no>")
                    {
                        sBoxNum = oDF.getBatchBoxNum(pScanProj, pAppNum, pBatchCode);
                        pDFormat.Add(pKey, sBoxNum);
                        pKey += 1;
                    }
                    else if (pFormat == "<box label>")
                    {
                        sBoxNum = oDF.getBatchBoxLabel(pScanProj, pAppNum, pBatchCode);
                        pDFormat.Add(pKey, sBoxNum);
                        pKey += 1;
                    }
                    else if (pFormat == "<reference>")
                    {
                        pDFormat.Add(pKey, pBatchCode);
                        pKey += 1;
                    }
                    else if (pFormat == "<export no>")
                    {                        
                        if (sRunFmt.Trim() == "0")
                            pDFormat.Add(pKey, pRunningNum.ToString());
                        else
                            pDFormat.Add(pKey, pRunningNum.ToString(sRunFmt));

                        pKey += 1;
                    }
                }
                else
                {
                    if (pFormat.Contains("<scan proj code>"))
                    {
                        if (pKey == pDelims.Length - 1) pDFormat.Add(pKey, "_" + pScanProj); else pDFormat.Add(pKey, pScanProj + "_");
                        pKey += 1;
                    }
                    else if (pFormat.Contains("<sys app no>"))
                    {
                        if (pKey == pDelims.Length - 1) pDFormat.Add(pKey, "_" + pAppNum); else pDFormat.Add(pKey, pAppNum + "_");
                        pKey += 1;
                    }
                    else if (pFormat.Contains("<batch running no>"))
                    {
                        sBatchNum = oDF.getBatchCodeNum(pScanProj, pAppNum, pBatchCode);
                        if (pKey == pDelims.Length - 1) pDFormat.Add(pKey, "_" + sBatchNum); else pDFormat.Add(pKey, sBatchNum + "_");
                        pKey += 1;
                    }
                    else if (pFormat.Contains("<set running no>"))
                    {
                        if (pKey == pDelims.Length - 1) pDFormat.Add(pKey, "_" + pSetNum); else pDFormat.Add(pKey, pSetNum + "_");
                        pKey += 1;
                    }
                    else if (pFormat.Contains("<document type>"))
                    {
                        if (pKey == pDelims.Length - 1) pDFormat.Add(pKey, "_" + pDocType); else pDFormat.Add(pKey, pDocType + "_");
                        pKey += 1;
                    }
                    else if (pFormat == "<yyyyMMdd>")
                    {
                        if (pKey == pDelims.Length - 1)
                        {
                            pDFormat.Add(pKey, "_" + oDF.getFormatBatchNum(sFilename, DateTime.Now, "yyyyMMdd"));
                        }
                        else
                        {
                            pDFormat.Add(pKey, oDF.getFormatBatchNum(sFilename, DateTime.Now, "yyyyMMdd") + "_");
                        }
                        pKey += 1;
                    }
                    else if (pFormat == "<ddMMyyyy>")
                    {
                        if (pKey == pDelims.Length - 1)
                        {
                            pDFormat.Add(pKey, "_" + oDF.getFormatBatchNum(sFilename, DateTime.Now, "ddMMyyyy"));
                        }
                        else
                        {
                            pDFormat.Add(pKey, oDF.getFormatBatchNum(sFilename, DateTime.Now, "ddMMyyyy") + "_");
                        }
                        pKey += 1;
                    }
                    else if (pFormat == "<hhmmssttt>")
                    {
                        if (pKey == pDelims.Length - 1)
                        {
                            pDFormat.Add(pKey, "_" + oDF.getFormatBatchNum(sFilename, DateTime.Now, "hhmmssfff"));
                        }
                        else
                        {
                            pDFormat.Add(pKey, oDF.getFormatBatchNum(sFilename, DateTime.Now, "hhmmssfff") + "_");
                        }
                        pKey += 1;
                    }
                    else if (pFormat == "<tttssmmhh>")
                    {
                        if (pKey == pDelims.Length - 1)
                        {
                            pDFormat.Add(pKey, "_" + oDF.getFormatBatchNum(sFilename, DateTime.Now, "fffssmmhh"));
                        }
                        else
                        {
                            pDFormat.Add(pKey, oDF.getFormatBatchNum(sFilename, DateTime.Now, "fffssmmhh") + "_");
                        }
                        pKey += 1;
                    }
                    else if (pFormat == "<box no>")
                    {
                        sBoxNum = oDF.getBatchBoxNum(pScanProj, pAppNum, pBatchCode);
                        if (pKey == pDelims.Length - 1)
                        {
                            pDFormat.Add(pKey, "_" + sBoxNum);
                        }
                        else
                        {
                            pDFormat.Add(pKey, sBoxNum + "_");
                        }
                        pKey += 1;
                    }
                    else if (pFormat == "<box label>")
                    {
                        sBoxNum = oDF.getBatchBoxLabel(pScanProj, pAppNum, pBatchCode);
                        if (pKey == pDelims.Length - 1)
                        {
                            pDFormat.Add(pKey, "_" + sBoxNum);
                        }
                        else
                        {
                            pDFormat.Add(pKey, sBoxNum + "_");
                        }
                        pKey += 1;
                    }
                    else if (pFormat == "<reference>")
                    {
                        if (pKey == pDelims.Length - 1)
                        {
                            pDFormat.Add(pKey, "_" + pBatchCode);
                        }
                        else
                        {
                            pDFormat.Add(pKey, pBatchCode + "_");
                        }
                        pKey += 1;
                    }
                    else if (pFormat == "<export no>")
                    {
                        if (sRunFmt.Trim() == "0")
                        {
                            if (pKey == pDelims.Length - 1)
                            {
                                pDFormat.Add(pKey, "_" + pRunningNum.ToString());
                            }
                            else
                            {
                                pDFormat.Add(pKey, pRunningNum.ToString() + "_");
                            }
                        }
                        else
                        {
                            if (pKey == pDelims.Length - 1)
                            {
                                pDFormat.Add(pKey, "_" + pRunningNum.ToString(sRunFmt));
                            }
                            else
                            {
                                pDFormat.Add(pKey, pRunningNum.ToString(sRunFmt) + "_");
                            }
                        }
                        pKey += 1;
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return pDFormat;
        }

        public string getIndexedFieldValueListByValueDB(string pCurrScanProj, string pAppNum, string pDocDefId, string pFldName, string pListValue)
        {
            string sRet = "";
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT fldname,valname FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TValueList a ";
                sSQL += "WHERE a.scanpjcode='" + pCurrScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND a.sysappnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND a.docdefid='" + pDocDefId.Trim().Replace("'", "") + "' ";
                sSQL += "AND a.fldname='" + pFldName.Trim().Replace("'", "") + "' ";
                sSQL += "AND a.listtype='Value List' ";
                sSQL += "AND a.namevalue='" + pListValue.Trim().Replace("'", "") + "' ";

                DataRow oDR = mGlobal.objDB.ReturnSingleRow(sSQL);

                if (oDR != null)
                {
                    sRet = oDR[1].ToString();
                }

                oDR = null;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return sRet;
        }

        public bool deleteAllIndexedRecordDB(string pScanProj, string pAppNum, string pBatchCode)
        {
            bool bDeleted = true;
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "DELETE FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TIndexFieldValue ";
                sSQL += "WHERE scanpjcode='" + pScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND sysappnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";

                if (mGlobal.objDB.UpdateRows(sSQL, false) == false)
                {
                    bDeleted = false;
                }

            }
            catch (Exception ex)
            {
                bDeleted = false;
                //throw new Exception(ex.Message);                            
            }

            return bDeleted;
        }

        public bool deleteSetIndexedRecordDB(string pScanProj, string pAppNum, string pBatchCode, string pSetNum)
        {
            bool bDeleted = true;
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "DELETE FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TIndexFieldValue ";
                sSQL += "WHERE scanpjcode='" + pScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND sysappnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";
                sSQL += "AND setnum='" + pSetNum.Trim().Replace("'", "") + "' ";

                if (mGlobal.objDB.UpdateRows(sSQL, false) == false)
                {
                    bDeleted = false;
                }
            }
            catch (Exception ex)
            {
                bDeleted = false;
                //throw new Exception(ex.Message);                            
            }

            return bDeleted;
        }

        public bool updatePageIndexFieldDB(string pScanProj, string pAppNum, string pBatchCode, string pSetNum, string pDocDefId, 
            string pOldPageId, string pPageId)
        {
            bool bSaved = true;
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "UPDATE " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TIndexFieldValue ";
                sSQL += "SET pageid='" + pPageId.Trim().Replace("'", "") + "' ";
                sSQL += "WHERE scanpjcode='" + pScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND sysappnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";

                if (pSetNum.Trim() != string.Empty)
                    sSQL += "AND setnum='" + pSetNum.Trim().Replace("'", "") + "' ";

                sSQL += "AND docdefid='" + pDocDefId.Trim().Replace("'", "") + "' ";

                if (pOldPageId.Trim() != string.Empty)
                    sSQL += "AND pageid=" + pOldPageId.Trim().Replace("'", "") + " ";

                bSaved = mGlobal.objDB.UpdateRows(sSQL, true);
            }
            catch (Exception ex)
            {
                bSaved = false;
                //throw new Exception(ex.Message);                            
            }

            return bSaved;
        }

        public bool reorderPageIndexFieldDB(string pScanProj, string pAppNum, string pBatchCode, string pSetNum, string pDocType = "")
        {
            try
            {
                string sSQL = "";
                string sSetNo = "";

                sSQL = "SELECT rowid,imageseq FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuScan ";
                sSQL += "WHERE scanproj='" + pScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";

                if (pSetNum.Trim() != string.Empty)
                    sSQL += "AND setnum='" + pSetNum.Trim().Replace("'", "") + "' ";

                if (pDocType.Trim() != string.Empty)
                    sSQL += "AND doctype='" + pDocType.Trim().Replace("'", "") + "' ";

                sSQL += "ORDER BY imageseq,createddate DESC ";

                DataRowCollection drs = mGlobal.objDB.ReturnRows(sSQL);

                if (drs != null)
                {
                    if (drs.Count > 0)
                    {
                        sSQL = ""; string sRowid = ""; int iImgSeq = 1;
                        int i = 0;
                        while (i < drs.Count)
                        {
                            sRowid = drs[i][0].ToString();
                            iImgSeq = Convert.ToInt32(drs[i][1].ToString());

                            if (staMain.stcProjCfg.NoSeparator.Trim() == "0")
                            {
                                sSetNo = iImgSeq.ToString(staMain.sSetNumFmt);
                            }                                
                            else
                                sSetNo = pSetNum;

                            sSQL = "UPDATE " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TIndexFieldValue ";
                            if (staMain.stcProjCfg.NoSeparator.Trim() == "0")
                                sSQL += "SET setnum='" + sSetNo + "' ";
                            else
                                sSQL += "SET pageid=" + sRowid + " ";

                            sSQL += "WHERE scanpjcode='" + pScanProj.Trim().Replace("'", "") + "' ";
                            sSQL += "AND sysappnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                            sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";
                            sSQL += "AND pageid='" + sRowid.Trim().Replace("'", "") + "' ";

                            mGlobal.objDB.UpdateRows(sSQL);

                            i += 1;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                return false;
                //throw new Exception(ex.Message);   
            }

            return true;
        }

        public bool updatePageKeyFieldDB(string pScanProj, string pAppNum, string pBatchCode, string pSetNum, string pDocType,
            string pOldPageId, string pPageId)
        {
            bool bSaved = true;
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "UPDATE " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TKeyFieldValue ";
                sSQL += "SET pageid='" + pPageId.Trim().Replace("'", "") + "' ";
                sSQL += "WHERE scanpjcode='" + pScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND sysappnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";

                if (pSetNum.Trim() != string.Empty)
                    sSQL += "AND setnum='" + pSetNum.Trim().Replace("'", "") + "' ";

                sSQL += "AND doctype='" + pDocType.Trim().Replace("'", "") + "' ";
                sSQL += "AND pageid=" + pOldPageId.Trim().Replace("'", "") + " ";

                bSaved = mGlobal.objDB.UpdateRows(sSQL, true);
            }
            catch (Exception ex)
            {
                bSaved = false;
                //throw new Exception(ex.Message);                            
            }

            return bSaved;
        }

        public DataRowCollection getIndexedKeyFieldValueByPageDB(string pScanProj, string pAppNum, string pBatchCode, string pSetNum, string pDocType,
            string pPageId)
        {
            try
            {
                string sSQL = "";
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT fieldid,keyname,fldvalue,keydatatype FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TKeyFieldValue ";
                sSQL += "WHERE scanpjcode='" + pScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND sysappnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";
                sSQL += "AND setnum='" + pSetNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND doctype='" + pDocType.Trim().Replace("'", "") + "' ";
                sSQL += "AND pageid=" + pPageId.Trim().Replace("'", "") + " ";

                return mGlobal.objDB.ReturnRows(sSQL);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public bool deleteAllIDPModelDB(string pScanProj, string pAppNum, string pBatchCode, string pSetNum = "", string pDocType = "")
        {
            bool bDeleted = true;
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "DELETE FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchModResult ";
                sSQL += "WHERE scanproj='" + pScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";

                if (pSetNum.Trim() != "")
                    sSQL += "AND setnum='" + pSetNum.Trim().Replace("'", "") + "' ";
                if (pDocType.Trim() != "")
                    sSQL += "AND doctype='" + pDocType.Trim().Replace("'", "") + "' ";

                if (mGlobal.objDB.UpdateRows(sSQL) == false)
                {
                    bDeleted = false;
                }
            }
            catch (Exception ex)
            {
                bDeleted = false;
                //throw new Exception(ex.Message);                            
            }

            return bDeleted;
        }

        public bool deleteAllIDPTableColsDB(string pScanProj, string pAppNum, string pBatchCode, string pSetNum = "", string pDocType = "",
            string pPageId = "")
        {
            bool bDeleted = true;
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "DELETE FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TIDPTableCols ";
                sSQL += "WHERE scanproj='" + pScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";

                if (pSetNum.Trim() != "")
                    sSQL += "AND setnum='" + pSetNum.Trim().Replace("'", "") + "' ";
                if (pDocType.Trim() != "")
                    sSQL += "AND doctype='" + pDocType.Trim().Replace("'", "") + "' ";
                if (pPageId.Trim() != "")
                    sSQL += "AND pageid='" + pPageId.Trim().Replace("'", "") + "' ";

                if (mGlobal.objDB.UpdateRows(sSQL) == false)
                {
                    bDeleted = false;
                }
            }
            catch (Exception ex)
            {
                bDeleted = false;
                //throw new Exception(ex.Message);                            
            }

            return bDeleted;
        }

        public bool deleteAllIDPKeyFieldValuesDB(string pScanProj, string pAppNum, string pBatchCode, string pSetNum = "", string pDocType = "",
            string pPageId = "")
        {
            bool bDeleted = true;
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "DELETE FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TKeyFieldValue ";
                sSQL += "WHERE scanpjcode='" + pScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND sysappnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";

                if (pSetNum.Trim() != "")
                    sSQL += "AND setnum='" + pSetNum.Trim().Replace("'", "") + "' ";
                if (pDocType.Trim() != "")
                    sSQL += "AND doctype='" + pDocType.Trim().Replace("'", "") + "' ";
                if (pPageId.Trim() != "")
                    sSQL += "AND pageid='" + pPageId.Trim().Replace("'", "") + "' ";

                if (mGlobal.objDB.UpdateRows(sSQL) == false)
                {
                    bDeleted = false;
                }
            }
            catch (Exception ex)
            {
                bDeleted = false;
                //throw new Exception(ex.Message);                            
            }

            return bDeleted;
        }

        public bool deleteAllIDPKeyValuesDB(string pScanProj, string pAppNum, string pBatchCode, string pSetNum = "", string pDocType = "", 
            string pPageId = "")
        {
            bool bDeleted = true;
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "DELETE FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TIDPKeysValues ";
                sSQL += "WHERE scanproj='" + pScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";

                if (pSetNum.Trim() != "")
                    sSQL += "AND setnum='" + pSetNum.Trim().Replace("'", "") + "' ";
                if (pDocType.Trim() != "")
                    sSQL += "AND doctype='" + pDocType.Trim().Replace("'", "") + "' ";
                if (pPageId.Trim() != "")
                    sSQL += "AND pageid='" + pPageId.Trim().Replace("'", "") + "' ";

                if (mGlobal.objDB.UpdateRows(sSQL) == false)
                {
                    bDeleted = false;
                }
            }
            catch (Exception ex)
            {
                bDeleted = false;
                //throw new Exception(ex.Message);                            
            }

            return bDeleted;
        }

        public bool deleteAllIDPTableValuesDB(string pScanProj, string pAppNum, string pBatchCode, string pSetNum = "", string pDocType = "", 
            string pPageId = "")
        {
            bool bDeleted = true;
            string sSQL = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "DELETE FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TIDPTableValues ";
                sSQL += "WHERE scanproj='" + pScanProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";

                if (pSetNum.Trim() != "")
                    sSQL += "AND setnum='" + pSetNum.Trim().Replace("'", "") + "' ";
                if (pDocType.Trim() != "")
                    sSQL += "AND doctype='" + pDocType.Trim().Replace("'", "") + "' ";
                if (pPageId.Trim() != "")
                    sSQL += "AND pageid='" + pPageId.Trim().Replace("'", "") + "' ";

                if (mGlobal.objDB.UpdateRows(sSQL) == false)
                {
                    bDeleted = false;
                }
            }
            catch (Exception ex)
            {
                bDeleted = false;
                //throw new Exception(ex.Message);                            
            }

            return bDeleted;
        }


    }    
}
