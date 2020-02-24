using System;
using ImgXml;
using BotFramework;
using System.Drawing;
using System.IO;
namespace UI
{
    public class ArchwitchEvent
    {
        static int error = 0;
        public static int FullBossEnergy, CurrentBossEnergy, FullWalkEnergy, CurrentWalkEnergy;
        public static void ArchwitchEnter()
        {
            do
            {
                VCBotScript.LocateMainScreen();
            }
            while (!PrivateVariable.InMainScreen);
            //Enter battle screen
            BotCore.SendTap(170, 630);
            BotCore.Delay(5000, false);
            for (int x = 0; x < 5; x++)
            {
                VCBotScript.image = Screenshot.ImageCapture();
                Point? located = BotCore.FindImage(VCBotScript.image, Environment.CurrentDirectory + "\\Img\\LocateEventSwitch.png", true);
                if (located == null)
                {
                    x -= 1;
                    BotCore.Delay(1000, false);
                    if (error > 10)
                    {
                        ScriptErrorHandler.Reset("Unable to locate Event Switch screen! Returning main screen!");
                        error = 0;
                        return;
                    }
                    error++;
                    ScriptErrorHandler.ErrorHandle();
                    continue;
                }
                else
                {
                    break;
                }
            }
            //ArchwitchHunt capture detected, try to get into event
            if (File.Exists("Img\\ArchEvent.png"))
            {
                var point = BotCore.FindImage(VCBotScript.image, "Img\\ArchEvent.png", false);
                if (point != null)
                {
                    BotCore.SendTap(point.Value);
                    //Enter event
                    SwitchStage();
                }
                else
                {
                    Variables.ScriptLog("Unable to find ArchEvent.png at event page. Exiting function! ", Color.Red);
                }
            }
            else
            {
                Variables.ScriptLog("ArchEvent.png not found! Exiting function! ", Color.Red);
            }
            //not found, we have to exit now!
            //Our next online time had passed. Online it NOWWWWWW!!!!!
            if(VCBotScript.nextOnline < DateTime.Now)
            {
                do
                {
                    VCBotScript.LocateMainScreen();
                }
                while (!PrivateVariable.InMainScreen);
                VCBotScript.Stuck = false;
            }
            else
            {
                BotCore.KillGame(VCBotScript.game);
            }
            return;
        }

