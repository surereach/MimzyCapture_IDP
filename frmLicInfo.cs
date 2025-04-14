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
    public partial class frmLicInfo : Form
    {
        public frmLicInfo()
        {
            InitializeComponent();
        }

        private void frmLicInfo_Load(object sender, EventArgs e)
        {
            try
            {
                staMain.sessionStop(false);
                string sLicCode = mGlobal.GetAppCfg("SRLic");
                string sInfo = string.Empty;

                if (sLicCode.Trim() != string.Empty)
                {
                    LicLibGEN.clsLicGEN oLicLib = new LicLibGEN.clsLicGEN();
                    oLicLib.sCred = mGlobal.GetAppCfg("Crede");
                    oLicLib.sCred1 = mGlobal.GetAppCfg("Crede1");
                    oLicLib.sBaseUrl = mGlobal.GetAppCfg("BaseUrl");

                    LicLibGEN.License oLic = oLicLib.getLicenseInfo(sLicCode);

                    if (oLic.Code.Trim() == "0")
                    {
                        txtLicCode.Text = sLicCode;
                        sInfo = "Name: " + oLic.Name + Environment.NewLine;
                        sInfo += Environment.NewLine;
                        sInfo += "Unlimited: ";
                        sInfo += oLic.Unlimit.ToUpper() == "Y" ? "Yes" : "No" + Environment.NewLine;
                        sInfo += Environment.NewLine;
                        sInfo += "Start Date: " + oLic.Start + Environment.NewLine;
                        sInfo += Environment.NewLine;
                        sInfo += "Expiry Date: " + oLic.Last + Environment.NewLine;
                        sInfo += Environment.NewLine;
                        sInfo += "Days: " + oLic.Days + Environment.NewLine;
                        sInfo += Environment.NewLine;
                        sInfo += "Scan Users: " + oLic.Scan + Environment.NewLine;
                        sInfo += Environment.NewLine;
                        sInfo += "Index Users: " + oLic.Index + Environment.NewLine;
                        sInfo += Environment.NewLine;
                        sInfo += "Verify Users: " + oLic.Verify + Environment.NewLine;

                        rtxInfo.Text = sInfo;
                    }
                    else
                    {
                        rtxInfo.Text = "";
                        mGlobal.Write2Log("Lic. Info." + oLic.Code + ".." + oLic.Message);
                    }
                        
                }
                else
                    MessageBox.Show("SR License code not found!", "Message");
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log("License info.." + ex.Message);
                mGlobal.Write2Log(ex.StackTrace.ToString());
                MessageBox.Show(this, "Error! " + ex.Message);
            }
        }

        private void frmLicInfo_FormClosed(object sender, FormClosedEventArgs e)
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
