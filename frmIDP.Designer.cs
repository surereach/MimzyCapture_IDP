
namespace SRDocScanIDP
{
    partial class frmIDP
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmIDP));
            txtPathFile = new TextBox();
            btnSend = new Button();
            lvwData = new ListView();
            col1 = new ColumnHeader();
            col2 = new ColumnHeader();
            col3 = new ColumnHeader();
            col4 = new ColumnHeader();
            col5 = new ColumnHeader();
            cbxMethod = new ComboBox();
            btnLoad = new Button();
            toolTip1 = new ToolTip(components);
            openFileDialog = new OpenFileDialog();
            SuspendLayout();
            // 
            // txtPathFile
            // 
            txtPathFile.Location = new Point(237, 15);
            txtPathFile.Margin = new Padding(3, 4, 3, 4);
            txtPathFile.Name = "txtPathFile";
            txtPathFile.ReadOnly = true;
            txtPathFile.Size = new Size(438, 27);
            txtPathFile.TabIndex = 6;
            // 
            // btnSend
            // 
            btnSend.Location = new Point(681, 10);
            btnSend.Margin = new Padding(3, 4, 3, 4);
            btnSend.Name = "btnSend";
            btnSend.Size = new Size(100, 40);
            btnSend.TabIndex = 10;
            btnSend.Text = "Send";
            btnSend.UseVisualStyleBackColor = true;
            btnSend.Click += btnSend_Click;
            // 
            // lvwData
            // 
            lvwData.BackColor = SystemColors.ControlLight;
            lvwData.Columns.AddRange(new ColumnHeader[] { col1, col2, col3, col4, col5 });
            lvwData.FullRowSelect = true;
            lvwData.GridLines = true;
            lvwData.Location = new Point(7, 59);
            lvwData.Margin = new Padding(3, 4, 3, 4);
            lvwData.MultiSelect = false;
            lvwData.Name = "lvwData";
            lvwData.Size = new Size(775, 436);
            lvwData.TabIndex = 30;
            lvwData.UseCompatibleStateImageBehavior = false;
            lvwData.View = View.Details;
            lvwData.MouseDoubleClick += lvwData_MouseDoubleClick;
            // 
            // col1
            // 
            col1.Text = "id";
            col1.Width = 0;
            // 
            // col2
            // 
            col2.Text = "Col1";
            col2.Width = 160;
            // 
            // col3
            // 
            col3.Text = "Col2";
            col3.Width = 160;
            // 
            // col4
            // 
            col4.Text = "Col3";
            col4.Width = 160;
            // 
            // col5
            // 
            col5.Text = "Col4";
            col5.Width = 160;
            // 
            // cbxMethod
            // 
            cbxMethod.DropDownStyle = ComboBoxStyle.DropDownList;
            cbxMethod.FormattingEnabled = true;
            cbxMethod.Items.AddRange(new object[] { "Classify", "Analysis", "Training" });
            cbxMethod.Location = new Point(7, 15);
            cbxMethod.Margin = new Padding(3, 4, 3, 4);
            cbxMethod.Name = "cbxMethod";
            cbxMethod.Size = new Size(168, 28);
            cbxMethod.TabIndex = 0;
            // 
            // btnLoad
            // 
            btnLoad.FlatStyle = FlatStyle.Popup;
            btnLoad.Image = Properties.Resources.Load_Images;
            btnLoad.Location = new Point(180, 10);
            btnLoad.Name = "btnLoad";
            btnLoad.Size = new Size(50, 40);
            btnLoad.TabIndex = 5;
            toolTip1.SetToolTip(btnLoad, "Load File");
            btnLoad.UseVisualStyleBackColor = true;
            btnLoad.Click += btnLoad_Click;
            // 
            // frmIDP
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(789, 510);
            Controls.Add(btnLoad);
            Controls.Add(cbxMethod);
            Controls.Add(lvwData);
            Controls.Add(btnSend);
            Controls.Add(txtPathFile);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(3, 4, 3, 4);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmIDP";
            Text = "IDP";
            Load += frmIDP_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TextBox txtPathFile;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.ListView lvwData;
        private System.Windows.Forms.ColumnHeader col1;
        private System.Windows.Forms.ColumnHeader col2;
        private System.Windows.Forms.ColumnHeader col3;
        private System.Windows.Forms.ColumnHeader col4;
        private System.Windows.Forms.ColumnHeader col5;
        private System.Windows.Forms.ComboBox cbxMethod;
        private Button btnLoad;
        private ToolTip toolTip1;
        private OpenFileDialog openFileDialog;
    }
}