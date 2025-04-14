
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
            this.label1 = new System.Windows.Forms.Label();
            this.cbxStatus = new System.Windows.Forms.ComboBox();
            this.grbFileTypes = new System.Windows.Forms.GroupBox();
            this.rdbTIFF = new System.Windows.Forms.RadioButton();
            this.rdbPDF = new System.Windows.Forms.RadioButton();
            this.btnExport = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOpenFolder = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rdbXML = new System.Windows.Forms.RadioButton();
            this.rdbCSV = new System.Windows.Forms.RadioButton();
            this.grbFileTypes.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(21, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(104, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "Document Sets";
            // 
            // cbxStatus
            // 
            this.cbxStatus.FormattingEnabled = true;
            this.cbxStatus.Location = new System.Drawing.Point(158, 23);
            this.cbxStatus.Name = "cbxStatus";
            this.cbxStatus.Size = new System.Drawing.Size(329, 24);
            this.cbxStatus.TabIndex = 1;
            // 
            // grbFileTypes
            // 
            this.grbFileTypes.Controls.Add(this.rdbTIFF);
            this.grbFileTypes.Controls.Add(this.rdbPDF);
            this.grbFileTypes.Location = new System.Drawing.Point(24, 65);
            this.grbFileTypes.Name = "grbFileTypes";
            this.grbFileTypes.Size = new System.Drawing.Size(463, 58);
            this.grbFileTypes.TabIndex = 10;
            this.grbFileTypes.TabStop = false;
            this.grbFileTypes.Text = "Export File Type";
            // 
            // rdbTIFF
            // 
            this.rdbTIFF.AutoSize = true;
            this.rdbTIFF.Location = new System.Drawing.Point(172, 27);
            this.rdbTIFF.Name = "rdbTIFF";
            this.rdbTIFF.Size = new System.Drawing.Size(44, 21);
            this.rdbTIFF.TabIndex = 1;
            this.rdbTIFF.Text = "tiff";
            this.rdbTIFF.UseVisualStyleBackColor = true;
            // 
            // rdbPDF
            // 
            this.rdbPDF.AutoSize = true;
            this.rdbPDF.Checked = true;
            this.rdbPDF.Location = new System.Drawing.Point(40, 27);
            this.rdbPDF.Name = "rdbPDF";
            this.rdbPDF.Size = new System.Drawing.Size(49, 21);
            this.rdbPDF.TabIndex = 0;
            this.rdbPDF.TabStop = true;
            this.rdbPDF.Text = "pdf";
            this.rdbPDF.UseVisualStyleBackColor = true;
            // 
            // btnExport
            // 
            this.btnExport.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnExport.Location = new System.Drawing.Point(226, 213);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(118, 35);
            this.btnExport.TabIndex = 20;
            this.btnExport.Text = "&Export";
            this.btnExport.UseVisualStyleBackColor = true;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnCancel.Location = new System.Drawing.Point(369, 213);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(118, 35);
            this.btnCancel.TabIndex = 21;
            this.btnCancel.Text = "&Close";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOpenFolder
            // 
            this.btnOpenFolder.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnOpenFolder.Location = new System.Drawing.Point(24, 213);
            this.btnOpenFolder.Name = "btnOpenFolder";
            this.btnOpenFolder.Size = new System.Drawing.Size(118, 35);
            this.btnOpenFolder.TabIndex = 22;
            this.btnOpenFolder.Text = "&Open Folder";
            this.btnOpenFolder.UseVisualStyleBackColor = true;
            this.btnOpenFolder.Click += new System.EventHandler(this.btnOpenFolder_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.rdbXML);
            this.groupBox1.Controls.Add(this.rdbCSV);
            this.groupBox1.Location = new System.Drawing.Point(24, 135);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(463, 63);
            this.groupBox1.TabIndex = 23;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Export Data File Type";
            // 
            // rdbXML
            // 
            this.rdbXML.AutoSize = true;
            this.rdbXML.Enabled = false;
            this.rdbXML.Location = new System.Drawing.Point(172, 27);
            this.rdbXML.Name = "rdbXML";
            this.rdbXML.Size = new System.Drawing.Size(49, 21);
            this.rdbXML.TabIndex = 1;
            this.rdbXML.Text = "xml";
            this.rdbXML.UseVisualStyleBackColor = true;
            this.rdbXML.Visible = false;
            // 
            // rdbCSV
            // 
            this.rdbCSV.AutoSize = true;
            this.rdbCSV.Checked = true;
            this.rdbCSV.Location = new System.Drawing.Point(40, 27);
            this.rdbCSV.Name = "rdbCSV";
            this.rdbCSV.Size = new System.Drawing.Size(50, 21);
            this.rdbCSV.TabIndex = 0;
            this.rdbCSV.TabStop = true;
            this.rdbCSV.Text = "csv";
            this.rdbCSV.UseVisualStyleBackColor = true;
            // 
            // frmExport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(510, 263);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnOpenFolder);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnExport);
            this.Controls.Add(this.grbFileTypes);
            this.Controls.Add(this.cbxStatus);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmExport";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Export Documents";
            this.Activated += new System.EventHandler(this.frmExport_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmExport_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmExport_FormClosed);
            this.Load += new System.EventHandler(this.frmExport_Load);
            this.Resize += new System.EventHandler(this.frmExport_Resize);
            this.grbFileTypes.ResumeLayout(false);
            this.grbFileTypes.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

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