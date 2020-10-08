using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Server.Views
{
    public partial class VictimSettings : Form
    {
        public VictimSettings()
        {
            InitializeComponent();
            checkBox1.Checked = lines[lines.Length - 1] != "..." ? true : false;         
        }
        string[] lines = File.ReadAllLines("settings.tht");
        private void button1_Click(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                using (OpenFileDialog op = new OpenFileDialog()
                {
                    Multiselect = false,
                    Filter = "Ses dalgası (*.wav)|*.wav",
                    Title = "Yeni bağlantı için bir ses dalgası dosyası seçiniz.."
                })
                {
                    lines[lines.Length - 1] = op.ShowDialog() == DialogResult.OK ? op.FileName : "...";
                    File.WriteAllLines("settings.tht", lines);
                    Close();
                }
            }
            else
            {
                lines[lines.Length - 1] = "...";
                Close();
            }
        }      
    }
}
