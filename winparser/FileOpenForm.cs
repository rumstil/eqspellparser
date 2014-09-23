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
using Everquest;

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
                foreach (var s in servers.Split(','))
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
            var files = dir.GetFiles("spells_us*.txt");
            ListViewItem item = null;
            foreach (var f in files)
            {
                item = new ListViewItem(f.Name);
                item.SubItems.Add(f.Length.ToString());
                item.SubItems.Add(CountFields(f.Name).ToString());
                listView1.Items.Add(item);
            }

            if (item == null)
                Status.Text = "spells_us.txt was not found. Use the download button or copy a file into " + Directory.GetCurrentDirectory();
            else
            {
                item.EnsureVisible();
                item.Selected = true;
            }
        }

        /// <summary>
        /// Open a spell file in the parser.
        /// </summary>
        private void Open(string spellPath)
        {
            var f = new MainForm();
            f.Load(spellPath, spellPath.Replace("spells_us", "dbstr_us"));
            f.Show();
        }

        /// <summary>
        /// Download an updated spell file from the patch server.
        /// </summary>
        private void Download(string server)
        {
            Cursor.Current = Cursors.WaitCursor;

            var files = LaunchpadPatcher.DownloadManifest(server);

            var spell = files["spells_us.txt"];
            spell.Name = spell.Name.Replace(".txt", "-" + spell.LastModified.ToString("yyyy-MM-dd") + server + ".txt");
            LaunchpadPatcher.DownloadFile(spell.Url, spell.Name);

            // the desc file can sometimes be older than the spell file. we need to save it with the spell file timestamp 
            // so that there is always a corresponding copy
            var desc = files["dbstr_us.txt"];
            desc.Name = desc.Name.Replace(".txt", "-" + spell.LastModified.ToString("yyyy-MM-dd") + server + ".txt");
            LaunchpadPatcher.DownloadFile(desc.Url, desc.Name);

            Cursor.Current = Cursors.Default;

            Status.Text = String.Format("Downloaded {0} {1}", spell.Name, spell.LastModified.ToString());

            var item = listView1.FindItemWithText(spell.Name);
            if (item == null)
            {
                item = new ListViewItem(spell.Name);
                item.SubItems.Add(spell.UncompressedSize.ToString());
                item.SubItems.Add(CountFields(spell.Name).ToString());
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

        private int CountFields(string path)
        {
            var f = File.OpenText(path);
            return f.ReadLine().Split('^').Length;
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            if (listView1.FocusedItem != null)
            {
                Hide();
                Open(listView1.FocusedItem.Text);
                
            }
        }

        private void OpenBtn_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < listView1.SelectedItems.Count; i++)
            {
                Open(listView1.SelectedItems[i].Text);
                Hide();
            }
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
