
namespace SRDocScanIDP
{
    partial class frmStation
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
            this.txtStationCode = new System.Windows.Forms.TextBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.txtBranch = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.lblBranchExist = new System.Windows.Forms.Label();
            this.lblStnExist = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.ddlSessMax = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.lblSessExist = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 64);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(88, 17);
            this.label1.TabIndex = 2;
            this.label1.Text = "Scan Station";
            // 
            // txtStationCode
            // 
            this.txtStationCode.Location = new System.Drawing.Point(142, 62);
            this.txtStationCode.MaxLength = 20;
            this.txtStationCode.Name = "txtStationCode";
            this.txtStationCode.Size = new System.Drawing.Size(210, 22);
            this.txtStationCode.TabIndex = 4;
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(263, 158);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 35);
            this.btnOK.TabIndex = 50;
            this.btnOK.Text = "&OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // txtBranch
            // 
            this.txtBranch.Location = new System.Drawing.Point(142, 22);
            this.txtBranch.MaxLength = 20;
            this.txtBranch.Name = "txtBranch";
            this.txtBranch.Size = new System.Drawing.Size(210, 22);
            this.txtBranch.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 24);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(90, 17);
            this.label2.TabIndex = 0;
            this.label2.Text = "Branch Code";
            // 
            // lblBranchExist
            // 
            this.lblBranchExist.AutoSize = true;
            this.lblBranchExist.Location = new System.Drawing.Point(12, 145);
            this.lblBranchExist.Name = "lblBranchExist";
            this.lblBranchExist.Size = new System.Drawing.Size(0, 17);
            this.lblBranchExist.TabIndex = 21;
            this.lblBranchExist.Visible = false;
            // 
            // lblStnExist
            // 
            this.lblStnExist.AutoSize = true;
            this.lblStnExist.Location = new System.Drawing.Point(83, 104);
            this.lblStnExist.Name = "lblStnExist";
            this.lblStnExist.Size = new System.Drawing.Size(0, 17);
            this.lblStnExist.TabIndex = 22;
            this.lblStnExist.Visible = false;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 104);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(113, 17);
            this.label3.TabIndex = 23;
            this.label3.Text = "Session Timeout";
            // 
            // ddlSessMax
            // 
            this.ddlSessMax.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddlSessMax.FormattingEnabled = true;
            this.ddlSessMax.Items.AddRange(new object[] {
            "0",
            "30",
            "60",
            "120",
            "300"});
            this.ddlSessMax.Location = new System.Drawing.Point(142, 101);
            this.ddlSessMax.Name = "ddlSessMax";
            this.ddlSessMax.Size = new System.Drawing.Size(90, 24);
            this.ddlSessMax.TabIndex = 24;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(241, 104);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(63, 17);
            this.label4.TabIndex = 51;
            this.label4.Text = "Seconds";
            // 
            // lblSessExist
            // 
            this.lblSessExist.AutoSize = true;
            this.lblSessExist.Location = new System.Drawing.Point(21, 131);
            this.lblSessExist.Name = "lblSessExist";
            this.lblSessExist.Size = new System.Drawing.Size(0, 17);
            this.lblSessExist.TabIndex = 52;
            this.lblSessExist.Visible = false;
            // 
            // frmStation
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(367, 213);
            this.Controls.Add(this.lblSessExist);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.ddlSessMax);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.lblStnExist);
            this.Controls.Add(this.lblBranchExist);
            this.Controls.Add(this.txtBranch);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.txtStationCode);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmStation";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Scan Station Settings";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmStation_FormClosed);
            this.Load += new System.EventHandler(this.frmStation_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtStationCode;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.TextBox txtBranch;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblBranchExist;
        private System.Windows.Forms.Label lblStnExist;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox ddlSessMax;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lblSessExist;
    }
}