        private static void SwitchStage()
        {
            Variables.ScriptLog("Entering stage!",Color.Lime);
            do
            {
                BotCore.Delay(1000, 1200);
                VCBotScript.image = Screenshot.ImageCapture();
            }
            while (BotCore.FindImage(VCBotScript.image, Img.Archwitch, true) == null);
            //swipe to start of UI
            BotCore.SendSwipe(new Point(125, 385), new Point(1100, 385),1000);
            Color notUnlocked = Color.FromArgb(20,22,22);
            switch (VCBotScript.Archwitch_Stage)
            {
                case 1.1:
                    //Must be unlocked, no detection needed
                    BotCore.SendTap(404, 536);
                    break;
                case 1.2:
                    if(!BotCore.RGBComparer(new Point(274, 511), notUnlocked, 10))
                    {
                        BotCore.SendTap(274, 512);
                    }
                    else
                    {
                        Variables.ScriptLog("Stage not unlocked, trying to enter previous one!");
                        VCBotScript.Archwitch_Stage--;
                        SwitchStage();
                    }
                    break;
                case 1.3:
                    if (!BotCore.RGBComparer(new Point(224, 340), notUnlocked, 10))
                    {
                        BotCore.SendTap(211, 340);
                    }
                    else
                    {
                        Variables.ScriptLog("Stage not unlocked, trying to enter previous one!");
                        VCBotScript.Archwitch_Stage--;
                        SwitchStage();
                    }
                    break;
                case 2.1:
                    if (!BotCore.RGBComparer(new Point(339, 170), notUnlocked, 10))
                    {
                        BotCore.SendTap(333, 172);
                    }
                    else
                    {
                        Variables.ScriptLog("Stage not unlocked, trying to enter previous one!");
                        VCBotScript.Archwitch_Stage--;
                        SwitchStage();
                    }
                    break;
                case 2.2:
                    if (!BotCore.RGBComparer(new Point(528, 168), notUnlocked, 10))
                    {
                        BotCore.SendTap(527, 171);
                    }
                    else
                    {
                        Variables.ScriptLog("Stage not unlocked, trying to enter previous one!");
                        VCBotScript.Archwitch_Stage--;
                        SwitchStage();
                    }
                    break;
                case 2.3:
                    if (!BotCore.RGBComparer(new Point(643, 327), notUnlocked, 10))
                    {
                        BotCore.SendTap(621, 323);
                    }
                    else
                    {
                        Variables.ScriptLog("Stage not unlocked, trying to enter previous one!");
                        VCBotScript.Archwitch_Stage--;
                        SwitchStage();
                    }
                    break;
                case 3.1:
                    if (!BotCore.RGBComparer(new Point(739, 457), notUnlocked, 10))
                    {
                        BotCore.SendTap(740, 457);
                    }
                    else
                    {
                        Variables.ScriptLog("Stage not unlocked, trying to enter previous one!");
                        VCBotScript.Archwitch_Stage--;
                        SwitchStage();
                    }
                    break;
                case 3.2:
                    if (!BotCore.RGBComparer(new Point(890, 517), notUnlocked, 10))
                    {
                        BotCore.SendTap(887, 518);
                    }
                    else
                    {
                        Variables.ScriptLog("Stage not unlocked, trying to enter previous one!");
                        VCBotScript.Archwitch_Stage--;
                        SwitchStage();
                    }
                    break;
                //stage at next page
                case 3.3:
                    BotCore.SendSwipe(new Point(1100, 385), new Point(125, 385), 1000);
                    BotCore.Delay(500, 1000);
                    if (!BotCore.RGBComparer(new Point(445, 546), notUnlocked, 10))
                    {
                        BotCore.SendTap(444, 544);
                    }
                    else
                    {
                        Variables.ScriptLog("Stage not unlocked, trying to enter previous one!");
                        VCBotScript.Archwitch_Stage--;
                        SwitchStage();
                    }
                    break;
                case 4.1:
                    BotCore.SendSwipe(new Point(1100, 385), new Point(125, 385), 1000);
                    BotCore.Delay(500, 1000);
                    if (!BotCore.RGBComparer(new Point(643, 532), notUnlocked, 10))
                    {
                        BotCore.SendTap(643, 532);
                    }
                    else
                    {
                        Variables.ScriptLog("Stage not unlocked, trying to enter previous one!");
                        VCBotScript.Archwitch_Stage--;
                        SwitchStage();
                    }
                    break;
                case 4.2:
                    BotCore.SendSwipe(new Point(1100, 385), new Point(125, 385), 1000);
                    BotCore.Delay(500, 1000);
                    if (!BotCore.RGBComparer(new Point(823, 530), notUnlocked, 10))
                    {
                        BotCore.SendTap(823, 530);
                    }
                    else
                    {
                        Variables.ScriptLog("Stage not unlocked, trying to enter previous one!");
                        VCBotScript.Archwitch_Stage--;
                        SwitchStage();
                    }
                    break;
                case 4.3:
                    BotCore.SendSwipe(new Point(1100, 385), new Point(125, 385), 1000);
                    BotCore.Delay(500, 1000);
                    if (!BotCore.RGBComparer(new Point(1010, 514), notUnlocked, 10))
                    {
                        BotCore.SendTap(1010, 514);
                    }
                    else
                    {
                        Variables.ScriptLog("Stage not unlocked, trying to enter previous one!");
                        VCBotScript.Archwitch_Stage--;
                        SwitchStage();
                    }
                    break;
                case 5.1:
                    BotCore.SendSwipe(new Point(1100, 385), new Point(125, 385), 1000);
                    BotCore.Delay(500, 1000);
                    if (!BotCore.RGBComparer(new Point(1110, 364), notUnlocked, 10))
                    {
                        BotCore.SendTap(1110, 364);
                    }
                    else
                    {
                        Variables.ScriptLog("Stage not unlocked, trying to enter previous one!");
                        VCBotScript.Archwitch_Stage--;
                        SwitchStage();
                    }
                    break;
                case 5.2:
                    BotCore.SendSwipe(new Point(1100, 385), new Point(125, 385), 1000);
                    BotCore.Delay(500, 1000);
                    if (!BotCore.RGBComparer(new Point(1100, 222), notUnlocked, 10))
                    {
                        BotCore.SendTap(1100, 222);
                    }
                    else
                    {
                        Variables.ScriptLog("Stage not unlocked, trying to enter previous one!");
                        VCBotScript.Archwitch_Stage--;
                        SwitchStage();
                    }
                    break;
                case 5.3:
                    BotCore.SendSwipe(new Point(1100, 385), new Point(125, 385), 1000);
                    BotCore.Delay(500, 1000);
                    if (!BotCore.RGBComparer(new Point(951, 205), notUnlocked, 10))
                    {
                        BotCore.SendTap(951, 205);
                    }
                    else
                    {
                        Variables.ScriptLog("Stage not unlocked, trying to enter previous one!");
                        VCBotScript.Archwitch_Stage--;
                        SwitchStage();
                    }
                    break;
            }
            Point? p = null;
            do
            {
                BotCore.Delay(1500);
                VCBotScript.image = Screenshot.ImageCapture();
                p = BotCore.FindImage(VCBotScript.image, Img.GreenButton, false);
            } while (p == null);
            BotCore.SendTap(p.Value);
            //Entered Stage
            Attack();
        }

