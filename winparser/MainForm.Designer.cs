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
            this.SearchCategory = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.CompareNotes = new System.Windows.Forms.Label();
            this.SearchLevel = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.CompareBtn = new System.Windows.Forms.Button();
            this.DisplayText = new System.Windows.Forms.RadioButton();
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
            this.panel1.Controls.Add(this.SearchCategory);
            this.panel1.Controls.Add(this.label6);
            this.panel1.Controls.Add(this.CompareNotes);
            this.panel1.Controls.Add(this.SearchLevel);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.CompareBtn);
            this.panel1.Controls.Add(this.DisplayText);
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
            // SearchCategory
            // 
            this.SearchCategory.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.SearchCategory.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.SearchCategory.FormattingEnabled = true;
            this.SearchCategory.Location = new System.Drawing.Point(12, 203);
            this.SearchCategory.Name = "SearchCategory";
            this.SearchCategory.Size = new System.Drawing.Size(222, 23);
            this.SearchCategory.Sorted = true;
            this.SearchCategory.TabIndex = 9;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(9, 175);
            this.label6.Name = "label6";
            this.label6.Padding = new System.Windows.Forms.Padding(0, 10, 0, 0);
            this.label6.Size = new System.Drawing.Size(55, 25);
            this.label6.TabIndex = 8;
            this.label6.Text = "Category";
            // 
            // CompareNotes
            // 
            this.CompareNotes.AutoSize = true;
            this.CompareNotes.Location = new System.Drawing.Point(92, 416);
            this.CompareNotes.Name = "CompareNotes";
            this.CompareNotes.Size = new System.Drawing.Size(16, 15);
            this.CompareNotes.TabIndex = 16;
            this.CompareNotes.Text = "...";
            // 
            // SearchLevel
            // 
            this.SearchLevel.Enabled = false;
            this.SearchLevel.Location = new System.Drawing.Point(159, 95);
            this.SearchLevel.Name = "SearchLevel";
            this.SearchLevel.Size = new System.Drawing.Size(75, 23);
            this.SearchLevel.TabIndex = 5;
            this.SearchLevel.Text = "80-254";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(156, 67);
            this.label5.Name = "label5";
            this.label5.Padding = new System.Windows.Forms.Padding(0, 10, 0, 0);
            this.label5.Size = new System.Drawing.Size(34, 25);
            this.label5.TabIndex = 4;
            this.label5.Text = "Level";
            // 
            // CompareBtn
            // 
            this.CompareBtn.Location = new System.Drawing.Point(11, 412);
            this.CompareBtn.Name = "CompareBtn";
            this.CompareBtn.Size = new System.Drawing.Size(75, 23);
            this.CompareBtn.TabIndex = 15;
            this.CompareBtn.Text = "Compare";
            this.CompareBtn.UseVisualStyleBackColor = true;
            this.CompareBtn.Click += new System.EventHandler(this.CompareBtn_Click);
            // 
            // DisplayText
            // 
            this.DisplayText.AutoSize = true;
            this.DisplayText.Location = new System.Drawing.Point(12, 284);
            this.DisplayText.Name = "DisplayText";
            this.DisplayText.Size = new System.Drawing.Size(60, 19);
            this.DisplayText.TabIndex = 11;
            this.DisplayText.Text = "Details";
            this.DisplayText.UseVisualStyleBackColor = true;
            // 
            // DisplayTable
            // 
            this.DisplayTable.AutoSize = true;
            this.DisplayTable.Checked = true;
            this.DisplayTable.Location = new System.Drawing.Point(12, 309);
            this.DisplayTable.Name = "DisplayTable";
            this.DisplayTable.Size = new System.Drawing.Size(54, 19);
            this.DisplayTable.TabIndex = 12;
            this.DisplayTable.TabStop = true;
            this.DisplayTable.Text = "Table";
            this.DisplayTable.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(9, 253);
            this.label4.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.label4.Name = "label4";
            this.label4.Padding = new System.Windows.Forms.Padding(0, 10, 0, 0);
            this.label4.Size = new System.Drawing.Size(61, 25);
            this.label4.TabIndex = 10;
            this.label4.Text = "Display As";
            // 
            // SearchNotes
            // 
            this.SearchNotes.AutoSize = true;
            this.SearchNotes.Location = new System.Drawing.Point(92, 363);
            this.SearchNotes.Name = "SearchNotes";
            this.SearchNotes.Size = new System.Drawing.Size(16, 15);
            this.SearchNotes.TabIndex = 14;
            this.SearchNotes.Text = "...";
            // 
            // SearchEffect
            // 
            this.SearchEffect.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.SearchEffect.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.SearchEffect.FormattingEnabled = true;
            this.SearchEffect.Location = new System.Drawing.Point(12, 149);
            this.SearchEffect.Name = "SearchEffect";
            this.SearchEffect.Size = new System.Drawing.Size(222, 23);
            this.SearchEffect.Sorted = true;
            this.SearchEffect.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 121);
            this.label3.Name = "label3";
            this.label3.Padding = new System.Windows.Forms.Padding(0, 10, 0, 0);
            this.label3.Size = new System.Drawing.Size(60, 25);
            this.label3.TabIndex = 6;
            this.label3.Text = "Has Effect";
            // 
            // SearchBtn
            // 
            this.SearchBtn.Location = new System.Drawing.Point(11, 359);
            this.SearchBtn.Name = "SearchBtn";
            this.SearchBtn.Size = new System.Drawing.Size(75, 23);
            this.SearchBtn.TabIndex = 13;
            this.SearchBtn.Text = "Search";
            this.SearchBtn.UseVisualStyleBackColor = true;
            this.SearchBtn.Click += new System.EventHandler(this.SearchBtn_Click);
            // 
            // SearchClass
            // 
            this.SearchClass.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.SearchClass.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.SearchClass.FormattingEnabled = true;
            this.SearchClass.Location = new System.Drawing.Point(12, 95);
            this.SearchClass.Name = "SearchClass";
            this.SearchClass.Size = new System.Drawing.Size(141, 23);
            this.SearchClass.Sorted = true;
            this.SearchClass.TabIndex = 3;
            this.SearchClass.TextChanged += new System.EventHandler(this.SearchClass_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 67);
            this.label2.Name = "label2";
            this.label2.Padding = new System.Windows.Forms.Padding(0, 10, 0, 0);
            this.label2.Size = new System.Drawing.Size(34, 25);
            this.label2.TabIndex = 2;
            this.label2.Text = "Class";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 13);
            this.label1.Name = "label1";
            this.label1.Padding = new System.Windows.Forms.Padding(0, 10, 0, 0);
            this.label1.Size = new System.Drawing.Size(57, 25);
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
        private System.Windows.Forms.RadioButton DisplayText;
        private System.Windows.Forms.RadioButton DisplayTable;
        private System.Windows.Forms.Button CompareBtn;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox SearchLevel;
        private System.Windows.Forms.Label CompareNotes;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox SearchCategory;

    }
}

