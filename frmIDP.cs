using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace SRDocScanIDP
{

    public partial class frmIDP : Form
    {
        private string sReturn;
        public string returnValue
        {
            get { return sReturn; }
        }

        public frmIDP()
        {
            InitializeComponent();
        }

        private void frmIDP_Load(object sender, EventArgs e)
        {
            try
            {
                cbxMethod.SelectedIndex = 0;
                clsIDP.loadIDPMappingDB(staMain.stcProjCfg.PrjCode.Trim(), staMain.stcProjCfg.AppNum.Trim(), staMain.stcProjCfg.DocDefId.Trim());
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                if (txtPathFile.Text.Trim() == string.Empty)
                {
                    MessageBox.Show(this, "The filename path is required!");
                    return;
                }
                else if (File.Exists(txtPathFile.Text.Trim()) == false)
                {
                    MessageBox.Show(this, "The file does not exist!");
                    return;
                }
                Document[] aDocs = null;

                string sLocalEndPoint = mGlobal.GetAppCfg("IDPBaseUrl").Trim() + "/analyze";
                clsIDP oIDP = new clsIDP();
                RequestIDP oReq = new RequestIDP();
                oReq.id = DateTime.Now.ToString("yyyyMMddhhmmss");
                oReq.endPoint = clsIDP.stcIDPMapping.EndPoint.Trim();//mGlobal.GetAppCfg("IDPEndPoint").Trim();
                oReq.apiKey = clsIDP.stcIDPMapping.IDPKey.Trim();//mGlobal.GetAppCfg("IDPKey").Trim();
                byte[] data = File.ReadAllBytes(txtPathFile.Text.Trim());
                oReq.baseString = Convert.ToBase64String(data);

                string sMsg = "";
                if (cbxMethod.SelectedIndex == 0) //Classify document.
                {
                    oReq.classify = "Yes";
                    oReq.model = "DocClassDemo";

                    dynamic dResult = oIDP.docClassify(sLocalEndPoint, oReq, ref sMsg);
                    if (dResult != null)
                    {
                        if (dResult?.GetType() == typeof(bool))
                        {
                            if (dResult == false)
                                MessageBox.Show(this, "Failed! " + sMsg);
                        }
                        else
                        {
                            ClassifyResult oClr = ClassifyResult.FromJson(dResult);

                            if (oClr != null)
                            {
                                aDocs = oClr.Documents;
                                MessageBox.Show(this, "Doc. Type: " + oClr.Documents[0].DocumentType);
                            }

                        }
                    }
                }
                else if (cbxMethod.SelectedIndex == 0)//analyse document.
                {
                    oReq.classify = "No";
                    oReq.model = "BTNForm";

                    dynamic dResult = oIDP.docAnalysis(sLocalEndPoint, oReq, ref sMsg);
                    if (dResult != null)
                    {
                        if (dResult?.GetType() == typeof(bool))
                        {
                            if (dResult == false)
                                MessageBox.Show(this, "Failed! " + sMsg);
                        }
                        else
                        {
                            AnalysisResult oAna = AnalysisResult.FromJson(dResult);
                        }
                    }
                }
                else //train the model.
                {
                    RequestBuild oRequ = new RequestBuild();
                    oRequ.id = oReq.id;
                    oRequ.endPoint = oReq.endPoint;
                    oRequ.apiKey = oReq.apiKey;
                    oRequ.model = "BTNForm5";
                    oRequ.filename = Path.GetFileName(txtPathFile.Text.Trim());
                    oRequ.baseString = oReq.baseString;
                    sLocalEndPoint = mGlobal.GetAppCfg("IDPBaseUrl").Trim() + "/build";

                    dynamic dResult = trainModel(sLocalEndPoint, oRequ, ref sMsg);

                    if (dResult != null)
                    {
                        if (dResult?.GetType() == typeof(bool))
                        {
                            if (dResult == false)
                                MessageBox.Show(this, "Failed! " + sMsg);
                        }
                        else
                        {                            
                            mGlobal.Write2Log(dResult.ToString());
                        }
                    }
                }

                sReturn = "Returned";
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Error! " + ex.Message);
            }
        }

        private void lvwData_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            sReturn = "Returned";
            this.Close();
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            try
            {
                openFileDialog.InitialDirectory = @"C:\";
                openFileDialog.Title = "Browse directories and filename for IDP.";
                openFileDialog.Filter = "Pdf file (*.pdf)|*.pdf|Tiff file (*.tiff)|*.tiff";
                openFileDialog.FilterIndex = 0;
                openFileDialog.Multiselect = false;
                openFileDialog.AddExtension = true;
                openFileDialog.CheckFileExists = false;
                openFileDialog.CheckPathExists = true;
                openFileDialog.DefaultExt = "pdf";
                openFileDialog.FileName = "";

                if (openFileDialog.ShowDialog(this) == DialogResult.OK)
                {
                    txtPathFile.Text = openFileDialog.FileName;
                    openFileDialog.Dispose();
                }
                else
                {
                    openFileDialog.Dispose();
                    return;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error! " + ex.Message);
            }
        }


        private dynamic trainModel(string pBaseUrl, RequestBuild pIDP, ref string pMsg)
        {
            try
            {
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

                    ResponseBuild oResult = BuildResultObj.FromJson(sResp);
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
                else
                    MessageBox.Show(this, pMsg);
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log(ex.Message + Environment.NewLine + ex.StackTrace.ToString());
                throw new Exception(ex.Message);
            }
            return false;
        }


    }
}
