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
        protected static string FileName;

        public static void PrepairDebug()
        {
                if (FileName == null)
                {
                    Debug_.FileName = "Profiles\\" + Variables.Instance + "\\Logs\\" + DateTime.Now.ToString().Replace(' ', '_').Replace('/', '_').Replace(':', '_') + ".log";
                    if (!Directory.Exists("Profiles\\" + Variables.Instance + "\\Logs\\"))
                        Directory.CreateDirectory("Profiles\\" + Variables.Instance + "\\Logs\\");
                    if (!File.Exists(FileName))
                        File.Create(FileName);
            }
        }

        protected static Thread t, controller;

        public static void WriteLine(string log_, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
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

                }
            
        }

        public static void WriteLine([CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
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

                }
            
        }

        public static void WriteLine(byte[] img)
        {
                try
                {
                    File.WriteAllBytes(FileName.Remove(FileName.LastIndexOf('\\'))+ "\\debug.png",img);
                }
                catch
                {

                }
            
        }

        protected static string Encrypt(string log)
        {
            string newlog = "[" + DateTime.Now + "]: " + log;
            return EmulatorController.Encrypt(newlog);
        }
    }
}
