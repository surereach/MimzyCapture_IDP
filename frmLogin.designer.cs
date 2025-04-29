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
            pictureBox1 = new PictureBox();
            panel1 = new Panel();
            btnCancel = new Button();
            txtUserId = new TextBox();
            btnLogin = new Button();
            label19 = new Label();
            txtUserPwd = new TextBox();
            label18 = new Label();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // pictureBox1
            // 
            pictureBox1.Dock = DockStyle.Top;
            pictureBox1.Image = (Image)resources.GetObject("pictureBox1.Image");
            pictureBox1.Location = new Point(0, 0);
            pictureBox1.Margin = new Padding(3, 4, 3, 4);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(457, 125);
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.TabIndex = 13;
            pictureBox1.TabStop = false;
            // 
            // panel1
            // 
            panel1.Controls.Add(btnCancel);
            panel1.Controls.Add(txtUserId);
            panel1.Controls.Add(btnLogin);
            panel1.Controls.Add(label19);
            panel1.Controls.Add(txtUserPwd);
            panel1.Controls.Add(label18);
            panel1.Dock = DockStyle.Bottom;
            panel1.Location = new Point(0, 128);
            panel1.Name = "panel1";
            panel1.Size = new Size(457, 178);
            panel1.TabIndex = 14;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(256, 131);
            btnCancel.Margin = new Padding(3, 4, 3, 4);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(86, 43);
            btnCancel.TabIndex = 17;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // txtUserId
            // 
            txtUserId.Location = new Point(230, 18);
            txtUserId.Margin = new Padding(3, 4, 3, 4);
            txtUserId.MaxLength = 18;
            txtUserId.Name = "txtUserId";
            txtUserId.Size = new Size(159, 23);
            txtUserId.TabIndex = 11;
            txtUserId.WordWrap = false;
            // 
            // btnLogin
            // 
            btnLogin.Location = new Point(110, 131);
            btnLogin.Margin = new Padding(3, 4, 3, 4);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(86, 43);
            btnLogin.TabIndex = 16;
            btnLogin.Text = "Login";
            btnLogin.UseVisualStyleBackColor = true;
            btnLogin.Click += btnLogin_Click;
            // 
            // label19
            // 
            label19.Location = new Point(65, 16);
            label19.Name = "label19";
            label19.Size = new Size(129, 27);
            label19.TabIndex = 12;
            label19.Text = "User ID:";
            label19.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtUserPwd
            // 
            txtUserPwd.Location = new Point(230, 66);
            txtUserPwd.Margin = new Padding(3, 4, 3, 4);
            txtUserPwd.MaxLength = 12;
            txtUserPwd.Name = "txtUserPwd";
            txtUserPwd.PasswordChar = '*';
            txtUserPwd.Size = new Size(159, 23);
            txtUserPwd.TabIndex = 15;
            txtUserPwd.UseSystemPasswordChar = true;
            txtUserPwd.WordWrap = false;
            txtUserPwd.KeyDown += txtUserPwd_KeyDown;
            // 
            // label18
            // 
            label18.Location = new Point(65, 63);
            label18.Name = "label18";
            label18.Size = new Size(159, 29);
            label18.TabIndex = 18;
            label18.Text = "User Password:";
            label18.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // frmLogin
            // 
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(457, 306);
            ControlBox = false;
            Controls.Add(pictureBox1);
            Controls.Add(panel1);
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
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private System.Windows.Forms.PictureBox pictureBox1;
        private Panel panel1;
        private TextBox txtUserId;
        private Label label19;
        private Button btnCancel;
        private Button btnLogin;
        private TextBox txtUserPwd;
        private Label label18;
    }
}