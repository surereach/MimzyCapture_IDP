using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.IO;

namespace SRDocScanIDP
{
    public partial class frmStation : Form
    {
        public frmStation()
        {
            InitializeComponent();
        }

        private void frmStation_Load(object sender, EventArgs e)
        {
            try
            {
                txtBranch.Text = mGlobal.secDecrypt(mGlobal.GetAppCfg("Branch"), mGlobal.objSecData);
                txtStationCode.Text = mGlobal.secDecrypt(mGlobal.GetAppCfg("ScanStn"), mGlobal.objSecData);
                lblBranchExist.Text = txtBranch.Text;
                lblStnExist.Text = txtStationCode.Text;
                
                string sTimeout = mGlobal.GetAppCfg("SessMaxTimeout").Trim();
                if (sTimeout == string.Empty) sTimeout = "0"; 
                ddlSessMax.Text = sTimeout;
                lblSessExist.Text = sTimeout;

                //if (MDIMain.strUserID.Trim().ToLower() == "srappadmin")
                if (staMain.validateUserRole("SR Scan Admin"))
                {
                    txtBranch.Enabled = true;
                    txtStationCode.Enabled = true;
                    ddlSessMax.Enabled = true;
                }
                else
                {
                    txtBranch.Enabled = false;
                    txtStationCode.Enabled = false;
                    ddlSessMax.Enabled = false;
                }
                staMain.sessionStop(false);
            }
            catch (Exception ex)
            {
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            int iCnt = 0;
            try
            {
                bool bEdit = false;

                if (txtBranch.Text.Trim() != lblBranchExist.Text.Trim() || txtStationCode.Text.Trim() != lblStnExist.Text.Trim()
                    || ddlSessMax.Text.Trim() != lblSessExist.Text.Trim())
                    bEdit = true;

                if (bEdit)
                {
                    if (ddlSessMax.Enabled)
                    {
                        bool bOK = mGlobal.writeAppCfg("SessMaxTimeout", ddlSessMax.Text.Trim());

                        if (bOK)
                        {
                            MDIMain.iSessMaxTimeout = Convert.ToInt32(ddlSessMax.Text.Trim());
                            iCnt += 1;
                        }
                    }

                    if (txtBranch.Enabled)
                    {
                        bool bOK = false;

                        if (checkBranchExist())
                            bOK = mGlobal.writeAppCfg("Branch", mGlobal.secEncrypt(txtBranch.Text.Trim(), mGlobal.objSecData));
                        else
                        {
                            MessageBox.Show(this, "Branch code does not exists or invalid!", "Message");
                            return;
                        }

                        if (bOK)
                        {
                            iCnt += 1;
                        }
                    }

                    if (txtStationCode.Enabled)
                    {
                        bool bOK = mGlobal.writeAppCfg("ScanStn", mGlobal.secEncrypt(txtStationCode.Text.Trim(), mGlobal.objSecData));

                        if (bOK)
                        {
                            MDIMain.strStation = txtStationCode.Text.Trim();
                            iCnt += 1;
                        }
                    }

                    if (iCnt > 1)
                    {
                        lblBranchExist.Text = txtBranch.Text;
                        lblStnExist.Text = txtStationCode.Text;
                        if (ddlSessMax.Text.Trim() == string.Empty)
                            lblSessExist.Text = mGlobal.GetAppCfg("SessMaxTimeout").Trim();
                        else
                            lblSessExist.Text = ddlSessMax.Text.Trim();
                        MessageBox.Show(this, "Save successfully.", "Message");
                    }
                }
                
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Error! " + ex.Message, "Message");
            }
        }

        private bool checkBranchExist()
        {
            bool bExist = false;
            try
            {
                clsHttpHandler oWebAPI = new clsHttpHandler();
                oWebAPI.gCred = mGlobal.GetAppCfg("Crede") + ":" + mGlobal.GetAppCfg("Crede1");

                string sMsg = "";
                System.Collections.Hashtable tHeader = new System.Collections.Hashtable(4);
                tHeader.Add("m", "chkbranch");
                tHeader.Add("brcd", txtBranch.Text.Trim());

                System.Net.HttpWebResponse oResp = oWebAPI.HttpCall("Get", mGlobal.GetAppCfg("BaseUrl"), "text/app; encoding='utf-8'", tHeader, ref sMsg);

                if (sMsg.Trim() == "")
                {
                    Stream oRespStrm = oResp.GetResponseStream();
                    StreamReader oSReader = new StreamReader(oRespStrm);
                    string sResp = oSReader.ReadToEnd();

                    mGlobal.Write2Log("Branch.." + sResp);

                    if (sResp.Trim().ToLower() == "valid")
                    {
                        bExist = true;
                    }

                    oResp.Close();
                    oRespStrm.Close();
                    oSReader.Close();
                }
                else
                {
                    if (oResp != null) oResp.Close();
                    mGlobal.Write2Log("Branch setting.." + sMsg);
                }
            }
            catch (Exception ex)
            {
                bExist = false;
            }

            return bExist;
        }

        private void frmStation_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                staMain.sessionRestart();
            }
            catch (Exception ex)
            {
            }
        }


    }
}
