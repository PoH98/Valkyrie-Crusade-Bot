using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace BotFramework
{
    /// <summary>
    /// Logs writing
    /// </summary>
    public class Debug_
    {
        private static string FileName;
        private static StreamWriter s;
        private static bool EncryptLog;
        /// <summary>
        /// Get a log file prepaired
        /// <param name="encrypt">Check if log have to be encrypted from users!</param>
        /// </summary>
        public static void PrepairDebug(bool encrypt)
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
                FileName = "Profiles\\" + Variables.Instance + "\\Logs\\" + DateTime.Now.ToString().Replace(' ', '_').Replace('/', '_').Replace(':', '_') + ".log";
                if (!Directory.Exists("Profiles\\" + Variables.Instance + "\\Logs\\"))
                    Directory.CreateDirectory("Profiles\\" + Variables.Instance + "\\Logs\\");
                s = File.AppendText(FileName);
            }
            else
            {
                s = File.AppendText(FileName);
            }
            EncryptLog = encrypt;
        }
        /// <summary>
        /// Write Log to log file. Needed to be prepaired log file first!
        /// </summary>
        /// <param name="log_"></param>
        /// <param name="lineNumber"></param>
        /// <param name="caller"></param>
        public static void WriteLine(string log_, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            if (!ScriptRun.Run)
            {
                return;
            }
            try
            {
                if(s != null)
                {
                    if (EncryptLog)
                    {
                        s.WriteLine(Encrypt(log_ + " Line: " + lineNumber + " Caller: " + caller as string));
                    }
                    else
                    {
                        s.WriteLine(log_ + " Line: " + lineNumber + " Caller: " + caller as string);
                    }
                }
            }
            catch
            {

            }
        }
        /// <summary>
        /// Write Log to log file. Needed to be prepaired log file first!
        /// </summary>
        /// <param name="lineNumber"></param>
        /// <param name="caller"></param>
        public static void WriteLine([CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            if (!ScriptRun.Run)
            {
                return;
            }
            try
            {
                if(s != null)
                {
                    if (EncryptLog)
                    {
                        s.WriteLine(Encrypt("Line: " + lineNumber + " Called by : " + caller as string));
                    }
                    else
                    {
                        s.WriteLine("Line: " + lineNumber + " Called by : " + caller as string);
                    }
                }
            }
            catch
            {

            }

        }

        private static string Encrypt(string log)
        {
            string newlog = "[" + DateTime.Now + "]: " + log;
            return Encryption.Encrypt(newlog);
        }
    }
}
