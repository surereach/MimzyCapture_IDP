using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.IO;
using System.Globalization;
using SixLabors.ImageSharp.PixelFormats;
using Syncfusion.Pdf.Graphics;

namespace SRDocScanIDP
{
    extern alias clsUtilComn;
    using static System.Windows.Forms.VisualStyles.VisualStyleElement;

    public static class mGlobal
    {
        public static string strModule = "Document Scan GEN IDP";
        public static string strCurrLoginID;
        public static string strCurrUserName;
        public static List<string> strCurrUserRoles;
        public static string strClearTextCred;
        public static bool blnIsLogin;

        public static bool blnCancel;

        //settings in config
        public static string strLogFolder;

        public static string strDBMode;
        public static string strDSNName;
        public static string strDBServer;
        public static string strDBName;
        public static string strDBLogin;
        public static string strDBPwd;
        public static string strDBDriver;

        public static string strErrDesc;

        //public static clsUtilComn.clsDatabase objDB = new clsUtilComn.clsDatabase();
        public static clsUtilComn.IDatabase objDB = new clsUtilComn.clsDatabase();
        public static string strConnectionString = "Driver={" + strDBDriver + "}" +
                                        ";Server=" + strDBServer +
                                        ";Database=" + strDBName +
                                        ";UID=" + strDBLogin +
                                        ";Pwd=" + strDBPwd + ";"; //generates database connection string.

        public static string strConnectionString1 = "Server=" + strDBServer +
                                        ";Database=" + strDBName +
                                        ";User ID=" + strDBLogin +
                                        ";Password=" + strDBPwd + ";Trusted_Connection=True;";

        public static clsUtilComn.clsCryptography.Symmetric objSec = new clsUtilComn.clsCryptography.Symmetric(clsUtilComn.clsCryptography.Symmetric.Provider.Rijndael);

        public static clsUtilComn.clsCryptography.Data objSecData = new clsUtilComn.clsCryptography.Data("RS65432$Two");
        public static clsUtilComn.clsCryptography.Data objSecDBData = new clsUtilComn.clsCryptography.Data("RSDBA2*/8b");
        public static clsUtilComn.clsCryptography.Data objSecAuth = new clsUtilComn.clsCryptography.Data("RSur36532$Two"); 

        public static void Write2Log(string sLog, string strErrCode = "")
        {
            try
            {
                clsUtilComn.clsLog objLog = new clsUtilComn.clsLog();
                objLog.sLogFolderPath = GetAppCfg("LogPath");
                //objLog.Log(" " + strErrCode + " " + sLog);
                Task oTask = Task.Run(async () => objLog.Log(" " + strErrCode + " " + sLog));
                oTask.Wait();

                if (oTask.IsCompleted) oTask.Dispose();

            } catch (Exception ex){
                Console.WriteLine("Write log error. "+ex.Message);
            }
        }

        public static string GetAppCfg(string strKey){ //make sure it have right permission to read config file.
            string strCfg = "";
            try{
                clsUtilComn.clsConfig objCfg = new clsUtilComn.clsConfig();

                objCfg.sConfigFilePath = System.Windows.Forms.Application.StartupPath + @"\DocScan.config";
                
                strCfg = objCfg.GetAppKeyValue(strKey, ""); //Get from app.config key value
            }
            catch (Exception ex){
                Write2Log("Get configuration..." + ex.Message + ": " + ex.StackTrace);
            }
            return strCfg;
        }

        public static bool writeAppCfg(string strKey, string strValue)
        {
            bool bOk = true;
            try
            {
                clsUtilComn.clsConfig objCfg = new clsUtilComn.clsConfig();

                objCfg.sConfigFilePath = System.Windows.Forms.Application.StartupPath + @"\DocScan.config";

                objCfg.SetAppKeyValue(strKey, strValue);
            }
            catch (Exception ex)
            {
                bOk = false;
                Write2Log("Write configuration..." + ex.Message + ": " + ex.StackTrace);
            }
            return bOk;
        }

        public static bool LoadAppCfg(){
            bool LoadAppCfg = true;

            try{
                strLogFolder = GetAppCfg("LogPath").ToString();
                
            } catch (Exception ex){
                LoadAppCfg = false;
                Write2Log(ex.Message + ": " + ex.StackTrace);
            }

            return LoadAppCfg;
        }

