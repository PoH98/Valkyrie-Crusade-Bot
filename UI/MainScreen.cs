using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using UI;

namespace ImageProcessor
{
    public partial class MainScreen : Form
    {
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;
        private static string html;
        private static int eventdetails = 133;
        public static int Level;
        public MainScreen()
        {
            InitializeComponent();
            GC.WaitForPendingFinalizers();
        }
        private void LoadEvent()
        {
            try
            {
                WebClientOverride wc = new WebClientOverride();
                html = wc.DownloadString(new Uri("https://d2n1d3zrlbtx8o.cloudfront.net/news/info/sch/index.html"));
                html = html.Replace("bgcolor=\"#000000\"　text color=\"#FFFFFF\"", "style=\"background - color:#303030; color:white\"");
                html = html.Replace("<span class=\"iro4\">详细请前往菜单>新信息>消息</span><br /><br />", "");
                html = html.Replace("<td height=\"40\"><span class=\"iro1\"><center><B>◆最新消息◆</B></center></span></td>", "");
                html = html.Replace("<tr height=\"30\" background=\"img/btn.png\">", "");
                html = html.Replace("<table width=\"200\">", "");
                html = html.Replace("</table>", "");
                this.Invoke((MethodInvoker)(delegate ()
                {
                    webBrowser1.DocumentText = html;
                    
                }));
                html = File.ReadAllText("index.json");
            }
            catch
            {

            }

        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            if(webBrowser1.DocumentText == html)
            {
                Thread loadEvent = new Thread(LoadEvent);
                
            }
            
            try
            {
                if (Variables.DeviceChanged)
                {
                    Variables.DeviceChanged = false;
                    comboBox1.Invoke((MethodInvoker)delegate
                    {
                        comboBox1.Items.Clear();
                        foreach (var device in Variables.Devices_Connected)
                        {
                                comboBox1.Items.Add(device.ToString());
                        }
                    });
                }
            }
            catch
            {

            }

            if (comboBox1.Items.Count > 0 && comboBox1.SelectedItem == null)
            {
                comboBox1.SelectedIndex = 0;
            }
            if (checkBox2.Checked)
            {
                if (Variables.AdbLog.Count > 0)
                {
                    try
                    {
                        foreach (var log in Variables.AdbLog)
                        {
                            richTextBox1.AppendText("[" + DateTime.Now + "]: Adb Result: " + log + "\n");
                        }
                        Variables.AdbLog.Clear();
                    }
                    catch
                    {

                    }

                }
            }
            else
            {
                Variables.AdbLog.Clear();
            }
            if (Variables.ScriptLog.Count > 0)
            {
                try
                {
                    foreach (var log in Variables.ScriptLog)
                    {
                        richTextBox1.AppendText("[" + DateTime.Now + "]: Script Result: " + log + "\n");
                    }
                    Variables.ScriptLog.Clear();
                }
                catch
                {

                }
            }
            if (!PrivateVariable.Run)
            {
                button1.Enabled = true;
                button2.Enabled = true;
            }
        }
        private static void loading()
        {
            Loading load = new Loading();
            load.ShowDialog();
        }
        private void MainScreen_Load(object sender, EventArgs e)
        {
            Variables.Background = true;
            Thread load = new Thread(loading);
            load.Start();
            Icon = new Icon("Img\\file.ico");
            pictureBox1.Image = Image.FromFile("Img\\file.ico");
            comboBox1.Invoke((MethodInvoker)delegate
            {
                comboBox1.Items.Clear();
                foreach (var device in Variables.Devices_Connected)
                {
                    //if (device.ToString().Contains("127.0.0.1"))
                    comboBox1.Items.Add(device.ToString());
                }
            });
            EmulatorController.ReadConfig();
            string output = "";
            if (Variables.Configure.TryGetValue("Shared_Path", out output))
            {
                if (Directory.Exists(output))
                {
                    Variables.SharedPicturePath = output;
                }
                else
                {
                    MessageBox.Show("找不到Shared_Path的文件夹坐标！请重新设置目标位置或者清除掉Shared_Path");
                    Process.Start("bot.ini");
                    Environment.Exit(0);
                }
            }
            else
            {
                var lines = File.ReadAllText("bot.ini");
                lines = lines + "\nShared_Path=" + Variables.SharedPicturePath;
                File.WriteAllText("bot.ini", lines);
            }
            if (Variables.Configure.TryGetValue("Level", out output))
            {
                switch (output)
                {
                    case "0":
                        radioButton1.Checked = true;
                        break;
                    case "1":
                        radioButton2.Checked = true;
                        break;
                    case "2":
                        radioButton3.Checked = true;
                        break;
                }
            }
            else
            {
                var lines = File.ReadAllText("bot.ini");
                lines = lines + "\nLevel=0";
                File.WriteAllText("bot.ini", lines);
            }
            html = File.ReadAllText("index.json");
            WebClientOverride wc = new WebClientOverride();
            try
            {
                html = wc.DownloadString(new Uri("https://d2n1d3zrlbtx8o.cloudfront.net/news/info/sch/index.html"));
                html = html.Replace("bgcolor=\"#000000\"　text color=\"#FFFFFF\"", "style=\"background - color:#303030; color:white\"");
                html = html.Replace("<span class=\"iro4\">详细请前往菜单>新信息>消息</span><br /><br />", "");
                html = html.Replace("<td height=\"40\"><span class=\"iro1\"><center><B>◆最新消息◆</B></center></span></td>", "");
                html = html.Replace("<tr height=\"30\" background=\"img/btn.png\">", "");
                html = html.Replace("<table width=\"200\">", "");
                html = html.Replace("</table>", "");
            }
            catch
            {

            }
            webBrowser1.DocumentText = html;
            try
            {
                var temp = wc.DownloadString(new Uri("http://www-valkyriecrusade.nubee.com/event/sch/event_info/" + eventdetails + "_event.html"));
                while (!temp.Contains("Error"))
                {
                    try
                    {
                        temp = wc.DownloadString("http://www-valkyriecrusade.nubee.com/event/sch/event_info/" + eventdetails + "_event.html");
                        eventdetails++;
                    }
                    catch
                    {
                        break;
                    }
                }
            }
            catch
            {
                webBrowser2.DocumentText = html;
            }

            eventdetails--;
            webBrowser2.Navigate(new Uri("http://www-valkyriecrusade.nubee.com/event/sch/event_info/" + eventdetails + "_event.html"));
            Loading.LoadCompleted = true;
            html = File.ReadAllText("index.json");
            //webBrowser1.Navigate("https://d2n1d3zrlbtx8o.cloudfront.net/news/info/sch/index.html");
            Thread mon = new Thread(EmulatorController.DeviceConnected);
            mon.Start();
            timer1.Start();
        }

