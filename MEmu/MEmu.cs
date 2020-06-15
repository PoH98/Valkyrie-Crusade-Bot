using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using BotFramework;
using Microsoft.Win32;

namespace MEmu
{
    public class MEmu : EmulatorInterface
    {

        public string EmulatorProcessName()
        {
            return "MEmu|逍遥模拟器";
        }

        public string EmulatorName()
        {
            return "MEmu|逍遥模拟器";
        }

        public string EmulatorDefaultInstanceName()
        {
            return "MEmu";
        }

        public bool LoadEmulatorSettings()
        {
            RegistryKey reg = Registry.LocalMachine;
            try
            {
                object location = null;
                var drives = DriveInfo.GetDrives();
                foreach(var d in drives)
                {
                    if (File.Exists(d.Name + @"Program Files\Microvirt\MEmu\MEmu.exe"))
                    {
                        location = d.Name + @"Program Files\Microvirt";
                    }
                }
                if(location == null)
                {
                    //We will try getting running processes as user might helped us opened it
                    foreach (var process in Process.GetProcesses().Where(x => x.ProcessName.Contains("MEmu")))
                    {
                        if (File.Exists(process.MainModule.FileName) && process.MainModule.FileName.EndsWith("MEmu.exe"))
                        {
                            location = process.MainModule.FileName.Replace(@"\MEmu\MEmu.exe", "");
                            if (BotCore.Is64BitOperatingSystem())
                            {
                                RegistryKey r = reg.OpenSubKey("SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\MEmu");
                                if (r == null)
                                {
                                    //MEmu didnt have this registered, lets do it for next load we will able to get file easily
                                    r = reg.CreateSubKey("SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\MEmu");
                                    r.SetValue("InstallLocation", location);
                                }
                            }
                            else
                            {
                                RegistryKey r = reg.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\MEmu");
                                if (r == null)
                                {
                                    //MEmu didnt have this registered, lets do it for next load we will able to get file easily
                                    r = reg.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\MEmu");
                                    r.SetValue("InstallLocation", location);
                                }
                            }
                            break;
                        }
                    }
                }
                if(location == null)
                {
                    RegistryKey r = reg.OpenSubKey("SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\MEmu");
                    if(r == null)
                    {
                        r = reg.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\MEmu");
                    }
                    if(r == null)
                    {
                        return false;
                    }
                    location = r.GetValue("InstallLocation");
                    if(location == null)
                    {
                        location = r.GetValue("DisplayIcon");
                        if(location != null)
                        {
                            location = location.ToString().Remove(location.ToString().LastIndexOf("\\"));
                        }
                    }
                }
                //Found all the path of MEmu
                if (location != null)
                {
                    var path = location.ToString().Replace("\0", "");
                    if (Directory.Exists(path))
                    {
                        Variables.VBoxManagerPath = path + @"\MEmuHyperv\MEmuManage.exe";
                        ProcessStartInfo fetch = new ProcessStartInfo(Variables.VBoxManagerPath);
                        if (Variables.Instance.Length > 0)
                        {
                            fetch.Arguments = "showvminfo " + Variables.Instance;
                        }
                        else
                        {
                            fetch.Arguments = "showvminfo MEmu";
                        }
                        fetch.CreateNoWindow = true;
                        fetch.RedirectStandardOutput = true;
                        fetch.UseShellExecute = false;
                        do
                        {
                            Process fetching = Process.Start(fetch);
                            string result = fetching.StandardOutput.ReadToEnd();
                            string[] splitted = result.Split('\n');
                            foreach (var s in splitted)
                            {
                                if (s.Contains("Name: 'download', Host path:"))
                                {
                                    string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
                                    Regex re = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
                                    path = s.ToLower().Replace("name: 'download', host path: '", "").Replace("' (machine mapping), writable", "");
                                    path = re.Replace(path, "\\");
                                    path = path.Replace("\\\\", ":\\");
                                    Variables.SharedPath = path;
                                    if (!Directory.Exists(Variables.SharedPath))
                                    {
                                        MessageBox.Show("共享路径包含不允许字符，请重新设置模拟器的共享路径！");
                                        Environment.Exit(0);
                                    }
                                }
                                else if (s.Contains("name = ADB"))
                                {
                                    var port = s.Substring(s.IndexOf("port = ") + 7, 5).Replace(" ", "");
                                    Variables.AdbIpPort = "127.0.0.1:" + port;
                                }
                            }
                        }
                        while (Variables.SharedPath == null || Variables.AdbIpPort == null);
                        Variables.AndroidSharedPath = "/sdcard/Download/";
                        Variables.ClickPointMultiply = 1;
                        return true;
                    }
                }
            }
            catch(Exception ex)
            {
                Variables.AdvanceLog(ex.ToString());
            }
            return false;
        }

