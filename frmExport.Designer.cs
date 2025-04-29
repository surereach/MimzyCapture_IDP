
namespace SRDocScanIDP
{
    partial class frmExport
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
            label1 = new Label();
            cbxStatus = new ComboBox();
            grbFileTypes = new GroupBox();
            rdbTIFF = new RadioButton();
            rdbPDF = new RadioButton();
            btnExport = new Button();
            btnCancel = new Button();
            btnOpenFolder = new Button();
            groupBox1 = new GroupBox();
            rdbXML = new RadioButton();
            rdbCSV = new RadioButton();
            grbFileTypes.SuspendLayout();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.Location = new Point(18, 20);
            label1.Name = "label1";
            label1.Size = new Size(193, 25);
            label1.TabIndex = 0;
            label1.Text = "Document Sets";
            label1.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // cbxStatus
            // 
            cbxStatus.FormattingEnabled = true;
            cbxStatus.Location = new Point(218, 20);
            cbxStatus.Name = "cbxStatus";
            cbxStatus.Size = new Size(208, 23);
            cbxStatus.TabIndex = 1;
            // 
            // grbFileTypes
            // 
            grbFileTypes.Controls.Add(rdbTIFF);
            grbFileTypes.Controls.Add(rdbPDF);
            grbFileTypes.Location = new Point(21, 61);
            grbFileTypes.Name = "grbFileTypes";
            grbFileTypes.Size = new Size(405, 86);
            grbFileTypes.TabIndex = 10;
            grbFileTypes.TabStop = false;
            grbFileTypes.Text = "Export File Type";
            // 
            // rdbTIFF
            // 
            rdbTIFF.Location = new Point(150, 25);
            rdbTIFF.Name = "rdbTIFF";
            rdbTIFF.Size = new Size(100, 35);
            rdbTIFF.TabIndex = 1;
            rdbTIFF.Text = "tiff";
            rdbTIFF.UseVisualStyleBackColor = true;
            // 
            // rdbPDF
            // 
            rdbPDF.Checked = true;
            rdbPDF.Location = new Point(35, 25);
            rdbPDF.Name = "rdbPDF";
            rdbPDF.Size = new Size(100, 35);
            rdbPDF.TabIndex = 0;
            rdbPDF.TabStop = true;
            rdbPDF.Text = "pdf";
            rdbPDF.UseVisualStyleBackColor = true;
            // 
            // btnExport
            // 
            btnExport.FlatStyle = FlatStyle.Popup;
            btnExport.Location = new Point(206, 243);
            btnExport.Name = "btnExport";
            btnExport.Size = new Size(103, 35);
            btnExport.TabIndex = 20;
            btnExport.Text = "&Export";
            btnExport.UseVisualStyleBackColor = true;
            btnExport.Click += btnExport_Click;
            // 
            // btnCancel
            // 
            btnCancel.FlatStyle = FlatStyle.Popup;
            btnCancel.Location = new Point(323, 243);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(103, 35);
            btnCancel.TabIndex = 21;
            btnCancel.Text = "&Close";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // btnOpenFolder
            // 
            btnOpenFolder.FlatStyle = FlatStyle.Popup;
            btnOpenFolder.Location = new Point(21, 243);
            btnOpenFolder.Name = "btnOpenFolder";
            btnOpenFolder.Size = new Size(153, 35);
            btnOpenFolder.TabIndex = 22;
            btnOpenFolder.Text = "&Open Folder";
            btnOpenFolder.UseVisualStyleBackColor = true;
            btnOpenFolder.Click += btnOpenFolder_Click;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(rdbXML);
            groupBox1.Controls.Add(rdbCSV);
            groupBox1.Location = new Point(21, 150);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(405, 86);
            groupBox1.TabIndex = 23;
            groupBox1.TabStop = false;
            groupBox1.Text = "Export Data File Type";
            // 
            // rdbXML
            // 
            rdbXML.Enabled = false;
            rdbXML.Location = new Point(150, 25);
            rdbXML.Name = "rdbXML";
            rdbXML.Size = new Size(100, 35);
            rdbXML.TabIndex = 1;
            rdbXML.Text = "xml";
            rdbXML.UseVisualStyleBackColor = true;
            rdbXML.Visible = false;
            // 
            // rdbCSV
            // 
            rdbCSV.Checked = true;
            rdbCSV.Location = new Point(35, 25);
            rdbCSV.Name = "rdbCSV";
            rdbCSV.Size = new Size(100, 35);
            rdbCSV.TabIndex = 0;
            rdbCSV.TabStop = true;
            rdbCSV.Text = "csv";
            rdbCSV.UseVisualStyleBackColor = true;
            // 
            // frmExport
            // 
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(466, 346);
            Controls.Add(label1);
            Controls.Add(grbFileTypes);
            Controls.Add(groupBox1);
            Controls.Add(btnOpenFolder);
            Controls.Add(btnCancel);
            Controls.Add(btnExport);
            Controls.Add(cbxStatus);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmExport";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Export Documents";
            Activated += frmExport_Activated;
            FormClosing += frmExport_FormClosing;
            FormClosed += frmExport_FormClosed;
            Load += frmExport_Load;
            Paint += frmExport_Paint;
            Resize += frmExport_Resize;
            grbFileTypes.ResumeLayout(false);
            groupBox1.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cbxStatus;
        private System.Windows.Forms.GroupBox grbFileTypes;
        private System.Windows.Forms.RadioButton rdbTIFF;
        private System.Windows.Forms.RadioButton rdbPDF;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOpenFolder;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton rdbXML;
        private System.Windows.Forms.RadioButton rdbCSV;
    }
}