using System.Windows.Forms;

namespace Server.Views
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
