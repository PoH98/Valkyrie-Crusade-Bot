using Microsoft.Win32;
using System;
using System.Reflection;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using UI;
using System.Net;
using System.Text.RegularExpressions;
using SharpAdbClient;
using System.Text;
using System.Security.Principal;
using ImgXml;
using System.Net.Sockets;
using System.Collections.Generic;
using System.ComponentModel;
using MetroFramework.Forms;
using System.Windows.Forms;

namespace ImageProcessor
{
    public partial class MainScreen : MetroForm
    {

        private static string html;

        public static int Level;

        static bool Docked = false;

        static System.Windows.Forms.Timer timeout = new System.Windows.Forms.Timer();

        static List<CheckBox> customScriptEnable = new List<CheckBox>();


        public MainScreen()
        {
            InitializeComponent();
            Debug_.PrepairDebug();
            Variables.richTextBox = richTextBox1;
            timeout.Interval = 5000;
            timeout.Tick += Timeout_Tick;
        }

        private void Timeout_Tick(object sender, EventArgs e)
        {
            if (webBrowser3.IsBusy)
            {
                webBrowser3.Stop();
                webBrowser3.DocumentText = Img.index;
            }
        }

        private static void loading()
        {
            Loading load = new Loading();
            load.ShowDialog();
        }

        private static string Get45PlusFromRegistry()
        {
            const string subkey = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\";

            using (RegistryKey ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(subkey))
            {
                if (ndpKey != null && ndpKey.GetValue("Release") != null)
                {
                    return CheckFor45PlusVersion((int)ndpKey.GetValue("Release"));
                }
                else
                {
                    return null;
                }
            }
        }

        // Checking the version using >= will enable forward compatibility.
        private static string CheckFor45PlusVersion(int releaseKey)
        {
            if (releaseKey >= 461808)
                return "4.7.2 以上";
            if (releaseKey >= 461308)
                return "4.7.1";
            if (releaseKey >= 460798)
                return "4.7";
            if (releaseKey >= 394802)
                return "4.6.2";
            if (releaseKey >= 394254)
                return "4.6.1";
            if (releaseKey >= 393295)
                return "4.6";
            if (releaseKey >= 379893)
                return "4.5.2";
            if (releaseKey >= 378675)
                return "4.5.1";
            if (releaseKey >= 378389)
                return "4.5";
            // This code should never execute. A non-null release key should mean
            // that 4.5 or later is installed.
            return "4.5 以上";
        }

