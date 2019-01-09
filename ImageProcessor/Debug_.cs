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
                log.Add(log_ + " Line: " + lineNumber + " Caller: " + caller);
                if (controller == null)
                {
                    controller = new Thread(Thread_Controller);
                    controller.Start();
                    controller.IsBackground = true;
                }
            }
        }

        public static void WriteLine(Image log_, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            if (Enable_Debug)
            {
                log.Add(log_);
                log.Add("Save Debug Image called. Line: " + lineNumber + " Caller: " + caller);
                if (controller == null)
                {
                    controller = new Thread(Thread_Controller);
                    controller.Start();
                    controller.IsBackground = true;
                }
            }
        }

        public static void WriteLine([CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            if (Enable_Debug)
            {
                log.Add("Debug Called. Line: " + lineNumber + " Caller: " + caller);
                if (controller == null)
                {
                    controller = new Thread(Thread_Controller);
                    controller.Start();
                    controller.IsBackground = true;
                }
            }
        }

        protected static void Thread_Controller()
        {
            while (true)
            {
                if(t == null)
                {
                    t = new Thread(Write);
                    t.Start();
                }
                if (t.IsAlive)
                {
                    Thread.Sleep(100);
                }
                else
                {
                    t = new Thread(Write);
                    t.Start();
                }
            }
        }

        protected static List<object> log = new List<object>();

        private static void Write()
        {
            if (Enable_Debug)
            {
                for (int x = 0; x < log.Count; x++)
                {
                    try
                    {
                        if (log[x] is string)
                        {
                            using (StreamWriter s = File.AppendText(FileName))
                            {
                                s.WriteLine(Encrypt(log[x] as string));
                            }
                        }
                        else if (log[x] is Image)
                        {
                            (log[x] as Image).Save(Environment.CurrentDirectory + "\\Profiles\\" + EmulatorController.profilePath + "\\" + DateTime.Now + ".debug");
                        }
                        log.Remove(log[x]);
                    }
                    catch
                    {

                    }
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
