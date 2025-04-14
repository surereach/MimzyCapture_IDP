using System;
using System.Text;
using System.Net;
using System.Collections;
using System.IO;

using SRDocScanIDP;
using System.Data;

public class clsHttpHandler
{
    public string gCred;
    public byte[] upBytes; //= File.ReadAllBytes(Path.Combine(Path, filename));

    public string userid { get; set; }
    public string pswd { get; set; }

    public string HttpCall(string pMethod, string pUrl, ref string pContType) //E.g. JSon String.
    {
        string sMsg = "";
        //string sUid = "";
        //string sPwd = "";

        try
        {
            HttpWebRequest oReq = (HttpWebRequest) WebRequest.Create(pUrl);

            //Cred = sUid + ":" + sPwd;
            if (gCred.Trim() == "")
            {
                throw new Exception("User credential is required!");
                return sMsg;
            }

            string authz = Convert.ToBase64String(Encoding.Default.GetBytes(gCred));
            oReq.Headers["Authorization"] = "Basic " + authz;

            oReq.Method = pMethod;
            //oReq.Headers.Add("Access-Control-Allow-Methods", "DELETE"); //need to configure IIS if to use Delete method.
            //oReq.Headers.Add("X-HTTP-Method-Override", "POST");
            oReq.ContentType = pContType; //"text/xml; encoding='utf - 8'";
            oReq.KeepAlive = true;
            oReq.Timeout = 65000;
            oReq.ContentLength = 0;

            HttpWebResponse oResp = (HttpWebResponse) oReq.GetResponse();
            pContType = oResp.ContentType;

            Stream oRespStream = oResp.GetResponseStream();
            StreamReader sr = new StreamReader(oRespStream);
            sMsg = sr.ReadToEnd();
            sr.Close();

            oRespStream.Close();
            oResp.Close();
        }
        catch (Exception ex)
        {
            sMsg = ex.Message;
        }

        return sMsg;
    }

    public HttpWebResponse HttpCall(string pMethod, string pUrl, string pContType, Hashtable pHeaders, ref string pMsg)
    {
        HttpWebResponse oRes = null;
        //string sUid = "";
        //string sPwd = "";
        try
        {
            HttpWebRequest oReq = (HttpWebRequest) WebRequest.Create(pUrl);

            //gCred = sUid + ":" + sPwd;
            if (gCred.Trim() == "")
            {
                throw new Exception("User credential is required!");
                return oRes;
            }

            string authz = Convert.ToBase64String(Encoding.Default.GetBytes(gCred));
            oReq.Headers["Authorization"] = "Basic " + authz;

            foreach (DictionaryEntry he in pHeaders)
            { 
                oReq.Headers.Add(he.Key.ToString(), he.Value.ToString());
            }

            oReq.Method = pMethod;
            //oReq.Headers.Add("Access-Control-Allow-Methods", "DELETE"); //need to configure IIS if to use Delete method.
            //oReq.Headers.Add("X-HTTP-Method-Override", "POST");
            oReq.ContentType = pContType; //"text/xml; encoding='utf - 8'";
            oReq.KeepAlive = true;
            oReq.Timeout = 65000;
            oReq.ContentLength = 0;

            if (oReq.Method.ToUpper().Trim() == "PUT")
            {
                if (upBytes.Length > 0)
                {
                    oReq.ContentLength = upBytes.Length;

                    using (Stream oReqStream = oReq.GetRequestStream())
                    {
                        //Send the file as body request. 
                        oReqStream.Write(upBytes, 0, upBytes.Length);
                        oReqStream.Close();
                    }
                }
            }

            oRes = (HttpWebResponse) oReq.GetResponse();
        }
        catch (Exception ex)
        {
            pMsg = ex.Message;
        }

        return oRes;
    }

