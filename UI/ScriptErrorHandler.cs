using ImageProcessor;
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
        private static List<Bitmap> errorImages = new List<Bitmap>();
        //Click away all error messages
        public static void ErrorHandle()
        {
            foreach (var f in Directory.GetFiles("Img\\Errors"))
            {
                Thread.Sleep(10);
                using (Stream bmp = File.Open(f, FileMode.Open))
                {
                    Image temp = Image.FromStream(bmp);
                    errorImages.Add(new Bitmap(temp));
                }
            }
            while (PrivateVariable.Run)
            {
                if (Variables.Proc != null)
                {
                    try
                    {
                        foreach (var e in errorImages)
                        {
                            Thread.Sleep(1000);
                            Point? p = EmulatorController.FindImage(Script.image, e, false);
                            if (p != null)
                            {
                                EmulatorController.SendTap(p.Value);
                                EmulatorController.KillGame("com.nubee.valkyriecrusade");
                                Reset("Error message found!");
                            }
                        }
                    }
                    catch
                    {

                    }
                    WentShop();
                }

            }
        }
        //Reset back to just started the script
        public static void Reset(string log)
        {
            PrivateVariable.InMainScreen = false;
            PrivateVariable.InEventScreen = false;
            PrivateVariable.Battling = false;
            PrivateVariable.EventType = -1;
            Variables.ScriptLog.Add(log);
        }
        //Check the script went to shop
        private static void WentShop()
        {
            Point? loc = EmulatorController.FindImage(Script.image, "Img\\Errors\\Shop\\Background.png", false);
            if (loc != null)
            {
                EmulatorController.KillGame("com.nubee.valkyriecrusade");
                Variables.ScriptLog.Add("Entered Shop! Maybe no energy left?");
                Reset("Critical error found! Trying to restart game!");
            }
            else
            {
                Thread.Sleep(1000);
                loc = EmulatorController.FindImage(Script.image, "Img\\Errors\\Shop\\Background_Light.png", false);
                if(loc != null)
                {
                    EmulatorController.KillGame("com.nubee.valkyriecrusade");
                    Variables.ScriptLog.Add("Entered Shop! Maybe no energy left?");
                    Reset("Critical error found! Trying to restart game!");
                }
            } 

        }
    }
}
