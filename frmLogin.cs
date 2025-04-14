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
    public partial class frmLogin : Form
    {
        private const int FORM_MIN_WIDTH = 475;
        private const int FORM_MIN_HEIGHT = 405;
        private int iFormOffset = 60;

        public frmLogin()
        {
            InitializeComponent();
        }

        private void frmLogin_Load(object sender, EventArgs e)
        {
            try
            {
                mGlobal.strClearTextCred = "0";

                mGlobal.strCurrLoginID = "";
                mGlobal.strCurrUserName = "";
                MDIMain.strUserID = "";
                MDIMain.intLoginAttempts = 0;
                this.WindowState = FormWindowState.Normal;
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log(ex.Message);
            }
        }

        private void frmLogin_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                if (System.Windows.Forms.Screen.AllScreens[0].Bounds.Width == FORM_MIN_WIDTH && System.Windows.Forms.Screen.AllScreens[0].Bounds.Height == FORM_MIN_HEIGHT)
                {
                    iFormOffset = 60;
                }
                else
                {
                    iFormOffset = 70;
                }
                //formResize();
            }
            catch (Exception ex)
            {
            }
        }

        private void frmLogin_Resize(object sender, EventArgs e)
        {
            try
            {
                formResize();
                //if (this.WindowState != FormWindowState.Minimized)
                //{
                //    if (this.WindowState == FormWindowState.Normal)
                //    {
                //        if (this.Width < FORM_MIN_WIDTH) pictureBox1.Width = FORM_MIN_WIDTH;
                //        if (this.Height < FORM_MIN_HEIGHT) pictureBox1.Height = FORM_MIN_HEIGHT / 4;
                //    }

                //    this.Width = pictureBox1.Width + 20;
                //    this.Height = btnLogin.Location.Y + btnLogin.Height + 70;
                //    if (pictureBox1.Width > Width)
                //        pictureBox1.Width = this.Width - 20;
                //    else
                //        pictureBox1.Width = Width - 10;
                //}
            }
            catch (Exception ex)
            {
            }
        }

        private void formResize()
        {
            try
            {
                if (this.WindowState != FormWindowState.Minimized)
                {
                    if (this.WindowState == FormWindowState.Normal)
                    {
                        if (this.Width > FORM_MIN_WIDTH) Width = FORM_MIN_WIDTH;
                        if (this.Height > FORM_MIN_HEIGHT) Height = FORM_MIN_HEIGHT;
                        //iFormOffset = -20;
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            bool bLicReg = true;
            try
            {
                bool bAppAdmin = false;
                clsLogin objLogin = new clsLogin();
                objLogin.getUserNameAndRole(txtUserId.Text.Trim());
                bAppAdmin = objLogin.validateUserRole("SR Scan Admin");

                if (mGlobal.GetAppCfg("SRLic") == string.Empty)
                {
                    MessageBox.Show("License is not available or not registered! Please contact your system administrator.", "Login", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    bLicReg = false;
                }

                if (bLicReg == false)
                {
                    if (bAppAdmin)
                    {
                        if (validateField())
                            verifyUserPwd(txtUserId.Text.Trim(), txtUserPwd.Text.Trim());
                    }
                }
                else
                {
                    if (validateField())
                        verifyUserPwd(txtUserId.Text.Trim(), txtUserPwd.Text.Trim());
                }

            }
            catch (Exception ex)
            {
                mGlobal.Write2Log("Login exception.." + ex.Message);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            try
            {
                //if (!mGlobal.blnIsLogin)
                //{
                //    mGlobal.enableMenuItem(this.MdiParent, false, "fileMenu");
                //    mGlobal.enableMenuItem(this.MdiParent, false, "viewMenu");
                //    mGlobal.enableMenuItem(this.MdiParent, false, "tasksMenu");
                //    mGlobal.enableMenuItem(this.MdiParent, false, "windowsMenu");
                //}

                this.Close();
                //Application.Exit();
                Application.ExitThread();
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log("Login cancel exception.." + ex.Message);
            }
        }

        private bool validateField()
        {
            bool validateField = true;

            try
            {
                if (txtUserId.Text.Trim() == "" || txtUserPwd.Text.Trim() == "")
                {
                    MessageBox.Show("Please key in User ID and Password!", "Login", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    validateField = false;
                }
                else
                    validateField = true;
            }
            catch (Exception ex)
            {
                validateField = false;
                mGlobal.Write2Log("verifying exception.." + ex.Message);
            }

            return validateField;
        }

        private void verifyUserPwd(string strUserID, string strUserPwd)
        {
            try
            {
                clsLogin objLogin = new clsLogin();
                objLogin.Attempts = MDIMain.intLoginAttempts + 1;
                MDIMain.intLoginAttempts = objLogin.Attempts;
                string sMsg = "";
                bool bAdmin = false;

                //if (objLogin.validateLogin(strUserID, strUserPwd)) {
                if (objLogin.authUserCred(strUserID, strUserPwd, mGlobal.GetAppCfg("SRLic"), ref sMsg))
                {
                    mGlobal.blnIsLogin = true;
                    MDIMain.intLoginAttempts = 0;

                    //if (mGlobal.strCurrLoginID.Trim().ToLower() == "srappadmin")
                    //    mGlobal.strCurrUserName = "SR Station Admin";

                    if (objLogin.validateUserRole("SR Scan Admin"))
                    {
                        mGlobal.strCurrUserName = "SR Station Admin";
                        bAdmin = true;
                    }

                    //frmProcSumm oProcSum = null;
                    frmProjList oProjList = null;
                    bool bList = false;

                    MDIMain.strUserID = mGlobal.strCurrLoginID.Trim();

                    //oProcSum = new frmProcSumm();
                    //oProcSum.MdiParent = this.MdiParent;
                    //oProcSum.Activated += oProcSum.refreshProcSummary;
                    //oProcSum.ShowIcon = false;
                    //oProcSum.Show();
                    //oProcSum.WindowState = FormWindowState.Maximized;

                    //((MDIMain)oProcSum.MdiParent).MainStatusStrip.Text = "User login: " + mGlobal.strCurrUserName;
                    if (!bAdmin)
                    {
                        oProjList = new frmProjList();
                        oProjList.MdiParent = this.MdiParent;
                        oProjList.Show();
                        bList = true;
                    }
                    else //Admin.
                    {
                        mGlobal.enableToolMenuItem((ToolStripMenuItem)this.MdiParent.MainMenuStrip.Items["viewMenu"], false, "mnuProcSummStrip");
                        mGlobal.enableToolMenuItem((ToolStripMenuItem)this.MdiParent.MainMenuStrip.Items["helpMenu"], true, "regLicToolStripMenuItem");
                        mGlobal.enableToolMenuItem((ToolStripMenuItem)this.MdiParent.MainMenuStrip.Items["helpMenu"], true, "licInfoToolStripMenuItem");
                    }

                    if (!bList)
                    {
                        mGlobal.enableToolMenuItem((ToolStripMenuItem)this.MdiParent.MainMenuStrip.Items["viewMenu"], true, "toolBarToolStripMenuItem");
                        mGlobal.enableToolMenuItem((ToolStripMenuItem)this.MdiParent.MainMenuStrip.Items["fileMenu"], true, "mnuStationStrip");
                        mGlobal.enableToolMenuItem((ToolStripMenuItem)this.MdiParent.MainMenuStrip.Items["fileMenu"], true, "mnuScanSettingStrip");
                        mGlobal.enableMenuItem(this.MdiParent, true, "fileMenu");
                        mGlobal.enableMenuItem(this.MdiParent, true, "viewMenu");
                        mGlobal.enableMenuItem(this.MdiParent, true, "tasksMenu");
                        mGlobal.enableMenuItem(this.MdiParent, true, "windowsMenu");
                    }

                    if (this.MdiParent != null)
                        this.MdiParent.Controls["toolStrip"].Controls[0].Enabled = false;

                    MDIMain.strUserID = mGlobal.strCurrLoginID.Trim().ToLower();

                    staMain.sessionRestart();

                    this.Close();
                }
                else
                {
                    if (objLogin.AttemptFail)
                        MessageBox.Show(this, "Login failed with login attempts exceeded! Account is locked.", "Login", MessageBoxButtons.OK);
                    else
                    {
                        if (sMsg.Trim() != string.Empty)
                            MessageBox.Show(this, "Login failed! " + sMsg, "Login", MessageBoxButtons.OK);
                        else
                            MessageBox.Show(this, "Login failed!", "Login", MessageBoxButtons.OK);
                    }

                    txtUserPwd.Text = "";
                    txtUserPwd.Focus();
                }
            }
            catch (Exception ex)
            {
                mGlobal.Write2Log("Verifying exception.." + ex.Message);
            }
        }

        private void txtUserPwd_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnLogin_Click(sender, e);
            }
        }
    }
}
