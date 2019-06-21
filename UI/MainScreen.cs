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
using System.Text;
using System.Security.Principal;
using ImgXml;
using System.Collections.Generic;
using MetroFramework.Forms;
using System.Windows.Forms;
using System.Linq;

namespace BotFramework
{
    public partial class MainScreen : MetroForm
    {

        private static string html;

        public static int Level;

        static bool Docked = false;

        static System.Windows.Forms.Timer timeout = new System.Windows.Forms.Timer();

        static List<CheckBox> customScriptEnable = new List<CheckBox>();

        public static Dictionary<string, string> UILanguage = new Dictionary<string, string>();
        public MainScreen()
        {
            InitializeComponent();
            Debug_.PrepairDebug(true);
            Variables.richTextBox = richTextBox1;
            timeout.Interval = 15000;
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

        private void loading()
        {
            Login load = new Login();
            load.ShowDialog();
            if(load.DialogResult == DialogResult.Abort)
            {
                Environment.Exit(0);
            }
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
            return "4.5+";
        }

        private void MainScreen_Load(object sender, EventArgs e)
        {
            Thread load = new Thread(loading);
            load.SetApartmentState(ApartmentState.STA);
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
            comboBox1.Items.Clear();
            Variables.Background = true;
            OCR.PrepairOcr(whitelist: "$0123456789", blacklist: "!?@#$%&*()<>_-+=/:;'\"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz");
            string output = "";
            Variables.EmulatorPath();
            if (File.Exists("bot.ini"))
            {
                if (File.Exists(Environment.CurrentDirectory + "\\Profiles\\" + BotCore.profilePath + "\\bot.ini"))
                {
                    File.Delete(Environment.CurrentDirectory + "\\Profiles\\" + BotCore.profilePath + "\\bot.ini");
                }
                try
                {
                    File.Copy("bot.ini", Environment.CurrentDirectory + "\\Profiles\\" + BotCore.profilePath + "\\bot.ini", true);
                    File.Delete("bot.ini");
                }
                catch
                {

                }
            }
            BotCore.ReadConfig();
            string _NET = Get45PlusFromRegistry();
            if (!Directory.Exists("Language"))
            {
                MessageBox.Show("Lost files, please reinstall the bot!");
                Environment.Exit(0);
            }
            foreach(var lang in Directory.GetFiles("Language"))
            {
                comboBox1.Items.Add(lang.Replace("Language\\","").Replace(".ini",""));
            }
            if (Variables.Configure.TryGetValue("Lang", out output))
            {
                int index = comboBox1.Items.IndexOf(output);
                if (index < 0)
                {
                    comboBox1.SelectedIndex = 0;
                }
                else
                {
                    comboBox1.SelectedIndex = index;
                }
            }
            else
            {
                comboBox1.SelectedIndex = 0;
            }
            label3.Text = Variables.VBoxManagerPath;
            label4.Text = Variables.SharedPath;
            if (Variables.Configure.TryGetValue("Level", out output))
            {
                switch (output)
                {
                    case "0":
                        chk_begin.Checked = true;
                        break;
                    case "1":
                        chk_inter.Checked = true;
                        break;
                    case "2":
                        chk_advan.Checked = true;
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
                    chk_twoE.Checked = true;
                }
            }
            if (Variables.Configure.TryGetValue("Manual_Rune", out output))
            {
                if (output == "true")
                {
                    chk_manuRT.Checked = true;
                }
            }
            if(Variables.Configure.TryGetValue("Suspend_PC",out output))
            {
                if(output == "true")
                {
                    Suspend_Chk.Checked = true;
                }
            }
            webBrowser1.ScriptErrorsSuppressed = true;
            webBrowser3.ScriptErrorsSuppressed = true;
            PrivateVariable.nospam = DateTime.Now;
            string ua = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/60.0.3112.113 Safari/537.36";
            DllImport.UrlMkSetSessionOption(DllImport.URLMON_OPTION_USERAGENT, ua, ua.Length, 0);
            webBrowser3.Navigating += OnNavigating;
            webBrowser3.Navigated += WebBrowser3_Navigated;
            GetEventXML.LoadXMLEvent();
            webBrowser3.Navigate(new Uri("http://www-valkyriecrusade.nubee.com/" + GetEventXML.Eventlink.Replace("/en/", "/"+ comboBox1.SelectedItem.ToString() +"/") + ".html"));
            VCBotScript.Read_Plugins();
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
            chk_item.Enabled = chk_autoRT.Checked;
            Login.LoadCompleted = true;
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
                return;
            }
            PrivateVariable.nospam = DateTime.Now;
            richTextBox1.Text = "";
            btn_Start.Enabled = false;
            if (chk_begin.Checked)
            {
                Level = 0;
            }
            else if (chk_inter.Checked)
            {
                Level = 1;
            }
            else if (chk_advan.Checked)
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
                    PrivateVariable.Skills.Add(BotCore.Compress(image));
                }
            }
            if (panel3.Visible == false)
            {
                Width += 700;
                panel3.Visible = true;
            }
            panel3.Enabled = false;
            ScriptRun.RunScript(true, (new VCBotScript() as ScriptInterface));
            Thread cap = new Thread(Capt);
            cap.Start();
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            richTextBox1.ScrollToCaret();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            PrivateVariable.nospam = DateTime.Now;
            PrivateVariable.Battling = false;
            PrivateVariable.InEventScreen = false;
            PrivateVariable.InMainScreen = false;
            PrivateVariable.InMap = false;
            BotCore.EjectSockets();
            Variables.ScriptLog("Script Stopped!",Color.White);
            if (Width > 480)
            {
                Width -= 700;
                panel3.Visible = false;
            }
            if (Variables.Proc != null)
            {
                DllImport.SetParent(Variables.Proc.MainWindowHandle, IntPtr.Zero);
                DllImport.MoveWindow(Variables.Proc.MainWindowHandle, PrivateVariable.EmuDefaultLocation.X, PrivateVariable.EmuDefaultLocation.Y, 1318, 752, true);
                Docked = false;
            }
            ScriptRun.StopScript();
            btn_Start.Enabled = true;
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            GC.Collect();
            if (PrivateVariable.EventType == 0)
            {
                ED_Box.Text = UILanguage["Tower"];
                lbl_Rune.Text = UILanguage["Rune_Tower"];
                progressBar1.Value = VCBotScript.energy;
                progressBar2.Value = VCBotScript.runes;
                label7.Text = VCBotScript.runes + "/5";
                label6.Text = VCBotScript.energy + "/5";
                if (VCBotScript.nextOnline != null)
                {
                    if (VCBotScript.nextOnline > DateTime.Now)
                    {
                        TimeSpan time = VCBotScript.nextOnline - DateTime.Now;
                        label9.Text = time.Hours.ToString("00") + " : " + time.Minutes.ToString("00") + " : " + time.Seconds.ToString("00");
                    }
                }
                if (VCBotScript.Tower_Floor.Length > 0)
                {
                    try
                    {
                        label15.Text = VCBotScript.Tower_Floor.Replace(" ", "").Replace("F", " F");
                    }
                    catch
                    {

                    }
                }
                if (VCBotScript.Tower_Rank.Length > 0)
                {
                    try
                    {
                        label16.Text = VCBotScript.Tower_Rank.Replace(" ", "");
                    }
                    catch
                    {

                    }
                }
            }
            else if (PrivateVariable.EventType == 2)
            {
                ED_Box.Text = UILanguage["Demon"];
                lbl_Rune.Text = UILanguage["Rune_Demon"];
                label7.Text = VCBotScript.runes + "/4";
                label6.Text = VCBotScript.energy + "/5";
                progressBar2.Maximum = 4;
                progressBar1.Value = VCBotScript.energy;
                progressBar2.Value = VCBotScript.runes;
                if (VCBotScript.nextOnline != null)
                {
                    if (VCBotScript.nextOnline > DateTime.Now)
                    {
                        TimeSpan time = VCBotScript.nextOnline - DateTime.Now;
                        label9.Text = time.Hours.ToString("00") + " : " + time.Minutes.ToString("00") + " : " + time.Seconds.ToString("00");
                    }
                }
                if (VCBotScript.Tower_Floor.Length > 0)
                {
                    try
                    {
                        label15.Text = VCBotScript.Tower_Floor.Replace(" ", "").Replace("F", " F");
                    }
                    catch
                    {

                    }
                }
                if (VCBotScript.Tower_Rank.Length > 0)
                {
                    try
                    {
                        label16.Text = VCBotScript.Tower_Rank.Replace(" ", "");
                    }
                    catch
                    {

                    }
                }
            }
            else
            {
                ED_Box.Text = UILanguage["Unknown"];
            }
        }
        /// <summary>
        /// Capture loop
        /// </summary>
        
        private void Capt()
        {
            do
            {
                Thread.Sleep(1000);
                if (Variables.Proc != null)
                {
                    if (Variables.Proc.HasExited)
                    {
                        Variables.Proc = null;
                        Docked = false;
                        continue;
                    }
                    if (Variables.emulator.EmulatorName() == "Nox")
                    {
                        var hide = DllImport.GetAllChildrenWindowHandles(IntPtr.Zero, "Qt5QWindowToolSaveBits", "Form", 2);
                        foreach (var h in hide)
                        {
                            DllImport.ShowWindow(h, 0);
                        }
                    }
                    try
                    {
                        var handle = Variables.Proc.MainWindowHandle;
                        var parent = DllImport.GetParent(handle);
                        DllImport.Rect rect = new DllImport.Rect();
                        DllImport.GetWindowRect(handle, ref rect);
                        if (!Docked)
                        {
                            if (!ScriptRun.Run)
                            {
                                return;
                            }
                            PrivateVariable.EmuDefaultLocation = new Point(rect.left, rect.top);
                            panel3.Invoke((MethodInvoker)delegate
                            {
                                DllImport.SetParent(Variables.Proc.MainWindowHandle, panel3.Handle);
                            });
                            Docked = true;
                        }
                        if (rect.left != -1 || rect.top != -55)
                        {
                            DllImport.MoveWindow(Variables.Proc.MainWindowHandle, -1, -30, 736, 600, false);
                        }
                    }
                    catch
                    {

                    }
                }
                else
                {
                    Docked = false;
                }
            } while (ScriptRun.Run);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (File.Exists("Updater.exe"))
            {
                Process.Start("Updater.exe", "http://dl.memuplay.com/download/backup/Memu-Setup-3.7.0.0.exe");
            }
        }

        private void checkBox8_CheckedChanged(object sender, EventArgs e)
        {
            WriteConfig("Double_Event", chk_twoE.Checked.ToString().ToLower());
            if (chk_twoE.Checked && !File.Exists("Img\\Event.png"))
            {
                MessageBox.Show("请截图好活动的进入按钮样貌，保存到Img文件夹内，命名为Event.png!");
            }
        }

        private void checkBox8_MouseEnter(object sender, EventArgs e)
        {
            toolTip1.Show("如果在主城页面的左边选择活动区域拥有多个活动，影响挂机进入塔楼/魔女，请打勾这个！请慎用！", chk_twoE);
        }

        private void checkBox8_MouseLeave(object sender, EventArgs e)
        {
            toolTip1.Hide(chk_twoE);
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

        private void radioButton9_CheckedChanged(object sender, EventArgs e)
        {
            if (chk_autoRT.Checked)
            {
                WriteConfig("Manual_Rune", "false");
                chk_item.Enabled = chk_autoRT.Checked;
            }
        }

        private void radioButton10_CheckedChanged(object sender, EventArgs e)
        {
            if (chk_manuRT.Checked)
            {
                WriteConfig("Manual_Rune", "true");
                chk_item.Checked = false;
                chk_item.Enabled = false;
            }
        }

        private void webBrowser3_NewWindow(object sender, System.ComponentModel.CancelEventArgs e)
        {
            webBrowser3.Navigate(webBrowser3.StatusText);
            e.Cancel = true;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (chk_begin.Checked)
            {
                WriteConfig("Level", "0");
                Level = 0;
            }
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (chk_inter.Checked)
            {
                WriteConfig("Level", "1");
                Level = 1;
            }
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (chk_advan.Checked)
            {
                WriteConfig("Level", "2");
                Level = 2;
            }
        }

        private void checkBox10_CheckedChanged(object sender, EventArgs e)
        {
            WriteConfig("Use_Item", chk_item.Checked.ToString().ToLower());
            PrivateVariable.Use_Item = chk_item.Checked;
        }


        private static void WriteConfig(string key, string value)
        {
            if(!Directory.Exists("Profiles\\" + BotCore.profilePath))
            {
                Directory.CreateDirectory("Profiles\\" + BotCore.profilePath);
            }
            if (File.Exists("Profiles\\" + BotCore.profilePath + "\\bot.ini"))
            {
                var config = File.ReadAllLines("Profiles\\" + BotCore.profilePath + "\\bot.ini");
                int x = 0;
                foreach (var c in config)
                {
                    if (c.Contains(key + "="))
                    {
                        config[x] = key + "=" + value;
                        File.WriteAllLines("Profiles\\" + BotCore.profilePath + "\\bot.ini", config);
                        return;
                    }
                    x++;
                }
                config[config.Length - 1] = config[config.Length - 1] + "\n" + key + "=" + value;
                            File.WriteAllLines("Profiles\\" + BotCore.profilePath + "\\bot.ini", config);
            }
            if (Variables.Configure.ContainsKey(key))
            {
                Variables.Configure[key] = value;
            }
            else
            {
                Variables.Configure.Add(key, value);
            }
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
            if (chk_extre.Checked)
            {
                WriteConfig("Level", "3");
                Level = 3;
            }
        }

        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {
            if (chk_ultim.Checked)
            {
                WriteConfig("Level", "4");
                Level = 4;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (File.Exists("OCR.png"))
            {
                var img = BotCore.Compress(Image.FromFile("OCR.png"));
                MessageBox.Show(OCR.OcrImage(img, "eng"));
            }
        }

        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            Variables.AdvanceLogShow = chk_Log.Checked;
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
                webBrowser3.Document.BackColor = Color.FromArgb(445561);
            }
            webBrowser3.Show();
        }

        private void MainScreen_FormClosing(object sender, FormClosingEventArgs e)
        {
            var lines = File.ReadAllLines("Profiles\\" + BotCore.profilePath + "\\bot.ini");
            if (chk_begin.Checked)
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
            else if (chk_inter.Checked)
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
            else if (chk_advan.Checked)
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
            File.WriteAllLines("Profiles\\" + BotCore.profilePath + "\\bot.ini", lines);
            if (ScriptRun.Run)
            {
                button3_Click(sender, e);
            }
            Environment.Exit(0);
        }
        
        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            WriteConfig("Lang", comboBox1.SelectedItem.ToString());
            ChangeLanguage(comboBox1.SelectedItem.ToString(), this);
            html = Img.index;
            WebClientOverride wc = new WebClientOverride();
            try
            {
                html = wc.DownloadString(new Uri("https://d2n1d3zrlbtx8o.cloudfront.net/news/info/" + comboBox1.SelectedItem.ToString() + "/index.html"));
                html = html.Replace("bgcolor=\"#000000\"　text color=\"#FFFFFF\"", "style=\"background - color:#303030; color:white\"");
                html = html.Remove(html.IndexOf("<table width=\"200\">"), html.IndexOf("</table>") - html.IndexOf("<table width=\"200\">"));
                html = Regex.Replace(html, "(\\<span class\\=\"iro4\"\\>.*</span>)", "");
            }
            catch
            {

            }
            webBrowser1.DocumentText = html;
        }
        public void ChangeLanguage(string language, Control form)
        {
            if (!Directory.Exists("Language"))
            {
                MessageBox.Show("No language files located! Please redownload the bot!");
                Environment.Exit(0);
            }
            if (File.Exists(Environment.CurrentDirectory + "\\Language\\" + language + ".ini"))
            {
                var buffer = File.ReadAllLines(Environment.CurrentDirectory + "\\Language\\" + language + ".ini",Encoding.Unicode);
                foreach (var line in buffer)
                {
                    var split = line.Split('=');
                    if (split.Length > 1)
                    {
                        if (UILanguage.Keys.Contains(split[0]))
                        {
                            UILanguage[split[0]] = split[1];
                        }
                        else
                        {
                            UILanguage.Add(split[0], split[1]);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Language " + language + " not found! No change is applied!");
                Environment.Exit(0);
            }
            changeLang(form);
        }
        private void changeLang(Control form)
        {
            foreach (Control control in form.Controls)
            {
                string text = "";
                if (UILanguage.TryGetValue(control.Name, out text))
                {
                    control.Text = text;
                }
                if (control.Controls.Count > 0)
                {
                    changeLang(control);
                }
            }
        }

        private void WebBrowser3_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            webBrowser3.Hide();
        }

        private void Rdb_event_Click(object sender, EventArgs e)
        {
            if (rdb_event.Checked)
            {
                if (chk_browser.Checked)
                {
                    Process.Start("http://www-valkyriecrusade.nubee.com/" + GetEventXML.Eventlink.Replace("/en/", "/" + comboBox1.SelectedItem.ToString() + "/") + ".html");
                }
                else
                {
                    webBrowser3.Navigate("http://www-valkyriecrusade.nubee.com/" + GetEventXML.Eventlink.Replace("/en/", "/" + comboBox1.SelectedItem.ToString() + "/") + ".html");
                }

            }
        }

        private void Rdb_card_Click(object sender, EventArgs e)
        {
            if (comboBox1.Text == "sch")
            {
                if (rdb_card.Checked)
                {
                    if (chk_browser.Checked)
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
            else
            {
                if (rdb_card.Checked)
                {
                    Process.Start("https://valkyriecrusade.fandom.com/wiki/Category:Cards");
                }
            }
        }

        private void Rbd_help_Click(object sender, EventArgs e)
        {
            if (rbd_help.Checked)
            {
                if (chk_browser.Checked)
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

        private void MetroButton1_Click(object sender, EventArgs e)
        {
            SleepWake.SetWakeTimer(DateTime.Now.AddMinutes(1));
            PCController.DoMouseClick(100,100);
        }

        private void Suspend_Chk_CheckedChanged(object sender, EventArgs e)
        {
            WriteConfig("Suspend_PC",Suspend_Chk.Checked.ToString().ToLower());
        }
    }
}