        private void MainScreen_Load(object sender, EventArgs e)
        {
            Thread load = new Thread(loading);
            load.Start();
            if (!IsRunAsAdministrator())
            {
                var processInfo = new ProcessStartInfo(Assembly.GetExecutingAssembly().CodeBase);
                // The following properties run the new process as administrator
                processInfo.UseShellExecute = true;
                processInfo.Verb = "runas";
                // Start the new process
                try
                {
                    Process.Start(processInfo);
                }
                catch (Exception)
                {
                    // The user did not allow the application to run as administrator
                    MessageBox.Show("挂机需要权限才能运行！");
                }
                // Shut down the current process
                Application.Exit();
            }
            if (!Directory.Exists("Audio"))
            {
                Directory.CreateDirectory("Audio");
            }
            string _NET = Get45PlusFromRegistry();
            if (_NET.Length > 0)
            {
                label10.Text = "当前.NET版本：" + _NET;
            }
            Variables.Background = true;
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            this.Text = this.Text + "  v" + fvi.FileVersion;
            if (File.Exists("Updater.exe"))
            {
                ProcessStartInfo updater = new ProcessStartInfo();
                updater.FileName = "Updater.exe";
                updater.WindowStyle = ProcessWindowStyle.Minimized;
                updater.Arguments = fvi.FileVersion.ToString();
                Process.Start(updater);
            }
            try
            {
                IntPtr ico = Img.Icon.GetHicon();
                Icon = Icon.FromHandle(ico);
            }
            catch
            {

            }
            string output = "";
            string[] args = Environment.GetCommandLineArgs();
            if (File.Exists("bot.ini"))
            {
                if (File.Exists(Environment.CurrentDirectory + "\\Profiles\\" + EmulatorController.profilePath + "\\bot.ini"))
                {
                    File.Delete(Environment.CurrentDirectory + "\\Profiles\\" + EmulatorController.profilePath + "\\bot.ini");
                }
                try
                {
                    File.Copy("bot.ini", Environment.CurrentDirectory + "\\Profiles\\" + EmulatorController.profilePath + "\\bot.ini", true);
                    File.Delete("bot.ini");
                }
                catch
                {

                }
            }
            Variables.EmulatorPath(args);
            EmulatorController.ReadConfig();
            label11.Text = "逍模拟器：" + Variables.emulator.EmulatorName();
            label12.Text = "模拟器共享文件夹：" + Variables.SharedPath;
            foreach(var a in args)
            {
                label1.Text += a;
            }
            bool second = false;
            if (File.Exists("Archwitch.ini"))
            {
                foreach (var line in File.ReadAllLines("Archwitch.ini"))
                {
                    try
                    {
                        if (line.Contains("[2]"))
                        {
                            second = true;
                            continue;
                        }
                        else if (line.Contains("[1]"))
                        {
                            second = false;
                            continue;
                        }
                        string key = line.Split('=')[0];
                        Point value = new Point(Convert.ToInt32(line.Split('=')[1].Split(',')[0]), Convert.ToInt32(line.Split('=')[1].Split(',')[1]));
                        comboBox1.Items.Add(key);
                        if (!second)
                        {
                            PrivateVariable.Archwitch.Add(key, value);
                        }
                        else
                        {
                            PrivateVariable.Archwitch2.Add(key, value);
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
            else
            {
                MessageBox.Show("请重新安装挂机，挂机缺少了文件！");
                Environment.Exit(0);
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
                WriteConfig("Level", "0");
            }
            if (Variables.Configure.TryGetValue("Double_Event", out output))
            {
                if (output == "true")
                {
                    checkBox8.Checked = true;
                }
            }
            if (Variables.Configure.TryGetValue("Treasure_Hunt", out output))
            {
                if (output != "-1")
                {
                    try
                    {
                        checkBox12.Checked = true;
                        comboBox2.SelectedIndex = Convert.ToInt32(output);
                    }
                    catch
                    {

                    }
                }
            }
            if (Variables.Configure.TryGetValue("Manual_Rune", out output))
            {
                if (output == "true")
                {
                    radioButton10.Checked = true;
                    PrivateVariable.EnterRune = false;
                }
            }
            if (Variables.Configure.TryGetValue("Close_Emulator", out output))
            {
                if (output == "true")
                {
                    checkBox13.Checked = true;
                }
            }
            if (Variables.Configure.TryGetValue("Archwitch", out output))
            {
                comboBox1.SelectedIndex = comboBox1.Items.IndexOf(output);
            }
            if (EmulatorController.Is64BitOperatingSystem())
            {
                label13.Text = "系统资料：64位系统";
            }
            else
            {
                label13.Text = "系统资料：32位系统";
            }
            webBrowser1.ScriptErrorsSuppressed = true;
            webBrowser3.ScriptErrorsSuppressed = true;
            PrivateVariable.nospam = DateTime.Now;
            string ua = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/60.0.3112.113 Safari/537.36";
            DllImport.UrlMkSetSessionOption(DllImport.URLMON_OPTION_USERAGENT, ua, ua.Length, 0);
            html = Img.index;
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
            webBrowser3.Navigating += OnNavigating;
            webBrowser3.Navigated += WebBrowser3_Navigated;
            GetEventXML.LoadXMLEvent();
            webBrowser3.Navigate(new Uri("http://www-valkyriecrusade.nubee.com/" + GetEventXML.Eventlink.Replace("/en/", "/sch/") + ".html"));
            Script.Read_Plugins();
            foreach (var s in PrivateVariable.BattleScript)
            {
                tabControl2.TabPages.Add(s.ScriptName());
                CheckBox chk = new CheckBox();
                chk.Text = "使用脚本";
                chk.Checked = false;
                chk.CheckedChanged += Chk_CheckedChanged;
                chk.Location = new Point(10, 250);
                chk.AutoSize = true;
                tabControl2.TabPages[tabControl2.TabPages.Count - 1].Controls.Add(chk);
                customScriptEnable.Add(chk);
                foreach (var c in s.CreateUI())
                {
                    tabControl2.TabPages[tabControl2.TabPages.Count - 1].Controls.Add(c);
                }
            }
            string n = "";
            if (Variables.Configure.TryGetValue("Selected_Script", out n))
            {
                try
                {
                    int i = Convert.ToInt32(n);
                    if (i > customScriptEnable.Count)
                    {
                        i = 0;
                    }
                    customScriptEnable[i].Checked = true;
                }
                catch
                {
                    customScriptEnable[0].Checked = true;
                }
            }
            else
            {
                customScriptEnable[0].Checked = true;
            }
            if (GetEventXML.RandomImage != null)
            {
                var request = WebRequest.Create("http://www-valkyriecrusade.nubee.com/" + GetEventXML.RandomImage);
                using (var response = request.GetResponse())
                using (var stream = response.GetResponseStream())
                {
                    pictureBox4.Image = Image.FromStream(stream);
                }
            }
            metroTabControl1.SelectedIndex = 0;
            checkBox10.Enabled = radioButton9.Checked;
            Loading.LoadCompleted = true;
            PrivateVariable.EventType = -1;
            timer2.Start();
        }

        private void Chk_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox ck = sender as CheckBox;
            if (ck.Checked)
            {
                PrivateVariable.Selected_Script = customScriptEnable.IndexOf(ck);
                WriteConfig("Selected_Script", PrivateVariable.Selected_Script.ToString());
            }
            foreach (var c in customScriptEnable)
            {
                if (c != ck)
                {
                    c.Checked = !ck.Checked;
                }
            }
        }

        private void WebBrowser3_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            timeout.Stop();
        }

        private void OnNavigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            //Reset Timer
            timeout.Stop();
            timeout.Start();

        }

        private bool IsRunAsAdministrator()
        {
            var wi = WindowsIdentity.GetCurrent();
            var wp = new WindowsPrincipal(wi);
            return wp.IsInRole(WindowsBuiltInRole.Administrator);
        }
        private void button1_Click(object sender, EventArgs e)
        {
            ScriptErrorHandler.errorImages.Clear();
            foreach (var f in Directory.GetFiles("Img\\Errors"))
            {
                Thread.Sleep(10);
                using (Stream bmp = File.Open(f, FileMode.Open))
                {
                    Image temp = Image.FromStream(bmp);
                    ScriptErrorHandler.errorImages.Add(new Bitmap(temp));
                }
            }
            if ((PrivateVariable.nospam - DateTime.Now).Duration() < TimeSpan.FromSeconds(4))
            {
                MessageBox.Show("啊！亚麻跌！慢点！好疼啊！");
                return;
            }
            Point output;
            if (comboBox1.SelectedIndex == -1)
            {
                comboBox1.SelectedIndex = 0;
            }
            if (PrivateVariable.Archwitch.TryGetValue(comboBox1.Items[comboBox1.SelectedIndex].ToString(), out output))
            {
                Script.Archwitch_Stage = comboBox1.SelectedIndex;
                WriteConfig("Second_Page", "false");
                Script.archwitch_level_location = PrivateVariable.Archwitch[comboBox1.Items[comboBox1.SelectedIndex].ToString()];
            }
            else
            {
                Script.Archwitch_Stage = comboBox1.SelectedIndex - PrivateVariable.Archwitch.Count;
                WriteConfig("Second_Page", "true");
                Script.archwitch_level_location = PrivateVariable.Archwitch2[comboBox1.Items[comboBox1.SelectedIndex].ToString()];
            }
            PrivateVariable.nospam = DateTime.Now;
            PrivateVariable.Run = true;
            if (textBox2.Text.Length > 0)
            {
                if (textBox2.Text.Contains("来点色图"))
                {
                    MessageBox.Show("恭喜您启动了隐藏的功能！");
                    Suprise sup = new Suprise();
                    sup.Show();
                    return;
                }
                try
                {
                    if (textBox2.Text.Length == 15)
                    {
                        Convert.ToInt64(textBox2.Text);
                    }
                    else
                    {
                        MessageBox.Show("请输入正确的IMEI (只有数字)");
                        return;
                    }
                }
                catch
                {
                    MessageBox.Show("请输入正确的IMEI (只有数字)");
                    return;
                }
                string path = "";
                Variables.Configure.TryGetValue("Path", out path);
                path = path.Replace("MEmu\\MEmu.exe", "MEmuHyperv\\cmd.bat");
                string text = "MEmuManage.exe guestproperty set MEmu imei " + textBox2.Text;
                ProcessStartInfo server = new ProcessStartInfo();
                File.WriteAllText(path, text);
                server.FileName = path;
                server.UseShellExecute = true;
                server.WorkingDirectory = path.Replace("\\cmd.bat", "");
                server.CreateNoWindow = true;
                Process p = Process.Start(server);
            }
            richTextBox1.Text = "";
            button1.Enabled = false;
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
            if (panel3.Visible == false)
            {
                Width += 700;
                panel3.Visible = true;
            }
            panel3.Enabled = false;
            Thread cap = new Thread(Capt);
            cap.Start();
            Thread run = new Thread(Script.Bot);
            run.Start();
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            richTextBox1.ScrollToCaret();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            PrivateVariable.nospam = DateTime.Now;
            PrivateVariable.Run = false;
            PrivateVariable.Battling = false;
            PrivateVariable.InEventScreen = false;
            PrivateVariable.InMainScreen = false;
            PrivateVariable.InMap = false;
            Variables.ScriptLog("Script Stopped!",Color.White);
            if (Width > 480)
            {
                Width -= 700;
                panel3.Visible = false;
            }
            if (EmulatorController.handle != null && Variables.Proc != null)
            {
                DllImport.SetParent(EmulatorController.handle, IntPtr.Zero);
                DllImport.MoveWindow(EmulatorController.handle, PrivateVariable.EmuDefaultLocation.X, PrivateVariable.EmuDefaultLocation.Y, 1318, 752, true);
                EmulatorController.handle = IntPtr.Zero;
                Docked = false;
            }
            Variables.start = null;
            button1.Enabled = true;
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            GC.Collect();
            if (PrivateVariable.EventType == 0)
            {
                groupBox8.Text = "塔楼活动";
                progressBar1.Value = Script.energy;
                progressBar2.Value = Script.runes;
                label7.Text = Script.runes + "/5";
                label6.Text = Script.energy + "/5";
                if (Script.nextOnline != null)
                {
                    if (Script.nextOnline > DateTime.Now)
                    {
                        TimeSpan time = Script.nextOnline - DateTime.Now;
                        label9.Text = time.Hours.ToString("00") + " : " + time.Minutes.ToString("00") + " : " + time.Seconds.ToString("00");
                    }
                }
                if (Script.Tower_Floor.Length > 0)
                {
                    try
                    {
                        label15.Text = Script.Tower_Floor.Replace(" ", "").Replace("F", " F");
                    }
                    catch
                    {

                    }
                }
                if (Script.Tower_Rank.Length > 0)
                {
                    try
                    {
                        label16.Text = Script.Tower_Rank.Replace(" ", "");
                    }
                    catch
                    {

                    }
                }
            }
            else if (PrivateVariable.EventType == 1)
            {
                groupBox8.Text = "魔女讨伐";
            }
            else if (PrivateVariable.EventType == 2)
            {
                groupBox8.Text = "魔界活动";
                label5.Text = "地图碎片数量";
                label7.Text = Script.runes + "/4";
                label6.Text = Script.energy + "/5";
                progressBar2.Maximum = 4;
                progressBar1.Value = Script.energy;
                progressBar2.Value = Script.runes;
                if (Script.nextOnline != null)
                {
                    if (Script.nextOnline > DateTime.Now)
                    {
                        TimeSpan time = Script.nextOnline - DateTime.Now;
                        label9.Text = time.Hours.ToString("00") + " : " + time.Minutes.ToString("00") + " : " + time.Seconds.ToString("00");
                    }
                }
                if (Script.Tower_Floor.Length > 0)
                {
                    try
                    {
                        label15.Text = Script.Tower_Floor.Replace(" ", "").Replace("F", " F");
                    }
                    catch
                    {

                    }
                }
                if (Script.Tower_Rank.Length > 0)
                {
                    try
                    {
                        label16.Text = Script.Tower_Rank.Replace(" ", "");
                    }
                    catch
                    {

                    }
                }
            }
            else
            {
                groupBox8.Text = "未知的活动";
            }
        }
        /// <summary>
        /// Capture loop
        /// </summary>
        private void Capt()
        {
            while (PrivateVariable.Run)
            {
                Thread.Sleep(1000);
                if (Variables.Proc != null)
                {
                    if (EmulatorController.handle == IntPtr.Zero || EmulatorController.handle == null)
                    {
                        EmulatorController.ConnectAndroidEmulator();
                    }
                    if (!DllImport.IsWindow(EmulatorController.handle))
                    {
                        EmulatorController.handle = IntPtr.Zero;
                        Docked = false;
                    }
                    if(Variables.emulator.EmulatorName() == "Nox")
                    {
                        var hide = DllImport.GetAllChildrenWindowHandles(IntPtr.Zero, "Qt5QWindowToolSaveBits", "Form", 2);
                        foreach(var h in hide)
                        {
                            DllImport.ShowWindow(h, 0);
                        }
                    }
                    panel3.Invoke((MethodInvoker)delegate
                    {
                        if (DllImport.GetParent(EmulatorController.handle) != panel3.Handle)
                        {
                            if (PrivateVariable.Run && !Docked)
                            {
                                DllImport.Rect rect = new DllImport.Rect();
                                DllImport.GetWindowRect(EmulatorController.handle, ref rect);
                                PrivateVariable.EmuDefaultLocation = new Point(rect.left, rect.top);
                                DllImport.SetParent(EmulatorController.handle, panel3.Handle);
                                DllImport.MoveWindow(EmulatorController.handle, -1, -30, 736, 600, false);
                                Docked = true;
                            }
                            else if (Docked)
                            {
                                DllImport.Rect rect = new DllImport.Rect();
                                DllImport.GetWindowRect(EmulatorController.handle, ref rect);
                                if (rect.left != -1 || rect.top != -55)
                                {
                                    DllImport.MoveWindow(EmulatorController.handle, -1, -30, 736, 600, false);
                                }
                            }
                        }
                    });
                    try
                    {
                        var newimage = EmulatorController.ImageCapture();
                        if (newimage != null)
                        {
                            Script.image = newimage;
                        }
                    }
                    catch (Exception ex)
                    {
                        File.WriteAllText("error.log", ex.ToString());
                    }
                }
                else
                {
                    Docked = false;
                    int error = 0;
                    while (Variables.Proc == null)
                    {
                        EmulatorController.ConnectAndroidEmulator();
                        error++;
                        Thread.Sleep(1000);
                        if (error > 60)
                        {
                            EmulatorController.CloseEmulator();
                            EmulatorController.StartEmulator();
                            error = 0;
                        }
                    }
                    Variables.ScriptLog("Emulator Started",Color.White);
                }
            }
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                DllImport.ReleaseCapture();
                DllImport.SendMessage(Handle, DllImport.WM_NCLBUTTONDOWN, DllImport.HT_CAPTION, 0);
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (File.Exists("Updater.exe"))
            {
                Process.Start("Updater.exe", "http://dl.memuplay.com/download/backup/Memu-Setup-3.7.0.0.exe");
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (File.Exists("Updater.exe"))
            {
                Process.Start("Updater.exe", "https://github.com/PoH98/Bot/raw/master/神女控.apk");
            }
        }

        private void button1_MouseEnter(object sender, EventArgs e)
        {
            button1.BackColor = Color.Lime;
        }

        private void button1_MouseLeave(object sender, EventArgs e)
        {
            button1.BackColor = Color.Silver;
        }

        private void button3_MouseEnter(object sender, EventArgs e)
        {
            button3.BackColor = Color.Red;
        }

        private void button3_MouseLeave(object sender, EventArgs e)
        {
            button3.BackColor = Color.Silver;
        }

        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {
            PrivateVariable.TakePartInNormalStage = checkBox6.Checked;
            comboBox3.SelectedIndex = 0;
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            PrivateVariable.NormalStageNum = comboBox3.SelectedIndex + 1;
        }

        private void checkBox8_CheckedChanged(object sender, EventArgs e)
        {
            WriteConfig("Double_Event", checkBox8.Checked.ToString().ToLower());
            if (checkBox8.Checked && !File.Exists("Img\\Event.png"))
            {
                MessageBox.Show("请截图好活动的进入按钮样貌，保存到Img文件夹内，命名为Event.png!");
            }
        }

        private void checkBox8_MouseEnter(object sender, EventArgs e)
        {
            toolTip1.Show("如果在主城页面的左边选择活动区域拥有多个活动，影响挂机进入塔楼/魔女，请打勾这个！请慎用！", checkBox8);
        }

        private void checkBox8_MouseLeave(object sender, EventArgs e)
        {
            toolTip1.Hide(checkBox8);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            if (File.Exists("battlescript.txt"))
            {
                Process.Start("battlescript.txt");
            }
            else
            {
                File.WriteAllText("battlescript.txt", "重返| | ");
                Process.Start("battlescript.txt");
            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            Script.GetEnergy();
        }

        private void radioButton8_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton8.Checked)
            {
                if (checkBox11.Checked)
                {
                    Process.Start("http://www.xldsdr.com/valkyriecrusade");
                }
                else
                {
                    if (webBrowser3.Url != new Uri("http://www.xldsdr.com/valkyriecrusade"))
                    {
                        webBrowser3.Navigate(new Uri("http://www.xldsdr.com/valkyriecrusade"));
                    }
                }
            }

        }

        private void radioButton6_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton6.Checked)
            {
                if (checkBox11.Checked)
                {
                    Process.Start("http://www-valkyriecrusade.nubee.com/" + GetEventXML.Eventlink.Replace("/en/", "/sch/") + ".html");
                }
                else
                {
                    webBrowser3.Navigate(new Uri("http://www-valkyriecrusade.nubee.com/" + GetEventXML.Eventlink.Replace("/en/", "/sch/") + ".html"));
                }

            }

        }

