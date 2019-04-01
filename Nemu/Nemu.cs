using ImageProcessor;
using Microsoft.Win32;
using SharpAdbClient;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

namespace Nemu
{
    public class Nemu : EmulatorInterface
    {
        public void CloseEmulator()
        {
            if (Variables.Proc != null)
            {
                EmulatorController.KillProcessAndChildren(Variables.Proc.Id);
            }
        }

        public void ConnectEmulator()
        {
            string[] Nemu = { "MuMu模拟器" };
            foreach (var p in Process.GetProcessesByName("NemuPlayer"))
            {
                Debug_.WriteLine(p.MainWindowTitle);
                if (Nemu.Contains(p.MainWindowTitle))
                {
                    IntPtr handle = DllImport.FindWindowEx(p.MainWindowHandle, IntPtr.Zero, string.Empty, string.Empty);
                    Variables.Proc = p;
                    EmulatorController.handle = p.MainWindowHandle;
                    Variables.ScriptLog("Emulator ID: " + p.Id, Color.DarkGreen);
                    break;
                }
            }
        }

        public string EmulatorName()
        {
            return "Nemu";
        }

        public bool LoadEmulatorSettings()
        {
            RegistryKey reg = Registry.LocalMachine;
            RegistryKey r;
            if (EmulatorController.Is64BitOperatingSystem())
            {
                r = reg.OpenSubKey("SOFTWARE\\WOW6432Node\\Nemu");
            }
            else
            {
                r = reg.OpenSubKey("SOFTWARE\\Nemu");
            }
            var location = r.GetValue("Install_Dir");
            if (location != null)
            {
                var path = location.ToString().Replace("\0", "");
                if (Directory.Exists(path))
                {
                    Variables.VBoxManagerPath = path + @"\EmulatorShell\NemuPlayer.exe";
                    foreach(var line in File.ReadAllLines(path + @"\vms\myandrovm_vbox86\myandrovm_vbox86.nemu"))
                    {
                        if (line.Contains("adb"))
                        {
                            int split0 = line.IndexOf("hostip=");
                            int split1 = line.IndexOf("guestport=");
                            Variables.AdbIpPort = line.Substring(split0).Remove(split1 - split0).Replace("hostport=", ":");
                            Variables.AdbIpPort = Variables.AdbIpPort.Replace("\"","").Replace("hostip=","").Replace("guestport=", "").Replace(" ","");
                        }
                        if(line.Contains("<SharedFolder name=\"MuMu&#x5171;&#x4EAB;&#x6587;&#x4EF6;&#x5939;\""))
                        {
                            int split0 = line.IndexOf("hostPath=");
                            int split1 = line.IndexOf(" writable=");
                            Variables.SharedPath = line.Substring(split0).Remove(split1-split0).Replace("hostPath=","");
                            Variables.SharedPath = Variables.SharedPath.Replace("\"","").Replace("&#x5171;&#x4EAB;&#x6587;&#x4EF6;&#x5939;", "共享文件夹");
                        }
                    }
                    Variables.AndroidSharedPath = "/storage/emulated/0/";
                    Variables.NeedPull = true;
                    return true;
                }
            }
            return false;
        }

        public void SetResolution(int x, int y)
        {
            MessageBox.Show("分辨率不支持！请自己设置模拟器分辨率！（1280x720）");
            Environment.Exit(0);
        }

        public void StartEmulator()
        {
            try
            {
                if (!File.Exists(Variables.VBoxManagerPath))
                {
                    MessageBox.Show("Unable to locate path of emulator!");
                    Process.Start("Profiles\\" + EmulatorController.profilePath + "\\bot.ini");
                }
                Process.Start(Variables.VBoxManagerPath);
                DnsEndPoint dnsEnd = new DnsEndPoint(Variables.AdbIpPort.Split(':')[0], Convert.ToInt32(Variables.AdbIpPort.Split(':')[1]));
                AdbServer.Instance.StartServer(Environment.CurrentDirectory + "\\adb\\adb.exe", false);
                IAdbClient adbClient = AdbClient.Instance;
                adbClient.Connect(dnsEnd);
                Variables.ScriptLog("Using special connection method for Nemu. Emulators detected: " + adbClient.GetDevices().Count, Color.Aqua);
                if (adbClient.GetDevices().Count > 0)
                {
                    Variables.Controlled_Device = adbClient.GetDevices().First();
                }
                if (Variables.Controlled_Device == null)
                {
                    Thread.Sleep(1000);
                    StartEmulator();
                }
                Thread.Sleep(10000);
            }
            catch (SocketException)
            {

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while starting emulator! Error message: " + ex.Message);
                Environment.Exit(0);
            }
        }
    }
}
