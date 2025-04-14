using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SRDocScanIDP
{
    public partial class frmLicReg : Form
    {
        public frmLicReg()
        {
            InitializeComponent();
        }

        private void frmLicInfo_Load(object sender, EventArgs e)
        {
            try
            {
                txtLicCode.Text = string.Empty;
                staMain.sessionStop(false);
            }
            catch (Exception ex)
            {
            }
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            string sMsg = string.Empty;
            try
            {
                if (MessageBox.Show(this,"Confirm to register this license?","Confirm Register License",MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    return;
                }
                string sLicCode = txtLicCode.Text.Trim();

                if (sLicCode != string.Empty)
                {
                    LicLibGEN.clsLicGEN oLicLib = new LicLibGEN.clsLicGEN();
                    oLicLib.sCred = mGlobal.GetAppCfg("Crede");
                    oLicLib.sCred1 = mGlobal.GetAppCfg("Crede1");
                    oLicLib.sBaseUrl = mGlobal.GetAppCfg("BaseUrl");

                    bool bOK = oLicLib.validateLicense(sLicCode, ref sMsg);

                    if (bOK)
                    {
                        mGlobal.writeAppCfg("SRLic", sLicCode);
                        mGlobal.Write2Log("Lic registration succeed." + sMsg);
                        MessageBox.Show("License registration succeed!", "Message");

                        sMsg = "";
                        oLicLib.updateLicenseReg(sLicCode, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), ref sMsg);
                        mGlobal.Write2Log("Lic registration update." + sMsg);

                        this.Close();
                    }
                    else
                    {
                        mGlobal.Write2Log("Lic registration.." + sMsg);
                        MessageBox.Show("License registration failed! Please contact your System Administrator."
                            + Environment.NewLine + sMsg, "Message");
                    }
                }
                else
                {
                    MessageBox.Show("License code is required! Please contact your System Administrator.", "Message");
                }
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log("Register lic.." + ex.Message);
                mGlobal.Write2Log(ex.StackTrace.ToString());
                MessageBox.Show(ex.Message, "Error Message");
            }
        }

        private void frmLicReg_FormClosed(object sender, FormClosedEventArgs e)
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
