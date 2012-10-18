namespace winparser
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.panel1 = new System.Windows.Forms.Panel();
            this.DisplayDetails = new System.Windows.Forms.RadioButton();
            this.DisplayTable = new System.Windows.Forms.RadioButton();
            this.label4 = new System.Windows.Forms.Label();
            this.SearchNotes = new System.Windows.Forms.Label();
            this.SearchEffect = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.SearchBtn = new System.Windows.Forms.Button();
            this.SearchClass = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.SearchText = new System.Windows.Forms.TextBox();
            this.SearchBrowser = new System.Windows.Forms.WebBrowser();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.DisplayDetails);
            this.panel1.Controls.Add(this.DisplayTable);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.SearchNotes);
            this.panel1.Controls.Add(this.SearchEffect);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.SearchBtn);
            this.panel1.Controls.Add(this.SearchClass);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.SearchText);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(247, 730);
            this.panel1.TabIndex = 0;
            // 
            // DisplayDetails
            // 
            this.DisplayDetails.AutoSize = true;
            this.DisplayDetails.Location = new System.Drawing.Point(136, 183);
            this.DisplayDetails.Name = "DisplayDetails";
            this.DisplayDetails.Size = new System.Drawing.Size(60, 19);
            this.DisplayDetails.TabIndex = 10;
            this.DisplayDetails.Text = "Details";
            this.DisplayDetails.UseVisualStyleBackColor = true;
            // 
            // DisplayTable
            // 
            this.DisplayTable.AutoSize = true;
            this.DisplayTable.Checked = true;
            this.DisplayTable.Location = new System.Drawing.Point(76, 183);
            this.DisplayTable.Name = "DisplayTable";
            this.DisplayTable.Size = new System.Drawing.Size(54, 19);
            this.DisplayTable.TabIndex = 9;
            this.DisplayTable.TabStop = true;
            this.DisplayTable.Text = "Table";
            this.DisplayTable.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(9, 185);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(61, 15);
            this.label4.TabIndex = 8;
            this.label4.Text = "Display As";
            // 
            // SearchNotes
            // 
            this.SearchNotes.AutoSize = true;
            this.SearchNotes.Location = new System.Drawing.Point(93, 273);
            this.SearchNotes.Name = "SearchNotes";
            this.SearchNotes.Size = new System.Drawing.Size(16, 15);
            this.SearchNotes.TabIndex = 7;
            this.SearchNotes.Text = "...";
            // 
            // SearchEffect
            // 
            this.SearchEffect.FormattingEnabled = true;
            this.SearchEffect.Location = new System.Drawing.Point(12, 149);
            this.SearchEffect.Name = "SearchEffect";
            this.SearchEffect.Size = new System.Drawing.Size(222, 23);
            this.SearchEffect.Sorted = true;
            this.SearchEffect.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 131);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(37, 15);
            this.label3.TabIndex = 4;
            this.label3.Text = "Effect";
            // 
            // SearchBtn
            // 
            this.SearchBtn.Location = new System.Drawing.Point(12, 269);
            this.SearchBtn.Name = "SearchBtn";
            this.SearchBtn.Size = new System.Drawing.Size(75, 23);
            this.SearchBtn.TabIndex = 6;
            this.SearchBtn.Text = "Search";
            this.SearchBtn.UseVisualStyleBackColor = true;
            this.SearchBtn.Click += new System.EventHandler(this.SearchBtn_Click);
            // 
            // SearchClass
            // 
            this.SearchClass.FormattingEnabled = true;
            this.SearchClass.Location = new System.Drawing.Point(12, 94);
            this.SearchClass.Name = "SearchClass";
            this.SearchClass.Size = new System.Drawing.Size(222, 23);
            this.SearchClass.Sorted = true;
            this.SearchClass.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 76);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(34, 15);
            this.label2.TabIndex = 2;
            this.label2.Text = "Class";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(57, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Text or ID";
            // 
            // SearchText
            // 
            this.SearchText.Location = new System.Drawing.Point(12, 41);
            this.SearchText.Name = "SearchText";
            this.SearchText.Size = new System.Drawing.Size(222, 23);
            this.SearchText.TabIndex = 1;
            // 
            // SearchBrowser
            // 
            this.SearchBrowser.AllowWebBrowserDrop = false;
            this.SearchBrowser.CausesValidation = false;
            this.SearchBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SearchBrowser.Location = new System.Drawing.Point(247, 0);
            this.SearchBrowser.MinimumSize = new System.Drawing.Size(20, 20);
            this.SearchBrowser.Name = "SearchBrowser";
            this.SearchBrowser.Size = new System.Drawing.Size(761, 730);
            this.SearchBrowser.TabIndex = 1;
            this.SearchBrowser.Navigating += new System.Windows.Forms.WebBrowserNavigatingEventHandler(this.SearchBrowser_Navigating);
            // 
            // MainForm
            // 
            this.AcceptButton = this.SearchBtn;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1008, 730);
            this.Controls.Add(this.SearchBrowser);
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "EQ Spell Parser";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox SearchText;
        private System.Windows.Forms.Button SearchBtn;
        private System.Windows.Forms.ComboBox SearchClass;
        private System.Windows.Forms.WebBrowser SearchBrowser;
        private System.Windows.Forms.ComboBox SearchEffect;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label SearchNotes;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.RadioButton DisplayDetails;
        private System.Windows.Forms.RadioButton DisplayTable;

    }
}

