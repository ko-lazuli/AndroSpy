using System;
using System.Drawing;
using System.IO;
using System.Media;
using System.Windows.Forms;

namespace SV
{
    public partial class Notification : Form
    {
        SoundPlayer sp = null;
        public Notification(string isim, string marka_model,string apiAndroidVersion, Image bayrak)
        {
            InitializeComponent();
            Screen ekran = Screen.FromPoint(Location);
            Location = new Point(ekran.WorkingArea.Right - Width, ekran.WorkingArea.Bottom - Height - MainWindow.topOf );
            label1.Text = isim; label2.Text = marka_model.ToUpper(); label4.Text = apiAndroidVersion;
            if (bayrak != null)
            {
                pictureBox1.Image = bayrak;
            }
            string[] lines = File.ReadAllLines("settings.tht");
            if (lines[lines.Length - 1] != "...")
            {
                sp = new SoundPlayer(lines[lines.Length - 1]);
                sp.Play();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if(sp != null)
            {
                sp.Stop();
                sp.Dispose();
            }
            MainWindow.topOf -= 125;
            Close();
        }
    }
}
