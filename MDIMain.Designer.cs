
namespace SRDocScanIDP
{
    partial class MDIMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MDIMain));
            menuStrip = new MenuStrip();
            fileMenu = new ToolStripMenuItem();
            mnuStationStrip = new ToolStripMenuItem();
            toolStripSeparator6 = new ToolStripSeparator();
            mnuProfileStrip = new ToolStripMenuItem();
            mnuScanSettingStrip = new ToolStripMenuItem();
            mnuScannerStrip = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            mnuLogoutStrip = new ToolStripMenuItem();
            toolStripSeparator8 = new ToolStripSeparator();
            mnuExitStrip = new ToolStripMenuItem();
            viewMenu = new ToolStripMenuItem();
            mnuProcSummStrip = new ToolStripMenuItem();
            toolStripSeparator7 = new ToolStripSeparator();
            toolBarToolStripMenuItem = new ToolStripMenuItem();
            statusBarToolStripMenuItem = new ToolStripMenuItem();
            tasksMenu = new ToolStripMenuItem();
            mnuScanStrip = new ToolStripMenuItem();
            mnuRescanStrip = new ToolStripMenuItem();
            toolStripSeparator3 = new ToolStripSeparator();
            mnuIndexingStrip = new ToolStripMenuItem();
            mnuRejectIdxStrip = new ToolStripMenuItem();
            toolStripSeparator4 = new ToolStripSeparator();
            mnuVerifyStrip = new ToolStripMenuItem();
            toolStripSeparator5 = new ToolStripSeparator();
            mnuExportStrip = new ToolStripMenuItem();
            toolStripSeparator10 = new ToolStripSeparator();
            mnuIDPStrip = new ToolStripMenuItem();
            windowsMenu = new ToolStripMenuItem();
            cascadeToolStripMenuItem = new ToolStripMenuItem();
            tileVerticalToolStripMenuItem = new ToolStripMenuItem();
            tileHorizontalToolStripMenuItem = new ToolStripMenuItem();
            closeAllToolStripMenuItem = new ToolStripMenuItem();
            arrangeIconsToolStripMenuItem = new ToolStripMenuItem();
            helpMenu = new ToolStripMenuItem();
            aboutToolStripMenuItem = new ToolStripMenuItem();
            helpToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator9 = new ToolStripSeparator();
            regLicToolStripMenuItem = new ToolStripMenuItem();
            licInfoToolStripMenuItem = new ToolStripMenuItem();
            toolStrip = new ToolStrip();
            ScanToolStripBtn = new ToolStripButton();
            toolStripLabel1 = new ToolStripLabel();
            ScanPrjToolStripCbx = new ToolStripComboBox();
            toolStripSeparator2 = new ToolStripSeparator();
            lblAppNameStrip = new ToolStripLabel();
            lblProjectStrip = new ToolStripLabel();
            statusStrip = new StatusStrip();
            MainStatusStrip = new ToolStripStatusLabel();
            toolTip = new ToolTip(components);
            menuStrip.SuspendLayout();
            toolStrip.SuspendLayout();
            statusStrip.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip
            // 
            menuStrip.ImageScalingSize = new Size(24, 24);
            menuStrip.Items.AddRange(new ToolStripItem[] { fileMenu, viewMenu, tasksMenu, windowsMenu, helpMenu });
            menuStrip.Location = new Point(0, 0);
            menuStrip.MdiWindowListItem = windowsMenu;
            menuStrip.Name = "menuStrip";
            menuStrip.Padding = new Padding(4, 2, 0, 2);
            menuStrip.Size = new Size(738, 24);
            menuStrip.TabIndex = 0;
            menuStrip.Text = "MenuStrip";
            // 
            // fileMenu
            // 
            fileMenu.DropDownItems.AddRange(new ToolStripItem[] { mnuStationStrip, toolStripSeparator6, mnuProfileStrip, mnuScanSettingStrip, mnuScannerStrip, toolStripSeparator1, mnuLogoutStrip, toolStripSeparator8, mnuExitStrip });
            fileMenu.ImageTransparentColor = SystemColors.ActiveBorder;
            fileMenu.Name = "fileMenu";
            fileMenu.Size = new Size(37, 20);
            fileMenu.Text = "&File";
            // 
            // mnuStationStrip
            // 
            mnuStationStrip.Name = "mnuStationStrip";
            mnuStationStrip.Size = new Size(198, 22);
            mnuStationStrip.Text = "Scan Station";
            mnuStationStrip.Click += mnuStationStrip_Click;
            // 
            // toolStripSeparator6
            // 
            toolStripSeparator6.Name = "toolStripSeparator6";
            toolStripSeparator6.Size = new Size(195, 6);
            // 
            // mnuProfileStrip
            // 
            mnuProfileStrip.Name = "mnuProfileStrip";
            mnuProfileStrip.Size = new Size(198, 22);
            mnuProfileStrip.Text = "Scanner Profile Settings";
            mnuProfileStrip.Click += mnuProfileStrip_Click;
            // 
            // mnuScanSettingStrip
            // 
            mnuScanSettingStrip.Name = "mnuScanSettingStrip";
            mnuScanSettingStrip.Size = new Size(198, 22);
            mnuScanSettingStrip.Text = "S&canner Settings";
            mnuScanSettingStrip.Click += mnuScanSettingStrip_Click;
            // 
            // mnuScannerStrip
            // 
            mnuScannerStrip.Name = "mnuScannerStrip";
            mnuScannerStrip.Size = new Size(198, 22);
            mnuScannerStrip.Text = "Scanner &Source";
            mnuScannerStrip.Click += mnuScannerStrip_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(195, 6);
            // 
            // mnuLogoutStrip
            // 
            mnuLogoutStrip.Name = "mnuLogoutStrip";
            mnuLogoutStrip.Size = new Size(198, 22);
            mnuLogoutStrip.Text = "Logout";
            mnuLogoutStrip.Click += mnuLogoutStrip_Click;
            // 
            // toolStripSeparator8
            // 
            toolStripSeparator8.Name = "toolStripSeparator8";
            toolStripSeparator8.Size = new Size(195, 6);
            // 
            // mnuExitStrip
            // 
            mnuExitStrip.Name = "mnuExitStrip";
            mnuExitStrip.Size = new Size(198, 22);
            mnuExitStrip.Text = "E&xit";
            mnuExitStrip.Click += ExitToolsStripMenuItem_Click;
            // 
            // viewMenu
            // 
            viewMenu.DropDownItems.AddRange(new ToolStripItem[] { mnuProcSummStrip, toolStripSeparator7, toolBarToolStripMenuItem, statusBarToolStripMenuItem });
            viewMenu.Name = "viewMenu";
            viewMenu.Size = new Size(44, 20);
            viewMenu.Text = "&View";
            // 
            // mnuProcSummStrip
            // 
            mnuProcSummStrip.Name = "mnuProcSummStrip";
            mnuProcSummStrip.Size = new Size(168, 22);
            mnuProcSummStrip.Text = "Process Summary";
            mnuProcSummStrip.Click += mnuProcSummStrip_Click;
            // 
            // toolStripSeparator7
            // 
            toolStripSeparator7.Name = "toolStripSeparator7";
            toolStripSeparator7.Size = new Size(165, 6);
            // 
            // toolBarToolStripMenuItem
            // 
            toolBarToolStripMenuItem.Checked = true;
            toolBarToolStripMenuItem.CheckOnClick = true;
            toolBarToolStripMenuItem.CheckState = CheckState.Checked;
            toolBarToolStripMenuItem.Name = "toolBarToolStripMenuItem";
            toolBarToolStripMenuItem.Size = new Size(168, 22);
            toolBarToolStripMenuItem.Text = "&Project Toolbar";
            toolBarToolStripMenuItem.Click += ToolBarToolStripMenuItem_Click;
            // 
            // statusBarToolStripMenuItem
            // 
            statusBarToolStripMenuItem.Checked = true;
            statusBarToolStripMenuItem.CheckOnClick = true;
            statusBarToolStripMenuItem.CheckState = CheckState.Checked;
            statusBarToolStripMenuItem.Name = "statusBarToolStripMenuItem";
            statusBarToolStripMenuItem.Size = new Size(168, 22);
            statusBarToolStripMenuItem.Text = "&Status Bar";
            statusBarToolStripMenuItem.Click += StatusBarToolStripMenuItem_Click;
            // 
            // tasksMenu
            // 
            tasksMenu.DropDownItems.AddRange(new ToolStripItem[] { mnuScanStrip, mnuRescanStrip, toolStripSeparator3, mnuIndexingStrip, mnuRejectIdxStrip, toolStripSeparator4, mnuVerifyStrip, toolStripSeparator5, mnuExportStrip, toolStripSeparator10, mnuIDPStrip });
            tasksMenu.Name = "tasksMenu";
            tasksMenu.Size = new Size(46, 20);
            tasksMenu.Text = "&Tasks";
            tasksMenu.Click += tasksMenu_Click;
            tasksMenu.MouseHover += tasksMenu_MouseHover;
            // 
            // mnuScanStrip
            // 
            mnuScanStrip.Name = "mnuScanStrip";
            mnuScanStrip.Size = new Size(169, 22);
            mnuScanStrip.Text = "&Scan";
            mnuScanStrip.Click += mnuScanStrip_Click;
            // 
            // mnuRescanStrip
            // 
            mnuRescanStrip.Name = "mnuRescanStrip";
            mnuRescanStrip.Size = new Size(169, 22);
            mnuRescanStrip.Text = "Rescan";
            mnuRescanStrip.Visible = false;
            mnuRescanStrip.Click += mnuRescanStrip_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(166, 6);
            toolStripSeparator3.Visible = false;
            // 
            // mnuIndexingStrip
            // 
            mnuIndexingStrip.Name = "mnuIndexingStrip";
            mnuIndexingStrip.Size = new Size(169, 22);
            mnuIndexingStrip.Text = "&Indexing";
            mnuIndexingStrip.Visible = false;
            mnuIndexingStrip.Click += mnuIndexingStrip_Click;
            // 
            // mnuRejectIdxStrip
            // 
            mnuRejectIdxStrip.Name = "mnuRejectIdxStrip";
            mnuRejectIdxStrip.Size = new Size(169, 22);
            mnuRejectIdxStrip.Text = "Reverted Indexing";
            mnuRejectIdxStrip.Visible = false;
            mnuRejectIdxStrip.Click += mnuRejectIdxStrip_Click;
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new Size(166, 6);
            // 
            // mnuVerifyStrip
            // 
            mnuVerifyStrip.Name = "mnuVerifyStrip";
            mnuVerifyStrip.Size = new Size(169, 22);
            mnuVerifyStrip.Text = "&Verify";
            mnuVerifyStrip.Click += mnuVerifyStrip_Click;
            // 
            // toolStripSeparator5
            // 
            toolStripSeparator5.Name = "toolStripSeparator5";
            toolStripSeparator5.Size = new Size(166, 6);
            // 
            // mnuExportStrip
            // 
            mnuExportStrip.Name = "mnuExportStrip";
            mnuExportStrip.Size = new Size(169, 22);
            mnuExportStrip.Text = "Export";
            mnuExportStrip.Click += mnuExportStrip_Click;
            // 
            // toolStripSeparator10
            // 
            toolStripSeparator10.Name = "toolStripSeparator10";
            toolStripSeparator10.Size = new Size(166, 6);
            // 
            // mnuIDPStrip
            // 
            mnuIDPStrip.Name = "mnuIDPStrip";
            mnuIDPStrip.Size = new Size(169, 22);
            mnuIDPStrip.Text = "IDP";
            mnuIDPStrip.Click += mnuIDPStrip_Click;
            // 
            // windowsMenu
            // 
            windowsMenu.DropDownItems.AddRange(new ToolStripItem[] { cascadeToolStripMenuItem, tileVerticalToolStripMenuItem, tileHorizontalToolStripMenuItem, closeAllToolStripMenuItem, arrangeIconsToolStripMenuItem });
            windowsMenu.Name = "windowsMenu";
            windowsMenu.Size = new Size(68, 20);
            windowsMenu.Text = "&Windows";
            // 
            // cascadeToolStripMenuItem
            // 
            cascadeToolStripMenuItem.Name = "cascadeToolStripMenuItem";
            cascadeToolStripMenuItem.Size = new Size(150, 22);
            cascadeToolStripMenuItem.Text = "&Cascade";
            cascadeToolStripMenuItem.Click += CascadeToolStripMenuItem_Click;
            // 
            // tileVerticalToolStripMenuItem
            // 
            tileVerticalToolStripMenuItem.Name = "tileVerticalToolStripMenuItem";
            tileVerticalToolStripMenuItem.Size = new Size(150, 22);
            tileVerticalToolStripMenuItem.Text = "Tile &Vertical";
            tileVerticalToolStripMenuItem.Click += TileVerticalToolStripMenuItem_Click;
            // 
            // tileHorizontalToolStripMenuItem
            // 
            tileHorizontalToolStripMenuItem.Name = "tileHorizontalToolStripMenuItem";
            tileHorizontalToolStripMenuItem.Size = new Size(150, 22);
            tileHorizontalToolStripMenuItem.Text = "Tile &Horizontal";
            tileHorizontalToolStripMenuItem.Click += TileHorizontalToolStripMenuItem_Click;
            // 
            // closeAllToolStripMenuItem
            // 
            closeAllToolStripMenuItem.Name = "closeAllToolStripMenuItem";
            closeAllToolStripMenuItem.Size = new Size(150, 22);
            closeAllToolStripMenuItem.Text = "C&lose All";
            closeAllToolStripMenuItem.Click += CloseAllToolStripMenuItem_Click;
            // 
            // arrangeIconsToolStripMenuItem
            // 
            arrangeIconsToolStripMenuItem.Name = "arrangeIconsToolStripMenuItem";
            arrangeIconsToolStripMenuItem.Size = new Size(150, 22);
            arrangeIconsToolStripMenuItem.Text = "&Arrange Icons";
            arrangeIconsToolStripMenuItem.Click += ArrangeIconsToolStripMenuItem_Click;
            // 
            // helpMenu
            // 
            helpMenu.DropDownItems.AddRange(new ToolStripItem[] { aboutToolStripMenuItem, helpToolStripMenuItem, toolStripSeparator9, regLicToolStripMenuItem, licInfoToolStripMenuItem });
            helpMenu.Name = "helpMenu";
            helpMenu.Size = new Size(44, 20);
            helpMenu.Text = "&Help";
            helpMenu.DropDownOpening += helpMenu_DropDownOpening;
            helpMenu.Click += helpMenu_Click;
            helpMenu.MouseHover += helpMenu_MouseHover;
            // 
            // aboutToolStripMenuItem
            // 
            aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            aboutToolStripMenuItem.Size = new Size(158, 22);
            aboutToolStripMenuItem.Text = "&About This";
            aboutToolStripMenuItem.Click += aboutToolStripMenuItem_Click;
            // 
            // helpToolStripMenuItem
            // 
            helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            helpToolStripMenuItem.Size = new Size(158, 22);
            helpToolStripMenuItem.Text = "&User Manual";
            helpToolStripMenuItem.Click += helpToolStripMenuItem_Click;
            // 
            // toolStripSeparator9
            // 
            toolStripSeparator9.Name = "toolStripSeparator9";
            toolStripSeparator9.Size = new Size(155, 6);
            // 
            // regLicToolStripMenuItem
            // 
            regLicToolStripMenuItem.Name = "regLicToolStripMenuItem";
            regLicToolStripMenuItem.Size = new Size(158, 22);
            regLicToolStripMenuItem.Text = "&Register License";
            regLicToolStripMenuItem.Click += regLicToolStripMenuItem_Click;
            // 
            // licInfoToolStripMenuItem
            // 
            licInfoToolStripMenuItem.Name = "licInfoToolStripMenuItem";
            licInfoToolStripMenuItem.Size = new Size(158, 22);
            licInfoToolStripMenuItem.Text = "&License";
            licInfoToolStripMenuItem.Click += licInfoToolStripMenuItem_Click;
            // 
            // toolStrip
            // 
            toolStrip.AutoSize = false;
            toolStrip.ImageScalingSize = new Size(20, 20);
            toolStrip.ImeMode = ImeMode.On;
            toolStrip.Items.AddRange(new ToolStripItem[] { ScanToolStripBtn, toolStripLabel1, ScanPrjToolStripCbx, toolStripSeparator2, lblAppNameStrip, lblProjectStrip });
            toolStrip.Location = new Point(0, 24);
            toolStrip.Name = "toolStrip";
            toolStrip.Padding = new Padding(0, 0, 3, 0);
            toolStrip.Size = new Size(738, 55);
            toolStrip.TabIndex = 1;
            toolStrip.Text = "ToolStrip";
            // 
            // ScanToolStripBtn
            // 
            ScanToolStripBtn.Enabled = false;
            ScanToolStripBtn.Image = (Image)resources.GetObject("ScanToolStripBtn.Image");
            ScanToolStripBtn.ImageTransparentColor = Color.Magenta;
            ScanToolStripBtn.Name = "ScanToolStripBtn";
            ScanToolStripBtn.Size = new Size(64, 52);
            ScanToolStripBtn.Text = "Scan Now";
            ScanToolStripBtn.TextAlign = ContentAlignment.BottomCenter;
            ScanToolStripBtn.TextDirection = ToolStripTextDirection.Horizontal;
            ScanToolStripBtn.TextImageRelation = TextImageRelation.ImageAboveText;
            ScanToolStripBtn.Click += ScanToolStripBtn_Click;
            // 
            // toolStripLabel1
            // 
            toolStripLabel1.Name = "toolStripLabel1";
            toolStripLabel1.Padding = new Padding(0, 0, 5, 0);
            toolStripLabel1.Size = new Size(120, 52);
            toolStripLabel1.Text = "Current Scan Project";
            // 
            // ScanPrjToolStripCbx
            // 
            ScanPrjToolStripCbx.AutoCompleteSource = AutoCompleteSource.ListItems;
            ScanPrjToolStripCbx.DropDownStyle = ComboBoxStyle.DropDownList;
            ScanPrjToolStripCbx.Name = "ScanPrjToolStripCbx";
            ScanPrjToolStripCbx.Size = new Size(225, 55);
            ScanPrjToolStripCbx.SelectedIndexChanged += ScanPrjToolStripCbx_SelectedIndexChanged;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(6, 55);
            // 
            // lblAppNameStrip
            // 
            lblAppNameStrip.Name = "lblAppNameStrip";
            lblAppNameStrip.Size = new Size(39, 52);
            lblAppNameStrip.Text = "Name";
            // 
            // lblProjectStrip
            // 
            lblProjectStrip.Name = "lblProjectStrip";
            lblProjectStrip.Size = new Size(28, 52);
            lblProjectStrip.Text = "Info";
            // 
            // statusStrip
            // 
            statusStrip.ImageScalingSize = new Size(24, 24);
            statusStrip.Items.AddRange(new ToolStripItem[] { MainStatusStrip });
            statusStrip.Location = new Point(0, 502);
            statusStrip.Name = "statusStrip";
            statusStrip.Padding = new Padding(3, 0, 17, 0);
            statusStrip.Size = new Size(738, 22);
            statusStrip.TabIndex = 2;
            statusStrip.Text = "StatusStrip";
            // 
            // MainStatusStrip
            // 
            MainStatusStrip.Name = "MainStatusStrip";
            MainStatusStrip.Size = new Size(39, 17);
            MainStatusStrip.Text = "Status";
            // 
            // MDIMain
            // 
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(738, 524);
            Controls.Add(statusStrip);
            Controls.Add(toolStrip);
            Controls.Add(menuStrip);
            Icon = (Icon)resources.GetObject("$this.Icon");
            IsMdiContainer = true;
            MainMenuStrip = menuStrip;
            Margin = new Padding(4);
            Name = "MDIMain";
            Text = "SR Mimzy Capture Gen. IDP";
            FormClosing += MDIMain_FormClosing;
            Load += MDIMain_Load;
            menuStrip.ResumeLayout(false);
            menuStrip.PerformLayout();
            toolStrip.ResumeLayout(false);
            toolStrip.PerformLayout();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }
        #endregion


        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tileHorizontalToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fileMenu;
        private System.Windows.Forms.ToolStripMenuItem mnuExitStrip;
        private System.Windows.Forms.ToolStripMenuItem viewMenu;
        private System.Windows.Forms.ToolStripMenuItem toolBarToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tasksMenu;
        private System.Windows.Forms.ToolStripMenuItem mnuScanStrip;
        private System.Windows.Forms.ToolStripMenuItem windowsMenu;
        private System.Windows.Forms.ToolStripMenuItem cascadeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tileVerticalToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem closeAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem arrangeIconsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpMenu;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripMenuItem mnuIndexingStrip;
        private System.Windows.Forms.ToolStripMenuItem statusBarToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mnuScannerStrip;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem mnuScanSettingStrip;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem mnuVerifyStrip;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem mnuRejectIdxStrip;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem mnuExportStrip;
        private System.Windows.Forms.ToolStripMenuItem mnuProfileStrip;
        private System.Windows.Forms.ToolStripMenuItem mnuStationStrip;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripMenuItem mnuRescanStrip;
        private System.Windows.Forms.ToolStripMenuItem mnuProcSummStrip;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripLabel lblAppNameStrip;
        internal System.Windows.Forms.ToolStripStatusLabel MainStatusStrip;
        private System.Windows.Forms.ToolStripMenuItem mnuLogoutStrip;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private System.Windows.Forms.ToolStripLabel lblProjectStrip;
        private System.Windows.Forms.ToolStripMenuItem regLicToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem licInfoToolStripMenuItem;
        public System.Windows.Forms.ToolStripComboBox ScanPrjToolStripCbx;
        public System.Windows.Forms.ToolStripButton ScanToolStripBtn;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator9;
        private ToolStripSeparator toolStripSeparator10;
        private ToolStripMenuItem mnuIDPStrip;
    }
}



