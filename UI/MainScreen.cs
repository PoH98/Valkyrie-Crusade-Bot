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
using System.IO.Compression;
using System.Runtime.InteropServices;

namespace BotFramework
{
    public partial class MainScreen : MetroForm
    {

        public static string html;

        public static int Level;

        static readonly System.Windows.Forms.Timer timeout = new System.Windows.Forms.Timer();

        static readonly List<CheckBox> customScriptEnable = new List<CheckBox>();

        public static Dictionary<string, string> UILanguage = new Dictionary<string, string>();

        private static Point loc;

        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool InternetSetCookie(string lpszUrlName, string lpszCookieName, string lpszCookieData);

        private Thread cap;

        private static TransparentPanel tp;
        [DllImport("kernel32.dll",
            EntryPoint = "GetStdHandle",
            SetLastError = true,
            CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr GetStdHandle(int nStdHandle);
        [DllImport("kernel32.dll",
            EntryPoint = "AllocConsole",
            SetLastError = true,
            CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall)]
        private static extern int AllocConsole();
        private const int STD_OUTPUT_HANDLE = -11;
        private const int MY_CODE_PAGE = 437;

        public MainScreen()
        {
            InitializeComponent();
            Debug_.PrepairDebug(false);
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

        private void Loading()
        {
            Login load = new Login();
            load.ShowDialog();
            if (load.DialogResult == DialogResult.Abort)
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
            Thread load = new Thread(Loading);
            load.SetApartmentState(ApartmentState.STA);
            load.Start();
            if (!IsRunAsAdministrator())
            {
                var processInfo = new ProcessStartInfo(Assembly.GetExecutingAssembly().CodeBase)
                {
                    // The following properties run the new process as administrator
                    UseShellExecute = true,
                    Verb = "runas"
                };
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
            if (!Directory.Exists("Img"))
            {
                File.WriteAllBytes("Img.zip", Img.Images);
                ZipFile.ExtractToDirectory("Img.zip", Environment.CurrentDirectory);
                File.Delete("Img.zip");
            }
            comboBox1.Items.Clear();
            OCR.PrepairOcr(whitelist: "$0123456789", blacklist: "!?@#$%&*()<>_-+=/:;'\"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz");
            Variables.LoadEmulator();
            if (File.Exists("bot.ini"))
            {
                if (File.Exists(Environment.CurrentDirectory + "\\Profiles\\" + AdbInstance.Instance.profilePath + "\\bot.ini"))
                {
                    File.Delete(Environment.CurrentDirectory + "\\Profiles\\" + AdbInstance.Instance.profilePath + "\\bot.ini");
                }
                try
                {
                    File.Copy("bot.ini", Environment.CurrentDirectory + "\\Profiles\\" + AdbInstance.Instance.profilePath + "\\bot.ini", true);
                    File.Delete("bot.ini");
                }
                catch
                {

                }
            }
            Variables.ReadConfig();
            CheckVersion.CheckUpdate();
            string _NET = Get45PlusFromRegistry();
            if (!Directory.Exists("Language"))
            {
                MessageBox.Show("Lost files, please reinstall the bot!");
                Environment.Exit(0);
            }
            foreach (var lang in Directory.GetFiles("Language"))
            {
                comboBox1.Items.Add(lang.Replace("Language\\", "").Replace(".ini", ""));
            }
            if (Variables.FindConfig("General", "Lang", out string output))
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
            if (Variables.FindConfig("General", "Level", out output))
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
                    case "3":
                        chk_extre.Checked = true;
                        break;
                    case "4":
                        chk_ultim.Checked = true;
                        break;
                }
            }
            else
            {
                Variables.ModifyConfig("General", "Level", "0");
            }
            if (Variables.FindConfig("General", "Double_Event", out output))
            {
                if (output == "true")
                {
                    chk_twoE.Checked = true;
                }
            }
            if (Variables.FindConfig("General", "Manual_Rune", out output))
            {
                if (output == "true")
                {
                    chk_manuRT.Checked = true;
                }
            }
            if (Variables.FindConfig("General", "Suspend_PC", out output))
            {
                if (output == "true")
                {
                    Suspend_Chk.Checked = true;
                }
            }
            if (Variables.FindConfig("General", "biubiu", out output))
            {
                if (output == "true")
                {
                    Biubiu.Checked = true;
                }
            }
            if (Variables.FindConfig("General", "ArWiEv", out output))
            {
                if (output == "true")
                {
                    Chk_Archwitch.Checked = true;
                }
            }
            if (Variables.FindConfig("General", "SoulEv", out output))
            {
                if (output == "true")
                {
                    chk_SoulEv.Checked = true;
                }
            }
            if (Variables.FindConfig("General", "ArWiSt", out output))
            {
                string temp = output.Replace(".", "-");
                Combo_Archwitch.SelectedItem = temp;
            }
            else
            {
                Combo_Archwitch.SelectedIndex = 0;
            }
            if (Variables.FindConfig("General", "SoWeSt", out output))
            {
                string temp = output.Replace(".", "-");
                Combo_Weapon.SelectedItem = temp;
            }
            else
            {
                Combo_Weapon.SelectedIndex = 0;
            }
            if(Variables.FindConfig("GuildWar", "Manual", out output))
            {
                if(output == "true")
                {
                    chk_GWW.Checked = true;
                }
            }
            if (Variables.ForceWinApiCapt)
            {
                WinAPi.Checked = true;
                WinAPi.Enabled = false;
            }
            else
            {
                if (Variables.FindConfig("General", "WinApi", out output))
                {
                    if (output == "true")
                    {
                        WinAPi.Checked = true;
                    }
                }
            }
            if (Variables.FindConfig("GuildWar", "Manual", out output))
            {
                if (output == "true")
                {
                    chk_GWW.Checked = true;
                }
            }
            try
            {
                if (Variables.FindConfig("Version", "Version", out output))
                {
                    if (output != CheckVersion.currentVersion)
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (var line in CheckVersion.BufferUpdateText)
                        {
                            sb.Append(line + "\n");
                        }
                        CheckVersion.UpdateText = "# Thanks for supporting VCBot! \n" + sb.ToString();
                        Variables.ModifyConfig("Version", "Version", CheckVersion.currentVersion);
                    }
                }
                else
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var line in CheckVersion.BufferUpdateText)
                    {
                        sb.Append(line + "\n");
                    }
                    CheckVersion.UpdateText = "# Thanks for supporting VCBot! \n" + sb.ToString();
                    Variables.ModifyConfig("Version", "Version", CheckVersion.currentVersion);
                }
            }
            catch
            {

            }

