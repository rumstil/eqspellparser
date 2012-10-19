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

            var dir = new DirectoryInfo(".");
            var files = dir.GetFiles("spells_us*.txt");
            foreach (var f in files)
            {
                var item = new ListViewItem(f.Name);
                item.SubItems.Add(f.Length.ToString());
                listView1.Items.Add(item);
                listView1.FocusedItem = item;
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

            var desc = files["dbstr_us.txt"];
            desc.Name = desc.Name.Replace(".txt", "-" + desc.LastModified.ToString("yyyy-MM-dd") + server + ".txt");
            LaunchpadPatcher.DownloadFile(desc.Url, desc.Name);

            Cursor.Current = Cursors.Default;

            if (listView1.FindItemWithText(spell.Name) == null)
            {
                var item = new ListViewItem(spell.Name);
                item.SubItems.Add(spell.UncompressedSize.ToString());
                listView1.Items.Add(item);
                //listView1.FocusedItem = item;
            }

            //Open(spell.Name);
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            if (listView1.FocusedItem != null)
                Open(listView1.FocusedItem.Text);
            Hide();
        }

        private void OpenBtn_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < listView1.SelectedItems.Count; i++)
                Open(listView1.SelectedItems[i].Text);
            Hide();
        }

        private void DownloadLive_Click(object sender, EventArgs e)
        {
            Download((sender as Button).AccessibleName);
        }



    }
}
