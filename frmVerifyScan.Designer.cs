
namespace SRDocScanIDP
{
    partial class frmVerifyScan
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmVerifyScan));
            dsvImg = new Dynamsoft.Forms.DSViewer();
            txtInfo = new TextBox();
            IndexStatusStrip = new StatusStrip();
            VerifyingStatusBar = new ToolStripStatusLabel();
            VerifyingStatusBar1 = new ToolStripStatusLabel();
            VerifyingStatusBar2 = new ToolStripStatusLabel();
            CurrDateTime = new ToolStripStatusLabel();
            VerifyToolStrip = new ToolStrip();
            btnRotateRightStrip = new ToolStripButton();
            btnRotateLeftStrip = new ToolStripButton();
            toolStripSeparator4 = new ToolStripSeparator();
            btnActualStrip = new ToolStripButton();
            btnFitStrip = new ToolStripButton();
            btnZoomInStrip = new ToolStripButton();
            btnZoomOutStrip = new ToolStripButton();
            btnExpaAllStrip = new ToolStripButton();
            btnCollAllStrip = new ToolStripButton();
            toolStripSeparator2 = new ToolStripSeparator();
            btnPrevSetStrip = new ToolStripButton();
            btnNextSetStrip = new ToolStripButton();
            toolStripSeparator1 = new ToolStripSeparator();
            boxStripLbl = new ToolStripLabel();
            boxNoStripTxt = new ToolStripTextBox();
            boxLabelStripLbl = new ToolStripLabel();
            boxLabelStripTxt = new ToolStripTextBox();
            timer1 = new System.Windows.Forms.Timer(components);
            dsvThumbnailList = new Dynamsoft.Forms.DSViewer();
            toolTip1 = new ToolTip(components);
            tvwSet = new TreeView();
            label1 = new Label();
            txtRemarks = new TextBox();
            panel1 = new Panel();
            txtTotImgNum = new TextBox();
            txtCurrImgIdx = new TextBox();
            lbDiv = new Label();
            picboxPrevious = new PictureBox();
            picboxNext = new PictureBox();
            picboxLast = new PictureBox();
            picboxFirst = new PictureBox();
            IndexStatusStrip.SuspendLayout();
            VerifyToolStrip.SuspendLayout();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picboxPrevious).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picboxNext).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picboxLast).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picboxFirst).BeginInit();
            SuspendLayout();
            // 
            // dsvImg
            // 
            dsvImg.Location = new Point(268, 47);
            dsvImg.Margin = new Padding(3, 2, 3, 2);
            dsvImg.Name = "dsvImg";
            dsvImg.RightToLeft = RightToLeft.No;
            dsvImg.SelectionRectAspectRatio = 0D;
            dsvImg.Size = new Size(1050, 592);
            dsvImg.TabIndex = 15;
            toolTip1.SetToolTip(dsvImg, "Hold Ctrl + Mouse to zoom and drag");
            // 
            // txtInfo
            // 
            txtInfo.BackColor = SystemColors.ControlLight;
            txtInfo.Location = new Point(268, 648);
            txtInfo.Margin = new Padding(3, 2, 3, 2);
            txtInfo.Name = "txtInfo";
            txtInfo.ReadOnly = true;
            txtInfo.Size = new Size(750, 23);
            txtInfo.TabIndex = 20;
            // 
            // IndexStatusStrip
            // 
            IndexStatusStrip.ImageScalingSize = new Size(24, 24);
            IndexStatusStrip.Items.AddRange(new ToolStripItem[] { VerifyingStatusBar, VerifyingStatusBar1, VerifyingStatusBar2, CurrDateTime });
            IndexStatusStrip.Location = new Point(0, 682);
            IndexStatusStrip.Name = "IndexStatusStrip";
            IndexStatusStrip.Padding = new Padding(1, 0, 10, 0);
            IndexStatusStrip.Size = new Size(1496, 35);
            IndexStatusStrip.TabIndex = 60;
            IndexStatusStrip.Text = "statusStrip1";
            // 
            // VerifyingStatusBar
            // 
            VerifyingStatusBar.AutoSize = false;
            VerifyingStatusBar.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            VerifyingStatusBar.BorderStyle = Border3DStyle.Sunken;
            VerifyingStatusBar.ImageScaling = ToolStripItemImageScaling.None;
            VerifyingStatusBar.Name = "VerifyingStatusBar";
            VerifyingStatusBar.Size = new Size(268, 30);
            VerifyingStatusBar.Text = "Verifying";
            // 
            // VerifyingStatusBar1
            // 
            VerifyingStatusBar1.AutoSize = false;
            VerifyingStatusBar1.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            VerifyingStatusBar1.BorderStyle = Border3DStyle.Sunken;
            VerifyingStatusBar1.Name = "VerifyingStatusBar1";
            VerifyingStatusBar1.Size = new Size(608, 30);
            VerifyingStatusBar1.Text = "   ";
            // 
            // VerifyingStatusBar2
            // 
            VerifyingStatusBar2.AutoSize = false;
            VerifyingStatusBar2.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            VerifyingStatusBar2.BorderStyle = Border3DStyle.Sunken;
            VerifyingStatusBar2.Name = "VerifyingStatusBar2";
            VerifyingStatusBar2.Size = new Size(308, 30);
            // 
            // CurrDateTime
            // 
            CurrDateTime.AutoSize = false;
            CurrDateTime.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            CurrDateTime.BorderStyle = Border3DStyle.Sunken;
            CurrDateTime.Name = "CurrDateTime";
            CurrDateTime.Size = new Size(250, 30);
            CurrDateTime.Text = "Curr. Date Time";
            CurrDateTime.TextAlign = ContentAlignment.MiddleRight;
            // 
            // VerifyToolStrip
            // 
            VerifyToolStrip.AutoSize = false;
            VerifyToolStrip.ImageScalingSize = new Size(24, 24);
            VerifyToolStrip.Items.AddRange(new ToolStripItem[] { btnRotateRightStrip, btnRotateLeftStrip, toolStripSeparator4, btnActualStrip, btnFitStrip, btnZoomInStrip, btnZoomOutStrip, btnExpaAllStrip, btnCollAllStrip, toolStripSeparator2, btnPrevSetStrip, btnNextSetStrip, toolStripSeparator1, boxStripLbl, boxNoStripTxt, boxLabelStripLbl, boxLabelStripTxt });
            VerifyToolStrip.Location = new Point(0, 0);
            VerifyToolStrip.Name = "VerifyToolStrip";
            VerifyToolStrip.Size = new Size(1496, 45);
            VerifyToolStrip.TabIndex = 61;
            VerifyToolStrip.Text = "Verify Tools";
            // 
            // btnRotateRightStrip
            // 
            btnRotateRightStrip.Image = (Image)resources.GetObject("btnRotateRightStrip.Image");
            btnRotateRightStrip.ImageTransparentColor = Color.Magenta;
            btnRotateRightStrip.Name = "btnRotateRightStrip";
            btnRotateRightStrip.Size = new Size(76, 42);
            btnRotateRightStrip.Text = "Rotate Right";
            btnRotateRightStrip.TextAlign = ContentAlignment.BottomCenter;
            btnRotateRightStrip.TextImageRelation = TextImageRelation.ImageAboveText;
            btnRotateRightStrip.Click += btnRotateRightStrip_Click;
            // 
            // btnRotateLeftStrip
            // 
            btnRotateLeftStrip.Image = (Image)resources.GetObject("btnRotateLeftStrip.Image");
            btnRotateLeftStrip.ImageTransparentColor = Color.Magenta;
            btnRotateLeftStrip.Name = "btnRotateLeftStrip";
            btnRotateLeftStrip.Size = new Size(68, 42);
            btnRotateLeftStrip.Text = "Rotate Left";
            btnRotateLeftStrip.TextAlign = ContentAlignment.BottomCenter;
            btnRotateLeftStrip.TextImageRelation = TextImageRelation.ImageAboveText;
            btnRotateLeftStrip.Click += btnRotateLeftStrip_Click;
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new Size(6, 45);
            // 
            // btnActualStrip
            // 
            btnActualStrip.Image = (Image)resources.GetObject("btnActualStrip.Image");
            btnActualStrip.ImageTransparentColor = Color.Magenta;
            btnActualStrip.Name = "btnActualStrip";
            btnActualStrip.Size = new Size(68, 42);
            btnActualStrip.Text = "Actual Size";
            btnActualStrip.TextAlign = ContentAlignment.BottomCenter;
            btnActualStrip.TextImageRelation = TextImageRelation.ImageAboveText;
            btnActualStrip.Click += btnActualStrip_Click;
            // 
            // btnFitStrip
            // 
            btnFitStrip.Image = (Image)resources.GetObject("btnFitStrip.Image");
            btnFitStrip.ImageTransparentColor = Color.Magenta;
            btnFitStrip.Name = "btnFitStrip";
            btnFitStrip.Size = new Size(62, 42);
            btnFitStrip.Text = "Fit Screen";
            btnFitStrip.TextAlign = ContentAlignment.BottomCenter;
            btnFitStrip.TextImageRelation = TextImageRelation.ImageAboveText;
            btnFitStrip.Click += btnFitStrip_Click;
            // 
            // btnZoomInStrip
            // 
            btnZoomInStrip.Image = (Image)resources.GetObject("btnZoomInStrip.Image");
            btnZoomInStrip.ImageTransparentColor = Color.Magenta;
            btnZoomInStrip.Name = "btnZoomInStrip";
            btnZoomInStrip.Size = new Size(56, 42);
            btnZoomInStrip.Text = "Zoom In";
            btnZoomInStrip.TextAlign = ContentAlignment.BottomCenter;
            btnZoomInStrip.TextImageRelation = TextImageRelation.ImageAboveText;
            btnZoomInStrip.Click += btnZoomInStrip_Click;
            // 
            // btnZoomOutStrip
            // 
            btnZoomOutStrip.Image = (Image)resources.GetObject("btnZoomOutStrip.Image");
            btnZoomOutStrip.ImageTransparentColor = Color.Magenta;
            btnZoomOutStrip.Name = "btnZoomOutStrip";
            btnZoomOutStrip.Size = new Size(66, 42);
            btnZoomOutStrip.Text = "Zoom Out";
            btnZoomOutStrip.TextAlign = ContentAlignment.BottomCenter;
            btnZoomOutStrip.TextImageRelation = TextImageRelation.ImageAboveText;
            btnZoomOutStrip.Click += btnZoomOutStrip_Click;
            // 
            // btnExpaAllStrip
            // 
            btnExpaAllStrip.Image = (Image)resources.GetObject("btnExpaAllStrip.Image");
            btnExpaAllStrip.ImageTransparentColor = Color.Magenta;
            btnExpaAllStrip.Name = "btnExpaAllStrip";
            btnExpaAllStrip.Size = new Size(95, 42);
            btnExpaAllStrip.Text = "Expand All View";
            btnExpaAllStrip.TextAlign = ContentAlignment.BottomCenter;
            btnExpaAllStrip.TextImageRelation = TextImageRelation.ImageAboveText;
            btnExpaAllStrip.Click += btnExpaAllStrip_Click;
            // 
            // btnCollAllStrip
            // 
            btnCollAllStrip.Image = (Image)resources.GetObject("btnCollAllStrip.Image");
            btnCollAllStrip.ImageTransparentColor = Color.Magenta;
            btnCollAllStrip.Name = "btnCollAllStrip";
            btnCollAllStrip.Size = new Size(101, 42);
            btnCollAllStrip.Text = "Collapse All View";
            btnCollAllStrip.TextAlign = ContentAlignment.BottomCenter;
            btnCollAllStrip.TextImageRelation = TextImageRelation.ImageAboveText;
            btnCollAllStrip.Click += btnCollAllStrip_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(6, 45);
            // 
            // btnPrevSetStrip
            // 
            btnPrevSetStrip.Image = (Image)resources.GetObject("btnPrevSetStrip.Image");
            btnPrevSetStrip.ImageTransparentColor = Color.Magenta;
            btnPrevSetStrip.Name = "btnPrevSetStrip";
            btnPrevSetStrip.Size = new Size(89, 42);
            btnPrevSetStrip.Text = "Previous Batch";
            btnPrevSetStrip.TextAlign = ContentAlignment.BottomCenter;
            btnPrevSetStrip.TextImageRelation = TextImageRelation.ImageAboveText;
            btnPrevSetStrip.Click += btnPrevSetStrip_Click;
            // 
            // btnNextSetStrip
            // 
            btnNextSetStrip.Image = (Image)resources.GetObject("btnNextSetStrip.Image");
            btnNextSetStrip.ImageTransparentColor = Color.Magenta;
            btnNextSetStrip.Name = "btnNextSetStrip";
            btnNextSetStrip.Size = new Size(69, 42);
            btnNextSetStrip.Text = "Next Batch";
            btnNextSetStrip.TextAlign = ContentAlignment.BottomCenter;
            btnNextSetStrip.TextImageRelation = TextImageRelation.ImageAboveText;
            btnNextSetStrip.Click += btnNextSetStrip_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 45);
            // 
            // boxStripLbl
            // 
            boxStripLbl.Name = "boxStripLbl";
            boxStripLbl.Size = new Size(34, 42);
            boxStripLbl.Text = "Box#";
            // 
            // boxNoStripTxt
            // 
            boxNoStripTxt.BackColor = SystemColors.ControlLight;
            boxNoStripTxt.Font = new Font("Arial", 8F);
            boxNoStripTxt.MaxLength = 8;
            boxNoStripTxt.Name = "boxNoStripTxt";
            boxNoStripTxt.ReadOnly = true;
            boxNoStripTxt.Size = new Size(69, 45);
            // 
            // boxLabelStripLbl
            // 
            boxLabelStripLbl.Name = "boxLabelStripLbl";
            boxLabelStripLbl.Size = new Size(58, 42);
            boxLabelStripLbl.Text = "Box Label";
            // 
            // boxLabelStripTxt
            // 
            boxLabelStripTxt.Font = new Font("Arial", 8F);
            boxLabelStripTxt.MaxLength = 30;
            boxLabelStripTxt.Name = "boxLabelStripTxt";
            boxLabelStripTxt.ReadOnly = true;
            boxLabelStripTxt.Size = new Size(184, 45);
            // 
            // timer1
            // 
            timer1.Tick += timer1_Tick;
            // 
            // dsvThumbnailList
            // 
            dsvThumbnailList.AutoScroll = true;
            dsvThumbnailList.Location = new Point(1324, 47);
            dsvThumbnailList.Margin = new Padding(3, 2, 3, 2);
            dsvThumbnailList.Name = "dsvThumbnailList";
            dsvThumbnailList.RightToLeft = RightToLeft.No;
            dsvThumbnailList.SelectionRectAspectRatio = 0D;
            dsvThumbnailList.Size = new Size(158, 627);
            dsvThumbnailList.TabIndex = 70;
            dsvThumbnailList.OnMouseClick += dsvThumbnailList_OnMouseClick;
            // 
            // tvwSet
            // 
            tvwSet.Location = new Point(5, 71);
            tvwSet.Margin = new Padding(3, 2, 3, 2);
            tvwSet.Name = "tvwSet";
            tvwSet.Size = new Size(258, 605);
            tvwSet.TabIndex = 13;
            tvwSet.AfterSelect += tvwSet_AfterSelect;
            tvwSet.KeyPress += tvwSet_KeyPress;
            // 
            // label1
            // 
            label1.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label1.Location = new Point(10, 47);
            label1.Name = "label1";
            label1.Size = new Size(248, 20);
            label1.TabIndex = 12;
            label1.Text = "Verify Document Set View";
            label1.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // txtRemarks
            // 
            txtRemarks.Location = new Point(228, 62);
            txtRemarks.MaxLength = 250;
            txtRemarks.Name = "txtRemarks";
            txtRemarks.Size = new Size(440, 23);
            txtRemarks.TabIndex = 10;
            txtRemarks.Visible = false;
            // 
            // panel1
            // 
            panel1.Controls.Add(txtTotImgNum);
            panel1.Controls.Add(txtCurrImgIdx);
            panel1.Controls.Add(lbDiv);
            panel1.Controls.Add(picboxPrevious);
            panel1.Controls.Add(picboxNext);
            panel1.Controls.Add(picboxLast);
            panel1.Controls.Add(picboxFirst);
            panel1.Location = new Point(1023, 645);
            panel1.Name = "panel1";
            panel1.Size = new Size(296, 27);
            panel1.TabIndex = 71;
            // 
            // txtTotImgNum
            // 
            txtTotImgNum.Enabled = false;
            txtTotImgNum.Location = new Point(158, 2);
            txtTotImgNum.Margin = new Padding(3, 2, 3, 2);
            txtTotImgNum.Name = "txtTotImgNum";
            txtTotImgNum.ReadOnly = true;
            txtTotImgNum.Size = new Size(49, 23);
            txtTotImgNum.TabIndex = 73;
            txtTotImgNum.Text = "0";
            // 
            // txtCurrImgIdx
            // 
            txtCurrImgIdx.Enabled = false;
            txtCurrImgIdx.Location = new Point(91, 2);
            txtCurrImgIdx.Margin = new Padding(3, 2, 3, 2);
            txtCurrImgIdx.Name = "txtCurrImgIdx";
            txtCurrImgIdx.ReadOnly = true;
            txtCurrImgIdx.Size = new Size(49, 23);
            txtCurrImgIdx.TabIndex = 71;
            txtCurrImgIdx.Text = "0";
            txtCurrImgIdx.TextAlign = HorizontalAlignment.Right;
            // 
            // lbDiv
            // 
            lbDiv.AutoSize = true;
            lbDiv.BackColor = Color.Transparent;
            lbDiv.Location = new Point(143, 5);
            lbDiv.Name = "lbDiv";
            lbDiv.Size = new Size(12, 15);
            lbDiv.TabIndex = 72;
            lbDiv.Text = "/";
            // 
            // picboxPrevious
            // 
            picboxPrevious.BorderStyle = BorderStyle.FixedSingle;
            picboxPrevious.Image = Properties.Resources.picboxPrevious_Enter;
            picboxPrevious.Location = new Point(47, 4);
            picboxPrevious.Margin = new Padding(3, 2, 3, 2);
            picboxPrevious.Name = "picboxPrevious";
            picboxPrevious.Size = new Size(40, 18);
            picboxPrevious.SizeMode = PictureBoxSizeMode.CenterImage;
            picboxPrevious.TabIndex = 70;
            picboxPrevious.TabStop = false;
            picboxPrevious.Tag = "Previous Image";
            picboxPrevious.Click += picboxPrevious_Click;
            // 
            // picboxNext
            // 
            picboxNext.BorderStyle = BorderStyle.FixedSingle;
            picboxNext.Image = Properties.Resources.picboxNext_Enter;
            picboxNext.Location = new Point(210, 4);
            picboxNext.Margin = new Padding(3, 2, 3, 2);
            picboxNext.Name = "picboxNext";
            picboxNext.Size = new Size(40, 18);
            picboxNext.SizeMode = PictureBoxSizeMode.CenterImage;
            picboxNext.TabIndex = 74;
            picboxNext.TabStop = false;
            picboxNext.Tag = "Next Image";
            picboxNext.Click += picboxNext_Click;
            // 
            // picboxLast
            // 
            picboxLast.BorderStyle = BorderStyle.FixedSingle;
            picboxLast.Image = Properties.Resources.picboxLast_Enter;
            picboxLast.Location = new Point(253, 4);
            picboxLast.Margin = new Padding(3, 2, 3, 2);
            picboxLast.Name = "picboxLast";
            picboxLast.Size = new Size(40, 18);
            picboxLast.SizeMode = PictureBoxSizeMode.CenterImage;
            picboxLast.TabIndex = 75;
            picboxLast.TabStop = false;
            picboxLast.Tag = "Last Image";
            picboxLast.Click += picboxLast_Click;
            // 
            // picboxFirst
            // 
            picboxFirst.BorderStyle = BorderStyle.FixedSingle;
            picboxFirst.Image = Properties.Resources.picboxFirst_Enter;
            picboxFirst.Location = new Point(4, 4);
            picboxFirst.Margin = new Padding(3, 2, 3, 2);
            picboxFirst.Name = "picboxFirst";
            picboxFirst.Size = new Size(40, 18);
            picboxFirst.SizeMode = PictureBoxSizeMode.CenterImage;
            picboxFirst.TabIndex = 69;
            picboxFirst.TabStop = false;
            picboxFirst.Tag = "First Image";
            picboxFirst.Click += picboxFirst_Click;
            // 
            // frmVerifyScan
            // 
            AutoScaleMode = AutoScaleMode.None;
            AutoScroll = true;
            ClientSize = new Size(1496, 717);
            Controls.Add(panel1);
            Controls.Add(dsvImg);
            Controls.Add(IndexStatusStrip);
            Controls.Add(label1);
            Controls.Add(tvwSet);
            Controls.Add(dsvThumbnailList);
            Controls.Add(VerifyToolStrip);
            Controls.Add(txtInfo);
            Controls.Add(txtRemarks);
            FormBorderStyle = FormBorderStyle.Fixed3D;
            Margin = new Padding(3, 2, 3, 2);
            MinimizeBox = false;
            Name = "frmVerifyScan";
            ShowIcon = false;
            Text = "Scan Verification";
            WindowState = FormWindowState.Maximized;
            Activated += frmVerifyScan_Activated;
            FormClosed += frmVerifyScan_FormClosed;
            Load += frmVerifyScan_Load;
            Paint += frmVerifyScan_Paint;
            Resize += frmVerifyScan_Resize;
            IndexStatusStrip.ResumeLayout(false);
            IndexStatusStrip.PerformLayout();
            VerifyToolStrip.ResumeLayout(false);
            VerifyToolStrip.PerformLayout();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picboxPrevious).EndInit();
            ((System.ComponentModel.ISupportInitialize)picboxNext).EndInit();
            ((System.ComponentModel.ISupportInitialize)picboxLast).EndInit();
            ((System.ComponentModel.ISupportInitialize)picboxFirst).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Dynamsoft.Forms.DSViewer dsvImg;
        private System.Windows.Forms.TextBox txtInfo;
        private System.Windows.Forms.StatusStrip IndexStatusStrip;
        private System.Windows.Forms.ToolStripStatusLabel VerifyingStatusBar;
        private System.Windows.Forms.ToolStrip VerifyToolStrip;
        private System.Windows.Forms.ToolStripStatusLabel VerifyingStatusBar1;
        private System.Windows.Forms.ToolStripStatusLabel CurrDateTime;
        private System.Windows.Forms.ToolStripStatusLabel VerifyingStatusBar2;
        private System.Windows.Forms.Timer timer1;
        private Dynamsoft.Forms.DSViewer dsvThumbnailList;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.TreeView tvwSet;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtRemarks;
        private System.Windows.Forms.ToolStripButton btnActualStrip;
        private System.Windows.Forms.ToolStripButton btnFitStrip;
        private System.Windows.Forms.ToolStripButton btnZoomInStrip;
        private System.Windows.Forms.ToolStripButton btnZoomOutStrip;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripButton btnRotateRightStrip;
        private System.Windows.Forms.ToolStripButton btnRotateLeftStrip;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton btnPrevSetStrip;
        private System.Windows.Forms.ToolStripButton btnNextSetStrip;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton btnExpaAllStrip;
        private System.Windows.Forms.ToolStripButton btnCollAllStrip;
        private System.Windows.Forms.ToolStripLabel boxStripLbl;
        private System.Windows.Forms.ToolStripTextBox boxNoStripTxt;
        private System.Windows.Forms.ToolStripLabel boxLabelStripLbl;
        private System.Windows.Forms.ToolStripTextBox boxLabelStripTxt;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox txtTotImgNum;
        private System.Windows.Forms.TextBox txtCurrImgIdx;
        private System.Windows.Forms.Label lbDiv;
        private System.Windows.Forms.PictureBox picboxPrevious;
        private System.Windows.Forms.PictureBox picboxNext;
        private System.Windows.Forms.PictureBox picboxLast;
        private System.Windows.Forms.PictureBox picboxFirst;
    }
}

