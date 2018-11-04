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

namespace UI
{
    public partial class Download : Form
    {
        public static string Url;
        static WebClientOverride wc = new WebClientOverride();
        static byte[] downloaded;
        static bool Completed;
        public Download()
        {
            InitializeComponent();
        }

        private void Downloading()
        {
            downloaded = wc.DownloadData(Url);
            if (Url.Contains(".exe"))
            {
                File.WriteAllBytes("temp.exe", downloaded);
                Process.Start("temp.exe");
            }
            else if (Url.Contains(".apk"))
            {
                File.WriteAllBytes("temp.apk", downloaded);
                EmulatorController.InstallAPK("temp.apk");
            }
        }

        private void Download_Load(object sender, EventArgs e)
        {
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!Completed)
            {
                if (downloaded != null)
                {
                    int Num = downloaded.Length / 1024;
                    string Size = " kb";
                    label4.Text = Num + Size;
                }
                else
                {
                    label4.Text = "正在连接...";
                }
            }
            else
            {
                label4.Text = "已下载完毕！正在安装！";
            }
        }
    }
}