        private static void Attack()
        {
            do
            {
                BotCore.Delay(1500);
            }
            while (BotCore.RGBComparer(new Point(400,400), Color.Black, 10));
            Variables.ScriptLog("Running stage!", Color.Lime);
            do
            {
                Variables.ScriptLog("Detecting UIs...",Color.White);
                if (!BotCore.GameIsForeground(VCBotScript.game))
                {
                    ScriptErrorHandler.Reset("Game closed!");
                    return;
                }
                ScriptErrorHandler.ErrorHandle();
                Random rnd = new Random();
                VCBotScript.image = Screenshot.ImageCapture();
                var crop = Screenshot.CropImage(VCBotScript.image, new Point(420, 360), new Point(855, 430));
                Point? buttons = BotCore.FindImage(crop, Img.GreenButton, false);
                if(buttons != null)
                {
                    CheckWalkEnergy();
                    if(error > 10)
                    {
                        Variables.ScriptLog("Unable to OCR energy! Exiting Event now!", Color.Red);
                        CurrentWalkEnergy = 0;
                    }
                    if(CurrentWalkEnergy < 15)
                    {
                        //No energy
                        Variables.ScriptLog("Archwitch Event have no energy. Exiting now! ", Color.Yellow);
                        return;
                    }
                    BotCore.SendTap(buttons.Value.X + rnd.Next(430, 845), buttons.Value.Y + rnd.Next(370, 420));
                    BotCore.Delay(2000, 3000);
                    continue;
                }
                buttons = BotCore.FindImage(VCBotScript.image, Img.Close2, true);
                if (buttons != null)
                {
                    VCBotScript.image = Screenshot.ImageCapture();
                    if(BotCore.FindImage(VCBotScript.image, Img.NoEnergy, true) != null)
                    {
                        //Means we should kill it as no energy left
                        Variables.ScriptLog("Archwitch Event have no energy. Exiting now! ", Color.Yellow);
                        return;
                    }
                    BotCore.SendTap(buttons.Value);
                    BotCore.Delay(1000, 1500);
                    VCBotScript.image = Screenshot.ImageCapture();
                }
                buttons = BotCore.FindImage(VCBotScript.image, Img.GreenButton, false);
                if(buttons != null)
                {
                    BotCore.SendTap(buttons.Value.X + rnd.Next(675, 940), buttons.Value.Y + rnd.Next(620, 650));
                    BotCore.Delay(1000, 1500);
                    continue;
                }
                crop = Screenshot.CropImage(VCBotScript.image, new Point(665, 565), new Point(970, 640));
                buttons = BotCore.FindImage(crop, Img.Red_Button, false);
                if(buttons != null)
                {
                    BotCore.SendTap(buttons.Value.X + rnd.Next(675, 960), buttons.Value.Y + rnd.Next(575, 630));
                    BotCore.Delay(5000);
                    do
                    {
                        BotCore.Delay(1500);
                    }
                    while (BotCore.RGBComparer(new Point(400, 400), Color.Black, 0));
                    buttons = null;
                    do
                    {
                        VCBotScript.image = Screenshot.ImageCapture();
                        crop = Screenshot.CropImage(VCBotScript.image, new Point(665, 565), new Point(970, 640));
                        buttons = BotCore.FindImage(crop, Img.Red_Button, false);
                        if (buttons != null)
                        {
                            BotCore.SendTap(buttons.Value.X + rnd.Next(675, 960), buttons.Value.Y + rnd.Next(575, 630));
                        }
                        crop = Screenshot.CropImage(VCBotScript.image, new Point(420, 360), new Point(855, 430));
                        buttons = BotCore.FindImage(crop, Img.Red_Button, false);
                        if (buttons != null)
                        {
                            CheckBossEnergy();
                            if (error > 10)
                            {
                                Variables.ScriptLog("Unable to OCR energy! Exiting Event now!", Color.Red);
                                CurrentBossEnergy = 0;
                                return;
                            }
                            CurrentBossEnergy--;
                            BotCore.SendTap(buttons.Value.X + rnd.Next(430, 845), buttons.Value.Y + rnd.Next(370, 420));
                            BotCore.Delay(300, 500);
                            BotCore.SendTap(buttons.Value.X + rnd.Next(430, 845), buttons.Value.Y + rnd.Next(370, 420));
                            BotCore.Delay(300, 500);
                            BotCore.SendTap(buttons.Value.X + rnd.Next(430, 845), buttons.Value.Y + rnd.Next(370, 420));
                            BotCore.Delay(2000, 3000);
                            break;
                        }
                        else
                        {
                            BotCore.Delay(1000, 1200);
                        }
                    }
                    while (buttons == null);
                    PrivateVariable.Battling = true;
                    VCBotScript.Battle();
                    if (CurrentBossEnergy == 0)
                    {
                        Variables.ScriptLog("Archwitch Event have no energy. Exiting now! ", Color.Yellow);
                        return;
                    }
                }
                crop = Screenshot.CropImage(VCBotScript.image, new Point(420, 360), new Point(855, 430));
                buttons = BotCore.FindImage(crop, Img.Red_Button, false);
                if (buttons != null )
                {
                    BotCore.SendTap(buttons.Value.X + rnd.Next(410, 880), buttons.Value.Y + rnd.Next(330, 450));
                    BotCore.SendTap(buttons.Value.X + rnd.Next(410, 880), buttons.Value.Y + rnd.Next(330, 450));
                    BotCore.SendTap(buttons.Value.X + rnd.Next(410, 880), buttons.Value.Y + rnd.Next(330, 450));
                    BotCore.Delay(2000, 3000);
                    PrivateVariable.Battling = true;
                    VCBotScript.Battle();
                    //End Event
                    return;
                }
                BotCore.Delay(1000, 2000);
                BotCore.SendTap(635, 390);
            }
            while (true);
        }
        public static void CheckBossEnergy()
        {
            try
            {
                var temp = OCR.OcrImage(Screenshot.CropImage(VCBotScript.image, new Point(33, 49), new Point(119, 78)), "eng");
                var temp_arr = temp.Split('/');
                FullBossEnergy = Convert.ToInt32(temp_arr[1]);
                CurrentBossEnergy = Convert.ToInt32(temp_arr[0]);
                error = 0;
                Variables.ScriptLog("Current boss energy is " + CurrentBossEnergy, Color.White);
            }
            catch
            {
                if(FullBossEnergy == 0)
                {
                    FullBossEnergy = 999;
                    CurrentBossEnergy = 999;
                }
                error++;
            }
        }

        public static void CheckWalkEnergy()
        {
            try
            {
                var temp = OCR.OcrImage(Screenshot.CropImage(VCBotScript.image, new Point(65, 54), new Point(166, 78)), "eng");
                var temp_arr = temp.Split('/');
                FullWalkEnergy = Convert.ToInt32(temp_arr[1]);
                CurrentWalkEnergy = Convert.ToInt32(temp_arr[0]);
                error = 0;
                Variables.ScriptLog("Current walk energy is " + CurrentWalkEnergy, Color.White);
            }
            catch
            {
                if(FullWalkEnergy == 0)
                {
                    CurrentWalkEnergy = 999;
                    FullWalkEnergy = 999;
                }
                error++;
            }
        }
    }
}
