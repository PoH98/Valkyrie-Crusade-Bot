using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace ImageProcessor
{
    public class Debug_
    {
        public static bool Enable_Debug = false;

        protected static string FileName;

        public static void PrepairDebug()
        {
            foreach (string file in Directory.GetFiles(Environment.CurrentDirectory + "\\Profiles\\" + EmulatorController.profilePath + "\\", "*.log"))
            {
                string name = file.Split('\\')[file.Split('\\').Length - 1];
                name = name.Replace(".log", "");
                try
                {
                    DateTime logtime = DateTime.MinValue;
                    DateTime.TryParse(name, out logtime);
                    TimeSpan saved = DateTime.Now - logtime;
                    if (saved.Days > 3)
                    {
                        File.Delete(file);
                    }
                }
                catch
                {

                }
            }
            if (Enable_Debug)
            {
                if (FileName == null)
                {
                    FileName = Environment.CurrentDirectory + "\\Profiles\\" + EmulatorController.profilePath + "\\" + DateTime.Now.ToString().Replace(' ','_').Replace('/','_').Replace(':','_') + ".log";
                    if(!File.Exists(FileName))
                        File.Create(FileName);
                }
            }
        }

        protected static Thread t, controller;

        public static void WriteLine(string log_, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            if (Enable_Debug)
            {
                try
                {
                    using (StreamWriter s = File.AppendText(FileName))
                    {
                        s.WriteLine(Encrypt(log_ + " Line: " + lineNumber + " Caller: " + caller as string));
                    }
                }
                catch
                {
                    WriteLine(log_, lineNumber,caller);
                }
            }
        }

       

        public static void WriteLine([CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            if (Enable_Debug)
            {
                try
                {
                    using (StreamWriter s = File.AppendText(FileName))
                    {
                        s.WriteLine(Encrypt("Debug Called. Line: " + lineNumber + " Caller: " + caller));
                    }
                }
                catch
                {
                    WriteLine(lineNumber, caller);
                }
                
            }
        }

        protected static string Encrypt(string log)
        {
            string newlog = "[" + DateTime.Now + "]: " + log;
            StringBuilder sb = new StringBuilder();
            foreach (char c in newlog)
            {
                char enc = c;
                char y = (char)(Convert.ToUInt16(enc) + 14);
                sb.Append(y);
            }
            return  sb.ToString();
        }
    }
}
