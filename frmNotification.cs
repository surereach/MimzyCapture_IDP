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
    public partial class frmNotification : Form
    {
        private string _currScanProj;
        private string _appNum;

        public string _batchCode;

        private clsDocuFile oDF = new clsDocuFile();

        private clsLvwColSorter lvwColSorter;

        public frmNotification()
        {
            InitializeComponent();
        }

        private void frmNotification_Load(object sender, EventArgs e)
        {
            try
            {
                _currScanProj = MDIMain.strScanProject; //this.MdiParent.Controls["toolStrip"].Controls[0].Text;
                _appNum = MDIMain.intAppNum.ToString("000");

                lvwColSorter = new clsLvwColSorter();
                lvwNotice.ListViewItemSorter = lvwColSorter;

                loadNotificationListHeaders();

                this.Height = lvwNotice.Height + 20;

                _batchCode = "";

                if (lvwNotice.Items.Count > 0)
                    btnDeleteAllStrip.Enabled = true;
                else
                    btnDeleteAllStrip.Enabled = false;
            }
            catch (Exception ex)
            {
            }
        }

        public void validateTreeview(string pBatchCode, List<string> pScan)
        {            
            try
            {
                _batchCode = pBatchCode;
                if (pScan != null)
                {
                    if (pScan.Count > 0) //Scan exist.
                    {
                        List<clsDocument> lDup = new List<clsDocument>();
                        clsDocument oDoc = new clsDocument();

                        lDup = oDF.findDuplicateDocType(pScan);

                        int i = 0;
                        while (i < lDup.Count)
                        {
                            oDoc = lDup[i];

                            saveNotificationDB(pBatchCode, oDoc.Set.Split('_')[1].Trim(), oDoc.DocType, oDoc.Set + " - " + oDoc.DocType, "Duplicate document type found in document set.");
                            i += 1;
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Validate error!\n" + ex.Message, "Error");
            }

        }

        public bool deleteAllNotificationDB(string pBatchCode)
        {
            string sSQL = "";

            try
            {
                mGlobal.LoadAppDBCfg();

                //Save notification.
                sSQL = "DELETE FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TNotification ";
                sSQL += "WHERE batchCode='" + pBatchCode.Trim().Replace("'", "") + "' ";
               
                //mGlobal.Write2Log(sSQL);

                mGlobal.objDB.UpdateRows(sSQL);
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        public void saveNotificationDB(string pBatchCode, string pSetNum, string pDocType, string pReference, string pDesc)
        {
            string sSQL = "";

            try
            {
                _batchCode = pBatchCode;
                mGlobal.LoadAppDBCfg();

                //Save notification.
                sSQL = "INSERT INTO " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TNotification ";
                sSQL += "([scanproj]";
                sSQL += ",[appnum]";
                sSQL += ",[batchcode]";
                sSQL += ",[setnum]";
                sSQL += ",[doctype]";
                sSQL += ",[reference]";
                sSQL += ",[desc]";
                sSQL += ",[remarks]";
                sSQL += ") VALUES (";
                sSQL += "'" + _currScanProj.Trim().Replace("'", "") + "'";
                sSQL += ",'" + _appNum.Trim().Replace("'", "") + "'";
                sSQL += ",'" + pBatchCode.Trim().Replace("'", "") + "'";
                sSQL += ",'" + pSetNum.Trim().Replace("'", "") + "'";
                sSQL += ",'" + pDocType.Trim().Replace("'", "") + "'";
                sSQL += ",'" + pReference.Trim().Replace("'", "") + "'";
                sSQL += ",'" + pDesc.Trim().Replace("'", "") + "'";
                sSQL += ",''";
                sSQL += ") ";

                //mGlobal.Write2Log(sSQL);

                mGlobal.objDB.UpdateRows(sSQL);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Save notification error!\n" + ex.Message, "Error");
            }

        }

        private void loadNotificationListHeaders()
        {
            try
            {
                lvwNotice.Visible = false;

                lvwNotice.Columns.Clear();

                lvwNotice.Columns.Add("Item", 40, HorizontalAlignment.Left);
                lvwNotice.Columns.Add("Mark", 50, HorizontalAlignment.Left);
                lvwNotice.Columns.Add("Date Time", 150, HorizontalAlignment.Left);
                lvwNotice.Columns.Add("Reference", 210, HorizontalAlignment.Left);
                lvwNotice.Columns.Add("Description", 390, HorizontalAlignment.Left);

                lvwNotice.Visible = true;
            }
            catch (Exception ex)
            {
            }
        }

        public void loadNotification(string pBatchCode)
        {
            string sSQL = "";
            try
            {
                _batchCode = pBatchCode;
                this.Text = this.Text + " " + _batchCode;

                lvwNotice.Items.Clear();

                mGlobal.LoadAppDBCfg();

                //Load batch.
                sSQL = "exec SPC_GET_NOTIFICATION ";
                sSQL += "'" + _currScanProj.Trim().Replace("'", "") + "',";
                sSQL += "'" + _appNum.Trim().Replace("'", "") + "',";
                sSQL += "'" + pBatchCode.Trim().Replace("'", "") + "'";

                DataRowCollection drs = mGlobal.objDB.ReturnRows(sSQL);

                if (drs != null)
                {
                    if (drs.Count > 0)
                    {
                        int i = 0;
                        while (i < drs.Count)
                        {

                            lvwNotice.Items.Add(drs[i][0].ToString());

                            lvwNotice.Items[i].SubItems.Add(drs[i][1].ToString());

                            lvwNotice.Items[i].SubItems.Add(drs[i][2].ToString());

                            lvwNotice.Items[i].SubItems.Add(drs[i][3].ToString());

                            lvwNotice.Items[i].SubItems.Add(drs[i][4].ToString());

                            i += 1;
                        }
                    }

                    drs = null;
                }

                if (lvwNotice.Items.Count > 0)
                    btnDeleteAllStrip.Enabled = true;
                else
                    btnDeleteAllStrip.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("load error!\n" + ex.Message, "Error");
            }

        }

        private void lvwNotice_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (lvwNotice.Items.Count > 1)
            {
                // Determine if clicked column is already the column that is being sorted.
                if (e.Column == lvwColSorter.SortColumn)
                {
                    // Reverse the current sort direction for this column.
                    if (lvwColSorter.Order == SortOrder.Ascending)
                    {
                        lvwColSorter.Order = SortOrder.Descending;
                    }
                    else
                    {
                        lvwColSorter.Order = SortOrder.Ascending;
                    }
                }
                else
                {
                    // Set the column number that is to be sorted; default to ascending.
                    lvwColSorter.SortColumn = e.Column;
                    lvwColSorter.Order = SortOrder.Ascending;
                }

                // Perform the sort with these new sort options.
                this.lvwNotice.Sort();
            }
        }

        private void frmNotification_Resize(object sender, EventArgs e)
        {
            if (lvwNotice.Width < this.Width)
                lvwNotice.Width = this.Width - 20;

            lvwNotice.Height = 275;
            this.Height = 360;
        }

        private void btnDeleteAllStrip_Click(object sender, EventArgs e)
        {
            try
            {
                bool bConfirm = false;

                if (lvwNotice.Items.Count == 0)
                    return;

                if (MessageBox.Show(this, "Confirm to delete ALL notification for batch " + _batchCode + "? Delete all will not able to revert!", 
                    "Delete All Notification Confirmation", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    bConfirm = true;

                if (bConfirm)
                {
                    bool bOK = deleteAllNotificationDB(_batchCode);

                    if (bOK)
                    {
                        MessageBox.Show("Notification delete successfully.");
                    }                        
                    else
                        MessageBox.Show("Notification delete unsuccessful.");

                    loadNotification(_batchCode); //refresh.
                }
                
            }
            catch (Exception ex)
            {
            }
        }

        private void frmNotification_FormClosed(object sender, FormClosedEventArgs e)
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
