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
    public partial class frmRemarks : Form
    {
        public string sRemarks;
        public bool bEdit;

        public frmRemarks()
        {
            InitializeComponent();
            sRemarks = "";
            bEdit = false;
        }

        private void frmRemarks_Load(object sender, EventArgs e)
        {
            try
            {
                if (sRemarks.Trim() != string.Empty)
                    rtbRemarks.Text = sRemarks.Trim();

                if (bEdit)
                {
                    rtbRemarks.ReadOnly = false;
                }
                else
                {
                    rtbRemarks.ReadOnly = true;
                }
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log("Remarks.." + ex.Message);
            }            
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                if (bEdit)
                { 
                    sRemarks = rtbRemarks.Text.Trim();
                }
                this.Close();
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log("Remarks.." + ex.Message);
                mGlobal.Write2Log(ex.StackTrace.ToString());
            }
        }

        private void frmRemarks_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                if (bEdit)
                {
                    sRemarks = rtbRemarks.Text.Trim();
                }
            }
            catch (Exception ex)
            {
            }
        }


    }
}
