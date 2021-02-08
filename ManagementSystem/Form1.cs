using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.IO;

namespace ManagementSystemGUI
{
    public partial class Form1 : Form
    {
        private Dictionary<string,RadioButton> radios = new Dictionary<string,RadioButton>();
        private ManagementSystem managementSystem = new ManagementSystem();
        private Thread t;
        private EntrySorter EntrySorter;
       

        public Form1(string iconPath)
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            using(var stream = File.OpenRead(iconPath)) { this.Icon = new Icon(stream);}
            EntrySorter = new EntrySorter();
            this.EntriesList.ListViewItemSorter = EntrySorter;
        }

        public void AddLog(string log)
        {
            this.LogBox.Items.Add($"[{DateTime.Now.ToString("H:mm:ss:ff")}]; {log}");
        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            this.LogBox.Items.Clear();
        }

        public void UploadHostBoxes()
        {
            foreach (var key in ConfigMSystem.hosts.Keys)
            {
                this.SourceBox.Items.Add(key);
                this.DestinationBox.Items.Add(key);
            }
        }

        public void LoadRadioButtons()
        {
            radios.Add("R1", this.radioButton1);
            radios.Add("R2", this.radioButton2);
            radios.Add("R3", this.radioButton3);
            radios.Add("R4", this.radioButton4);
            radios.Add("R5", this.radioButton5);
        }

        public void EnableButtons(string ipAddres)
        {
            radios[ConfigMSystem.GetNodeName(IPAddress.Parse(ipAddres))].Enabled = true;
            this.ConfigButton.Enabled = true;
        }

        private void ConfigButton_Click(object sender, EventArgs e)
        {
            string properNode = null;
            bool HostChosen = DestinationBox.Text.Length != 0 || SourceBox.Text.Length != 0;
            bool AreEven = DestinationBox.Text.Equals(SourceBox.Text);

            foreach(var key in radios.Keys)
            {
                if (radios[key].Checked)
                {
                    properNode = key;
                    break;
                }      
            }
            if (properNode != null && HostChosen && !AreEven)
            {
                if (checkAgg.Checked)
                    managementSystem.Action(EncapsulateMessage() + " " + properNode + " " + PrevBox.Text);
                else
                    managementSystem.Action(EncapsulateMessage() + " " + properNode + " " + "-");
            }
            else
            {
                MessageBox.Show("Possible errors:" + Environment.NewLine + "- None of routers is checked" + Environment.NewLine +
                    "- Hosts are not selected or they are equal", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        public string EncapsulateMessage() {

            StringBuilder packet = new StringBuilder();
            packet.Append(ManagementActions.ADD_MPLS_ENTRY + " ");
            packet.Append(ConfigMSystem.hosts[DestinationBox.Text].ToString() + " ");
            packet.Append(LINtext.Text + " ");
            packet.Append(LOUTtext.Text + " ");
            packet.Append(POUTtext.Text + " ");
            packet.Append(PINtext.Text);
            return packet.ToString();
        }


        public void ShowDefaultEntries(List<string> entries)
        {

            EntriesList.View = View.Details;
            EntriesList.GridLines = true;
            EntriesList.FullRowSelect = true;
            EntriesList.Columns.Add("Name", 100);
            EntriesList.Columns.Add("Port_IN", 160);
            EntriesList.Columns.Add("Label_IN", 160);
            EntriesList.Columns.Add("Port_OUT", 160);
            EntriesList.Columns.Add("Label_OUT", 160);
            EntriesList.Columns.Add("Prefix", 160);
            EntriesList.Columns.Add("Index", 80);
            EntriesList.Columns.Add("Prev_index", 80);

        }

        public void DisableButton(string ipAddress)
        {
            radios[ConfigMSystem.GetNodeName(IPAddress.Parse(ipAddress))].Enabled = false;
            radios[ConfigMSystem.GetNodeName(IPAddress.Parse(ipAddress))].Checked = false;

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ShowDefaultEntries(ConfigMSystem.DefaultRoutes);
            UploadHostBoxes();
            LoadRadioButtons();
            t = new Thread(() => managementSystem.Start(this));
            t.IsBackground = true;
            t.Start();
        }

        //Disabling a opportunity to resize entries list
        private void EntriesList_ColumnWidthChanging(object sender, ColumnWidthChangingEventArgs e)
        {
            e.Cancel = true;
            e.NewWidth = EntriesList.Columns[e.ColumnIndex].Width;
        }
        
        public void UploadEntriesList(string packet)
        {
            int i = 0;
            string[] parts = packet.Split(" ");
            ListViewItem item = new ListViewItem(new[] { parts[parts.Length - 3], parts[5], parts[2], parts[4], parts[3], parts[1]+"/24", parts[7], parts[8] });
            EntriesList.Items.Add(item);
            foreach (ListViewItem entry in EntriesList.Items)
            {
                entry.BackColor = i % 2 == 0 ? Color.LightSkyBlue : EntriesList.BackColor;
                i++;
            }
        }

        //Sorting a NHLFE table by clicking a column
        private void EntriesList_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            int i = 0;
            if(e.Column == 0)
            {
                if (e.Column == EntrySorter.SortColumn )
                {
                    if (EntrySorter.Order == SortOrder.Ascending)
                    {
                        EntrySorter.Order = SortOrder.Descending;
                    }
                    else
                    {
                        EntrySorter.Order = SortOrder.Ascending;
                    }
                }
                else
                {
                    EntrySorter.SortColumn = e.Column;
                    EntrySorter.Order = SortOrder.Ascending;
                }
                this.EntriesList.Sort();
                foreach (ListViewItem entry in EntriesList.Items)
                {
                    entry.BackColor = i % 2 == 0 ? Color.LightSkyBlue : EntriesList.BackColor;
                    i++;
                }
            }
        }

        private void checkAgg_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkAgg.Checked)
            {
                this.PrevBox.Enabled = true;
            }
            else
            {
                this.PrevBox.Clear();
                this.PrevBox.Enabled = false;
            }
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            ListViewItem x = null;
            StringBuilder sb = new StringBuilder();
            foreach(ListViewItem item in EntriesList.Items)
            {
                if (item.Selected)
                {
                    x = item;
                    break;
                }
            }
            if (x != null)
            {
                for (int i = 0; i < EntriesList.Columns.Count; i++)
                {
                    sb.Append(x.SubItems[i].Text + " ");
                }
                EntriesList.Items.Remove(x);
                string[] s = sb.ToString().Split(" ");
                DeleteEntry(ManagementActions.REMOVE_MPLS_ENTRY + " " + s[5].Replace("/24","") + " " + s[2] + " " + s[4] + " " + s[3] + " " + s[1] + " " + s[0] + " " + s[6] + " " + s[7]);
            }
            else
            {
                MessageBox.Show("Entry's not selected!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void DeleteEntry(string message)
        {
            managementSystem.Action(message);
        }
    }




}
