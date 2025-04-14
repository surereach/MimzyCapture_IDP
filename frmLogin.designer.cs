namespace SRDocScanIDP
{
    partial class frmLogin
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmLogin));
            txtUserPwd = new TextBox();
            label18 = new Label();
            txtUserId = new TextBox();
            label19 = new Label();
            btnLogin = new Button();
            btnCancel = new Button();
            pictureBox1 = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // txtUserPwd
            // 
            txtUserPwd.Location = new Point(162, 207);
            txtUserPwd.Margin = new Padding(3, 4, 3, 4);
            txtUserPwd.MaxLength = 12;
            txtUserPwd.Name = "txtUserPwd";
            txtUserPwd.PasswordChar = '*';
            txtUserPwd.Size = new Size(160, 27);
            txtUserPwd.TabIndex = 2;
            txtUserPwd.UseSystemPasswordChar = true;
            txtUserPwd.WordWrap = false;
            txtUserPwd.KeyDown += txtUserPwd_KeyDown;
            // 
            // label18
            // 
            label18.AutoSize = true;
            label18.Location = new Point(40, 209);
            label18.Name = "label18";
            label18.Size = new Size(106, 20);
            label18.TabIndex = 11;
            label18.Text = "User Password:";
            // 
            // txtUserId
            // 
            txtUserId.Location = new Point(162, 157);
            txtUserId.Margin = new Padding(3, 4, 3, 4);
            txtUserId.MaxLength = 18;
            txtUserId.Name = "txtUserId";
            txtUserId.Size = new Size(160, 27);
            txtUserId.TabIndex = 1;
            txtUserId.WordWrap = false;
            // 
            // label19
            // 
            label19.AutoSize = true;
            label19.Location = new Point(40, 159);
            label19.Name = "label19";
            label19.Size = new Size(60, 20);
            label19.TabIndex = 10;
            label19.Text = "User ID:";
            // 
            // btnLogin
            // 
            btnLogin.Location = new Point(71, 255);
            btnLogin.Margin = new Padding(3, 4, 3, 4);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(86, 42);
            btnLogin.TabIndex = 3;
            btnLogin.Text = "Login";
            btnLogin.UseVisualStyleBackColor = true;
            btnLogin.Click += btnLogin_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(223, 255);
            btnCancel.Margin = new Padding(3, 4, 3, 4);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(86, 42);
            btnCancel.TabIndex = 4;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // pictureBox1
            // 
            pictureBox1.Image = (Image)resources.GetObject("pictureBox1.Image");
            pictureBox1.Location = new Point(0, 0);
            pictureBox1.Margin = new Padding(3, 4, 3, 4);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(457, 125);
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.TabIndex = 13;
            pictureBox1.TabStop = false;
            // 
            // frmLogin
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(457, 358);
            ControlBox = false;
            Controls.Add(pictureBox1);
            Controls.Add(btnCancel);
            Controls.Add(btnLogin);
            Controls.Add(txtUserPwd);
            Controls.Add(label18);
            Controls.Add(txtUserId);
            Controls.Add(label19);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Margin = new Padding(3, 4, 3, 4);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmLogin";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "User Login";
            Load += frmLogin_Load;
            Paint += frmLogin_Paint;
            Resize += frmLogin_Resize;
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TextBox txtUserPwd;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.TextBox txtUserId;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Button btnLogin;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}