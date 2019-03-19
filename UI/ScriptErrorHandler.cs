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
        public static bool PauseErrorHandler;
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
                Thread.Sleep(500);
                if (PauseErrorHandler)
                {
                    Thread.Sleep(1000);
                    return;
                }
                if (Variables.Proc != null)
                {
                    try
                    {
                        foreach (var e in errorImages)
                        {
                            var crop = EmulatorController.CropImage(Script.image, new Point(350,180), new Point(980,515));
                            Point? p = EmulatorController.FindImage(crop, e, false);
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
                    //Went Shop
                    Point? loc = EmulatorController.FindImage(Script.image, "Img\\Errors\\Shop\\Background.png", false);
                    if (loc != null)
                    {
                        EmulatorController.KillGame("com.nubee.valkyriecrusade");
                        Variables.ScriptLog.Add("Entered Shop! Maybe no energy left?");
                        Reset("Critical error found! Trying to restart game!");
                    }
                    else
                    {
                        loc = EmulatorController.FindImage(Script.image, "Img\\Errors\\Shop\\Background_Light.png", false);
                        if (loc != null)
                        {
                            EmulatorController.KillGame("com.nubee.valkyriecrusade");
                            Variables.ScriptLog.Add("Entered Shop! Maybe no energy left?");
                            Reset("Critical error found! Trying to restart game!");
                        }
                    }
                    if (PrivateVariable.Battling == true && PrivateVariable.EventType == 2)
                    {
                        var point = EmulatorController.FindImage(Script.image, "Img\\HellLoc.png", false);
                        if (point != null)
                        {
                            PrivateVariable.Battling = false;
                            Variables.ScriptLog.Add("Battle Ended!");
                            return;
                        }
                        Thread.Sleep(100);
                    }
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
            PrivateVariable.EventType = -1;
            Variables.ScriptLog.Add(log);
        }
    }
}
