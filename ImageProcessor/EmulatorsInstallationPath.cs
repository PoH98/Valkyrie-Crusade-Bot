using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImageProcessor
{
    public class EmulatorsInstallationPath
    {
        public static bool MEmu(out string path)
        {
            RegistryKey reg = Registry.LocalMachine;
            try
            {
                var r = reg.OpenSubKey("SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\MEmu");
                var result = r.GetValue("InstallLocation");
                if (result != null)
                {
                    path = result.ToString();
                    if (File.Exists(path))
                    {
                        return true;
                    }
                }
            }
            catch
            {
                
            }
            path = null;
            return false;
        }

        public static  bool Bluestack(out string path)
        {
            bool Plus = false;
            string[] frontend = { "HD-Frontend.exe" , "HD-Player.exe" };
            RegistryKey reg = Registry.LocalMachine;
            try
            {
                var r = reg.OpenSubKey("SOFTWARE\\BlueStacks\\");
                if (r.GetValue("Engine").ToString().Contains("plus"))
                {
                    Plus = true;
                }
                if (Plus)
                {
                    frontend = new string[] { "HD-Plus-Frontend.exe" };
                }
                path = r.GetValue("InstallDir").ToString();
                if (!File.Exists(path + frontend[0]))
                {
                    path = path + frontend[0];
                }
                else
                {
                    path = path + frontend[1];
                }
                if (File.Exists(path))
                {
                    return true;
                }
            }
            catch
            {

            }
            path = null;
            return false;
        }

        public static bool Nox(out string path)
        {
            RegistryKey reg = Registry.LocalMachine;
            try
            {
                var r = reg.OpenSubKey(@"SOFTWARE\BigNox\VirtualBox\");
                var result = r.GetValue("InstallDir");
                if (result != null)
                {
                    path = result.ToString();
                    if (File.Exists(path))
                    {
                        return true;
                    }
                }
                if (Is64BitOperatingSystem())
                {
                    r = reg.OpenSubKey("SOFTWARE\\Wow6432Node\\DuoDianOnline\\SetupInfo\\");
                    result = r.GetValue("InstallPath");
                    if (result != null)
                    {
                        path = result.ToString();
                        if (File.Exists(path))
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    r = reg.OpenSubKey("SOFTWARE\\DuoDianOnline\\SetupInfo\\");
                    result = r.GetValue("InstallPath");
                    if (result != null)
                    {
                        path = result.ToString();
                        if (File.Exists(path))
                        {
                            return true;
                        }
                    }
                }
            }
            catch
            {
                
            }
            path = null;
            return false;
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
    }

    public class EmulatorConfigProgram
    {
        /// <summary>
        /// Set the Variables.AdbIpPort, Variables.SharedPath & Variables.VBoxManagerPath of MEmu
        /// </summary>
        /// <param name="path">MEmu path by using EmulatorsInstallationPath.MEmu</param>
        public static void MEmu(string path)
        {
            string vbox = path + @"\MEmuHyperv\MEmuManage.exe";
            ProcessStartInfo fetch = new ProcessStartInfo(vbox);
            if (Variables.Instance.Length > 0)
            {
                fetch.Arguments = "showvminfo " + Variables.Instance;
            }
            else
            {
                fetch.Arguments = "showvminfo MEmu";
            }
            var codePage = Console.OutputEncoding.CodePage;
            fetch.RedirectStandardOutput = true;
            fetch.UseShellExecute = false;
            fetch.CreateNoWindow = true;
            fetch.StandardOutputEncoding = Encoding.GetEncoding(codePage);
            Process fetching = Process.Start(fetch);
            string result = fetching.StandardOutput.ReadToEnd();
            string[] splitted = result.Split('\n');
            foreach (var s in splitted)
            {
                if (s.Contains("Name: 'download', Host path:"))
                {
                    string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
                    Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
                    var p = s.ToLower().Replace("name: 'download', host path: '", "").Replace("' (machine mapping), writable", "");
                    p = r.Replace(p, "\\");
                    p = path.Replace("\\\\", ":\\");
                    Variables.SharedPath = path;
                    if (!Directory.Exists(path))
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
            Variables.VBoxManagerPath = vbox;
        }

        /// <summary>
        /// Set the Variables.AdbIpPort, Variables.SharedPath & Variables.VBoxManagerPath of Bluestack
        /// </summary>
        /// <param name="path">Bluestack path by using EmulatorsInstallationPath.Bluestack</param>
        public static void Bluestack(string path)
        {
            Variables.VBoxManagerPath = path + "\\BstkVMMgr.exe";
            RegistryKey reg = Registry.LocalMachine;
            if(Variables.Instance.Length < 1)
            {
                Variables.Instance = "Android";
            }
            var r = reg.OpenSubKey(@"SOFTWARE\BlueStacks\Guests\" + Variables.Instance + @"\Config\");
            var result = r.GetValue("BstAdbPort");
            Variables.AdbIpPort = "127.0.0.1:" + result.ToString();
            for(int x = 0; x < 5; x++)
            {
                r = reg.OpenSubKey(@"SOFTWARE\BlueStacks\Guests\" + Variables.Instance + @"\SharedFolder\" + x);
                result = r.GetValue("Name");
                if (result.ToString().Contains("BstSharedFolder"))
                {
                    r = reg.OpenSubKey(@"SOFTWARE\BlueStacks\Guests\" + Variables.Instance + @"\SharedFolder\" + x);
                    Variables.SharedPath = r.GetValue("Path").ToString();
                    return;
                }
            }
        }

        public static void Nox(string path)
        {

        }
    }
}
