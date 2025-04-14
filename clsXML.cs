using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Data;

namespace SRDocScanIDP
{
    public class clsXML
    {
        public string _sCurrProj;
        public string _sAppNum;
        public string _sDocDefId;

        public staMain._stcExportInfo _stcXmlInfo;

        private string sSetNumFmt;

        public clsXML()
        {
            _sCurrProj = "";
            _sAppNum = "";
            _sDocDefId = "";

            _stcXmlInfo = new staMain._stcExportInfo();

            if (staMain.stcProjCfg.BatchType.Trim().ToLower() == "set")
                sSetNumFmt = "000000";
            else
                sSetNumFmt = "000";
        }

        public bool generateXML(string pExpFileExt)
        {
            bool bRet = true;
            string sBatchCode = "";
            string sDocType = "";

            string sFile = "";
            int iTotSet = 1;
            string[] sFiles = null;

            string sDir = mGlobal.addDirSep(staMain.stcProjCfg.ExpDir);
            if (Directory.Exists(sDir))
                Directory.CreateDirectory(sDir);

            string XmlFilename = sDir + DateTime.Now.ToString("yyyyMMddhhmmss") + ".xml";

            try
            {
                // Create an XmlWriterSettings object with the correct options.
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.IndentChars = "    "; //  "\t";
                settings.OmitXmlDeclaration = false;
                settings.Encoding = System.Text.Encoding.UTF8;

                using (System.Xml.XmlWriter writer = System.Xml.XmlWriter.Create(XmlFilename, settings))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("sets");

                    sFiles = _stcXmlInfo.ExpFiles.ToArray();
                    string sNextFile = "";

                    DataRowCollection hdrs = frmExport.getExportIndexHeaderDB(_sCurrProj, _sAppNum, _sDocDefId);
                    int iIdx = 0; int iSet = 0;
                    int iF = 0; int iFd = 0; int iTotFd = 0; string sSetDocType = "", sSetNum = ""; int iFldId = 0; string sValue = "";

                    for (int i = 0; i < sFiles.Length; ++i)
                    {
                        sFile = mGlobal.getFileNameFromPath(sFiles[i].ToString());

                        if (sFile.Trim() != "")
                        {
                            if (staMain.stcProjCfg.BatchType.Trim().ToLower() == "box") //Box type.
                            {
                                if (sFile.Split('_').Length > 2)
                                    sBatchCode = sFile.Split('_').GetValue(0).ToString() + "-" + sFile.Split('_').GetValue(1).ToString() + "-" + sFile.Split('_').GetValue(2).ToString(); //e.g. 001_20230316000002_2.
                                else
                                    sBatchCode = sFile.Split('_').GetValue(0).ToString() + "-" + sFile.Split('_').GetValue(1).ToString();
                            }
                            else
                                sBatchCode = sFile.Split('_').GetValue(0).ToString() + "-" + sFile.Split('_').GetValue(1).ToString();

                            sDocType = sFile.Split('_').GetValue(2).ToString().ToUpper().Replace("." + pExpFileExt, "");

                            if (i + 1 < sFiles.Length)
                            {
                                sNextFile = mGlobal.getFileNameFromPath(sFiles[i + 1].ToString());

                                if (sBatchCode.Trim() == sNextFile.Split('_').GetValue(0).ToString().Trim() + "-" + sNextFile.Split('_').GetValue(1).ToString())
                                {
                                    iTotSet += 1;
                                }
                                else
                                {
                                    iSet += 1;
                                    writer.WriteStartElement("set");
                                    writer.WriteAttributeString("form_count", iTotSet.ToString());
                                    writer.WriteAttributeString("GUID", "");
                                    writer.WriteAttributeString("set_name", sBatchCode);
                                    writer.WriteAttributeString("id", (iSet).ToString());

                                    iF = 0; iFd = 0; iTotFd = 0; sSetDocType = ""; sSetNum = ""; iFldId = 0; sValue = "";
                                    while (iF < iTotSet)
                                    {
                                        if (hdrs != null) iTotFd = hdrs.Count;

                                        writer.WriteStartElement("forms");
                                        writer.WriteAttributeString("GUID", "");
                                        writer.WriteAttributeString("id", (iF + 1).ToString());
                                        writer.WriteAttributeString("field_count", iTotFd.ToString());
                                        writer.WriteAttributeString("back_image_name", "");
                                        writer.WriteAttributeString("front_image_name", sFiles[iIdx].ToString());
                                        writer.WriteAttributeString("form_name", "");
                                        writer.WriteAttributeString("total_page", _stcXmlInfo.TotPages[iIdx].ToString());

                                        sSetNum = _stcXmlInfo.Setnum[iIdx].ToString();
                                        sSetDocType = mGlobal.getFileNameFromPath(sFiles[iIdx].ToString()).Split('_').GetValue(3).ToString().ToUpper().Replace("." + pExpFileExt.ToUpper(), "");                                      

                                        iFd = 0;
                                        while (iFd < iTotFd)
                                        {
                                            writer.WriteStartElement("field");

                                            iFldId = Convert.ToInt32(hdrs[iFd]["rowid"].ToString());
                                            if (staMain.stcProjCfg.NoSeparator.Trim() == "0" && staMain.stcProjCfg.ExpMerge.Trim() == "Y") //Set number = Page image sequence in TIndexFieldValue table.
                                            {
                                                sValue = frmExport.getKeyIndexValueDB(_sCurrProj, _sAppNum, 1.ToString(sSetNumFmt), sBatchCode.Replace("-", "_"), _sDocDefId, iFldId); //Take the first set index value.
                                            }
                                            else
                                            {
                                                sValue = frmExport.getKeyIndexValueDB(_sCurrProj, _sAppNum, sSetNum, sBatchCode.Replace("-", "_"), _sDocDefId, iFldId);
                                            }

                                            writer.WriteAttributeString("name", hdrs[iFd]["fldname"].ToString().Trim().Replace(" ", "_").Replace("#", "_Number").Replace(".",""));
                                            writer.WriteString(sValue);

                                            //if (iFd == 0)
                                            //{
                                            //    writer.WriteAttributeString("name", "AA_NUMBER");
                                            //    writer.WriteString(sCAppNum);
                                            //}
                                            //else if (iFd == 1)
                                            //{
                                            //    writer.WriteAttributeString("name", "DOC_TYPE");
                                            //    writer.WriteString(sSetDocType);
                                            //}
                                            //else
                                            //{
                                            //    writer.WriteAttributeString("name", "Holder_Name");
                                            //    writer.WriteString(sCHldName);
                                            //}

                                            writer.WriteEndElement();

                                            iFd += 1;
                                        }

                                        writer.WriteEndElement();

                                        iIdx += 1;
                                        iF += 1;
                                    }

                                    writer.WriteEndElement();
                                    iTotSet = 1;
                                }
                            }
                            else //Last set element.
                            {
                                iSet += 1;
                                writer.WriteStartElement("set");
                                writer.WriteAttributeString("form_count", iTotSet.ToString());
                                writer.WriteAttributeString("GUID", "");
                                writer.WriteAttributeString("set_name", sBatchCode);
                                writer.WriteAttributeString("id", (iSet).ToString());

                                iF = 0; iFd = 0; iTotFd = 3; sSetDocType = ""; sSetNum = ""; iFldId = 0; sValue = "";
                                while (iF < iTotSet)
                                {
                                    if (hdrs != null) iTotFd = hdrs.Count;

                                    writer.WriteStartElement("forms");
                                    writer.WriteAttributeString("GUID", "");
                                    writer.WriteAttributeString("id", (iF + 1).ToString());
                                    writer.WriteAttributeString("field_count", iTotFd.ToString());
                                    writer.WriteAttributeString("back_image_name", "");
                                    writer.WriteAttributeString("front_image_name", sFiles[iIdx].ToString());
                                    writer.WriteAttributeString("form_name", "");
                                    writer.WriteAttributeString("total_page", _stcXmlInfo.TotPages[iIdx].ToString());

                                    sSetNum = _stcXmlInfo.Setnum[iIdx].ToString();
                                    sSetDocType = mGlobal.getFileNameFromPath(sFiles[iIdx].ToString()).Split('_').GetValue(3).ToString().ToUpper().Replace("." + pExpFileExt.ToUpper(), "");
                                                                
                                    iFd = 0;
                                    while (iFd < iTotFd)
                                    {
                                        writer.WriteStartElement("field");

                                        iFldId = Convert.ToInt32(hdrs[iFd]["rowid"].ToString());

                                        if (staMain.stcProjCfg.NoSeparator.Trim() == "0" && staMain.stcProjCfg.ExpMerge.Trim() == "Y") //Set number = Page image sequence in TIndexFieldValue table.
                                        {
                                            sValue = frmExport.getKeyIndexValueDB(_sCurrProj, _sAppNum, 1.ToString(sSetNumFmt), sBatchCode.Replace("-", "_"), _sDocDefId, iFldId); //Take the first set index value.
                                        }
                                        else
                                        {
                                            sValue = frmExport.getKeyIndexValueDB(_sCurrProj, _sAppNum, sSetNum, sBatchCode.Replace("-", "_"), _sDocDefId, iFldId);
                                        }

                                        writer.WriteAttributeString("name", hdrs[iFd]["fldname"].ToString().Trim().Replace(" ", "_").Replace("#", "_Number"));
                                        writer.WriteString(sValue);

                                        //if (iFd == 0)
                                        //{
                                        //    writer.WriteAttributeString("name", "AA_NUMBER");
                                        //    writer.WriteString(sCAppNum);
                                        //}
                                        //else if (iFd == 1)
                                        //{
                                        //    writer.WriteAttributeString("name", "DOC_TYPE");
                                        //    writer.WriteString(sSetDocType);
                                        //}
                                        //else
                                        //{
                                        //    writer.WriteAttributeString("name", "Holder_Name");
                                        //    writer.WriteString(sCHldName);
                                        //}                                       

                                        writer.WriteEndElement();

                                        iFd += 1;
                                    }

                                    writer.WriteEndElement();

                                    iIdx += 1;
                                    iF += 1;
                                }

                                writer.WriteEndElement();
                                iTotSet = 1;
                            }
                        }
                        
                    } //End for.

                    writer.WriteEndElement();

                    writer.Flush();
                    writer.Close();
                } // End Using writer                 
                
            }
            catch (Exception ex)
            {
                bRet = false;
                mGlobal.Write2Log(ex.StackTrace.ToString());
                throw new Exception(ex.Message);
            }           
                        
            return bRet;
        }
    }
}
