using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Converters;

namespace SRDocScanIDP
{
    public partial class ScanSumm
    {
        [JsonProperty("sctype")]
        public string Scantype { get; set; }

        [JsonProperty("scproj")]
        public string Scanproj { get; set; }

        [JsonProperty("sysappnum")]
        public string Appnum { get; set; }

        [JsonProperty("setno")]
        public string Setnum { get; set; }

        [JsonProperty("batchcd")]
        public string Batchcode { get; set; }

        [JsonProperty("docutype")]
        public string Doctype { get; set; }

        [JsonProperty("scbranch")]
        public string Scanbranch { get; set; }

        [JsonProperty("scstation")]
        public string Scanstation { get; set; }

        [JsonProperty("scuser")]
        public string Scanuser { get; set; }

        [JsonProperty("scstart")]
        public DateTimeOffset Scanstart { get; set; }

        [JsonProperty("scend")]
        public DateTimeOffset Scanend { get; set; }

        [JsonProperty("scstatus")]
        public string Scanstatus { get; set; }
    }

    public static class SerializeSumm
    {
        public static string ToJson(this ScanSumm[] self) => JsonConvert.SerializeObject(self, SummConverter.Settings);
    }

    internal static class SummConverter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fff" }
            },
        };
    }

    public static class staMain
    {
        public static string _ScanProj;
        public static string _AppNum;
        public static _stcImgInfo stcImgInfoBySet;
        public static _stcExportInfo stcExportInfo;
        public static _stcSettingInfo stcProjCfg;
        public static _stcIndexSetting stcIndexSetting;
        public static string sSetNumFmt;

        public struct _stcSettingInfo
        {
            public string PrjCode;
            public string PrjName;
            public string AppNum;
            public string AppName;
            public string BatchType;
            public string VerifyEnable;
            public string RescanEnable;
            public string WorkDir;
            public string IndexDir;
            public string DocDefId;
            public string DocDefName;
            public string BatchAuto;
            public string BatchFmt;
            public string ExportEnable;
            public string ExpDir;
            public string ExpFileFmt;
            public string ExpSearchPDF;
            public string ExpMerge;
            public string ExpRemvBarSep;
            public string ExpRunDigitsFmt;
            public string Remarks;
            public string NoSeparator;
            public string SeparatorText;
            public string Active;
            public string ExpSubDirFmt;
            public string IndexEnable;
        }

        public struct _stcIndexSetting
        {
            public string PrjCode;
            public string AppNum;
            public string DocDefId;
            public List<int> RowId;
            public List<string> FieldName;
            public List<string> FieldDataType;
            public List<int> FieldSize;
            public List<int> FieldDecimal;
            public List<string> FieldMand;
            public List<string> FieldEdit;
            public List<string> FieldDefault;
            public List<int> FieldDispSeq;
            public List<string> FieldAutoFill;
			public List<string> FieldZoneOCR;
            public List<string> FieldAutoOCR;
            public List<string> ValueListType;
        }

        public struct _stcImgInfo
        {            
            public string ScanPrj;
            public string AppNum;
            public string[] SetNum;
            public string[] CollaNum;
            public string BatchCode;
            public short CurrImgIdx;
            public int[] ImgIdx;
            public string[] DocType;
            public string[] ImgFile;
            public int[] ImgSeq;
            public int[] Rowid;
            public string[] Page;
        }

        public struct _stcExportInfo
        {
            public List<string> BatchCodes;
            public List<string> Setnum;
            public List<string> DocTypes;
            public List<string> ExpFiles;
            public List<int> TotPages;
            //public string[] DocNames;
        }

        //public struct _stcIndexFieldValues
        //{
        //    public int[] FieldId;
        //    public string[] FieldName;
        //    public string[] FieldValue;
        //}

        public static void loadProjSettings(string pPrjCode, string pAppNum)
        {
            string sSQL = "";
            try
            {                
                DataRow dr = null;

                if (pPrjCode.Trim() != string.Empty && pAppNum.Trim() != string.Empty)
                {
                    mGlobal.LoadAppDBCfg();

                    sSQL = "SELECT * FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TScanProjSet ";
                    sSQL += "WHERE scanpjcode='" + pPrjCode + "' ";
                    sSQL += "AND sysappnum='" + pAppNum + "' ";

                    dr = mGlobal.objDB.ReturnSingleRow(sSQL);
                }                

                if (dr != null)
                {
                    stcProjCfg.PrjCode = dr[1].ToString().Trim();
                    stcProjCfg.PrjName = dr[2].ToString().Trim();
                    stcProjCfg.AppNum = dr[3].ToString().Trim();
                    stcProjCfg.AppName = dr[4].ToString().Trim();
                    stcProjCfg.BatchType = dr[5].ToString().Trim();
                    stcProjCfg.VerifyEnable = dr[6].ToString().Trim();
                    stcProjCfg.RescanEnable = dr[7].ToString().Trim();
                    stcProjCfg.WorkDir = dr[8].ToString().Trim();
                    stcProjCfg.IndexDir = dr[9].ToString().Trim();
                    stcProjCfg.DocDefId = dr[10].ToString().Trim();
                    stcProjCfg.DocDefName = dr[11].ToString().Trim();
                    stcProjCfg.BatchAuto = dr[12].ToString().Trim();
                    stcProjCfg.BatchFmt = dr[13].ToString().Trim();
                    stcProjCfg.ExportEnable = dr[14].ToString().Trim();
                    stcProjCfg.ExpDir = dr[15].ToString().Trim();
                    stcProjCfg.ExpFileFmt = dr[16].ToString().Trim();
                    stcProjCfg.ExpSearchPDF = dr[17].ToString().Trim();
                    stcProjCfg.ExpMerge = dr[18].ToString().Trim();
                    stcProjCfg.ExpRemvBarSep = dr[19].ToString().Trim();
                    stcProjCfg.ExpRunDigitsFmt = dr[20].ToString().Trim();
                    stcProjCfg.Remarks = dr[21].ToString().Trim();
                    stcProjCfg.NoSeparator = dr[22].ToString().Trim();
                    stcProjCfg.SeparatorText = dr[23].ToString().Trim().ToUpper();
                    stcProjCfg.Active = dr[24].ToString().Trim();
                    stcProjCfg.ExpSubDirFmt = dr[27].ToString().Trim();
                    stcProjCfg.IndexEnable = dr[28].ToString().Trim();
                }
                else
                {
                    stcProjCfg.PrjCode = String.Empty;
                    stcProjCfg.PrjName = String.Empty;
                    stcProjCfg.AppNum = String.Empty;
                    stcProjCfg.AppName = String.Empty;
                    stcProjCfg.BatchType = String.Empty;
                    stcProjCfg.VerifyEnable = String.Empty;
                    stcProjCfg.RescanEnable = String.Empty;
                    stcProjCfg.WorkDir = String.Empty;
                    stcProjCfg.IndexDir = String.Empty;
                    stcProjCfg.DocDefId = String.Empty;
                    stcProjCfg.DocDefName = String.Empty;
                    stcProjCfg.BatchAuto = String.Empty;
                    stcProjCfg.BatchFmt = String.Empty;
                    stcProjCfg.ExportEnable = String.Empty;
                    stcProjCfg.ExpDir = String.Empty;
                    stcProjCfg.ExpFileFmt = String.Empty;
                    stcProjCfg.ExpSearchPDF = String.Empty;
                    stcProjCfg.ExpMerge = String.Empty;
                    stcProjCfg.ExpRemvBarSep = String.Empty;
                    stcProjCfg.ExpRunDigitsFmt = "0";
                    stcProjCfg.Remarks = String.Empty;
                    stcProjCfg.NoSeparator = String.Empty;
                    stcProjCfg.SeparatorText = String.Empty;
                    stcProjCfg.Active = String.Empty;
                    stcProjCfg.ExpSubDirFmt = String.Empty;
                    stcProjCfg.IndexEnable = String.Empty;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static void initIndexConfig(int iTot)
        {
            if (iTot == 0) iTot = 1;

            int i = 0;
            while (i < iTot)
            {
                stcIndexSetting.RowId = new List<int>(iTot);
                stcIndexSetting.FieldName = new List<string>(iTot);
                stcIndexSetting.FieldDataType = new List<string>(iTot);
                stcIndexSetting.FieldSize = new List<int>(iTot);
                stcIndexSetting.FieldDecimal = new List<int>(iTot);
                stcIndexSetting.FieldMand = new List<string>(iTot);
                stcIndexSetting.FieldEdit = new List<string>(iTot);
                stcIndexSetting.FieldDefault = new List<string>(iTot);
                stcIndexSetting.FieldDispSeq = new List<int>(iTot);
                stcIndexSetting.FieldAutoFill = new List<string>(iTot);
				stcIndexSetting.FieldZoneOCR = new List<string>(iTot);
                stcIndexSetting.FieldAutoOCR = new List<string>(iTot);
                stcIndexSetting.ValueListType = new List<string>(iTot);

                i += 1;
            }
        }

        public static bool checkIsSetPageIndexed(string pPrjCode, string pAppNum, string pBatchCode, string pSetNum, string pDocType, 
            int pPageId, bool pIncludeBlankValue)
        {
            bool ret = false;
            try
            {
                string sSQL;
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT IsNull(COUNT(setnum),0) FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TKeyFieldValue ";
                sSQL += "WHERE scanpjcode='" + pPrjCode + "' ";
                sSQL += "AND sysappnum='" + pAppNum + "' ";
                sSQL += "AND batchcode='" + pBatchCode + "' ";
                sSQL += "AND setnum='" + pSetNum + "' ";
                sSQL += "AND doctype='" + pDocType + "' ";
                sSQL += "AND pageid='" + pPageId.ToString() + "' ";

                if (pIncludeBlankValue)
                {
                    sSQL += "AND TRIM(fldvalue)='' ";
                }

                DataRow dr = mGlobal.objDB.ReturnSingleRow(sSQL);

                if (dr != null)
                {
                    if (Convert.ToInt32(dr[0].ToString()) > 0) ret = true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return ret;
        }

        public static bool checkIsSetIndexed(string pPrjCode, string pAppNum, string pBatchCode, string pSetNum, int pPageId, bool pIncludeBlankValue)
        {
            bool ret = false;
            try
            {
                string sSQL;
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT IsNull(COUNT(setnum),0) FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TIndexFieldValue ";
                sSQL += "WHERE scanpjcode='" + pPrjCode + "' ";
                sSQL += "AND sysappnum='" + pAppNum + "' ";
                sSQL += "AND batchcode='" + pBatchCode + "' ";
                sSQL += "AND setnum='" + pSetNum + "' ";
                sSQL += "AND pageid='" + pPageId.ToString() + "' ";

                if (pIncludeBlankValue)
                {
                    sSQL += "AND TRIM(fldvalue)='' ";
                }

                DataRow dr = mGlobal.objDB.ReturnSingleRow(sSQL);

                if (dr != null)
                {
                    if (Convert.ToInt32(dr[0].ToString()) > 0) ret = true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return ret;
        }

        public static bool checkIsSetIndexed(string pPrjCode, string pAppNum, string pBatchCode, string pSetNum, bool pIncludeBlankValue)
        {
            bool ret = false;
            try
            {
                string sSQL;
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT IsNull(COUNT(setnum),0) FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TIndexFieldValue ";
                sSQL += "WHERE scanpjcode='" + pPrjCode + "' ";
                sSQL += "AND sysappnum='" + pAppNum + "' ";
                sSQL += "AND batchcode='" + pBatchCode + "' ";
                sSQL += "AND setnum='" + pSetNum + "' ";

                if (pIncludeBlankValue)
                {
                    sSQL += "AND TRIM(fldvalue)='' ";
                }

                DataRow dr = mGlobal.objDB.ReturnSingleRow(sSQL);

                if (dr != null)
                {
                    if (Convert.ToInt32(dr[0].ToString()) > 0) ret = true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return ret;
        }

        public static bool checkIsSetKeyIndexed(string pPrjCode, string pAppNum, string pBatchCode, string pSetNum, string pDocType, 
            bool pIncludeBlankValue)
        {
            bool ret = false;
            try
            {
                string sSQL;
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT IsNull(COUNT(setnum),0) FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TKeyFieldValue ";
                sSQL += "WHERE scanpjcode='" + pPrjCode + "' ";
                sSQL += "AND sysappnum='" + pAppNum + "' ";
                sSQL += "AND batchcode='" + pBatchCode + "' ";
                sSQL += "AND setnum='" + pSetNum + "' ";
                sSQL += "AND doctype='" + pDocType + "' ";

                if (pIncludeBlankValue)
                {
                    sSQL += "AND TRIM(fldvalue)='' ";
                }

                DataRow dr = mGlobal.objDB.ReturnSingleRow(sSQL);

                if (dr != null)
                {
                    if (Convert.ToInt32(dr[0].ToString()) > 0) ret = true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return ret;
        }

        public static bool checkIsSetKeyIndexed(string pPrjCode, string pAppNum, string pBatchCode, string pSetNum, string pDocType,
            int pPageId, bool pIncludeBlankValue)
        {
            bool ret = false;
            try
            {
                string sSQL;
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT IsNull(COUNT(setnum),0) FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TKeyFieldValue ";
                sSQL += "WHERE scanpjcode='" + pPrjCode + "' ";
                sSQL += "AND sysappnum='" + pAppNum + "' ";
                sSQL += "AND batchcode='" + pBatchCode + "' ";
                sSQL += "AND setnum='" + pSetNum + "' ";
                sSQL += "AND doctype='" + pDocType + "' ";
                sSQL += "AND pageid=" + pPageId + " ";

                if (pIncludeBlankValue)
                {
                    sSQL += "AND TRIM(fldvalue)='' ";
                }

                DataRow dr = mGlobal.objDB.ReturnSingleRow(sSQL);

                if (dr != null)
                {
                    if (Convert.ToInt32(dr[0].ToString()) > 0) ret = true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return ret;
        }

        public static bool checkIsFullSetIndexed(string pPrjCode, string pAppNum, string pBatchCode, int pTotSet, ref int pTotSetRec)
        {
            bool ret = false;
            try
            {
                string sSQL;
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT COUNT(*) FROM ( ";
                sSQL += "SELECT setnum FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TIndexFieldValue ";
                sSQL += "WHERE scanpjcode='" + pPrjCode + "' ";
                sSQL += "AND sysappnum='" + pAppNum + "' ";
                sSQL += "AND batchcode='" + pBatchCode + "' ";
                sSQL += "GROUP BY setnum ) a";

                DataRow dr = mGlobal.objDB.ReturnSingleRow(sSQL);

                if (dr != null)
                {
                    pTotSetRec = Convert.ToInt32(dr[0].ToString());
                    if (pTotSetRec == pTotSet) ret = true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return ret;
        }

        public static bool checkIsFullSetKeyIndexed(string pPrjCode, string pAppNum, string pBatchCode, string pSetNum, 
            int pTotSet, ref int pTotSetRec)
        {
            bool ret = false;
            try
            {
                string sSQL;
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT COUNT(*) FROM ( ";
                sSQL += "SELECT doctype FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TKeyFieldValue ";
                sSQL += "WHERE scanpjcode='" + pPrjCode + "' ";
                sSQL += "AND sysappnum='" + pAppNum + "' ";
                sSQL += "AND batchcode='" + pBatchCode + "' ";
                sSQL += "AND setnum='" + pSetNum + "' ";
                sSQL += "GROUP BY doctype ) a";

                DataRow dr = mGlobal.objDB.ReturnSingleRow(sSQL);

                if (dr != null)
                {
                    pTotSetRec = Convert.ToInt32(dr[0].ToString());
                    if (pTotSetRec == pTotSet) ret = true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return ret;
        }

        public static void loadIndexConfigDB(string pPrjCode, string pAppNum, string pDocDefId)
        {
            try
            {
                string sSQL;
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT * FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuDefiSet ";
                sSQL += "WHERE scanpjcode='" + pPrjCode + "' ";
                sSQL += "AND sysappnum='" + pAppNum + "' ";
                sSQL += "AND docdefid='" + pDocDefId + "' ";
                sSQL += "ORDER BY flddispseq ";

                DataRowCollection drs = mGlobal.objDB.ReturnRows(sSQL);

                if (drs != null)
                {
                    stcIndexSetting.PrjCode = pPrjCode;
                    stcIndexSetting.AppNum = pAppNum;
                    stcIndexSetting.DocDefId = pDocDefId;

                    initIndexConfig(drs.Count);

                    int i = 0;
                    while (i < drs.Count)
                    {
                        stcIndexSetting.RowId.Add(Convert.ToInt32(drs[i]["rowid"].ToString()));
                        stcIndexSetting.FieldName.Add(drs[i]["fldname"].ToString());
                        stcIndexSetting.FieldDataType.Add(drs[i]["flddatatype"].ToString());
                        stcIndexSetting.FieldSize.Add(Convert.ToInt32(drs[i]["fldsize"].ToString()));
                        stcIndexSetting.FieldDecimal.Add(Convert.ToInt32(drs[i]["decimaldigit"].ToString()));
                        stcIndexSetting.FieldMand.Add(drs[i]["fldmand"].ToString());
                        stcIndexSetting.FieldEdit.Add(drs[i]["fldeditable"].ToString());
                        stcIndexSetting.FieldDefault.Add(drs[i]["flddefaultval"].ToString());
                        stcIndexSetting.FieldDispSeq.Add(Convert.ToInt32(drs[i]["flddispseq"].ToString()));
                        stcIndexSetting.FieldAutoFill.Add(drs[i]["fldautofill"].ToString());
						stcIndexSetting.FieldZoneOCR.Add(drs[i]["fldzoneocr"].ToString());
                        stcIndexSetting.FieldAutoOCR.Add(drs[i]["fldautoocr"].ToString());
                        stcIndexSetting.ValueListType.Add(drs[i]["valuelisttype"].ToString());

                        i += 1;
                    }

                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }            
        }

       public static void loadBatchTreeviewNoSep(TreeView tvwSet, string pBatchType, string pBatchCode, string pBatchStatusIn,
            string pDocStatusIn, string pStation = "", string pUserId = "",
            string pDateStart = "", string pDateEnd = "")
        {
            string sSQL = "";
            DataSet dsTVw = null;
            string sTag = "";
            string sNodeText = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                //Load batch.
                sSQL = "SELECT Distinct batchcode ";
                sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan ";
                sSQL += "WHERE scanproj='" + _ScanProj.Trim() + "' ";
                sSQL += "AND appnum='" + _AppNum.Trim() + "' ";
                sSQL += "AND batchtype='" + pBatchType.Trim() + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim() + "' ";
                sSQL += "AND batchstatus IN (" + pBatchStatusIn + ") ";

                if (pStation.Trim() != "")
                    sSQL += "AND scanstation='" + pStation.Trim().Replace("'", "") + "' ";

                if (pUserId.Trim() != "")
                    sSQL += "AND scanuser='" + pUserId.Trim().Replace("'", "") + "' ";

                if (pDateStart != "")
                    sSQL += "AND Convert(datetime,scanstart,103)>='" + pDateStart + "' "; //"yyyy-MM-dd HH:mm:ss"

                if (pDateEnd != "")
                    sSQL += "AND Convert(datetime,scanend,103)<='" + pDateEnd + "' "; //"yyyy-MM-dd HH:mm:ss"

                dsTVw = new DataSet();
                dsTVw = mGlobal.returnRecordset("tTVw", sSQL);

                //Load batches.
                if (dsTVw.Tables != null)
                {
                    if (dsTVw.Tables.Count > 0)
                    {
                        if (dsTVw.Tables[0].Rows.Count > 0)
                        {
                            tvwSet.Nodes.Clear();

                            int i = 0;
                            TreeNode node; DataRow dr = null;
                            while (i < dsTVw.Tables[0].Rows.Count)
                            {
                                dr = dsTVw.Tables[0].Rows[i];

                                sTag = "Sep|" + dr["batchcode"].ToString();
                                node = new TreeNode("Batch " + dr["batchcode"].ToString().Replace("_", "-"));
                                node.Tag = new String(sTag.ToCharArray());
                                tvwSet.Nodes.Add(node);
                                tvwSet.Nodes[0].Name = tvwSet.Nodes[0].FullPath;

                                i += 1;
                            }
                        }
                    }
                }
                dsTVw.Dispose();
                dsTVw = null;
                
                //Load Document.
                int nc = 0;                
                sTag = "";

                while (nc < tvwSet.Nodes.Count)
                {                    
                    sNodeText = tvwSet.Nodes[nc].Tag.ToString().Substring(4);

                    sSQL = "SELECT DISTINCT rowid,batchcode,setnum,collanum,doctype,scanstart,imageseq ";
                    sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuScan ";
                    sSQL += "WHERE scanproj='" + _ScanProj.Trim() + "' ";
                    sSQL += "AND appnum='" + _AppNum.Trim() + "' ";
                    sSQL += "AND batchcode='" + sNodeText + "' ";
                    sSQL += "AND doctype<>'" + staMain.stcProjCfg.SeparatorText + "' ";
                    sSQL += "AND scanstatus IN (" + pDocStatusIn + ") ";

                    if (pStation.Trim() != "")
                        sSQL += "AND scanstation='" + pStation.Trim().Replace("'", "") + "' ";

                    if (pUserId.Trim() != "")
                        sSQL += "AND scanuser='" + pUserId.Trim().Replace("'", "") + "' ";

                    //sSQL += "ORDER BY setnum,doctype,imageseq,scanstart ";
                    sSQL += "ORDER BY setnum,imageseq,scanstart ";

                    dsTVw = new DataSet();
                    dsTVw = mGlobal.returnRecordset("tTVw", sSQL);

                    if (dsTVw.Tables != null)
                    {
                        if (dsTVw.Tables.Count > 0)
                        {
                            if (dsTVw.Tables[0].Rows.Count > 0)
                            {
                                int i = 0;
                                TreeNode node; DataRow dr = null;
                                while (i < dsTVw.Tables[0].Rows.Count)
                                {
                                    dr = dsTVw.Tables[0].Rows[i];

                                    sTag = "Doc|" + dr["batchcode"].ToString() + "|" + dr["doctype"].ToString() + "|" +
                                        dr["rowid"].ToString() + "|" + dr["setnum"].ToString() + "|" +
                                        dr["collanum"].ToString() + "|" + dr["imageseq"].ToString();
                                    node = new TreeNode("Page " + (i + 1));
                                    node.Tag = new String(sTag.ToCharArray());

                                    tvwSet.Nodes[nc].Nodes.Add(node);
                                    tvwSet.Nodes[nc].LastNode.Name = tvwSet.Nodes[nc].LastNode.FullPath;

                                    i += 1;
                                }
                            }
                        }
                    }
                    dsTVw.Dispose();
                    dsTVw = null;

                    nc += 1;
                }

            }
            catch (Exception ex)
            {
            }
        }

        public static void loadBatchTreeview(TreeView tvwSet, string pBatchType, string pBatchCode, string pBatchStatusIn,
            string pDocStatusIn, string pStation = "", string pUserId = "", 
            string pDateStart = "", string pDateEnd = "")
        {
            string sSQL = "";
            DataSet dsTVw = null;
            string sTag = "";
            string sNodeText = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                //Load batch.
                sSQL = "SELECT Distinct batchcode ";
                sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan ";
                sSQL += "WHERE scanproj='" + _ScanProj.Trim() + "' ";
                sSQL += "AND appnum='" + _AppNum.Trim() + "' ";
                sSQL += "AND batchtype='" + pBatchType.Trim() + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim() + "' ";
                sSQL += "AND batchstatus IN (" + pBatchStatusIn + ") ";
                
                if (pStation.Trim() != "")
                    sSQL += "AND scanstation='" + pStation.Trim().Replace("'", "") + "' ";

                if (pUserId.Trim() != "")
                    sSQL += "AND scanuser='" + pUserId.Trim().Replace("'", "") + "' ";

                if (pDateStart != "")
                    sSQL += "AND Convert(datetime,scanstart,103)>='" + pDateStart + "' "; //"yyyy-MM-dd HH:mm:ss"

                if (pDateEnd != "")
                    sSQL += "AND Convert(datetime,scanend,103)<='" + pDateEnd + "' "; //"yyyy-MM-dd HH:mm:ss"

                dsTVw = new DataSet();
                dsTVw = mGlobal.returnRecordset("tTVw", sSQL);

                //Load batches.
                if (dsTVw.Tables != null)
                {
                    if (dsTVw.Tables.Count > 0)
                    {
                        if (dsTVw.Tables[0].Rows.Count > 0)
                        {
                            tvwSet.Nodes.Clear();

                            int i = 0;
                            TreeNode node; DataRow dr = null;
                            while (i < dsTVw.Tables[0].Rows.Count)
                            {
                                dr = dsTVw.Tables[0].Rows[i];

                                sTag = "Sep|" + dr["batchcode"].ToString();
                                node = new TreeNode("Batch " + dr["batchcode"].ToString().Replace("_", "-"));
                                node.Tag = new String(sTag.ToCharArray());
                                tvwSet.Nodes.Add(node);
								tvwSet.Nodes[0].Name = tvwSet.Nodes[0].FullPath;

                                i += 1;
                            }
                        }
                    }
                }
                dsTVw.Dispose();
                dsTVw = null;

                //Load Set
                int nc = 0;
                sTag = "";
                if (stcProjCfg.NoSeparator.Trim() != "C")
                {
                    while (nc < tvwSet.Nodes.Count)
                    {
                        sNodeText = tvwSet.Nodes[nc].Tag.ToString().Substring(4);

                        sSQL = "SELECT Distinct setnum ";
                        sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan ";
                        sSQL += "WHERE scanproj='" + _ScanProj.Trim() + "' ";
                        sSQL += "AND appnum='" + _AppNum.Trim() + "' ";
                        sSQL += "AND batchtype='" + pBatchType.Trim() + "' ";
                        sSQL += "AND batchcode='" + sNodeText.Trim() + "' ";
                        sSQL += "AND batchstatus IN (" + pBatchStatusIn + ") ";
                        //sSQL += "AND totpagecnt > 0 "; //page count more than 0 for batch type only.

                        if (pStation.Trim() != "")
                            sSQL += "AND scanstation='" + pStation.Trim().Replace("'", "") + "' ";

                        if (pUserId.Trim() != "")
                            sSQL += "AND scanuser='" + pUserId.Trim().Replace("'", "") + "' ";

                        if (pDateStart != "")
                            sSQL += "AND Convert(datetime,scanstart,103)>='" + pDateStart + "' "; //"yyyy-MM-dd HH:mm:ss"

                        if (pDateEnd != "")
                            sSQL += "AND Convert(datetime,scanend,103)<='" + pDateEnd + "' "; //"yyyy-MM-dd HH:mm:ss"

                        dsTVw = new DataSet();
                        dsTVw = mGlobal.returnRecordset("tTVw", sSQL);

                        if (dsTVw.Tables != null)
                        {
                            if (dsTVw.Tables.Count > 0)
                            {
                                if (dsTVw.Tables[0].Rows.Count > 0)
                                {
                                    int i = 0;
                                    TreeNode node; DataRow dr = null;
                                    while (i < dsTVw.Tables[0].Rows.Count)
                                    {
                                        dr = dsTVw.Tables[0].Rows[i];

                                        sTag = "Sep1|" + dr["setnum"].ToString();
                                        node = new TreeNode("Document Set " + dr["setnum"].ToString());
                                        node.Tag = new String(sTag.ToCharArray());
                                        tvwSet.Nodes[nc].Nodes.Add(node);
                                        tvwSet.Nodes[nc].LastNode.Name = tvwSet.Nodes[nc].LastNode.FullPath;

                                        i += 1;
                                    }
                                }
                            }
                        }
                        dsTVw.Dispose();
                        dsTVw = null;

                        nc += 1;
                    }
                }

                int iNodesCnt = 0, iLev = 1;
                //Load Document Type
                int nnc = 0; string sNodeText1 = "";
                if (stcProjCfg.NoSeparator.Trim() == "2" || stcProjCfg.NoSeparator.Trim() == "C")
                {
                    nc = 0;
                    sTag = "";

                    while (nc < tvwSet.Nodes.Count)
                    {
                        nnc = 0;
                        sNodeText = tvwSet.Nodes[nc].Tag.ToString().Substring(4);

                        if (stcProjCfg.NoSeparator.Trim() == "C")
                            iNodesCnt = tvwSet.Nodes.Count;
                        else
                            iNodesCnt = tvwSet.Nodes[nc].Nodes.Count;

                        while (nnc < iNodesCnt)
                        {
                            if (stcProjCfg.NoSeparator.Trim() == "2")
                                sNodeText1 = tvwSet.Nodes[nc].Nodes[nnc].Tag.ToString().Substring(5);

                            sSQL = "SELECT DISTINCT setnum,collanum,doctype ";
                            sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuScan ";
                            sSQL += "WHERE scanproj='" + _ScanProj.Trim() + "' ";
                            sSQL += "AND appnum='" + _AppNum.Trim() + "' ";
                            sSQL += "AND batchcode='" + sNodeText + "' ";
                            if (stcProjCfg.NoSeparator.Trim() == "2")
                                sSQL += "AND setnum=" + sNodeText1 + " ";
                            sSQL += "AND scanstatus IN (" + pDocStatusIn + ") "; //'S','A'
                            sSQL += "AND doctype<>'" + staMain.stcProjCfg.SeparatorText + "' ";

                            if (pStation.Trim() != "")
                                sSQL += "AND scanstation='" + pStation.Trim().Replace("'", "") + "' ";

                            if (pUserId.Trim() != "")
                                sSQL += "AND scanuser='" + pUserId.Trim().Replace("'", "") + "' ";

                            if (pDateStart != "")
                                sSQL += "AND Convert(datetime,scanstart,103)>='" + pDateStart + "' "; //"yyyy-MM-dd HH:mm:ss"

                            if (pDateEnd != "")
                                sSQL += "AND Convert(datetime,scanend,103)<='" + pDateEnd + "' "; //"yyyy-MM-dd HH:mm:ss"

                            //sSQL += "ORDER BY setnum,doctype ";
                            sSQL += "ORDER BY setnum ";

                            dsTVw = new DataSet();
                            dsTVw = mGlobal.returnRecordset("tTVw", sSQL);

                            if (dsTVw.Tables != null)
                            {
                                if (dsTVw.Tables.Count > 0)
                                {
                                    if (dsTVw.Tables[0].Rows.Count > 0)
                                    {
                                        int i = 0;
                                        TreeNode node; DataRow dr = null;
                                        while (i < dsTVw.Tables[0].Rows.Count)
                                        {
                                            dr = dsTVw.Tables[0].Rows[i];

                                            if (stcProjCfg.NoSeparator.Trim() == "C") //Classifier as separator.
                                                sTag = "Sep1|" + dr["doctype"].ToString() + "|" + dr["setnum"].ToString() + "|" + dr["collanum"].ToString();
                                            else
                                                sTag = "Sep2|" + dr["doctype"].ToString() + "|" + sNodeText1 + "|" + dr["collanum"].ToString();
                                            node = new TreeNode(dr["doctype"].ToString());
                                            node.Tag = new String(sTag.ToCharArray());
                                            if (stcProjCfg.NoSeparator.Trim() == "C")
                                            {
                                                tvwSet.Nodes[nc].Nodes.Add(node);
                                                tvwSet.Nodes[nc].LastNode.Name = tvwSet.Nodes[nc].LastNode.FullPath;
                                            }
                                            else
                                            {
                                                tvwSet.Nodes[nc].Nodes[nnc].Nodes.Add(node);
                                                tvwSet.Nodes[nc].Nodes[nnc].LastNode.Name = tvwSet.Nodes[nc].Nodes[nnc].LastNode.FullPath;
                                            }
											
                                            i += 1;
                                        }
                                    }
                                }
                            }
                            dsTVw.Dispose();
                            dsTVw = null;

                            nnc += 1;
                        }

                        nc += 1;
                    }
                }

                //Load Document.
                nc = 0;
                nnc = 0; sNodeText1 = ""; int nnnc = 0; string sNodeText2 = "";
                sTag = "";
                iNodesCnt = 0; iLev = 1; 

                while (nc < tvwSet.Nodes.Count)
                {
                    nnc = 0;
                    sNodeText = tvwSet.Nodes[nc].Tag.ToString().Substring(4);                    

                    while (nnc < tvwSet.Nodes[nc].Nodes.Count)
                    {
                        nnnc = 0;
                        if (stcProjCfg.NoSeparator.Trim() == "C")
                        {
                            sNodeText1 = tvwSet.Nodes[nc].Nodes[nnc].Tag.ToString().Split('|').GetValue(2).ToString(); //Setnum.
                            sNodeText2 = tvwSet.Nodes[nc].Nodes[nnc].Tag.ToString().Split('|').GetValue(1).ToString(); //DocType.
                        }
                        else
                            sNodeText1 = tvwSet.Nodes[nc].Nodes[nnc].Tag.ToString().Substring(5); //Setnum.

                        if (stcProjCfg.NoSeparator.Trim() == "2")
                        {
                            iNodesCnt = tvwSet.Nodes[nc].Nodes[nnc].Nodes.Count;
                            iLev = 2;
                        }
                        else
                        {
                            iNodesCnt = tvwSet.Nodes[nc].Nodes.Count;
                            iLev = 1;
                            nnnc = nnc;
                        }

                        while (nnnc < iNodesCnt)
                        {
                            //sNodeText2 = tvwSet.Nodes[nc].Nodes[nnc].Nodes[nnnc].Tag.ToString().Substring(5);
                            if (iLev == 2)
                                sNodeText2 = tvwSet.Nodes[nc].Nodes[nnc].Nodes[nnnc].Tag.ToString().Split('|').GetValue(1).ToString();

                            sSQL = "SELECT DISTINCT rowid,batchcode,setnum,collanum,doctype,scanstart,imageseq ";
                            sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuScan ";
                            sSQL += "WHERE scanproj='" + _ScanProj.Trim() + "' ";
                            sSQL += "AND appnum='" + _AppNum.Trim() + "' ";
                            sSQL += "AND batchcode='" + sNodeText + "' ";
                            sSQL += "AND setnum=" + sNodeText1 + " ";                            
                            sSQL += "AND doctype<>'" + staMain.stcProjCfg.SeparatorText + "' ";
                            if (iLev == 2 || stcProjCfg.NoSeparator.Trim() == "C")
                                sSQL += "AND doctype='" + sNodeText2 + "' ";
                            sSQL += "AND scanstatus IN (" + pDocStatusIn + ") ";

                            if (pStation.Trim() != "")
                                sSQL += "AND scanstation='" + pStation.Trim().Replace("'", "") + "' ";

                            if (pUserId.Trim() != "")
                                sSQL += "AND scanuser='" + pUserId.Trim().Replace("'", "") + "' ";

                            //sSQL += "ORDER BY setnum,doctype,imageseq,scanstart ";
                            sSQL += "ORDER BY setnum,imageseq,scanstart ";

                            dsTVw = new DataSet();
                            dsTVw = mGlobal.returnRecordset("tTVw", sSQL);

                            if (dsTVw.Tables != null)
                            {
                                if (dsTVw.Tables.Count > 0)
                                {
                                    if (dsTVw.Tables[0].Rows.Count > 0)
                                    {
                                        int i = 0;
                                        TreeNode node; DataRow dr = null;
                                        while (i < dsTVw.Tables[0].Rows.Count)
                                        {
                                            dr = dsTVw.Tables[0].Rows[i];

                                            sTag = "Doc|" + dr["batchcode"].ToString() + "|" + dr["doctype"].ToString() + "|" +
                                                dr["rowid"].ToString() + "|" + dr["setnum"].ToString() + "|" +
                                                dr["collanum"].ToString() + "|" + dr["imageseq"].ToString();
                                            node = new TreeNode("Page " + (i + 1));
                                            node.Tag = new String(sTag.ToCharArray());

                                            if (iLev == 2)
											{	
                                                tvwSet.Nodes[nc].Nodes[nnc].Nodes[nnnc].Nodes.Add(node);
												tvwSet.Nodes[nc].Nodes[nnc].Nodes[nnnc].LastNode.Name = tvwSet.Nodes[nc].Nodes[nnc].Nodes[nnnc].LastNode.FullPath;
                                            } 
											else
											{	
                                                tvwSet.Nodes[nc].Nodes[nnc].Nodes.Add(node);
												tvwSet.Nodes[nc].Nodes[nnc].LastNode.Name = tvwSet.Nodes[nc].Nodes[nnc].LastNode.FullPath;
												
											}
											
                                            i += 1;
                                        }
                                    }
                                }
                            }
                            dsTVw.Dispose();
                            dsTVw = null;

                            nnnc += 1;
                            if (iLev == 1) break;
                        }                        

                        nnc += 1;
                    }

                    nc += 1;
                }

            }
            catch (Exception ex)
            {
            }
        }

        public static void loadDocuBatchSetTreeview(TreeView tvwSet, string pBatchType, string pBatchCode, string pBatchStatusIn,
            string pDocStatusIn, string pStation = "", string pUserId = "",
            string pDateStart = "", string pDateEnd = "")
        {
            string sSQL = "";
            DataSet dsTVw = null;
            string sTag = "";
            string sNodeText = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT Distinct batchcode ";
                sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan ";
                sSQL += "WHERE scanproj='" + _ScanProj.Trim() + "' ";
                sSQL += "AND appnum='" + _AppNum.Trim() + "' ";
                sSQL += "AND batchtype='" + pBatchType.Trim() + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim() + "' ";
                sSQL += "AND batchstatus IN (" + pBatchStatusIn + ") ";

                if (pStation.Trim() != "")
                    sSQL += "AND scanstation='" + pStation.Trim().Replace("'", "") + "' ";

                if (pUserId.Trim() != "")
                    sSQL += "AND scanuser='" + pUserId.Trim().Replace("'", "") + "' ";

                if (pDateStart != "")
                    sSQL += "AND Convert(datetime,scanstart,103)>='" + pDateStart + "' "; //"yyyy-MM-dd HH:mm:ss"

                if (pDateEnd != "")
                    sSQL += "AND Convert(datetime,scanend,103)<='" + pDateEnd + "' "; //"yyyy-MM-dd HH:mm:ss"

                dsTVw = new DataSet();
                dsTVw = mGlobal.returnRecordset("tTVw", sSQL);

                //Load batches.
                if (dsTVw.Tables != null)
                {
                    if (dsTVw.Tables.Count > 0)
                    {
                        if (dsTVw.Tables[0].Rows.Count > 0)
                        {
                            tvwSet.Nodes.Clear();

                            int i = 0;
                            TreeNode node; DataRow dr = null;
                            while (i < dsTVw.Tables[0].Rows.Count)
                            {
                                dr = dsTVw.Tables[0].Rows[i];

                                //sTag = "Sep|" + dr["batchcode"].ToString();
                                //node = new TreeNode("Document Set " + dr["batchcode"].ToString().Replace("_", "-"));
                                sTag = "Sep|" + dr["batchcode"].ToString();
                                node = new TreeNode("Document Set " + dr["batchcode"].ToString().Replace("_", "-"));
                                node.Tag = new String(sTag.ToCharArray());
                                tvwSet.Nodes.Add(node);

                                i += 1;
                            }
                        }
                    }
                }
                dsTVw.Dispose();
                dsTVw = null;

                //Load Document Set\Type
                int nc = 0;
                sTag = "";

                while (nc < tvwSet.Nodes.Count)
                {
                    sNodeText = tvwSet.Nodes[nc].Tag.ToString().Substring(4);

                    sSQL = "SELECT Distinct setnum,collanum,doctype ";
                    sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuScan ";
                    sSQL += "WHERE scanproj='" + _ScanProj.Trim() + "' ";
                    sSQL += "AND appnum='" + _AppNum.Trim() + "' ";
                    sSQL += "AND batchcode='" + sNodeText + "' ";
                    sSQL += "AND scanstatus IN (" + pDocStatusIn + ") "; //'S','A'
                    sSQL += "AND doctype<>'" + staMain.stcProjCfg.SeparatorText + "' ";
                    //sSQL += "AND doctype<>'SURE-REACH' ";

                    if (pStation.Trim() != "")
                        sSQL += "AND scanstation='" + pStation.Trim().Replace("'", "") + "' ";

                    if (pUserId.Trim() != "")
                        sSQL += "AND scanuser='" + pUserId.Trim().Replace("'", "") + "' ";

                    if (pDateStart != "")
                        sSQL += "AND Convert(datetime,scanstart,103)>='" + pDateStart + "' "; //"yyyy-MM-dd HH:mm:ss"

                    if (pDateEnd != "")
                        sSQL += "AND Convert(datetime,scanend,103)<='" + pDateEnd + "' "; //"yyyy-MM-dd HH:mm:ss"

                    dsTVw = new DataSet();
                    dsTVw = mGlobal.returnRecordset("tTVw", sSQL);

                    if (dsTVw.Tables != null)
                    {
                        if (dsTVw.Tables.Count > 0)
                        {
                            if (dsTVw.Tables[0].Rows.Count > 0)
                            {
                                int i = 0;
                                TreeNode node; DataRow dr = null;
                                while (i < dsTVw.Tables[0].Rows.Count)
                                {
                                    dr = dsTVw.Tables[0].Rows[i];

                                    sTag = "Sep1|" + dr["doctype"].ToString() + "|" + dr["setnum"].ToString() + "|" + dr["collanum"].ToString();
                                    node = new TreeNode(dr["doctype"].ToString());
                                    node.Tag = new String(sTag.ToCharArray());
                                    tvwSet.Nodes[nc].Nodes.Add(node);

                                    i += 1;
                                }
                            }
                        }
                    }
                    dsTVw.Dispose();
                    dsTVw = null;

                    nc += 1;
                }

                //Load Document.
                nc = 0;
                int nnc = 0; string sNodeText1 = "";
                sTag = "";

                while (nc < tvwSet.Nodes.Count)
                {
                    nnc = 0;
                    sNodeText = tvwSet.Nodes[nc].Tag.ToString().Substring(4);

                    while (nnc < tvwSet.Nodes[nc].Nodes.Count)
                    {
                        //sNodeText1 = tvwSet.Nodes[nc].Nodes[nnc].Tag.ToString().Substring(5);
                        sNodeText1 = tvwSet.Nodes[nc].Nodes[nnc].Tag.ToString().Split('|').GetValue(1).ToString();

                        sSQL = "SELECT Distinct rowid,batchcode,setnum,collanum,doctype,scanstart,imageseq ";
                        sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuScan ";
                        sSQL += "WHERE scanproj='" + _ScanProj.Trim() + "' ";
                        sSQL += "AND appnum='" + _AppNum.Trim() + "' ";
                        sSQL += "AND batchcode='" + sNodeText + "' ";
                        sSQL += "AND setnum='" + sNodeText1 + "' ";
                        sSQL += "AND scanstatus IN (" + pDocStatusIn + ") ";
                        sSQL += "AND doctype<>'" + staMain.stcProjCfg.SeparatorText + "' ";
                        //sSQL += "AND doctype<>'SURE-REACH' ";
                        sSQL += "AND doctype='" + sNodeText1 + "' ";

                        if (pStation.Trim() != "")
                            sSQL += "AND scanstation='" + pStation.Trim().Replace("'", "") + "' ";

                        if (pUserId.Trim() != "")
                            sSQL += "AND scanuser='" + pUserId.Trim().Replace("'", "") + "' ";

                        if (pDateStart != "")
                            sSQL += "AND Convert(datetime,scanstart,103)>='" + pDateStart + "' "; //"yyyy-MM-dd HH:mm:ss"

                        if (pDateEnd != "")
                            sSQL += "AND Convert(datetime,scanend,103)<='" + pDateEnd + "' "; //"yyyy-MM-dd HH:mm:ss"

                        sSQL += "ORDER BY setnum,doctype,imageseq,scanstart ";

                        dsTVw = new DataSet();
                        dsTVw = mGlobal.returnRecordset("tTVw", sSQL);

                        if (dsTVw.Tables != null)
                        {
                            if (dsTVw.Tables.Count > 0)
                            {
                                if (dsTVw.Tables[0].Rows.Count > 0)
                                {
                                    int i = 0;
                                    TreeNode node; DataRow dr = null;
                                    while (i < dsTVw.Tables[0].Rows.Count)
                                    {
                                        dr = dsTVw.Tables[0].Rows[i];

                                        sTag = "Doc|" + dr["batchcode"].ToString() + "|" + dr["doctype"].ToString() + "|" +
                                            dr["rowid"].ToString() + "|" + dr["setnum"].ToString() + "|" +
                                            dr["collanum"].ToString() + "|" + dr["imageseq"].ToString();
                                        node = new TreeNode("Page " + (i + 1));
                                        node.Tag = new String(sTag.ToCharArray());
                                        tvwSet.Nodes[nc].Nodes[nnc].Nodes.Add(node);

                                        i += 1;
                                    }
                                }
                            }
                        }
                        dsTVw.Dispose();
                        dsTVw = null;

                        nnc += 1;
                    }

                    nc += 1;
                }

            }
            catch (Exception ex)
            {
            }
        }

        public static void loadDocuSetTreeview(TreeView tvwSet, string pBatchType, string pBatchCode, string pBatchStatusIn,
            string pDocStatusIn, string pStation = "", string pUserId = "",
            string pDateStart = "", string pDateEnd = "")
        {
            string sSQL = "";
            DataSet dsTVw = null;
            string sTag = "";
            string sNodeText = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT Distinct batchcode ";
                sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan ";
                sSQL += "WHERE scanproj='" + _ScanProj.Trim() + "' ";
                sSQL += "AND appnum='" + _AppNum.Trim() + "' ";
                sSQL += "AND batchtype='" + pBatchType.Trim() + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim() + "' ";
                sSQL += "AND batchstatus IN (" + pBatchStatusIn + ") ";

                if (pStation.Trim() != "")
                    sSQL += "AND scanstation='" + pStation.Trim().Replace("'", "") + "' ";

                if (pUserId.Trim() != "")
                    sSQL += "AND scanuser='" + pUserId.Trim().Replace("'", "") + "' ";

                if (pDateStart != "")
                    sSQL += "AND Convert(datetime,scanstart,103)>='" + pDateStart + "' "; //"yyyy-MM-dd HH:mm:ss"

                if (pDateEnd != "")
                    sSQL += "AND Convert(datetime,scanend,103)<='" + pDateEnd + "' "; //"yyyy-MM-dd HH:mm:ss"

                dsTVw = new DataSet();
                dsTVw = mGlobal.returnRecordset("tTVw", sSQL);

                //Load batches.
                if (dsTVw.Tables != null)
                {
                    if (dsTVw.Tables.Count > 0)
                    {
                        if (dsTVw.Tables[0].Rows.Count > 0)
                        {
                            tvwSet.Nodes.Clear();

                            int i = 0;
                            TreeNode node; DataRow dr = null;
                            while (i < dsTVw.Tables[0].Rows.Count)
                            {
                                dr = dsTVw.Tables[0].Rows[i];

                                sTag = "Sep|" + dr["batchcode"].ToString();
                                node = new TreeNode("Document Set " + dr["batchcode"].ToString().Replace("_", "-"));
                                node.Tag = new String(sTag.ToCharArray());
                                tvwSet.Nodes.Add(node);
								tvwSet.Nodes[0].Name = tvwSet.Nodes[0].FullPath;

                                i += 1;
                            }
                        }
                    }
                }
                dsTVw.Dispose();
                dsTVw = null;

                //Load Document Type
                int nc = 0;
                sTag = "";
                while (nc < tvwSet.Nodes.Count)
                {
                    sNodeText = tvwSet.Nodes[nc].Tag.ToString().Substring(4);

                    sSQL = "SELECT Distinct setnum,collanum,doctype ";
                    sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuScan ";
                    sSQL += "WHERE scanproj='" + _ScanProj.Trim() + "' ";
                    sSQL += "AND appnum='" + _AppNum.Trim() + "' ";
                    sSQL += "AND batchcode='" + sNodeText + "' ";
                    sSQL += "AND scanstatus IN (" + pDocStatusIn + ") "; //'S','A'
                    sSQL += "AND doctype<>'" + staMain.stcProjCfg.SeparatorText + "' ";

                    if (pStation.Trim() != "")
                        sSQL += "AND scanstation='" + pStation.Trim().Replace("'", "") + "' ";

                    if (pUserId.Trim() != "")
                        sSQL += "AND scanuser='" + pUserId.Trim().Replace("'", "") + "' ";

                    if (pDateStart != "")
                        sSQL += "AND Convert(datetime,scanstart,103)>='" + pDateStart + "' "; //"yyyy-MM-dd HH:mm:ss"

                    if (pDateEnd != "")
                        sSQL += "AND Convert(datetime,scanend,103)<='" + pDateEnd + "' "; //"yyyy-MM-dd HH:mm:ss"

                    dsTVw = new DataSet();
                    dsTVw = mGlobal.returnRecordset("tTVw", sSQL);

                    if (dsTVw.Tables != null)
                    {
                        if (dsTVw.Tables.Count > 0)
                        {
                            if (dsTVw.Tables[0].Rows.Count > 0)
                            {
                                int i = 0;
                                TreeNode node; DataRow dr = null;
                                while (i < dsTVw.Tables[0].Rows.Count)
                                {
                                    dr = dsTVw.Tables[0].Rows[i];

                                    sTag = "Sep1|" + dr["doctype"].ToString() + "|" + dr["setnum"].ToString() + "|" + dr["collanum"].ToString();
                                    node = new TreeNode(dr["doctype"].ToString());
                                    node.Tag = new String(sTag.ToCharArray());
                                    tvwSet.Nodes[nc].Nodes.Add(node);
									tvwSet.Nodes[nc].LastNode.Name = tvwSet.Nodes[nc].LastNode.FullPath;

                                    i += 1;
                                }
                            }
                        }
                    }
                    dsTVw.Dispose();
                    dsTVw = null;

                    nc += 1;
                }

                //Load Document.
                nc = 0;
                int nnc = 0; string sNodeText1 = "";
                sTag = "";

                while (nc < tvwSet.Nodes.Count)
                {
                    nnc = 0;
                    sNodeText = tvwSet.Nodes[nc].Tag.ToString().Substring(4);

                    while (nnc < tvwSet.Nodes[nc].Nodes.Count)
                    {
                        //sNodeText1 = tvwSet.Nodes[nc].Nodes[nnc].Tag.ToString().Substring(5);
                        sNodeText1 = tvwSet.Nodes[nc].Nodes[nnc].Tag.ToString().Split('|').GetValue(1).ToString();

                        sSQL = "SELECT Distinct rowid,batchcode,setnum,collanum,doctype,scanstart,imageseq ";
                        sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuScan ";
                        sSQL += "WHERE scanproj='" + _ScanProj.Trim() + "' ";
                        sSQL += "AND appnum='" + _AppNum.Trim() + "' ";
                        sSQL += "AND batchcode='" + sNodeText + "' ";
                        sSQL += "AND scanstatus IN (" + pDocStatusIn + ") ";
                        sSQL += "AND doctype<>'" + staMain.stcProjCfg.SeparatorText + "' ";
                        sSQL += "AND doctype='" + sNodeText1 + "' ";

                        if (pStation.Trim() != "")
                            sSQL += "AND scanstation='" + pStation.Trim().Replace("'", "") + "' ";

                        if (pUserId.Trim() != "")
                            sSQL += "AND scanuser='" + pUserId.Trim().Replace("'", "") + "' ";

                        if (pDateStart != "")
                            sSQL += "AND Convert(datetime,scanstart,103)>='" + pDateStart + "' "; //"yyyy-MM-dd HH:mm:ss"

                        if (pDateEnd != "")
                            sSQL += "AND Convert(datetime,scanend,103)<='" + pDateEnd + "' "; //"yyyy-MM-dd HH:mm:ss"
                        sSQL += "ORDER BY setnum,doctype,imageseq,scanstart ";

                        dsTVw = new DataSet();
                        dsTVw = mGlobal.returnRecordset("tTVw", sSQL);

                        if (dsTVw.Tables != null)
                        {
                            if (dsTVw.Tables.Count > 0)
                            {
                                if (dsTVw.Tables[0].Rows.Count > 0)
                                {
                                    int i = 0;
                                    TreeNode node; DataRow dr = null;
                                    while (i < dsTVw.Tables[0].Rows.Count)
                                    {
                                        dr = dsTVw.Tables[0].Rows[i];

                                        sTag = "Doc|" + dr["batchcode"].ToString() + "|" + dr["doctype"].ToString() + "|" +
                                            dr["rowid"].ToString() + "|" + dr["setnum"].ToString() + "|" +
                                            dr["collanum"].ToString() + "|" + dr["imageseq"].ToString();
                                        node = new TreeNode("Page " + (i + 1));
                                        node.Tag = new String(sTag.ToCharArray());
                                        tvwSet.Nodes[nc].Nodes[nnc].Nodes.Add(node);
										tvwSet.Nodes[nc].Nodes[nnc].LastNode.Name = tvwSet.Nodes[nc].Nodes[nnc].LastNode.FullPath;

                                        i += 1;
                                    }
                                }
                            }
                        }
                        dsTVw.Dispose();
                        dsTVw = null;

                        nnc += 1;
                    }

                    nc += 1;
                }

            }
            catch (Exception ex)
            {
            }
        }

        public static void loadIndexSetTreeview(TreeView tvwSet, string pBatchType, string pBatchCode, string pBatchStatusIn,
            string pDocStatusIn, string pStation = "", string pUserId = "",
            string pDateStart = "", string pDateEnd = "")
        {
            string sSQL = "";
            DataSet dsTVw = null;
            string sTag = "";
            string sNodeText = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT Distinct batchcode ";
                sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan ";
                sSQL += "WHERE scanproj='" + _ScanProj.Trim() + "' ";
                sSQL += "AND appnum='" + _AppNum.Trim() + "' ";
                sSQL += "AND batchtype='" + pBatchType.Trim() + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim() + "' ";
                sSQL += "AND batchstatus IN (" + pBatchStatusIn + ") ";

                if (pStation.Trim() != "")
                    sSQL += "AND scanstation='" + pStation.Trim().Replace("'", "") + "' ";

                if (pUserId.Trim() != "")
                    sSQL += "AND indexby='" + pUserId.Trim().Replace("'", "") + "' ";

                if (pDateStart != "")
                    sSQL += "AND Convert(datetime,scanstart,103)>='" + pDateStart + "' "; //"yyyy-MM-dd HH:mm:ss"

                if (pDateEnd != "")
                    sSQL += "AND Convert(datetime,scanend,103)<='" + pDateEnd + "' "; //"yyyy-MM-dd HH:mm:ss"

                dsTVw = new DataSet();
                dsTVw = mGlobal.returnRecordset("tTVw", sSQL);

                //Load batches.
                if (dsTVw.Tables != null)
                {
                    if (dsTVw.Tables.Count > 0)
                    {
                        if (dsTVw.Tables[0].Rows.Count > 0)
                        {
                            tvwSet.Nodes.Clear();

                            int i = 0;
                            TreeNode node; DataRow dr = null;
                            while (i < dsTVw.Tables[0].Rows.Count)
                            {
                                dr = dsTVw.Tables[0].Rows[i];

                                sTag = "Sep|" + dr["batchcode"].ToString();
                                node = new TreeNode("Document Set " + dr["batchcode"].ToString().Replace("_", "-"));
                                node.Tag = new String(sTag.ToCharArray());
                                tvwSet.Nodes.Add(node);
								tvwSet.Nodes[0].Name = tvwSet.Nodes[0].FullPath;

                                i += 1;
                            }
                        }
                    }
                }
                dsTVw.Dispose();
                dsTVw = null;

                //Load Document Type
                int nc = 0;
                sTag = "";

                while (nc < tvwSet.Nodes.Count)
                {
                    sNodeText = tvwSet.Nodes[nc].Tag.ToString().Substring(4);

                    sSQL = "SELECT Distinct setnum,collanum,doctype ";
                    sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuIndex ";
                    sSQL += "WHERE scanproj='" + _ScanProj.Trim() + "' ";
                    sSQL += "AND appnum='" + _AppNum.Trim() + "' ";
                    sSQL += "AND batchcode='" + sNodeText + "' ";
                    sSQL += "AND indexstatus IN (" + pDocStatusIn + ") "; //'S','A'
                    sSQL += "AND doctype<>'" + staMain.stcProjCfg.SeparatorText + "' ";

                    if (pStation.Trim() != "")
                        sSQL += "AND indexstation='" + pStation.Trim().Replace("'", "") + "' ";

                    if (pUserId.Trim() != "")
                        sSQL += "AND indexuser='" + pUserId.Trim().Replace("'", "") + "' ";

                    if (pDateStart != "")
                        sSQL += "AND Convert(datetime,indexstart,103)>='" + pDateStart + "' "; //"yyyy-MM-dd HH:mm:ss"

                    if (pDateEnd != "")
                        sSQL += "AND Convert(datetime,indexend,103)<='" + pDateEnd + "' "; //"yyyy-MM-dd HH:mm:ss"

                    dsTVw = new DataSet();
                    dsTVw = mGlobal.returnRecordset("tTVw", sSQL);

                    if (dsTVw.Tables != null)
                    {
                        if (dsTVw.Tables.Count > 0)
                        {
                            if (dsTVw.Tables[0].Rows.Count > 0)
                            {
                                int i = 0;
                                TreeNode node; DataRow dr = null;
                                while (i < dsTVw.Tables[0].Rows.Count)
                                {
                                    dr = dsTVw.Tables[0].Rows[i];

                                    sTag = "Sep1|" + dr["doctype"].ToString() + "|" + dr["setnum"].ToString() + "|" + dr["collanum"].ToString();
                                    node = new TreeNode(dr["doctype"].ToString());
                                    node.Tag = new String(sTag.ToCharArray());
                                    tvwSet.Nodes[nc].Nodes.Add(node);
									tvwSet.Nodes[nc].LastNode.Name = tvwSet.Nodes[nc].LastNode.FullPath;

                                    i += 1;
                                }
                            }
                        }
                    }
                    dsTVw.Dispose();
                    dsTVw = null;

                    nc += 1;
                }

                //Load Document.
                nc = 0;
                int nnc = 0; string sNodeText1 = "";
                sTag = "";

                while (nc < tvwSet.Nodes.Count)
                {
                    nnc = 0;
                    sNodeText = tvwSet.Nodes[nc].Tag.ToString().Substring(4);

                    while (nnc < tvwSet.Nodes[nc].Nodes.Count)
                    {
                        //sNodeText1 = tvwSet.Nodes[nc].Nodes[nnc].Tag.ToString().Substring(5);
                        sNodeText1 = tvwSet.Nodes[nc].Nodes[nnc].Tag.ToString().Split('|').GetValue(1).ToString();

                        sSQL = "SELECT Distinct rowid,batchcode,setnum,collanum,doctype,indexstart,imageseq ";
                        sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuIndex ";
                        sSQL += "WHERE scanproj='" + _ScanProj.Trim() + "' ";
                        sSQL += "AND appnum='" + _AppNum.Trim() + "' ";
                        sSQL += "AND batchcode='" + sNodeText + "' ";
                        sSQL += "AND indexstatus IN (" + pDocStatusIn + ") ";
                        sSQL += "AND doctype<>'" + staMain.stcProjCfg.SeparatorText + "' ";
                        sSQL += "AND doctype='" + sNodeText1 + "' ";

                        if (pStation.Trim() != "")
                            sSQL += "AND indexstation='" + pStation.Trim().Replace("'", "") + "' ";

                        if (pUserId.Trim() != "")
                            sSQL += "AND indexuser='" + pUserId.Trim().Replace("'", "") + "' ";

                        if (pDateStart != "")
                            sSQL += "AND Convert(datetime,indexstart,103)>='" + pDateStart + "' "; //"yyyy-MM-dd HH:mm:ss"

                        if (pDateEnd != "")
                            sSQL += "AND Convert(datetime,indexend,103)<='" + pDateEnd + "' "; //"yyyy-MM-dd HH:mm:ss"
                        sSQL += "ORDER BY imageseq,indexstart ";

                        dsTVw = new DataSet();
                        dsTVw = mGlobal.returnRecordset("tTVw", sSQL);

                        if (dsTVw.Tables != null)
                        {
                            if (dsTVw.Tables.Count > 0)
                            {
                                if (dsTVw.Tables[0].Rows.Count > 0)
                                {
                                    int i = 0;
                                    TreeNode node; DataRow dr = null;
                                    while (i < dsTVw.Tables[0].Rows.Count)
                                    {
                                        dr = dsTVw.Tables[0].Rows[i];

                                        sTag = "Doc|" + dr["batchcode"].ToString() + "|" + dr["doctype"].ToString() + "|" +
                                            dr["rowid"].ToString() + "|" + dr["setnum"].ToString() + "|" +
                                            dr["collanum"].ToString() + "|" + dr["imageseq"].ToString();
                                        node = new TreeNode("Page " + (i + 1));
                                        node.Tag = new String(sTag.ToCharArray());
                                        tvwSet.Nodes[nc].Nodes[nnc].Nodes.Add(node);
										tvwSet.Nodes[nc].Nodes[nnc].LastNode.Name = tvwSet.Nodes[nc].Nodes[nnc].LastNode.FullPath;

                                        i += 1;
                                    }
                                }
                            }
                        }
                        dsTVw.Dispose();
                        dsTVw = null;

                        nnc += 1;
                    }

                    nc += 1;
                }

            }
            catch (Exception ex)
            {
            }
        }

        public static void loadIndexBatchTreeview(TreeView tvwSet, string pBatchType, string pBatchCode, string pBatchStatusIn,
            string pDocStatusIn, string pStation = "", string pUserId = "",
            string pDateStart = "", string pDateEnd = "")
        {
            string sSQL = "";
            DataSet dsTVw = null;
            string sTag = "";
            string sNodeText = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                //Load batch.
                sSQL = "SELECT Distinct batchcode ";
                sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan ";
                sSQL += "WHERE scanproj='" + _ScanProj.Trim() + "' ";
                sSQL += "AND appnum='" + _AppNum.Trim() + "' ";
                sSQL += "AND batchtype='" + pBatchType.Trim() + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim() + "' ";
                sSQL += "AND batchstatus IN (" + pBatchStatusIn + ") ";

                if (pStation.Trim() != "")
                    sSQL += "AND scanstation='" + pStation.Trim().Replace("'", "") + "' ";

                if (pUserId.Trim() != "")
                    sSQL += "AND scanuser='" + pUserId.Trim().Replace("'", "") + "' ";

                if (pDateStart != "")
                    sSQL += "AND Convert(datetime,scanstart,103)>='" + pDateStart + "' "; //"yyyy-MM-dd HH:mm:ss"

                if (pDateEnd != "")
                    sSQL += "AND Convert(datetime,scanend,103)<='" + pDateEnd + "' "; //"yyyy-MM-dd HH:mm:ss"

                dsTVw = new DataSet();
                dsTVw = mGlobal.returnRecordset("tTVw", sSQL);

                //Load batches.
                if (dsTVw.Tables != null)
                {
                    if (dsTVw.Tables.Count > 0)
                    {
                        if (dsTVw.Tables[0].Rows.Count > 0)
                        {
                            tvwSet.Nodes.Clear();

                            int i = 0;
                            TreeNode node; DataRow dr = null;
                            while (i < dsTVw.Tables[0].Rows.Count)
                            {
                                dr = dsTVw.Tables[0].Rows[i];

                                sTag = "Sep|" + dr["batchcode"].ToString();
                                node = new TreeNode("Batch " + dr["batchcode"].ToString().Replace("_", "-"));
                                node.Tag = new String(sTag.ToCharArray());
                                tvwSet.Nodes.Add(node);
								tvwSet.Nodes[0].Name = tvwSet.Nodes[0].FullPath;

                                i += 1;
                            }
                        }
                    }
                }
                dsTVw.Dispose();
                dsTVw = null;

                //Load Set
                int nc = 0;
                sTag = "";
                if (stcProjCfg.NoSeparator.Trim() != "C")
                {
                    while (nc < tvwSet.Nodes.Count)
                    {
                        sNodeText = tvwSet.Nodes[nc].Tag.ToString().Substring(4);

                        sSQL = "SELECT Distinct setnum ";
                        sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan ";
                        sSQL += "WHERE scanproj='" + _ScanProj.Trim() + "' ";
                        sSQL += "AND appnum='" + _AppNum.Trim() + "' ";
                        sSQL += "AND batchtype='" + pBatchType.Trim() + "' ";
                        sSQL += "AND batchcode='" + sNodeText.Trim() + "' ";
                        sSQL += "AND batchstatus IN (" + pBatchStatusIn + ") ";

                        if (pStation.Trim() != "")
                            sSQL += "AND scanstation='" + pStation.Trim().Replace("'", "") + "' ";

                        if (pUserId.Trim() != "")
                            sSQL += "AND scanuser='" + pUserId.Trim().Replace("'", "") + "' ";

                        if (pDateStart != "")
                            sSQL += "AND Convert(datetime,scanstart,103)>='" + pDateStart + "' "; //"yyyy-MM-dd HH:mm:ss"

                        if (pDateEnd != "")
                            sSQL += "AND Convert(datetime,scanend,103)<='" + pDateEnd + "' "; //"yyyy-MM-dd HH:mm:ss"

                        dsTVw = new DataSet();
                        dsTVw = mGlobal.returnRecordset("tTVw", sSQL);

                        if (dsTVw.Tables != null)
                        {
                            if (dsTVw.Tables.Count > 0)
                            {
                                if (dsTVw.Tables[0].Rows.Count > 0)
                                {
                                    int i = 0;
                                    TreeNode node; DataRow dr = null;
                                    while (i < dsTVw.Tables[0].Rows.Count)
                                    {
                                        dr = dsTVw.Tables[0].Rows[i];

                                        sTag = "Sep1|" + dr["setnum"].ToString();
                                        node = new TreeNode("Document Set " + dr["setnum"].ToString());
                                        node.Tag = new String(sTag.ToCharArray());
                                        tvwSet.Nodes[nc].Nodes.Add(node);
                                        tvwSet.Nodes[nc].LastNode.Name = tvwSet.Nodes[nc].LastNode.FullPath;

                                        i += 1;
                                    }
                                }
                            }
                        }
                        dsTVw.Dispose();
                        dsTVw = null;

                        nc += 1;
                    } //End While.
                }

                int iNodesCnt = 0, iLev = 1;
                //Load Document Type
                nc = 0;
                int nnc = 0; string sNodeText1 = "";                
                if (stcProjCfg.NoSeparator.Trim() == "2" || stcProjCfg.NoSeparator.Trim() == "C")
                {
                    sTag = "";

                    while (nc < tvwSet.Nodes.Count)
                    {
                        nnc = 0;
                        sNodeText = tvwSet.Nodes[nc].Tag.ToString().Substring(4);

                        if (stcProjCfg.NoSeparator.Trim() == "C")
                            iNodesCnt = tvwSet.Nodes.Count;
                        else
                            iNodesCnt = tvwSet.Nodes[nc].Nodes.Count;

                        while (nnc < iNodesCnt)
                        {
                            if (stcProjCfg.NoSeparator.Trim() == "2")
                                sNodeText1 = tvwSet.Nodes[nc].Nodes[nnc].Tag.ToString().Substring(5);

                            sSQL = "SELECT Distinct collanum,doctype,setnum ";
                            sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuIndex ";
                            sSQL += "WHERE scanproj='" + _ScanProj.Trim() + "' ";
                            sSQL += "AND appnum='" + _AppNum.Trim() + "' ";
                            sSQL += "AND batchcode='" + sNodeText + "' ";
                            if (stcProjCfg.NoSeparator.Trim() == "2")
                                sSQL += "AND setnum=" + sNodeText1 + " ";
                            sSQL += "AND indexstatus IN (" + pDocStatusIn + ") "; //'S','A'
                            sSQL += "AND doctype<>'" + staMain.stcProjCfg.SeparatorText + "' ";

                            if (pStation.Trim() != "")
                                sSQL += "AND indexstation='" + pStation.Trim().Replace("'", "") + "' ";

                            if (pUserId.Trim() != "")
                                sSQL += "AND indexuser='" + pUserId.Trim().Replace("'", "") + "' ";

                            if (pDateStart != "")
                                sSQL += "AND Convert(datetime,indexstart,103)>='" + pDateStart + "' "; //"yyyy-MM-dd HH:mm:ss"

                            if (pDateEnd != "")
                                sSQL += "AND Convert(datetime,indexend,103)<='" + pDateEnd + "' "; //"yyyy-MM-dd HH:mm:ss"

                            dsTVw = new DataSet();
                            dsTVw = mGlobal.returnRecordset("tTVw", sSQL);

                            if (dsTVw.Tables != null)
                            {
                                if (dsTVw.Tables.Count > 0)
                                {
                                    if (dsTVw.Tables[0].Rows.Count > 0)
                                    {
                                        int i = 0;
                                        TreeNode node; DataRow dr = null;
                                        while (i < dsTVw.Tables[0].Rows.Count)
                                        {
                                            dr = dsTVw.Tables[0].Rows[i];

                                            if (stcProjCfg.NoSeparator.Trim() == "C")
                                                sTag = "Sep1|" + dr["doctype"].ToString() + "|" + dr["setnum"].ToString() + "|" + dr["collanum"].ToString();
                                            else
                                                sTag = "Sep2|" + dr["doctype"].ToString() + "|" + sNodeText1 + "|" + dr["collanum"].ToString();
                                            node = new TreeNode(dr["doctype"].ToString());
                                            node.Tag = new String(sTag.ToCharArray());

                                            if (stcProjCfg.NoSeparator.Trim() == "C")
                                            {
                                                tvwSet.Nodes[nc].Nodes.Add(node);
                                                tvwSet.Nodes[nc].LastNode.Name = tvwSet.Nodes[nc].LastNode.FullPath;
                                            }
                                            else
                                            {
                                                tvwSet.Nodes[nc].Nodes[nnc].Nodes.Add(node);
                                                tvwSet.Nodes[nc].Nodes[nnc].LastNode.Name = tvwSet.Nodes[nc].Nodes[nnc].LastNode.FullPath;
                                            }

                                            i += 1;
                                        }
                                    }
                                }
                            }
                            dsTVw.Dispose();
                            dsTVw = null;

                            nnc += 1;
                        }

                        nc += 1;
                    }
                }

                //Load Document.
                nc = 0;
                nnc = 0; sNodeText1 = ""; int nnnc = 0; string sNodeText2 = "";
                sTag = "";
                iNodesCnt = 0; iLev = 1;

                while (nc < tvwSet.Nodes.Count)
                {
                    nnc = 0;
                    sNodeText = tvwSet.Nodes[nc].Tag.ToString().Substring(4);

                    while (nnc < tvwSet.Nodes[nc].Nodes.Count)
                    {
                        nnnc = 0;
                        if (stcProjCfg.NoSeparator.Trim() == "C")
                        {
                            sNodeText1 = tvwSet.Nodes[nc].Nodes[nnc].Tag.ToString().Split('|').GetValue(2).ToString(); //Setnum.
                            sNodeText2 = tvwSet.Nodes[nc].Nodes[nnc].Tag.ToString().Split('|').GetValue(1).ToString(); //DocType.
                        }
                        else
                            sNodeText1 = tvwSet.Nodes[nc].Nodes[nnc].Tag.ToString().Substring(5); //Setnum.
                        if (stcProjCfg.NoSeparator.Trim() == "2")
                        {
                            iNodesCnt = tvwSet.Nodes[nc].Nodes[nnc].Nodes.Count;
                            iLev = 2;
                        }
                        else
                        {
                            iNodesCnt = tvwSet.Nodes[nc].Nodes.Count;
                            iLev = 1;
                            nnnc = nnc;
                        }

                        while (nnnc < iNodesCnt)
                        {
                            //sNodeText2 = tvwSet.Nodes[nc].Nodes[nnc].Nodes[nnnc].Tag.ToString().Substring(5);
                            if (iLev == 2)
                                sNodeText2 = tvwSet.Nodes[nc].Nodes[nnc].Nodes[nnnc].Tag.ToString().Split('|').GetValue(1).ToString();
                           
                            sSQL = "SELECT Distinct rowid,batchcode,setnum,collanum,doctype,indexstart,imageseq ";
                            sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuIndex ";
                            sSQL += "WHERE scanproj='" + _ScanProj.Trim() + "' ";
                            sSQL += "AND appnum='" + _AppNum.Trim() + "' ";
                            sSQL += "AND batchcode='" + sNodeText + "' ";
                            sSQL += "AND setnum=" + sNodeText1 + " ";
                            sSQL += "AND doctype<>'" + staMain.stcProjCfg.SeparatorText + "' ";
                            if (iLev == 2 || stcProjCfg.NoSeparator.Trim() == "C")
                                sSQL += "AND doctype='" + sNodeText2 + "' ";
                            sSQL += "AND indexstatus IN (" + pDocStatusIn + ") ";

                            if (pStation.Trim() != "")
                                sSQL += "AND indexstation='" + pStation.Trim().Replace("'", "") + "' ";

                            if (pUserId.Trim() != "")
                                sSQL += "AND indexuser='" + pUserId.Trim().Replace("'", "") + "' ";

                            sSQL += "ORDER BY imageseq,indexstart ";

                            dsTVw = new DataSet();
                            dsTVw = mGlobal.returnRecordset("tTVw", sSQL);

                            if (dsTVw.Tables != null)
                            {
                                if (dsTVw.Tables.Count > 0)
                                {
                                    if (dsTVw.Tables[0].Rows.Count > 0)
                                    {
                                        int i = 0;
                                        TreeNode node; DataRow dr = null;
                                        while (i < dsTVw.Tables[0].Rows.Count)
                                        {
                                            dr = dsTVw.Tables[0].Rows[i];

                                            sTag = "Doc|" + dr["batchcode"].ToString() + "|" + dr["doctype"].ToString() + "|" +
                                                dr["rowid"].ToString() + "|" + dr["setnum"].ToString() + "|" +
                                                dr["collanum"].ToString() + "|" + dr["imageseq"].ToString();
                                            node = new TreeNode("Page " + (i + 1));
                                            node.Tag = new String(sTag.ToCharArray());

                                            if (iLev == 2)
                                            {
                                                tvwSet.Nodes[nc].Nodes[nnc].Nodes[nnnc].Nodes.Add(node);
                                                tvwSet.Nodes[nc].Nodes[nnc].Nodes[nnnc].LastNode.Name = tvwSet.Nodes[nc].Nodes[nnc].Nodes[nnnc].LastNode.FullPath;
                                            }
                                            else
                                            {
                                                tvwSet.Nodes[nc].Nodes[nnc].Nodes.Add(node);
                                                tvwSet.Nodes[nc].Nodes[nnc].LastNode.Name = tvwSet.Nodes[nc].Nodes[nnc].LastNode.FullPath;

                                            }

                                            i += 1;
                                        }
                                    }
                                }
                            }
                            dsTVw.Dispose();
                            dsTVw = null;

                            nnnc += 1;
                            if (iLev == 1) break;
                        }

                        nnc += 1;
                    }

                    nc += 1;
                }

            }
            catch (Exception ex)
            {
            }
        }

        public static void loadIndexBatchTreeviewNoSep(TreeView tvwSet, string pBatchType, string pBatchCode, string pBatchStatusIn,
            string pDocStatusIn, string pStation = "", string pUserId = "",
            string pDateStart = "", string pDateEnd = "")
        {
            string sSQL = "";
            DataSet dsTVw = null;
            string sTag = "";
            string sNodeText = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                //Load batch.
                sSQL = "SELECT Distinct batchcode ";
                sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan ";
                sSQL += "WHERE scanproj='" + _ScanProj.Trim() + "' ";
                sSQL += "AND appnum='" + _AppNum.Trim() + "' ";
                sSQL += "AND batchtype='" + pBatchType.Trim() + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim() + "' ";
                sSQL += "AND batchstatus IN (" + pBatchStatusIn + ") ";

                if (pStation.Trim() != "")
                    sSQL += "AND scanstation='" + pStation.Trim().Replace("'", "") + "' ";

                if (pUserId.Trim() != "")
                    sSQL += "AND scanuser='" + pUserId.Trim().Replace("'", "") + "' ";

                if (pDateStart != "")
                    sSQL += "AND Convert(datetime,scanstart,103)>='" + pDateStart + "' "; //"yyyy-MM-dd HH:mm:ss"

                if (pDateEnd != "")
                    sSQL += "AND Convert(datetime,scanend,103)<='" + pDateEnd + "' "; //"yyyy-MM-dd HH:mm:ss"

                dsTVw = new DataSet();
                dsTVw = mGlobal.returnRecordset("tTVw", sSQL);

                //Load batches.
                if (dsTVw.Tables != null)
                {
                    if (dsTVw.Tables.Count > 0)
                    {
                        if (dsTVw.Tables[0].Rows.Count > 0)
                        {
                            tvwSet.Nodes.Clear();

                            int i = 0;
                            TreeNode node; DataRow dr = null;
                            while (i < dsTVw.Tables[0].Rows.Count)
                            {
                                dr = dsTVw.Tables[0].Rows[i];

                                sTag = "Sep|" + dr["batchcode"].ToString();
                                node = new TreeNode("Batch " + dr["batchcode"].ToString().Replace("_", "-"));
                                node.Tag = new String(sTag.ToCharArray());
                                tvwSet.Nodes.Add(node);
                                tvwSet.Nodes[0].Name = tvwSet.Nodes[0].FullPath;

                                i += 1;
                            }
                        }
                    }
                }
                dsTVw.Dispose();
                dsTVw = null;                

                //Load Document.
                int nc = 0;
                sTag = "";

                while (nc < tvwSet.Nodes.Count)
                {
                    sNodeText = tvwSet.Nodes[nc].Tag.ToString().Substring(4);

                    sSQL = "SELECT Distinct rowid,batchcode,setnum,collanum,doctype,indexstart,imageseq ";
                    sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuIndex ";
                    sSQL += "WHERE scanproj='" + _ScanProj.Trim() + "' ";
                    sSQL += "AND appnum='" + _AppNum.Trim() + "' ";
                    sSQL += "AND batchcode='" + sNodeText + "' ";
                    sSQL += "AND doctype<>'" + staMain.stcProjCfg.SeparatorText + "' ";
                    sSQL += "AND indexstatus IN (" + pDocStatusIn + ") ";

                    if (pStation.Trim() != "")
                        sSQL += "AND indexstation='" + pStation.Trim().Replace("'", "") + "' ";

                    if (pUserId.Trim() != "")
                        sSQL += "AND indexuser='" + pUserId.Trim().Replace("'", "") + "' ";

                    sSQL += "ORDER BY imageseq,indexstart ";

                    dsTVw = new DataSet();
                    dsTVw = mGlobal.returnRecordset("tTVw", sSQL);

                    if (dsTVw.Tables != null)
                    {
                        if (dsTVw.Tables.Count > 0)
                        {
                            if (dsTVw.Tables[0].Rows.Count > 0)
                            {
                                int i = 0;
                                TreeNode node; DataRow dr = null;
                                while (i < dsTVw.Tables[0].Rows.Count)
                                {
                                    dr = dsTVw.Tables[0].Rows[i];

                                    sTag = "Doc|" + dr["batchcode"].ToString() + "|" + dr["doctype"].ToString() + "|" +
                                        dr["rowid"].ToString() + "|" + dr["setnum"].ToString() + "|" +
                                        dr["collanum"].ToString() + "|" + dr["imageseq"].ToString();
                                    node = new TreeNode("Page " + (i + 1));
                                    node.Tag = new String(sTag.ToCharArray());

                                    tvwSet.Nodes[nc].Nodes.Add(node);
                                    tvwSet.Nodes[nc].LastNode.Name = tvwSet.Nodes[nc].LastNode.FullPath;

                                    i += 1;
                                }
                            }
                        }
                    }
                    dsTVw.Dispose();
                    dsTVw = null;

                    nc += 1;
                }

            }
            catch (Exception ex)
            {
            }
        }

        public static void loadIndexSetTreeviewNoSep(TreeView tvwSet, string pBatchType, string pBatchCode, string pBatchStatusIn,
            string pDocStatusIn, string pStation = "", string pUserId = "",
            string pDateStart = "", string pDateEnd = "")
        {
            string sSQL = "";
            DataSet dsTVw = null;
            string sTag = "";
            string sNodeText = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT Distinct batchcode ";
                sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan ";
                sSQL += "WHERE scanproj='" + _ScanProj.Trim() + "' ";
                sSQL += "AND appnum='" + _AppNum.Trim() + "' ";
                sSQL += "AND batchtype='" + pBatchType.Trim() + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim() + "' ";
                sSQL += "AND batchstatus IN (" + pBatchStatusIn + ") ";

                if (pStation.Trim() != "")
                    sSQL += "AND scanstation='" + pStation.Trim().Replace("'", "") + "' ";

                if (pUserId.Trim() != "")
                    sSQL += "AND indexby='" + pUserId.Trim().Replace("'", "") + "' ";

                if (pDateStart != "")
                    sSQL += "AND Convert(datetime,scanstart,103)>='" + pDateStart + "' "; //"yyyy-MM-dd HH:mm:ss"

                if (pDateEnd != "")
                    sSQL += "AND Convert(datetime,scanend,103)<='" + pDateEnd + "' "; //"yyyy-MM-dd HH:mm:ss"

                dsTVw = new DataSet();
                dsTVw = mGlobal.returnRecordset("tTVw", sSQL);

                //Load batches.
                if (dsTVw.Tables != null)
                {
                    if (dsTVw.Tables.Count > 0)
                    {
                        if (dsTVw.Tables[0].Rows.Count > 0)
                        {
                            tvwSet.Nodes.Clear();

                            int i = 0;
                            TreeNode node; DataRow dr = null;
                            while (i < dsTVw.Tables[0].Rows.Count)
                            {
                                dr = dsTVw.Tables[0].Rows[i];

                                sTag = "Sep|" + dr["batchcode"].ToString();
                                node = new TreeNode("Document Set " + dr["batchcode"].ToString().Replace("_", "-"));
                                node.Tag = new String(sTag.ToCharArray());
                                tvwSet.Nodes.Add(node);
                                tvwSet.Nodes[0].Name = tvwSet.Nodes[0].FullPath;

                                i += 1;
                            }
                        }
                    }
                }
                dsTVw.Dispose();
                dsTVw = null;

                //Load Document.
                int nc = 0;
                sTag = "";

                while (nc < tvwSet.Nodes.Count)
                {
                    sNodeText = tvwSet.Nodes[nc].Tag.ToString().Substring(4);

                    sSQL = "SELECT Distinct rowid,batchcode,setnum,collanum,doctype,indexstart,imageseq ";
                    sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuIndex ";
                    sSQL += "WHERE scanproj='" + _ScanProj.Trim() + "' ";
                    sSQL += "AND appnum='" + _AppNum.Trim() + "' ";
                    sSQL += "AND batchcode='" + sNodeText + "' ";
                    sSQL += "AND indexstatus IN (" + pDocStatusIn + ") ";
                    sSQL += "AND doctype<>'" + staMain.stcProjCfg.SeparatorText + "' ";

                    if (pStation.Trim() != "")
                        sSQL += "AND indexstation='" + pStation.Trim().Replace("'", "") + "' ";

                    if (pUserId.Trim() != "")
                        sSQL += "AND indexuser='" + pUserId.Trim().Replace("'", "") + "' ";

                    if (pDateStart != "")
                        sSQL += "AND Convert(datetime,indexstart,103)>='" + pDateStart + "' "; //"yyyy-MM-dd HH:mm:ss"

                    if (pDateEnd != "")
                        sSQL += "AND Convert(datetime,indexend,103)<='" + pDateEnd + "' "; //"yyyy-MM-dd HH:mm:ss"
                    sSQL += "ORDER BY imageseq,indexstart ";

                    dsTVw = new DataSet();
                    dsTVw = mGlobal.returnRecordset("tTVw", sSQL);

                    if (dsTVw.Tables != null)
                    {
                        if (dsTVw.Tables.Count > 0)
                        {
                            if (dsTVw.Tables[0].Rows.Count > 0)
                            {
                                int i = 0;
                                TreeNode node; DataRow dr = null;
                                while (i < dsTVw.Tables[0].Rows.Count)
                                {
                                    dr = dsTVw.Tables[0].Rows[i];

                                    sTag = "Doc|" + dr["batchcode"].ToString() + "|" + dr["doctype"].ToString() + "|" +
                                        dr["rowid"].ToString() + "|" + dr["setnum"].ToString() + "|" +
                                        dr["collanum"].ToString() + "|" + dr["imageseq"].ToString();
                                    node = new TreeNode("Page " + (i + 1));
                                    node.Tag = new String(sTag.ToCharArray());
                                    tvwSet.Nodes[nc].Nodes.Add(node);
                                    tvwSet.Nodes[nc].LastNode.Name = tvwSet.Nodes[nc].LastNode.FullPath;

                                    i += 1;
                                }
                            }
                        }
                    }
                    dsTVw.Dispose();
                    dsTVw = null;

                    nc += 1;
                }

            }
            catch (Exception ex)
            {
            }
        }

        public static string postScanSummary(string pProcessType, string pScanPrj, string pAppNum, string pSetNum, string pBatchCode, string pDocType,
            string pStatus, string pBranchCode, string pStation, string pUserId, string pDateStart, string pDateEnd)
        {
            try
            {
                ScanSumm oSumm = new ScanSumm();
                oSumm.Scantype = pProcessType.Trim();
                oSumm.Scanproj = pScanPrj.Trim();
                oSumm.Appnum = pAppNum.Trim();
                oSumm.Setnum = pSetNum.Trim();
                oSumm.Batchcode = pBatchCode.Trim();
                oSumm.Doctype = pDocType.Trim();
                oSumm.Scanstatus = pStatus.Trim();
                oSumm.Scanbranch = pBranchCode.Trim();
                oSumm.Scanstation = pStation.Trim();
                oSumm.Scanuser = pUserId.Trim();
                oSumm.Scanstart = Convert.ToDateTime(pDateStart.Trim());
                oSumm.Scanend = Convert.ToDateTime(pDateEnd.Trim());

                ScanSumm[] oSumms = new ScanSumm[1];
                oSumms[0] = oSumm;

                string sJson = SerializeSumm.ToJson(oSumms);

                clsHttpHandler oWebAPI = new clsHttpHandler();

                oWebAPI.gCred = mGlobal.GetAppCfg("Crede") + ":" + mGlobal.GetAppCfg("Crede1");

                string sMsg = "";
                string sContType = "text/json; encoding='utf-8'";
                System.Collections.Hashtable tHeader = new System.Collections.Hashtable(2);
                tHeader.Add("m", "scsumm");

                System.Net.HttpWebResponse oResp = oWebAPI.HttpCall("Post", mGlobal.GetAppCfg("BaseUrl"), sContType, tHeader, sJson, ref sMsg);

                if (sMsg.Trim() == "")
                {
                    Stream oRespStrm = oResp.GetResponseStream();
                    StreamReader oSReader = new StreamReader(oRespStrm);
                    string sResp = oSReader.ReadToEnd();

                    if (sResp.Trim().ToLower() != "completed")
                        return sResp;
                }
                else
                    return sMsg;

            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return "";
        }

        public static bool validateUserRole(string pRoleName)
        {
            try
            {
                return new clsLogin().validateUserRole(pRoleName);
            }
            catch (Exception ex)
            {
                return false;
            }
            return false;
        }

        public static string getBatchCodeByProj(string pBatchType, string pAppNum, string pSetNum, string pBatchNum)
        {
            string sBatchCode = "";
            string[] sFormats;
            string sFormat;
            string[] sDelims;
            clsDocuFile oDF = new clsDocuFile();

            if (stcProjCfg.BatchFmt.Trim() != "" && staMain.stcProjCfg.BatchAuto.Trim().ToUpper() == "N")
            {
                sFormats = stcProjCfg.BatchFmt.Split('-');
                if (stcProjCfg.BatchFmt.IndexOf("-") == -1) sFormats = stcProjCfg.BatchFmt.Split('_');

                int i = 0; int j = 0;
                while (i < sFormats.Length)
                {
                    sFormat = sFormats[i].Trim();
                    sDelims = sFormat.Split(new string[] { "><" }, StringSplitOptions.None);

                    while (j < sDelims.Length)
                    {
                        sFormat = sDelims[j].Trim();
                        if (sFormat.IndexOf("<") == -1) sFormat = "<" + sFormat;
                        if (sFormat.IndexOf(">") == -1) sFormat = sFormat + ">";

                        if (sFormat == "<sys app no>")
                        {
                            if (i == sFormats.Length - 1) sBatchCode += pAppNum; else sBatchCode += pAppNum + "_";
                        }
                        else if (sFormat == "<batch running no>")
                        {
                            if (i == sFormats.Length - 1)
                            {
                                sBatchCode += pBatchNum;
                            }
                            else
                            {
                                sBatchCode += pBatchNum + "_";
                            }
                        }
                        else if (sFormat == "<set running no>")
                        {
                            if (i == sFormats.Length - 1)
                            {
                                sBatchCode += pSetNum;
                            }
                            else
                            {
                                sBatchCode += pSetNum + "_";
                            }
                        }
                        else if (sFormat == "<yyyyMMdd>")
                        {
                            if (i == sFormats.Length - 1)
                            {
                                sBatchCode = oDF.getFormatBatchNum(sBatchCode, DateTime.Now, "yyyyMMdd");
                            }
                            else
                            {
                                sBatchCode = oDF.getFormatBatchNum(sBatchCode, DateTime.Now, "yyyyMMdd") + "_";
                            }
                        }
                        else if (sFormat == "<ddMMyyyy>")
                        {
                            if (i == sFormats.Length - 1)
                            {
                                sBatchCode = oDF.getFormatBatchNum(sBatchCode, DateTime.Now, "ddMMyyyy");
                            }
                            else
                            {
                                sBatchCode = oDF.getFormatBatchNum(sBatchCode, DateTime.Now, "ddMMyyyy") + "_";
                            }
                        }

                        j += 1;
                    }

                    j = 0;
                    i += 1;
                }
            }
            else //Auto, <sys app no>_<yyyyMMdd><batch running no>
            {
                //if (pBatchType.Trim().ToLower() == "set")
                //{
                //    if (staMain.stcProjCfg.NoSeparator.Trim() == "1") //2 separators is not possible for SET batch type.
                //        sBatchCode = pAppNum + "_" + DateTime.Now.ToString("yyyyMMdd") + pBatchNum;
                //    else
                //        sBatchCode = pAppNum + "_" + DateTime.Now.ToString("yyyyMMdd") + pSetNum;
                //}                    
                //else
                sBatchCode = pAppNum + "_" + DateTime.Now.ToString("yyyyMMdd") + pBatchNum;
            }

            return sBatchCode;
        }

        public static bool checkIndexAutoOCREnabled()
        {
            try
            {
                if (staMain.stcIndexSetting.FieldAutoOCR != null)
                {
                    if (staMain.stcIndexSetting.FieldAutoOCR.Count > 0)
                    {
                        int i = 0;
                        while (i < staMain.stcIndexSetting.FieldAutoOCR.Count)
                        {
                            if (staMain.stcIndexSetting.FieldAutoOCR[i].ToString().ToUpper() == "Y")
                            {
                                return true;
                            }

                            i += 1;
                        }
                    }                    
                }

                return false;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return false;
        }

        public static List<string> checkIndexAutoOCRBarcodeEnabled()
        {
            List<string> lNames = new List<string>();
            try
            {
                if (staMain.stcIndexSetting.FieldDataType != null)
                {
                    if (staMain.stcIndexSetting.FieldDataType.Count > 0)
                    {
                        int i = 0;
                        while (i < staMain.stcIndexSetting.FieldDataType.Count)
                        {
                            if (staMain.stcIndexSetting.FieldDataType[i].ToLower() == "barcode")
                            {
                                lNames.Add(staMain.stcIndexSetting.FieldName[i]);
                            }
                            
                            i += 1;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return lNames;
        }

        public static bool checkImageNodesAvailable(TreeView pTvwBatch)
        {
            try
            {
                if (staMain.stcProjCfg.NoSeparator.Trim() == "0")
                {
                    if (pTvwBatch.Nodes[0].Nodes.Count > 0) return true;
                }
                else if (staMain.stcProjCfg.NoSeparator.Trim() == "1")
                {
                    if (pTvwBatch.Nodes[0].Nodes[0].Nodes.Count > 0) return true;
                }
                else if (staMain.stcProjCfg.NoSeparator.Trim() == "C")
                {
                    if (pTvwBatch.Nodes[0].Nodes[0].Nodes.Count > 0) return true;
                }
                else //2 separators.
                {
                    if (pTvwBatch.Nodes[0].Nodes[0].Nodes[0].Nodes.Count > 0) return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return false;
        }

        public static void loadBatchTreeviewUnion(TreeView tvwSet, string pBatchType, string pBatchCode, string pBatchStatusIn,
        string pDocStatusIn, string pStation = "", string pUserId = "",
        string pDateStart = "", string pDateEnd = "")
        {
            string sSQL = "";
            DataSet dsTVw = null;
            string sTag = "";
            string sNodeText = "";
            try
            {
                mGlobal.LoadAppDBCfg();

                //Load batch.
                sSQL = "SELECT Distinct batchcode ";
                sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan ";
                sSQL += "WHERE scanproj='" + _ScanProj.Trim() + "' ";
                sSQL += "AND appnum='" + _AppNum.Trim() + "' ";
                sSQL += "AND batchtype='" + pBatchType.Trim() + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim() + "' ";
                sSQL += "AND batchstatus IN (" + pBatchStatusIn + ") ";

                if (pStation.Trim() != "")
                    sSQL += "AND scanstation='" + pStation.Trim().Replace("'", "") + "' ";

                if (pUserId.Trim() != "")
                    sSQL += "AND scanuser='" + pUserId.Trim().Replace("'", "") + "' ";

                if (pDateStart != "")
                    sSQL += "AND Convert(datetime,scanstart,103)>='" + pDateStart + "' "; //"yyyy-MM-dd HH:mm:ss"

                if (pDateEnd != "")
                    sSQL += "AND Convert(datetime,scanend,103)<='" + pDateEnd + "' "; //"yyyy-MM-dd HH:mm:ss"

                dsTVw = new DataSet();
                dsTVw = mGlobal.returnRecordset("tTVw", sSQL);

                //Load batches.
                if (dsTVw.Tables != null)
                {
                    if (dsTVw.Tables.Count > 0)
                    {
                        if (dsTVw.Tables[0].Rows.Count > 0)
                        {
                            tvwSet.Nodes.Clear();

                            int i = 0;
                            TreeNode node; DataRow dr = null;
                            while (i < dsTVw.Tables[0].Rows.Count)
                            {
                                dr = dsTVw.Tables[0].Rows[i];

                                sTag = "Sep|" + dr["batchcode"].ToString();
                                node = new TreeNode("Batch " + dr["batchcode"].ToString().Replace("_", "-"));
                                node.Tag = new String(sTag.ToCharArray());
                                tvwSet.Nodes.Add(node);
                                tvwSet.Nodes[0].Name = tvwSet.Nodes[0].FullPath;

                                i += 1;
                            }
                        }
                    }
                }
                dsTVw.Dispose();
                dsTVw = null;

                //Load Set
                int nc = 0;
                sTag = "";

                while (nc < tvwSet.Nodes.Count)
                {
                    sNodeText = tvwSet.Nodes[nc].Tag.ToString().Substring(4);

                    sSQL = "SELECT Distinct setnum ";
                    sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan ";
                    sSQL += "WHERE scanproj='" + _ScanProj.Trim() + "' ";
                    sSQL += "AND appnum='" + _AppNum.Trim() + "' ";
                    sSQL += "AND batchtype='" + pBatchType.Trim() + "' ";
                    sSQL += "AND batchcode='" + sNodeText.Trim() + "' ";
                    sSQL += "AND batchstatus IN (" + pBatchStatusIn + ") ";

                    if (pStation.Trim() != "")
                        sSQL += "AND scanstation='" + pStation.Trim().Replace("'", "") + "' ";

                    if (pUserId.Trim() != "")
                        sSQL += "AND scanuser='" + pUserId.Trim().Replace("'", "") + "' ";

                    if (pDateStart != "")
                        sSQL += "AND Convert(datetime,scanstart,103)>='" + pDateStart + "' "; //"yyyy-MM-dd HH:mm:ss"

                    if (pDateEnd != "")
                        sSQL += "AND Convert(datetime,scanend,103)<='" + pDateEnd + "' "; //"yyyy-MM-dd HH:mm:ss"

                    dsTVw = new DataSet();
                    dsTVw = mGlobal.returnRecordset("tTVw", sSQL);

                    if (dsTVw.Tables != null)
                    {
                        if (dsTVw.Tables.Count > 0)
                        {
                            if (dsTVw.Tables[0].Rows.Count > 0)
                            {
                                int i = 0;
                                TreeNode node; DataRow dr = null;
                                while (i < dsTVw.Tables[0].Rows.Count)
                                {
                                    dr = dsTVw.Tables[0].Rows[i];

                                    sTag = "Sep1|" + dr["setnum"].ToString();
                                    node = new TreeNode("Document Set " + dr["setnum"].ToString());
                                    node.Tag = new String(sTag.ToCharArray());
                                    tvwSet.Nodes[nc].Nodes.Add(node);
                                    tvwSet.Nodes[nc].LastNode.Name = tvwSet.Nodes[nc].LastNode.FullPath;

                                    i += 1;
                                }
                            }
                        }
                    }
                    dsTVw.Dispose();
                    dsTVw = null;

                    nc += 1;
                }

                //Load Document Type
                nc = 0;
                int nnc = 0; string sNodeText1 = "";
                if (stcProjCfg.NoSeparator.Trim() == "2")
                {
                    sTag = "";

                    while (nc < tvwSet.Nodes.Count)
                    {
                        nnc = 0;
                        sNodeText = tvwSet.Nodes[nc].Tag.ToString().Substring(4);

                        while (nnc < tvwSet.Nodes[nc].Nodes.Count)
                        {
                            sNodeText1 = tvwSet.Nodes[nc].Nodes[nnc].Tag.ToString().Substring(5);

                            sSQL = "SELECT Distinct collanum,doctype ";
                            sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuIndex ";
                            sSQL += "WHERE scanproj='" + _ScanProj.Trim() + "' ";
                            sSQL += "AND appnum='" + _AppNum.Trim() + "' ";
                            sSQL += "AND batchcode='" + sNodeText + "' ";
                            sSQL += "AND setnum=" + sNodeText1 + " ";
                            sSQL += "AND indexstatus IN (" + pDocStatusIn + ") "; //'S','A'
                            sSQL += "AND doctype<>'" + staMain.stcProjCfg.SeparatorText + "' ";

                            if (pStation.Trim() != "")
                                sSQL += "AND indexstation='" + pStation.Trim().Replace("'", "") + "' ";

                            if (pUserId.Trim() != "")
                                sSQL += "AND indexuser='" + pUserId.Trim().Replace("'", "") + "' ";

                            if (pDateStart != "")
                                sSQL += "AND Convert(datetime,indexstart,103)>='" + pDateStart + "' "; //"yyyy-MM-dd HH:mm:ss"

                            if (pDateEnd != "")
                                sSQL += "AND Convert(datetime,indexend,103)<='" + pDateEnd + "' "; //"yyyy-MM-dd HH:mm:ss"

                            sSQL += "UNION ALL ";

                            sSQL += "SELECT Distinct collanum,doctype ";
                            sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuScan ";
                            sSQL += "WHERE scanproj='" + _ScanProj.Trim() + "' ";
                            sSQL += "AND appnum='" + _AppNum.Trim() + "' ";
                            sSQL += "AND batchcode='" + sNodeText + "' ";
                            sSQL += "AND setnum=" + sNodeText1 + " ";
                            sSQL += "AND scanstatus IN (" + pDocStatusIn + ") "; //'S','A'
                            sSQL += "AND doctype<>'" + staMain.stcProjCfg.SeparatorText + "' ";

                            if (pStation.Trim() != "")
                                sSQL += "AND scanstation='" + pStation.Trim().Replace("'", "") + "' ";

                            if (pUserId.Trim() != "")
                                sSQL += "AND scanuser='" + pUserId.Trim().Replace("'", "") + "' ";

                            if (pDateStart != "")
                                sSQL += "AND Convert(datetime,scanstart,103)>='" + pDateStart + "' "; //"yyyy-MM-dd HH:mm:ss"

                            if (pDateEnd != "")
                                sSQL += "AND Convert(datetime,scanend,103)<='" + pDateEnd + "' "; //"yyyy-MM-dd HH:mm:ss"

                            dsTVw = new DataSet();
                            dsTVw = mGlobal.returnRecordset("tTVw", sSQL);

                            if (dsTVw.Tables != null)
                            {
                                if (dsTVw.Tables.Count > 0)
                                {
                                    if (dsTVw.Tables[0].Rows.Count > 0)
                                    {
                                        int i = 0;
                                        TreeNode node; DataRow dr = null;
                                        while (i < dsTVw.Tables[0].Rows.Count)
                                        {
                                            dr = dsTVw.Tables[0].Rows[i];

                                            sTag = "Sep2|" + dr["doctype"].ToString() + "|" + sNodeText1 + "|" + dr["collanum"].ToString();
                                            node = new TreeNode(dr["doctype"].ToString());
                                            node.Tag = new String(sTag.ToCharArray());
                                            tvwSet.Nodes[nc].Nodes[nnc].Nodes.Add(node);
                                            tvwSet.Nodes[nc].Nodes[nnc].LastNode.Name = tvwSet.Nodes[nc].Nodes[nnc].LastNode.FullPath;

                                            i += 1;
                                        }
                                    }
                                }
                            }
                            dsTVw.Dispose();
                            dsTVw = null;

                            nnc += 1;
                        }

                        nc += 1;
                    }
                }

                //Load Document.
                nc = 0;
                nnc = 0; sNodeText1 = ""; int nnnc = 0; string sNodeText2 = "";
                sTag = "";
                int iNodesCnt = 0, iLev = 1;

                while (nc < tvwSet.Nodes.Count)
                {
                    nnc = 0;
                    sNodeText = tvwSet.Nodes[nc].Tag.ToString().Substring(4);

                    while (nnc < tvwSet.Nodes[nc].Nodes.Count)
                    {
                        nnnc = 0;
                        sNodeText1 = tvwSet.Nodes[nc].Nodes[nnc].Tag.ToString().Substring(5); //Setnum.
                        if (stcProjCfg.NoSeparator.Trim() == "2")
                        {
                            iNodesCnt = tvwSet.Nodes[nc].Nodes[nnc].Nodes.Count;
                            iLev = 2;
                        }
                        else
                        {
                            iNodesCnt = tvwSet.Nodes[nc].Nodes.Count;
                            iLev = 1;
                            nnnc = nnc;
                        }

                        while (nnnc < iNodesCnt)
                        {
                            //sNodeText2 = tvwSet.Nodes[nc].Nodes[nnc].Nodes[nnnc].Tag.ToString().Substring(5);
                            if (iLev == 2)
                                sNodeText2 = tvwSet.Nodes[nc].Nodes[nnc].Nodes[nnnc].Tag.ToString().Split('|').GetValue(1).ToString();

                            sSQL = "SELECT Distinct rowid,batchcode,setnum,collanum,doctype,indexstart,imageseq ";
                            sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuIndex ";
                            sSQL += "WHERE scanproj='" + _ScanProj.Trim() + "' ";
                            sSQL += "AND appnum='" + _AppNum.Trim() + "' ";
                            sSQL += "AND batchcode='" + sNodeText + "' ";
                            sSQL += "AND setnum=" + sNodeText1 + " ";
                            sSQL += "AND doctype<>'" + staMain.stcProjCfg.SeparatorText + "' ";
                            if (iLev == 2)
                                sSQL += "AND doctype='" + sNodeText2 + "' ";
                            sSQL += "AND indexstatus IN (" + pDocStatusIn + ") ";

                            if (pStation.Trim() != "")
                                sSQL += "AND indexstation='" + pStation.Trim().Replace("'", "") + "' ";

                            if (pUserId.Trim() != "")
                                sSQL += "AND indexuser='" + pUserId.Trim().Replace("'", "") + "' ";

                            //sSQL += "ORDER BY imageseq,indexstart ";

                            sSQL += "UNION ALL ";

                            sSQL += "SELECT Distinct rowid,batchcode,setnum,collanum,doctype,scanstart,imageseq ";
                            sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TDocuScan ";
                            sSQL += "WHERE scanproj='" + _ScanProj.Trim() + "' ";
                            sSQL += "AND appnum='" + _AppNum.Trim() + "' ";
                            sSQL += "AND batchcode='" + sNodeText + "' ";
                            sSQL += "AND setnum=" + sNodeText1 + " ";
                            sSQL += "AND doctype<>'" + staMain.stcProjCfg.SeparatorText + "' ";
                            if (iLev == 2)
                                sSQL += "AND doctype='" + sNodeText2 + "' ";
                            sSQL += "AND scanstatus IN (" + pDocStatusIn + ") ";

                            if (pStation.Trim() != "")
                                sSQL += "AND scanstation='" + pStation.Trim().Replace("'", "") + "' ";

                            if (pUserId.Trim() != "")
                                sSQL += "AND scanuser='" + pUserId.Trim().Replace("'", "") + "' ";

                            //sSQL += "ORDER BY imageseq,scanstart ";
                            sSQL += "ORDER BY 7,6 ";

                            dsTVw = new DataSet();
                            dsTVw = mGlobal.returnRecordset("tTVw", sSQL);

                            if (dsTVw.Tables != null)
                            {
                                if (dsTVw.Tables.Count > 0)
                                {
                                    if (dsTVw.Tables[0].Rows.Count > 0)
                                    {
                                        int i = 0;
                                        TreeNode node; DataRow dr = null;
                                        while (i < dsTVw.Tables[0].Rows.Count)
                                        {
                                            dr = dsTVw.Tables[0].Rows[i];

                                            sTag = "Doc|" + dr["batchcode"].ToString() + "|" + dr["doctype"].ToString() + "|" +
                                                dr["rowid"].ToString() + "|" + dr["setnum"].ToString() + "|" +
                                                dr["collanum"].ToString() + "|" + dr["imageseq"].ToString();
                                            node = new TreeNode("Page " + (i + 1));
                                            node.Tag = new String(sTag.ToCharArray());

                                            if (iLev == 2)
                                            {
                                                tvwSet.Nodes[nc].Nodes[nnc].Nodes[nnnc].Nodes.Add(node);
                                                tvwSet.Nodes[nc].Nodes[nnc].Nodes[nnnc].LastNode.Name = tvwSet.Nodes[nc].Nodes[nnc].Nodes[nnnc].LastNode.FullPath;
                                            }
                                            else
                                            {
                                                tvwSet.Nodes[nc].Nodes[nnc].Nodes.Add(node);
                                                tvwSet.Nodes[nc].Nodes[nnc].LastNode.Name = tvwSet.Nodes[nc].Nodes[nnc].LastNode.FullPath;

                                            }

                                            i += 1;
                                        }
                                    }
                                }
                            }
                            dsTVw.Dispose();
                            dsTVw = null;

                            nnnc += 1;
                            if (iLev == 1) break;
                        }

                        nnc += 1;
                    }

                    nc += 1;
                }

            }
            catch (Exception ex)
            {
            }
        }

        public static void enableScan(Form pMdiParent, bool pEnable)
        {
            try
            {
                if (pMdiParent != null)
                {
                    ToolStrip oToolStrip = (ToolStrip)pMdiParent.Controls["toolStrip"];
                    oToolStrip.Items[0].Enabled = pEnable;
                }
            }
            catch (Exception ex)
            {
            }
        }

        public static bool checkScanFormOpened()
        {
            bool bOpened = false;
            try
            {
                string[] sForms = new string[] { "frmScan1", "frmScanBox1", "frmReScan1", "frmReScanBox1" };
                string sForm = "";

                int i = 0;
                while (i < sForms.Length)
                {
                    sForm = sForms[i];

                    if (mGlobal.checkOpenedForms(sForm))
                    {
                        bOpened = true;
                        break;
                    }

                    i += 1;
                }
            }
            catch (Exception ex)
            {
            }
            return bOpened;
        }

        public static void sessionRestart()
        {
            try
            {
                MDIMain.iSessTimeout = 0;
                MDIMain.oSessTimer.Enabled = true;
                MDIMain.oSessTimer.Stop();
                MDIMain.oSessTimer.Start();
            }
            catch (Exception ex)
            {
            }
        }

        public static void sessionStart(int pTimeout = 0)
        {
            try
            {
                MDIMain.iSessTimeout = pTimeout;
                MDIMain.oSessTimer.Enabled = true;
                MDIMain.oSessTimer.Start();
            }
            catch (Exception ex)
            {
            }
        }

        public static void sessionStop(bool pEnable, int pTimeout = 0)
        {
            try
            {
                MDIMain.iSessTimeout = pTimeout;
                MDIMain.oSessTimer.Enabled = true;
                MDIMain.oSessTimer.Stop();
                MDIMain.oSessTimer.Enabled = pEnable;                
            }
            catch (Exception ex)
            {
            }
        }


    }
}
