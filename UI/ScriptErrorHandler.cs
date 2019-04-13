using BotFramework;
using SharpAdbClient;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UI
{
    class ScriptErrorHandler
    {
        public static List<Bitmap> errorImages = new List<Bitmap>();
        public static bool PauseErrorHandler;
        /// <summary>
        /// Check some error message that need to restart the game
        /// </summary>
        public static void ErrorHandle()
        {
            if (PauseErrorHandler)
            {
                Thread.Sleep(1000);
                return;
            }
            if (Variables.Proc != null)
            {
                try
                {
                    Parallel.ForEach(errorImages, error => 
                    {
                        var crop = BotCore.CropImage(Script.image, new Point(350, 180), new Point(980, 515));
                        Thread.Sleep(1000);
                        Point? p = BotCore.FindImage(crop, error, false);
                        if (p != null)
                        {
                            BotCore.SendTap(p.Value);
                            BotCore.KillGame("com.nubee.valkyriecrusade");
                            Reset("Error message found!");
                        }
                    });
                }
                catch
                {

                }
            }
        }
        //Reset back to just started the script
        public static void Reset(string log)
        {
            PrivateVariable.InMainScreen = false;
            PrivateVariable.InEventScreen = false;
            PrivateVariable.Battling = false;
            PrivateVariable.InMap = false;
            Variables.ScriptLog(log,Color.Yellow);
        }
    }
}
