﻿using System;
using System.Net.Sockets;
using System.Windows.Forms;

namespace Server.Views
{
    public partial class AddContact : Form
    {
        Socket sckt;
        public AddContact(Socket sck)
        {
            InitializeComponent();
            sckt = sck;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                MainWindow.KomutGonder("REHBERISIM", "[VERI]" + textBox1.Text + "=" + textBox2.Text + "=[VERI][0x09]", sckt);
            }
            catch (Exception) { }
            Close();
        }
    }
}
