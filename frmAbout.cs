using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SRDocScanIDP
{
    public partial class frmAbout : Form
    {
        public frmAbout()
        {
            InitializeComponent();
        }

        private void frmAbout_Load(object sender, EventArgs e)
        {
            label1.Text = Application.ProductName;
            label2.Text = "(Version " + Application.ProductVersion + ") General Edition IDP"; //include <Version/> in .csproj file if using Net Core.
            staMain.sessionStop(false);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            try
            {
                staMain.sessionRestart();
                this.Close();
            }
            catch (Exception ex)
            {
            }
        }

    }
}
