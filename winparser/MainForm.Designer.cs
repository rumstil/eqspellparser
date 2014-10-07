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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.SearchFilters = new System.Windows.Forms.Panel();
            this.ResetBtn = new System.Windows.Forms.LinkLabel();
            this.SearchEffectSlot3 = new System.Windows.Forms.ComboBox();
            this.SearchEffectSlot2 = new System.Windows.Forms.ComboBox();
            this.SearchEffect3 = new System.Windows.Forms.ComboBox();
            this.SearchEffect2 = new System.Windows.Forms.ComboBox();
            this.ShowRelated = new System.Windows.Forms.CheckBox();
            this.SearchEffectSlot1 = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.SearchCategory = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.SearchLevel = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.CompareBtn = new System.Windows.Forms.Button();
            this.DisplayText = new System.Windows.Forms.RadioButton();
            this.DisplayTable = new System.Windows.Forms.RadioButton();
            this.label4 = new System.Windows.Forms.Label();
            this.SearchNotes = new System.Windows.Forms.Label();
            this.SearchEffect1 = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.SearchBtn = new System.Windows.Forms.Button();
            this.SearchClass = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.SearchText = new System.Windows.Forms.TextBox();
            this.SearchBrowser = new System.Windows.Forms.WebBrowser();
            this.AutoSearch = new System.Windows.Forms.Timer(this.components);
            this.ToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.SearchFilters.SuspendLayout();
            this.SuspendLayout();
            // 
            // SearchFilters
            // 
            this.SearchFilters.Controls.Add(this.ResetBtn);
            this.SearchFilters.Controls.Add(this.SearchEffectSlot3);
            this.SearchFilters.Controls.Add(this.SearchEffectSlot2);
            this.SearchFilters.Controls.Add(this.SearchEffect3);
            this.SearchFilters.Controls.Add(this.SearchEffect2);
            this.SearchFilters.Controls.Add(this.ShowRelated);
            this.SearchFilters.Controls.Add(this.SearchEffectSlot1);
            this.SearchFilters.Controls.Add(this.label7);
            this.SearchFilters.Controls.Add(this.SearchCategory);
            this.SearchFilters.Controls.Add(this.label6);
            this.SearchFilters.Controls.Add(this.SearchLevel);
            this.SearchFilters.Controls.Add(this.label5);
            this.SearchFilters.Controls.Add(this.CompareBtn);
            this.SearchFilters.Controls.Add(this.DisplayText);
            this.SearchFilters.Controls.Add(this.DisplayTable);
            this.SearchFilters.Controls.Add(this.label4);
            this.SearchFilters.Controls.Add(this.SearchNotes);
            this.SearchFilters.Controls.Add(this.SearchEffect1);
            this.SearchFilters.Controls.Add(this.label3);
            this.SearchFilters.Controls.Add(this.SearchBtn);
            this.SearchFilters.Controls.Add(this.SearchClass);
            this.SearchFilters.Controls.Add(this.label2);
            this.SearchFilters.Controls.Add(this.label1);
            this.SearchFilters.Controls.Add(this.SearchText);
            this.SearchFilters.Dock = System.Windows.Forms.DockStyle.Left;
            this.SearchFilters.Location = new System.Drawing.Point(0, 0);
            this.SearchFilters.Name = "SearchFilters";
            this.SearchFilters.Size = new System.Drawing.Size(247, 730);
            this.SearchFilters.TabIndex = 0;
            // 
            // ResetBtn
            // 
            this.ResetBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ResetBtn.AutoSize = true;
            this.ResetBtn.Location = new System.Drawing.Point(189, 18);
            this.ResetBtn.Name = "ResetBtn";
            this.ResetBtn.Size = new System.Drawing.Size(45, 20);
            this.ResetBtn.TabIndex = 22;
            this.ResetBtn.TabStop = true;
            this.ResetBtn.Text = "Reset";
            this.ResetBtn.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.ToolTip.SetToolTip(this.ResetBtn, "Reset all search fields.");
            this.ResetBtn.Click += new System.EventHandler(this.ResetBtn_Click);
            // 
            // SearchEffectSlot3
            // 
            this.SearchEffectSlot3.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.SearchEffectSlot3.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.SearchEffectSlot3.FormattingEnabled = true;
            this.SearchEffectSlot3.Items.AddRange(new object[] {
            "",
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10",
            "11",
            "12"});
            this.SearchEffectSlot3.Location = new System.Drawing.Point(159, 246);
            this.SearchEffectSlot3.Name = "SearchEffectSlot3";
            this.SearchEffectSlot3.Size = new System.Drawing.Size(75, 28);
            this.SearchEffectSlot3.TabIndex = 13;
            this.ToolTip.SetToolTip(this.SearchEffectSlot3, "Limit the effect filter to a single slot.");
            this.SearchEffectSlot3.TextChanged += new System.EventHandler(this.SearchText_TextChanged);
            // 
            // SearchEffectSlot2
            // 
            this.SearchEffectSlot2.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.SearchEffectSlot2.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.SearchEffectSlot2.FormattingEnabled = true;
            this.SearchEffectSlot2.Items.AddRange(new object[] {
            "",
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10",
            "11",
            "12"});
            this.SearchEffectSlot2.Location = new System.Drawing.Point(159, 212);
            this.SearchEffectSlot2.Name = "SearchEffectSlot2";
            this.SearchEffectSlot2.Size = new System.Drawing.Size(75, 28);
            this.SearchEffectSlot2.TabIndex = 11;
            this.ToolTip.SetToolTip(this.SearchEffectSlot2, "Limit the effect filter to a single slot.");
            this.SearchEffectSlot2.TextChanged += new System.EventHandler(this.SearchText_TextChanged);
            // 
            // SearchEffect3
            // 
            this.SearchEffect3.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.SearchEffect3.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.SearchEffect3.FormattingEnabled = true;
            this.SearchEffect3.Location = new System.Drawing.Point(13, 246);
            this.SearchEffect3.Name = "SearchEffect3";
            this.SearchEffect3.Size = new System.Drawing.Size(141, 28);
            this.SearchEffect3.Sorted = true;
            this.SearchEffect3.TabIndex = 12;
            this.ToolTip.SetToolTip(this.SearchEffect3, "Select a spell effect type from the list, enter an SPA number, or type some text " +
        "that appears in the effect description.");
            this.SearchEffect3.TextChanged += new System.EventHandler(this.SearchText_TextChanged);
            // 
            // SearchEffect2
            // 
            this.SearchEffect2.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.SearchEffect2.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.SearchEffect2.FormattingEnabled = true;
            this.SearchEffect2.Location = new System.Drawing.Point(13, 212);
            this.SearchEffect2.Name = "SearchEffect2";
            this.SearchEffect2.Size = new System.Drawing.Size(141, 28);
            this.SearchEffect2.Sorted = true;
            this.SearchEffect2.TabIndex = 10;
            this.ToolTip.SetToolTip(this.SearchEffect2, "Select a spell effect type from the list, enter an SPA number, or type some text " +
        "that appears in the effect description.");
            this.SearchEffect2.TextChanged += new System.EventHandler(this.SearchText_TextChanged);
            // 
            // ShowRelated
            // 
            this.ShowRelated.AutoSize = true;
            this.ShowRelated.Checked = true;
            this.ShowRelated.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ShowRelated.Location = new System.Drawing.Point(13, 368);
            this.ShowRelated.Name = "ShowRelated";
            this.ShowRelated.Size = new System.Drawing.Size(159, 24);
            this.ShowRelated.TabIndex = 16;
            this.ShowRelated.Text = "Show related spells";
            this.ShowRelated.UseVisualStyleBackColor = true;
            this.ShowRelated.CheckedChanged += new System.EventHandler(this.SearchText_TextChanged);
            // 
            // SearchEffectSlot1
            // 
            this.SearchEffectSlot1.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.SearchEffectSlot1.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.SearchEffectSlot1.FormattingEnabled = true;
            this.SearchEffectSlot1.Items.AddRange(new object[] {
            "",
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10",
            "11",
            "12"});
            this.SearchEffectSlot1.Location = new System.Drawing.Point(159, 178);
            this.SearchEffectSlot1.Name = "SearchEffectSlot1";
            this.SearchEffectSlot1.Size = new System.Drawing.Size(75, 28);
            this.SearchEffectSlot1.TabIndex = 9;
            this.ToolTip.SetToolTip(this.SearchEffectSlot1, "Limit the effect filter to a single slot.");
            this.SearchEffectSlot1.TextChanged += new System.EventHandler(this.SearchText_TextChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(156, 145);
            this.label7.Name = "label7";
            this.label7.Padding = new System.Windows.Forms.Padding(0, 10, 0, 0);
            this.label7.Size = new System.Drawing.Size(51, 30);
            this.label7.TabIndex = 8;
            this.label7.Text = "In Slot";
            // 
            // SearchCategory
            // 
            this.SearchCategory.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.SearchCategory.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.SearchCategory.FormattingEnabled = true;
            this.SearchCategory.Location = new System.Drawing.Point(13, 319);
            this.SearchCategory.Name = "SearchCategory";
            this.SearchCategory.Size = new System.Drawing.Size(222, 28);
            this.SearchCategory.Sorted = true;
            this.SearchCategory.TabIndex = 15;
            this.SearchCategory.TextChanged += new System.EventHandler(this.SearchText_TextChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(9, 286);
            this.label6.Name = "label6";
            this.label6.Padding = new System.Windows.Forms.Padding(0, 10, 0, 0);
            this.label6.Size = new System.Drawing.Size(69, 30);
            this.label6.TabIndex = 14;
            this.label6.Text = "Category";
            // 
            // SearchLevel
            // 
            this.SearchLevel.Enabled = false;
            this.SearchLevel.Location = new System.Drawing.Point(159, 108);
            this.SearchLevel.Name = "SearchLevel";
            this.SearchLevel.Size = new System.Drawing.Size(75, 27);
            this.SearchLevel.TabIndex = 5;
            this.SearchLevel.Text = "81-254";
            this.ToolTip.SetToolTip(this.SearchLevel, "Enter a single level (e.g. 81) or a level range (e.g. 81 - 85).  This filter is o" +
        "nly applied when a class is selected. AA are level 254.");
            this.SearchLevel.TextChanged += new System.EventHandler(this.SearchText_TextChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(156, 75);
            this.label5.Name = "label5";
            this.label5.Padding = new System.Windows.Forms.Padding(0, 10, 0, 0);
            this.label5.Size = new System.Drawing.Size(43, 30);
            this.label5.TabIndex = 4;
            this.label5.Text = "Level";
            // 
            // CompareBtn
            // 
            this.CompareBtn.Location = new System.Drawing.Point(11, 557);
            this.CompareBtn.Name = "CompareBtn";
            this.CompareBtn.Size = new System.Drawing.Size(91, 27);
            this.CompareBtn.TabIndex = 21;
            this.CompareBtn.Text = "Compare";
            this.ToolTip.SetToolTip(this.CompareBtn, "Perform the same search on two spell files and compare the results. ");
            this.CompareBtn.UseVisualStyleBackColor = true;
            this.CompareBtn.Click += new System.EventHandler(this.CompareBtn_Click);
            // 
            // DisplayText
            // 
            this.DisplayText.AutoSize = true;
            this.DisplayText.Location = new System.Drawing.Point(12, 431);
            this.DisplayText.Name = "DisplayText";
            this.DisplayText.Size = new System.Drawing.Size(76, 24);
            this.DisplayText.TabIndex = 18;
            this.DisplayText.Text = "Details";
            this.DisplayText.UseVisualStyleBackColor = true;
            this.DisplayText.Click += new System.EventHandler(this.SearchText_TextChanged);
            // 
            // DisplayTable
            // 
            this.DisplayTable.AutoSize = true;
            this.DisplayTable.Checked = true;
            this.DisplayTable.Location = new System.Drawing.Point(12, 461);
            this.DisplayTable.Name = "DisplayTable";
            this.DisplayTable.Size = new System.Drawing.Size(67, 24);
            this.DisplayTable.TabIndex = 19;
            this.DisplayTable.TabStop = true;
            this.DisplayTable.Text = "Table";
            this.DisplayTable.UseVisualStyleBackColor = true;
            this.DisplayTable.Click += new System.EventHandler(this.SearchText_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(9, 395);
            this.label4.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.label4.Name = "label4";
            this.label4.Padding = new System.Windows.Forms.Padding(0, 10, 0, 0);
            this.label4.Size = new System.Drawing.Size(78, 30);
            this.label4.TabIndex = 17;
            this.label4.Text = "Display As";
            // 
            // SearchNotes
            // 
            this.SearchNotes.AutoSize = true;
            this.SearchNotes.Location = new System.Drawing.Point(108, 527);
            this.SearchNotes.Name = "SearchNotes";
            this.SearchNotes.Size = new System.Drawing.Size(18, 20);
            this.SearchNotes.TabIndex = 20;
            this.SearchNotes.Text = "...";
            // 
            // SearchEffect1
            // 
            this.SearchEffect1.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.SearchEffect1.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.SearchEffect1.FormattingEnabled = true;
            this.SearchEffect1.Location = new System.Drawing.Point(12, 178);
            this.SearchEffect1.Name = "SearchEffect1";
            this.SearchEffect1.Size = new System.Drawing.Size(141, 28);
            this.SearchEffect1.Sorted = true;
            this.SearchEffect1.TabIndex = 7;
            this.ToolTip.SetToolTip(this.SearchEffect1, "Select a spell effect type from the list, enter an SPA number, or type some text " +
        "that appears in the effect description.");
            this.SearchEffect1.TextChanged += new System.EventHandler(this.SearchText_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 145);
            this.label3.Name = "label3";
            this.label3.Padding = new System.Windows.Forms.Padding(0, 10, 0, 0);
            this.label3.Size = new System.Drawing.Size(76, 30);
            this.label3.TabIndex = 6;
            this.label3.Text = "Has Effect";
            // 
            // SearchBtn
            // 
            this.SearchBtn.Location = new System.Drawing.Point(11, 524);
            this.SearchBtn.Name = "SearchBtn";
            this.SearchBtn.Size = new System.Drawing.Size(91, 27);
            this.SearchBtn.TabIndex = 20;
            this.SearchBtn.Text = "Search";
            this.SearchBtn.UseVisualStyleBackColor = true;
            this.SearchBtn.Click += new System.EventHandler(this.SearchBtn_Click);
            // 
            // SearchClass
            // 
            this.SearchClass.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.SearchClass.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.SearchClass.FormattingEnabled = true;
            this.SearchClass.Location = new System.Drawing.Point(12, 108);
            this.SearchClass.Name = "SearchClass";
            this.SearchClass.Size = new System.Drawing.Size(141, 28);
            this.SearchClass.TabIndex = 3;
            this.SearchClass.TextChanged += new System.EventHandler(this.SearchClass_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 75);
            this.label2.Name = "label2";
            this.label2.Padding = new System.Windows.Forms.Padding(0, 10, 0, 0);
            this.label2.Size = new System.Drawing.Size(42, 30);
            this.label2.TabIndex = 2;
            this.label2.Text = "Class";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 8);
            this.label1.Name = "label1";
            this.label1.Padding = new System.Windows.Forms.Padding(0, 10, 0, 0);
            this.label1.Size = new System.Drawing.Size(74, 30);
            this.label1.TabIndex = 0;
            this.label1.Text = "Text or ID";
            // 
            // SearchText
            // 
            this.SearchText.Location = new System.Drawing.Point(12, 41);
            this.SearchText.Name = "SearchText";
            this.SearchText.Size = new System.Drawing.Size(222, 27);
            this.SearchText.TabIndex = 1;
            this.SearchText.TextChanged += new System.EventHandler(this.SearchText_TextChanged);
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
            // AutoSearch
            // 
            this.AutoSearch.Interval = 500;
            this.AutoSearch.Tick += new System.EventHandler(this.SearchBtn_Click);
            // 
            // MainForm
            // 
            this.AcceptButton = this.SearchBtn;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1008, 730);
            this.Controls.Add(this.SearchBrowser);
            this.Controls.Add(this.SearchFilters);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "EQ Spell Parser";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.SearchFilters.ResumeLayout(false);
            this.SearchFilters.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel SearchFilters;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox SearchText;
        private System.Windows.Forms.Button SearchBtn;
        private System.Windows.Forms.ComboBox SearchClass;
        private System.Windows.Forms.WebBrowser SearchBrowser;
        private System.Windows.Forms.ComboBox SearchEffect1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label SearchNotes;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.RadioButton DisplayText;
        private System.Windows.Forms.RadioButton DisplayTable;
        private System.Windows.Forms.Button CompareBtn;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox SearchLevel;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox SearchCategory;
        private System.Windows.Forms.Timer AutoSearch;
        private System.Windows.Forms.ToolTip ToolTip;
        private System.Windows.Forms.ComboBox SearchEffectSlot1;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.CheckBox ShowRelated;
        private System.Windows.Forms.ComboBox SearchEffect3;
        private System.Windows.Forms.ComboBox SearchEffect2;
        private System.Windows.Forms.ComboBox SearchEffectSlot3;
        private System.Windows.Forms.ComboBox SearchEffectSlot2;
        private System.Windows.Forms.LinkLabel ResetBtn;

    }
}

