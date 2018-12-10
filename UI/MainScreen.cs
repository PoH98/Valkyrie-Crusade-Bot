using Microsoft.Win32;
using System;
using System.Reflection;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using UI;
using System.Net;
using System.Text.RegularExpressions;
using SharpAdbClient;
using System.Text;
using System.Collections.Generic;
using System.Security.Principal;

namespace ImageProcessor
{
    public partial class MainScreen : Form
    {
        
        private static string html;

        private static int eventdetails = 133, ReloadTime = 0;

        public static int Level;

        static bool Docked = false;

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
            if(ReloadTime == 0)
            {
                Thread l = new Thread(LoadEventBrowser);
                l.Start();
                ReloadTime = 36000;

            }
            else
            {
                ReloadTime--;
            }
            if (Adb_Log.Checked)
            {
                File.AppendAllLines("日志.log", Variables.AdbLog);
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
                    MessageBox.Show("Sorry, this application must be run as Administrator.");
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
            label3.Text = label3.Text + "  v" + fvi.FileVersion;
            if (File.Exists("Updater.exe"))
            {
                Process.Start("Updater.exe", fvi.FileVersion.ToString());
            }
            Thread load = new Thread(loading);
            load.Start();
            Icon = new Icon("Img\\file.ico");
            pictureBox1.Image = Image.FromFile("Img\\file.ico");
            EmulatorController.ReadConfig();
            string output = "";
            string[] args = Environment.GetCommandLineArgs();
            foreach(var arg in args)
            {
                if (arg.Contains("MEmu"))
                {
                    Variables.Instance = arg;
                    label3.Text += "("+Variables.Instance+")";
                }
                label1.Text += arg.ToLower() + " ";
            }
            string startuppath = "C:";
            RegistryKey reg = Registry.LocalMachine;
            try
            {
                var r = reg.OpenSubKey("SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\MEmu");
                var result = r.GetValue("InstallLocation");
                if (result != null)
                {
                    startuppath = result.ToString();
                }
            }
            catch //Not able to locate registry, no MEmu found
            {
                MessageBox.Show("请安装逍遥模拟器再继续运行！");
                Download.Url = "http://dl.memuplay.com/download/backup/Memu-Setup-3.7.0.0.exe";
                Download d = new Download();
                d.ShowDialog();
            }
            openFileDialog1.InitialDirectory = startuppath;
            if (Variables.Configure.TryGetValue("Path", out output))
            {
                if (!File.Exists(output))
                {
                    openFileDialog1.FileName = "";
                    openFileDialog1.Title = "选择逍遥模拟器 (MEmu.exe)";
                    openFileDialog1.Filter = "MEmu.exe|MEmu.exe";
                    openFileDialog1.ShowDialog();
                    if (openFileDialog1.CheckFileExists)
                    {
                        Variables.Configure["Path"] = openFileDialog1.FileName;
                        var lines = File.ReadAllLines("bot.ini");
                        int x = 0;
                        foreach (var l in lines)
                        {
                            if (l.Contains("Path="))
                            {
                                lines[x] = "Path=" + openFileDialog1.FileName;
                                break;
                            }
                            x++;
                        }
                        output = openFileDialog1.FileName;
                        File.WriteAllLines("bot.ini", lines);
                    }
                }
            }
            else
            {
                openFileDialog1.FileName = "";
                openFileDialog1.Title = "选择逍遥模拟器 (MEmu.exe)";
                openFileDialog1.Filter = "MEmu.exe|MEmu.exe";
                openFileDialog1.ShowDialog();
                if (openFileDialog1.CheckFileExists)
                {
                    Variables.Configure.Add("Path",openFileDialog1.FileName);
                    var lines = File.ReadAllText("bot.ini");
                    lines = lines + "\nPath=" + openFileDialog1.FileName;
                    File.WriteAllText("bot.ini", lines);
                    output = openFileDialog1.FileName;
                }
            }
            if (!output.Contains("MEmu.exe"))
            {
                MessageBox.Show("请安装逍遥模拟器再继续运行！");
                Download.Url = "http://dl.memuplay.com/download/backup/Memu-Setup-3.7.0.0.exe";
                Download d = new Download();
                d.ShowDialog();
            }
            Variables.VBoxManagerPath = output.Replace("\\MEmu\\MEmu.exe", "\\MEmuHyperv");
            ProcessStartInfo fetch = new ProcessStartInfo(Variables.VBoxManagerPath + "\\MEmuManage.exe");
            if (Variables.Instance.Length > 0)
            {
                fetch.Arguments = "showvminfo " + Variables.Instance;
            }
            else
            {
                fetch.Arguments = "showvminfo MEmu";
            }
            fetch.RedirectStandardOutput = true;
            fetch.UseShellExecute = false;
            fetch.CreateNoWindow = true;
            fetch.StandardOutputEncoding = Encoding.ASCII;
            while (Variables.SharedPath == null || PrivateVariable.Adb_IP == null)
            {
                Process fetching = Process.Start(fetch);
                string result = fetching.StandardOutput.ReadToEnd();
                string[] splitted = result.Split('\n');
                foreach (var s in splitted)
                {
                    if (s.Contains("Name: 'download', Host path:"))
                    {
                        string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
                        Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
                        var path = s.ToLower().Replace("name: 'download', host path: '", "").Replace("' (machine mapping), writable", "");
                        path = r.Replace(path, "\\");
                        path = path.Replace("\\\\", ":\\");
                        if (output != path)
                        {
                            Variables.SharedPath = path;
                            WriteConfig("Shared_Path", path);
                            if (!Directory.Exists(path))
                            {
                                MessageBox.Show("不支持英文字母以外的文件夹路径，请先在模拟器内设置好新路径后重启挂机");
                                Environment.Exit(0);
                            }
                        }
                        else
                        {
                            Variables.SharedPath = output;
                        }
                    }
                    else if (s.Contains("name = ADB"))
                    {
                        var port = s.Substring(s.IndexOf("port = ") + 7, 5).Replace(" ", "");
                        PrivateVariable.Adb_IP = "127.0.0.1:" + port;
                    }
                }
            }
            label11.Text = "逍遥模拟器安装位置：" + output.ToLower(); ;
            label12.Text = "逍遥模拟器共享文件夹：" + Variables.SharedPath;
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
                WriteConfig("Level","0");
            }
            if (Variables.Configure.TryGetValue("background", out output))
            {
                if (output == "true")
                {
                    Variables.Background = true;
                }
                else
                {
                    checkBox1.Checked = false;
                    Variables.Background = false;
                }
            }
            else
            {
                Variables.Configure.Add("background", "true");
                Variables.Background = true;
                using (var stream = File.AppendText("bot.ini"))
                {
                    stream.WriteLine("background=true");
                }
            }
            if(Variables.Configure.TryGetValue("Double_Event",out output))
            {
                if(output == "true")
                {
                    checkBox8.Checked = true;
                }
            }
            if (Variables.Configure.TryGetValue("CustomScript", out output))
            {
                if (output == "true")
                {
                    PrivateVariable.CustomScript = true;
                    checkBox10.Checked = true;
                }
                else
                {
                    PrivateVariable.CustomScript = false;
                }
            }
            else
            {
                Variables.Configure.Add("CustomScript", "true");
                PrivateVariable.CustomScript = false;
                using (var stream = File.AppendText("bot.ini"))
                {
                    stream.WriteLine("CustomScript=false");
                }
            }
            if (Variables.Configure.TryGetValue("Archwitch_New", out output))
            {
                if (output == "true")
                {
                    PrivateVariable.AlwaysAttackNew = true;
                    checkBox9.Checked = true;
                }
                else
                {
                    PrivateVariable.AlwaysAttackNew = false;
                    checkBox9.Checked = false;
                }
            }
            if(Variables.Configure.TryGetValue("Treasure_Hunt",out output))
            {
                if(output != "-1")
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
            if(Variables.Configure.TryGetValue("Manual_Rune",out output))
            {
                if(output == "true")
                {
                    radioButton10.Checked = true;
                    PrivateVariable.EnterRune = false;
                }
            }
            if(Variables.Configure.TryGetValue("Close_Emulator",out output))
            {
                if(output == "true")
                {
                    checkBox13.Checked = true;
                }
            }
            if (Is64BitOperatingSystem())
            {
                label13.Text = "系统资料：64位系统" ;
            }
            else
            {
                label13.Text = "系统资料：32位系统";
            }
            PrivateVariable.nospam = DateTime.Now;
            string ua = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/60.0.3112.113 Safari/537.36";
            DllImport.UrlMkSetSessionOption(DllImport.URLMON_OPTION_USERAGENT, ua, ua.Length, 0);
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
            LoadEventBrowser();
            webBrowser3.Navigate(new Uri("http://www.xldsdr.com/valkyriecrusade"));
            Loading.LoadCompleted = true;
            Thread mon = new Thread(DeviceConnected);
            mon.Start();
            PrivateVariable.EventType = -1;
            timer1.Start();
            timer2.Start();
            EmulatorController.StartAdb();
        }

