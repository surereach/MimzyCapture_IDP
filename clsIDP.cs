using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Net;
using System.Data.Odbc;
using System.IO;

namespace SRDocScanIDP
{
    public partial class RequestIDP
    {
        public string id { get; set; }
        public string endPoint { get; set; }
        public string apiKey { get; set; }
        public string model { get; set; }
        public string classify { get; set; }
        public string baseString { get; set; }
    }

    public partial class ResponseResult
    {
        public int responseCode { get; set; }
        public string id { get; set; }
        public string status { get; set; }
        public string message { get; set; }
    }

    public class RequestBuild
    {
        public string id { get; set; }
        public string endPoint { get; set; }
        public string apiKey { get; set; }
        public string model { get; set; }
        public string filename { get; set; }
        public string baseString { get; set; }
    }

    public class ResponseBuild
    {
        public int responseCode { get; set; }
        public string id { get; set; }
        public string status { get; set; }
        public string message { get; set; }
        public string model { get; set; }
        public string blobUri { get; set; }
    }

    public static class SerializeObject
    {
        public static string ToJson(this ResponseResult self) => JsonConvert.SerializeObject(self, ConverterIDP.Settings);
        public static string ToJson(this RequestIDP self) => JsonConvert.SerializeObject(self, ConverterIDP.Settings);
        public static string ToJson(this RequestBuild self) => JsonConvert.SerializeObject(self, ConverterIDP.Settings);
    }

    public partial class ResultObj
    {
        public static ResponseResult FromJson(string json) => JsonConvert.DeserializeObject<ResponseResult>(json, ConverterIDP.Settings);        
    }

    public partial class BuildResultObj
    {
        public static ResponseBuild FromJson(string json) => JsonConvert.DeserializeObject<ResponseBuild>(json, ConverterIDP.Settings);
    }

    internal static class ConverterIDP
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

    public class clsIDP
    {
        public static _stcIDPMapping stcIDPMapping;
        public static _stcIDPFieldMap stcIDPFieldMap;

        public struct _stcIDPMapping
        {
            public string PrjCode;
            public string AppNum;
            public string DocDefId;
            public string EndPoint;
            public string IDPKey;
            public string ModelId;
            public string ClsModelId;
        }

        public struct _stcIDPFieldMap
        {
            public string PrjCode;
            public string AppNum;
            public string DocDefId;
            public string DocType;
            public List<string> FieldName;
            public List<string> FieldDispName;
        }

        public dynamic docClassify(string pBaseUrl, RequestIDP pIDP, ref string pMsg)
        {
            try
            {
                if (pIDP.endPoint.Trim() == string.Empty)
                    throw new Exception("End point is blank for classifying!");
                if (pIDP.apiKey.Trim() == string.Empty)
                    throw new Exception("End point key is blank for classifying!");

                pIDP.classify = "Yes";

                clsHttpHandler oWebAPI = new clsHttpHandler();
                oWebAPI.gCred = mGlobal.GetAppCfg("Crede") + ":" + mGlobal.GetAppCfg("Crede1");

                string sBody = SerializeObject.ToJson(pIDP);
                string sConType = "application/json; encoding='utf-8'";
                HttpWebResponse oResp = oWebAPI.HttpCall("Post", pBaseUrl, sConType, sBody, ref pMsg);

                if (oResp != null && pMsg.Trim() == "")
                {
                    Stream oRespStrm = oResp.GetResponseStream();
                    StreamReader oSReader = new StreamReader(oRespStrm);
                    string sResp = oSReader.ReadToEnd();

                    ResponseResult oResult = ResultObj.FromJson(sResp);
                    //ClassifyResult oCResult = ClassifyResult.FromJson(sResp);
                    if (oResult != null)
                    {
                        if (oResult.message != null)
                        {
                            if (oResult.message.Trim() != string.Empty)
                            {
                                pMsg = oResult.responseCode + "-" + oResult.message;
                            }
                        }
                        else //if (oCResult != null)
                        {
                            return sResp;
                        }
                    }
                    else //if (oCResult != null)
                    {
                        return sResp;
                    }

                    oResp.Close();
                    oRespStrm.Close();
                    oRespStrm.Dispose();
                    oSReader.Close();
                    oSReader.Dispose();
                }
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log(ex.Message + Environment.NewLine + ex.StackTrace.ToString());
                throw new Exception(ex.Message);
            }
            return false;
        }

