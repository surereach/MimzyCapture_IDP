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
    public partial class frmProcSumm : Form
    {
        private string _currScanProj;
        private string _appNum;
        private string _batchType;

        private string sProcType;

        private clsDocuFile oDF = new clsDocuFile();

        private clsLvwColSorter lvwColSorter;

        private const int FORM_MIN_WIDTH = 1280;
        private const int FORM_MIN_HEIGHT = 720;
        private int iFormOffset = 15;

        public frmProcSumm()
        {
            InitializeComponent();
            try
            {
                Application.AddMessageFilter(new clsMouseFilterMess(this));
                staMain.sessionRestart();
            }
            catch (Exception ex)
            {
            }
        }

        private void frmProcSumm_Load(object sender, EventArgs e)
        {
            try
            {
                this.Text = this.Text + ": " + MDIMain.strScanProject;
                this.timer1.Enabled = true;

                this.ShowIcon = false;
                //this.MaximizeBox = false;
                //this.MinimizeBox = false;
                this.WindowState = FormWindowState.Maximized;
                this.ProcStatusStrip.Width = this.Width;
                //this.ProcStatusBar1.Width = this.ProcStatusStrip.Width - this.ProcStatusBar.Width - this.ProcStatusBar2.Width - this.CurrDateTime.Width;

                _currScanProj = MDIMain.strScanProject;
                _appNum = MDIMain.intAppNum.ToString("000");
                _batchType = MDIMain.sBatchType;

                sProcType = "";

                loadBatchProcessTreeview();
                if (tvwProcess.Nodes.Count > 0)
                    tvwProcess.ExpandAll();

                lvwColSorter = new clsLvwColSorter();
                lvwBatches.ListViewItemSorter = lvwColSorter;
                staMain.sessionRestart();
            }
            catch (Exception ex)
            {
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            CurrDateTime.Text = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt");
        }

        private void frmProcSumm_Paint(object sender, PaintEventArgs e)
        {
            if (System.Windows.Forms.Screen.AllScreens[0].Bounds.Width == FORM_MIN_WIDTH && System.Windows.Forms.Screen.AllScreens[0].Bounds.Height == FORM_MIN_HEIGHT)
            {
                iFormOffset = 5;
            }
            else
            {
                iFormOffset = 15;
            }
            formResize();
        }

        private void frmProcSumm_Resize(object sender, EventArgs e)
        {
            formResize();
        }

        private void formResize()
        {
            if (this.WindowState != FormWindowState.Minimized)
            {
                //MessageBox.Show(this.Width.ToString() + "," + this.ProcStatusStrip.Width.ToString());
                //this.ProcStatusStrip.Width = this.Width;
                //this.ProcStatusBar1.Width = this.ProcStatusBar1.Width + 135;
                if (this.WindowState == FormWindowState.Normal)
                {
                    if (this.Width < FORM_MIN_WIDTH) this.Width = FORM_MIN_WIDTH;
                    if (this.Height < FORM_MIN_HEIGHT) this.Height = FORM_MIN_HEIGHT;
                }

                tvwProcess.Height = this.Height - ProcToolStrip.Height - ProcStatusStrip.Height - 60;
                lvwBatches.Height = this.Height - ProcToolStrip.Height - ProcStatusStrip.Height - 60;
                lvwBatches.Width = this.Width - tvwProcess.Width - 25 - iFormOffset;

                this.ProcStatusBar2.Width = this.Width - this.ProcStatusBar.Width - this.ProcStatusBar1.Width - this.CurrDateTime.Width - iFormOffset;

                if (this.WindowState == FormWindowState.Normal && this.ProcStatusBar2.Width > 506)
                    this.ProcStatusBar2.Width = 506;
                else if (this.WindowState == FormWindowState.Maximized && this.ProcStatusBar2.Width > 506)
                {
                    if (this.Width > ProcToolStrip.Width)
                        this.ProcStatusBar2.Width = ProcToolStrip.Width - this.ProcStatusBar.Width - this.ProcStatusBar1.Width - this.CurrDateTime.Width - iFormOffset;
                }

                if (this.ProcStatusBar1.Width < 713)
                    this.ProcStatusBar1.Width = 713;
            }
        }

        public void loadBatchProcessTreeview()
        {
            string sSQL = "";
            string sTag = "";
            //string sNodeText = "";
            DataRow dr = null;
            TreeNode node = null;
            try
            {
                TreeNode root = new TreeNode("Process");
                root.Tag = new String("Process".ToCharArray());
                tvwProcess.Nodes.Add(root);

                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT IsNull(COUNT(DISTINCT batchcode),0) ";
                sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan ";
                sSQL += "WHERE scanproj='" + MDIMain.strScanProject.Trim().Replace("'", "") + "' ";
                sSQL += "AND appnum='" + MDIMain.intAppNum.ToString("000").Trim() + "' ";
                sSQL += "AND batchtype='" + MDIMain.sBatchType.Trim() + "' ";
                sSQL += "AND batchstatus IN ('1','8') ";

                if (staMain.validateUserRole("Scan Operator"))
                    sSQL += "AND scanuser='" + MDIMain.strUserID.Replace("'", "").Trim() + "' ";

                dr = mGlobal.objDB.ReturnSingleRow(sSQL);

                //Load Scanning batches.
                if (staMain.validateUserRole("Scan Operator") || staMain.validateUserRole("Index Executive"))
                {
                    if (dr != null)
                    {
                        sTag = "Scan:" + dr[0].ToString();
                        node = new TreeNode("Index [" + dr[0].ToString() + "]");
                        node.Tag = new String(sTag.ToCharArray());
                        tvwProcess.Nodes[0].Nodes.Add(node);
                    }
                    else
                    {
                        sTag = "Scan:" + 0.ToString();
                        node = new TreeNode("Index [0]");
                        node.Tag = new String(sTag.ToCharArray());
                        tvwProcess.Nodes[0].Nodes.Add(node);
                    }
                }

                //Load Indexed batches
                if (staMain.validateUserRole("Verify Executive"))
                {
                    dr = null;
                    sSQL = "SELECT IsNull(COUNT(DISTINCT batchcode),0) ";
                    sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan ";
                    sSQL += "WHERE scanproj='" + MDIMain.strScanProject.Trim().Replace("'", "") + "' ";
                    sSQL += "AND appnum='" + MDIMain.intAppNum.ToString("000").Trim() + "' ";
                    sSQL += "AND batchtype='" + MDIMain.sBatchType.Trim() + "' ";
                    sSQL += "AND batchstatus IN ('2','6') ";

                    dr = mGlobal.objDB.ReturnSingleRow(sSQL);

                    if (dr != null)
                    {
                        sTag = "Index:" + dr[0].ToString();
                        node = new TreeNode("Verify [" + dr[0].ToString() + "]");
                        node.Tag = new String(sTag.ToCharArray());
                        tvwProcess.Nodes[0].Nodes.Add(node);
                    }
                    else
                    {
                        sTag = "Index:" + 0.ToString();
                        node = new TreeNode("Verify [0]");
                        node.Tag = new String(sTag.ToCharArray());
                        tvwProcess.Nodes[0].Nodes.Add(node);
                    }
                }

                if (staMain.stcProjCfg.ExportEnable.Trim().ToUpper() == "Y" && staMain.validateUserRole("Index Executive"))
                {
                    //Load Exported batches
                    dr = null;
                    sSQL = "SELECT IsNull(COUNT(DISTINCT batchcode),0) ";
                    sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan ";
                    sSQL += "WHERE scanproj='" + MDIMain.strScanProject.Trim().Replace("'", "") + "' ";
                    sSQL += "AND appnum='" + MDIMain.intAppNum.ToString("000").Trim() + "' ";
                    sSQL += "AND batchtype='" + MDIMain.sBatchType.Trim() + "' ";
                    sSQL += "AND exportcnt > 0 ";
                    sSQL += "AND batchstatus NOT IN ('3') ";
                    sSQL += "AND scanuser='" + MDIMain.strUserID.Replace("'", "").Trim() + "' ";

                    dr = mGlobal.objDB.ReturnSingleRow(sSQL);

                    if (dr != null)
                    {
                        sTag = "Export:" + dr[0].ToString();
                        node = new TreeNode("Export [" + dr[0].ToString() + "]");
                        node.Tag = new String(sTag.ToCharArray());
                        tvwProcess.Nodes[0].Nodes.Add(node);
                    }
                    else
                    {
                        sTag = "Export:" + 0.ToString();
                        node = new TreeNode("Export [0]");
                        node.Tag = new String(sTag.ToCharArray());
                        tvwProcess.Nodes[0].Nodes.Add(node);
                    }
                }

                //Load Rescan batches
                if (staMain.stcProjCfg.RescanEnable.Trim().ToUpper() == "Y" && staMain.validateUserRole("Scan Operator"))
                {
                    dr = null;
                    sSQL = "SELECT IsNull(COUNT(DISTINCT batchcode),0) ";
                    sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan ";
                    sSQL += "WHERE scanproj='" + MDIMain.strScanProject.Trim().Replace("'", "") + "' ";
                    sSQL += "AND appnum='" + MDIMain.intAppNum.ToString("000").Trim() + "' ";
                    sSQL += "AND batchtype='" + MDIMain.sBatchType.Trim() + "' ";
                    sSQL += "AND batchstatus IN ('4') ";
                    sSQL += "AND scanuser='" + MDIMain.strUserID.Replace("'", "").Trim() + "' ";

                    dr = mGlobal.objDB.ReturnSingleRow(sSQL);

                    if (dr != null)
                    {
                        sTag = "Rescan:" + dr[0].ToString();
                        node = new TreeNode("Rescan [" + dr[0].ToString() + "]");
                        node.Tag = new String(sTag.ToCharArray());
                        tvwProcess.Nodes[0].Nodes.Add(node);
                    }
                    else
                    {
                        sTag = "Rescan:" + 0.ToString();
                        node = new TreeNode("Rescan [0]");
                        node.Tag = new String(sTag.ToCharArray());
                        tvwProcess.Nodes[0].Nodes.Add(node);
                    }
                }

                if (staMain.validateUserRole("Scan Operator"))
                {
                    //Load Scan\Rescan Deleted batches
                    dr = null;
                    sSQL = "SELECT IsNull(COUNT(DISTINCT batchcode),0) ";
                    sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan ";
                    sSQL += "WHERE scanproj='" + MDIMain.strScanProject.Trim().Replace("'", "") + "' ";
                    sSQL += "AND appnum='" + MDIMain.intAppNum.ToString("000").Trim() + "' ";
                    sSQL += "AND batchtype='" + MDIMain.sBatchType.Trim() + "' ";
                    sSQL += "AND batchstatus IN ('3') ";
                    sSQL += "AND scanuser='" + MDIMain.strUserID.Replace("'", "").Trim() + "' ";

                    dr = mGlobal.objDB.ReturnSingleRow(sSQL);

                    if (dr != null)
                    {
                        sTag = "Deleted:" + dr[0].ToString();
                        node = new TreeNode("Delete [" + dr[0].ToString() + "]");
                        node.Tag = new String(sTag.ToCharArray());
                        tvwProcess.Nodes[0].Nodes.Add(node);
                    }
                    else
                    {
                        sTag = "Deleted:" + 0.ToString();
                        node = new TreeNode("Delete [0]");
                        node.Tag = new String(sTag.ToCharArray());
                        tvwProcess.Nodes[0].Nodes.Add(node);
                    }
                }

                if (staMain.stcProjCfg.VerifyEnable.Trim().ToUpper() == "Y" && staMain.validateUserRole("Verify Executive"))
                {
                    //Load Verified batches
                    dr = null;
                    sSQL = "SELECT IsNull(COUNT(DISTINCT batchcode),0) ";
                    sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan ";
                    sSQL += "WHERE scanproj='" + MDIMain.strScanProject.Trim().Replace("'", "") + "' ";
                    sSQL += "AND appnum='" + MDIMain.intAppNum.ToString("000").Trim() + "' ";
                    sSQL += "AND batchtype='" + MDIMain.sBatchType.Trim() + "' ";
                    sSQL += "AND batchstatus IN ('5','6') "; //6 for exported.

                    dr = mGlobal.objDB.ReturnSingleRow(sSQL);

                    if (dr != null)
                    {
                        sTag = "Verify:" + dr[0].ToString();
                        node = new TreeNode("Verified [" + dr[0].ToString() + "]");
                        node.Tag = new String(sTag.ToCharArray());
                        tvwProcess.Nodes[0].Nodes.Add(node);
                    }
                    else
                    {
                        sTag = "Verify:" + 0.ToString();
                        node = new TreeNode("Verified [0]");
                        node.Tag = new String(sTag.ToCharArray());
                        tvwProcess.Nodes[0].Nodes.Add(node);
                    }
                }                

                dr = null;
            }
            catch (Exception ex)
            {
            }
        }

        //private void lvwBatches_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        //{
        //    e.Graphics.FillRectangle(Brushes.Gray, e.Bounds);
        //    e.DrawText();
        //}

        private void tvwProcess_AfterSelect(object sender, TreeViewEventArgs e)
        {
            string sNodeTag = "";
            string sNodeType = "";
            bool bRoot = false;
            try
            {
                sNodeTag = e.Node.Tag.ToString().Trim();             

                if (sNodeTag.ToLower() == "process")
                {
                    bRoot = true;
                    lvwBatches.Items.Clear();
                    ProcStatusBar2.Text = "Total Batches: 0";
                }
                else
                {
                    if (sNodeTag.Split(':').Length > 1)
                    {
                        sNodeType = sNodeTag.Split(':').GetValue(0).ToString().Trim();
                        sProcType = sNodeType;
                    }
                }

                if (!bRoot)
                {
                    string sStatus = "";

                    if (sNodeType.ToLower() == "scan")
                        sStatus = "'1'";
                    else if (sNodeType.ToLower() == "rescan")
                        sStatus = "'4'";
                    else if (sNodeType.ToLower() == "deleted")
                        sStatus = "'3'";
                    else if (sNodeType.ToLower() == "index")
                        sStatus = "'2'";
                    else if (sNodeType.ToLower() == "verify")
                        sStatus = "'5'";
                    else if (sNodeType.ToLower() == "export")
                        sStatus = "'6'";

                    lvwBatches.Items.Clear();
                    loadBatchProcessListHeaders(sStatus.Replace("'", ""));
                    loadBatchProcessList(sStatus);

                    ProcStatusBar.Text = e.Node.Text;
                }
                
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log("After select.." + ex.Message);
                mGlobal.Write2Log(ex.StackTrace.ToString());
            }
        }

        private void loadBatchProcessListHeaders(string sStatus)
        {
            try
            {
                lvwBatches.Visible = false;

                lvwBatches.Columns.Clear();

                if (sStatus.ToLower() == "2") //indexed.
                {
                    lvwBatches.Columns.Add("id", 0, HorizontalAlignment.Left);
                    lvwBatches.Columns.Add("Batch", 200, HorizontalAlignment.Left);
                    lvwBatches.Columns.Add("Total Page", 80, HorizontalAlignment.Center);
                    lvwBatches.Columns.Add("Index By", 100, HorizontalAlignment.Left);
                    lvwBatches.Columns.Add("Index Date", 150, HorizontalAlignment.Center);
                    lvwBatches.Columns.Add("Created Date", 150, HorizontalAlignment.Center);
                    lvwBatches.Columns.Add("Modified Date", 150, HorizontalAlignment.Center);
                }
                else if (sStatus.ToLower() == "5") //verified.
                {
                    lvwBatches.Columns.Add("id", 0, HorizontalAlignment.Left);
                    lvwBatches.Columns.Add("Batch", 200, HorizontalAlignment.Left);
                    lvwBatches.Columns.Add("Total Page", 80, HorizontalAlignment.Center);
                    lvwBatches.Columns.Add("Verify By", 100, HorizontalAlignment.Left);
                    lvwBatches.Columns.Add("Verify Date", 150, HorizontalAlignment.Center);
                    lvwBatches.Columns.Add("Created Date", 150, HorizontalAlignment.Center);
                    lvwBatches.Columns.Add("Modified Date", 150, HorizontalAlignment.Center);
                }
                else
                {
                    lvwBatches.Columns.Add("id", 0, HorizontalAlignment.Left);
                    lvwBatches.Columns.Add("Batch", 200, HorizontalAlignment.Left);
                    lvwBatches.Columns.Add("Total Page", 80, HorizontalAlignment.Center);
                    lvwBatches.Columns.Add("Scan Start", 150, HorizontalAlignment.Center);
                    lvwBatches.Columns.Add("Scan End", 150, HorizontalAlignment.Center);
                    lvwBatches.Columns.Add("From", 80, HorizontalAlignment.Center);
                    lvwBatches.Columns.Add("Created Date", 150, HorizontalAlignment.Center);
                    lvwBatches.Columns.Add("Modified Date", 150, HorizontalAlignment.Center);
                }

                lvwBatches.Visible = true;

            }
            catch (Exception ex)
            {
            }
        }

        private long loadBatchProcessList(string sStatusIn)
        {
            string sSQL = "";
            bool bDefault = false;
            int i = 0;
            try
            {
                mGlobal.LoadAppDBCfg();

                if (sStatusIn.ToLower().Replace("'", "") == "2") //indexed.
                {
                    //sSQL = "SELECT rowid,batchcode,totpagecnt,indexby,";
                    //sSQL += "IsNull(Convert(VARCHAR,indexdate),''),";
                    //sSQL +="createddate,IsNull(Convert(VARCHAR,modifieddate),'') ";
                    //sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan a ";
                    //sSQL += "WHERE scanproj='" + MDIMain.strScanProject.Trim().Replace("'", "") + "' ";
                    //sSQL += "AND appnum='" + _appNum.Trim() + "' ";
                    //sSQL += "AND batchtype='" + _batchType.Trim() + "' ";
                    //sSQL += "AND batchstatus IN (" + sStatusIn.Trim() + ") ";
                    if (_batchType.Trim().ToLower() == "set")
                    {
                        sSQL = "SELECT DISTINCT * FROM (";
                        sSQL += "select(select top 1 rowid from " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan b where b.scanproj=a.scanproj and b.appnum=a.appnum and b.batchcode = a.batchcode and b.batchstatus=a.batchstatus order by rowid) rowid,";
                        sSQL += "(select top 1 batchcode from " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan b where b.scanproj=a.scanproj and b.appnum=a.appnum and b.batchcode = a.batchcode and b.batchstatus=a.batchstatus order by rowid) batchcode,";
                        sSQL += "(select SUM(totpagecnt) from " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan b where b.scanproj=a.scanproj and b.appnum=a.appnum and b.batchcode = a.batchcode and b.batchstatus=a.batchstatus) totpagecnt,";
                        sSQL += "(select top 1 indexby from " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan b where b.scanproj=a.scanproj and b.appnum=a.appnum and b.batchcode = a.batchcode and b.batchstatus=a.batchstatus order by rowid) indexby,";
                        sSQL += "(select top 1 indexdate from " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan b where b.scanproj=a.scanproj and b.appnum=a.appnum and b.batchcode = a.batchcode and b.batchstatus=a.batchstatus order by rowid) indexdate,";
                        sSQL += "(select top 1 fromproc from " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan b where b.scanproj=a.scanproj and b.appnum=a.appnum and b.batchcode = a.batchcode and b.batchstatus=a.batchstatus order by rowid) frompro,";
                        sSQL += "(select top 1 createddate from " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan b where b.scanproj=a.scanproj and b.appnum=a.appnum and b.batchcode = a.batchcode and b.batchstatus=a.batchstatus order by rowid) createddate,";
                        sSQL += "(select top 1 IsNull(Convert(VARCHAR,modifieddate),'') from " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan b where b.scanproj=a.scanproj and b.appnum=a.appnum and b.batchcode = a.batchcode and b.batchstatus=a.batchstatus order by rowid) modifieddate ";
                        sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan a ";
                        sSQL += "WHERE scanproj='" + MDIMain.strScanProject.Trim().Replace("'", "") + "' ";
                        sSQL += "AND appnum='" + _appNum.Trim() + "' ";
                        sSQL += "AND batchtype='" + _batchType.Trim() + "' ";
                        sSQL += "AND batchstatus IN (" + sStatusIn.Trim() + ",'6') ) d ";
                    }
                    else
                    {
                        sSQL = "SELECT DISTINCT * FROM (";
                        sSQL += "select(select top 1 rowid from " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan b where b.scanproj=a.scanproj and b.appnum=a.appnum and b.batchcode = a.batchcode and b.batchstatus=a.batchstatus order by rowid) rowid,";
                        sSQL += "batchcode, totpagecnt,";
                        sSQL += "(select top 1 indexby from " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan b where b.scanproj=a.scanproj and b.appnum=a.appnum and b.batchcode = a.batchcode and b.batchstatus=a.batchstatus order by rowid) indexby,";
                        sSQL += "(select top 1 indexdate from " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan b where b.scanproj=a.scanproj and b.appnum=a.appnum and b.batchcode = a.batchcode and b.batchstatus=a.batchstatus order by rowid) indexdate,";
                        sSQL += "(select top 1 fromproc from " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan b where b.scanproj=a.scanproj and b.appnum=a.appnum and b.batchcode = a.batchcode and b.batchstatus=a.batchstatus order by rowid) frompro,";
                        sSQL += "(select top 1 createddate from " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan b where b.scanproj=a.scanproj and b.appnum=a.appnum and b.batchcode = a.batchcode and b.batchstatus=a.batchstatus order by rowid) createddate,";
                        sSQL += "(select top 1 IsNull(Convert(VARCHAR,modifieddate),'') from " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan b where b.scanproj=a.scanproj and b.appnum=a.appnum and b.batchcode = a.batchcode and b.batchstatus=a.batchstatus order by rowid) modifieddate ";
                        sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan a ";
                        sSQL += "WHERE scanproj='" + MDIMain.strScanProject.Trim().Replace("'", "") + "' ";
                        sSQL += "AND appnum='" + _appNum.Trim() + "' ";
                        sSQL += "AND batchtype='" + _batchType.Trim() + "' ";
                        sSQL += "AND batchstatus IN (" + sStatusIn.Trim() + ",'6') ) d ";
                    }
                }
                else if (sStatusIn.ToLower().Replace("'", "") == "5") //verified.
                {
                    //sSQL = "SELECT rowid,batchcode,totpagecnt,verifyby,";
                    //sSQL += "IsNull(Convert(VARCHAR,verifydate),''),";
                    //sSQL += "createddate,IsNull(Convert(VARCHAR,modifieddate),'') ";
                    //sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan a ";
                    //sSQL += "WHERE scanproj='" + MDIMain.strScanProject.Trim().Replace("'", "") + "' ";
                    //sSQL += "AND appnum='" + _appNum.Trim() + "' ";
                    //sSQL += "AND batchtype='" + _batchType.Trim() + "' ";
                    //sSQL += "AND batchstatus IN (" + sStatusIn.Trim() + ") ";
                    if (_batchType.Trim().ToLower() == "set")
                    {
                        sSQL = "SELECT DISTINCT * FROM (";
                        sSQL += "select(select top 1 rowid from " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan b where b.scanproj=a.scanproj and b.appnum=a.appnum and b.batchcode = a.batchcode and b.batchstatus=a.batchstatus order by rowid) rowid,";
                        sSQL += "(select top 1 batchcode from " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan b where b.scanproj=a.scanproj and b.appnum=a.appnum and b.batchcode = a.batchcode and b.batchstatus=a.batchstatus order by rowid) batchcode,";
                        sSQL += "(select SUM(totpagecnt) from " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan b where b.scanproj=a.scanproj and b.appnum=a.appnum and b.batchcode = a.batchcode and b.batchstatus=a.batchstatus) totpagecnt,";
                        sSQL += "(select top 1 verifyby from " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan b where b.scanproj=a.scanproj and b.appnum=a.appnum and b.batchcode = a.batchcode and b.batchstatus=a.batchstatus order by rowid) verifyby,";
                        sSQL += "(select top 1 IsNull(Convert(VARCHAR,verifydate),'') from " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan b where b.scanproj=a.scanproj and b.appnum=a.appnum and b.batchcode = a.batchcode and b.batchstatus=a.batchstatus order by rowid) verifydate,";
                        sSQL += "(select top 1 fromproc from " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan b where b.scanproj=a.scanproj and b.appnum=a.appnum and b.batchcode = a.batchcode and b.batchstatus=a.batchstatus order by rowid) frompro,";
                        sSQL += "(select top 1 createddate from " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan b where b.scanproj=a.scanproj and b.appnum=a.appnum and b.batchcode = a.batchcode and b.batchstatus=a.batchstatus order by rowid) createddate,";
                        sSQL += "(select top 1 IsNull(Convert(VARCHAR,modifieddate),'') from " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan b where b.scanproj=a.scanproj and b.appnum=a.appnum and b.batchcode = a.batchcode and b.batchstatus=a.batchstatus order by rowid) modifieddate ";
                        sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan a ";
                        sSQL += "WHERE scanproj='" + MDIMain.strScanProject.Trim().Replace("'", "") + "' ";
                        sSQL += "AND appnum='" + _appNum.Trim() + "' ";
                        sSQL += "AND batchtype='" + _batchType.Trim() + "' ";
                        sSQL += "AND batchstatus IN (" + sStatusIn.Trim() + ",'6') ) d ";
                    }
                    else
                    {
                        sSQL = "SELECT DISTINCT * FROM (";
                        sSQL += "select(select top 1 rowid from " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan b where b.scanproj=a.scanproj and b.appnum=a.appnum and b.batchcode = a.batchcode and b.batchstatus=a.batchstatus order by rowid) rowid,";
                        sSQL += "batchcode, totpagecnt,";
                        sSQL += "(select top 1 verifyby from " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan b where b.scanproj=a.scanproj and b.appnum=a.appnum and b.batchcode = a.batchcode and b.batchstatus=a.batchstatus order by rowid) verifyby,";
                        sSQL += "(select top 1 IsNull(Convert(VARCHAR,verifydate),'') from " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan b where b.scanproj=a.scanproj and b.appnum=a.appnum and b.batchcode = a.batchcode and b.batchstatus=a.batchstatus order by rowid) verifydate,";
                        sSQL += "(select top 1 fromproc from " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan b where b.scanproj=a.scanproj and b.appnum=a.appnum and b.batchcode = a.batchcode and b.batchstatus=a.batchstatus order by rowid) frompro,";
                        sSQL += "(select top 1 createddate from " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan b where b.scanproj=a.scanproj and b.appnum=a.appnum and b.batchcode = a.batchcode and b.batchstatus=a.batchstatus order by rowid) createddate,";
                        sSQL += "(select top 1 IsNull(Convert(VARCHAR,modifieddate),'') from " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan b where b.scanproj=a.scanproj and b.appnum=a.appnum and b.batchcode = a.batchcode and b.batchstatus=a.batchstatus order by rowid) modifieddate ";
                        sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan a ";
                        sSQL += "WHERE scanproj='" + MDIMain.strScanProject.Trim().Replace("'", "") + "' ";
                        sSQL += "AND appnum='" + _appNum.Trim() + "' ";
                        sSQL += "AND batchtype='" + _batchType.Trim() + "' ";
                        sSQL += "AND batchstatus IN (" + sStatusIn.Trim() + ",'6') ) d ";
                    }
                }
                else if (sStatusIn.ToLower().Replace("'", "") == "3") //Rescan deleted.
                {
                    //sSQL = "SELECT DISTINCT * FROM (";
                    //sSQL += "select(select top 1 rowid from " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan b where b.scanproj=a.scanproj and b.appnum=a.appnum and b.batchcode = a.batchcode and b.batchstatus=a.batchstatus order by setnum DESC,rowid) rowid,";
                    //sSQL += "batchcode, totpagecnt,";
                    //sSQL += "(select top 1 scanstart from " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan b where b.scanproj=a.scanproj and b.appnum=a.appnum and b.batchcode = a.batchcode and b.batchstatus=a.batchstatus order by setnum DESC,rowid) scanstart,";
                    //sSQL += "(select top 1 scanend from " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan b where b.scanproj=a.scanproj and b.appnum=a.appnum and b.batchcode = a.batchcode and b.batchstatus=a.batchstatus order by setnum DESC,rowid) scanend,";
                    //sSQL += "(select top 1 createddate from " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan b where b.scanproj=a.scanproj and b.appnum=a.appnum and b.batchcode = a.batchcode and b.batchstatus=a.batchstatus order by setnum DESC,rowid) createddate,";
                    //sSQL += "(select top 1 IsNull(Convert(VARCHAR,modifieddate),'') from " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan b where b.scanproj=a.scanproj and b.appnum=a.appnum and b.batchcode = a.batchcode and b.batchstatus=a.batchstatus order by setnum DESC,rowid) modifieddate ";
                    //sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan a ";
                    //sSQL += "WHERE scanproj='" + MDIMain.strScanProject.Trim().Replace("'", "") + "' ";
                    //sSQL += "AND appnum='" + _appNum.Trim() + "' ";
                    //sSQL += "AND batchtype='" + _batchType.Trim() + "' ";
                    //sSQL += "AND batchstatus IN (" + sStatusIn.Trim() + ") ) d ";
                    if (_batchType.Trim().ToLower() == "set")
                    {
                        sSQL = "SELECT DISTINCT * FROM (";
                        sSQL += "select(select top 1 rowid from " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan b where b.scanproj=a.scanproj and b.appnum=a.appnum and b.batchcode = a.batchcode and b.batchstatus=a.batchstatus and b.scanuser=a.scanuser order by setnum DESC,rowid) rowid,";
                        sSQL += "(select top 1 batchcode from " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan b where b.scanproj=a.scanproj and b.appnum=a.appnum and b.batchcode = a.batchcode and b.batchstatus=a.batchstatus and b.scanuser=a.scanuser order by setnum DESC,rowid) batchcode,";
                        sSQL += "(select SUM(totpagecnt) from " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan b where b.scanproj=a.scanproj and b.appnum=a.appnum and b.batchcode = a.batchcode and b.batchstatus=a.batchstatus and b.scanuser=a.scanuser) totpagecnt,";
                        sSQL += "(select top 1 scanstart from " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan b where b.scanproj=a.scanproj and b.appnum=a.appnum and b.batchcode = a.batchcode and b.batchstatus=a.batchstatus and b.scanuser=a.scanuser order by setnum DESC,rowid) scanstart,";
                        sSQL += "(select top 1 scanend from " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan b where b.scanproj=a.scanproj and b.appnum=a.appnum and b.batchcode = a.batchcode and b.batchstatus=a.batchstatus and b.scanuser=a.scanuser order by setnum DESC,rowid) scanend,";
                        sSQL += "(select top 1 fromproc from " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan b where b.scanproj=a.scanproj and b.appnum=a.appnum and b.batchcode = a.batchcode and b.batchstatus=a.batchstatus and b.scanuser=a.scanuser order by setnum DESC,rowid) frompro,";
                        sSQL += "(select top 1 createddate from " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan b where b.scanproj=a.scanproj and b.appnum=a.appnum and b.batchcode = a.batchcode and b.batchstatus=a.batchstatus and b.scanuser=a.scanuser order by setnum DESC,rowid) createddate,";
                        sSQL += "(select top 1 IsNull(Convert(VARCHAR,modifieddate),'') from " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan b where b.scanproj=a.scanproj and b.appnum=a.appnum and b.batchcode = a.batchcode and b.batchstatus=a.batchstatus and b.scanuser=a.scanuser order by setnum DESC,rowid) modifieddate ";
                        sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan a ";
                        sSQL += "WHERE scanproj='" + MDIMain.strScanProject.Trim().Replace("'", "") + "' ";
                        sSQL += "AND appnum='" + _appNum.Trim() + "' ";
                        sSQL += "AND batchtype='" + _batchType.Trim() + "' ";
                        sSQL += "AND scanuser='" + MDIMain.strUserID.Replace("'", "").Trim() + "' ";
                        sSQL += "AND batchstatus IN (" + sStatusIn.Trim() + ") ) d ";
                    }
                    else
                    {
                        sSQL = "SELECT DISTINCT * FROM (";
                        sSQL += "select(select top 1 rowid from " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan b where b.scanproj=a.scanproj and b.appnum=a.appnum and b.batchcode = a.batchcode and b.batchstatus=a.batchstatus and b.scanuser=a.scanuser order by setnum DESC,rowid) rowid,";
                        sSQL += "batchcode, totpagecnt,";
                        sSQL += "(select top 1 scanstart from " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan b where b.scanproj=a.scanproj and b.appnum=a.appnum and b.batchcode = a.batchcode and b.batchstatus=a.batchstatus and b.scanuser=a.scanuser order by setnum DESC,rowid) scanstart,";
                        sSQL += "(select top 1 scanend from " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan b where b.scanproj=a.scanproj and b.appnum=a.appnum and b.batchcode = a.batchcode and b.batchstatus=a.batchstatus and b.scanuser=a.scanuser order by setnum DESC,rowid) scanend,";
                        sSQL += "(select top 1 fromproc from " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan b where b.scanproj=a.scanproj and b.appnum=a.appnum and b.batchcode = a.batchcode and b.batchstatus=a.batchstatus and b.scanuser=a.scanuser order by setnum DESC,rowid) frompro,";
                        sSQL += "(select top 1 createddate from " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan b where b.scanproj=a.scanproj and b.appnum=a.appnum and b.batchcode = a.batchcode and b.batchstatus=a.batchstatus and b.scanuser=a.scanuser order by setnum DESC,rowid) createddate,";
                        sSQL += "(select top 1 IsNull(Convert(VARCHAR,modifieddate),'') from " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan b where b.scanproj=a.scanproj and b.appnum=a.appnum and b.batchcode = a.batchcode and b.batchstatus=a.batchstatus and b.scanuser=a.scanuser order by setnum DESC,rowid) modifieddate ";
                        sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan a ";
                        sSQL += "WHERE scanproj='" + MDIMain.strScanProject.Trim().Replace("'", "") + "' ";
                        sSQL += "AND appnum='" + _appNum.Trim() + "' ";
                        sSQL += "AND batchtype='" + _batchType.Trim() + "' ";
                        sSQL += "AND scanuser='" + MDIMain.strUserID.Replace("'", "").Trim() + "' ";
                        sSQL += "AND batchstatus IN (" + sStatusIn.Trim() + ") ) d ";
                    }
                }
                else
                {
                    //sSQL = "SELECT rowid,batchcode,totpagecnt,scanstart,scanend,createddate,IsNull(Convert(VARCHAR,modifieddate),'') ";
                    //sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan ";
                    //sSQL += "WHERE scanproj='" + MDIMain.strScanProject.Trim().Replace("'", "") + "' ";
                    //sSQL += "AND appnum='" + _appNum.Trim() + "' ";
                    //sSQL += "AND batchtype='" + _batchType.Trim() + "' ";
                    //sSQL += "AND batchstatus IN (" + sStatusIn.Trim() + ") ";
                    if (_batchType.Trim().ToLower() == "set")
                    {
                        sSQL = "SELECT DISTINCT * FROM (";
                        sSQL += "select (select top 1 rowid from " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan b where b.scanproj=a.scanproj and b.appnum=a.appnum and b.batchcode = a.batchcode and b.batchstatus=a.batchstatus and b.scanuser=a.scanuser order by setnum DESC,rowid) rowid,";
                        sSQL += "(select top 1 batchcode from " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan b where b.scanproj=a.scanproj and b.appnum=a.appnum and b.batchcode = a.batchcode and b.batchstatus=a.batchstatus and b.scanuser=a.scanuser order by setnum DESC,rowid) batchcode,";
                        sSQL += "(select SUM(totpagecnt) from " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan b where b.scanproj=a.scanproj and b.appnum=a.appnum and b.batchcode = a.batchcode and b.batchstatus=a.batchstatus and b.scanuser=a.scanuser) totpagecnt,";
                        sSQL += "(select top 1 scanstart from " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan b where b.scanproj=a.scanproj and b.appnum=a.appnum and b.batchcode = a.batchcode and b.batchstatus=a.batchstatus and b.scanuser=a.scanuser order by setnum DESC,rowid) scanstart,";
                        sSQL += "(select top 1 scanend from " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan b where b.scanproj=a.scanproj and b.appnum=a.appnum and b.batchcode = a.batchcode and b.batchstatus=a.batchstatus and b.scanuser=a.scanuser order by setnum DESC,rowid) scanend,";
                        sSQL += "(select top 1 fromproc from " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan b where b.scanproj=a.scanproj and b.appnum=a.appnum and b.batchcode = a.batchcode and b.batchstatus=a.batchstatus and b.scanuser=a.scanuser order by setnum DESC,rowid) frompro,";
                        sSQL += "(select top 1 createddate from " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan b where b.scanproj=a.scanproj and b.appnum=a.appnum and b.batchcode = a.batchcode and b.batchstatus=a.batchstatus and b.scanuser=a.scanuser order by setnum DESC,rowid) createddate,";
                        sSQL += "(select top 1 IsNull(Convert(VARCHAR,modifieddate),'') from " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan b where b.scanproj=a.scanproj and b.appnum=a.appnum and b.batchcode = a.batchcode and b.batchstatus=a.batchstatus and b.scanuser=a.scanuser order by setnum DESC,rowid) modifieddate ";
                        sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan a ";
                        sSQL += "WHERE scanproj='" + MDIMain.strScanProject.Trim().Replace("'", "") + "' ";
                        sSQL += "AND appnum='" + _appNum.Trim() + "' ";
                        sSQL += "AND batchtype='" + _batchType.Trim() + "' ";
                    }
                    else
                    {
                        sSQL = "SELECT DISTINCT * FROM (";
                        sSQL += "select(select top 1 rowid from " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan b where b.scanproj=a.scanproj and b.appnum=a.appnum and b.batchcode = a.batchcode and b.batchstatus=a.batchstatus and b.scanuser=a.scanuser order by setnum DESC,rowid) rowid,";
                        sSQL += "batchcode, totpagecnt,";
                        sSQL += "(select top 1 scanstart from " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan b where b.scanproj=a.scanproj and b.appnum=a.appnum and b.batchcode = a.batchcode and b.batchstatus=a.batchstatus and b.scanuser=a.scanuser order by setnum DESC,rowid) scanstart,";
                        sSQL += "(select top 1 scanend from " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan b where b.scanproj=a.scanproj and b.appnum=a.appnum and b.batchcode = a.batchcode and b.batchstatus=a.batchstatus and b.scanuser=a.scanuser order by setnum DESC,rowid) scanend,";
                        sSQL += "(select top 1 fromproc from " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan b where b.scanproj=a.scanproj and b.appnum=a.appnum and b.batchcode = a.batchcode and b.batchstatus=a.batchstatus and b.scanuser=a.scanuser order by setnum DESC,rowid) frompro,";
                        sSQL += "(select top 1 createddate from " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan b where b.scanproj=a.scanproj and b.appnum=a.appnum and b.batchcode = a.batchcode and b.batchstatus=a.batchstatus and b.scanuser=a.scanuser order by setnum DESC,rowid) createddate,";
                        sSQL += "(select top 1 IsNull(Convert(VARCHAR,modifieddate),'') from " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan b where b.scanproj=a.scanproj and b.appnum=a.appnum and b.batchcode = a.batchcode and b.batchstatus=a.batchstatus and b.scanuser=a.scanuser order by setnum DESC,rowid) modifieddate ";
                        sSQL += "FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TBatchScan a ";
                        sSQL += "WHERE scanproj='" + MDIMain.strScanProject.Trim().Replace("'", "") + "' ";
                        sSQL += "AND appnum='" + _appNum.Trim() + "' ";
                        sSQL += "AND batchtype='" + _batchType.Trim() + "' ";
                    }

                    if (staMain.validateUserRole("Scan Operator"))
                        sSQL += "AND scanuser='" + MDIMain.strUserID.Replace("'", "").Trim() + "' ";

                    if (sStatusIn.ToLower().Replace("'", "") == "6") //exported.
                        sSQL += "AND exportcnt > 0) d ";
                    else if (sStatusIn.ToLower().Replace("'", "") == "1") //Include request for reindex status.
                        sSQL += "AND batchstatus IN (" + sStatusIn.Trim() + ",'8')) d ";
                    else
                        sSQL += "AND batchstatus IN (" + sStatusIn.Trim() + ")) d ";

                    bDefault = true;
                }                

                if (sStatusIn.ToLower().Replace("'", "") == "2") //indexed.
                {
                    sSQL += "ORDER BY indexdate DESC ";
                }
                else
                    sSQL += "ORDER BY createddate DESC ";

                DataRowCollection drs = mGlobal.objDB.ReturnRows(sSQL);

                ProcStatusBar2.Text = "Total Batches: 0";

                if (drs != null)
                {
                    if (drs.Count > 0)
                    {                           
                        while (i < drs.Count)
                        {                      
                            lvwBatches.Items.Add(drs[i][0].ToString());

                            lvwBatches.Items[i].SubItems.Add(drs[i][1].ToString());

                            lvwBatches.Items[i].SubItems.Add(drs[i][2].ToString());

                            if (bDefault)
                            {
                                lvwBatches.Items[i].SubItems.Add(Convert.ToDateTime(drs[i][3].ToString()).ToString("dd/MM/yyyy HH:mm:ss"));
                            }
                            else
                            {
                                lvwBatches.Items[i].SubItems.Add(drs[i][3].ToString());                                
                            }

                            if (bDefault)
                            {
                                lvwBatches.Items[i].SubItems.Add(Convert.ToDateTime(drs[i][4].ToString()).ToString("dd/MM/yyyy HH:mm:ss"));
                            }
                            else
                            {
                                if (drs[i][4].ToString() != "")
                                    lvwBatches.Items[i].SubItems.Add(Convert.ToDateTime(drs[i][4].ToString()).ToString("dd/MM/yyyy HH:mm:ss"));
                                else
                                    lvwBatches.Items[i].SubItems.Add("");
                            }

                            lvwBatches.Items[i].SubItems.Add(drs[i][5].ToString());

                            lvwBatches.Items[i].SubItems.Add(Convert.ToDateTime(drs[i][6].ToString()).ToString("dd/MM/yyyy HH:mm:ss"));

                            if (drs[i][7].ToString() != "")
                                lvwBatches.Items[i].SubItems.Add(Convert.ToDateTime(drs[i][7].ToString()).ToString("dd/MM/yyyy HH:mm:ss"));
                            else
                                lvwBatches.Items[i].SubItems.Add("");

                            i += 1;
                        }

                        ProcStatusBar2.Text = "Total Batches: " + i.ToString();
                    }
                }

                drs = null;
            }
            catch (Exception ex)
            {
            }

            return i;
        }

        private void btnProcRefreshStrip_Click(object sender, EventArgs e)
        {
            string sNodeType = "";

            try
            {
                lvwColSorter = new clsLvwColSorter();
                lvwBatches.ListViewItemSorter = lvwColSorter;

                if (tvwProcess.Nodes.Count > 0)
                {
                    tvwProcess.Focus();
                }

                if (tvwProcess.SelectedNode != null)
                {
                    if (tvwProcess.SelectedNode.Tag != null)
                    {
                        sNodeType = tvwProcess.SelectedNode.Tag.ToString().Trim();
                    }
                }                

                if (sNodeType.Trim().ToLower() != "process" && sNodeType.Trim() != "")
                {
                    if (sNodeType.Split(':').Length > 1)
                        sNodeType = sNodeType.Split(':').GetValue(0).ToString().Trim();

                    string sStatus = "";

                    if (sNodeType.ToLower() == "scan")
                        sStatus = "'1'";
                    else if (sNodeType.ToLower() == "rescan")
                        sStatus = "'4'";
                    else if (sNodeType.ToLower() == "deleted")
                        sStatus = "'3'";
                    else if (sNodeType.ToLower() == "index")
                        sStatus = "'2'";
                    else if (sNodeType.ToLower() == "verify")
                        sStatus = "'5'";
                    else if (sNodeType.ToLower() == "export")
                        sStatus = "'6'";

                    lvwBatches.Items.Clear();
                    loadBatchProcessListHeaders(sStatus.Replace("'", ""));
                    long lCnt = loadBatchProcessList(sStatus);

                    tvwProcess.SelectedNode.Tag = sNodeType + ":" + lCnt.ToString();
                    tvwProcess.SelectedNode.Text = tvwProcess.SelectedNode.Text.Split(' ').GetValue(0).ToString() + " [" + lCnt.ToString() + "]";
                }
                else
                {
                    tvwProcess.Nodes.Clear();
                    loadBatchProcessTreeview();
                    if (tvwProcess.Nodes.Count > 0)
                        tvwProcess.ExpandAll();
                }
            }
            catch (Exception ex)
            {
            }           
            
        }

        private void lvwBatches_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            long lRowid = 0;
            try
            {
                if (sProcType.Trim().ToLower() == "scan")
                {
                    lRowid = Convert.ToInt64(lvwBatches.SelectedItems[0].Text.Trim());

                    if (staMain.stcProjCfg.NoSeparator.ToUpper() == "C")
                    {
                        if (mGlobal.checkOpenedForms("frmExtraction"))
                            MessageBox.Show("The IDP Extraction screen already opened!", "Message");
                        else
                        {
                            frmExtraction ofExtract = new frmExtraction();
                            ofExtract.ShowIcon = false;
                            //ofExtract.MaximizeBox = false;
                            //ofExtract.MinimizeBox = false;

                            ofExtract.MdiParent = this.MdiParent;
                            //this.MdiChildren[1].MaximizeBox = false;
                            //this.MdiChildren[1].MinimizeBox = false;                       

                            if (this.MdiParent != null)
                                this.MdiParent.Controls["toolStrip"].Controls[0].Enabled = false;

                            ofExtract.bSingleView = true;

                            ofExtract.Show();
                            ofExtract.reloadForCurrRow(lRowid);
                        }
                    }
                    else if (staMain.stcProjCfg.IndexEnable.ToUpper() == "Y")
                    {                        
                        if (mGlobal.checkOpenedForms("frmIndexing"))
                            MessageBox.Show("The Index screen already opened!", "Message");
                        else
                        {
                            frmIndexing ofIndexing = new frmIndexing();
                            ofIndexing.ShowIcon = false;
                            //ofIndexing.MaximizeBox = false;
                            //ofIndexing.MinimizeBox = false;

                            ofIndexing.MdiParent = this.MdiParent;
                            //this.MdiChildren[1].MaximizeBox = false;
                            //this.MdiChildren[1].MinimizeBox = false;                       

                            if (this.MdiParent != null)
                                this.MdiParent.Controls["toolStrip"].Controls[0].Enabled = false;

                            ofIndexing.bSingleView = true;

                            ofIndexing.Show();
                            ofIndexing.reloadForCurrRow(lRowid);
                        }
                    }
                    else
                    {
                        if (mGlobal.checkOpenedForms("frmVerifyScan"))
                            MessageBox.Show("The Scan Verify screen already opened!", "Message");
                        else
                        {
                            frmVerifyScan ofVerify = new frmVerifyScan();
                            ofVerify.ShowIcon = false;
                            //ofVerify.MaximizeBox = false;
                            //ofVerify.MinimizeBox = false;

                            ofVerify.MdiParent = this.MdiParent;
                            //this.MdiChildren[1].MaximizeBox = false;
                            //this.MdiChildren[1].MinimizeBox = false;                       

                            if (this.MdiParent != null)
                                this.MdiParent.Controls["toolStrip"].Controls[0].Enabled = false;

                            ofVerify.bSingleView = true;

                            ofVerify.Show();
                            ofVerify.reloadForCurrRow(lRowid, true);
                        }
                    }
                }
                else if ((sProcType.Trim().ToLower() == "rescan" || sProcType.Trim().ToLower() == "deleted") && staMain.stcProjCfg.RescanEnable.ToUpper() == "Y")
                {
                    lRowid = Convert.ToInt64(lvwBatches.SelectedItems[0].Text.Trim());

                    if (staMain.stcProjCfg.BatchType.Trim() == "Box" && mGlobal.checkOpenedForms("frmRescanBox1"))
                        MessageBox.Show("The Box Rescan screen already opened!", "Message");
                    else if (staMain.stcProjCfg.BatchType.Trim() != "Box" && mGlobal.checkOpenedForms("frmRescan1"))
                        MessageBox.Show("The Rescan screen already opened!", "Message");
                    else
                    {
                        if (staMain.stcProjCfg.BatchType.Trim() == "Box")
                        {
                            frmRescanBox1 ofRescan = new frmRescanBox1();
                            ofRescan.ShowIcon = false;
                            //ofRescan.MaximizeBox = false;
                            //ofRescan.MinimizeBox = false;

                            ofRescan.MdiParent = this.MdiParent;
                            //this.MdiChildren[1].MaximizeBox = false;
                            //this.MdiChildren[1].MinimizeBox = false;                       

                            if (this.MdiParent != null)
                                this.MdiParent.Controls["toolStrip"].Controls[0].Enabled = false;

                            string sBatchCode = lvwBatches.SelectedItems[0].SubItems[1].Text.Trim();
                            ofRescan.Show();
                            ofRescan.reloadForCurrRow(lRowid, sBatchCode);
                        }
                        else
                        {
                            frmRescan1 ofRescan = new frmRescan1();
                            ofRescan.ShowIcon = false;
                            //ofRescan.MaximizeBox = false;
                            //ofRescan.MinimizeBox = false;

                            ofRescan.MdiParent = this.MdiParent;
                            //this.MdiChildren[1].MaximizeBox = false;
                            //this.MdiChildren[1].MinimizeBox = false;                       

                            if (this.MdiParent != null)
                                this.MdiParent.Controls["toolStrip"].Controls[0].Enabled = false;

                            string sBatchCode = lvwBatches.SelectedItems[0].SubItems[1].Text.Trim();
                            ofRescan.Show();
                            ofRescan.reloadForCurrRow(lRowid, sBatchCode);
                        }
                    }
                }
                else if (sProcType.Trim().ToLower() == "index")
                {
                    lRowid = Convert.ToInt64(lvwBatches.SelectedItems[0].Text.Trim());

                    if (mGlobal.checkOpenedForms("frmVerify"))
                        MessageBox.Show("The Verify screen already opened!", "Message");
                    else
                    {
                        frmVerify ofVerify = new frmVerify();
                        ofVerify.ShowIcon = false;
                        //ofVerify.MaximizeBox = false;
                        //ofVerify.MinimizeBox = false;

                        ofVerify.MdiParent = this.MdiParent;
                        //this.MdiChildren[1].MaximizeBox = false;
                        //this.MdiChildren[1].MinimizeBox = false;                       

                        if (this.MdiParent != null)
                            this.MdiParent.Controls["toolStrip"].Controls[0].Enabled = false;

                        ofVerify.bSingleView = true;

                        ofVerify.Show();
                        ofVerify.reloadForCurrRow(lRowid, false);
                    }
                }
                else if (sProcType.Trim().ToLower() == "verify") //load verify in case from rescan.
                {
                    lRowid = Convert.ToInt64(lvwBatches.SelectedItems[0].Text.Trim());

                    if (mGlobal.checkOpenedForms("frmVerify"))
                        MessageBox.Show("The Verify screen already opened!", "Message");
                    else
                    {
                        frmVerify ofVerify = new frmVerify();
                        ofVerify.ShowIcon = false;
                        //ofVerify.MaximizeBox = false;
                        //ofVerify.MinimizeBox = false;

                        ofVerify.MdiParent = this.MdiParent;
                        //this.MdiChildren[1].MaximizeBox = false;
                        //this.MdiChildren[1].MinimizeBox = false;                       

                        if (this.MdiParent != null)
                            this.MdiParent.Controls["toolStrip"].Controls[0].Enabled = false;

                        ofVerify.bSingleView = true;

                        ofVerify.Show();
                        ofVerify.reloadForCurrRow(lRowid, true);
                    }
                }
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log("DClick.." + ex.Message);
                mGlobal.Write2Log(ex.StackTrace.ToString());
            }           

        }

        private void frmProcSumm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (this.MdiParent != null && mGlobal.checkAnyOpenedForms() == false)
                this.MdiParent.Controls["toolStrip"].Controls[0].Enabled = true;

            staMain.sessionRestart();
        }

        private void lvwBatches_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                if (tvwProcess.Nodes.Count > 0)
                    ProcStatusBar.Text = tvwProcess.SelectedNode.Text;
            }
            catch (Exception ex)
            {
            }
        }

        public void refreshProcSummary(object sender, EventArgs e)
        {
            try
            {
                tvwProcess.Focus();
                btnProcRefreshStrip_Click(sender, e);
            }
            catch (Exception ex)
            {
            }
        }

        private void lvwBatches_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (lvwBatches.Items.Count > 1)
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
                this.lvwBatches.Sort();
            }           
        }

        private void frmProcSumm_Activated(object sender, EventArgs e)
        {
            try
            {
                if (this.MdiParent != null)
                {
                    ToolStrip oToolStrip = (ToolStrip)this.MdiParent.Controls["toolStrip"];
                    oToolStrip.Items[0].Enabled = false;
                }
                staMain.sessionRestart();
                this.refreshProcSummaryTreeview();
            }
            catch (Exception ex)
            {
            }
        }

        public void refreshProcSummaryTreeview()
        {
            try
            {
                tvwProcess.Nodes.Clear();
                loadBatchProcessTreeview();
                if (tvwProcess.Nodes.Count > 0)
                {
                    tvwProcess.Update();
                    tvwProcess.Refresh();
                    tvwProcess.Focus();
                    tvwProcess.SelectedNode = tvwProcess.Nodes[0];
                }
            }
            catch (Exception ex)
            {
            }
        }

    }
}
