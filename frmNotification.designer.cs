
namespace SRDocScanIDP
{
    partial class frmNotification
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
            NotifyToolStrip = new ToolStrip();
            btnDeleteAllStrip = new ToolStripButton();
            lvwNotice = new ListView();
            colItem = new ColumnHeader();
            colMark = new ColumnHeader();
            colDateTime = new ColumnHeader();
            colRef = new ColumnHeader();
            colDesc = new ColumnHeader();
            NotifyToolStrip.SuspendLayout();
            SuspendLayout();
            // 
            // NotifyToolStrip
            // 
            NotifyToolStrip.AutoSize = false;
            NotifyToolStrip.ImageScalingSize = new Size(24, 24);
            NotifyToolStrip.Items.AddRange(new ToolStripItem[] { btnDeleteAllStrip });
            NotifyToolStrip.Location = new Point(0, 0);
            NotifyToolStrip.Name = "NotifyToolStrip";
            NotifyToolStrip.Size = new Size(612, 30);
            NotifyToolStrip.TabIndex = 1;
            NotifyToolStrip.Text = "toolStrip1";
            // 
            // btnDeleteAllStrip
            // 
            btnDeleteAllStrip.DisplayStyle = ToolStripItemDisplayStyle.Image;
            btnDeleteAllStrip.Image = Properties.Resources.Delete_All;
            btnDeleteAllStrip.ImageTransparentColor = Color.Magenta;
            btnDeleteAllStrip.Name = "btnDeleteAllStrip";
            btnDeleteAllStrip.Size = new Size(28, 27);
            btnDeleteAllStrip.Text = "Delete all";
            btnDeleteAllStrip.Click += btnDeleteAllStrip_Click;
            // 
            // lvwNotice
            // 
            lvwNotice.Columns.AddRange(new ColumnHeader[] { colItem, colMark, colDateTime, colRef, colDesc });
            lvwNotice.FullRowSelect = true;
            lvwNotice.GridLines = true;
            lvwNotice.Location = new Point(1, 32);
            lvwNotice.MultiSelect = false;
            lvwNotice.Name = "lvwNotice";
            lvwNotice.Size = new Size(610, 254);
            lvwNotice.TabIndex = 2;
            lvwNotice.UseCompatibleStateImageBehavior = false;
            lvwNotice.View = View.Details;
            lvwNotice.ColumnClick += lvwNotice_ColumnClick;
            // 
            // colItem
            // 
            colItem.Text = "Item";
            colItem.Width = 40;
            // 
            // colMark
            // 
            colMark.Text = "Mark";
            colMark.Width = 50;
            // 
            // colDateTime
            // 
            colDateTime.Text = "Date Time";
            colDateTime.Width = 150;
            // 
            // colRef
            // 
            colRef.Text = "Reference";
            colRef.Width = 210;
            // 
            // colDesc
            // 
            colDesc.Text = "Description";
            colDesc.Width = 390;
            // 
            // frmNotification
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(612, 311);
            Controls.Add(lvwNotice);
            Controls.Add(NotifyToolStrip);
            MaximizeBox = false;
            Name = "frmNotification";
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Notification";
            FormClosed += frmNotification_FormClosed;
            Load += frmNotification_Load;
            Resize += frmNotification_Resize;
            NotifyToolStrip.ResumeLayout(false);
            NotifyToolStrip.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.ToolStrip NotifyToolStrip;
        private System.Windows.Forms.ListView lvwNotice;
        private System.Windows.Forms.ColumnHeader colItem;
        private System.Windows.Forms.ColumnHeader colMark;
        private System.Windows.Forms.ColumnHeader colRef;
        private System.Windows.Forms.ColumnHeader colDesc;
        private System.Windows.Forms.ColumnHeader colDateTime;
        private System.Windows.Forms.ToolStripButton btnDeleteAllStrip;
    }
}