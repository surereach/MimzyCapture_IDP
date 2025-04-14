using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Dynamsoft;
using Dynamsoft.DBR;
using Dynamsoft.TWAIN;
using Dynamsoft.TWAIN.Interface;
using Dynamsoft.Core;
//using Dynamsoft.PDF;
using Dynamsoft.Core.Enums;
using Dynamsoft.Core.Annotation;

namespace SRDocScanIDP
{
    public partial class MDIMain : Form
    {
        //private int childFormNumber = 0;

        public const short intMaxImgBuff = 32767;

        public static string strProductKey;
        public static string strScanProject;
        public static int intAppNum;
        public static string strStation;
        public static string strUserID;
        public static int intLoginAttempts;
        public static List<string> strUserRoles;
        public static string strScanProjectApp;

        public static TwainManager oTMgr = null;
        public static ImageCore oImgCore = null;

        public static bool bIsComplete;
        public static string sIsAddOn;

        public static int iRecognitionMode;

        public static string sBatchType;
        public static string sDocDefId;

        public static string strIronLic;
        public static string strIronBcLic;
        public static string strIronPdfLic;

        private static Form frmScanMain;
        public static int iSessTimeout;
        public static System.Windows.Forms.Timer oSessTimer;
        public static bool bSessTimeout;
        public static int iSessMaxTimeout;

        public MDIMain()
        {
            InitializeComponent();
            try
            {
                strProductKey = mGlobal.GetAppCfg("LicKey");
                strIronLic = mGlobal.GetAppCfg("IRLicKey");
                strIronBcLic = mGlobal.GetAppCfg("IRBCLicKey");
                strIronPdfLic = mGlobal.GetAppCfg("IRPDFLicKey");

                frmScanMain = null;

                iSessMaxTimeout = 30; //seconds
                bSessTimeout = false;
                iSessTimeout = 0;
                //Application.AddMessageFilter(new clsFilterMess(this));
                oSessTimer = new System.Windows.Forms.Timer();
                oSessTimer.Interval = 1000; //miliseconds
                oSessTimer.Tick += SessionTimer_Tick;
                oSessTimer.Enabled = false;
                oSessTimer.Stop();
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log("Main.." + ex.Message);
            }

        }

        private void SessionTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                iSessTimeout += 1;
                if (MDIMain.iSessMaxTimeout != 0 && iSessTimeout > MDIMain.iSessMaxTimeout)
                {
                    bSessTimeout = true;
                    //CloseAllToolStripMenuItem_Click(sender, e);
                    mnuLogoutStrip_Click(sender, e);

                    if (iSessTimeout == MDIMain.iSessMaxTimeout + 1)
                        MessageBox.Show(this, "Mimzy Capture GEN is inactive! logging out..", "Login Expired");

                    staMain.sessionStop(false, 0);
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void ExitToolsStripMenuItem_Click(object sender, EventArgs e)
        {
            //try
            //{
            //    clsLogin oLogout = new clsLogin();
            //    oLogout.userLogout("Scan", MDIMain.strUserID);
            //}
            //catch (Exception ex)
            //{
            //}
            this.Close();
            Application.Exit();
        }

        private void ToolBarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toolStrip.Visible = toolBarToolStripMenuItem.Checked;
        }

        private void StatusBarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            statusStrip.Visible = statusBarToolStripMenuItem.Checked;
        }

