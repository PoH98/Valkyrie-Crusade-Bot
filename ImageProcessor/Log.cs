using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace BotFramework
{
    /// <summary>
    /// Writting Logs
    /// </summary>
    public class Log : Variables
    {
        /// <summary>
        /// The advanced log which might not show, except enabled for debuging...
        /// </summary>
        /// <param name="log"></param>
        /// <param name="lineNumber"></param>
        /// <param name="caller"></param>
        public static void AdvanceLog(string log, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            if (Instance.richTextBox != null)
            {
                try
                {
                    if (Instance.AdvanceLogShow)
                    {
                        Instance.richTextBox.Invoke((MethodInvoker)delegate
                        {
                            Instance.richTextBox.SelectionColor = Color.Red;
                            Instance.richTextBox.AppendText("[" + DateTime.Now.ToLongTimeString() + "]:" + log + "\n");
                        });
                    }
                    Instance.Debug.WriteLine(log, lineNumber, caller);
                }
                catch
                {

                }
            }
        }
        /// <summary>
        /// Script log for showing
        /// </summary>
        /// <param name="log">The log</param>
        /// <param name="color">The color of log</param>
        /// <param name="lineNumber"></param>
        /// <param name="caller"></param>
        public static void ScriptLog(string log, Color color, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            if (Instance.richTextBox != null)
            {
                try
                {
                    Instance.richTextBox.Invoke((MethodInvoker)delegate
                    {
                        Instance.richTextBox.SelectionColor = color;
                        Instance.richTextBox.AppendText("[" + DateTime.Now.ToLongTimeString() + "]:" + log + "\n");
                    });
                    Instance.Debug.WriteLine("ScriptLog: " + log, lineNumber, caller);
                }
                catch
                {

                }
            }

        }
        /// <summary>
        /// Script log for showing
        /// </summary>
        /// <param name="log">The log</param>
        /// <param name="lineNumber"></param>
        /// <param name="caller"></param>
        public static void ScriptLog(string log, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            ScriptLog(log, Color.Black, lineNumber, caller);
        }
    }
}
