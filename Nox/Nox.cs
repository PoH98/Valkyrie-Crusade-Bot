using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using BotFramework;
using Microsoft.Win32;

namespace Nox
{
    public class Nox : EmulatorInterface
    {
        private static string NoxFile;
        public string EmulatorName()
        {
            return "Nox|夜神模拟器";
        }

        public bool LoadEmulatorSettings()
        {
            RegistryKey reg = Registry.LocalMachine;
            string path;
            var r = reg.OpenSubKey(@"SOFTWARE\BigNox\VirtualBox\");
            if(r != null)
            {
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
                catch(Exception ex)
                {
                    Variables.AdvanceLog(ex.ToString());
                }
            }
            if (BotCore.Is64BitOperatingSystem())
            {
                r = reg.OpenSubKey("SOFTWARE\\Wow6432Node\\DuoDianOnline\\SetupInfo\\");
                if(r != null)
                {
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
            }
            else
            {
                r = reg.OpenSubKey("SOFTWARE\\DuoDianOnline\\SetupInfo\\");
                if(r != null)
                {
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
            }
            return false;
        }

        private static string GetRTPath()
        {
            var path = Environment.ExpandEnvironmentVariables("%ProgramW6432%") + "\\BigNox\\BigNoxVM\\RT\\";
            if (!Directory.Exists(path))
            {
                path = Environment.ExpandEnvironmentVariables("%ProgramFiles(x86)%") + "\\BigNox\\BigNoxVM\\RT\\";
            }
            if (!Directory.Exists(path))
            {
                path = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) +"\\BigNox\\BigNoxVM\\RT\\";
            }
            if (!Directory.Exists(path))
            {
                RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\BigNox\VirtualBox\");
                var value = key.GetValue("InstallDir");
                if (value != null)
                {
                    path = value.ToString();
                }
                else
                {
                    path = string.Empty;
                }
            }
            return path;
        }
        private static void InitNox()
        {
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
            Regex host = new Regex(".*host ip = ([^,]+), .* guest port = 5555");
            var regexresult = host.Match(result).Value;
            string ip = "127.0.0.1", port= "62001";
            ip = regexresult.Substring(regexresult.IndexOf("host ip = ") + 10);
            ip = ip.Remove(ip.IndexOf("host port =") - 2);
            port = regexresult.Substring(regexresult.IndexOf("host port = ") + 12, 5);

            Variables.AdbIpPort = ip + ":" + port;//Adb Port Get
            Regex regex = new Regex("Name: 'Other', Host path: '(.*)'.*");
            var match = regex.Match(result);
            if (match.Success)
            {
                var shared = match.Value.Substring(match.Value.IndexOf("'",25));
                Variables.SharedPath = shared.Remove(shared.LastIndexOf("'"));
            }
            else
            {
                if (Directory.Exists((Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Nox_share\Other\").Replace("\\\\", "\\")))
                {
                    Variables.SharedPath = (Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Nox_share\Other\").Replace("\\\\", "\\");
                }
                else if (Directory.Exists((Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Nox_share\OtherShare\").Replace("\\\\", "\\")))
                {
                    Variables.SharedPath = (Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Nox_share\OtherShare\").Replace("\\\\", "\\");
                }
                else if (Directory.Exists((Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Nox_share\").Replace("\\\\", "\\")))
                {
                    Variables.SharedPath = (Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Nox_share\").Replace("\\\\", "\\");
                }
                else
                {
                    Variables.ForceWinApiCapt = true;
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
                    Process.Start("Profiles\\" + AdbInstance.Instance.profilePath + "\\bot.ini");
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
                Process.Start("Profiles\\" + AdbInstance.Instance.profilePath + "\\bot.ini");
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

        public string EmulatorDefaultInstanceName()
        {
            return "Nox";
        }

        public string EmulatorProcessName()
        {
            return "Nox|夜神模拟器";
        }

        public void UnUnBotify()
        {
            
        }
    }
}
