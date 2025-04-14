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

    public partial class frmLookup : Form
    {
        private string sReturn;
        public string returnValue
        {
            get { return sReturn; }
        }

        public frmLookup()
        {
            InitializeComponent();
        }

        private void frmLookup_Load(object sender, EventArgs e)
        {

        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                string sSQL = "";

                mGlobal.LoadAppDBCfg();

                sReturn = "Returned";
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error! " + ex.Message);
            }
        }

        private void lvwData_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            sReturn = "Returned";
            this.Close();
        }
    }
}
