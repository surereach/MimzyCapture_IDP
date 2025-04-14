using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SRDocScanIDP
{
    extern alias clsUtilComn;

    public class clsUserRoles
    {
        [JsonProperty("uname")]
        public string Uname { get; set; }

        [JsonProperty("roles")]
        public Role[] Roles { get; set; }
    }

    public partial class Role
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("rname")]
        public string Rname { get; set; }
    }


    internal static class Converter
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

    public partial class UserRoles
    {
        public static clsUserRoles[] FromJson(string json) => JsonConvert.DeserializeObject<clsUserRoles[]>(json, Converter.Settings);
    }

    public class clsLogin
    {
        public int Attempts { get; set; }
        public bool AttemptFail { get; set; }

        public bool authUserCred(string pLogin, string pPwd, string pLicCode, ref string pMsg)
        {
            bool bAuth = false;
            try
            {
                clsHttpHandler oWebAPI = new clsHttpHandler();
                oWebAPI.gCred = mGlobal.GetAppCfg("Crede") + ":" + mGlobal.GetAppCfg("Crede1");

                AttemptFail = false;
                string sMsg = "";
                System.Collections.Hashtable tHeader = new System.Collections.Hashtable(4);
                tHeader.Add("m", "onauth");
                tHeader.Add("md", "Scan");
                tHeader.Add("id", pLogin);
                tHeader.Add("pw", mGlobal.secEncrypt(pPwd, mGlobal.objSecAuth));
                tHeader.Add("at", Attempts.ToString());
                tHeader.Add("lc", pLicCode);

                System.Net.HttpWebResponse oResp = oWebAPI.HttpCall("Get", mGlobal.GetAppCfg("BaseUrl"), "text/app; encoding='utf-8'", tHeader, ref sMsg);

                if (sMsg.Trim() == "")
                {
                    Stream oRespStrm = oResp.GetResponseStream();
                    StreamReader oSReader = new StreamReader(oRespStrm);
                    string sResp = oSReader.ReadToEnd();

                    if (sResp.Trim().ToLower() == "passed")
                    {
                        mGlobal.strCurrLoginID = pLogin;
                        mGlobal.strCurrUserName = getUserNameAndRole(mGlobal.strCurrLoginID);
                        bAuth = true;

                        mGlobal.Write2Log("Login.." + sResp);
                    }
                    else if (sResp.Split('-').Length > 2)
                    {
                        if (sResp.Split('-')[2] == "10") //error code.
                            AttemptFail = true;

                        mGlobal.Write2Log("Login.." + sResp + " " + pLogin);
                    }
                    else if (sResp.Trim().ToLower() != "failed")
                    {
                        pMsg = sResp;
                        bAuth = false;
                        mGlobal.Write2Log("Login.." + sResp);
                    }
                    else
                        mGlobal.Write2Log("Login.." + sResp + " " + pLogin);

                    oResp.Close();
                    oRespStrm.Close();
                    oSReader.Close();
                }
                else
                {
                    if (oResp != null) oResp.Close();
                    mGlobal.Write2Log("User auth.." + sMsg);
                }
            }
            catch (Exception ex)
            {
                bAuth = false;
                mGlobal.Write2Log("validate..:" + ex.Message);
            }
            return bAuth;
        }

        public bool validateLogin(string strLogin, string strPwd)
        {
            bool blnValid = false;

            try
            {
                mGlobal.LoadAppDBCfg();

                string sSQL = "";
                if (mGlobal.strClearTextCred == "0")
                    sSQL = "Select * from TUsrAcct Where EUSERID ='" + strLogin.Trim().Replace("'", "") + "' and EPWORD ='" + strPwd.Trim().Replace("'", "") + "' ";
                else
                    sSQL = "Select * from TUsrAcct Where EUSERID ='" + strLogin.Trim().Replace("'", "") + "' and EPWORD ='" + mGlobal.secEncrypt(strPwd.Trim().Replace("'", ""), mGlobal.objSecData) + "' ";

                sSQL = sSQL + "and MODULETYPE='SCAN' ";
 
                System.Data.DataSet rsUser;
                rsUser = mGlobal.returnRecordset("TUserAcc", sSQL);

                if (rsUser!=null) {
                    if (rsUser.Tables[0].Rows.Count > 0) {
                        mGlobal.strCurrLoginID = strLogin;
                        getUserNameDB(mGlobal.strCurrLoginID);

                        //mGlobal.Write2Log("User:" + mGlobal.strCurrLoginID + " has logged in successfully.");
                        blnValid = true;
                    }
                    rsUser.Dispose();  
                }
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log("validate.." + ex.Message);
            }

            return blnValid;
        }

        public bool validateUserRole(string pUserRoleName)
        {
            try
            {
                if (mGlobal.strCurrUserRoles != null)
                {
                    if (mGlobal.strCurrUserRoles.Count > 0)
                    {
                        int i = 0;
                        while (i < mGlobal.strCurrUserRoles.Count)
                        {
                            if (mGlobal.strCurrUserRoles[i].ToLower().Trim() == pUserRoleName.ToLower().Trim())
                            {
                                return true;
                            }

                            i += 1;
                        }
                    }
                    else
                        return false;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                return false;
            }
            return false;
        }

        public bool updatePwd(string strLogin, string strNewPwd)
        {
            bool blnUpdate = true;
            try
            {
                mGlobal.objDB.sConnectionString = ""; //Init  
                mGlobal.LoadAppDBCfg();

                if (mGlobal.objDB == null)
                {
                    mGlobal.objDB = new clsDatabase();
                }

                string sSQL = "";

                if (mGlobal.strClearTextCred == "0")
                    sSQL = "Update TUsrAcct Set EPWORD='" + strNewPwd.Trim().Replace("'", "") + "' Where EUSERID='" + strLogin.Trim().Replace("'", "") + "' and MODULETYPE='SCAN' ";
                else
                    sSQL = "Update TUsrAcct Set EPWORD='" + mGlobal.secEncrypt(strNewPwd.Trim().Replace("'", ""), mGlobal.objSecData) + "' Where EUSERID='" + strLogin.Trim().Replace("'", "") + "' and MODULETYPE='SCAN' ";

                blnUpdate = mGlobal.objDB.UpdateRows(sSQL, true);
                
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log("update login.." + ex.Message);
            }

            return blnUpdate;
        }

        public string getUserNameAndRole(string pLogin)
        {
            string sName = "";

            try
            {
                clsHttpHandler oWebAPI = new clsHttpHandler();
                oWebAPI.gCred = mGlobal.GetAppCfg("Crede") + ":" + mGlobal.GetAppCfg("Crede1");

                string sMsg = "";
                System.Collections.Hashtable tHeader = new System.Collections.Hashtable(2);
                tHeader.Add("m", "getuname");
                tHeader.Add("lg", pLogin);

                System.Net.HttpWebResponse oResp = oWebAPI.HttpCall("Get", mGlobal.GetAppCfg("BaseUrl"), "text/app; encoding='utf-8'", tHeader, ref sMsg);

                if (sMsg.Trim() == "")
                {
                    Stream oRespStrm = oResp.GetResponseStream();
                    StreamReader oSReader = new StreamReader(oRespStrm);
                    string sResp = oSReader.ReadToEnd();

                    //mGlobal.Write2Log("User ID roles.." + pLogin);

                    if (sResp.Trim().ToLower() != "record not found" && oResp.ContentType.ToLower().IndexOf("json") > 0)
                    {
                        clsUserRoles[] oRoles = UserRoles.FromJson(sResp);

                        if (oRoles != null)
                        {
                            sName = oRoles[0].Uname;

                            mGlobal.strCurrUserRoles = new List<string>();
                            int i = 0;
                            while (i < oRoles[0].Roles.Length)
                            {
                                mGlobal.strCurrUserRoles.Add(oRoles[0].Roles[i].Rname);
                                i += 1;
                            }
                        }
                        
                    }
                    else
                        mGlobal.Write2Log("User roles error.." + sResp);

                    oResp.Close();
                    oRespStrm.Close();
                    oSReader.Close();
                }
                else
                {
                    if (oResp != null) oResp.Close();
                    mGlobal.Write2Log("User name.." + sMsg);
                }
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log("get user name.." + ex.Message);
            }

            return sName;
        }

        public bool getUserNameDB(string strLoginID)
        {
            bool blnSucc = false;

            try
            {
                mGlobal.LoadAppDBCfg();

                string sSQL = "";
                sSQL = "Select username from TUsrAcct Where EUSERID='" + strLoginID.Trim().Replace("'", "") + "' ";

                System.Data.DataSet rsUser;
                rsUser = mGlobal.returnRecordset("TUsrAcct", sSQL);
                if (rsUser != null)
                {
                    if (rsUser.Tables[0].Rows.Count > 0)
                    {
                        mGlobal.strCurrUserName = rsUser.Tables[0].Rows[0][0].ToString();

                        blnSucc = true;                        
                    }
                    rsUser.Dispose();
                }
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log("get user name.." + ex.Message);
            }

            return blnSucc;
        }

        public bool userLogout(string pModule, string pLogin)
        {
            bool bLogout = false;
            try
            {
                clsHttpHandler oWebAPI = new clsHttpHandler();
                oWebAPI.gCred = mGlobal.GetAppCfg("Crede") + ":" + mGlobal.GetAppCfg("Crede1");

                string sMsg = "";
                System.Collections.Hashtable tHeader = new System.Collections.Hashtable(4);
                tHeader.Add("m", "logout");
                tHeader.Add("md", pModule);
                tHeader.Add("id", pLogin);

                System.Net.HttpWebResponse oResp = oWebAPI.HttpCall("Post", mGlobal.GetAppCfg("BaseUrl"), "text/app; encoding='utf-8'", tHeader, ref sMsg);

                if (sMsg.Trim() == "")
                {
                    Stream oRespStrm = oResp.GetResponseStream();
                    StreamReader oSReader = new StreamReader(oRespStrm);
                    string sResp = oSReader.ReadToEnd();

                    if (sResp.Trim().ToLower() == "completed")
                        bLogout = true;

                    oResp.Close();
                    oRespStrm.Close();
                    oSReader.Close();
                }
                else
                {
                    if (oResp != null) oResp.Close();
                    mGlobal.Write2Log("Logout.." + sMsg);
                }
            }
            catch (Exception ex)
            {
                bLogout = false;
                mGlobal.Write2Log("Logout..:" + ex.Message);
            }
            return bLogout;
        }


    }

}