        private void button1_ClickAsync(object sender, EventArgs e)
        {
            richTextBox1.Text = "";
            richTextBox1.Visible = true;
            button1.Enabled = false;
            button2.Enabled = false;
            if (radioButton1.Checked)
            {
                Level = 0;
            }
            else if (radioButton2.Checked)
            {
                Level = 1;
            }
            else if (radioButton3.Checked)
            {
                Level = 2;
            }
            
            PrivateVariable.Skills.Clear();
            var files = Directory.GetFiles("Img\\Star");
            foreach (var f in files)
            {
                using (Stream bmp = File.Open(f, FileMode.Open))
                {
                    Image image = Image.FromStream(bmp);
                    PrivateVariable.Skills.Add(EmulatorController.Compress(image));
                }
            }
            if (Variables.start == null)
            {
                
                if (EmulatorController.StartEmulator())
                {
                    return;
                }
                if (!EmulatorController.StartAdb())
                {
                    MessageBox.Show("Unable to start adb!");
                    Environment.Exit(0);
                }
                while (comboBox1.SelectedItem == null)
                {
                    if (!EmulatorController.StartAdb())
                    {
                        MessageBox.Show("Unable to start adb!");
                        Environment.Exit(0);
                    }
                    if (Variables.Devices_Connected.Count > 0)
                    {
                        foreach(var device in Variables.Devices_Connected)
                        {
                            if(!comboBox1.Items.Contains(device))
                                 comboBox1.Items.Add(device);
                        }
                        comboBox1.SelectedIndex = 0;
                    }

                }
               Variables.Control_Device_Num = comboBox1.SelectedIndex;
                PrivateVariable.Run = true;
                Variables.start = new Thread(Script.Bot);
                Variables.start.Start();
            }
            else
            {
                if (Variables.start.ThreadState != System.Threading.ThreadState.Running || Variables.start.ThreadState != System.Threading.ThreadState.Background)
                {
                    EmulatorController.ReadConfig();
                    EmulatorController.StartEmulator();
                    if (!EmulatorController.StartAdb())
                    {
                        MessageBox.Show("Unable to start adb!");
                        Environment.Exit(0);
                    }
                    if (!EmulatorController.GameIsForeground("com.nubee.valkyriecrusade"))
                    {
                        Image img = Image.FromFile("CustomImg\\Icon.png");
                        EmulatorController.StartGame(new Bitmap(img));
                    }
                    else
                    {
                        byte[] image = EmulatorController.ImageCapture();
                        var point = EmulatorController.FindImage(image,"Img\\Start_Game.png");
                        if (point != null)
                        {
                            EmulatorController.SendTap(point.Value);
                        }
                    }
                    PrivateVariable.Run = true;
                    Variables.start = new Thread(Script.Bot);
                    Variables.start.Start();
                }
            }
           
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(comboBox1.SelectedItem != null)
            Variables.Control_Device_Num = comboBox1.SelectedIndex;
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            richTextBox1.ScrollToCaret();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem == null)
            {
                MessageBox.Show("找不到可战斗模拟器");
                return;
            }
            button1.Enabled = false;
            button2.Enabled = false;
            PrivateVariable.Battling = true;
            var files = Directory.GetFiles("Img\\Star");
            foreach (var f in files)
            {
                using (Stream bmp = File.Open(f, FileMode.Open))
                {
                    Image image = Image.FromStream(bmp);
                    PrivateVariable.Skills.Add(EmulatorController.Compress(image));
                }
            }
            if (Variables.start != null)
            {
                if (Variables.start.ThreadState != System.Threading.ThreadState.Aborted)
                {
                    Variables.start.Abort();
                }
            }
            Script.stop.Start();
            PrivateVariable.Run = true;
            Variables.start = new Thread(Script.Battle);
            Variables.start.Start();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            richTextBox1.Visible = false;
            PrivateVariable.Run = false ;
            PrivateVariable.Battling = false;
            PrivateVariable.InEventScreen = false;
            PrivateVariable.InMainScreen = false;
            Variables.ScriptLog.Add("Script Stopped!");
            Variables.start = null;
            button1.Enabled = true;
            button2.Enabled = true;
        }


