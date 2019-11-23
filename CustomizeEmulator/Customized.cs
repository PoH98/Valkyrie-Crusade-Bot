using System;
using System.Diagnostics;
using System.IO;
using BotFramework;
using IniParser;
using IniParser.Model;
using System.Text;
using System.Windows.Forms;

namespace CustomizeEmulator
{
    public class Customized : EmulatorInterface
    {
        private static Process process;
        private static readonly string emulatorINI = "Emulator.ini";
        private static IniData Config;
        public void CloseEmulator()
        {
            if(process != null)
            {
                if (!process.HasExited)
                {
                    BotCore.KillProcessAndChildren(process.Id);
                }
            }
        }

        public void ConnectEmulator()
        {
            
        }

        public string EmulatorName()
        {
            return "Customize Emulator";
        }

        public bool LoadEmulatorSettings()
        {
            bool readsuccess = true;
            if (!File.Exists("Emulator.ini"))
            {
                File.WriteAllText("Emulator.ini","");
            }
            FileIniDataParser parser = new FileIniDataParser();
            Config = parser.ReadFile(emulatorINI);
            if(FindConfig("Emulator", "UseThis", out string result))
            {
                if(bool.TryParse(result, out bool use))
                {
                    if (use)
                    {
                        if (FindConfig("Emulator", "ExePath", out string path))
                        {
                            Variables.VBoxManagerPath = path;
                        }
                        else
                        {
                            ModifyConfig("Emulator", "ExePath", "");
                            readsuccess = false;
                        }
                        if (FindConfig("Emulator", "Android_SharedPath", out string shared))
                        {
                            Variables.AndroidSharedPath = shared;
                        }
                        else
                        {
                            ModifyConfig("Emulator", "Android_SharedPath", "");
                            readsuccess = false;
                        }
                        if (FindConfig("Emulator", "PC_SharedPath", out shared))
                        {
                            Variables.SharedPath = shared;
                        }
                        else
                        {
                            ModifyConfig("Emulator", "PC_SharedPath", "");
                            readsuccess = false;
                        }
                        if(FindConfig("Emulator", "Adb_Ip", out string ip))
                        {
                            if(FindConfig("Emulator", "Adb_Port", out string port))
                            {
                                Variables.AdbIpPort = ip + ":" + port;
                            }
                            else
                            {
                                ModifyConfig("Emulator", "Adb_Port", "");
                                readsuccess = false;
                            }
                        }
                        else
                        {
                            ModifyConfig("Emulator", "Adb_Ip", "");
                            readsuccess = false;
                        }
                    }
                }
                else
                {
                    
                }
            }
            else
            {
                ModifyConfig("Emulator", "UseThis", "false");
                ModifyConfig("Emulator", "ExePath", "");
                ModifyConfig("Emulator", "Android_SharedPath", "");
                ModifyConfig("Emulator", "PC_SharedPath", "");
                ModifyConfig("Emulator", "Adb_Ip", "");
                ModifyConfig("Emulator", "Adb_Port", "");
                readsuccess = false;
            }
            
            return readsuccess;
        }
        private static void ModifyConfig(string section, string key, string value)
        {
            if (Config.Sections.ContainsSection(section))
            {
                if (Config[section].ContainsKey(key))
                {
                    Config[section][key] = value;
                }
                else
                {
                    Config[section].AddKey(key);
                    Config[section][key] = value;
                }
            }
            else
            {
                Config.Sections.AddSection(section);
                Config[section].AddKey(key);
                Config[section][key] = value;
            }
            SaveConfig();
        }
        private static void SaveConfig()
        {
            FileIniDataParser p = new FileIniDataParser();
            Config.Configuration.AssigmentSpacer = "";
            p.WriteFile(emulatorINI, Config, Encoding.Unicode);
        }
        private static bool FindConfig(string section, string key, out string result)
        {
            if (Config.Sections.ContainsSection(section))
            {
                if (Config[section].ContainsKey(key))
                {
                    result = Config[section][key];
                    return true;
                }
            }
            result = null;
            return false;
        }
        public void SetResolution(int x, int y, int dpi)
        {
            MessageBox.Show("You have to set the emulator resolution to 1280 * 720 for the bot works!");
        }

        public void StartEmulator()
        {
            Variables.Proc = Process.Start(Variables.VBoxManagerPath);
        }
    }
}
