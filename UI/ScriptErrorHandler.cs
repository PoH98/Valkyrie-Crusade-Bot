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
        public static bool ErrorHandle()
        {
            if (Variables.Proc != null)
            {
                try
                {
                    if (BotCore.RGBComparer(new Point(145, 170), Color.Black, 2, VCBotScript.image) && BotCore.RGBComparer(new Point(1000, 355), Color.Black, 2, VCBotScript.image))
                    {
                        if (BotCore.RGBComparer(new Point(410, 285), Color.FromArgb(25,44,58), 10, VCBotScript.image) && BotCore.RGBComparer(new Point(400, 430), Color.FromArgb(56, 98, 128), 10, VCBotScript.image))
                        {
                            //Error Messagebox found
                            BotCore.KillGame("com.nubee.valkyriecrusade");
                            Reset("Error message found!");
                            return true;
                        }
                    }
                }
                catch
                {

                }
                return false;
                /*try
                {
                    var crop = Screenshot.CropImage(VCBotScript.image, new Point(350, 180), new Point(980, 515));
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
            return true;
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