            webBrowser1.ScriptErrorsSuppressed = true;
            webBrowser3.ScriptErrorsSuppressed = true;
            webBrowser2.ScriptErrorsSuppressed = true;
            PrivateVariable.Instance.nospam = DateTime.Now;
            string ua = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/60.0.3112.113 Safari/537.36";
            DllImport.UrlMkSetSessionOption(DllImport.URLMON_OPTION_USERAGENT, ua, ua.Length, 0);
            webBrowser3.Navigating += OnNavigating;
            webBrowser3.Navigated += WebBrowser3_Navigated;
            GetEventXML.LoadXMLEvent();
            try
            {
                if (comboBox1.SelectedItem.ToString().Contains("ch"))
                {
                    webBrowser3.Navigate(new Uri("http://www-valkyriecrusade.nubee.com/" + GetEventXML.Eventlink.Replace("/en/", "/sch/") + ".html"));
                }
                else
                {
                    webBrowser3.Navigate(new Uri("http://www-valkyriecrusade.nubee.com/" + GetEventXML.Eventlink + ".html"));
                }
                if (comboBox1.SelectedItem.ToString().Contains("ch"))
                {
                    webBrowser2.Navigate(new Uri("http://www-valkyriecrusade.nubee.com/" + GetEventXML.GuildwarLink.Replace("/en/", "/sch/") + ".html"));
                }
                else
                {
                    webBrowser2.Navigate(new Uri("http://www-valkyriecrusade.nubee.com/" + GetEventXML.GuildwarLink + ".html"));
                }
                webBrowser2.DocumentCompleted += (sender1, e1) =>
                {
                    webBrowser2.Document.Body.MouseEnter += (sender2, e2) =>
                    {
                        webBrowser2.Dock = DockStyle.Fill;
                    };
                    webBrowser2.Document.Body.MouseLeave += (sender2, e2) =>
                    {
                        webBrowser2.Dock = DockStyle.None;
                    };
                };
            }
            catch
            {
                webBrowser2.Visible = false;
            }

