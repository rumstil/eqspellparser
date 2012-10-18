namespace winparser
{
    partial class FileOpenForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FileOpenForm));
            this.panel1 = new System.Windows.Forms.Panel();
            this.OpenBtn = new System.Windows.Forms.Button();
            this.DownloadTest = new System.Windows.Forms.Button();
            this.DownloadLive = new System.Windows.Forms.Button();
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.OpenBtn);
            this.panel1.Controls.Add(this.DownloadTest);
            this.panel1.Controls.Add(this.DownloadLive);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 264);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(660, 59);
            this.panel1.TabIndex = 1;
            // 
            // OpenBtn
            // 
            this.OpenBtn.Location = new System.Drawing.Point(573, 20);
            this.OpenBtn.Name = "OpenBtn";
            this.OpenBtn.Size = new System.Drawing.Size(75, 27);
            this.OpenBtn.TabIndex = 2;
            this.OpenBtn.Text = "Open";
            this.OpenBtn.UseVisualStyleBackColor = true;
            this.OpenBtn.Click += new System.EventHandler(this.listView1_DoubleClick);
            // 
            // DownloadTest
            // 
            this.DownloadTest.Location = new System.Drawing.Point(130, 20);
            this.DownloadTest.Name = "DownloadTest";
            this.DownloadTest.Size = new System.Drawing.Size(112, 27);
            this.DownloadTest.TabIndex = 1;
            this.DownloadTest.Text = "Download Test";
            this.DownloadTest.UseVisualStyleBackColor = true;
            this.DownloadTest.Click += new System.EventHandler(this.DownloadTest_Click);
            // 
            // DownloadLive
            // 
            this.DownloadLive.Location = new System.Drawing.Point(12, 20);
            this.DownloadLive.Name = "DownloadLive";
            this.DownloadLive.Size = new System.Drawing.Size(112, 27);
            this.DownloadLive.TabIndex = 0;
            this.DownloadLive.Text = "Download Live";
            this.DownloadLive.UseVisualStyleBackColor = true;
            this.DownloadLive.Click += new System.EventHandler(this.DownloadLive_Click);
            // 
            // listView1
            // 
            this.listView1.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this.listView1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.listView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView1.FullRowSelect = true;
            this.listView1.Location = new System.Drawing.Point(0, 0);
            this.listView1.MultiSelect = false;
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(660, 264);
            this.listView1.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.listView1.TabIndex = 0;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.DoubleClick += new System.EventHandler(this.listView1_DoubleClick);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "File";
            this.columnHeader1.Width = 400;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Size";
            this.columnHeader2.Width = 80;
            // 
            // FileOpenForm
            // 
            this.AcceptButton = this.OpenBtn;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(660, 323);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FileOpenForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Select a spell file to open";
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button DownloadLive;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.Button DownloadTest;
        private System.Windows.Forms.Button OpenBtn;
    }
}