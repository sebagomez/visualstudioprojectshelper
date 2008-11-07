using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Diagnostics;

namespace VSRecentProjectsHelper
{
    public partial class Form1 : Form
    {
        RegistryKey m_location;
        string[] m_list;
        int m_count = 0;
        bool m_reload = false;
        Dictionary<string,string> m_projects;
        bool m_formLoaded = false;

        ListViewColumnSorter m_columnSorter;

        public Form1()
        {
                InitializeComponent();

                m_columnSorter = new ListViewColumnSorter();
                this.listView1.ListViewItemSorter = m_columnSorter;


                cmbVS.DataSource = VSVersion.GetVersions();

                LoadFromRegistry();
        }

        private void LoadFromRegistry()
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                m_projects = new Dictionary<string, string>();

                RegistryKey reg = Registry.CurrentUser;
                string subkey = "SOFTWARE\\Microsoft\\VisualStudio\\" + cmbVS.SelectedValue + "\\ProjectMRUList";

                m_location = reg.OpenSubKey(subkey, true);

                if (m_location == null)
                    throw new ApplicationException(string.Format("You don't have Visual Studio {0} installed", ((VSVersion)cmbVS.SelectedItem).Name));

                m_list = m_location.GetValueNames();

                
                foreach (string prog in m_list)
                {
                    if (m_location.GetValue(prog) == null || string.IsNullOrEmpty(m_location.GetValue(prog).ToString()))
                        continue;

                    m_projects.Add(prog, m_location.GetValue(prog).ToString());
                }

                m_reload = false;
            }
            catch (Exception ex)
            {
                AppHelper.MessageHandler.ShowException(ex);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadProjects("");
            m_formLoaded = true;
        }

        private void LoadProjects(string matchingText)
        {
            try
            {
                if (m_reload)
                    LoadFromRegistry();

                m_count = 0;
                foreach (string entry in m_projects.Keys)
                {
                    if (!string.IsNullOrEmpty(matchingText))
                    {
                        string[] strings = matchingText.Split(' ');

                        bool matches = true;
                        for (int i = 0; i < strings.Length; i++)
                        {
                            if (!m_projects[entry].ToLower().Contains(strings[i].ToLower()))
                            {
                                matches = false;
                                break;
                            }
                        }

                        if (!matches)
                            continue;
                    }

                    ProjectInfo proj = ProjectInfo.CreateProjectInfo(m_projects[entry], entry);
                    ListViewItem item = new ListViewItem();
                    item.Name = proj.FullPath;
                    item.Tag = proj;
                    item.Text = proj.Name;
                    item.SubItems.Add(proj.Extension);
                    item.SubItems.Add(proj.FullPath);
                    item.ImageKey = string.Format("{0}.ico", proj.Extension);

                    listView1.Items.Add(item);

                    m_count++;
                }
            }
            catch (Exception ex)
            {
                AppHelper.MessageHandler.ShowException(ex);
                this.Close();
            }
            status.Text = string.Format("{0} projects found", m_count);
        }

        private void ClearChecked()
        {
            iconsToolStripMenuItem.Checked = false;
            tilesToolStripMenuItem.Checked = false;
            smallIconsToolStripMenuItem.Checked = false;
            listToolStripMenuItem.Checked = false;
            detailsToolStripMenuItem.Checked = false;
        }