        public dynamic docAnalysis(string pBaseUrl, RequestIDP pIDP, ref string pMsg)
        {
            try
            {
                if (pIDP.endPoint.Trim() == string.Empty)
                    throw new Exception("End point is blank in analysis!");
                if (pIDP.apiKey.Trim() == string.Empty)
                    throw new Exception("End point key is blank in analysis!");

                pIDP.classify = "No";

                clsHttpHandler oWebAPI = new clsHttpHandler();
                oWebAPI.gCred = mGlobal.GetAppCfg("Crede") + ":" + mGlobal.GetAppCfg("Crede1");

                string sBody = SerializeObject.ToJson(pIDP);
                string sConType = "application/json; encoding='utf-8'";
                HttpWebResponse oResp = oWebAPI.HttpCall("Post", pBaseUrl, sConType, sBody, ref pMsg);

                if (oResp != null && pMsg.Trim() == "")
                {
                    Stream oRespStrm = oResp.GetResponseStream();
                    StreamReader oSReader = new StreamReader(oRespStrm);
                    string sResp = oSReader.ReadToEnd();

                    ResponseResult oResult = ResultObj.FromJson(sResp);
                    //AnalysisResult oAResult = AnalysisResult.FromJson(sResp);
                    if (oResult != null)
                    {
                        if (oResult.message != null)
                        {
                            if (oResult.message.Trim() != string.Empty)
                            {
                                pMsg = oResult.responseCode + "-" + oResult.message;
                            }
                        }
                        else //if (oAResult != null)
                        {
                            return sResp;
                        }
                    }
                    else //if (oAResult != null)
                    {
                        return sResp;
                    }

                    oResp.Close();
                    oRespStrm.Close();
                    oRespStrm.Dispose();
                    oSReader.Close();
                    oSReader.Dispose();
                }
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log(ex.Message + Environment.NewLine + ex.StackTrace.ToString());
                throw new Exception(ex.Message);
            }
            return false;
        }