            VCBotScript.Read_Plugins();
            foreach (var s in PrivateVariable.Instance.BattleScript)
            {
                tabControl2.TabPages.Add(s.ScriptName());
                tabControl2.TabPages[tabControl2.TabPages.Count - 1].BackColor = Color.Black;
                tabControl2.TabPages[tabControl2.TabPages.Count - 1].ForeColor = Color.White;
                CheckBox chk = new CheckBox
                {
                    Text = "使用脚本",
                    Checked = false
                };
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
            if (Variables.FindConfig("General", "Selected_Script", out string n))
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
            foreach (Control box in ED_Box.Controls)
            {
                foreach (Control control in box.Controls)
                {
                    control.MouseDown += Tp_MouseDown;
                }
                box.MouseDown += Tp_MouseDown;
            }
            Text = "VCBot (" + Variables.emulator.GetType().Name + ")";
            label2.Text = Variables.AdbIpPort;
            Refresh();
            ED_Box.MouseDown += Tp_MouseDown;
            metroTabControl1.SelectedIndex = 0;
            chk_item.Enabled = chk_autoRT.Checked;
            Login.LoadCompleted = true;
            PrivateVariable.Instance.VCevent = PrivateVariable.EventType.Unknown;
            timer2.Start();
        }

        private void Chk_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox ck = sender as CheckBox;
            if (ck.Checked)
            {
                PrivateVariable.Instance.Selected_Script = customScriptEnable.IndexOf(ck);
                Variables.ModifyConfig("General", "Selected_Script", PrivateVariable.Instance.Selected_Script.ToString());
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
        private void Button1_Click(object sender, EventArgs e)
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
            if ((PrivateVariable.Instance.nospam - DateTime.Now).Duration() < TimeSpan.FromSeconds(1))
            {
                return;
            }
            PrivateVariable.Instance.nospam = DateTime.Now;
            Variables.ProchWnd = panel3.Handle;
            richTextBox1.Text = "";
            btn_Start.Enabled = false;
            PrivateVariable.Instance.Skills.Clear();
            var files = Directory.GetFiles("Img\\Star");
            foreach (var f in files)
            {
                using (Stream bmp = File.Open(f, FileMode.Open))
                {
                    Image image = Image.FromStream(bmp);
                    PrivateVariable.Instance.Skills.Add(Screenshot.Compress(image as Bitmap));
                }
            }
            if (panel3.Visible == false)
            {
                Width += 1280;
                panel3.Visible = true;
            }
            panel3.Enabled = false;
            tp = new TransparentPanel
            {
                Location = panel3.Location,
                Size = panel3.Size,
                Enabled = true,
                Visible = true
            };
            tp.MouseDown += Tp_MouseDown;
            Controls.Add(tp);
            foreach (Control control in Debug.Controls)
            {
                if (control is Button)
                {
                    control.Enabled = false;
                }
            }
            ScriptRun.RunScript(true, (new VCBotScript() as ScriptInterface));
            cap = new Thread(Capt);
            cap.Start();
        }