    public HttpWebResponse HttpCall(string pMethod, string pUrl, string pContType, Hashtable pHeaders, string pBody, ref string pMsg)
    {
        HttpWebResponse oRes = null;
        //string sUid = "";
        //string sPwd = "";
        try
        {
            HttpWebRequest oReq = (HttpWebRequest)WebRequest.Create(pUrl);

            //gCred = sUid + ":" + sPwd;
            if (gCred.Trim() == "")
            {
                throw new Exception("User credential is required!");
                return oRes;
            }

            string authz = Convert.ToBase64String(Encoding.Default.GetBytes(gCred));
            oReq.Headers["Authorization"] = "Basic " + authz;

            foreach (DictionaryEntry he in pHeaders)
            {
                oReq.Headers.Add(he.Key.ToString(), he.Value.ToString());
            }

            oReq.Method = pMethod;
            //oReq.Headers.Add("Access-Control-Allow-Methods", "DELETE"); //need to configure IIS if to use Delete method.
            //oReq.Headers.Add("X-HTTP-Method-Override", "POST");
            oReq.ContentType = pContType; //"text/xml; encoding='utf - 8'";
            oReq.KeepAlive = true;
            oReq.Timeout = 65000;
            oReq.ContentLength = 0;

            if (oReq.Method.ToUpper().Trim() == "PUT") //Upload preparation.
            {
                if (upBytes.Length > 0)
                {
                    oReq.ContentLength = upBytes.Length;

                    using (Stream oReqStream = oReq.GetRequestStream())
                    {
                        //Send the file as body request. 
                        oReqStream.Write(upBytes, 0, upBytes.Length);
                        oReqStream.Close();
                    }
                }
            }
            else
            {
                if (pBody.Trim() != "")
                {
                    oReq.ContentLength = pBody.Length;
                    upBytes = Encoding.UTF8.GetBytes(pBody);

                    using (Stream oReqStream = oReq.GetRequestStream())
                    {
                        //Send the file as body request. 
                        oReqStream.Write(upBytes, 0, upBytes.Length);
                        oReqStream.Close();
                    }
                }
            }

            oRes = (HttpWebResponse)oReq.GetResponse();
        }
        catch (Exception ex)
        {
            pMsg = ex.Message;
        }

        return oRes;
    }

    public HttpWebResponse HttpCall(string pMethod, string pUrl, string pContType, string pBody, ref string pMsg)
    {
        HttpWebResponse oRes = null;
        //string sUid = "";
        //string sPwd = "";
        try
        {
            HttpWebRequest oReq = (HttpWebRequest)WebRequest.Create(pUrl);

            //gCred = sUid + ":" + sPwd;
            if (gCred.Trim() == "")
            {
                throw new Exception("User credential is required!");
                return oRes;
            }

            string authz = Convert.ToBase64String(Encoding.Default.GetBytes(gCred));
            oReq.Headers["Authorization"] = "Basic " + authz;

            oReq.Method = pMethod;
            //oReq.Headers.Add("Access-Control-Allow-Methods", "DELETE"); //need to configure IIS if to use Delete method.
            //oReq.Headers.Add("X-HTTP-Method-Override", "POST");
            oReq.ContentType = pContType; //"text/xml; encoding='utf - 8'";
            oReq.KeepAlive = true;
            oReq.Timeout = 65000;
            oReq.ContentLength = 0;

            if (oReq.Method.ToUpper().Trim() == "PUT") //Upload preparation.
            {
                if (upBytes.Length > 0)
                {
                    oReq.ContentLength = upBytes.Length;

                    using (Stream oReqStream = oReq.GetRequestStream())
                    {
                        //Send the file as body request. 
                        oReqStream.Write(upBytes, 0, upBytes.Length);
                        oReqStream.Close();
                    }
                }
            }
            else
            {
                if (pBody.Trim() != "")
                {
                    oReq.ContentLength = pBody.Length;
                    upBytes = Encoding.UTF8.GetBytes(pBody);

                    using (Stream oReqStream = oReq.GetRequestStream())
                    {
                        //Send the file as body request. 
                        oReqStream.Write(upBytes, 0, upBytes.Length);
                        oReqStream.Close();
                    }
                }
            }

            oRes = (HttpWebResponse)oReq.GetResponse();
        }
        catch (Exception ex)
        {
            pMsg = ex.Message;
        }

        return oRes;
    }

}
