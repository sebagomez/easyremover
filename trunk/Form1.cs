using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using System.IO;
using System.Globalization;
using System.Diagnostics;
using Program_Finder.Properties;
using AppHelper;

namespace Program_Finder
{
    public partial class Form1 : Form
    {
        //RegistryKey m_location;
        string[] m_list;
        int m_count = 0;
        Entry m_entry;
        bool m_reload = false;
        Dictionary<string, Entry> m_entries;
        Dictionary<string, RegistryKey> m_locations;

        public Form1()
        {
            InitializeComponent();


            listView1.SmallImageList = new ImageList();
            listView1.SmallImageList.ImageSize = new Size(16, 16);
            listView1.SmallImageList.TransparentColor = Color.White;
            listView1.SmallImageList.ColorDepth = ColorDepth.Depth32Bit;

            listView1.LargeImageList = new ImageList();
            listView1.LargeImageList.ImageSize = new Size(32, 32);
            listView1.LargeImageList.TransparentColor = Color.White;
            listView1.LargeImageList.ColorDepth = ColorDepth.Depth32Bit;

            LoadFromRegistry();
        }

        private void LoadFromRegistry()
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                m_count = 0;
                m_entries = new Dictionary<string, Entry>();

                RegistryKey reg = Registry.LocalMachine;
                string subkey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall";

                LoadFromKey(reg.OpenSubKey(subkey), "");
                LoadUserSpecific();

                m_reload = false;
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void LoadUserSpecific()
        {
            RegistryKey reg = Registry.Users;
            string[] users = reg.GetSubKeyNames();

            foreach (string user in users)
            {
                string subkey = user + "\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall";
                RegistryKey userKey = reg.OpenSubKey(subkey, true);

                if (userKey == null)
                    continue;

                LoadFromKey(userKey,user);
            }
        }

        private void LoadFromKey(RegistryKey key, string user)
        {
            m_list = key.GetSubKeyNames();

            foreach (string prog in m_list)
            {
                RegistryKey local = key.OpenSubKey(prog);

                if (local.GetValue("SystemComponent") != null)
                    continue;

                if (local.GetValue("IsMinorUpgrade") != null)
                    continue;

                if (local.GetValue("DisplayName") == null || string.IsNullOrEmpty(local.GetValue("DisplayName").ToString()))
                    continue;

                Entry entry = LoadEntry(key,prog);
                entry.RegistryUser = user;
                m_entries.Add(prog, entry);
                listView1.SmallImageList.Images.Add(entry.Name, entry.SmallIcon);
                listView1.LargeImageList.Images.Add(entry.Name, entry.LargeIcon);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadPrograms("");
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            SomethingChanged();
        }

        private void SomethingChanged()
        {
            listView1.Items.Clear();
            LoadPrograms(txtSearch.Text);
            listView1.Sort();

            ClearDescription();
        }

        private void ClearDescription()
        {
            lblDescription.Text = "";
            lblVersion.Text = "";
            lblPublisher.Text = "";
            lblInstallation.Text = "";
            pbIcon.Image = null;
            lnkHelp.Visible = false;
            lnkHelp.LinkVisited = false;

            btnUninstall.Enabled = false;
            m_entry = null;
        }

        private void LoadPrograms(string matchingText)
        {
            if (m_reload)
                LoadFromRegistry();

            m_count = 0;
            foreach (Entry entry in m_entries.Values)
            {
                if (entry.IsUpdate && !chkUpdates.Checked)
                    continue;

                if (!string.IsNullOrEmpty(matchingText))
                {
                    string[] strings = matchingText.Split(' ');

                    bool matches = true;
                    for (int i = 0; i < strings.Length; i++)
                    {
                        if (!entry.Description.ToLower().Contains(strings[i].ToLower()))
                        {
                            matches = false;
                            break;
                        }
                    }

                    if (!matches)
                        continue;
                }

                ListViewItem item = new ListViewItem();
                item.Tag = entry.Name;
                item.Text = entry.Description;
                item.ImageKey = entry.Name;
                item.SubItems.Add(entry.Publisher);

                listView1.Items.Add(item);


                m_count++;
            }
            status.Text = string.Format("Programs found: {0}", m_count);
        }

        private void listView1_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            ShowEntry(m_entries[e.Item.Tag.ToString()]);
        }

