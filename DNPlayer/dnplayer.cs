using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BotFramework;
using Microsoft.Win32;

namespace DNPlayer
{
    public class dnplayer : EmulatorInterface
    {
        public void CloseEmulator()
        {
            Process.Start(Variables.VBoxManagerPath, "quit --index 0");
        }

        public void ConnectEmulator()
        {
            int id = -1, error = 0;
            do
            {
                ProcessStartInfo start = new ProcessStartInfo(Variables.VBoxManagerPath);
                start.Arguments = "list2";
                start.RedirectStandardOutput = true;
                start.UseShellExecute = false;
                var proc = Process.Start(start);
                var result = proc.StandardOutput.ReadToEnd();
                id = int.Parse(result.Split(',')[5]);
                error++;
                if(id == -1)
                {
                    BotCore.Delay(1000,1500);
                }
            }
            while (id == -1 && error < 30);
            if(id == -1)
            {
                Variables.ScriptLog("Unable to connect to emulator!",Color.Red);
                throw new Exception("LDPlayer Emulator refused to load!");
            }
            Variables.Proc = Process.GetProcessById((id));
            //Start ldemulator's adb, as it only respond to its adb

        }

        public string EmulatorName()
        {
            return "LDEmulator";
        }

        public bool LoadEmulatorSettings()
        {
            try
            {
                RegistryKey reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\dnplayer");
                Variables.VBoxManagerPath = reg.GetValue("DisplayIcon").ToString().Replace("dnplayer.exe", "ldconsole.exe");
                Debug_.WriteLine("DNPlayer detected, path: " + Variables.VBoxManagerPath);
                Variables.AdbIpPort = "127.0.0.1:5555";
                Variables.AndroidSharedPath = "/sdcard/Pictures/";
                Variables.SharedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\雷电模拟器\Pictures\";
                return true;
            }
            catch
            {

            }
            return false;
        }

        public void SetResolution(int x, int y)
        {
            Process.Start(Variables.VBoxManagerPath, "modify --index 0 --resolution " + x + "," + y + ",160");
        }

        public void StartEmulator()
        {
            Process.Start(Variables.VBoxManagerPath, "launch --index 0");
            BotCore.Delay(5000, 10000);
        }
    }
}