        private bool IsRunAsAdministrator()
        {
            var wi = WindowsIdentity.GetCurrent();
            var wp = new WindowsPrincipal(wi);

            return wp.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public static bool Is64BitOperatingSystem()
        {
            // Check if this process is natively an x64 process. If it is, it will only run on x64 environments, thus, the environment must be x64.
            if (IntPtr.Size == 8)
                return true;
            // Check if this process is an x86 process running on an x64 environment.
            IntPtr moduleHandle = DllImport.GetModuleHandle("kernel32");
            if (moduleHandle != IntPtr.Zero)
            {
                IntPtr processAddress = DllImport.GetProcAddress(moduleHandle, "IsWow64Process");
                if (processAddress != IntPtr.Zero)
                {
                    bool result;
                    if (DllImport.IsWow64Process(DllImport.GetCurrentProcess(), out result) && result)
                        return true;
                }
            }
            // The environment must be an x86 environment.
            return false;
        }
        /// <summary>
        /// Line that await for new devices connected and refresh Variables.Devices_Connected
        /// </summary>
        public static void DeviceConnected()
        {
            try
            {
                var monitor = new DeviceMonitor(new AdbSocket(new IPEndPoint(IPAddress.Loopback, AdbClient.AdbServerPort)));
                monitor.DeviceConnected += OnDeviceConnected;
                monitor.DeviceDisconnected += Monitor_DeviceDisconnected;
                monitor.Start();
            }
            catch
            {
                Thread.Sleep(1000);
                DeviceConnected();
            }
        }

        private static void Monitor_DeviceDisconnected(object sender, DeviceDataEventArgs e)
        {
            Variables.Devices_Connected.Clear();
            Variables.Devices_Connected = AdbClient.Instance.GetDevices();
            Variables.DeviceChanged = true;
            Docked = false;
        }

        private static void OnDeviceConnected(object sender, DeviceDataEventArgs e)
        {
            
            Variables.Devices_Connected.Clear();
            Variables.Devices_Connected = AdbClient.Instance.GetDevices();
            Variables.DeviceChanged = true;
        }

        private void LoadEventBrowser()
        {
            try
            {
                int old = eventdetails;
                while (true)
                {
                    try
                    {
                        WebRequest request = WebRequest.Create("http://www-valkyriecrusade.nubee.com/event/sch/event_info/" + eventdetails + "_event.html");
                        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            break;
                        }
                        response.Close();
                        eventdetails++;
                    }
                    catch (WebException)
                    {
                        eventdetails--;
                        break;
                    }
                }
                if(eventdetails != old)
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        DllImport.DeleteUrlCacheEntry("http://www-valkyriecrusade.nubee.com/event/sch/event_info/" + eventdetails + "_event.html");
                        webBrowser2.Navigate(new Uri("http://www-valkyriecrusade.nubee.com/event/sch/event_info/" + eventdetails + "_event.html"));
                    });
                }

                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                webBrowser2.DocumentText = html;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if ((PrivateVariable.nospam - DateTime.Now).Duration() < TimeSpan.FromSeconds(5))
            {
                MessageBox.Show("啊！亚麻跌！慢点！好疼啊！");
                return;
            }
            PrivateVariable.nospam = DateTime.Now;
            PrivateVariable.Run = true;
            if (textBox2.Text.Length > 0)
            {
                try
                {
                    Convert.ToInt64(textBox2.Text);
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
                Width += 1104;
                panel3.Visible = true;
            }
            panel3.Enabled = false;
            PrivateVariable.AlwaysAttackNew = checkBox9.Checked;
            Variables.start = new Thread(Script.Bot);
            Variables.start.Start();
            Thread capture = new Thread(Capt);
            capture.Start();
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            richTextBox1.ScrollToCaret();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            PrivateVariable.Run = false;
            PrivateVariable.Battling = false;
            PrivateVariable.InEventScreen = false;
            PrivateVariable.InMainScreen = false;
            Variables.ScriptLog.Add("Script Stopped!");
            if(Width > 1054)
            {
                Width -= 1104;
                panel3.Visible = false;
            }
            if (EmulatorController.handle != null && Variables.Proc != null)
            {
                if (Variables.Instance.Length > 0)
                {
                    List<IntPtr> MEmu = DllImport.GetAllChildrenWindowHandles(IntPtr.Zero, "Qt5QWindowIcon", "(" + Variables.Instance + ")", 40);
                    foreach (var main in MEmu)
                    {
                        DllImport.SetParent(EmulatorController.handle, IntPtr.Zero);
                        DllImport.MoveWindow(EmulatorController.handle, 0, 0, 1280, 720, true);
                        DllImport.ShowWindow(main, 5);
                    }
                }
                else
                {
                    List<IntPtr> MEmu = DllImport.GetAllChildrenWindowHandles(IntPtr.Zero, "Qt5QWindowIcon", "MEmu", 40);
                    foreach (var main in MEmu)
                    {
                        DllImport.SetParent(EmulatorController.handle, IntPtr.Zero);
                        DllImport.MoveWindow(EmulatorController.handle, 0, 0, 1280, 720, true);
                        DllImport.ShowWindow(main, 5);
                    }
                }
           }
            Docked = false;
            Variables.start = null;
            button1.Enabled = true;
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            GC.Collect();
            var device = Variables.Devices_Connected.ToArray();
            int index = Array.IndexOf(device, PrivateVariable.Adb_IP);
            if (index > -1) //The Emulator is running
            {
                Variables.Control_Device_Num = index; //Register it, we need this to control our emulator
            }
            if (webBrowser2.DocumentText.Contains("楼层"))
            {
                groupBox8.Text = "塔楼活动";
                groupBox9.Text = "";
                progressBar1.Value = Script.energy;
                progressBar2.Value = Script.runes;
                label7.Text = Script.runes + "/5";
                label6.Text = Script.energy + "/5";
                if(Script.nextOnline != null)
                {
                    if(Script.nextOnline > DateTime.Now)
                    {
                        TimeSpan time = Script.nextOnline - DateTime.Now;
                        label9.Text = time.Hours + " : " + time.Minutes + " : " + time.Seconds;
                    }
                }
                PrivateVariable.EventType = 0;
            }
            else if (webBrowser2.DocumentText.Contains("魔女讨伐"))
            {
                groupBox8.Text = "";
                groupBox9.Text = "魔女讨伐";
                PrivateVariable.EventType = 1;
            }
            else
            {
                groupBox8.Text = "警告，无法获取活动资料";
                groupBox9.Text = "警告，无法获取活动资料";
                PrivateVariable.EventType = -1;
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
                string[] device = Variables.Devices_Connected.ConvertAll(x => x.ToString()).ToArray();
                int index = Array.IndexOf(device, PrivateVariable.Adb_IP);
                if (index > -1) //The Emulator is running
                {
                    if (Variables.Proc != null && Variables.Control_Device_Num > -1)
                    {
                        if (EmulatorController.handle == IntPtr.Zero || EmulatorController.handle == null)
                        {
                            EmulatorController.ConnectAndroidEmulator("Qt5QWindowIcon", "MainWindowWindow");
                        }

                            panel3.Invoke((MethodInvoker)delegate
                            {
                                if (DllImport.GetParent(EmulatorController.handle) != panel3.Handle)
                                {
                                    if (PrivateVariable.Run && !Docked)
                                    {
                                        DllImport.SetParent(EmulatorController.handle, panel3.Handle);
                                        DllImport.MoveWindow(EmulatorController.handle, -1, -30, 1104, 683, true);
                                        Docked = true;
                                    }
                                }
                            });
                        
                        
                        if (File.Exists(Variables.SharedPath + "\\" + Variables.Devices_Connected[Variables.Control_Device_Num].Name + ".dump"))
                        {
                            File.Delete(Variables.SharedPath + "\\" + Variables.Devices_Connected[Variables.Control_Device_Num].Name + ".dump");
                        }
                        try
                        {
                            byte[] newimage = EmulatorController.ImageCapture();
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
                        Variables.Control_Device_Num = index;
                        while (Variables.Proc == null)
                        {
                            Thread.Sleep(1000);
                        }
                        Variables.ScriptLog.Add("Emulator Started");
                    }
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            EmulatorController.StartEmulator();
            EmulatorController.StartAdb();
            
            Stopwatch s = new Stopwatch();
            s.Start();
            byte[] image = EmulatorController.ImageCapture();
            if (image == null)
            {
                MessageBox.Show("截图失败！图片为null！");
                return;
            }
            Image temp = EmulatorController.Decompress(image);
            temp.Save("debug.png");
            s.Stop();
            if (s.ElapsedMilliseconds > 1000)
            {
                MessageBox.Show("截图使用了" + s.ElapsedMilliseconds / 1000 + "秒！");
            }
            else
            {
                MessageBox.Show("截图使用了" + s.ElapsedMilliseconds + "毫秒！");
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

        private void button5_Click(object sender, EventArgs e)
        {
            var lines = File.ReadAllLines("bot.ini");
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
                    if (key == "background")
                    {
                        lines[x] = "background=true";
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
                    if (key == "background")
                    {
                        lines[x] = "background=false";
                        break;
                    }
                    x++;
                }
            }
            if (checkBox9.Checked)
            {
                int x = 0;
                foreach (var l in lines)
                {
                    string key = l.Split('=')[0];
                    if (key == "Archwitch_New")
                    {
                        lines[x] = "Archwitch_New=true";
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
                    if (key == "Archwitch_New")
                    {
                        lines[x] = "Archwitch_New=false";
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
            File.WriteAllLines("bot.ini", lines);
            if (PrivateVariable.Run)
            {
                button3_Click(sender, e);
            }
            Environment.Exit(0);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            EmulatorController.StartEmulator();
            EmulatorController.StartAdb();
            
            EmulatorController.SendSwipe(300, 300, 800, 300, 1000);
            EmulatorController.SendSwipe(800, 300, 300, 300, 1000);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Download.Url = "http://dl.memuplay.com/download/backup/Memu-Setup-3.7.0.0.exe";
            Download d = new Download();
            d.Show();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            EmulatorController.StartEmulator();
            EmulatorController.StartAdb();
            Thread.Sleep(10000);
            Download.Url = "https://github.com/PoH98/Bot/raw/master/神女控.apk";
            Download d = new Download();
            d.Show();
        }

        private void button5_MouseEnter(object sender, EventArgs e)
        {
            button5.BackColor = Color.Red;
        }

        private void button5_MouseLeave(object sender, EventArgs e)
        {
            button5.BackColor = Color.Black;
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

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            Variables.Background = checkBox1.Checked;
        }

        private void checkBox7_CheckedChanged(object sender, EventArgs e)
        {
            textBox3.Visible = checkBox7.Checked;
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
            Variables.Configure.Add("Double_Event", checkBox8.Checked.ToString().ToLower());
        }

        private void checkBox8_MouseEnter(object sender, EventArgs e)
        {
            toolTip1.Show("如果在主城页面的左边选择活动区域拥有多个活动，影响挂机进入塔楼/魔女，请打勾这个！请慎用！",checkBox8);
        }

        private void checkBox8_MouseLeave(object sender, EventArgs e)
        {
            toolTip1.Hide(checkBox8);
        }

        private void checkBox9_CheckedChanged(object sender, EventArgs e)
        {
            PrivateVariable.AlwaysAttackNew = checkBox9.Checked;
            WriteConfig("Archwitch_New", checkBox9.Checked.ToString().ToLower());
        }

        private void checkBox10_CheckedChanged(object sender, EventArgs e)
        {
            PrivateVariable.CustomScript = checkBox10.Checked;
            WriteConfig("CustomScript",checkBox9.Checked.ToString().ToLower());
        }

        private void button9_Click(object sender, EventArgs e)
        {
            if (File.Exists("battlescript.txt"))
            {
                Process.Start("battlescript.txt");
            }
            else
            {
                File.WriteAllText("battlescript.txt","重返| | ");
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
                    webBrowser3.Visible = true;
                }
            }
                
        }

        private void radioButton7_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton7.Checked)
            {

            }
        }

        private void radioButton6_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton6.Checked)
            {
                if (checkBox11.Checked)
                {
                    Process.Start("http://www-valkyriecrusade.nubee.com/event/sch/event_info/" + eventdetails + "_event.html");
                }
                else
                {
                    webBrowser3.Visible = false;
                }
                
            }
             
        }

        private void button11_Click_1(object sender, EventArgs e)
        {
            if (webBrowser3.Visible)
            {
                webBrowser3.Refresh();
            }
            else
            {
                webBrowser2.Refresh();
            }

        }

        private void webBrowser2_NewWindow(object sender, System.ComponentModel.CancelEventArgs e)
        {
            webBrowser2.Navigate(webBrowser2.StatusText);
            e.Cancel = true;
        }

        private void button12_Click(object sender, EventArgs e)
        {
            if (webBrowser3.Visible)
            {
                webBrowser3.GoBack();
            }
            else
            {
                webBrowser2.GoBack();
            }

        }

        private void button13_Click(object sender, EventArgs e)
        {
            if (webBrowser3.Visible)
            {
                webBrowser3.GoForward();
            }
            else
            {
                webBrowser2.GoForward();
            }

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

        private void button14_Click(object sender, EventArgs e)
        {
            ProcessStartInfo fetch = new ProcessStartInfo(Variables.VBoxManagerPath + "\\MEmuManage.exe");
            if (Variables.Instance.Length > 0)
            {
                fetch.Arguments = "showvminfo " + Variables.Instance;
            }
            else
            {
                fetch.Arguments = "showvminfo MEmu";
            }
            fetch.RedirectStandardOutput = true;
            fetch.UseShellExecute = false;
            fetch.CreateNoWindow = true;
            fetch.StandardOutputEncoding = System.Text.Encoding.ASCII;
            Process fetching = Process.Start(fetch);
            string result = fetching.StandardOutput.ReadToEnd();
            File.WriteAllText("debug.txt", result);
            Process.Start("debug.txt");
            
        }

        private void button15_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void button15_MouseEnter(object sender, EventArgs e)
        {
            if(sender is Button)
            {
                Button btn = (Button)sender;
                btn.BackColor = Color.Gray;
            }

        }

        private void button15_MouseLeave(object sender, EventArgs e)
        {
            if (sender is Button)
            {
                Button btn = (Button)sender;
                btn.BackColor = Color.Black;
            }
        }

        private void button16_Click(object sender, EventArgs e)
        {
            if (Width > 1280)
            {
                Width -= 1104;
                panel3.Visible = false;
                button16.Text = ">";
            }
            else
            {
                Width += 1104;
                panel3.Visible = true;
                button16.Text = "<";
            }
        }

        private void radioButton9_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton9.Checked)
            {
                PrivateVariable.EnterRune = true;
                WriteConfig("Manual_Rune", "false");
            }
            
        }

        private void radioButton10_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton10.Checked)
            {
                PrivateVariable.EnterRune = false;
                WriteConfig("Manual_Rune", "true");
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
                WriteConfig("Level","0");
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
            {
                if (checkBox11.Checked)
                {
                    Process.Start("https://jq.qq.com/?_wv=1027&k=51gVT8A");
                }
                else
                {
                    webBrowser3.Navigate("https://jq.qq.com/?_wv=1027&k=51gVT8A");
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Thread t = new Thread(Test);
            t.Start();
        }

        private static void Test()
        {
            EmulatorController.CloseEmulator("MEmuManage.exe");
        }

        private static void WriteConfig(string key, string value)
        {
            var config = File.ReadAllLines("bot.ini");
            int x = 0;
            foreach (var c in config)
            {
                if (c.Contains(key + "="))
                {
                    config[x] = key + "=" + value;
                    File.WriteAllLines("bot.ini", config);
                    return;
                }
                x++;
            }
            config[config.Length - 1] = config[config.Length - 1] + "\n" + key + "=" + value;
            File.WriteAllLines("bot.ini", config);
        }
    }
}