        public static bool LoadAppDBCfg(){
            bool LoadAppDBCfg = true;

            try{
                strDBMode = GetAppCfg("DBMode").ToString();
                strDSNName = GetAppCfg("DSNName").ToString();
                strDBServer = secDecrypt(GetAppCfg("DBHost").ToString(), objSecDBData);
                strDBName = secDecrypt(GetAppCfg("DBName").ToString(), objSecDBData);
                strDBLogin = secDecrypt(GetAppCfg("DBUid").ToString(), objSecDBData); //GetAppCfg("DBUid").ToString(); //secDecrypt(GetAppCfg("DBUid").ToString(), objSecDBData);
                strDBPwd = secDecrypt(GetAppCfg("DBPwd").ToString(), objSecDBData); //GetAppCfg("DBPwd").ToString(); //secDecrypt(GetAppCfg("DBPwd").ToString(), objSecDBData);
                strDBDriver = GetAppCfg("DBDriver").ToString();

                mGlobal.objDB.sDBMode = (IDatabase.ODBC_MODE)Convert.ToInt32(strDBMode);
                mGlobal.objDB.sDSNName = mGlobal.strDSNName;
                mGlobal.objDB.sDBHost = mGlobal.strDBServer;
                mGlobal.objDB.sDBName = mGlobal.strDBName;
                mGlobal.objDB.sDBUid = mGlobal.strDBLogin;
                mGlobal.objDB.sDBPwd = mGlobal.strDBPwd;
                mGlobal.objDB.sDBDriver = mGlobal.strDBDriver;

                if (mGlobal.objDB.sDBMode == clsUtilComn.IDatabase.ODBC_MODE.PROVIDER_MODE)
                {
                    mGlobal.objDB = new clsUtilComn.clsDatabaseOle();

                    mGlobal.objDB.sDSNName = mGlobal.strDSNName;
                    mGlobal.objDB.sDBHost = mGlobal.strDBServer;
                    mGlobal.objDB.sDBName = mGlobal.strDBName;
                    mGlobal.objDB.sDBUid = mGlobal.strDBLogin;
                    mGlobal.objDB.sDBPwd = mGlobal.strDBPwd;
                    mGlobal.objDB.sDBDriver = mGlobal.strDBDriver;

                    strConnectionString = "Provider=" + strDBDriver +
                                          ";Server=" + strDBServer +
                                          ";Database=" + strDBName +
                                          ";UID=" + strDBLogin +
                                          ";Pwd=" + strDBPwd + ";";
                }
                else
                    strConnectionString = "Driver={" + strDBDriver + "}" + 
                                          ";Server=" + strDBServer +
                                          ";Database=" + strDBName +
                                          ";UID=" + strDBLogin +
                                          ";Pwd=" + strDBPwd + ";";

                strConnectionString1 = "Server=" + strDBServer +
                                       ";Database=" + strDBName +
                                       ";User ID=" + strDBLogin +
                                       ";Password=" + strDBPwd + ";Trusted_Connection=False;";

                if (mGlobal.objDB.sDBMode == clsUtilComn.IDatabase.ODBC_MODE.PROVIDER_MODE)
                {
                    mGlobal.objDB.sDBMode = clsUtilComn.IDatabase.ODBC_MODE.PROVIDER_MODE;
                    mGlobal.objDB.sConnectionString = strConnectionString; //Default otherwise set in individual function.
                }
                else if(mGlobal.objDB.sDBMode == clsUtilComn.IDatabase.ODBC_MODE.SQLDRIVER_MODE)
                {
                    mGlobal.objDB.sDBMode = clsUtilComn.IDatabase.ODBC_MODE.SQLDRIVER_MODE;
                    mGlobal.objDB.sConnectionString = strConnectionString; //Default otherwise set in individual function.
                }
                else
                {
                    mGlobal.objDB.sDBMode = clsUtilComn.IDatabase.ODBC_MODE.DSN_MODE;
                    mGlobal.objDB.sConnectionString = "DSN=" + strDSNName +
                                  ";Uid=" + strDBLogin +
                                  ";Pwd=" + strDBPwd + ";"; //Default otherwise set in individual function.
                }

		    }catch (Exception ex){
                LoadAppDBCfg = false;
                Write2Log(ex.Message + ": " + ex.StackTrace);
            }

            return LoadAppDBCfg;
        }

        public static string secEncrypt(string strText, clsUtilComn.clsCryptography.Data objSecData) 
        {
            string strHexText = "";
            clsUtilComn.clsCryptography.Data objCrypData = new clsUtilComn.clsCryptography.Data(strText);
            try{
                strHexText = objSec.Encrypt(objCrypData, objSecData).ToHex();
            }catch (Exception ex){
                Write2Log("Encrypt error " + ex.Message);
            }
            finally
            {
                objCrypData = null;
            }
            return strHexText;
        }
        
