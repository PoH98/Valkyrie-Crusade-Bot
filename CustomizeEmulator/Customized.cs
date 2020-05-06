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
        private static readonly string emulatorINI = "Emulator.ini";
        private static IniData Config;
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
                            if (!File.Exists(path))
                            {
                                OpenFileDialog folder = new OpenFileDialog();
                                if (folder.ShowDialog() == DialogResult.OK)
                                {
                                    ModifyConfig("Emulator", "ExePath", folder.FileName);
                                    Variables.VBoxManagerPath = folder.FileName;
                                }
                                else
                                {
                                    ModifyConfig("Emulator", "UseThis", "false");
                                    readsuccess = false;
                                }
                            }
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
                            if (!Directory.Exists(shared))
                            {
                                FolderBrowserDialog folder = new FolderBrowserDialog();
                                if(folder.ShowDialog() == DialogResult.OK)
                                {
                                    ModifyConfig("Emulator", "PC_SharedPath", folder.SelectedPath);
                                    Variables.SharedPath = folder.SelectedPath;
                                }
                                else
                                {
                                    readsuccess = false;
                                }
                            }
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
                        if(FindConfig("Click", "Multiplier", out string value))
                        {
                            if(decimal.TryParse(value, out decimal multiplier))
                            {
                                Variables.ClickPointMultiply = multiplier;
                            }
                        }
                    }
                }
                else
                {
                    readsuccess = false;
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
                ModifyConfig("Emulator", "DefaultInstance", "");
                ModifyConfig("Emulator", "ProcessName", "");
                ModifyConfig("Emulator", "Arguments", "");
                ModifyConfig("Click", "Multiplier", "1");
                readsuccess = false;
            }
            if (Variables.AndroidSharedPath == "Null")
            {
                Variables.ForceWinApiCapt = true;
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
            if(FindConfig("Emulator", "Argument", out string args))
            {
                Variables.Proc = Process.Start(Variables.VBoxManagerPath, args);
            }
            else
            {
                Variables.Proc = Process.Start(Variables.VBoxManagerPath);
            }

        }

        public string EmulatorDefaultInstanceName()
        {
            if(FindConfig("Emulator", "DefaultInstance", out string value))
            {
                return value;
            }
            return "";
        }

        public string EmulatorProcessName()
        {
            if (FindConfig("Emulator", "ProcessName", out string value))
            {
                return value;
            }
            return "";
        }

        public void UnUnBotify()
        {
            
        }
    }
}