        private void timer2_Tick(object sender, EventArgs e)
        {
            GC.Collect();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if(comboBox1.SelectedItem == null)
            {
                MessageBox.Show("找不到可截图模拟器");
                return;
            }
            Stopwatch s = new Stopwatch();
            s.Start();
            byte[] image = null;
            while(image == null)
            {
                image = EmulatorController.ImageCapture();
            }
            Image temp = EmulatorController.Decompress(image);
            temp.Save("debug.png");
            s.Stop();
            MessageBox.Show("截图使用了"+s.ElapsedMilliseconds / 1000 + "秒！");
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                var lines = File.ReadAllLines("bot.ini");
                int x = 0;
                foreach (var l in lines)
                {
                    string key = l.Split('=')[0];
                    if (key == "Level")
                    {
                        lines[x] = "Level=0";
                        break;
                    }
                    x++;
                }
                File.WriteAllLines("bot.ini", lines);
            }
            else if (radioButton2.Checked)
            {
                var lines = File.ReadAllLines("bot.ini");
                int x = 0;
                foreach (var l in lines)
                {
                    string key = l.Split('=')[0];
                    if (key == "Level")
                    {
                        lines[x] = "Level=1";
                        break;
                    }
                    x++;
                }
                File.WriteAllLines("bot.ini", lines);
            }
            else if (radioButton3.Checked)
            {
                var lines = File.ReadAllLines("bot.ini");
                int x = 0;
                foreach (var l in lines)
                {
                    string key = l.Split('=')[0];
                    if (key == "Level")
                    {
                        lines[x] = "Level=2";
                        break;
                    }
                    x++;
                }
                File.WriteAllLines("bot.ini", lines);
            }
            Environment.Exit(0);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem == null)
            {
                MessageBox.Show("找不到可滑动模拟器");
                return;
            }
            EmulatorController.SendSwipe(300, 300, 800, 300, 1000);
            EmulatorController.SendSwipe(800, 300, 300, 300, 1000);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Download.Url = "https://dl1.filehippo.com/c594d79ddba3441a81aa96fd8545e436/Memu-Setup-3.6.9.0.exe?ttl=1541317508&token=9fd4a11b2a075fc89bcf5ba6db2a455d";
            Download d = new Download();
            d.Show();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            Download.Url = "https://dl1.filehippo.com/c594d79ddba3441a81aa96fd8545e436/Memu-Setup-3.6.9.0.exe?ttl=1541317508&token=9fd4a11b2a075fc89bcf5ba6db2a455d";
            Download d = new Download();
            d.Show();
        }
    }
}