        private void detailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClearChecked();
            listView1.View = View.Details;
        }

        private void tilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClearChecked();
            listView1.View = View.Tile;
        }

        private void listToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClearChecked();
            listView1.View = View.List;
        }

        private void smallIconsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClearChecked();
            listView1.View = View.SmallIcon;
        }

        private void iconsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClearChecked();
            listView1.View = View.LargeIcon;
        }

        private void toolStripButton1_ButtonClick(object sender, EventArgs e)
        {
            switch (listView1.View)
            {
                case View.Details:
                    tilesToolStripMenuItem_Click(sender, e);
                    break;
                case View.LargeIcon:
                    detailsToolStripMenuItem_Click(sender, e);
                    break;
                case View.List:
                    smallIconsToolStripMenuItem_Click(sender, e);
                    break;
                case View.SmallIcon:
                    iconsToolStripMenuItem_Click(sender, e);
                    break;
                case View.Tile:
                    listToolStripMenuItem_Click(sender, e);
                    break;
                default:
                    detailsToolStripMenuItem_Click(sender, e);
                    break;
            }
        }

        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {

            if (e.Column == m_columnSorter.SortColumn)
            {
                if (m_columnSorter.Order == SortOrder.Ascending)
                    m_columnSorter.Order = SortOrder.Descending;
                else
                    m_columnSorter.Order = SortOrder.Ascending;
            }
            else
            {
                m_columnSorter.SortColumn = e.Column;
                m_columnSorter.Order = SortOrder.Ascending;
            }

            listView1.Sort();
        }

        private ProjectInfo SelectedProject()
        {
            ListViewItem selected = listView1.SelectedItems[0];
            ProjectInfo proj = (ProjectInfo)selected.Tag;

            return proj;
        }

        private void mnuLaunch_Click(object sender, EventArgs e)
        {
            string proccessName = SelectedProject().FullPath;
            try
            {
                Process.Start(proccessName);
            }
            catch (Win32Exception)
            {
                if (AppHelper.MessageHandler.AskQuestion(string.Format("{0} {1}is no longer available, do you wish to remove it from the list?", proccessName,System.Environment.NewLine)))
                    mnuRemove_Click(sender, e);
            }
        }

        private void mnuOpenFolder_Click(object sender, EventArgs e)
        {
            string proccessName = SelectedProject().Folder;
            try
            {
                Process.Start(proccessName);
            }
            catch (Win32Exception)
            {
                if (AppHelper.MessageHandler.AskQuestion(string.Format("{0} {1}is no longer available, do you wish to remove it from the list?", proccessName, System.Environment.NewLine)))
                    mnuRemove_Click(sender,e);
            }

        }

        private void mnuRemove_Click(object sender, EventArgs e)
        {
            try
            {
                ProjectInfo proj = SelectedProject();
                MoveEmUp(proj.Entry);
            }
            catch (Exception ex)
            {
                AppHelper.MessageHandler.ShowException(ex);
                return;
            }
            m_reload = true;
            SomethingChanged();
        }

        private void MoveEmUp(string entry)
        {
            int i = int.Parse(entry.Remove(0, 4)); //The 'File' word
            string newKey = "File" + (i + 1).ToString();
            string prevKey = entry;

            while (m_location.GetValue(newKey) != null)
            {
                string val = m_location.GetValue(newKey).ToString();
                m_location.SetValue(prevKey, val);

                i++;
                prevKey = newKey;
                newKey = "File" + (i + 1).ToString();
            }

            m_location.DeleteValue(prevKey);
        }

        private void mnuContext_Opening(object sender, CancelEventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
            {
                e.Cancel = true;
                return;
            }

            string entry = SelectedProject().Entry;
            mnuUp.Enabled = (entry != "File1");

            int i = int.Parse(entry.Remove(0, 4)); //The 'File' word
            i++;

            mnuDown.Enabled = (m_location.GetValue(string.Format("File{0}", i)) != null);

        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            SomethingChanged();
        }

        private void SomethingChanged()
        {
            listView1.Items.Clear();
            LoadProjects(txtSearch.Text);
        }

        private void cmbVS_SelectedValueChanged(object sender, EventArgs e)
        {
            if (m_formLoaded)
            {
                m_reload = true;
                SomethingChanged();
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (m_location != null)
                m_location.Close();
        }

        private void mnuUp_Click(object sender, EventArgs e)
        {
            ProjectInfo proj = SelectedProject();
            try
            {
                ScootOver(proj.Entry,-1);
            }
            catch (Exception ex)
            {
                AppHelper.MessageHandler.ShowException(ex);
                return;
            }
            m_reload = true;
            SomethingChanged();

            listView1.Items[proj.FullPath].Selected = true;
        }

        private void ScootOver(string entry, int seed)
        {
            int i = int.Parse(entry.Remove(0, 4)); //The 'File' word
            string newKey = "File" + (i + seed).ToString();
            string prevKey = entry;

            if (m_location.GetValue(newKey) != null && m_location.GetValue(prevKey) != null)
            {
                string val = m_location.GetValue(newKey).ToString();
                string val1 = m_location.GetValue(prevKey).ToString();

                m_location.SetValue(prevKey, val);
                m_location.SetValue(newKey, val1);
            }
        }

        private void mnuDown_Click(object sender, EventArgs e)
        {
            ProjectInfo proj = SelectedProject();
            try
            {
                ScootOver(proj.Entry, 1);
            }
            catch (Exception ex)
            {
                AppHelper.MessageHandler.ShowException(ex);
                return;
            }
            m_reload = true;
            SomethingChanged();

            listView1.Items[proj.FullPath].Selected = true;
        }
    }
}
