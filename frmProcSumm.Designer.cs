
namespace SRDocScanIDP
{
    partial class frmProcSumm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmProcSumm));
            ProcToolStrip = new ToolStrip();
            toolStripSeparator1 = new ToolStripSeparator();
            btnProcRefreshStrip = new ToolStripButton();
            toolStripSeparator2 = new ToolStripSeparator();
            ProcStatusStrip = new StatusStrip();
            ProcStatusBar = new ToolStripStatusLabel();
            ProcStatusBar1 = new ToolStripStatusLabel();
            ProcStatusBar2 = new ToolStripStatusLabel();
            CurrDateTime = new ToolStripStatusLabel();
            timer1 = new System.Windows.Forms.Timer(components);
            toolTip1 = new ToolTip(components);
            tvwProcess = new TreeView();
            lvwBatches = new ListView();
            colBatchCode = new ColumnHeader();
            colTotPage = new ColumnHeader();
            colScanStart = new ColumnHeader();
            colScanEnd = new ColumnHeader();
            colFrom = new ColumnHeader();
            colCreatDate = new ColumnHeader();
            colModifDate = new ColumnHeader();
            ProcToolStrip.SuspendLayout();
            ProcStatusStrip.SuspendLayout();
            SuspendLayout();
            // 
            // ProcToolStrip
            // 
            ProcToolStrip.AutoSize = false;
            ProcToolStrip.ImageScalingSize = new Size(24, 24);
            ProcToolStrip.Items.AddRange(new ToolStripItem[] { toolStripSeparator1, btnProcRefreshStrip, toolStripSeparator2 });
            ProcToolStrip.Location = new Point(0, 0);
            ProcToolStrip.Name = "ProcToolStrip";
            ProcToolStrip.Size = new Size(1704, 48);
            ProcToolStrip.TabIndex = 1;
            ProcToolStrip.Text = "toolStrip1";
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 48);
            // 
            // btnProcRefreshStrip
            // 
            btnProcRefreshStrip.DisplayStyle = ToolStripItemDisplayStyle.Image;
            btnProcRefreshStrip.Image = (Image)resources.GetObject("btnProcRefreshStrip.Image");
            btnProcRefreshStrip.ImageTransparentColor = Color.Magenta;
            btnProcRefreshStrip.Name = "btnProcRefreshStrip";
            btnProcRefreshStrip.Size = new Size(29, 45);
            btnProcRefreshStrip.Text = "Refresh Process List";
            btnProcRefreshStrip.Click += btnProcRefreshStrip_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(6, 48);
            // 
            // ProcStatusStrip
            // 
            ProcStatusStrip.ImageScalingSize = new Size(24, 24);
            ProcStatusStrip.Items.AddRange(new ToolStripItem[] { ProcStatusBar, ProcStatusBar1, ProcStatusBar2, CurrDateTime });
            ProcStatusStrip.Location = new Point(0, 898);
            ProcStatusStrip.Name = "ProcStatusStrip";
            ProcStatusStrip.Padding = new Padding(1, 0, 12, 0);
            ProcStatusStrip.Size = new Size(1704, 35);
            ProcStatusStrip.TabIndex = 70;
            // 
            // ProcStatusBar
            // 
            ProcStatusBar.AutoSize = false;
            ProcStatusBar.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            ProcStatusBar.BorderStyle = Border3DStyle.Sunken;
            ProcStatusBar.ImageScaling = ToolStripItemImageScaling.None;
            ProcStatusBar.Name = "ProcStatusBar";
            ProcStatusBar.Size = new Size(268, 29);
            // 
            // ProcStatusBar1
            // 
            ProcStatusBar1.AutoSize = false;
            ProcStatusBar1.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            ProcStatusBar1.BorderStyle = Border3DStyle.Sunken;
            ProcStatusBar1.Name = "ProcStatusBar1";
            ProcStatusBar1.Size = new Size(713, 29);
            ProcStatusBar1.Text = "   ";
            // 
            // ProcStatusBar2
            // 
            ProcStatusBar2.AutoSize = false;
            ProcStatusBar2.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            ProcStatusBar2.BorderStyle = Border3DStyle.Sunken;
            ProcStatusBar2.Name = "ProcStatusBar2";
            ProcStatusBar2.Size = new Size(506, 29);
            // 
            // CurrDateTime
            // 
            CurrDateTime.AutoSize = false;
            CurrDateTime.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            CurrDateTime.BorderStyle = Border3DStyle.Sunken;
            CurrDateTime.Name = "CurrDateTime";
            CurrDateTime.Size = new Size(190, 29);
            CurrDateTime.Text = "Curr. Date Time";
            CurrDateTime.TextAlign = ContentAlignment.MiddleRight;
            // 
            // timer1
            // 
            timer1.Tick += timer1_Tick;
            // 
            // tvwProcess
            // 
            tvwProcess.Location = new Point(8, 56);
            tvwProcess.Margin = new Padding(3, 2, 3, 2);
            tvwProcess.Name = "tvwProcess";
            tvwProcess.Size = new Size(294, 838);
            tvwProcess.TabIndex = 20;
            tvwProcess.AfterSelect += tvwProcess_AfterSelect;
            // 
            // lvwBatches
            // 
            lvwBatches.BackColor = SystemColors.ControlLight;
            lvwBatches.Columns.AddRange(new ColumnHeader[] { colBatchCode, colTotPage, colScanStart, colScanEnd, colFrom, colCreatDate, colModifDate });
            lvwBatches.FullRowSelect = true;
            lvwBatches.GridLines = true;
            lvwBatches.Location = new Point(307, 56);
            lvwBatches.Margin = new Padding(3, 4, 3, 4);
            lvwBatches.MultiSelect = false;
            lvwBatches.Name = "lvwBatches";
            lvwBatches.Size = new Size(1388, 838);
            lvwBatches.TabIndex = 30;
            lvwBatches.UseCompatibleStateImageBehavior = false;
            lvwBatches.View = View.Details;
            lvwBatches.ColumnClick += lvwBatches_ColumnClick;
            lvwBatches.MouseClick += lvwBatches_MouseClick;
            lvwBatches.MouseDoubleClick += lvwBatches_MouseDoubleClick;
            // 
            // colBatchCode
            // 
            colBatchCode.Text = "Batch";
            colBatchCode.Width = 190;
            // 
            // colTotPage
            // 
            colTotPage.Text = "Total Page";
            colTotPage.TextAlign = HorizontalAlignment.Center;
            colTotPage.Width = 80;
            // 
            // colScanStart
            // 
            colScanStart.Text = "Scan Start";
            colScanStart.TextAlign = HorizontalAlignment.Center;
            colScanStart.Width = 130;
            // 
            // colScanEnd
            // 
            colScanEnd.Text = "Scan End";
            colScanEnd.TextAlign = HorizontalAlignment.Center;
            colScanEnd.Width = 130;
            // 
            // colFrom
            // 
            colFrom.Text = "From";
            colFrom.Width = 80;
            // 
            // colCreatDate
            // 
            colCreatDate.Text = "Created Date";
            colCreatDate.TextAlign = HorizontalAlignment.Center;
            colCreatDate.Width = 130;
            // 
            // colModifDate
            // 
            colModifDate.Text = "Modified Date";
            colModifDate.TextAlign = HorizontalAlignment.Center;
            colModifDate.Width = 130;
            // 
            // frmProcSumm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1704, 933);
            Controls.Add(ProcStatusStrip);
            Controls.Add(lvwBatches);
            Controls.Add(tvwProcess);
            Controls.Add(ProcToolStrip);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Margin = new Padding(3, 4, 3, 4);
            MinimizeBox = false;
            Name = "frmProcSumm";
            ShowIcon = false;
            Text = "Process Summary";
            WindowState = FormWindowState.Maximized;
            Activated += frmProcSumm_Activated;
            FormClosed += frmProcSumm_FormClosed;
            Load += frmProcSumm_Load;
            Paint += frmProcSumm_Paint;
            Resize += frmProcSumm_Resize;
            ProcToolStrip.ResumeLayout(false);
            ProcToolStrip.PerformLayout();
            ProcStatusStrip.ResumeLayout(false);
            ProcStatusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolStrip ProcToolStrip;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.StatusStrip ProcStatusStrip;
        private System.Windows.Forms.ToolStripStatusLabel ProcStatusBar;
        private System.Windows.Forms.ToolStripStatusLabel ProcStatusBar1;
        private System.Windows.Forms.ToolStripStatusLabel ProcStatusBar2;
        private System.Windows.Forms.ToolStripStatusLabel CurrDateTime;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ListView lvwBatches;
        private System.Windows.Forms.ColumnHeader colBatchCode;
        private System.Windows.Forms.ColumnHeader colTotPage;
        private System.Windows.Forms.ColumnHeader colCreatDate;
        private System.Windows.Forms.ColumnHeader colModifDate;
        private System.Windows.Forms.ColumnHeader colScanStart;
        private System.Windows.Forms.ColumnHeader colScanEnd;
        private System.Windows.Forms.ToolStripButton btnProcRefreshStrip;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ColumnHeader colFrom;
        public System.Windows.Forms.TreeView tvwProcess;
    }
}