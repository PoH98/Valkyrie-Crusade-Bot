using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace BotFramework
{
    public class Debug_
    {
        private static string FileName;
        static StreamWriter s;

        public static void PrepairDebug()
        {
            if (!Directory.Exists("Profiles\\" + Variables.Instance + "\\Logs\\"))
            {
                Directory.CreateDirectory(("Profiles\\" + Variables.Instance + "\\Logs\\"));
            }
            foreach (var file in Directory.GetFiles("Profiles\\" + Variables.Instance + "\\Logs\\"))
            {
                FileInfo fi = new FileInfo(file);
                if (fi.Length < 10000)
                {
                    File.Delete(file);
                }
                if ((DateTime.Now - fi.CreationTime).Days > 3)
                {
                    File.Delete(file);
                }
            }
            if (FileName == null)
            {
                Debug_.FileName = "Profiles\\" + Variables.Instance + "\\Logs\\" + DateTime.Now.ToString().Replace(' ', '_').Replace('/', '_').Replace(':', '_') + ".log";
                if (!Directory.Exists("Profiles\\" + Variables.Instance + "\\Logs\\"))
                    Directory.CreateDirectory("Profiles\\" + Variables.Instance + "\\Logs\\");
                s = File.AppendText(FileName);
            }
        }

        public static void WriteLine(string log_, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            if (!ScriptRun.Run)
            {
                return;
            }
            try
            {
                s.WriteLine(Encrypt(log_ + " Line: " + lineNumber + " Caller: " + caller as string));
            }
            catch
            {

            }
        }

        public static void WriteLine([CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            if (!ScriptRun.Run)
            {
                return;
            }
            try
            {
                s.WriteLine(Encrypt("Line: " + lineNumber + " Called by : " + caller as string));
            }
            catch
            {

            }

        }

        protected static string Encrypt(string log)
        {
            string newlog = "[" + DateTime.Now + "]: " + log;
            return BotCore.Encrypt(newlog);
        }
    }
}
