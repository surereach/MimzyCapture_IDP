
namespace SRDocScanIDP
{
    partial class frmLicInfo
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmLicInfo));
            this.pnlInfo = new System.Windows.Forms.Panel();
            this.rtxInfo = new System.Windows.Forms.RichTextBox();
            this.txtLicCode = new System.Windows.Forms.TextBox();
            this.pnlInfo.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlInfo
            // 
            this.pnlInfo.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pnlInfo.Controls.Add(this.rtxInfo);
            this.pnlInfo.Location = new System.Drawing.Point(12, 49);
            this.pnlInfo.Name = "pnlInfo";
            this.pnlInfo.Size = new System.Drawing.Size(650, 380);
            this.pnlInfo.TabIndex = 10;
            // 
            // rtxInfo
            // 
            this.rtxInfo.Enabled = false;
            this.rtxInfo.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rtxInfo.Location = new System.Drawing.Point(4, 2);
            this.rtxInfo.Name = "rtxInfo";
            this.rtxInfo.Size = new System.Drawing.Size(638, 368);
            this.rtxInfo.TabIndex = 0;
            this.rtxInfo.Text = "";
            this.rtxInfo.WordWrap = false;
            // 
            // txtLicCode
            // 
            this.txtLicCode.BackColor = System.Drawing.SystemColors.ControlLight;
            this.txtLicCode.Location = new System.Drawing.Point(13, 13);
            this.txtLicCode.MaxLength = 300;
            this.txtLicCode.Name = "txtLicCode";
            this.txtLicCode.ReadOnly = true;
            this.txtLicCode.Size = new System.Drawing.Size(652, 22);
            this.txtLicCode.TabIndex = 1;
            this.txtLicCode.WordWrap = false;
            // 
            // frmLicInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(677, 450);
            this.Controls.Add(this.txtLicCode);
            this.Controls.Add(this.pnlInfo);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmLicInfo";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Mimzy Capture GEN License Information";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmLicInfo_FormClosed);
            this.Load += new System.EventHandler(this.frmLicInfo_Load);
            this.pnlInfo.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel pnlInfo;
        private System.Windows.Forms.TextBox txtLicCode;
        private System.Windows.Forms.RichTextBox rtxInfo;
    }
}