        public static string secDecrypt(string strHexText, clsUtilComn.clsCryptography.Data objSecData){
            string strText = "";
            clsUtilComn.clsCryptography.Data objCrypData;
            try
            {
                objCrypData = new clsUtilComn.clsCryptography.Data(new clsUtilComn.clsCryptography().FromHex2String(strHexText));
                strText = objSec.Decrypt(objCrypData, objSecData).ToString();
            } catch (Exception ex){
                Write2Log("Security error " + ex.Message + ": " + ex.StackTrace.ToString());
                objCrypData = null;
            }
            return strText;
        }

        public static string customMessage(string statusCode, string errorCode, string message)
        {
            string retMessage = "";
            try
            {
                retMessage = statusCode + "-" + mGlobal.strModule + "-" + errorCode + "-" + message;
            }
            catch (Exception ex)
            {
            }
            return retMessage;
        }

        public static bool logAudit(string strEventName, string strEventAction, string strCreatedBy, string strRemarks) 
        { 
            bool blnAudit = true;
            try
            {
                mGlobal.objDB.sConnectionString = ""; //Init  
                mGlobal.LoadAppDBCfg();

                if (mGlobal.objDB == null)
                {
                    mGlobal.objDB = new clsUtilComn.clsDatabase();
                }

                string strSQL = "INSERT INTO "+ mGlobal.strDBName.Replace("'", "") + ".[dbo].[AUDIT_LOG] ";
                strSQL = strSQL + "([EVENT_NAME] ";
                strSQL = strSQL + ",[EVENT_ACTION] ";
                strSQL = strSQL + ",[CREATED_BY] ";
                strSQL = strSQL + ",[REMARKS]) ";
                strSQL = strSQL + "VALUES ";
                strSQL = strSQL + "('" + strEventName.Replace("'", "''") + "' ";
                strSQL = strSQL + ",'" + strEventAction.Replace("'", "''") + "' ";
                strSQL = strSQL + ",'" + strCreatedBy.Replace("'", "''") + "' ";
                strSQL = strSQL + ",'" + strRemarks.Replace("'", "''") + "') ";

                mGlobal.objDB.UpdateRows(strSQL);
            }
            catch (Exception ex)
            {
                Write2Log("Error in audit logging.." + ex.Message);
            }
            return blnAudit;
        }
        
        public static System.Data.DataSet returnRecordset(string strTableName, string strSQL)
        {
           
            System.Data.DataSet returnRecordset = null;

            try{
                System.Data.DataSet rsData;
                System.Collections.Hashtable htSQL = new System.Collections.Hashtable();
                htSQL.Add(strTableName, strSQL);

                mGlobal.LoadAppDBCfg();
                objDB.objLog.sLogFolderPath = mGlobal.GetAppCfg("LogPath");

                rsData = objDB.ReturnMultiTables(htSQL);

                if (rsData != null)  //if record has been found.
                    returnRecordset = rsData;
                
                if (rsData != null) rsData.Dispose();
                rsData = null;

            }catch(Exception ex){
                Write2Log("Error in returnRecordset "+ ex.Message);
            }
            return returnRecordset;
        }

        public static bool isAnyInteger(string str)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(str))
                {
                    return false;
                }

