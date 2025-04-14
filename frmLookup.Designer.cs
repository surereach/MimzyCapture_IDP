
namespace SRDocScanIDP
{
    partial class frmLookup
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmLookup));
            this.txtLookup = new System.Windows.Forms.TextBox();
            this.btnSearch = new System.Windows.Forms.Button();
            this.lvwData = new System.Windows.Forms.ListView();
            this.col1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.col2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.col3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.col4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.col5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.cbxField = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // txtLookup
            // 
            this.txtLookup.Location = new System.Drawing.Point(252, 14);
            this.txtLookup.Name = "txtLookup";
            this.txtLookup.Size = new System.Drawing.Size(394, 22);
            this.txtLookup.TabIndex = 1;
            // 
            // btnSearch
            // 
            this.btnSearch.Location = new System.Drawing.Point(652, 8);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(130, 35);
            this.btnSearch.TabIndex = 2;
            this.btnSearch.Text = "Search";
            this.btnSearch.UseVisualStyleBackColor = true;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            // 
            // lvwData
            // 
            this.lvwData.BackColor = System.Drawing.SystemColors.ControlLight;
            this.lvwData.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.col1,
            this.col2,
            this.col3,
            this.col4,
            this.col5});
            this.lvwData.FullRowSelect = true;
            this.lvwData.GridLines = true;
            this.lvwData.HideSelection = false;
            this.lvwData.Location = new System.Drawing.Point(7, 47);
            this.lvwData.MultiSelect = false;
            this.lvwData.Name = "lvwData";
            this.lvwData.Size = new System.Drawing.Size(775, 350);
            this.lvwData.TabIndex = 31;
            this.lvwData.UseCompatibleStateImageBehavior = false;
            this.lvwData.View = System.Windows.Forms.View.Details;
            this.lvwData.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lvwData_MouseDoubleClick);
            // 
            // col1
            // 
            this.col1.Text = "id";
            this.col1.Width = 0;
            // 
            // col2
            // 
            this.col2.Text = "Col1";
            this.col2.Width = 160;
            // 
            // col3
            // 
            this.col3.Text = "Col2";
            this.col3.Width = 160;
            // 
            // col4
            // 
            this.col4.Text = "Col3";
            this.col4.Width = 160;
            // 
            // col5
            // 
            this.col5.Text = "Col4";
            this.col5.Width = 160;
            // 
            // cbxField
            // 
            this.cbxField.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxField.FormattingEnabled = true;
            this.cbxField.Location = new System.Drawing.Point(7, 12);
            this.cbxField.Name = "cbxField";
            this.cbxField.Size = new System.Drawing.Size(238, 24);
            this.cbxField.TabIndex = 0;
            // 
            // frmLookup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(789, 408);
            this.Controls.Add(this.cbxField);
            this.Controls.Add(this.lvwData);
            this.Controls.Add(this.btnSearch);
            this.Controls.Add(this.txtLookup);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmLookup";
            this.Text = "Search";
            this.Load += new System.EventHandler(this.frmLookup_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtLookup;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.ListView lvwData;
        private System.Windows.Forms.ColumnHeader col1;
        private System.Windows.Forms.ColumnHeader col2;
        private System.Windows.Forms.ColumnHeader col3;
        private System.Windows.Forms.ColumnHeader col4;
        private System.Windows.Forms.ColumnHeader col5;
        private System.Windows.Forms.ComboBox cbxField;
    }
}