        public static bool IDPResultExistDB(string pProj, string pAppNum, string pBatchCode, string pSetNum, string pDocType, 
            int pPageNum, string pEndPoint, string pModelId)
        {
            try
            {
                string sSQL = "SELECT COUNT(IsNull(modelid,0)) FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchModResult ";
                sSQL += "WHERE scanproj='" + pProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";
                sSQL += "AND setnum='" + pSetNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND doctype='" + pDocType.Trim().Replace("'", "") + "' ";
                sSQL += "AND pagenum=" + pPageNum + " ";
                sSQL += "AND endpoint='" + pEndPoint.Trim().Replace("'", "") + "' ";
                sSQL += "AND modelid='" + pModelId.Trim().Replace("'", "") + "' ";

                mGlobal.LoadAppDBCfg();

                System.Data.DataRow oRow = mGlobal.objDB.ReturnSingleRow(sSQL);
                if (oRow != null)
                {
                    if (Convert.ToInt32(oRow[0].ToString()) > 0) return true; else return false;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log(ex.Message + Environment.NewLine + ex.StackTrace.ToString());
                throw new Exception(ex.Message);
            }
            return true;
        }

        public bool saveIDPResultDB(string pProj, string pAppNum, string pBatchCode, string pSetNum, string pDocType, string pEndPoint, 
            string pModelId, int pPageNum, int pTotPage, string pStation, string pSendBy, DateTime pSendStart, DateTime pSendEnd, 
            string pDocFile, string pIDPResult)
        {
            try
            {
                string sSQL = "INSERT INTO " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchModResult ";
                sSQL += "([scanproj]";
                sSQL += ",[appnum]";
                sSQL += ",[batchcode]";
                sSQL += ",[setnum]";
                sSQL += ",[doctype]";
                sSQL += ",[endpoint]";
                sSQL += ",[modelid]";
                sSQL += ",[pagenum]";
                sSQL += ",[totpagecnt]";
                sSQL += ",[stationcode]";
                sSQL += ",[sendby]";
                sSQL += ",[sendstart]";
                sSQL += ",[sendend]";
                sSQL += ",[docimage]";
                sSQL += ",[idpresult]";
                sSQL += ") VALUES (";
                sSQL += "'" + pProj.Trim().Replace("'", "") + "'";
                sSQL += ",'" + pAppNum.Trim().Replace("'", "") + "'";
                sSQL += ",'" + pBatchCode.Trim().Replace("'", "") + "'";
                sSQL += ",'" + pSetNum.Trim().Replace("'", "") + "'";
                sSQL += ",'" + pDocType.Trim().Replace("'", "") + "'";
                sSQL += ",'" + pEndPoint.Trim().Replace("'", "") + "'";
                sSQL += ",'" + pModelId.Trim().Replace("'", "") + "'";
                sSQL += "," + pPageNum + "";
                sSQL += "," + pTotPage + "";
                sSQL += ",'" + pStation.Trim().Replace("'", "") + "'";
                sSQL += ",'" + pSendBy.Trim().Replace("'", "") + "'";
                sSQL += ",'" + pSendStart.ToString("yyyy-MM-dd HH:mm:ss") + "'";
                sSQL += ",'" + pSendEnd.ToString("yyyy-MM-dd HH:mm:ss") + "'";
                sSQL += ",'" + pDocFile.Trim().Replace("'", "") + "'";
                sSQL += ",'" + pIDPResult.Trim().Replace("'", "''") + "')";

                mGlobal.LoadAppDBCfg();

                mGlobal.objDB.UpdateRows(sSQL);
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log(ex.Message + Environment.NewLine + ex.StackTrace.ToString());
                return false;
            }
            return true;
        }

        public static string getIDPResultDB(string pProj, string pAppNum, string pBatchCode, string pSetNum, string pDocType, int pPageNum,
            string pEndPoint, string pModelId)
        {
            try
            {
                string sSQL = "SELECT TOP 1 IsNull(idpresult,'') FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchModResult ";
                sSQL += "WHERE scanproj='" + pProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";
                sSQL += "AND setnum='" + pSetNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND doctype='" + pDocType.Trim().Replace("'", "") + "' ";
                sSQL += "AND pagenum=" + pPageNum + " ";
                sSQL += "AND endpoint='" + pEndPoint.Trim().Replace("'", "") + "' ";
                sSQL += "AND modelid='" + pModelId.Trim().Replace("'", "") + "' ";

                mGlobal.LoadAppDBCfg();

                System.Data.DataRow oRow = mGlobal.objDB.ReturnSingleRow(sSQL);
                if (oRow != null)
                {
                    return oRow[0].ToString();
                }
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log(ex.Message + Environment.NewLine + ex.StackTrace.ToString());
                throw new Exception(ex.Message);
            }
            return "";
        }

        public static void loadIDPFieldMappingDB(string pProj, string pAppNum, string pDocDefId, string pDocType = "")
        {
            try
            {
                stcIDPFieldMap = new _stcIDPFieldMap();
                stcIDPFieldMap.PrjCode = pProj.Trim();
                stcIDPFieldMap.AppNum = pAppNum.Trim();
                stcIDPFieldMap.DocDefId = pDocDefId.Trim();

                string sSQL = "SELECT fldname,idpfldname,idpdoctype ";
                sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TIDPMapFields ";
                sSQL += "WHERE scanpjcode='" + pProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND sysappnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND docdefid='" + pDocDefId.Trim().Replace("'", "") + "' ";
                if (pDocType.Trim() != "")
                    sSQL += "AND idpdoctype='" + pDocType.Trim().Replace("'", "") + "' ";

                mGlobal.LoadAppDBCfg();

                System.Data.DataRowCollection oRows = mGlobal.objDB.ReturnRows(sSQL);
                if (oRows != null)
                {
                    stcIDPFieldMap.FieldName = new List<string>(oRows.Count);
                    stcIDPFieldMap.FieldDispName = new List<string>(oRows.Count);
                    int i = 0;
                    while (i < oRows.Count)
                    {
                        if (i == 0)
                            stcIDPFieldMap.DocType = oRows[i]["idpdoctype"].ToString().Trim();

                        stcIDPFieldMap.FieldName.Add(oRows[i]["idpfldname"].ToString().Trim());
                        stcIDPFieldMap.FieldDispName.Add(oRows[i]["fldname"].ToString().Trim());

                        i += 1;
                    }
                }
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log(ex.Message + Environment.NewLine + ex.StackTrace.ToString());
                throw new Exception(ex.Message);
            }
        }

        public static void loadIDPMappingDB(string pProj, string pAppNum, string pDocDefId)
        {
            try
            {
                stcIDPMapping = new _stcIDPMapping();
                stcIDPMapping.PrjCode = pProj.Trim();
                stcIDPMapping.AppNum = pAppNum.Trim();
                stcIDPMapping.DocDefId = pDocDefId.Trim();

                string sSQL = "SELECT TOP 1 endpoint,idpkey,modelid,classmodelid ";
                sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TIDPMapping ";
                sSQL += "WHERE scanpjcode='" + pProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND sysappnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND docdefid='" + pDocDefId.Trim().Replace("'", "") + "' ";

                mGlobal.LoadAppDBCfg();

                System.Data.DataRow oRow = mGlobal.objDB.ReturnSingleRow(sSQL);
                if (oRow != null)
                {
                    stcIDPMapping.EndPoint = oRow["endpoint"].ToString().Trim();
                    stcIDPMapping.IDPKey = oRow["idpkey"].ToString().Trim();
                    stcIDPMapping.ModelId = oRow["modelid"].ToString().Trim();
                    stcIDPMapping.ClsModelId = oRow["classmodelid"].ToString().Trim();
                }
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log(ex.Message + Environment.NewLine + ex.StackTrace.ToString());
                throw new Exception(ex.Message);
            }
        }

        public static bool IDPTableValueExistDB(string pProj, string pAppNum, string pBatchCode, string pSetNum, string pDocType,
            int pPageId, string pTableNum)
        {
            try
            {
                string sSQL = "SELECT COUNT(IsNull(tableno,0)) ";
                sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TIDPTableValues ";
                sSQL += "WHERE scanproj='" + pProj.Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + pAppNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND batchcode='" + pBatchCode.Trim().Replace("'", "") + "' ";
                sSQL += "AND setnum='" + pSetNum.Trim().Replace("'", "") + "' ";
                sSQL += "AND doctype='" + pDocType.Trim().Replace("'", "") + "' ";
                sSQL += "AND pageid=" + pPageId + " ";
                sSQL += "AND tableno='" + pTableNum + "' ";

                mGlobal.LoadAppDBCfg();

                System.Data.DataRow oRow = mGlobal.objDB.ReturnSingleRow(sSQL);
                if (oRow != null)
                {
                    if (Convert.ToInt32(oRow[0].ToString()) > 0) return true; else return false;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log(ex.Message + Environment.NewLine + ex.StackTrace.ToString());
                throw new Exception(ex.Message);
            }
            return true;
        }



    }
}
