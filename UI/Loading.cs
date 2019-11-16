using System;
using System.Drawing;
using System.Windows.Forms;
using MetroFramework.Forms;
using BotFramework;
using CommonMark;

namespace UI
{
    public partial class Login : MetroForm
    {
        public static bool LoadCompleted = false;

        private static int autoClose;
        public Login()
        {
            InitializeComponent();
            timer1.Start();
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            if(CheckVersion.UpdateText != null && CheckVersion.UpdateText != "")
            {
                if (pictureBox1.Visible)
                {
                    Width += 200;
                    Height += 200;
                    webBrowser1.Width += 200;
                    webBrowser1.Height += 200;
                    webBrowser1.DocumentText = CommonMarkConverter.Convert(CheckVersion.UpdateText);
                    metroProgressBar1.Visible = false;
                    pictureBox1.Visible = false;
                    Movable = true;
                }
                else
                {
                    autoClose++;
                    if(autoClose > 10)
                    {
                        Close();
                    }
                }
            }
            else
            {
                if (LoadCompleted)
                {
                    Close();
                }
            }
        }

        private void Login_Load(object sender, EventArgs e)
        {
            this.TopMost = true;
            this.Focus();
            this.BringToFront();
            this.TopMost = false;
        }

        private void Login_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!LoadCompleted)
            {
                Environment.Exit(0);
            }
        }

        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            Variables.ModifyConfig("General", "AlertUpdate", (!checkBox1.Checked).ToString().ToLower());
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            CheckVersion.UpdateText = null;
            pictureBox1.Visible = true;
            metroProgressBar1.Visible = true;
            Width -= 200;
            Height -= 200;
            webBrowser1.Width -= 200;
            webBrowser1.Height -= 200;
        }
    }
}
