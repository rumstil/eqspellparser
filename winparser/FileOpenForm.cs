using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using EQSpellParser;


namespace winparser
{
    public partial class FileOpenForm : Form
    {
        public FileOpenForm()
        {
            InitializeComponent();

            var servers = ConfigurationManager.AppSettings["AltServers"];
            if (servers != null)
            {
                Button clone = DownloadBtn;
                var list = servers.Replace(" ", "").Split(',');
                foreach (var s in list)
                {
                    var button = new Button();
                    button.Size = clone.Size;
                    button.Left = clone.Right + clone.Margin.Left;
                    button.Top = clone.Top;
                    button.AccessibleName = s;
                    button.Click += DownloadLive_Click;
                    button.Text = "Download " + s;
                    panel1.Controls.Add(button);
                    clone = button;
                }
            }
        }

        private void FindFiles()
        {
            listView1.Items.Clear();

            var dir = new DirectoryInfo(".");
            var files = dir.GetFiles("spells_us*.txt*");
            ListViewItem item = null;
            foreach (var f in files)
            {
                // ignore the spell_us_str file since it also matches
                if (f.Name.StartsWith("spells_us_str"))
                    continue;
                item = new ListViewItem(f.Name);
                item.SubItems.Add(f.Length.ToString("#,#"));
                item.SubItems.Add(SpellParser.CountFields(f.Name).ToString());
                listView1.Items.Add(item);
            }

            if (listView1.Items.Count > 0)
                listView1.Items[0].Selected = true;
            else
                Status.Text = "spells_us.txt was not found. Use the download button or copy a file into " + Directory.GetCurrentDirectory();
        }

        /// <summary>
        /// Load a spell file into a new window.
        /// </summary>
        private void Open(string spellPath)
        {
            MainForm other = Application.OpenForms.OfType<MainForm>().FirstOrDefault();

            if (other != null && other.SpellPath == spellPath)
            {
                Status.Text = spellPath + " has already been loaded.";
                return;
            }

            var f = new MainForm();
            f.Load(spellPath);
            f.Show();

            // DoEvents is a bit naughty because we could reenter this method on a double click, but it gives the browser a chance to init
            Application.DoEvents(); 

            // if a form is already loaded then assume we are going to be comparing right away
            if (other != null)
            {
                other.BringToFront();
                other.Compare(f);
            }

            Hide();
        }

        /// <summary>
        /// Download an updated spell file from the patch server. File names will include date (from server timestamp) and patch server type.
        /// </summary>
        private void Download(string server)
        {
            Cursor.Current = Cursors.WaitCursor;
            var path = LaunchpadPatcher.DownloadSpellFilesWithVersioning(server);
            Cursor.Current = Cursors.Default;
          
            Status.Text = String.Format("Downloaded {0}", path);

            var item = listView1.FindItemWithText(path);
            if (item == null)
            {
                var info = new FileInfo(path);
                item = new ListViewItem(path);
                item.SubItems.Add(info.Length.ToString("#,#"));
                item.SubItems.Add(SpellParser.CountFields(path).ToString());
                listView1.Items.Add(item);                
            }
            listView1.MultiSelect = false;
            item.EnsureVisible();
            item.Selected = true;
            listView1.MultiSelect = true;
        }

        public void SetStatus(string text)
        {
            Status.Text = text;
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            listView1.Enabled = false;
            if (listView1.FocusedItem != null)
                Open(listView1.FocusedItem.Text);
            listView1.Enabled = true;
        }

        private void OpenBtn_Click(object sender, EventArgs e)
        {
            listView1.Enabled = false;
            for (int i = 0; i < listView1.SelectedItems.Count; i++)
            { 
                Open(listView1.SelectedItems[i].Text);
            }
            listView1.Enabled = true;
        }

        private void DownloadLive_Click(object sender, EventArgs e)
        {
            Download((sender as Button).AccessibleName);
        }

        private void FileOpenForm_Load(object sender, EventArgs e)
        {
            FindFiles();
        }

        private void FileOpenForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Application.OpenForms.Count > 1)
            {
                Hide();
                e.Cancel = true;
            }
        }



    }
}
