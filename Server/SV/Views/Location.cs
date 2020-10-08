using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Windows.Forms;

namespace Server.Views
{
    public partial class Location : Form
    {
        Socket sck; public string ID = "";
        public Location(Socket soket, string aydi)
        {
            InitializeComponent();
            sck = soket; ID = aydi;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                MainWindow.GoToOurSocket("KONUM", "[VERI][0x09]", sck);
            }
            catch (Exception) { }
        }

        private void richTextBox1_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            try { Process.Start(e.LinkText); } catch (Exception) { }
        }
    }
}