        private void CascadeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.Cascade);
        }

        private void TileVerticalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileVertical);
        }

        private void TileHorizontalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileHorizontal);
        }

        private void ArrangeIconsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.ArrangeIcons);
        }

        private void CloseAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Form childForm in MdiChildren)
            {
                childForm.Close();
            }
        }

        private void MDIMain_Load(object sender, EventArgs e)
        {
            oTMgr = new TwainManager(strProductKey);

            this.WindowState = FormWindowState.Maximized;

            if (ScanPrjToolStripCbx.Items.Count > 1)
            {
                ScanPrjToolStripCbx.SelectedIndex = 1;
            }

            strScanProjectApp = ScanPrjToolStripCbx.Text.Trim();
            strScanProject = ScanPrjToolStripCbx.Text.Trim();
            intAppNum = 1;
            strStation = mGlobal.secDecrypt(mGlobal.GetAppCfg("ScanStn"), mGlobal.objSecData);
            strUserID = mGlobal.strCurrLoginID;
            strUserRoles = mGlobal.strCurrUserRoles;

            bIsComplete = true;
            sIsAddOn = "";

            iRecognitionMode = 1; //Default.
            sBatchType = "Batch"; //Default.

            mGlobal.enableToolMenuItem(viewMenu, false, "mnuProcSummStrip");
            mGlobal.enableToolMenuItem(viewMenu, false, "toolBarToolStripMenuItem");
            mGlobal.enableToolMenuItem(fileMenu, false, "mnuStationStrip");
            mGlobal.enableToolMenuItem(fileMenu, false, "mnuScanSettingStrip");
            mGlobal.enableToolMenuItem(helpMenu, false, "regLicToolStripMenuItem");
            mGlobal.enableToolMenuItem(helpMenu, false, "licInfoToolStripMenuItem");
            mGlobal.enableMenuItem(this, false, "fileMenu");
            mGlobal.enableMenuItem(this, false, "viewMenu");
            mGlobal.enableMenuItem(this, false, "tasksMenu");
            mGlobal.enableMenuItem(this, false, "windowsMenu");
            toolStrip.Controls[0].Enabled = false;

            //mnuProcSummStrip_Click(this, e);
            //toolStrip.Controls[0].Enabled = false;
            loadProjList();
            if (ScanPrjToolStripCbx.Items.Count > 0)
            {
                ScanPrjToolStripCbx.SelectedIndex = 0;
                strScanProjectApp = ScanPrjToolStripCbx.Text.Trim();
                strScanProject = ScanPrjToolStripCbx.Text.Trim();
            }

            if (strScanProject != "")
            {
                int iLastDash = strScanProject.LastIndexOf("-");
                string sPrj = strScanProject.Substring(0, iLastDash);
                string sApp = strScanProject.Substring(iLastDash + 1);
                loadProjSettings(sPrj, sApp);
            }
            else //Init to empty.
            {
                loadProjSettings("", "");
            }

            if (!mGlobal.checkOpenedForms("frmLogin"))
            {
                Form ofmLogin = new frmLogin();
                ofmLogin.WindowState = FormWindowState.Normal;
                ofmLogin.MdiParent = this;

                ofmLogin.Show();

                if (staMain.validateUserRole("Scan Operator"))
                    ScanToolStripBtn.Visible = true;
                else
                    ScanToolStripBtn.Visible = false;
            }

            if (staMain.stcProjCfg.ExportEnable.ToUpper() == "Y")
                mnuExportStrip.Visible = true;
            else
                mnuExportStrip.Visible = false;

            try
            {
                string sTimeout = mGlobal.GetAppCfg("SessMaxTimeout").Trim();
                if (sTimeout == string.Empty) sTimeout = "0";
                MDIMain.iSessMaxTimeout = Convert.ToInt32(sTimeout);
                if (MDIMain.iSessMaxTimeout == 0)
                {
                    oSessTimer.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MDIMain.iSessMaxTimeout = 30;
            }
        }

        private void mnuScanStrip_Click(object sender, EventArgs e)
        {
            if (mGlobal.checkOpenedForms("frmScan1") || mGlobal.checkOpenedForms("frmScanBox1"))
                MessageBox.Show("The Scan screen already opened!", "Message");
            else
            {
                this.WindowState = FormWindowState.Maximized;

                if (staMain.stcProjCfg.BatchType.Trim() == "Box")
                {
                    frmScanBox1 ofScan = new frmScanBox1();
                    frmScanMain = ofScan;
                    ofScan.ShowIcon = false;
                    //ofScan.MaximizeBox = false;
                    //ofScan.MinimizeBox = false;

                    ofScan.MdiParent = this;
                    //this.MdiChildren[0].MaximizeBox = false;
                    //this.MdiChildren[0].MinimizeBox = false;                       

                    toolStrip.Items[0].Visible = true;
                    toolStrip.Controls[0].Enabled = false;
                    ofScan.Show();
                }
                else
                {
                    frmScan1 ofScan = new frmScan1();
                    frmScanMain = ofScan;
                    ofScan.ShowIcon = false;
                    //ofScan.MaximizeBox = false;
                    //ofScan.MinimizeBox = false;

                    ofScan.MdiParent = this;
                    //this.MdiChildren[0].MaximizeBox = false;
                    //this.MdiChildren[0].MinimizeBox = false;                       

                    toolStrip.Items[0].Visible = true;
                    toolStrip.Controls[0].Enabled = false;
                    ofScan.Show();
                }
            }
        }

        private void mnuIndexingStrip_Click(object sender, EventArgs e)
        {
            if (mGlobal.checkOpenedForms("frmIndexing"))
                MessageBox.Show("The Index screen already opened!", "Message");
            else
            {
                frmIndexing ofIndexing = new frmIndexing();
                ofIndexing.ShowIcon = false;
                //ofIndexing.MaximizeBox = false;
                //ofIndexing.MinimizeBox = false;

                ofIndexing.MdiParent = this;
                //this.MdiChildren[1].MaximizeBox = false;
                //this.MdiChildren[1].MinimizeBox = false;                       

                toolStrip.Controls[0].Enabled = false;
                ofIndexing.Show();
            }
        }

        private void mnuRejectIdxStrip_Click(object sender, EventArgs e)
        {
            //if (mGlobal.checkOpenedForms("frmIndexingReject"))
            //    MessageBox.Show("The Rejected Index screen already opened!", "Message");
            //else
            //{
            //    frmIndexingReject ofIndexingRj = new frmIndexingReject();
            //    ofIndexingRj.ShowIcon = false;
            //    //ofIndexingRj.MaximizeBox = false;
            //    //ofIndexingRj.MinimizeBox = false;

            //    ofIndexingRj.MdiParent = this;
            //    //this.MdiChildren[1].MaximizeBox = false;
            //    //this.MdiChildren[1].MinimizeBox = false;                       

            //    toolStrip.Controls[0].Enabled = false;
            //    ofIndexingRj.Show();
            //}
        }

        private void mnuVerifyStrip_Click(object sender, EventArgs e)
        {
            if (mGlobal.checkOpenedForms("frmVerify"))
                MessageBox.Show("The Verify screen already opened!", "Message");
            else
            {
                frmVerifyScan ofVerify = new frmVerifyScan();
                ofVerify.ShowIcon = false;
                //ofVerify.MaximizeBox = false;
                //ofVerify.MinimizeBox = false;

                ofVerify.MdiParent = this;
                //this.MdiChildren[1].MaximizeBox = false;
                //this.MdiChildren[1].MinimizeBox = false;                       

                toolStrip.Controls[0].Enabled = false;
                ofVerify.Show();
            }
        }

        private void MDIMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (!MDIMain.bIsComplete)
                {
                    MessageBox.Show("The scanning process is running! Application not able to exit.", "Scan Process");
                    e.Cancel = true;
                }
                else
                {
                    CloseAllToolStripMenuItem_Click(sender, e);

                    if (MDIMain.strUserID.Trim() != string.Empty && mGlobal.blnIsLogin)
                    {
                        clsLogin oLogout = new clsLogin();
                        oLogout.userLogout("Scan", MDIMain.strUserID);
                    }

                    if (oSessTimer != null) oSessTimer.Dispose();
                    if (oTMgr != null) oTMgr.Dispose();
                    if (oImgCore != null) oImgCore.Dispose();

                    Environment.Exit(0);
                    Application.ExitThread();
                }
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log("App.." + ex.Message);
            }

        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //MessageBox.Show(Application.ProductName + " (" + Application.ProductVersion + ")", "About");
            try
            {
                frmAbout oAbt = new frmAbout();
                oAbt.ShowIcon = false;

                oAbt.ShowDialog(this);
            }
            catch (Exception ex)
            {
            }
        }

        private void mnuScannerStrip_Click(object sender, EventArgs e)
        {
            try
            {
                oTMgr.SelectSource();
                oTMgr.CloseSource();
                oTMgr.CloseSourceManager();
            }
            catch (Dynamsoft.TWAIN.TwainException te)
            {
                MessageBox.Show(te.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void ScanPrjToolStripCbx_SelectedIndexChanged(object sender, EventArgs e)
        {
            strScanProject = this.ScanPrjToolStripCbx.Text.Trim();
            strScanProjectApp = ScanPrjToolStripCbx.Text.Trim();
            if (strScanProject != "")
            {
                int iLastDash = strScanProject.LastIndexOf("-");
                string sPrj = strScanProject.Substring(0, iLastDash);
                string sApp = strScanProject.Substring(iLastDash + 1);
                loadProjSettings(sPrj, sApp);

                tasksMenu_Click(this, e);

                if (mGlobal.blnIsLogin && !mGlobal.checkOpenedForms("frmProcSumm"))
                {
                    mnuProcSummStrip_Click(sender, e);
                }
            }
        }

        private void mnuScanSettingStrip_Click(object sender, EventArgs e)
        {
            frmScannerCfg ofScanCfg = new frmScannerCfg();
            ofScanCfg.ShowDialog(this);
        }

        private void mnuExportStrip_Click(object sender, EventArgs e)
        {
            if (mGlobal.checkOpenedForms("frmExport"))
            {
                MessageBox.Show("The Export screen already opened!", "Message");
            }
            else
            {
                frmExport ofExport = new frmExport();
                ofExport.ShowIcon = false;

                ofExport.MdiParent = this;
                ofExport.StartPosition = FormStartPosition.CenterParent;

                toolStrip.Controls[0].Enabled = false;
                ofExport.Show();
                ofExport.WindowState = FormWindowState.Normal;

                //frmProcSumm ofProSumm = new frmProcSumm();
                //ofProSumm.Activated += ofProSumm.refreshProcSummary;
                //ofProSumm.refreshProcSummaryTreeview();
                //ofProSumm.Dispose();
            }
        }

        private void mnuProfileStrip_Click(object sender, EventArgs e)
        {
            try
            {
                oTMgr.EnableSourceUI();
            }
            catch (Exception ex)
            {
            }
        }

        private void mnuStationStrip_Click(object sender, EventArgs e)
        {
            frmStation ofStn = new frmStation();
            ofStn.ShowIcon = false;

            //ofStn.MdiParent = this;
            ofStn.StartPosition = FormStartPosition.CenterScreen;

            toolStrip.Controls[0].Enabled = false;
            ofStn.ShowDialog(this);
            ofStn.WindowState = FormWindowState.Normal;
        }

        private void mnuRescanStrip_Click(object sender, EventArgs e)
        {
            if (mGlobal.checkOpenedForms("frmRescan1") || mGlobal.checkOpenedForms("frmRescanBox1"))
                MessageBox.Show("The Rescan screen already opened!", "Message");
            else
            {
                this.WindowState = FormWindowState.Maximized;

                if (staMain.stcProjCfg.BatchType.Trim() == "Box")
                {
                    frmRescanBox1 ofReScan = new frmRescanBox1();
                    ofReScan.ShowIcon = false;
                    //ofReScan.MaximizeBox = false;
                    //ofReScan.MinimizeBox = false;

                    ofReScan.MdiParent = this;
                    //this.MdiChildren[0].MaximizeBox = false;
                    //this.MdiChildren[0].MinimizeBox = false;                       

                    toolStrip.Controls[0].Enabled = false;
                    ofReScan.Show();
                }
                else
                {
                    frmRescan1 ofReScan = new frmRescan1();
                    ofReScan.ShowIcon = false;
                    //ofReScan.MaximizeBox = false;
                    //ofReScan.MinimizeBox = false;

                    ofReScan.MdiParent = this;
                    //this.MdiChildren[0].MaximizeBox = false;
                    //this.MdiChildren[0].MinimizeBox = false;                       

                    toolStrip.Controls[0].Enabled = false;
                    ofReScan.Show();
                }

            }
        }

        private void mnuProcSummStrip_Click(object sender, EventArgs e)
        {
            if (mGlobal.checkOpenedForms("frmProcSumm"))
                MessageBox.Show("The Process Summary screen already opened!", "Message");
            else
            {
                this.WindowState = FormWindowState.Maximized;

                frmProcSumm ofProSumm = new frmProcSumm();
                ofProSumm.ShowIcon = false;
                //ofProSumm.MaximizeBox = false;
                //ofProSumm.MinimizeBox = false;
                ofProSumm.Activated += ofProSumm.refreshProcSummary;

                ofProSumm.MdiParent = this;
                //this.MdiChildren[0].MaximizeBox = false;
                //this.MdiChildren[0].MinimizeBox = false;                       

                toolStrip.Controls[0].Enabled = false;
                ofProSumm.Show();
            }
        }

        private void loadProjList()
        {
            string sSQL;
            try
            {
                mGlobal.LoadAppDBCfg();

                sSQL = "SELECT DISTINCT scanpjcode+'-'+sysappnum FROM " + mGlobal.strDBName.Trim().Replace("'", "") + ".dbo.TScanProjSet ";
                sSQL += "WHERE active='Y' ";
                sSQL += "ORDER BY scanpjcode+'-'+sysappnum ";
                DataRowCollection drs = mGlobal.objDB.ReturnRows(sSQL);

                if (drs != null)
                {
                    ScanPrjToolStripCbx.Items.Clear();
                    lblAppNameStrip.Text = "";

                    int i = 0;
                    while (i < drs.Count)
                    {
                        ScanPrjToolStripCbx.Items.Add(drs[i][0].ToString());

                        i += 1;
                    }
                }

            }
            catch (Exception ex)
            {
            }
        }

        private void loadProjSettings(string pScanpjcode, string pAppNum)
        {
            try
            {
                staMain.loadProjSettings(pScanpjcode, pAppNum);

                lblAppNameStrip.Text = staMain.stcProjCfg.AppName;
                if (staMain.stcProjCfg.NoSeparator.Trim().ToUpper() == "C")
                    lblProjectStrip.Text = staMain.stcProjCfg.BatchType + ": Class Separator";
                else
                    lblProjectStrip.Text = staMain.stcProjCfg.BatchType + ": " + staMain.stcProjCfg.NoSeparator + " Separator";

                MDIMain.strScanProject = staMain.stcProjCfg.PrjCode;
                MDIMain.intAppNum = Convert.ToInt32(staMain.stcProjCfg.AppNum);
                MDIMain.sBatchType = staMain.stcProjCfg.BatchType;
                MDIMain.sDocDefId = staMain.stcProjCfg.DocDefId;

                if (MDIMain.sBatchType.Trim().ToLower() == "set")
                    staMain.sSetNumFmt = "000000";
                else
                    staMain.sSetNumFmt = "000";
            }
            catch (Exception ex)
            {
            }
        }

        private void tasksMenu_Click(object sender, EventArgs e)
        {
            try
            {
                if (staMain.validateUserRole("Scan Operator"))
                {
                    tasksMenu.DropDownItems["mnuScanStrip"].Visible = true;
                    if (staMain.checkScanFormOpened())
                        ScanToolStripBtn.Visible = true;
                }
                else
                {
                    tasksMenu.DropDownItems["mnuScanStrip"].Visible = false;
                    ScanToolStripBtn.Visible = false;
                }

                if (staMain.stcProjCfg.RescanEnable.Trim().ToUpper() == "Y")
                {
                    tasksMenu.DropDownItems["mnuRescanStrip"].Visible = false; //true; //false due to access via Process Summary.
                }
                else
                {
                    tasksMenu.DropDownItems["mnuRescanStrip"].Visible = false;
                }

                if (tasksMenu.DropDownItems["mnuScanStrip"].Visible || tasksMenu.DropDownItems["mnuRescanStrip"].Visible)
                {
                    tasksMenu.DropDownItems["toolStripSeparator4"].Visible = true;
                    tasksMenu.DropDownItems["toolStripSeparator5"].Visible = true;
                }
                else
                {
                    tasksMenu.DropDownItems["toolStripSeparator4"].Visible = false;
                    tasksMenu.DropDownItems["toolStripSeparator5"].Visible = false;
                }

                if (staMain.stcProjCfg.VerifyEnable.Trim().ToUpper() == "Y")
                {
                    tasksMenu.DropDownItems["mnuVerifyStrip"].Visible = false; //false due to access via Process Summary.
                    tasksMenu.DropDownItems["toolStripSeparator4"].Visible = false; //false due to access via Process Summary.
                }
                else
                {
                    tasksMenu.DropDownItems["mnuVerifyStrip"].Visible = false;
                    tasksMenu.DropDownItems["toolStripSeparator4"].Visible = false;
                }

                if (staMain.stcProjCfg.ExportEnable.Trim().ToUpper() == "Y" && !staMain.validateUserRole("SR Scan Admin"))
                {
                    tasksMenu.DropDownItems["mnuExportStrip"].Visible = true;
                    tasksMenu.DropDownItems["toolStripSeparator5"].Visible = true;
                }
                else
                {
                    tasksMenu.DropDownItems["mnuExportStrip"].Visible = false;
                    tasksMenu.DropDownItems["toolStripSeparator5"].Visible = false;
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void mnuLogoutStrip_Click(object sender, EventArgs e)
        {
            try
            {
                if (!mGlobal.checkOpenedForms("frmLogin"))
                {
                    if (mGlobal.checkOpenedForms("frmStation"))
                    {
                        mGlobal.getOpenedForms("frmStation").Close();
                    }
                    if (mGlobal.checkOpenedForms("frmScannerCfg"))
                    {
                        mGlobal.getOpenedForms("frmScannerCfg").Close();
                    }
                    if (mGlobal.checkOpenedForms("frmLicReg"))
                    {
                        mGlobal.getOpenedForms("frmLicReg").Close();
                    }
                    if (mGlobal.checkOpenedForms("frmLicInfo"))
                    {
                        mGlobal.getOpenedForms("frmLicInfo").Close();
                    }
                    if (mGlobal.checkOpenedForms("frmHelp"))
                    {
                        mGlobal.getOpenedForms("frmHelp").Close();
                    }
                    if (mGlobal.checkOpenedForms("frmNotification"))
                    {
                        mGlobal.getOpenedForms("frmNotification").Close();
                    }
                    if (mGlobal.checkOpenedForms("frmAbout"))
                    {
                        mGlobal.getOpenedForms("frmAbout").Close();
                    }

                    CloseAllToolStripMenuItem_Click(sender, e);

                    mGlobal.enableToolMenuItem(viewMenu, false, "mnuProcSummStrip");
                    mGlobal.enableToolMenuItem(viewMenu, false, "toolBarToolStripMenuItem");
                    mGlobal.enableToolMenuItem(fileMenu, false, "mnuStationStrip");
                    mGlobal.enableToolMenuItem(fileMenu, false, "mnuScanSettingStrip");
                    mGlobal.enableToolMenuItem(helpMenu, false, "regLicToolStripMenuItem");
                    mGlobal.enableToolMenuItem(helpMenu, false, "licInfoToolStripMenuItem");
                    mGlobal.enableMenuItem(this, false, "fileMenu");
                    mGlobal.enableMenuItem(this, false, "viewMenu");
                    mGlobal.enableMenuItem(this, false, "tasksMenu");
                    mGlobal.enableMenuItem(this, false, "windowsMenu");
                    toolStrip.Controls[0].Enabled = false;
                    toolStrip.Items[0].Visible = false;
                    ScanPrjToolStripCbx.Items.Clear();
                    loadProjList();

                    lblAppNameStrip.Text = "";

                    clsLogin oLogin = new clsLogin();
                    oLogin.userLogout("Scan", MDIMain.strUserID);
                    mGlobal.blnIsLogin = false;

                    staMain.sessionStop(false, 0);

                    frmLogin ofmLogin = new frmLogin();
                    ofmLogin.WindowState = FormWindowState.Normal;
                    ofmLogin.MdiParent = this;
                    ofmLogin.Show();
                    if (mGlobal.checkOpenedForms("frmProcSumm"))
                    {
                        mGlobal.getOpenedForms("frmProcSumm").Close();
                    }
                }
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log("Logout.." + ex.Message);
            }
        }

        private void tasksMenu_MouseHover(object sender, EventArgs e)
        {
            tasksMenu_Click(sender, e);
        }

        private void regLicToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                frmLicReg oInfo = new frmLicReg();
                oInfo.ShowDialog(this);
                oInfo.Dispose();
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log(ex.StackTrace.ToString());
                MessageBox.Show(ex.Message, "Error");
            }
        }

        private void licInfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                frmLicInfo oInfo = new frmLicInfo();
                oInfo.ShowDialog(this);
                oInfo.Dispose();
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log(ex.StackTrace.ToString());
                MessageBox.Show(ex.Message, "Error");
            }
        }

        private void helpMenu_MouseHover(object sender, EventArgs e)
        {
            try
            {
                if (mGlobal.blnIsLogin)
                {
                    licInfoToolStripMenuItem.Visible = true;
                    licInfoToolStripMenuItem.Enabled = true;

                    if (staMain.validateUserRole("SR Scan Admin"))
                    {
                        regLicToolStripMenuItem.Visible = true;
                        regLicToolStripMenuItem.Enabled = true;
                    }
                    else
                    {
                        regLicToolStripMenuItem.Visible = false;
                        regLicToolStripMenuItem.Enabled = false;
                    }
                }
                else
                {
                    regLicToolStripMenuItem.Visible = false;
                    regLicToolStripMenuItem.Enabled = false;
                    licInfoToolStripMenuItem.Visible = false;
                    licInfoToolStripMenuItem.Enabled = false;
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void helpMenu_Click(object sender, EventArgs e)
        {
            try
            {
                if (mGlobal.blnIsLogin)
                {
                    licInfoToolStripMenuItem.Visible = true;
                    licInfoToolStripMenuItem.Enabled = true;

                    if (staMain.validateUserRole("SR Scan Admin"))
                    {
                        regLicToolStripMenuItem.Visible = true;
                        regLicToolStripMenuItem.Enabled = true;
                    }
                    else
                    {
                        regLicToolStripMenuItem.Visible = false;
                        regLicToolStripMenuItem.Enabled = false;
                    }
                }
                else
                {
                    regLicToolStripMenuItem.Visible = false;
                    regLicToolStripMenuItem.Enabled = false;
                    licInfoToolStripMenuItem.Visible = false;
                    licInfoToolStripMenuItem.Enabled = false;
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void ScanToolStripBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (frmScanMain != null)
                {
                    if (mGlobal.checkOpenedForms("frmScan1"))
                    {
                        if (frmScanMain.MdiParent.ActiveMdiChild.Name == "frmScan1")
                        {
                            frmScan1 fScan = (frmScan1)frmScanMain;
                            if (fScan.WindowState != FormWindowState.Maximized)
                                fScan.WindowState = FormWindowState.Maximized;

                            staMain.sessionStop(false, 0);
                            toolStrip.Items[0].Visible = true;
                            fScan.btnScan_Click(sender, e);
                        }
                        else
                        {
                            MessageBox.Show(this, "The Document Scan screen already opened!", "Message");
                            frmScanMain.Focus();
                        }
                    }
                    else if (mGlobal.checkOpenedForms("frmRescan1"))
                    {
                        if (frmScanMain.MdiParent.ActiveMdiChild.Name == "frmRescan1")
                        {
                            frmRescan1 fScan = (frmRescan1)frmScanMain;
                            if (fScan.WindowState != FormWindowState.Maximized)
                                fScan.WindowState = FormWindowState.Maximized;

                            staMain.sessionStop(false, 0);
                            toolStrip.Items[0].Visible = true;
                            fScan.btnScan_Click(sender, e);
                        }
                        else
                        {
                            MessageBox.Show(this, "The Document Scan screen already opened!", "Message");
                            frmScanMain.Focus();
                        }
                    }
                    else if (mGlobal.checkOpenedForms("frmScanBox1"))
                    {
                        if (frmScanMain.MdiParent.ActiveMdiChild.Name == "frmScanBox1")
                        {
                            frmScanBox1 fScan = (frmScanBox1)frmScanMain;
                            if (fScan.WindowState != FormWindowState.Maximized)
                                fScan.WindowState = FormWindowState.Maximized;

                            staMain.sessionStop(false, 0);
                            toolStrip.Items[0].Visible = true;
                            fScan.btnScan_Click(sender, e);
                        }
                        else
                        {
                            MessageBox.Show(this, "The Document Scan screen already opened!", "Message");
                            frmScanMain.Focus();
                        }
                    }
                    else if (mGlobal.checkOpenedForms("frmRescanBox1"))
                    {
                        if (frmScanMain.MdiParent.ActiveMdiChild.Name == "frmRescanBox1")
                        {
                            frmRescanBox1 fScan = (frmRescanBox1)frmScanMain;
                            if (fScan.WindowState != FormWindowState.Maximized)
                                fScan.WindowState = FormWindowState.Maximized;

                            staMain.sessionStop(false, 0);
                            toolStrip.Items[0].Visible = true;
                            fScan.btnScan_Click(sender, e);
                        }
                        else
                        {
                            MessageBox.Show(this, "The Document Scan screen already opened!", "Message");
                            frmScanMain.Focus();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log("Scan.." + ex.Message);
            }
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                //System.Diagnostics.Process.Start("iexplore", @"https://www.google.com/");
                frmHelp oHelp = new frmHelp();
                oHelp.Show(this);
            }
            catch (Exception ex)
            {
            }
        }

        private void helpMenu_DropDownOpening(object sender, EventArgs e)
        {
            try
            {
                helpMenu_MouseHover(sender, e);
            }
            catch (Exception ex)
            {
            }
        }

        private void mnuIDPStrip_Click(object sender, EventArgs e)
        {
            try
            {
                toolStrip.Controls[0].Enabled = false;
                frmIDP ofIDP = new frmIDP();
                ofIDP.ShowIcon = false;

                ofIDP.MdiParent = this;
                ofIDP.StartPosition = FormStartPosition.CenterParent;
                
                ofIDP.Show();
                ofIDP.WindowState = FormWindowState.Normal;
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log("IDP menu.." + ex.Message);
            }
        }


    }
}
