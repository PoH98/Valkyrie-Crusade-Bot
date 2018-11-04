using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using ImageProcessor;
using System.Windows.Forms;
using System.Net;
using System.Threading;

namespace UI
{
    public partial class Download : Form
    {
        public static string Url;
        static WebClient wc = new WebClient();
        static bool Completed;
        static bool Error;
        public Download()
        {
            InitializeComponent();
        }

        private void Downloading()
        {
            try
            {
                if (Url.Contains(".exe"))
                {
                    wc.DownloadFile(Url, "temp.exe");
                    Process.Start("temp.exe");
                }
                else if (Url.Contains(".apk"))
                {
                    wc.DownloadFile(Url, "temp.apk");
                    EmulatorController.InstallAPK("temp.apk");
                }
                Completed = true;
            }
            catch
            {
                Error = true;
                Completed = true;
            }
        }

        private void Download_Load(object sender, EventArgs e)
        {
            timer1.Start();
            Completed = false;
            Error = false;
            Thread down = new Thread(Downloading);
            down.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!Completed)
            {
                if (File.Exists("temp.exe"))
                {
                    FileInfo file = new FileInfo("temp.exe");
                    long Num = file.Length / 1024;
                    string Size = " kb";
                    if(Num > 1024)
                    {
                        Num = Num / 1024;
                        Size = " mb";
                    }
                    if (Num > 1024)
                    {
                        Num = Num / 1024;
                        Size = " gb";
                    }
                    label4.Text = Num + Size;
                }
                else if (File.Exists("temp.apk"))
                {
                    FileInfo file = new FileInfo("temp.apk");
                    long Num = file.Length / 1024;
                    string Size = " kb";
                    if (Num > 1024)
                    {
                        Num = Num / 1024;
                        Size = " mb";
                    }
                    if (Num > 1024)
                    {
                        Num = Num / 1024;
                        Size = " gb";
                    }
                    label4.Text = Num + Size;
                }
                else
                {
                    label4.Text = "正在连接...";
                }
            }
            else if(!Error)
            {
                label4.Text = "已下载完毕！正在安装！";
            }
            else
            {
                label4.Text = "发生错误！无法下载！";
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (File.Exists("temp.exe"))
                {
                    File.Delete("temp.exe");
                }
                if (File.Exists("temp.apk"))
                {
                    File.Delete("temp.apk");
                }
            }
            catch
            {

            }
            Close();
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                MainScreen.ReleaseCapture();
                MainScreen.SendMessage(Handle, MainScreen.WM_NCLBUTTONDOWN, MainScreen.HT_CAPTION, 0);
            }
        }
    }
}
