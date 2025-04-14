
namespace SRDocScanIDP
{
    partial class frmScanBox1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmScanBox1));
            tvwBatch = new TreeView();
            dsvImg = new Dynamsoft.Forms.DSViewer();
            txtInfo = new TextBox();
            ScanStatusStrip = new StatusStrip();
            ScanStatusBar = new ToolStripStatusLabel();
            ScanProgressBar = new ToolStripProgressBar();
            ScanStatusBar1 = new ToolStripStatusLabel();
            CurrDateTime = new ToolStripStatusLabel();
            ScanToolStrip = new ToolStrip();
            btnCloseStrip = new ToolStripButton();
            toolStripSeparator1 = new ToolStripSeparator();
            btnRotateRightStrip = new ToolStripButton();
            btnRotateLeftStrip = new ToolStripButton();
            btnMirrorStrip = new ToolStripButton();
            btnFlipStrip = new ToolStripButton();
            btnDeskewStrip = new ToolStripButton();
            toolStripSeparator11 = new ToolStripSeparator();
            btnActualStrip = new ToolStripButton();
            btnFitStrip = new ToolStripButton();
            btnZoomOutStrip = new ToolStripButton();
            btnZoomInStrip = new ToolStripButton();
            btnExpaAllStrip = new ToolStripButton();
            btnCollAllStrip = new ToolStripButton();
            toolStripSeparator2 = new ToolStripSeparator();
            btnHandStrip = new ToolStripButton();
            btnPointStrip = new ToolStripButton();
            btnCutAreaStrip = new ToolStripButton();
            btnCropAreaStrip = new ToolStripButton();
            toolStripSeparator5 = new ToolStripSeparator();
            btnDeleteStrip = new ToolStripButton();
            btnDeleteAllStrip = new ToolStripButton();
            btnLoadImgStrip = new ToolStripButton();
            toolStripSeparator3 = new ToolStripSeparator();
            btnNoticeStrip = new ToolStripButton();
            toolStripSeparator4 = new ToolStripSeparator();
            txtBrighttoolStrip = new ToolStripTextBox();
            txtContrtoolStrip = new ToolStripTextBox();
            toolStripSeparator10 = new ToolStripSeparator();
            boxStripLbl = new ToolStripLabel();
            boxNoStripTxt = new ToolStripTextBox();
            boxLabelStripLbl = new ToolStripLabel();
            boxLabelStripTxt = new ToolStripTextBox();
            batchLabelStripLbl = new ToolStripLabel();
            batchStripTxt = new ToolStripTextBox();
            timer1 = new System.Windows.Forms.Timer(components);
            timerProgress = new System.Windows.Forms.Timer(components);
            dsvThumbnailList = new Dynamsoft.Forms.DSViewer();
            cmnuTvwStrip = new ContextMenuStrip(components);
            mnuScanRepStrip = new ToolStripMenuItem();
            mnuImpRepStrip = new ToolStripMenuItem();
            toolStripSeparator9 = new ToolStripSeparator();
            mnuInsStrip = new ToolStripMenuItem();
            mnuImpStrip = new ToolStripMenuItem();
            toolStripSeparator8 = new ToolStripSeparator();
            mnuDeleteSetStrip = new ToolStripMenuItem();
            mnuDeleteImgStrip = new ToolStripMenuItem();
            toolStripSeparator6 = new ToolStripSeparator();
            mnuCopyStrip = new ToolStripMenuItem();
            mnuCutStrip = new ToolStripMenuItem();
            toolStripSeparator7 = new ToolStripSeparator();
            mnuReplaceStrip = new ToolStripMenuItem();
            mnuPasteStrip = new ToolStripMenuItem();
            toolStripSeparator13 = new ToolStripSeparator();
            mnuInsSepStrip = new ToolStripMenuItem();
            mnuImpSepStrip = new ToolStripMenuItem();
            ofdImportFile = new OpenFileDialog();
            ofdLoadImg = new OpenFileDialog();
            panel1 = new Panel();
            txtTotImgNum = new TextBox();
            txtCurrImgIdx = new TextBox();
            lbDiv = new Label();
            picboxPrevious = new PictureBox();
            picboxNext = new PictureBox();
            picboxLast = new PictureBox();
            picboxFirst = new PictureBox();
            ScanStatusStrip.SuspendLayout();
            ScanToolStrip.SuspendLayout();
            cmnuTvwStrip.SuspendLayout();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picboxPrevious).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picboxNext).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picboxLast).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picboxFirst).BeginInit();
            SuspendLayout();
            // 
            // tvwBatch
            // 
            tvwBatch.Location = new Point(7, 62);
            tvwBatch.Margin = new Padding(3, 2, 3, 2);
            tvwBatch.Name = "tvwBatch";
            tvwBatch.Size = new Size(293, 826);
            tvwBatch.TabIndex = 10;
            tvwBatch.AfterSelect += tvwBatch_AfterSelect;
            tvwBatch.NodeMouseClick += tvwBatch_NodeMouseClick;
            tvwBatch.MouseUp += tvwBatch_MouseUp;
            // 
            // dsvImg
            // 
            dsvImg.Location = new Point(306, 62);
            dsvImg.Margin = new Padding(3, 2, 3, 2);
            dsvImg.Name = "dsvImg";
            dsvImg.RightToLeft = RightToLeft.No;
            dsvImg.SelectionRectAspectRatio = 0D;
            dsvImg.Size = new Size(962, 787);
            dsvImg.TabIndex = 15;
            // 
            // txtInfo
            // 
            txtInfo.BackColor = SystemColors.ControlLight;
            txtInfo.Location = new Point(306, 860);
            txtInfo.Margin = new Padding(3, 2, 3, 2);
            txtInfo.Name = "txtInfo";
            txtInfo.ReadOnly = true;
            txtInfo.Size = new Size(616, 27);
            txtInfo.TabIndex = 20;
            // 
            // ScanStatusStrip
            // 
            ScanStatusStrip.ImageScalingSize = new Size(24, 24);
            ScanStatusStrip.Items.AddRange(new ToolStripItem[] { ScanStatusBar, ScanProgressBar, ScanStatusBar1, CurrDateTime });
            ScanStatusStrip.Location = new Point(0, 894);
            ScanStatusStrip.Name = "ScanStatusStrip";
            ScanStatusStrip.Padding = new Padding(1, 0, 12, 0);
            ScanStatusStrip.Size = new Size(1464, 35);
            ScanStatusStrip.TabIndex = 80;
            // 
            // ScanStatusBar
            // 
            ScanStatusBar.AutoSize = false;
            ScanStatusBar.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            ScanStatusBar.BorderStyle = Border3DStyle.Sunken;
            ScanStatusBar.ImageScaling = ToolStripItemImageScaling.None;
            ScanStatusBar.Name = "ScanStatusBar";
            ScanStatusBar.Size = new Size(206, 36);
            // 
            // ScanProgressBar
            // 
            ScanProgressBar.AutoSize = false;
            ScanProgressBar.Name = "ScanProgressBar";
            ScanProgressBar.Size = new Size(708, 34);
            // 
            // ScanStatusBar1
            // 
            ScanStatusBar1.AutoSize = false;
            ScanStatusBar1.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            ScanStatusBar1.BorderStyle = Border3DStyle.Sunken;
            ScanStatusBar1.Name = "ScanStatusBar1";
            ScanStatusBar1.Size = new Size(350, 36);
            ScanStatusBar1.Text = "   ";
            // 
            // CurrDateTime
            // 
            CurrDateTime.AutoSize = false;
            CurrDateTime.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            CurrDateTime.BorderStyle = Border3DStyle.Sunken;
            CurrDateTime.Name = "CurrDateTime";
            CurrDateTime.Size = new Size(180, 36);
            CurrDateTime.Text = "Curr. Date Time";
            CurrDateTime.TextAlign = ContentAlignment.MiddleRight;
            // 
            // ScanToolStrip
            // 
            ScanToolStrip.AutoSize = false;
            ScanToolStrip.ImageScalingSize = new Size(24, 24);
            ScanToolStrip.Items.AddRange(new ToolStripItem[] { btnCloseStrip, toolStripSeparator1, btnRotateRightStrip, btnRotateLeftStrip, btnMirrorStrip, btnFlipStrip, btnDeskewStrip, toolStripSeparator11, btnActualStrip, btnFitStrip, btnZoomOutStrip, btnZoomInStrip, btnExpaAllStrip, btnCollAllStrip, toolStripSeparator2, btnHandStrip, btnPointStrip, btnCutAreaStrip, btnCropAreaStrip, toolStripSeparator5, btnDeleteStrip, btnDeleteAllStrip, btnLoadImgStrip, toolStripSeparator3, btnNoticeStrip, toolStripSeparator4, txtBrighttoolStrip, txtContrtoolStrip, toolStripSeparator10, boxStripLbl, boxNoStripTxt, boxLabelStripLbl, boxLabelStripTxt, batchLabelStripLbl, batchStripTxt });
            ScanToolStrip.Location = new Point(0, 0);
            ScanToolStrip.Name = "ScanToolStrip";
            ScanToolStrip.Size = new Size(1464, 60);
            ScanToolStrip.TabIndex = 1;
            ScanToolStrip.Text = "toolStrip1";
            // 
            // btnCloseStrip
            // 
            btnCloseStrip.Image = (Image)resources.GetObject("btnCloseStrip.Image");
            btnCloseStrip.ImageTransparentColor = Color.Magenta;
            btnCloseStrip.Name = "btnCloseStrip";
            btnCloseStrip.Size = new Size(97, 57);
            btnCloseStrip.Text = "Close Screen";
            btnCloseStrip.TextAlign = ContentAlignment.BottomCenter;
            btnCloseStrip.TextImageRelation = TextImageRelation.ImageAboveText;
            btnCloseStrip.Click += btnCloseStrip_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 60);
            // 
            // btnRotateRightStrip
            // 
            btnRotateRightStrip.Image = (Image)resources.GetObject("btnRotateRightStrip.Image");
            btnRotateRightStrip.ImageTransparentColor = Color.Magenta;
            btnRotateRightStrip.Name = "btnRotateRightStrip";
            btnRotateRightStrip.Size = new Size(96, 57);
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
            btnRotateLeftStrip.Size = new Size(86, 57);
            btnRotateLeftStrip.Text = "Rotate Left";
            btnRotateLeftStrip.TextAlign = ContentAlignment.BottomCenter;
            btnRotateLeftStrip.TextImageRelation = TextImageRelation.ImageAboveText;
            btnRotateLeftStrip.Click += btnRotateLeftStrip_Click;
            // 
            // btnMirrorStrip
            // 
            btnMirrorStrip.Image = (Image)resources.GetObject("btnMirrorStrip.Image");
            btnMirrorStrip.ImageTransparentColor = Color.Magenta;
            btnMirrorStrip.Name = "btnMirrorStrip";
            btnMirrorStrip.Size = new Size(54, 57);
            btnMirrorStrip.Text = "Mirror";
            btnMirrorStrip.TextAlign = ContentAlignment.BottomCenter;
            btnMirrorStrip.TextImageRelation = TextImageRelation.ImageAboveText;
            btnMirrorStrip.Click += btnMirrorStrip_Click;
            // 
            // btnFlipStrip
            // 
            btnFlipStrip.Image = (Image)resources.GetObject("btnFlipStrip.Image");
            btnFlipStrip.ImageTransparentColor = Color.Magenta;
            btnFlipStrip.Name = "btnFlipStrip";
            btnFlipStrip.Size = new Size(37, 57);
            btnFlipStrip.Text = "Flip";
            btnFlipStrip.TextAlign = ContentAlignment.BottomCenter;
            btnFlipStrip.TextImageRelation = TextImageRelation.ImageAboveText;
            btnFlipStrip.Click += btnFlipStrip_Click;
            // 
            // btnDeskewStrip
            // 
            btnDeskewStrip.Image = (Image)resources.GetObject("btnDeskewStrip.Image");
            btnDeskewStrip.ImageTransparentColor = Color.Magenta;
            btnDeskewStrip.Name = "btnDeskewStrip";
            btnDeskewStrip.Size = new Size(81, 57);
            btnDeskewStrip.Text = "Straighten";
            btnDeskewStrip.TextAlign = ContentAlignment.BottomCenter;
            btnDeskewStrip.TextImageRelation = TextImageRelation.ImageAboveText;
            btnDeskewStrip.Click += btnDeskewStrip_Click;
            // 
            // toolStripSeparator11
            // 
            toolStripSeparator11.Name = "toolStripSeparator11";
            toolStripSeparator11.Size = new Size(6, 60);
            // 
            // btnActualStrip
            // 
            btnActualStrip.Image = (Image)resources.GetObject("btnActualStrip.Image");
            btnActualStrip.ImageTransparentColor = Color.Magenta;
            btnActualStrip.Name = "btnActualStrip";
            btnActualStrip.Size = new Size(86, 57);
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
            btnFitStrip.Size = new Size(77, 57);
            btnFitStrip.Text = "Fit Screen";
            btnFitStrip.TextAlign = ContentAlignment.BottomCenter;
            btnFitStrip.TextImageRelation = TextImageRelation.ImageAboveText;
            btnFitStrip.Click += btnFitStrip_Click;
            // 
            // btnZoomOutStrip
            // 
            btnZoomOutStrip.Image = (Image)resources.GetObject("btnZoomOutStrip.Image");
            btnZoomOutStrip.ImageTransparentColor = Color.Magenta;
            btnZoomOutStrip.Name = "btnZoomOutStrip";
            btnZoomOutStrip.Size = new Size(81, 57);
            btnZoomOutStrip.Text = "Zoom Out";
            btnZoomOutStrip.TextAlign = ContentAlignment.BottomCenter;
            btnZoomOutStrip.TextImageRelation = TextImageRelation.ImageAboveText;
            btnZoomOutStrip.Click += btnZoomOutStrip_Click;
            // 
            // btnZoomInStrip
            // 
            btnZoomInStrip.Image = (Image)resources.GetObject("btnZoomInStrip.Image");
            btnZoomInStrip.ImageTransparentColor = Color.Magenta;
            btnZoomInStrip.Name = "btnZoomInStrip";
            btnZoomInStrip.Size = new Size(69, 57);
            btnZoomInStrip.Text = "Zoom In";
            btnZoomInStrip.TextAlign = ContentAlignment.BottomCenter;
            btnZoomInStrip.TextImageRelation = TextImageRelation.ImageAboveText;
            btnZoomInStrip.Click += btnZoomInStrip_Click;
            // 
            // btnExpaAllStrip
            // 
            btnExpaAllStrip.Image = (Image)resources.GetObject("btnExpaAllStrip.Image");
            btnExpaAllStrip.ImageTransparentColor = Color.Magenta;
            btnExpaAllStrip.Name = "btnExpaAllStrip";
            btnExpaAllStrip.Size = new Size(98, 57);
            btnExpaAllStrip.Text = "Expand View";
            btnExpaAllStrip.TextAlign = ContentAlignment.BottomCenter;
            btnExpaAllStrip.TextImageRelation = TextImageRelation.ImageAboveText;
            btnExpaAllStrip.Click += btnExpaAllStrip_Click;
            // 
            // btnCollAllStrip
            // 
            btnCollAllStrip.Image = (Image)resources.GetObject("btnCollAllStrip.Image");
            btnCollAllStrip.ImageTransparentColor = Color.Magenta;
            btnCollAllStrip.Name = "btnCollAllStrip";
            btnCollAllStrip.Size = new Size(106, 57);
            btnCollAllStrip.Text = "Collapse View";
            btnCollAllStrip.TextAlign = ContentAlignment.BottomCenter;
            btnCollAllStrip.TextImageRelation = TextImageRelation.ImageAboveText;
            btnCollAllStrip.Click += btnCollAllStrip_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(6, 60);
            toolStripSeparator2.Visible = false;
            // 
            // btnHandStrip
            // 
            btnHandStrip.Image = (Image)resources.GetObject("btnHandStrip.Image");
            btnHandStrip.ImageTransparentColor = Color.Magenta;
            btnHandStrip.Name = "btnHandStrip";
            btnHandStrip.Size = new Size(50, 57);
            btnHandStrip.Text = "Move";
            btnHandStrip.TextAlign = ContentAlignment.BottomCenter;
            btnHandStrip.TextImageRelation = TextImageRelation.ImageAboveText;
            btnHandStrip.Visible = false;
            btnHandStrip.Click += btnHandStrip_Click;
            // 
            // btnPointStrip
            // 
            btnPointStrip.Image = (Image)resources.GetObject("btnPointStrip.Image");
            btnPointStrip.ImageTransparentColor = Color.Magenta;
            btnPointStrip.Name = "btnPointStrip";
            btnPointStrip.Size = new Size(53, 57);
            btnPointStrip.Text = "Select";
            btnPointStrip.TextAlign = ContentAlignment.BottomCenter;
            btnPointStrip.TextImageRelation = TextImageRelation.ImageAboveText;
            btnPointStrip.Visible = false;
            btnPointStrip.Click += btnPointStrip_Click;
            // 
            // btnCutAreaStrip
            // 
            btnCutAreaStrip.Image = (Image)resources.GetObject("btnCutAreaStrip.Image");
            btnCutAreaStrip.ImageTransparentColor = Color.Magenta;
            btnCutAreaStrip.Name = "btnCutAreaStrip";
            btnCutAreaStrip.Size = new Size(88, 57);
            btnCutAreaStrip.Text = "Cut an area";
            btnCutAreaStrip.TextAlign = ContentAlignment.BottomCenter;
            btnCutAreaStrip.TextImageRelation = TextImageRelation.ImageAboveText;
            btnCutAreaStrip.Visible = false;
            btnCutAreaStrip.Click += btnCutAreaStrip_Click;
            // 
            // btnCropAreaStrip
            // 
            btnCropAreaStrip.Image = (Image)resources.GetObject("btnCropAreaStrip.Image");
            btnCropAreaStrip.ImageTransparentColor = Color.Magenta;
            btnCropAreaStrip.Name = "btnCropAreaStrip";
            btnCropAreaStrip.Size = new Size(98, 57);
            btnCropAreaStrip.Text = "Crop an area";
            btnCropAreaStrip.TextAlign = ContentAlignment.BottomCenter;
            btnCropAreaStrip.TextImageRelation = TextImageRelation.ImageAboveText;
            btnCropAreaStrip.Visible = false;
            btnCropAreaStrip.Click += btnCropAreaStrip_Click;
            // 
            // toolStripSeparator5
            // 
            toolStripSeparator5.Name = "toolStripSeparator5";
            toolStripSeparator5.Size = new Size(6, 60);
            // 
            // btnDeleteStrip
            // 
            btnDeleteStrip.Image = (Image)resources.GetObject("btnDeleteStrip.Image");
            btnDeleteStrip.ImageTransparentColor = Color.Magenta;
            btnDeleteStrip.Name = "btnDeleteStrip";
            btnDeleteStrip.Size = new Size(103, 57);
            btnDeleteStrip.Text = "Delete Image";
            btnDeleteStrip.TextAlign = ContentAlignment.BottomCenter;
            btnDeleteStrip.TextImageRelation = TextImageRelation.ImageAboveText;
            btnDeleteStrip.ToolTipText = "Delete an image";
            btnDeleteStrip.Click += btnDeleteStrip_Click;
            // 
            // btnDeleteAllStrip
            // 
            btnDeleteAllStrip.Image = (Image)resources.GetObject("btnDeleteAllStrip.Image");
            btnDeleteAllStrip.ImageTransparentColor = Color.Magenta;
            btnDeleteAllStrip.Name = "btnDeleteAllStrip";
            btnDeleteAllStrip.Size = new Size(79, 57);
            btnDeleteAllStrip.Text = "Delete All";
            btnDeleteAllStrip.TextAlign = ContentAlignment.BottomCenter;
            btnDeleteAllStrip.TextImageRelation = TextImageRelation.ImageAboveText;
            btnDeleteAllStrip.Click += btnDeleteAllStrip_Click;
            // 
            // btnLoadImgStrip
            // 
            btnLoadImgStrip.Image = (Image)resources.GetObject("btnLoadImgStrip.Image");
            btnLoadImgStrip.ImageTransparentColor = Color.Magenta;
            btnLoadImgStrip.Name = "btnLoadImgStrip";
            btnLoadImgStrip.Size = new Size(98, 57);
            btnLoadImgStrip.Text = "Load Images";
            btnLoadImgStrip.TextAlign = ContentAlignment.BottomCenter;
            btnLoadImgStrip.TextImageRelation = TextImageRelation.ImageAboveText;
            btnLoadImgStrip.Click += btnLoadImgStrip_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(6, 60);
            // 
            // btnNoticeStrip
            // 
            btnNoticeStrip.Image = (Image)resources.GetObject("btnNoticeStrip.Image");
            btnNoticeStrip.ImageTransparentColor = Color.Magenta;
            btnNoticeStrip.Name = "btnNoticeStrip";
            btnNoticeStrip.Size = new Size(92, 57);
            btnNoticeStrip.Text = "Notification";
            btnNoticeStrip.TextAlign = ContentAlignment.BottomCenter;
            btnNoticeStrip.TextImageRelation = TextImageRelation.ImageAboveText;
            btnNoticeStrip.Click += btnNoticeStrip_Click;
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new Size(6, 60);
            // 
            // txtBrighttoolStrip
            // 
            txtBrighttoolStrip.MaxLength = 3;
            txtBrighttoolStrip.Name = "txtBrighttoolStrip";
            txtBrighttoolStrip.Size = new Size(50, 60);
            txtBrighttoolStrip.Text = "1";
            txtBrighttoolStrip.ToolTipText = "Brightness (1-100)";
            txtBrighttoolStrip.KeyDown += txtBrighttoolStrip_KeyDown;
            // 
            // txtContrtoolStrip
            // 
            txtContrtoolStrip.Name = "txtContrtoolStrip";
            txtContrtoolStrip.Size = new Size(50, 27);
            txtContrtoolStrip.Text = "1";
            txtContrtoolStrip.ToolTipText = "Contrast (1-255)";
            txtContrtoolStrip.KeyDown += txtContrtoolStrip_KeyDown;
            // 
            // toolStripSeparator10
            // 
            toolStripSeparator10.Name = "toolStripSeparator10";
            toolStripSeparator10.Size = new Size(6, 60);
            // 
            // boxStripLbl
            // 
            boxStripLbl.Name = "boxStripLbl";
            boxStripLbl.Size = new Size(43, 20);
            boxStripLbl.Text = "Box#";
            // 
            // boxNoStripTxt
            // 
            boxNoStripTxt.BackColor = SystemColors.Info;
            boxNoStripTxt.Font = new Font("Arial", 8F);
            boxNoStripTxt.MaxLength = 8;
            boxNoStripTxt.Name = "boxNoStripTxt";
            boxNoStripTxt.Size = new Size(78, 23);
            // 
            // boxLabelStripLbl
            // 
            boxLabelStripLbl.Name = "boxLabelStripLbl";
            boxLabelStripLbl.Size = new Size(74, 20);
            boxLabelStripLbl.Text = "Box Label";
            // 
            // boxLabelStripTxt
            // 
            boxLabelStripTxt.Font = new Font("Arial", 8F);
            boxLabelStripTxt.MaxLength = 30;
            boxLabelStripTxt.Name = "boxLabelStripTxt";
            boxLabelStripTxt.Size = new Size(210, 23);
            // 
            // batchLabelStripLbl
            // 
            batchLabelStripLbl.Name = "batchLabelStripLbl";
            batchLabelStripLbl.Size = new Size(34, 20);
            batchLabelStripLbl.Text = "Ref.";
            // 
            // batchStripTxt
            // 
            batchStripTxt.BackColor = SystemColors.Info;
            batchStripTxt.Font = new Font("Arial", 8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            batchStripTxt.MaxLength = 30;
            batchStripTxt.Name = "batchStripTxt";
            batchStripTxt.Size = new Size(200, 23);
            // 
            // timer1
            // 
            timer1.Tick += timer1_Tick;
            // 
            // timerProgress
            // 
            timerProgress.Interval = 600;
            timerProgress.Tick += timerProgress_Tick;
            // 
            // dsvThumbnailList
            // 
            dsvThumbnailList.AutoScroll = true;
            dsvThumbnailList.Location = new Point(1275, 62);
            dsvThumbnailList.Margin = new Padding(3, 2, 3, 2);
            dsvThumbnailList.Name = "dsvThumbnailList";
            dsvThumbnailList.RightToLeft = RightToLeft.No;
            dsvThumbnailList.SelectionRectAspectRatio = 0D;
            dsvThumbnailList.Size = new Size(180, 826);
            dsvThumbnailList.TabIndex = 70;
            dsvThumbnailList.OnMouseClick += dsvThumbnailList_OnMouseClick;
            // 
            // cmnuTvwStrip
            // 
            cmnuTvwStrip.ImageScalingSize = new Size(20, 20);
            cmnuTvwStrip.Items.AddRange(new ToolStripItem[] { mnuScanRepStrip, mnuImpRepStrip, toolStripSeparator9, mnuInsStrip, mnuImpStrip, toolStripSeparator8, mnuDeleteSetStrip, mnuDeleteImgStrip, toolStripSeparator6, mnuCopyStrip, mnuCutStrip, toolStripSeparator7, mnuReplaceStrip, mnuPasteStrip, toolStripSeparator13, mnuInsSepStrip, mnuImpSepStrip });
            cmnuTvwStrip.Name = "cmnuTvwStrip";
            cmnuTvwStrip.Size = new Size(197, 322);
            // 
            // mnuScanRepStrip
            // 
            mnuScanRepStrip.Name = "mnuScanRepStrip";
            mnuScanRepStrip.Size = new Size(196, 24);
            mnuScanRepStrip.Text = "Scan && Replace";
            mnuScanRepStrip.Click += mnuScanRepStrip_Click;
            // 
            // mnuImpRepStrip
            // 
            mnuImpRepStrip.Name = "mnuImpRepStrip";
            mnuImpRepStrip.Size = new Size(196, 24);
            mnuImpRepStrip.Text = "Import && Replace";
            mnuImpRepStrip.Click += mnuImpRepStrip_Click;
            // 
            // toolStripSeparator9
            // 
            toolStripSeparator9.Name = "toolStripSeparator9";
            toolStripSeparator9.Size = new Size(193, 6);
            // 
            // mnuInsStrip
            // 
            mnuInsStrip.Name = "mnuInsStrip";
            mnuInsStrip.Size = new Size(196, 24);
            mnuInsStrip.Text = "Insert Image";
            mnuInsStrip.Click += mnuInsStrip_Click;
            // 
            // mnuImpStrip
            // 
            mnuImpStrip.Name = "mnuImpStrip";
            mnuImpStrip.Size = new Size(196, 24);
            mnuImpStrip.Text = "Import Image";
            mnuImpStrip.Click += mnuImpStrip_Click;
            // 
            // toolStripSeparator8
            // 
            toolStripSeparator8.Name = "toolStripSeparator8";
            toolStripSeparator8.Size = new Size(193, 6);
            // 
            // mnuDeleteSetStrip
            // 
            mnuDeleteSetStrip.Name = "mnuDeleteSetStrip";
            mnuDeleteSetStrip.Size = new Size(196, 24);
            mnuDeleteSetStrip.Text = "Delete Set";
            mnuDeleteSetStrip.Click += mnuDeleteSetStrip_Click;
            // 
            // mnuDeleteImgStrip
            // 
            mnuDeleteImgStrip.Name = "mnuDeleteImgStrip";
            mnuDeleteImgStrip.Size = new Size(196, 24);
            mnuDeleteImgStrip.Text = "Delete Image";
            mnuDeleteImgStrip.Click += mnuDeleteImgStrip_Click;
            // 
            // toolStripSeparator6
            // 
            toolStripSeparator6.Name = "toolStripSeparator6";
            toolStripSeparator6.Size = new Size(193, 6);
            // 
            // mnuCopyStrip
            // 
            mnuCopyStrip.Name = "mnuCopyStrip";
            mnuCopyStrip.Size = new Size(196, 24);
            mnuCopyStrip.Text = "C&opy Image";
            mnuCopyStrip.Click += mnuCopyStrip_Click;
            // 
            // mnuCutStrip
            // 
            mnuCutStrip.Name = "mnuCutStrip";
            mnuCutStrip.Size = new Size(196, 24);
            mnuCutStrip.Text = "&Cut Image";
            mnuCutStrip.Click += mnuCutStrip_Click;
            // 
            // toolStripSeparator7
            // 
            toolStripSeparator7.Name = "toolStripSeparator7";
            toolStripSeparator7.Size = new Size(193, 6);
            // 
            // mnuReplaceStrip
            // 
            mnuReplaceStrip.Name = "mnuReplaceStrip";
            mnuReplaceStrip.Size = new Size(196, 24);
            mnuReplaceStrip.Text = "Replace Image";
            mnuReplaceStrip.Click += mnuReplaceStrip_Click;
            // 
            // mnuPasteStrip
            // 
            mnuPasteStrip.Name = "mnuPasteStrip";
            mnuPasteStrip.Size = new Size(196, 24);
            mnuPasteStrip.Text = "&Paste Image";
            mnuPasteStrip.Click += mnuPasteStrip_Click;
            // 
            // toolStripSeparator13
            // 
            toolStripSeparator13.Name = "toolStripSeparator13";
            toolStripSeparator13.Size = new Size(193, 6);
            // 
            // mnuInsSepStrip
            // 
            mnuInsSepStrip.Name = "mnuInsSepStrip";
            mnuInsSepStrip.Size = new Size(196, 24);
            mnuInsSepStrip.Text = "Insert Separator";
            mnuInsSepStrip.Visible = false;
            mnuInsSepStrip.Click += mnuInsSepStrip_Click;
            // 
            // mnuImpSepStrip
            // 
            mnuImpSepStrip.Name = "mnuImpSepStrip";
            mnuImpSepStrip.Size = new Size(196, 24);
            mnuImpSepStrip.Text = "Import Separator";
            mnuImpSepStrip.Visible = false;
            mnuImpSepStrip.Click += mnuImpSepStrip_Click;
            // 
            // ofdImportFile
            // 
            ofdImportFile.Title = "Import Image";
            // 
            // ofdLoadImg
            // 
            ofdLoadImg.Multiselect = true;
            ofdLoadImg.Title = "Load Images";
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
            panel1.Location = new Point(929, 855);
            panel1.Margin = new Padding(3, 4, 3, 4);
            panel1.Name = "panel1";
            panel1.Size = new Size(338, 33);
            panel1.TabIndex = 21;
            // 
            // txtTotImgNum
            // 
            txtTotImgNum.Enabled = false;
            txtTotImgNum.Location = new Point(180, 5);
            txtTotImgNum.Margin = new Padding(3, 2, 3, 2);
            txtTotImgNum.Name = "txtTotImgNum";
            txtTotImgNum.ReadOnly = true;
            txtTotImgNum.Size = new Size(55, 27);
            txtTotImgNum.TabIndex = 73;
            txtTotImgNum.Text = "0";
            // 
            // txtCurrImgIdx
            // 
            txtCurrImgIdx.Enabled = false;
            txtCurrImgIdx.Location = new Point(102, 5);
            txtCurrImgIdx.Margin = new Padding(3, 2, 3, 2);
            txtCurrImgIdx.Name = "txtCurrImgIdx";
            txtCurrImgIdx.ReadOnly = true;
            txtCurrImgIdx.Size = new Size(55, 27);
            txtCurrImgIdx.TabIndex = 71;
            txtCurrImgIdx.Text = "0";
            txtCurrImgIdx.TextAlign = HorizontalAlignment.Right;
            // 
            // lbDiv
            // 
            lbDiv.AutoSize = true;
            lbDiv.BackColor = Color.Transparent;
            lbDiv.Location = new Point(164, 7);
            lbDiv.Name = "lbDiv";
            lbDiv.Size = new Size(15, 20);
            lbDiv.TabIndex = 72;
            lbDiv.Text = "/";
            // 
            // picboxPrevious
            // 
            picboxPrevious.BorderStyle = BorderStyle.FixedSingle;
            picboxPrevious.Image = Properties.Resources.picboxPrevious_Enter;
            picboxPrevious.Location = new Point(54, 5);
            picboxPrevious.Margin = new Padding(3, 2, 3, 2);
            picboxPrevious.Name = "picboxPrevious";
            picboxPrevious.Size = new Size(46, 24);
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
            picboxNext.Location = new Point(238, 5);
            picboxNext.Margin = new Padding(3, 2, 3, 2);
            picboxNext.Name = "picboxNext";
            picboxNext.Size = new Size(46, 24);
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
            picboxLast.Location = new Point(286, 5);
            picboxLast.Margin = new Padding(3, 2, 3, 2);
            picboxLast.Name = "picboxLast";
            picboxLast.Size = new Size(46, 24);
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
            picboxFirst.Location = new Point(4, 5);
            picboxFirst.Margin = new Padding(3, 2, 3, 2);
            picboxFirst.Name = "picboxFirst";
            picboxFirst.Size = new Size(46, 24);
            picboxFirst.SizeMode = PictureBoxSizeMode.CenterImage;
            picboxFirst.TabIndex = 69;
            picboxFirst.TabStop = false;
            picboxFirst.Tag = "First Image";
            picboxFirst.Click += picboxFirst_Click;
            // 
            // frmScanBox1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoScroll = true;
            ClientSize = new Size(1464, 929);
            Controls.Add(panel1);
            Controls.Add(ScanStatusStrip);
            Controls.Add(dsvThumbnailList);
            Controls.Add(ScanToolStrip);
            Controls.Add(txtInfo);
            Controls.Add(dsvImg);
            Controls.Add(tvwBatch);
            FormBorderStyle = FormBorderStyle.Fixed3D;
            Margin = new Padding(3, 2, 3, 2);
            MinimizeBox = false;
            Name = "frmScanBox1";
            ShowIcon = false;
            Text = "Document Scan";
            WindowState = FormWindowState.Maximized;
            Activated += frmScanBox1_Activated;
            FormClosing += frmScanBox_FormClosing;
            FormClosed += frmScanBox_FormClosed;
            Load += frmScanBox_Load;
            Paint += frmScanBox1_Paint;
            Resize += frmScanBox_Resize;
            ScanStatusStrip.ResumeLayout(false);
            ScanStatusStrip.PerformLayout();
            ScanToolStrip.ResumeLayout(false);
            ScanToolStrip.PerformLayout();
            cmnuTvwStrip.ResumeLayout(false);
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
        private System.Windows.Forms.TreeView tvwBatch;
        private Dynamsoft.Forms.DSViewer dsvImg;
        private System.Windows.Forms.TextBox txtInfo;
        private System.Windows.Forms.StatusStrip ScanStatusStrip;
        private System.Windows.Forms.ToolStripStatusLabel ScanStatusBar;
        private System.Windows.Forms.ToolStripProgressBar ScanProgressBar;
        private System.Windows.Forms.ToolStrip ScanToolStrip;
        private System.Windows.Forms.ToolStripStatusLabel ScanStatusBar1;
        private System.Windows.Forms.ToolStripStatusLabel CurrDateTime;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Timer timerProgress;
        private Dynamsoft.Forms.DSViewer dsvThumbnailList;
        private System.Windows.Forms.ToolStripButton btnCloseStrip;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton btnActualStrip;
        private System.Windows.Forms.ToolStripButton btnFitStrip;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton btnZoomOutStrip;
        private System.Windows.Forms.ToolStripButton btnZoomInStrip;
        private System.Windows.Forms.ToolStripButton btnRotateRightStrip;
        private System.Windows.Forms.ToolStripButton btnRotateLeftStrip;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripButton btnDeleteStrip;
        private System.Windows.Forms.ToolStripButton btnDeleteAllStrip;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ContextMenuStrip cmnuTvwStrip;
        private System.Windows.Forms.ToolStripMenuItem mnuImpStrip;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private System.Windows.Forms.ToolStripMenuItem mnuDeleteSetStrip;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripMenuItem mnuCopyStrip;
        private System.Windows.Forms.ToolStripMenuItem mnuCutStrip;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripMenuItem mnuReplaceStrip;
        private System.Windows.Forms.ToolStripMenuItem mnuPasteStrip;
        private System.Windows.Forms.OpenFileDialog ofdImportFile;
        private System.Windows.Forms.ToolStripButton btnLoadImgStrip;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.OpenFileDialog ofdLoadImg;
        private System.Windows.Forms.ToolStripMenuItem mnuDeleteImgStrip;
        private System.Windows.Forms.ToolStripMenuItem mnuImpRepStrip;
        private System.Windows.Forms.ToolStripMenuItem mnuScanRepStrip;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator9;
        private System.Windows.Forms.ToolStripButton btnExpaAllStrip;
        private System.Windows.Forms.ToolStripButton btnCollAllStrip;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator10;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator11;
        private System.Windows.Forms.ToolStripButton btnCutAreaStrip;
        private System.Windows.Forms.ToolStripButton btnCropAreaStrip;
        private System.Windows.Forms.ToolStripButton btnHandStrip;
        private System.Windows.Forms.ToolStripButton btnPointStrip;
        private System.Windows.Forms.ToolStripButton btnMirrorStrip;
        private System.Windows.Forms.ToolStripButton btnFlipStrip;
        private System.Windows.Forms.ToolStripButton btnDeskewStrip;
        private System.Windows.Forms.ToolStripButton btnNoticeStrip;
        private System.Windows.Forms.ToolStripMenuItem mnuInsStrip;
        private System.Windows.Forms.ToolStripMenuItem mnuInsSepStrip;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator13;
        private System.Windows.Forms.ToolStripMenuItem mnuImpSepStrip;
        private System.Windows.Forms.ToolStripTextBox txtBrighttoolStrip;
        private System.Windows.Forms.ToolStripTextBox txtContrtoolStrip;
        private System.Windows.Forms.ToolStripLabel boxStripLbl;
        private System.Windows.Forms.ToolStripTextBox boxNoStripTxt;
        private System.Windows.Forms.ToolStripLabel boxLabelStripLbl;
        private System.Windows.Forms.ToolStripTextBox boxLabelStripTxt;
        private System.Windows.Forms.ToolStripLabel batchLabelStripLbl;
        private System.Windows.Forms.ToolStripTextBox batchStripTxt;
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

