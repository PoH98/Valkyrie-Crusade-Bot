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
        private static string NoxFile;
        public void CloseEmulator()
        {
            ProcessStartInfo close = new ProcessStartInfo();
            close.FileName = Variables.VBoxManagerPath;

            if (Variables.Instance.Length > 0)
            {
                close.Arguments = "controlvm " + Variables.Instance + " poweroff";
            }
            else
            {
                close.Arguments = "controlvm Nox poweroff";
            }
            close.CreateNoWindow = true;
            close.WindowStyle = ProcessWindowStyle.Hidden;
            try
            {
                if (Variables.Proc != null || !Variables.Proc.HasExited)
                {
                    Variables.Proc.Kill();
                }
            }
            catch
            {

            }
            Process p = Process.Start(close);
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
                        NoxFile = path;
                        Variables.VBoxManagerPath = GetRTPath() + "BigNoxVMMgr.exe";
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
                            NoxFile = path;
                            Variables.VBoxManagerPath = GetRTPath() + "BigNoxVMMgr.exe";
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
                            NoxFile = path + "\\bin\\Nox.exe";
                            Variables.VBoxManagerPath = GetRTPath() + "BigNoxVMMgr.exe";
                            InitNox();
                            return true;
                        }
                    }
                }
                catch
                {

                }
            }
            return false;
        }
        private static string GetRTPath()
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + "\\BigNox\\BigNoxVM\\RT\\";
            if (Directory.Exists(path))
            {
                return path;
            }
            path = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + "\\BigNox\\BigNoxVM\\RT\\";
            if (Directory.Exists(path))
            {
                return path;
            }
            RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\BigNox\VirtualBox\");
            var value = key.GetValue("InstallDir");
            if(value != null)
            {
                return value.ToString();
            }
            return null;
        }
        private static void InitNox()
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace("\\Roaming", "\\Local\\Nox\\conf.ini"));
            if (File.Exists(path))
            {
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
                            MessageBox.Show("Nox version is not supported! Please update it to Nox v6 above!");
                            Environment.Exit(0);
                        }
                        break;
                    }
                }
            }
            if(Variables.SharedPath == "")
            {
                if (Directory.Exists((Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Nox_share\Other\").Replace("\\\\", "\\")))
                {
                    Variables.SharedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Nox_share\Other\";
                }
                else if(Directory.Exists((Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Nox_share\OtherShare\").Replace("\\\\","\\")))
                {
                    Variables.SharedPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Nox_share\OtherShare\";
                }
                else
                {
                    MessageBox.Show("Nox version is not supported! Please update it to Nox v6 above!");
                    Environment.Exit(0);
                }
            }
            Variables.AndroidSharedPath = "/mnt/shared/Other|/mnt/shell/emulated/0/Download/other|/mnt/shell/emulated/0/Others/";
            Variables.SharedPath = Variables.SharedPath.Replace("\\\\","\\");
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = Variables.VBoxManagerPath;
            if (Variables.Instance.Length > 0)
            {
                info.Arguments = "showvminfo " + Variables.Instance;
            }
            else
            {
                info.Arguments = "showvminfo nox";
            }
            info.RedirectStandardOutput = true;
            info.UseShellExecute = false;
            info.CreateNoWindow = true;
            var p = Process.Start(info);
            var result = p.StandardOutput.ReadToEnd();
            string[] splitted = result.Split('\n');
            foreach (var s in splitted)
            {
                if (s.Contains("guest port = 5555"))
                {
                    var port = s.Substring(s.IndexOf("port = ") + 7, 5).Replace(" ", "");
                    Variables.AdbIpPort = "127.0.0.1:" + port;
                }
            }
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
                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = NoxFile + "\\bin\\Nox.exe";
                if (Variables.Instance.Length > 0)
                {
                    info.Arguments = "-clone:" + Variables.Instance;
                }
                else
                {
                    info.Arguments = "-clone:Nox_0";
                 }
                info.RedirectStandardOutput = true;
                info.UseShellExecute = false;
                info.CreateNoWindow = true;
                Process.Start(info);
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

        public void SetResolution(int x, int y, int dpi)
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
                        lines[a] = "h_dpi=" + dpi;
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
