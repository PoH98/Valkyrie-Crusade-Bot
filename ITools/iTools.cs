using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Windows.Forms;
using BotFramework;
using Microsoft.Win32;

namespace ITools
{
    public class ITools : EmulatorInterface
    {
        string emulatorpath;

        public string EmulatorDefaultInstanceName()
        {
            return "iToolsVM";
        }

        public string EmulatorName()
        {
            return "iTools";
        }

        public string EmulatorProcessName()
        {
            return "iToolsAVM";
        }

        public bool LoadEmulatorSettings()
        {
            try
            {
                RegistryKey reg;
                string path;
                //Read display icon
                if (BotCore.Is64BitOperatingSystem())
                    reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\iToolsAVM\");
                else
                    reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\iToolsAVM\");
                path = reg.GetValue("DisplayIcon").ToString();
                path = path.Remove(path.LastIndexOf("\\"));
                if (File.Exists(path + "\\iToolsAVM.exe"))
                    emulatorpath = path;
                else
                    return false;
                reg = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Oracle\\VirtualBox\\");
                if(reg != null)
                {
                    Variables.VBoxManagerPath = reg.GetValue("InstallDir").ToString() + "VBoxManage.exe";
                }
                else
                {
                    Variables.VBoxManagerPath = "C:\\Program Files\\Oracle\\VirtualBox\\VBoxManage.exe";
                }
                if (!File.Exists(Variables.VBoxManagerPath))
                {
                    return false;
                }
                ProcessStartInfo fetch = new ProcessStartInfo(Variables.VBoxManagerPath);
                if (Variables.Instance.Length > 0)
                {
                    fetch.Arguments = "showvminfo " + Variables.Instance;
                }
                else
                {
                    fetch.Arguments = "showvminfo iToolsVM";
                }
                fetch.CreateNoWindow = true;
                fetch.RedirectStandardOutput = true;
                fetch.UseShellExecute = false;
                Process fetching = Process.Start(fetch);
                string result = fetching.StandardOutput.ReadToEnd();
                string[] splitted = result.Split('\n');
                foreach (var s in splitted)
                {
                    if (s.Contains("name = ADB_PORT"))
                    {
                        var port = s.Substring(s.IndexOf("port = ") + 7, 5).Replace(" ", "");
                        Variables.AdbIpPort = "127.0.0.1:" + port;
                    }
                }
                Variables.SharedPath = emulatorpath + "\\UsersCommon\\";
                Variables.AndroidSharedPath = "/data/iToolsVMShare/";
                return true;
            }
            catch (Exception ex)
            {
                Variables.AdvanceLog(ex.ToString());
            }
            return false;
        }

        public void SetResolution(int x, int y, int dpi)
        {
            ProcessStartInfo s = new ProcessStartInfo(Variables.VBoxManagerPath);
            s.Arguments = "guestproperty set iToolsVM resolution_height " + y;
            Process.Start(s);
            s.Arguments = "guestproperty set iToolsVM resolution_width " + x;
            Process.Start(s);
            s.Arguments = "guestproperty set iToolsVM vbox_dpi " + dpi;
            Process.Start(s);
        }

        public void StartEmulator()
        {
            try
            {
                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = emulatorpath + "\\iToolsAVM.exe";
                if (Variables.Instance.Length > 0)
                {
                    info.Arguments = Variables.Instance;
                }
                else
                {
                    info.Arguments = "iToolsVM";
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
                Process.Start("Profiles\\" + AdbInstance.Instance.profilePath + "\\bot.ini");
                Environment.Exit(0);
            }
        }

        public void UnUnBotify()
        {

        }
    }
}
