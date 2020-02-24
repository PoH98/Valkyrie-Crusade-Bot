using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ImgXml;
using BotFramework;
using System.Drawing;

namespace UI
{
    class TowerEvent
    {
        static Random rnd = new Random();
        public static void Tower()
        {
            Debug_.WriteLine();
            BotCore.Delay(1000, false);
            var image = Screenshot.ImageCapture();
            Point? point = BotCore.FindImage(image, Img.Close2, false);
            if (point != null)
            {
                BotCore.SendTap(new Point(point.Value.X, point.Value.Y));
                BotCore.Delay(1000, false);
            }
            image = Screenshot.ImageCapture();
            Variables.ScriptLog("Locating Tower Event UI!", Color.White);
            if (BotCore.FindImage(image, Img.Locate_Tower, true) != null)
            {
                image = Screenshot.ImageCapture();
                VCBotScript.Tower_Floor = OCR.OcrImage(Screenshot.CropImage(image, new Point(280, 110), new Point(440, 145)), "eng");
                VCBotScript.Tower_Rank = OCR.OcrImage(Screenshot.CropImage(image, new Point(280, 145), new Point(410, 170)), "eng");
                Variables.ScriptLog("Tower Event Found!", Color.Lime);
                PrivateVariable.InEventScreen = true;
            }
            else
            {
                PrivateVariable.InMainScreen = false;
                PrivateVariable.InEventScreen = false;
                return;
            }
            image = Screenshot.ImageCapture();
            while (!BotCore.RGBComparer( new Point(135, 526), 13, 46, 74, 10))
            {
                BotCore.Delay(1000, true);
                image = Screenshot.ImageCapture();
            }
            VCBotScript.energy = VCBotScript.GetEnergy();
            VCBotScript.runes = VCBotScript.GetRune();
            Variables.ScriptLog("Current have " + VCBotScript.energy + " energy and " + VCBotScript.runes + " runes", Color.LightSkyBlue);
            if (VCBotScript.energy == 0)
            {
                Variables.ScriptLog("Waiting for energy", Color.Yellow);
                if (PrivateVariable.TakePartInNormalStage)
                {
                    BotCore.SendTap(1218, 662);
                    BotCore.Delay(400, 600);
                    BotCore.SendTap(744, 622);
                }
                else
                {
                    if (PrivateVariable.Use_Item)
                    {
                        if (VCBotScript.runes == 5)
                        {
                            Variables.ScriptLog("Use item as it is now rune!", Color.White);
                        }
                        else
                        {
                            Variables.ScriptLog("Close game and wait for energy because of no energy left", Color.White);
                            VCBotScript.NoEnergy();
                            PrivateVariable.InEventScreen = false;
                            PrivateVariable.InMainScreen = false;
                            PrivateVariable.Battling = false;
                            return;
                        }
                    }
                    else
                    {
                        Variables.ScriptLog("Close game and wait for energy because of no energy left", Color.White);
                        VCBotScript.NoEnergy();
                        PrivateVariable.InEventScreen = false;
                        PrivateVariable.InMainScreen = false;
                        PrivateVariable.Battling = false;
                        return;
                    }
                }
            }
            Variables.ScriptLog("Entering Stage!", Color.Lime);
            image = Screenshot.ImageCapture();
            if(BotCore.FindImage(image, Img.GreenButton, false) == null)
            {
                Variables.ScriptLog("Rune Boss found!", Color.Lime);
            }
            else
            {
                var templevel = MainScreen.Level;
                Loop:
                switch (templevel)
                {
                    case 0:
                        for(int x = 0; x < 5; x++)
                        {
                            VCBotScript.image = Screenshot.ImageCapture();
                            if(BotCore.FindImage(VCBotScript.image,Img.LV0, true) == null)
                            {
                                BotCore.SendTap(rnd.Next(410,420), rnd.Next(650, 660));
                                BotCore.Delay(500);
                            }
                        }
                        BotCore.SendTap(785, 618);
                        break;
                    case 1:
                        for (int x = 0; x < 5; x++)
                        {
                            VCBotScript.image = Screenshot.ImageCapture();
                            if (BotCore.FindImage(VCBotScript.image, Img.LV1, true) == null)
                            {
                                BotCore.SendTap(rnd.Next(410, 420), rnd.Next(650, 660));
                                BotCore.Delay(500);
                            }
                        }
                        if (BotCore.FindImage(VCBotScript.image, Img.LV1, true) == null)
                        {
                            Variables.ScriptLog("Unable to switch to stage. Stage not unlocked?",Color.Red);
                            templevel--;
                            goto Loop;
                        }
                        BotCore.SendTap(785, 618);
                        break;
                    case 2:
                        for (int x = 0; x < 5; x++)
                        {
                            VCBotScript.image = Screenshot.ImageCapture();
                            if (BotCore.FindImage(VCBotScript.image, Img.LV2, true) == null)
                            {
                                BotCore.SendTap(rnd.Next(410, 420), rnd.Next(650, 660));
                                BotCore.Delay(500);
                            }
                            else if(BotCore.FindImage(VCBotScript.image, Img.LV3, true) != null)
                            {
                                //This is not 上级，this is fucking 超上级
                                BotCore.SendTap(rnd.Next(410, 420), rnd.Next(650, 660));
                                BotCore.Delay(500);
                            }
                        }
                        if (BotCore.FindImage(VCBotScript.image, Img.LV2, true) == null)
                        {
                            Variables.ScriptLog("Unable to switch to stage. Stage not unlocked?", Color.Red);
                            templevel--;
                            goto Loop;
                        }
                        BotCore.SendTap(785, 618);
                        break;
                    case 3:
                        for (int x = 0; x < 5; x++)
                        {
                            VCBotScript.image = Screenshot.ImageCapture();
                            if (BotCore.FindImage(VCBotScript.image, Img.LV3, true) == null)
                            {
                                BotCore.SendTap(rnd.Next(410, 420), rnd.Next(650, 660));
                                BotCore.Delay(500);
                            }
                        }
                        if (BotCore.FindImage(VCBotScript.image, Img.LV3, true) == null)
                        {
                            Variables.ScriptLog("Unable to switch to stage. Stage not unlocked?", Color.Red);
                            templevel--;
                            goto Loop;
                        }
                        BotCore.SendTap(785, 618);
                        break;
                    case 4:
                        for (int x = 0; x < 5; x++)
                        {
                            VCBotScript.image = Screenshot.ImageCapture();
                            if (BotCore.FindImage(VCBotScript.image, Img.LV4, true) == null)
                            {
                                BotCore.SendTap(rnd.Next(410, 420), rnd.Next(650, 660));
                                BotCore.Delay(500);
                            }
                        }
                        if (BotCore.FindImage(VCBotScript.image, Img.LV4, true) == null)
                        {
                            Variables.ScriptLog("Unable to switch to stage. Stage not unlocked?", Color.Red);
                            templevel--;
                            goto Loop;
                        }
                        BotCore.SendTap(785, 618);
                        break;
                }
            }
            
            BotCore.Delay(2000);
            BotCore.SendTap(800, 660);
            /*
            image = Screenshot.ImageCapture();
            switch (MainScreen.Level)
            {
                case 0:
                    BotCore.SendTap(196, 648);
                    break;
                case 1:
                    if (BotCore.RGBComparer( new Point(328, 621), Color.FromArgb(13, 12, 12), 35))
                    {
                        Variables.ScriptLog("中级还没被解锁！自动往下挑战中！", Color.Red);
                        BotCore.SendTap(196, 648);
                        break;
                    }
                    BotCore.SendTap(391, 648);
                    break;
                case 2:
                    if (BotCore.RGBComparer( new Point(515, 625), Color.FromArgb(12, 11, 12), 35))
                    {
                        Variables.ScriptLog("上级还没被解锁！自动往下挑战中！", Color.Red);
                        if (BotCore.RGBComparer( new Point(328, 621), Color.FromArgb(13, 12, 12), 5))
                        {
                            Variables.ScriptLog("中级还没被解锁！自动往下挑战中！", Color.Red);
                            BotCore.SendTap(196, 648);
                            break;
                        }
                        BotCore.SendTap(391, 648);
                        break;
                    }
                    BotCore.SendTap(581, 646);
                    break;
                case 3:
                    if (BotCore.RGBComparer( new Point(703, 622), Color.FromArgb(32, 30, 30), 35))
                    {
                        Variables.ScriptLog("超上级还没被解锁！自动往下挑战中！", Color.Red);
                        if (BotCore.RGBComparer( new Point(515, 625), Color.FromArgb(12, 11, 12), 35))
                        {
                            Variables.ScriptLog("上级还没被解锁！自动往下挑战中！", Color.Red);
                            if (BotCore.RGBComparer( new Point(328, 621), Color.FromArgb(13, 12, 12), 35))
                            {
                                Variables.ScriptLog("中级还没被解锁！自动往下挑战中！", Color.Red);
                                BotCore.SendTap(196, 648);
                                break;
                            }
                            BotCore.SendTap(391, 648);
                            break;
                        }
                        BotCore.SendTap(581, 646);
                        break;
                    }
                    BotCore.SendTap(741, 623);
                    break;
                case 4:
                    if (BotCore.RGBComparer( new Point(885, 621), Color.FromArgb(107, 100, 100), 90))
                    {
                        Variables.ScriptLog("霸级还没被解锁！自动往下挑战中！", Color.Red);
                        if (BotCore.RGBComparer( new Point(703, 621), Color.FromArgb(107, 100, 100), 90))
                        {
                            Variables.ScriptLog("超上级还没被解锁！自动往下挑战中！", Color.Red);
                            if (BotCore.RGBComparer( new Point(515, 621), Color.FromArgb(107, 100, 100), 90))
                            {
                                Variables.ScriptLog("上级还没被解锁！自动往下挑战中！", Color.Red);
                                if (BotCore.RGBComparer( new Point(328, 621), Color.FromArgb(117, 100, 100), 90))
                                {
                                    Variables.ScriptLog("中级还没被解锁！自动往下挑战中！", Color.Red);
                                    BotCore.SendTap(196, 648);
                                    break;
                                }
                                BotCore.SendTap(391, 648);
                                break;
                            }
                            BotCore.SendTap(581, 646);
                            break;
                        }
                        BotCore.SendTap(741, 623);
                        break;
                    }
                    BotCore.SendTap(921, 620);
                    break;
            }*/
            BotCore.Delay(3000, false);
            do
            {
                if (!BotCore.GameIsForeground("com.nubee.valkyriecrusade"))
                {
                    return;
                }
                /*if (PrivateVariable.Use_Item && VCBotScript.energy == 0 && VCBotScript.runes == 5)
                {
                    image = Screenshot.ImageCapture();
                    if (BotCore.GetPixel(new Point(798, 313), image) != Color.FromArgb(27, 95, 22))
                    {
                        BotCore.Delay(1000, false);
                        continue;
                    }
                    BotCore.SendTap(798, 313);
                    image = Screenshot.ImageCapture();
                    Point? p = BotCore.FindImage(image, Img.GreenButton, false);
                    while (p == null)
                    {
                        if (!BotCore.GameIsForeground("com.nubee.valkyriecrusade"))
                        {
                            return;
                        }
                        BotCore.Delay(400, 600);
                        image = Screenshot.ImageCapture();
                        p = BotCore.FindImage(image, Img.GreenButton, false);
                    }
                    BotCore.SendTap(p.Value);
                    VCBotScript.energy = 5;
                    BotCore.Delay(5000, false);
                }*/
                image = Screenshot.ImageCapture();
                if (BotCore.RGBComparer( new Point(959, 656), 31, 102, 26, 4))
                {
                    Variables.ScriptLog("Start battle", Color.Lime);
                    BotCore.SendTap(new Point(959, 656));
                    BotCore.Delay(7000, false);
                    BotCore.SendTap(640, 400); //Tap away Round Battle Text
                    BotCore.Delay(2000, false);
                    VCBotScript.stop.Start();
                    PrivateVariable.Battling = true;
                    VCBotScript.energy--; //Calculate Energy used
                    if (VCBotScript.nextOnline < DateTime.Now)
                    {
                        VCBotScript.nextOnline = DateTime.Now;
                    }
                    VCBotScript.nextOnline = VCBotScript.nextOnline.AddMinutes(45);
                    BotCore.Delay(1000, false);
                    break;
                }
                else
                {
                    image = Screenshot.ImageCapture();
                    var crop = Screenshot.CropImage(image, new Point(125, 600), new Point(1270, 10));
                    point = BotCore.FindImage(crop, Img.Red_Button, false);
                    if (point != null)
                    {
                        Variables.ScriptLog("Rune boss found!", Color.Yellow);
                        BotCore.SendTap(new Point(point.Value.X + 125, point.Value.Y));
                        VCBotScript.RuneBoss = true;
                        BotCore.Delay(9000, 12000);
                    }
                    else
                    {
                        ScriptErrorHandler.ErrorHandle();
                    }
                }
                image = Screenshot.ImageCapture();
                ScriptErrorHandler.ErrorHandle();
            }
            while (!PrivateVariable.Battling);

        }
    }
}
