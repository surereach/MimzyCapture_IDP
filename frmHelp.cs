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
    public partial class frmHelp : Form
    {
        public frmHelp()
        {
            InitializeComponent();
        }

        private void frmHelp_Load(object sender, EventArgs e)
        {
            try
            {
                staMain.sessionStop(false);
                Navigate(@"https://www.google.com");
            }
            catch (Exception ex)
            {
            }
        }

        // Navigates to the given URL if it is valid.
        private void Navigate(String pAddress)
        {
            if (String.IsNullOrEmpty(pAddress)) return;
            if (pAddress.Equals("about:blank")) return;

            if (!pAddress.StartsWith("http://") && !pAddress.StartsWith("https://"))
            {
                pAddress = "http://" + pAddress;
            }

            try
            {
                //webBrowser1.Navigate(new Uri(pAddress))
                webBrowser1.DocumentText = "<html><body><h1>Mimzy Capture User Manual coming soon...</h1></body></html>";
            }
            catch (System.UriFormatException)
            {
                return;
            }
        }

        private void frmHelp_FormClosed(object sender, FormClosedEventArgs e)
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
