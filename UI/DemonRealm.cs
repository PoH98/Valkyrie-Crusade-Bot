using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BotFramework;
using ImgXml;
using System.Drawing;
using System.IO;

namespace UI
{
    class DemonRealm
    {
        public static void Demon_Realm()
        {
            Debug_.WriteLine();
            Point? point = null;
            int error = 0;
            while (true)
            {
                var image = BotCore.ImageCapture();
                point = BotCore.FindImage(image, Img.Close2, false);
                if (point != null)
                {
                    BotCore.SendTap(new Point(point.Value.X, point.Value.Y));
                    BotCore.Delay(1000, false);
                    continue;
                }
                point = BotCore.FindImage(image, Img.GreenButton, false);
                if (point != null)
                {
                    BotCore.SendTap(new Point(point.Value.X, point.Value.Y));
                    BotCore.Delay(1000, false);
                    continue;
                }
                if (!BotCore.GameIsForeground("com.nubee.valkyriecrusade"))
                {
                    return;
                }
                if (BotCore.RGBComparer( new Point(415, 678), Color.FromArgb(223, 192, 63), 10))
                {
                    PrivateVariable.VCevent = PrivateVariable.EventType.DemonRealm;
                    PrivateVariable.InEventScreen = true;
                    DemonStage_Enter();
                    return;
                }
                Variables.ScriptLog("Locating Demon Realm Event UI!", Color.White);
                if (BotCore.RGBComparer(new Point(600, 405), Color.FromArgb(59, 30, 37), 15))
                {
                    VCBotScript.Tower_Floor = OCR.OcrImage(BotCore.CropImage(image, new Point(300, 115), new Point(484, 142)), "eng");
                    VCBotScript.Tower_Rank = OCR.OcrImage(BotCore.CropImage(image, new Point(300, 150), new Point(458, 170)), "eng");
                    Variables.ScriptLog("Demon Realm Event Found!", Color.Lime);
                    PrivateVariable.InEventScreen = true;
                    VCBotScript.energy = VCBotScript.GetEnergy();
                    VCBotScript.runes = VCBotScript.GetRune();
                    break;
                }
                else
                {
                    BotCore.Delay(1000, false);
                    error++;
                    if (error > 20)
                    {
                        ScriptErrorHandler.Reset("Unable to locate event. Going back to main screen");
                        return;
                    }
                }
            }

            if (VCBotScript.energy == 0)
            {
                Variables.ScriptLog("Waiting for energy", Color.Yellow);
                Variables.ScriptLog("Close game and wait for energy because of no energy left", Color.Yellow);
                VCBotScript.NoEnergy();
                PrivateVariable.InEventScreen = false;
                PrivateVariable.InMainScreen = false;
                PrivateVariable.Battling = false;
                return;
            }
            Variables.ScriptLog("Enterting Stage", Color.White);
            BotCore.SendSwipe(new Point(307, 249), new Point(305, 403), 300);
            BotCore.Delay(1500);
            switch (MainScreen.Level)
            {
                case 0:
                    BotCore.SendTap(250, 284);
                    break;
                case 1:
                    if (BotCore.RGBComparer( new Point(143, 355), Color.FromArgb(51, 16, 5), 20))
                    {
                        Variables.ScriptLog("中级还没被解锁！自动往下挑战中！", Color.Red);
                        BotCore.SendTap(250, 284);
                        break;
                    }
                    BotCore.SendTap(362, 283);
                    break;
                case 2:
                    if (BotCore.RGBComparer( new Point(143, 355), Color.FromArgb(51, 16, 5), 20))
                    {
                        Variables.ScriptLog("上级还没被解锁！自动往下挑战中！", Color.Red);
                        if (BotCore.RGBComparer( new Point(324, 270), Color.FromArgb(51, 16, 5), 20))
                        {
                            Variables.ScriptLog("中级还没被解锁！自动往下挑战中！", Color.Red);
                            BotCore.SendTap(250, 284);
                            break;
                        }
                        BotCore.SendTap(362, 283);
                        break;
                    }
                    BotCore.SendTap(214, 370);
                    break;
                case 3:
                    if (BotCore.RGBComparer( new Point(324, 355), Color.FromArgb(51, 16, 5), 20))
                    {
                        Variables.ScriptLog("超上级还没被解锁！自动往下挑战中！", Color.Red);
                        if (BotCore.RGBComparer( new Point(143, 355), Color.FromArgb(51, 16, 5), 20))
                        {
                            Variables.ScriptLog("上级还没被解锁！自动往下挑战中！", Color.Red);
                            if (BotCore.RGBComparer( new Point(324, 270), Color.FromArgb(51, 16, 5), 20))
                            {
                                Variables.ScriptLog("中级还没被解锁！自动往下挑战中！", Color.Red);
                                BotCore.SendTap(250, 284);
                                break;
                            }
                            BotCore.SendTap(362, 283);
                            break;
                        }
                    }
                    BotCore.SendTap(353, 371);
                    break;
                case 4:
                    if (BotCore.RGBComparer( new Point(324, 355), Color.FromArgb(51, 16, 5), 20))
                    {
                        Variables.ScriptLog("超上级还没被解锁！自动往下挑战中！", Color.Red);
                        if (BotCore.RGBComparer( new Point(143, 355), Color.FromArgb(51, 16, 5), 20))
                        {
                            Variables.ScriptLog("上级还没被解锁！自动往下挑战中！", Color.Red);
                            if (BotCore.RGBComparer( new Point(324, 270), Color.FromArgb(51, 16, 5), 20))
                            {
                                Variables.ScriptLog("中级还没被解锁！自动往下挑战中！", Color.Red);
                                BotCore.SendTap(250, 284);
                                break;
                            }
                            BotCore.SendTap(362, 283);
                            break;
                        }
                    }
                    BotCore.SendTap(353, 371);
                    break;
            }
            bool EnteredStage = false;
            error = 0;
            do
            {
                if (BotCore.RGBComparer( new Point(959, 656), 31, 102, 26, 4))
                {
                    Variables.ScriptLog("Start battle", Color.Lime);
                    BotCore.SendTap(new Point(959, 656));
                    BotCore.Delay(2000, false);
                    if (VCBotScript.runes == 4 && VCBotScript.energy == 5)
                    {
                        BotCore.SendSwipe(new Point(640, 473), new Point(640, 280), 1000);
                        BotCore.Delay(500, false);
                    }
                    BotCore.SendTap(new Point(758, 566));
                    BotCore.Delay(6000, 8000);
                    BotCore.SendTap(640, 400); //Tap away Round Battle Text
                    BotCore.Delay(2000, false);
                    VCBotScript.stop.Start();
                    VCBotScript.energy--; //Calculate Energy used
                    if ((VCBotScript.nextOnline - DateTime.Now) < new TimeSpan(3, 15, 0))
                    {
                        VCBotScript.nextOnline = VCBotScript.nextOnline.AddMinutes(45);
                    }
                    EnteredStage = true;
                    BotCore.Delay(5000, false);
                    break;
                }
                else
                {
                    BotCore.Delay(1000, 1500);
                    error++;
                    if (error > 10)
                    {
                        return;
                    }
                }
                if (!BotCore.GameIsForeground("com.nubee.valkyriecrusade"))
                {
                    ScriptErrorHandler.Reset("Game is closed! Restarting all!");
                    return;
                }
            }
            while (!EnteredStage);
            DemonStage_Enter();
        }

