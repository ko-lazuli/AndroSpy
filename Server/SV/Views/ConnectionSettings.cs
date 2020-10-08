using System;
using System.Net.Sockets;
using System.Windows.Forms;

namespace Server.Views
{
    public partial class Connection : Form
    {
        public string ID = ""; Socket socket;
        public Connection(Socket sck, string aydi)
        {
            InitializeComponent();
            ID = aydi;
            socket = sck;
        }
        bool isNotEmpty = false;
        private void button1_Click(object sender, EventArgs e)
        {
            isNotEmpty = textBox1.Text != "" && textBox2.Text != "";
            if (isNotEmpty)
            {
                try
                {
                    MainWindow.KomutGonder("UPDATE", "[VERI]" + textBox1.Text + "[VERI]" + textBox2.Text + "[VERI]"
                        + numericUpDown1.Value.ToString() + "[VERI][0x09]", socket);
                    Close();
                }
                catch (Exception) { }
            }
        }
    }
}