        public void SetResolution(int x, int y, int dpi)
        {
            ProcessStartInfo s = new ProcessStartInfo(Variables.VBoxManagerPath);
            s.Arguments = "guestproperty set MEmu resolution_height " + y;
            Process.Start(s);
            s.Arguments = "guestproperty set MEmu resolution_width " + x;
            Process.Start(s);
            s.Arguments = "guestproperty set MEmu vbox_dpi " + dpi;
            Process.Start(s);
        }

        public void StartEmulator()
        {
            try
            {
                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = Variables.VBoxManagerPath.Replace(@"\MEmuHyperv\MEmuManage.exe", @"\MEmu\MEmuConsole.exe");
                if (Variables.Instance.Length > 0)
                {
                    info.Arguments = Variables.Instance;
                }
                else
                {
                    info.Arguments = "MEmu";
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
            try
            {
                //Remove 4 files which detect by Unbotify
                BotCore.AdbCommand("rm -f /system/bin/microvirtd", Variables.Controlled_Device);
                BotCore.AdbCommand("rm -f /system/etc/init.microvirt.sh", Variables.Controlled_Device);
                BotCore.AdbCommand("rm -f /system/bin/memud", Variables.Controlled_Device);
                BotCore.AdbCommand("rm -f /system/lib/memuguest.ko", Variables.Controlled_Device);
            }
            catch
            {

            }
        }

        public IntPtr DXScreen()
        {
            var MEmus = DllImport.GetAllChildrenWindowHandles(IntPtr.Zero, "Qt5QWindowIcon", null, 10);
            if(MEmus != null && MEmus.Count > 0)
            {
                foreach(var memu in MEmus)
                {
                    IntPtr MEmu = memu;
                    if(DllImport.GetAllChildrenWindowHandles(MEmu, "Qt5QWindowIcon", null, 5).Count < 1)
                    {
                        MEmu = DllImport.GetParent(MEmu);
                    }
                    var MainWindowWindow = DllImport.FindWindowEx(MEmu, IntPtr.Zero, null, "MainWindowWindow");
                    if (MainWindowWindow != null && MainWindowWindow != IntPtr.Zero)
                    {
                        var CenterWidgetWindow = DllImport.FindWindowEx(MainWindowWindow, IntPtr.Zero, null, "CenterWidgetWindow");
                        if (CenterWidgetWindow != null && CenterWidgetWindow != IntPtr.Zero)
                        {
                            var RenderWindowWindow = DllImport.FindWindowEx(CenterWidgetWindow, IntPtr.Zero, null, "RenderWindowWindow");
                            if (RenderWindowWindow != null && RenderWindowWindow != IntPtr.Zero)
                            {
                                var sub = DllImport.FindWindowEx(RenderWindowWindow, IntPtr.Zero, null, "sub");
                                if (sub != null && sub != IntPtr.Zero)
                                {
                                    return sub;
                                }
                            }
                        }
                    }
                }
            }
            
            return IntPtr.Zero;
        }
    }
}