                foreach (var chr in str)
                {
                    if (Char.IsNumber(chr))
                        return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static bool IsInteger(string str)
        {
            Regex regex = new Regex(@"^[0-9]+$");
            try
            {
                if (String.IsNullOrWhiteSpace(str))
                {
                    return false;
                }
                if (!regex.IsMatch(str))
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static bool IsDecimal(string str)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(str))
                {
                    return false;
                }
                else if (str.IndexOf('.') == -1)
                {
                    return false;
                }
                decimal decValue;
                if (!decimal.TryParse(str, out decValue))
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static bool isSpecialChars(string str)
        {
            Regex regex = new Regex(@"[~`!@#$%^&*()+=|\{}':;.,<>/?[\]""_-]");
            try
            {
                if (String.IsNullOrWhiteSpace(str))
                {
                    return true;
                }
                if (!regex.IsMatch(str))
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static bool isDate(string strDate, string[] dateFormats)
        {
            bool status = false;
            DateTime dOut;

            if (strDate.Trim() == string.Empty)
                return false;
            
            if (DateTime.TryParseExact(strDate, dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dOut))
            {
                status = true;
            }
            else
            {
                status = false;
            }

            return status;
        }

        public static void enableMenuItem(System.Windows.Forms.Form objForm, bool blnEnable, string strName)
        {
            Control[] controls = objForm.Controls.Find("menuStrip", true);
            foreach (Control ctrl in controls)
            {
                if (ctrl.Name == "menuStrip")
                {
                    MenuStrip strip = ctrl as MenuStrip;

                    foreach (ToolStripItem objMS in strip.Items)
                    {
                        if (objMS.Name.Trim() == strName.Trim())
                        {
                            objMS.Enabled = blnEnable;
                        }                        
                    }
                }
            }
        }

        public static void enableToolMenuItem(ToolStripMenuItem menu, bool enable, string name)
        {
            foreach (var subItem in menu.DropDownItems)
            {
                var item = subItem as ToolStripDropDownItem;

                if (item == null) continue;

                if (Equals(item.Name, name))
                {
                    item.Enabled = enable;
                    break;
                }                    
            }
        }

        public static void visibleToolMenuItem(ToolStripMenuItem menu, bool visible, string name)
        {
            foreach (var subItem in menu.DropDownItems)
            {
                var item = subItem as ToolStripDropDownItem;

                if (item == null) continue;

                if (Equals(item.Name, name))
                {
                    item.Visible = visible;
                    break;
                }                    
            }
        }

        public static string removeQoteEmbededCRLF(string text, bool pIsQuoted)
        {
            bool isQuoted = !pIsQuoted;
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                //look for '"' if found toggle isquoted
                if (c == '"') isQuoted = !isQuoted;

                //if CR found inside quoted text
                if (c == '\r' && isQuoted && i < text.Length - 1)
                {
                    //See if LF is next char
                    if (text[i + 1] == '\n')
                    {
                        //CRLF found, replace with space
                        c = ' ';
                        //step over LF
                        i += 1;
                    }
                }
                sb.Append(c);
            }
            return sb.ToString();
        }

        public static bool checkAnyOpenedForms()
        {
            FormCollection fc = Application.OpenForms;

            if (fc.Count > 1) return true; //But not including form itself.

            return false;
        }

        public static bool checkOpenedForms(string name)
        {
            FormCollection fc = Application.OpenForms;
            
            foreach (Form frm in fc)
            {
                if (frm.Name == name)
                {
                    return true;
                }
            }
            return false;
        }

        public static Form getOpenedForms(string name)
        {
            FormCollection fc = Application.OpenForms;

            foreach (Form frm in fc)
            {
                if (frm.Name == name)
                {
                    return frm;
                }
            }
            return null;
        }

        public static string getSqlConnection()
        {
            System.Data.SqlClient.SqlConnectionStringBuilder builder = new System.Data.SqlClient.SqlConnectionStringBuilder();
            try
            {
                LoadAppDBCfg();

                builder["Server"] = mGlobal.strDBServer;
                builder["Database"] = mGlobal.strDBName;
                builder["User ID"] = mGlobal.strDBLogin;
                builder["Password"] = mGlobal.strDBPwd;
                builder["Connection Timeout"] = "30";
            }
            catch (Exception ex)
            {
                return "";
            }
            return builder.ConnectionString;
        }

        public static string addDirSep(string pFullPath)
        {
            string strRtn = pFullPath.Trim();
            if (!pFullPath.EndsWith(@"\")) 
                strRtn = pFullPath + @"\";

            return strRtn;
        }
        public static bool verifyFolderName(string folderName)
        {
            try
            {
                if (folderName.LastIndexOfAny(System.IO.Path.GetInvalidPathChars()) == -1)
                    return true;
            }
            catch (Exception ex)
            {
            }
            return false;
        }

        public static bool verifyFileName(string fileName)
        {
            try
            {
                if (fileName.LastIndexOfAny(System.IO.Path.GetInvalidFileNameChars()) == -1)
                    return true;
            }
            catch (Exception ex)
            {
            }
            MessageBox.Show("The file name contains invalid chars!", "Save Image To File", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return false;
        }

        public static string getFileNameFromPath(string filenamePath)
        {
            return filenamePath.Substring(filenamePath.LastIndexOf(@"\") + 1);
        }

        public static string getDirectoriesFromPath(string filenamePath)
        {
            return filenamePath.Substring(0, filenamePath.LastIndexOf(@"\") + 1);
        }

        public static bool writeText(string sText, string pFilePath)
        {
            bool ret = true;
            try
            {
                using (TextWriter oWriter = File.CreateText(pFilePath))
                {
                    oWriter.Write(sText);

                    oWriter.Close();
                    oWriter.Dispose();
                }                
            }
            catch (Exception ex)
            {
                ret = false;
                throw new Exception(ex.Message);
            }
            return ret;
        }

        public static string replaceValidChars(string DirFileName, string Replace)
        {
            try
            {
                if (DirFileName.LastIndexOfAny(System.IO.Path.GetInvalidFileNameChars()) == -1)
                    return DirFileName;
                else
                {
                    int i = 0;
                    while (i < System.IO.Path.GetInvalidFileNameChars().Length)
                    {
                        DirFileName = DirFileName.Replace(System.IO.Path.GetInvalidFileNameChars()[i].ToString(), Replace);
                        i += 1;
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return DirFileName;
        }

        public static float convert2Points(Control pCtrl, float pPixels, string pDpiX)
        {
            float fPoints = 0;
            float fDPI = 0;
            try
            {
                System.Drawing.Graphics g = pCtrl.CreateGraphics();

                if (pDpiX.ToUpper() == "Y")
                    fDPI = g.DpiY;
                else
                    fDPI = g.DpiX;

                fPoints = pPixels * 72 / fDPI;

                g.Dispose();
            }
            catch (Exception ex)
            {
            }
            return fPoints;
        }

        public static int convertText2Pixels(Control pCtrl, string sText)
        {
            int iPixels = 0;
            try
            {
                using (Graphics G = pCtrl.CreateGraphics())
                {
                    iPixels = (int)(sText.Length * G.MeasureString("x", pCtrl.Font).Width);
                }
            }
            catch (Exception ex)
            {
            }
            return iPixels;
        }

        public static string sanitizeValue(string str)
        {
            return Regex.Replace(str, @"[\x00'""\b\n\r\t\cZ\\%_<>()#&]",
            delegate (Match match)
            {
                string v = match.Value;
                switch (v)
                {
                    case "\x00":            // ASCII NUL (0x00) character
                        return "\\0";
                    case "\b":              // BACKSPACE character
                        return "\\b";
                    case "\n":              // NEWLINE (linefeed) character
                        return "\\n";
                    case "\r":              // CARRIAGE RETURN character
                        return "\\r";
                    case "\t":              // TAB
                        return "\\t";
                    case "\u001A":          // Ctrl-Z
                        return "\\Z";
                    case "<":
                        return "&lt";
                    case ">":
                        return "&gt";
                    case "(":
                        return "&#40";
                    case ")":
                        return "&#41";
                    case "#":
                        return "&#35";
                    case "&":
                        return "&#38";
                    default:
                        return "\\" + v;
                }
            });
        }

        public static string sanitizeValue(string str, string strReplace)
        {
            return Regex.Replace(str, @"[\x00'""\b\n\r\t\cZ\\%_<>()#&]",
            delegate (Match match)
            {
                string v = match.Value;
                switch (v)
                {
                    case "\x00":            // ASCII NUL (0x00) character
                        return strReplace;
                    case "\b":              // BACKSPACE character
                        return strReplace;
                    case "\n":              // NEWLINE (linefeed) character
                        return strReplace;
                    case "\r":              // CARRIAGE RETURN character
                        return strReplace;
                    case "\t":              // TAB
                        return strReplace;
                    case "\u001A":          // Ctrl-Z
                        return strReplace;
                    case "<":
                        return strReplace;
                    case ">":
                        return strReplace;
                    case "(":
                        return strReplace;
                    case ")":
                        return strReplace;
                    case "#":
                        return strReplace;
                    case "&":
                        return strReplace;
                    default:
                        return "\\" + v;
                }
            });
        }

        public static string sanitizeDateValue(string str, string strReplace)
        {
            return Regex.Replace(str, @"[\x00'""\b\n\r\t\cZ\\%_<>()#&]",
            delegate (Match match)
            {
                string v = match.Value;
                switch (v)
                {
                    case "\x00":            // ASCII NUL (0x00) character
                        return strReplace;
                    case "\b":              // BACKSPACE character
                        return strReplace;
                    case "\n":              // NEWLINE (linefeed) character
                        return strReplace;
                    case "\r":              // CARRIAGE RETURN character
                        return strReplace;
                    case "\t":              // TAB
                        return strReplace;
                    case "\u001A":          // Ctrl-Z
                        return strReplace;
                    case "<":
                        return strReplace;
                    case ">":
                        return strReplace;
                    case "(":
                        return strReplace;
                    case ")":
                        return strReplace;
                    case "#":
                        return strReplace;
                    case "&":
                        return strReplace;
                    default:
                        return v;
                }
            });
        }

        public static string trimValueWithChars(Control pCtrl, Point pPoint, string pSuffix = "")
        {
            string sText = pCtrl.Text;
            try
            {
                if (pCtrl.Width >= pPoint.X)
                {
                    //int iPixels = convertText2Pixels(pCtrl, pCtrl.Text);
                    //int iSpacePix = convertText2Pixels(pCtrl, " ");
                    //int iSufPix = convertText2Pixels(pCtrl, pSuffix);                    
                    //int iTot = iPixels - iSufPix - iSpacePix;                    
                    float iTotPoint = convert2Points(pCtrl, pCtrl.Width, "X");
                    int iTot = (int)iTotPoint;

                    int iLblPix = pCtrl.Location.X + pPoint.X; //7 + 188
                    //iLblPix = pCtrl.Width - iSufPix - iSpacePix;
                    //float iLblPoint = convert2Points(pCtrl, iLblPix, "X");
                    //int iLbl = (int)iLblPoint;

                    if (pCtrl.Width > iLblPix)
                    {
                        iTot = (int)(pCtrl.Text.Length * 0.75); //Length * 3/4.
                        if (pCtrl.Text.Length > pSuffix.Length + iTot)
                        {
                            iTot = pCtrl.Text.Length - pSuffix.Length - iTot;
                            sText = String.Concat(pCtrl.Text.Substring(0, iTot), pSuffix);
                        }
                    }
                    else
                    {
                        if (pCtrl.Text.Length > pSuffix.Length + 5)
                        {
                            iTot = pCtrl.Text.Length - pSuffix.Length - 5;
                            sText = String.Concat(pCtrl.Text.Substring(0, iTot), pSuffix);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log("Trim.." + ex.Message + Environment.NewLine + ex.StackTrace.ToString());
                return pCtrl.Text;
            }
            return sText;
        }

        public static string trimTextWithChars(Control pCtrl, int pTotWidth, string pSuffix = "")
        {
            string sText = pCtrl.Text;
            try
            {
                if (pCtrl.Width >= pTotWidth)
                {
                    //int iPixels = convertText2Pixels(pCtrl, pCtrl.Text);
                    //int iSpacePix = convertText2Pixels(pCtrl, " ");
                    //int iSufPix = convertText2Pixels(pCtrl, pSuffix);                    
                    //int iTot = iPixels - iSufPix - iSpacePix;                    
                    float iTotPoint = convert2Points(pCtrl, pCtrl.Width, "X");
                    int iTot = (int)iTotPoint;

                    int iWidthPix = pCtrl.Location.X + pTotWidth; //6 + 900
                    //iLblPix = pCtrl.Width - iSufPix - iSpacePix;
                    //float iLblPoint = convert2Points(pCtrl, iLblPix, "X");
                    //int iLbl = (int)iLblPoint;

                    if (pCtrl.Width > iWidthPix)
                    {
                        iTot = (int)(pCtrl.Text.Length * 0.5); //Length * 2/4.
                        if (pCtrl.Text.Length > pSuffix.Length + iTot)
                        {
                            iTot = pCtrl.Text.Length - pSuffix.Length - iTot;
                            sText = String.Concat(pCtrl.Text.Substring(0, iTot), pSuffix);
                        }
                    }
                    else
                    {
                        if (pCtrl.Text.Length > pSuffix.Length + 5)
                        {
                            iTot = pCtrl.Text.Length - pSuffix.Length - 5;
                            sText = String.Concat(pCtrl.Text.Substring(0, iTot), pSuffix);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log("Trim.." + ex.Message + Environment.NewLine + ex.StackTrace.ToString());
                return pCtrl.Text;
            }
            return sText;
        }

        public static string getMapName(string pTechName, Array pMapNames, Array pDispName)
        {
            try
            {
                if (pMapNames == null || pDispName == null) return pTechName;
                int i = 0;
                while (i < pMapNames.Length)
                {
                    if (pMapNames.GetValue(i).ToString().CompareTo(pTechName) == 0)
                        return pDispName.GetValue(i).ToString();

                    i += 1;
                }
            }
            catch (Exception ex)
            {
            }
            return pTechName;
        }



    }

}
