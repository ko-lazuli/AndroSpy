﻿using System.Windows.Forms;

namespace SV
{
    public partial class ViewSms : Form
    {
        public ViewSms(string baslik, string icerik)
        {
            InitializeComponent();
            Text = "You are reading: " + baslik;
            richTextBox1.Text = icerik;
        }
    }
}
