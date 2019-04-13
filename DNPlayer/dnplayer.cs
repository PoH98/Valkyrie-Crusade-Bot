using System;
using System.Collections.Generic;
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
            throw new NotImplementedException();
        }

        public void ConnectEmulator()
        {
            throw new NotImplementedException();
        }

        public string EmulatorName()
        {
            throw new NotImplementedException();
        }

        public bool LoadEmulatorSettings()
        {
            try
            {
                RegistryKey reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\dnplayer");
                Variables.VBoxManagerPath = reg.GetValue("DisplayIcon").ToString();
                Debug_.WriteLine("DNPlayer detected, path: " + Variables.VBoxManagerPath);

            }
            catch
            {
                
            }
            return false;
        }

        public void SetResolution(int x, int y)
        {
            throw new NotImplementedException();
        }

        public void StartEmulator()
        {
            throw new NotImplementedException();
        }
    }
}
