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
                item.SubItems.Add(f.Length.ToString("#,#"));
                item.SubItems.Add(CountFields(f.Name).ToString());
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

            LaunchpadPatcher.DownloadManifest(server, "manifest.dat");
            var manifest = new LaunchpadManifest(new MemoryStream(File.ReadAllBytes("manifest.dat")));

            var spell = manifest.FindFile(LaunchpadManifest.SPELL_FILE);
            spell.Name = spell.Name.Replace(".txt", "-" + spell.LastModified.ToString("yyyy-MM-dd") + server + ".txt");
            LaunchpadPatcher.DownloadFile(spell.Url, spell.Name);

            // the desc file can sometimes be older than the spell file. we need to save it with the spell file timestamp 
            // so that there is always a corresponding copy
            var desc = manifest.FindFile(LaunchpadManifest.SPELLDESC_FILE);
            desc.Name = desc.Name.Replace(".txt", "-" + spell.LastModified.ToString("yyyy-MM-dd") + server + ".txt");
            LaunchpadPatcher.DownloadFile(desc.Url, desc.Name);

            // same for the stacking file
            var stack = manifest.FindFile(LaunchpadManifest.SPELLSTACK_FILE);
            stack.Name = stack.Name.Replace(".txt", "-" + spell.LastModified.ToString("yyyy-MM-dd") + server + ".txt");
            LaunchpadPatcher.DownloadFile(stack.Url, stack.Name);
            
            Status.Text = String.Format("Downloaded {0} {1}", spell.Name, spell.LastModified.ToString());

            var item = listView1.FindItemWithText(spell.Name);
            if (item == null)
            {
                item = new ListViewItem(spell.Name);
                item.SubItems.Add(spell.UncompressedSize.ToString("#,#"));
                item.SubItems.Add(CountFields(spell.Name).ToString());
                listView1.Items.Add(item);                
            }
            listView1.MultiSelect = false;
            item.EnsureVisible();
            item.Selected = true;
            listView1.MultiSelect = true;

            Cursor.Current = Cursors.Default;
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