        private static void DemonStage_Enter()
        {
            int error = 0;
            while (!BotCore.RGBComparer( new Point(415, 678), Color.FromArgb(223, 192, 63), 10))
            {
                if (!BotCore.GameIsForeground("com.nubee.valkyriecrusade"))
                {
                    ScriptErrorHandler.Reset("Game close, restarting...");
                    return;
                }
                error++;
                if (error > 10)
                {
                    ScriptErrorHandler.Reset("Event Locate Failed!");
                    BotCore.KillGame("com.nubee.valkyriecrusade");
                    return;
                }
                BotCore.Delay(1000, false);
            }
            error = 0;
            Variables.ScriptLog("Demon Realm Event Located", Color.Lime);
            List<Point> BlackListedLocation = new List<Point>();
            Variables.ScriptLog("Fetching stage images", Color.White);
            List<Image> Stage = new List<Image>();
            foreach (var file in Directory.GetFiles("Img\\DemonRealm", "*.png").OrderBy(f => f))
            {
                Stage.Add(Image.FromFile(file));
            }
            Point? p = null;
            while (error < 10 && p == null)
            {
                if (!BotCore.GameIsForeground("com.nubee.valkyriecrusade"))
                {
                    return;
                }
                VCBotScript.image = BotCore.ImageCapture();
                var crop = BotCore.CropImage(VCBotScript.image, new Point(0, 0), new Point(1280, 615));
                Variables.ScriptLog("Trying to find stages to enter", Color.LightSkyBlue);
                Bitmap screen = BotCore.Decompress(crop);
                foreach (var blacklist in BlackListedLocation)
                {
                    using (Graphics grf = Graphics.FromImage(screen))
                    {
                        using (Brush brsh = new SolidBrush(ColorTranslator.FromHtml("#000000")))
                        {
                            grf.FillEllipse(brsh, blacklist.X, blacklist.Y, 5, 5);
                        }
                    }
                }
                foreach (var stage in Stage)
                {
                    for (int x = 0; x < 4; x++)
                    {
                        p = BotCore.FindImage(screen, (Bitmap)stage, false);
                        if (p != null)
                        {
                            if (!BlackListedLocation.Contains(p.Value))
                            {
                                Variables.ScriptLog("Stage found!", Color.Lime);
                                BotCore.SendTap(p.Value);
                                BotCore.Delay(3000, false);
                                bool StageEnterable = false;
                                for(int y = 0;y < 10;y++)
                                {
                                    if (BotCore.RGBComparer(new Point(543, 122), Color.FromArgb(60, 106, 137), 20))
                                    {
                                        StageEnterable = true;
                                        break;
                                    }
                                    else
                                    {
                                        BotCore.Delay(1000);
                                    }
                                }
                                if (StageEnterable)
                                {
                                    BotCore.SendTap(768, 536);
                                    BotCore.Delay(4500, false);
                                    BotCore.SendTap(970, 614);
                                    BotCore.Delay(2000, false);
                                    BotCore.SendTap(753, 423);
                                }
                                else
                                {
                                    Variables.ScriptLog("Ops, looks like the stage is not able to enter!", Color.Red);
                                    BlackListedLocation.Add(p.Value);
                                    using (Graphics grf = Graphics.FromImage(screen))
                                    {
                                        using (Brush brsh = new SolidBrush(ColorTranslator.FromHtml("#000000")))
                                        {
                                            grf.FillEllipse(brsh, p.Value.X, p.Value.Y, 5, 5);
                                        }
                                    }
                                    continue;
                                }

                                break;
                            }
                        }
                    }
                    if (p != null)
                    {
                        break;
                    }
                    else
                    {
                        p = BotCore.FindImage(screen, Img.Boss, true);
                        if (p != null)
                        {
                            Variables.ScriptLog("Boss Stage found!", Color.Lime);
                            if (VCBotScript.runes == 3 && VCBotScript.energy != 5)
                            {
                                VCBotScript.StuckRune();
                                return;
                            }
                            BotCore.SendTap(p.Value);
                            break;
                        }
                    }
                }
                VCBotScript.image = BotCore.ImageCapture();
                Point? p2 = BotCore.FindImage(crop, Img.GreenButton, false);
                if (p2 != null)
                {
                    BotCore.SendTap(p2.Value);
                    BotCore.Delay(1000, false);
                    continue;
                }
                error++;
            }
            if (error > 9)
            {
                Variables.ScriptLog("Looks like we are in the trouble!", Color.Red);
                error = 0;
                BotCore.KillGame("com.nubee.valkyriecrusade");
                ScriptErrorHandler.Reset("Restarting game as unable to detect stages properly!");
            }
            Point? point = null;
            BotCore.Delay(5000, false);
            for (int x = 0; x < 20; x++)
            {
                VCBotScript.image = BotCore.ImageCapture();
                Point? p2 = BotCore.FindImage(VCBotScript.image, Img.GreenButton, false);
                if (p2 != null)
                {
                    BotCore.SendTap(p2.Value);
                    BotCore.Delay(1000, false);
                    continue;
                }
                if (!BotCore.GameIsForeground("com.nubee.valkyriecrusade"))
                {
                    return;
                }
                point = BotCore.FindImage(VCBotScript.image, Img.Red_Button, false);
                if (point != null)
                {
                    break;
                }
                BotCore.Delay(1000, false);
            }
            if (point == null)
            {
                BotCore.KillGame("com.nubee.valkyriecrusade");
                ScriptErrorHandler.Reset("No expected UI is shown, restarting game!");
                return;
            }
            BotCore.SendTap(point.Value);
            PrivateVariable.Battling = true;
            VCBotScript.stop.Start();
        }
    }
}
