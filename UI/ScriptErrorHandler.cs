using BotFramework;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace UI
{
    class ScriptErrorHandler
    {
        public static List<Bitmap> errorImages = new List<Bitmap>();
        /// <summary>
        /// Check some error message that need to restart the game
        /// </summary>
        public static void ErrorHandle()
        {
            if (Variables.Proc != null)
            {
                try
                {
                    var p = BotCore.FindImages(VCBotScript.image, errorImages.ToArray(), false, true);
                    if (p != null)
                    {
                        BotCore.KillGame("com.nubee.valkyriecrusade");
                        Reset("Error message found!");
                    }
                }
                catch
                {

                }

                /*try
                {
                    var crop = BotCore.CropImage(VCBotScript.image, new Point(350, 180), new Point(980, 515));
                    foreach(var error in errorImages)
                    {
                        BotCore.Delay(100);
                        Point? p = BotCore.FindImage(crop, error, false);
                        if (p != null)
                        {
                            BotCore.SendTap(p.Value);
                            BotCore.KillGame("com.nubee.valkyriecrusade");
                            Reset("Error message found!");
                        }
                    }
                }
                catch
                {

                }*/
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