        private Entry LoadEntry(RegistryKey key, string entryKey)
        {
            RegistryKey local = key.OpenSubKey(entryKey);
            Entry entry = new Entry(entryKey);

            if (local.GetValue("DisplayName") != null)
                entry.Description = local.GetValue("DisplayName").ToString();

            if (local.GetValue("Publisher") != null)
                entry.Publisher = local.GetValue("Publisher").ToString();

            if (local.GetValue("DisplayVersion") != null)
                entry.Version = local.GetValue("DisplayVersion").ToString();

            if (local.GetValue("InstallSource") != null)
                entry.Source = local.GetValue("InstallSource").ToString();

            if (local.GetValue("UninstallString") != null)
                entry.Unisntall = local.GetValue("UninstallString").ToString();

            if (local.GetValue("InstallDate") != null)
                entry.InstallationDate = DateHelper.TimeFromString(local.GetValue("InstallDate").ToString());

            if (local.GetValue("ParentKeyName") != null)
                entry.IsUpdate = true;

            if (local.GetValue("HelpLink") != null)
                entry.Help = local.GetValue("HelpLink").ToString();

            if (local.GetValue("DisplayIcon") != null)
            {
                Icon[] icons = IconHelper.GetIcons(local.GetValue("DisplayIcon").ToString());

                if (icons[0] != null)
                    entry.LargeIcon = icons[0];
                else
                    entry.LargeIcon = Resources._default;

                if (icons[1] != null)
                    entry.SmallIcon = icons[1];
                else
                    entry.SmallIcon = Resources._default;
            }
            else
            {
                entry.LargeIcon = Resources._default;
                entry.SmallIcon = Resources._default;
            }

            return entry;

        }

        private void ShowEntry(Entry entry)
        {
            lblDescription.Text = entry.Description;
            lblVersion.Text = entry.Version;
            lblPublisher.Text = entry.Publisher;

            if (entry.InstallationDate != DateTime.MinValue)
                lblInstallation.Text = entry.InstallationDate.ToShortDateString();
            else
                lblInstallation.Text = "N/A";

            pbIcon.Image = listView1.LargeImageList.Images[entry.Name];
            if (!string.IsNullOrEmpty(entry.Help))
            {
                lnkHelp.LinkVisited = false;
                lnkHelp.Visible = true;
                lnkHelp.Click += new EventHandler(lnkHelp_Click);
            }
            else 
                lnkHelp.Visible = false;

            btnUninstall.Enabled = true;
            m_entry = entry;
        }

        void lnkHelp_Click(object sender, EventArgs e)
        {
            lnkHelp.LinkVisited = true;
            Process.Start(m_entry.Help);
        }

        private void btnUninstall_Click(object sender, EventArgs e)
        {
            Unisntall();
        }

        private void Unisntall()
        {
            if (m_entry == null)
            {
                MessageHandler.ShowWarning("No program selected.");
                return;
            }

            try
            {
                Process.Start(GetProcessInfo(m_entry.Unisntall));
            }
            catch (Exception ex)
            {
                MessageHandler.ShowException(string.Format("Could not start {0}", m_entry.Unisntall), ex);
            }
            finally
            {
                m_reload = true;
            }
        }

        private ProcessStartInfo GetProcessInfo(string uninstallString)
        {
            ProcessStartInfo info = new ProcessStartInfo();
            uninstallString = uninstallString.Replace("\"", "").ToLower();

            if (uninstallString.EndsWith(".exe"))
                info.FileName = uninstallString;
            else
            {
                string command = uninstallString.Substring(0, uninstallString.IndexOf(".exe") + 4);
                string argument = uninstallString.Substring(uninstallString.IndexOf(".exe") + 5);

                info.FileName = command;
                info.Arguments = argument;
            }
            return info;
        }

        private void chkUpdates_CheckStateChanged(object sender, EventArgs e)
        {
            SomethingChanged();
        }

        private void unisntallToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Unisntall();
        }

        private void tilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClearChecked();
            listView1.View = View.Tile;
            tilesToolStripMenuItem.Checked = true;

        }

        private void listToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClearChecked();
            listView1.View = View.List;
            listToolStripMenuItem.Checked = true;
        }

        private void smallIconsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClearChecked();
            listView1.View = View.SmallIcon;
            smallIconsToolStripMenuItem.Checked = true;
        }

        private void iconsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClearChecked();
            listView1.View = View.LargeIcon;
            iconsToolStripMenuItem.Checked = true;
        }

        private void toolStripButton1_ButtonClick(object sender, EventArgs e)
        {
            ClearChecked();

            switch (listView1.View)
            {
                case View.List:
                    listView1.View = View.SmallIcon;
                    smallIconsToolStripMenuItem.Checked = true;
                    break;
                case View.LargeIcon:
                    listView1.View = View.Details;
                    detailsToolStripMenuItem.Checked = true;
                    break;
                case View.Tile:
                    listView1.View = View.List;
                    listToolStripMenuItem.Checked = true;
                    break;
                case View.SmallIcon:
                    listView1.View = View.LargeIcon;
                    iconsToolStripMenuItem.Checked = true;
                    break;
                case View.Details:
                    listView1.View = View.Tile;
                    tilesToolStripMenuItem.Checked = true;
                    break;
                default:
                    listView1.View = View.Details;
                    listToolStripMenuItem.Checked = true;
                    break;
            }
        }

        private void ClearChecked()
        {
            iconsToolStripMenuItem.Checked = false;
            tilesToolStripMenuItem.Checked = false;
            smallIconsToolStripMenuItem.Checked = false;
            listToolStripMenuItem.Checked = false;
            detailsToolStripMenuItem.Checked = false;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void detailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClearChecked();
            listView1.View = View.Details;
            detailsToolStripMenuItem.Checked = true;
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
                e.Cancel = true;

        }
    }
}