        private void Tp_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                DllImport.ReleaseCapture();
                DllImport.SendMessage(Handle, 0xA1, 0x2, 0);
            }
        }

        bool scroll = true;
        private void RichTextBox1_TextChanged(object sender, EventArgs e)
        {
            if (scroll)
            {
                richTextBox1.ScrollToCaret();
            }
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            if(cap != null)
            {
                try
                {
                    if (cap.IsAlive)
                    {
                        cap.Abort();
                    }
                }
                catch
                {

                }
            }
            PrivateVariable.Instance.nospam = DateTime.Now;
            PrivateVariable.Instance.Battling = false;
            PrivateVariable.Instance.InEventScreen = false;
            PrivateVariable.Instance.InMainScreen = false;
            PrivateVariable.Instance.InMap = false;
            EmulatorLoader.EjectSockets();
            Variables.ScriptLog("Script Stopped!", Color.White);
            if (Width > 1280)
            {
                Width -= 1280;
                panel3.Visible = false;
            }
            if (Variables.Proc != null)
            {
                DllImport.SetParent(Variables.Proc.MainWindowHandle, IntPtr.Zero);
                DllImport.MoveWindow(Variables.Proc.MainWindowHandle, PrivateVariable.Instance.EmuDefaultLocation.X, PrivateVariable.Instance.EmuDefaultLocation.Y, 1318, 752, true);
            }
            foreach (Control control in Debug.Controls)
            {
                control.Enabled = true;
            }
            ScriptRun.StopScript();
            btn_Start.Enabled = true;
            Controls.Remove(tp);
        }

        private void Timer2_Tick(object sender, EventArgs e)
        {
            GC.Collect();
            switch (PrivateVariable.Instance.VCevent)
            {
                case PrivateVariable.EventType.Tower:
                    lbl_CEvent.Text = UILanguage["Tower"];
                    lbl_Rune.Text = UILanguage["Rune_Tower"];
                    progressBar1.Maximum = 5;
                    progressBar2.Maximum = 5;
                    progressBar1.Value = VCBotScript.energy;
                    progressBar2.Value = VCBotScript.runes;
                    label7.Text = VCBotScript.runes + "/5";
                    label6.Text = VCBotScript.energy + "/5";
                    if (!string.IsNullOrEmpty(VCBotScript.Tower_Floor))
                    {
                        try
                        {
                            label15.Text = Regex.Match(VCBotScript.Tower_Floor, @"\d+").Value + " F";
                        }
                        catch
                        {

                        }
                    }
                    if (!string.IsNullOrEmpty(VCBotScript.Tower_Rank))
                    {
                        try
                        {
                            label16.Text = Regex.Match(VCBotScript.Tower_Rank, @"\d+").Value;
                        }
                        catch
                        {

                        }
                    }
                    break;
                case PrivateVariable.EventType.ArchWitch:
                    lbl_CEvent.Text = UILanguage["Archwitch"];
                    lbl_Rune.Text = UILanguage["BossE_Archwitch"];
                    label7.Text = ArchwitchEvent.CurrentBossEnergy + "/" + ArchwitchEvent.FullBossEnergy;
                    label6.Text = ArchwitchEvent.CurrentWalkEnergy + "/" + ArchwitchEvent.FullWalkEnergy;
                    progressBar2.Maximum = ArchwitchEvent.FullBossEnergy;
                    progressBar1.Maximum = ArchwitchEvent.FullWalkEnergy;
                    progressBar2.Value = ArchwitchEvent.CurrentBossEnergy;
                    progressBar1.Value = ArchwitchEvent.CurrentWalkEnergy;
                    break;
                case PrivateVariable.EventType.SoulWeapon:
                    lbl_CEvent.Text = UILanguage["Archwitch"];
                    lbl_Rune.Text = UILanguage["BossE_Archwitch"];
                    label7.Text = ArchwitchEvent.CurrentBossEnergy + "/" + ArchwitchEvent.FullBossEnergy;
                    label6.Text = ArchwitchEvent.CurrentWalkEnergy + "/" + ArchwitchEvent.FullWalkEnergy;
                    progressBar2.Maximum = ArchwitchEvent.FullBossEnergy;
                    progressBar1.Maximum = ArchwitchEvent.FullWalkEnergy;
                    progressBar2.Value = ArchwitchEvent.CurrentBossEnergy;
                    progressBar1.Value = ArchwitchEvent.CurrentWalkEnergy;
                    label15.Text = SoulWeapon.PT;
                    label16.Text = SoulWeapon.Rank;
                    break;
                case PrivateVariable.EventType.DemonRealm:
                    lbl_CEvent.Text = UILanguage["Demon"];
                    lbl_Rune.Text = UILanguage["Rune_Demon"];
                    label7.Text = VCBotScript.runes + "/4";
                    label6.Text = VCBotScript.energy + "/5";
                    progressBar1.Maximum = 5;
                    progressBar2.Maximum = 4;
                    progressBar1.Value = VCBotScript.energy;
                    progressBar2.Value = VCBotScript.runes;
                    if (!string.IsNullOrEmpty(VCBotScript.Tower_Floor))
                    {
                        try
                        {
                            label15.Text = Regex.Match(VCBotScript.Tower_Floor, @"\d+").Value + " F";
                        }
                        catch
                        {

                        }
                    }
                    if (!string.IsNullOrEmpty(VCBotScript.Tower_Rank))
                    {
                        try
                        {
                            label16.Text = Regex.Match(VCBotScript.Tower_Rank, @"\d+").Value;
                        }
                        catch
                        {

                        }
                    }
                    break;
                case PrivateVariable.EventType.GuildWar:
                    lbl_CEvent.Text = UILanguage["Guildwar"];
                    lbl_Rune.Text = UILanguage["Rune_Guildwar"];
                    label7.Text = VCBotScript.runes + "/6";
                    label6.Text = VCBotScript.energy + "/5";
                    progressBar1.Maximum = 5;
                    progressBar2.Maximum = 6;
                    progressBar1.Value = VCBotScript.energy;
                    progressBar2.Value = VCBotScript.runes;
                    break;
                default:
                    lbl_CEvent.Text = UILanguage["Unknown"];
                    break;
            }
            if (VCBotScript.nextOnline != null)
            {
                if (VCBotScript.nextOnline >= DateTime.Now)
                {
                    TimeSpan time = VCBotScript.nextOnline - DateTime.Now;
                    label9.Text = time.Hours.ToString("00") + " : " + time.Minutes.ToString("00") + " : " + time.Seconds.ToString("00");
                }
                else
                {
                    label9.Text = "00:00:00";
                }
            }
            else
            {
                label9.Text = "00:00:00";
            }
        }
        /// <summary>
        /// Capture loop
        /// </summary>

        private void Capt()
        {
            bool Docked = false;
            do
            {
                Thread.Sleep(2000);
                if (Variables.Proc != null)
                {
                    if (Variables.Proc.HasExited)
                    {
                        Variables.Proc = null;
                        Docked = false;
                        continue;
                    }
                    try
                    {
                        var handle = Variables.Proc.MainWindowHandle;
                        Rectangle rect = new Rectangle();
                        DllImport.GetWindowRect(handle, ref rect);
                        if (!Docked)
                        {
                            if (!ScriptRun.Run)
                            {
                                return;
                            }
                            PrivateVariable.Instance.EmuDefaultLocation = rect;
                            panel3.Invoke((MethodInvoker)delegate
                            {
                                DllImport.SetParent(Variables.Proc.MainWindowHandle, panel3.Handle);
                            });
                            tp.Invoke((MethodInvoker)delegate { tp.BringToFront(); });
                            panel3.Invoke((MethodInvoker)delegate
                            {
                                DllImport.SetParent(Variables.Proc.MainWindowHandle, panel3.Handle);
                            });
                            Docked = true;
                        }
                        if (rect.X != -1 || rect.Y != -30)
                        {
                            DllImport.MoveWindow(handle, -1, -30, 1318, 752, false);
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

        private void CheckBox8_CheckedChanged(object sender, EventArgs e)
        {
            Variables.ModifyConfig("General", "Double_Event", chk_twoE.Checked.ToString().ToLower());
        }

        private void checkBox8_MouseEnter(object sender, EventArgs e)
        {
            toolTip1.Show(UILanguage["Double_Tip"], chk_twoE);
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
                Variables.ModifyConfig("General", "Manual_Rune", "false");
                //chk_item.Enabled = chk_autoRT.Checked;
            }
        }

        private void radioButton10_CheckedChanged(object sender, EventArgs e)
        {
            if (chk_manuRT.Checked)
            {
                Variables.ModifyConfig("General", "Manual_Rune", "true");
                chk_item.Checked = false;
                //chk_item.Enabled = false;
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
                Variables.ModifyConfig("General", "Level", "0");
                Level = 0;
            }
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (chk_inter.Checked)
            {
                Variables.ModifyConfig("General", "Level", "1");
                Level = 1;
            }
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (chk_advan.Checked)
            {
                Variables.ModifyConfig("General", "Level", "2");
                Level = 2;
            }
        }

        private void checkBox10_CheckedChanged(object sender, EventArgs e)
        {
            Variables.ModifyConfig("General", "Use_Item", chk_item.Checked.ToString().ToLower());
            PrivateVariable.Instance.Use_Item = chk_item.Checked;
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            if (pictureBox4.Image != null)
            {
                SaveFileDialog s = new SaveFileDialog
                {
                    CheckPathExists = true,
                    OverwritePrompt = true,
                    AddExtension = false,
                    Filter = "(PNG)|*.png",
                    DefaultExt = "png"
                };
                s.AddExtension = true;
                var result = s.ShowDialog();
                if (result == DialogResult.OK)
                {
                    Bitmap OriginalBitmap = new Bitmap(pictureBox4.Image);
                    int new_wid = 640, new_hgt = 896;
                    var bmp = Screenshot.EnlargeImage(OriginalBitmap, new_wid, new_hgt);
                    bmp.Save(s.FileName);
                }
            }

        }

        private void PictureBox4_MouseEnter(object sender, EventArgs e)
        {
            toolTip1.Show("点击即可保存图片哦！", pictureBox4);
        }

        private void PictureBox4_MouseLeave(object sender, EventArgs e)
        {
            toolTip1.Hide(pictureBox4);
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            if (chk_extre.Checked)
            {
                Variables.ModifyConfig("General", "Level", "3");
                Level = 3;
            }
        }

        private void RadioButton5_CheckedChanged(object sender, EventArgs e)
        {
            if (chk_ultim.Checked)
            {
                Variables.ModifyConfig("General", "Level", "4");
                Level = 4;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (File.Exists("OCR.png"))
            {
                var img = Screenshot.Compress(Image.FromFile("OCR.png") as Bitmap);
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
                    else
                    {
                        div.Style = "color:White;background-color:#172636;";
                    }
                }
                foreach (HtmlElement li in webBrowser3.Document.GetElementsByTagName("li"))
                {
                    if(li.InnerHtml != null)
                    {
                        li.InnerHtml = li.InnerHtml.Replace("<a ", "<a style=\"color:#cfcffc;\" ").Replace("_神女控图鉴搜索", "").Replace("_Valkyrie Crusade Manual","");
                    }

                }
                foreach (HtmlElement footer in webBrowser3.Document.GetElementsByTagName("footer"))
                {
                    footer.InnerText = "";
                }
                webBrowser3.Document.BackColor = Color.FromArgb(23, 38, 54);
            }
            webBrowser3.Show();
        }

        private void MainScreen_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (ScriptRun.Run)
            {
                Button3_Click(sender, e);
            }
            EmulatorLoader.EjectSockets();
            Environment.Exit(0);
        }

        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Variables.ModifyConfig("General", "Lang", comboBox1.SelectedItem.ToString());
            if (comboBox1.SelectedItem.ToString().Contains("ch"))
            {
                Biubiu.Visible = true;
            }
            else
            {
                Biubiu.Visible = false;
            }
            ChangeLanguage(comboBox1.SelectedItem.ToString(), this);
            html = Img.index;
            using (WebClientOverride wc = new WebClientOverride())
            {
                if (comboBox1.SelectedItem.ToString().Contains("ch"))
                {
                    try
                    {
                        html = wc.DownloadString(new Uri("https://d2n1d3zrlbtx8o.cloudfront.net/news/info/sch/index.html"));
                        html = html.Replace("bgcolor=\"#000000\"　text color=\"#FFFFFF\"", "style=\"background - color:#303030; color:white\"");
                        html = html.Remove(html.IndexOf("<table width=\"200\">"), html.IndexOf("</table>") - html.IndexOf("<table width=\"200\">"));
                        html = Regex.Replace(html, "(\\<span class\\=\"iro4\"\\>.*</span>)", "");
                    }
                    catch
                    {

                    }
                }
                else
                {
                    try
                    {
                        html = wc.DownloadString(new Uri("https://d2n1d3zrlbtx8o.cloudfront.net/news/info/en/index.html"));
                        html = html.Replace("bgcolor=\"#000000\"　text color=\"#FFFFFF\"", "style=\"background - color:#303030; color:white\"");
                        html = html.Remove(html.IndexOf("<table width=\"200\">"), html.IndexOf("</table>") - html.IndexOf("<table width=\"200\">"));
                        html = Regex.Replace(html, "(\\<span class\\=\"iro4\"\\>.*</span>)", "");
                    }
                    catch
                    {

                    }

                }
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
                var buffer = File.ReadAllLines(Environment.CurrentDirectory + "\\Language\\" + language + ".ini", Encoding.Unicode);
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
                if (UILanguage.TryGetValue(control.Name, out string text))
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
                    if (comboBox1.SelectedItem.ToString().Contains("ch"))
                    {
                        Process.Start("http://www-valkyriecrusade.nubee.com/" + GetEventXML.Eventlink.Replace("/en/", "/sch/") + ".html");
                    }
                    else
                    {
                        Process.Start("http://www-valkyriecrusade.nubee.com/" + GetEventXML.Eventlink + ".html");
                    }
                }
                else
                {
                    if (comboBox1.SelectedItem.ToString().Contains("ch"))
                    {
                        webBrowser3.Navigate("http://www-valkyriecrusade.nubee.com/" + GetEventXML.Eventlink.Replace("/en/", "/sch/") + ".html");
                    }
                    else
                    {
                        webBrowser3.Navigate("http://www-valkyriecrusade.nubee.com/" + GetEventXML.Eventlink + ".html");
                    }
                }

            }
        }

        private void Rdb_card_Click(object sender, EventArgs e)
        {
            if (comboBox1.Text == "sch")
            {
                Cookie temp = new Cookie("va_lang", "zh-cn", "/", ".xldsdr.com");
                InternetSetCookie("http://www.xldsdr.com/valkyriecrusade", null, temp.ToString() + ";");
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
                Cookie temp = new Cookie("va_lang", "en", "/", ".xldsdr.com");
                InternetSetCookie("http://www.xldsdr.com/valkyriecrusade", null, temp.ToString() + ";");
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
        }

        private void Rbd_help_Click(object sender, EventArgs e)
        {
            if (rbd_help.Checked)
            {
                if (chk_browser.Checked)
                {
                    if (comboBox1.SelectedItem.ToString().Contains("ch"))
                    {
                        Process.Start("http://d2n1d3zrlbtx8o.cloudfront.net/news/help/sch/index.html");
                    }
                    else
                    {
                        Process.Start("http://d2n1d3zrlbtx8o.cloudfront.net/news/help/en/index.html");
                    }
                }
                else
                {
                    webBrowser3.Visible = true;
                    if (comboBox1.SelectedItem.ToString().Contains("ch"))
                    {
                        webBrowser3.Navigate("http://d2n1d3zrlbtx8o.cloudfront.net/news/help/sch/index.html");
                    }
                    else
                    {
                        webBrowser3.Navigate("http://d2n1d3zrlbtx8o.cloudfront.net/news/help/en/index.html");
                    }
                }
            }
        }

        private void Suspend_Chk_CheckedChanged(object sender, EventArgs e)
        {
            Variables.ModifyConfig("General", "Suspend_PC", Suspend_Chk.Checked.ToString().ToLower());
        }

        private void btn_Sleep_Click(object sender, EventArgs e)
        {
            SleepWake.SetWakeTimer(DateTime.Now.AddMinutes(1));
            PCController.DoMouseClick(100, 100);
        }

        private void MetroButton1_Click(object sender, EventArgs e)
        {
            ScriptRun.ThrowException(new Exception("This is a testing exception!"));
        }

        private void biubiu_CheckedChanged(object sender, EventArgs e)
        {
            PrivateVariable.Instance.biubiu = Biubiu.Checked;
            Variables.ModifyConfig("General", "biubiu", Biubiu.Checked.ToString().ToLower());
        }

        private void MainScreen_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                WindowState = FormWindowState.Normal;//Restore normal state of form
                if (Location == new Point(-1000, -1000))
                {
                    Location = loc;
                }
                else
                {
                    loc = Location;
                    Location = new Point(-1000, -1000);
                }
            }
            else if (WindowState == FormWindowState.Maximized)
            {
                WindowState = FormWindowState.Normal;
            }
        }

        private void RichTextBox1_MouseEnter(object sender, EventArgs e)
        {
            scroll = false;
        }

        private void RichTextBox1_MouseLeave(object sender, EventArgs e)
        {
            scroll = true;
        }

        private void WinAPi_CheckedChanged(object sender, EventArgs e)
        {
            Variables.ModifyConfig("General", "WinApi", WinAPi.Checked.ToString().ToLower());
            Variables.WinApiCapt = WinAPi.Checked;
        }

        private void Chk_Archwitch_CheckedChanged(object sender, EventArgs e)
        {
            Variables.ModifyConfig("General", "ArWiEv", Chk_Archwitch.Checked.ToString().ToLower());
        }

        private void Combo_Archwitch_SelectedIndexChanged(object sender, EventArgs e)
        {
            Variables.ModifyConfig("General", "ArWiSt", Combo_Archwitch.SelectedItem.ToString().Replace("-", "."));
            Variables.FindConfig("General", "ArWiSt", out string config);
            VCBotScript.Archwitch_Stage = Convert.ToDouble(config);
        }

        private void Combo_Weapon_SelectedIndexChanged(object sender, EventArgs e)
        {
            Variables.ModifyConfig("General", "SoWeSt", Combo_Weapon.SelectedItem.ToString().Replace("-", "."));
            Variables.FindConfig("General", "SoWeSt", out string config);
            VCBotScript.Weapon_Stage = Convert.ToDouble(config);
        }


        private void btn_SwitchImg_Click(object sender, EventArgs e)
        {
            GetEventXML.GetRandomImage();
            if (GetEventXML.RandomImage != null)
            {
                var request = WebRequest.Create("http://www-valkyriecrusade.nubee.com/" + GetEventXML.RandomImage);
                using (var response = request.GetResponse())
                using (var stream = response.GetResponseStream())
                {
                    pictureBox4.Image = Image.FromStream(stream);
                }
            }
        }

        private void chk_GWW_CheckedChanged(object sender, EventArgs e)
        {
            Variables.ModifyConfig("GuildWar", "Manual", chk_GWW.Checked.ToString().ToLower());
        }

        private void checkBox1_CheckedChanged_1(object sender, EventArgs e)
        {
            Variables.ModifyConfig("General", "SoulEv", chk_SoulEv.Checked.ToString().ToLower());
        }

        private void checkBox1_CheckedChanged_2(object sender, EventArgs e)
        {
            Variables.ImageDebug = checkBox1.Checked;
        }
    }
}