        private void button11_Click_1(object sender, EventArgs e)
        {
            webBrowser3.Refresh();
        }

        private void button12_Click(object sender, EventArgs e)
        {
            webBrowser3.GoBack();
        }

        private void button13_Click(object sender, EventArgs e)
        {
            webBrowser3.GoForward();
        }

        private void checkBox12_CheckedChanged(object sender, EventArgs e)
        {
            //comboBox2.Enabled = checkBox12.Checked;
            MessageBox.Show("这是个测试的功能，如果发现有任何卡着不动或者瞎点的情况，请尽快汇报！");
            if (!checkBox12.Checked)
            {
                WriteConfig("Treasure_Hunt", "-1");
            }
            else
            {
                comboBox2.SelectedIndex = 0;
                WriteConfig("Treasure_Hunt", comboBox2.SelectedIndex.ToString());
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            Script.TreasureHuntIndex = comboBox2.SelectedIndex;
        }

        private void radioButton9_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton9.Checked)
            {
                PrivateVariable.EnterRune = true;
                WriteConfig("Manual_Rune", "false");
                checkBox10.Enabled = radioButton9.Checked;
            }
        }

        private void radioButton10_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton10.Checked)
            {
                PrivateVariable.EnterRune = false;
                WriteConfig("Manual_Rune", "true");
                checkBox10.Checked = false;
                checkBox10.Enabled = false;
            }
        }

        private void webBrowser3_NewWindow(object sender, System.ComponentModel.CancelEventArgs e)
        {
            webBrowser3.Navigate(webBrowser3.StatusText);
            e.Cancel = true;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                WriteConfig("Level", "0");
                Level = 0;
            }
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
            {
                WriteConfig("Level", "1");
                Level = 1;
            }
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton3.Checked)
            {
                WriteConfig("Level", "2");
                Level = 2;
            }
        }

        private void checkBox13_CheckedChanged(object sender, EventArgs e)
        {
            WriteConfig("Close_Emulator", checkBox13.Checked.ToString().ToLower());
            PrivateVariable.CloseEmulator = checkBox13.Checked;
        }

        private void radioButton11_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton11.Checked)
                webBrowser3.Navigate("https://jq.qq.com/?_wv=1027&k=51gVT8A");
        }

        private void checkBox10_CheckedChanged(object sender, EventArgs e)
        {
            WriteConfig("Use_Item", checkBox10.Checked.ToString().ToLower());
            PrivateVariable.Use_Item = checkBox10.Checked;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            WriteConfig("Archwitch", comboBox1.Items[comboBox1.SelectedIndex].ToString());
        }

        private void radioButton7_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton7.Checked)
            {
                if (checkBox11.Checked)
                {
                    Process.Start("http://d2n1d3zrlbtx8o.cloudfront.net/news/help/sch/index.html");
                }
                else
                {
                    webBrowser3.Visible = true;
                    webBrowser3.Navigate("http://d2n1d3zrlbtx8o.cloudfront.net/news/help/sch/index.html");
                }
            }
        }

        private static void WriteConfig(string key, string value)
        {
            var config = File.ReadAllLines("Profiles\\" + EmulatorController.profilePath + "\\bot.ini");
            int x = 0;
            foreach (var c in config)
            {
                if (c.Contains(key + "="))
                {
                    config[x] = key + "=" + value;
                    File.WriteAllLines("Profiles\\" + EmulatorController.profilePath + "\\bot.ini", config);
                    return;
                }
                x++;
            }
            config[config.Length - 1] = config[config.Length - 1] + "\n" + key + "=" + value;
            if (Variables.Configure.ContainsKey(key))
            {
                Variables.Configure[key] = value;
            }
            else
            {
                Variables.Configure.Add(key, value);
            }
            File.WriteAllLines("Profiles\\" + EmulatorController.profilePath + "\\bot.ini", config);
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            if (pictureBox4.Image != null)
            {
                SaveFileDialog s = new SaveFileDialog();
                s.CheckPathExists = true;
                s.OverwritePrompt = true;
                s.AddExtension = false;
                s.Filter = "(PNG)|*.png";
                s.DefaultExt = "png";
                s.AddExtension = true;
                var result = s.ShowDialog();
                if (result == DialogResult.OK)
                {
                    pictureBox4.Image.Save(s.FileName);
                }
            }

        }

        private void pictureBox4_MouseEnter(object sender, EventArgs e)
        {
            toolTip1.Show("点击即可保存图片哦！", pictureBox4);
        }

        private void pictureBox4_MouseLeave(object sender, EventArgs e)
        {
            toolTip1.Hide(pictureBox4);
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton4.Checked)
            {
                WriteConfig("Level", "3");
                Level = 3;
            }
        }

        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton5.Checked)
            {
                WriteConfig("Level", "4");
                Level = 4;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (File.Exists("OCR.png"))
            {
                var img = EmulatorController.Compress(Image.FromFile("OCR.png"));
                MessageBox.Show(OCR.OcrImage(img, "eng"));
            }
        }

        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            Variables.AdbLogShow = checkBox1.Checked;
        }

        private void MetroLink1_Click(object sender, EventArgs e)
        {
            Process.Start("www.github.com/PoH98/");
        }

        private void WebBrowser3_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (e.Url.ToString().Contains("http://www.xldsdr.com/valkyriecrusade"))
            {
                var divs = webBrowser3.Document.GetElementsByTagName("div");
                foreach (HtmlElement div in divs)
                {
                    if (div.GetAttribute("className") == "more-info-box")
                    {
                        div.InnerText = "";
                    }
                }
                foreach(HtmlElement footer in webBrowser3.Document.GetElementsByTagName("footer"))
                {
                    footer.InnerText = "";
                }

            }
        }

        private void MainScreen_FormClosing(object sender, FormClosingEventArgs e)
        {
            var lines = File.ReadAllLines("Profiles\\" + EmulatorController.profilePath + "\\bot.ini");
            if (radioButton1.Checked)
            {
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
            }
            else if (radioButton2.Checked)
            {
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
            }
            else if (radioButton3.Checked)
            {
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
            }
            if (Variables.Background == true)
            {
                int x = 0;
                foreach (var l in lines)
                {
                    string key = l.Split('=')[0];
                    if (key == "Background")
                    {
                        lines[x] = "Background=true";
                        break;
                    }
                    x++;
                }
            }
            else
            {
                int x = 0;
                foreach (var l in lines)
                {
                    string key = l.Split('=')[0];
                    if (key == "Background")
                    {
                        lines[x] = "Background=false";
                        break;
                    }
                    x++;
                }
            }

            if (checkBox12.Checked)
            {
                int x = 0;
                foreach (var l in lines)
                {
                    string key = l.Split('=')[0];
                    if (key == "Treasure_Hunt")
                    {
                        lines[x] = "Treasure_Hunt=" + comboBox2.SelectedIndex;
                        break;
                    }
                    x++;
                }
            }
            else
            {
                int x = 0;
                foreach (var l in lines)
                {
                    string key = l.Split('=')[0];
                    if (key == "Treasure_Hunt")
                    {
                        lines[x] = "Treasure_Hunt=-1";
                        break;
                    }
                    x++;
                }
            }
            File.WriteAllLines("Profiles\\" + EmulatorController.profilePath + "\\bot.ini", lines);
            if (PrivateVariable.Run)
            {
                button3_Click(sender, e);
            }
            Environment.Exit(0);
        }
    }
}
