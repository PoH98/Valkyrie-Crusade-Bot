using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BotFramework;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace Mumu
{
    public class Mumu : EmulatorInterface
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
            return "Mumu/Nemu";
        }

        public bool LoadEmulatorSettings()
        {
            RegistryKey reg;
            if (BotCore.Is64BitOperatingSystem())
            {
                reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Nemu");
            }
            else
            {
                reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Nemu");
            }
            var installpath = reg.GetValue("DisplayIcon").ToString();
            return false;

        }

        public void SetResolution(int x, int y, int dpi)
        {
            throw new NotImplementedException();
        }

        public void StartEmulator()
        {
            throw new NotImplementedException();
        }
    }
}
