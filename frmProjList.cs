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
    public partial class frmProjList : Form
    {
        public static string strScanProject;
        public static string strScanProjectApp;

        private bool bCancel;

        public frmProjList()
        {
            InitializeComponent();
        }

        private void frmProjList_Load(object sender, EventArgs e)
        {
            try
            {
                bCancel = true;

                if (ddlProjects.Items.Count > 1)
                {
                    ddlProjects.SelectedIndex = 1;
                }
                strScanProjectApp = ddlProjects.Text.Trim();
                strScanProject = ddlProjects.Text.Trim();

                loadProjList();
                if (ddlProjects.Items.Count > 0)
                {
                    ddlProjects.SelectedIndex = 0;
                    strScanProjectApp = ddlProjects.Text.Trim();
                    strScanProject = ddlProjects.Text.Trim();
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

                mGlobal.enableToolMenuItem((ToolStripMenuItem)this.MdiParent.MainMenuStrip.Items["viewMenu"], false, "mnuProcSummStrip");
                mGlobal.enableToolMenuItem((ToolStripMenuItem)this.MdiParent.MainMenuStrip.Items["helpMenu"], false, "regLicToolStripMenuItem");
                mGlobal.enableToolMenuItem((ToolStripMenuItem)this.MdiParent.MainMenuStrip.Items["helpMenu"], true, "licInfoToolStripMenuItem");
                mGlobal.enableMenuItem(this.MdiParent, true, "fileMenu");
                mGlobal.enableMenuItem(this.MdiParent, true, "viewMenu");
                mGlobal.enableMenuItem(this.MdiParent, false, "tasksMenu");
                mGlobal.enableMenuItem(this.MdiParent, false, "windowsMenu");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
            }
        }

        private void ddlProjects_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                strScanProject = ddlProjects.Text.Trim();
                strScanProjectApp = ddlProjects.Text.Trim();
                if (strScanProject != "")
                {
                    int iLastDash = strScanProject.LastIndexOf("-");
                    string sPrj = strScanProject.Substring(0, iLastDash);
                    string sApp = strScanProject.Substring(iLastDash + 1);
                    loadProjSettings(sPrj, sApp);                    
                }
            }
            catch (Exception ex)
            {
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
                    ddlProjects.Items.Clear();
                    lblName.Text = "";
                    lblNote.Text = "";

                    int i = 0;
                    while (i < drs.Count)
                    {
                        ddlProjects.Items.Add(drs[i][0].ToString());

                        i += 1;
                    }
                }
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log(ex.Message + ".." + ex.StackTrace.ToString());
            }
        }

        private void loadProjSettings(string pScanpjcode, string pAppNum)
        {
            try
            {
                staMain.loadProjSettings(pScanpjcode, pAppNum);

                lblName.Text = staMain.stcProjCfg.AppName;
                lblNote.Text = staMain.stcProjCfg.BatchType + ": " + staMain.stcProjCfg.NoSeparator + " Separator";

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

        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                frmProcSumm oProcSum = null;

                oProcSum = new frmProcSumm();
                oProcSum.MdiParent = this.ParentForm;
                oProcSum.Activated += oProcSum.refreshProcSummary;
                oProcSum.ShowIcon = false;
                oProcSum.Show();
                oProcSum.WindowState = FormWindowState.Maximized;

                ((MDIMain)oProcSum.MdiParent).MainStatusStrip.Text = "User login: " + mGlobal.strCurrUserName;
                if (((MDIMain)oProcSum.MdiParent).ScanPrjToolStripCbx.Items.Count > 0)
                {
                    ((MDIMain)oProcSum.MdiParent).ScanPrjToolStripCbx.SelectedIndex = ddlProjects.SelectedIndex;
                }

                bCancel = false;
                
                this.Close();
            }
            catch (Exception)
            {
            }
        }

        private void frmProjList_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (bCancel)
            {
                frmProcSumm oProcSum = new frmProcSumm();
                oProcSum.MdiParent = this.ParentForm;
                oProcSum.Activated += oProcSum.refreshProcSummary;
                oProcSum.ShowIcon = false;
                oProcSum.Show();
                oProcSum.WindowState = FormWindowState.Maximized;

                ((MDIMain)this.MdiParent).MainStatusStrip.Text = "User login: " + mGlobal.strCurrUserName;
                if (((MDIMain)this.MdiParent).ScanPrjToolStripCbx.Items.Count > 0)
                {
                    ((MDIMain)this.MdiParent).ScanPrjToolStripCbx.SelectedIndex = 0;
                }
            }
        }

        private void frmProjList_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                mGlobal.enableToolMenuItem((ToolStripMenuItem)this.MdiParent.MainMenuStrip.Items["viewMenu"], true, "mnuProcSummStrip");
                mGlobal.enableToolMenuItem((ToolStripMenuItem)this.MdiParent.MainMenuStrip.Items["helpMenu"], false, "regLicToolStripMenuItem");
                mGlobal.enableToolMenuItem((ToolStripMenuItem)this.MdiParent.MainMenuStrip.Items["helpMenu"], true, "licInfoToolStripMenuItem");

                mGlobal.enableToolMenuItem((ToolStripMenuItem)this.MdiParent.MainMenuStrip.Items["viewMenu"], true, "toolBarToolStripMenuItem");
                mGlobal.enableToolMenuItem((ToolStripMenuItem)this.MdiParent.MainMenuStrip.Items["fileMenu"], true, "mnuStationStrip");
                mGlobal.enableToolMenuItem((ToolStripMenuItem)this.MdiParent.MainMenuStrip.Items["fileMenu"], true, "mnuScanSettingStrip");
                mGlobal.enableMenuItem(this.MdiParent, true, "fileMenu");
                mGlobal.enableMenuItem(this.MdiParent, true, "viewMenu");
                mGlobal.enableMenuItem(this.MdiParent, true, "tasksMenu");
                mGlobal.enableMenuItem(this.MdiParent, true, "windowsMenu");
            }
            catch (Exception ex)
            {
            }
        }




    }
}
