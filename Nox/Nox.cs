using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BotFramework;
using Microsoft.Win32;

namespace Nox
{
    public class Nox : EmulatorInterface
    {
        public void CloseEmulator()
        {
            if(Variables.Proc != null)
            {
                BotCore.KillProcessAndChildren(Variables.Proc.Id);
            }
        }

        public string EmulatorName()
        {
            return "Nox";
        }

        public bool LoadEmulatorSettings()
        {
            RegistryKey reg = Registry.LocalMachine;
            string path;
            var r = reg.OpenSubKey(@"SOFTWARE\BigNox\VirtualBox\");
            try
            {
                var result = r.GetValue("InstallDir");
                if (result != null)
                {
                    path = result.ToString();
                    if (Directory.Exists(path))
                    {
                        Variables.VBoxManagerPath = path;
                        InitNox();
                        return true;
                    }
                }
            }
            catch
            {

            }
            if (BotCore.Is64BitOperatingSystem())
            {
                r = reg.OpenSubKey("SOFTWARE\\Wow6432Node\\DuoDianOnline\\SetupInfo\\");
                try
                {
                    var result = r.GetValue("InstallPath");
                    if (result != null)
                    {
                        path = result.ToString();
                        if (Directory.Exists(path))
                        {
                            Variables.VBoxManagerPath = path + "\\bin\\Nox.exe";
                            InitNox();
                            return true;
                        }
                    }
                }
                catch
                {

                }
            }
            else
            {
                r = reg.OpenSubKey("SOFTWARE\\DuoDianOnline\\SetupInfo\\");
                try
                {
                    var result = r.GetValue("InstallPath");
                    if (result != null)
                    {
                        path = result.ToString();
                        if (Directory.Exists(path))
                        {
                            Variables.VBoxManagerPath = path + "\\bin\\Nox.exe";
                            InitNox();
                            return true;
                        }
                    }
                }
                catch
                {

                }
            }
            path = null;
            return false;
        }
        private static void InitNox()
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace("\\Roaming", "\\Local\\Nox\\conf.ini"));
            string[] lines = File.ReadAllLines(path);
            foreach (var line in lines)
            {
                if (line.Contains("share_path"))
                {
                    path = line.Replace(@"\\", @"\").Replace("share_path=", "");
                    if (Directory.Exists(path + "\\OtherShare\\"))
                    {
                        Variables.SharedPath = path + "\\OtherShare";
                    }
                    else
                    {
                        MessageBox.Show("Nox模拟器版本不被支持！请更新至 6.2.7.1！");
                        Environment.Exit(0);
                    }
                    Variables.SharedPath = Variables.SharedPath.Replace("\\\\", "\\");
                }
                else if (line.Contains("adb_port"))
                {
                    Variables.AdbIpPort = "127.0.0.1:" + line.Replace("adb_port=", "");
                }
            }
            Variables.AndroidSharedPath = "/mnt/shell/emulated/0/Others/";
        }
        public void StartEmulator()
        {
            try
            {
                if (!File.Exists(Variables.VBoxManagerPath))
                {
                    MessageBox.Show("Unable to locate path of emulator!");
                    Process.Start("Profiles\\" + BotCore.profilePath + "\\bot.ini");
                }
                Process.Start(Variables.VBoxManagerPath);
                Thread.Sleep(10000);
            }
            catch (SocketException)
            {

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while starting emulator! Error message: " + ex.Message);
                Process.Start("Profiles\\" + BotCore.profilePath + "\\bot.ini");
                Environment.Exit(0);
            }
        }

        public void SetResolution(int x, int y)
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace("\\Roaming", "\\Local\\Nox\\conf.ini"));
            if (File.Exists(path))
            {
                string[] lines = File.ReadAllLines(path);
                for (int a = 0; a < lines.Length; a++)
                {
                    if (lines[a].Contains("h_resolution"))
                    {
                        lines[a] = "h_resolution=" + x + "x" + y;
                    }
                    else if (lines[a].Contains("h_dpi"))
                    {
                        lines[a] = "h_dpi=160";
                    }
                }
                File.WriteAllLines(path, lines);
            }
            else
            {
                MessageBox.Show("找不到Nox模拟器conf.ini文件！请确保模拟器重新安装！");
                Environment.Exit(0);
            }
        }

        public void ConnectEmulator()
        {
            string[] Nox = { "NoxPlayer", "夜神模拟器" };
            foreach (var p in Process.GetProcessesByName("Nox"))
            {
                Debug_.WriteLine(p.MainWindowTitle);
                if (Nox.Contains(p.MainWindowTitle))
                {
                    IntPtr handle = DllImport.FindWindowEx(p.MainWindowHandle, IntPtr.Zero, string.Empty, string.Empty);
                    Variables.Proc = p;
                    Variables.ScriptLog("Emulator ID: " + p.Id, Color.DarkGreen);
                    break;
                }
            }
        }
    }
}
