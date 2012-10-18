using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Everquest;

namespace winparser
{
    public partial class FileOpenForm : Form
    {
        public FileOpenForm()
        {
            InitializeComponent();

            //var files = Directory.GetFiles("spells_us*.txt");
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

        private void Open(string spellPath)
        {
            Hide();

            var f = new MainForm();
            f.Load(spellPath, spellPath.Replace("spells_us", "dbstr_us"));
            f.Show();
        }

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

            Open(spell.Name);
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            if (listView1.FocusedItem != null)
                Open(listView1.FocusedItem.Text);
        }

        private void DownloadLive_Click(object sender, EventArgs e)
        {
            Download(null);
        }

        private void DownloadTest_Click(object sender, EventArgs e)
        {
            Download("-test");
        }

    }
}
