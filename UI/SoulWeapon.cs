using BotFramework;
using ImgXml;
using System;
using System.Drawing;
using System.IO;

namespace UI
{
    class SoulWeapon
    {
        //static bool UnhandledException;

        //static int error = 0;
        /*public static void SoulWeaponEnter()
        {
            if (UnhandledException)
            {
                Variables.ScriptLog("Unhandled exception had found in SoulWeapon event UI. Skip it! ",Color.Red);
                return;
            }
            do
            {
                VCBotScript.LocateMainScreen();
            }
            while (!PrivateVariable.InMainScreen);
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
            if (File.Exists("Img\\WeaponEvent.png"))
            {
                var point = BotCore.FindImage(VCBotScript.image, "Img\\WeaponEvent.png", false);
                if (point != null)
                {
                    BotCore.SendTap(point.Value);
                    //Enter event
                    SwitchStage();
                }
                else
                {
                    Variables.ScriptLog("Unable to find WeaponEvent.png at event page. Exiting function! ", Color.Red);
                }
            }
            else
            {
                Variables.ScriptLog("WeaponEvent.png not found! Exiting function! ", Color.Red);
            }
        }*/
        //public static void SwitchEvent
        public static void SoulWeaponEnter()
        {
            Variables.ScriptLog("Soul Weapon Event found!",Color.Lime);
            //Check do we still can get into the stage
            /*do
            {
                BotCore.Delay(1000, 1200);
                VCBotScript.image = Screenshot.ImageCapture();
                if (BotCore.FindImage(Screenshot.CropImage(VCBotScript.image, new Point(634, 374), new Point(694, 421)), Img.Archwitch_Rec, false) != null)
                {
                    Attack();
                    return;
                }
            }
            while (BotCore.FindImage(VCBotScript.image, Img.SoulWeapon, true) == null);*/
            int error = 0;
            //Check if we didnt need to switch stage
            do
            {
                /*var crop = Screenshot.CropImage(VCBotScript.image, new Point(73, 83), new Point(112, 102));
                var tempstring = OCR.OcrImage(crop, "eng");
                var leftTimes = tempstring.Split('/')[0];
                if (leftTimes == "0")
                {
                    Variables.ScriptLog("Today had already attacked 5 times. Exiting...", Color.Lime);
                    return;
                }
                else
                {*/
                VCBotScript.image = Screenshot.ImageCapture();
                if (BotCore.FindImage(VCBotScript.image, Img.SoulArrow, false) != null)
                {
                    Variables.ScriptLog("Already in a stage, running now...",Color.Lime);
                    Attack();
                }
                //Back to 1-1 first before we continue
                BotCore.Delay(1000);
                if(Variables.FindConfig("General", "SoulEv", out string data))
                {
                    if (bool.Parse(data))
                    {
                        var New = BotCore.FindImage(VCBotScript.image, Img.NEW, false);
                        if(New != null)
                        {
                            Variables.ScriptLog("Found new stage!", Color.Blue);
                            BotCore.SendTap(New.Value);
                            goto SkipSelectStage;
                        }
                        New = BotCore.FindImage(VCBotScript.image, Img.NEW1, false);
                        if (New != null)
                        {
                            Variables.ScriptLog("Found new stage!", Color.Blue);
                            BotCore.SendTap(New.Value);
                            goto SkipSelectStage;
                        }
                        New = BotCore.FindImage(VCBotScript.image, Img.NEW2, false);
                        if (New != null)
                        {
                            Variables.ScriptLog("Found new stage!", Color.Blue);
                            BotCore.SendTap(New.Value);
                            goto SkipSelectStage;
                        }
                        else
                        {
                            Variables.ScriptLog("New stage not found! Try getting into Boss stage!", Color.Blue);
                            New = BotCore.FindImage(VCBotScript.image, Img.Castle, true);
                            if(New != null)
                            {
                                BotCore.SendTap(New.Value);
                                goto SkipSelectStage;
                            }
                            Variables.ScriptLog("No Stage located, force getting into stage!",Color.Red);
                        }
                    }
                }
                //BotCore.Minitouch("d 0 290 340 150\nc\nm 0 340 340 150\nc\nm 0 390 340 150\nc\nm 0 440 340 150\nc\nm 0 490 340 150\nc\nm 0 540 340 150\nc\nm 0 590 340 150\nc\nm 0 640 340 150\nc\nm 0 740 340 150\nc\nm 0 840 340 150\nc\nm 0 940 340 150\nc\nu 0\nc\n");
                BotCore.SendSwipe(500, 440,1200, 360, 1000);//Why it can't swipe????
                BotCore.Delay(1000);

                switch (VCBotScript.Weapon_Stage)
                {
                    case 1.1:
                        BotCore.SendTap(449, 532);
                        break;
                    case 1.2:
                        BotCore.SendTap(577, 422);
                        break;
                    case 1.3:
                        BotCore.SendTap(692, 277);
                        break;
                    case 2.1:
                        BotCore.SendTap(820, 423);
                        break;
                    case 2.2:
                        BotCore.SendTap(944, 306);
                        break;
                    case 2.3:
                        BotCore.SendTap(1053, 210);
                        break;
                    case 3.1:
                        BotCore.SendTap(1191, 310);
                        break;
                    //next page
                    case 3.2:
                        BotCore.SendSwipe(1191, 310, 670, 310, 3000);
                        BotCore.Delay(1000);
                        BotCore.SendTap(315, 427);
                        break;
                }
                SkipSelectStage:
                BotCore.Delay(1000, 1200);
                VCBotScript.image = Screenshot.ImageCapture();
                var point = BotCore.FindImage(VCBotScript.image, Img.GreenButton, false);
                if (point != null)
                {
                    BotCore.SendTap(point.Value);
                    BotCore.Delay(1000, 2000);
                    Attack();
                    if (!PrivateVariable.Instance.InEventScreen || BotCore.GameIsForeground(VCBotScript.game))
                    {
                        PrivateVariable.Instance.InEventScreen = false;
                        PrivateVariable.Instance.InMainScreen = false;
                        return;
                    }
                    error = 0;
                    if (Variables.FindConfig("General", "SoulEv", out data))
                    {
                        if (!bool.Parse(data))
                        {
                            if (VCBotScript.Weapon_Stage < 3.0)
                            {
                                VCBotScript.Weapon_Stage += 0.1;
                                if (VCBotScript.Weapon_Stage % 1 > 0.3)
                                {
                                    VCBotScript.Weapon_Stage += 0.7;
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (error > 9)
                    {
                        BotCore.KillGame("com.nubee.valkyriecrusade");
                        ScriptErrorHandler.Reset("Extreme error happens. Restarting game...");
                        return;
                    }
                    else
                    {
                        Variables.ScriptLog("Something error happens. Unable to get detect expcted UI", Color.Red);
                        BotCore.SendSwipe(395, 365, 1100, 380, 1000);
                        error++;
                    }
                    //Variables.ScriptLog("Unexpected error found! Unable to get into stage! Exiting for now!", Color.Red);
                    //return;
                }
                //}

            }
            while (error < 10);
            
        }

        private static void Attack()
        {
            do
            {
                BotCore.Delay(1500);
                VCBotScript.image = Screenshot.ImageCapture();
            }
            while (BotCore.RGBComparer(new Point(400, 400), Color.Black, 10, VCBotScript.image));
            Variables.ScriptLog("Running stage!", Color.Lime);
            int error = 0;
            do
            {
                Random rnd = new Random();
                VCBotScript.image = Screenshot.ImageCapture();
                var crop = Screenshot.CropImage(VCBotScript.image, new Point(420, 360), new Point(855, 430));
                Point? buttons = BotCore.FindImage(crop, Img.GreenButton, false);
                if (buttons != null)
                {
                    ArchwitchEvent.CheckWalkEnergy();
                    if (ArchwitchEvent.CurrentWalkEnergy < 15 || (ArchwitchEvent.CurrentBossEnergy < 3 && ArchwitchEvent.FullBossEnergy > 0))
                    {
                        //No energy
                        Variables.ScriptLog("SoulWeapon Event have no energy. Exiting now! ", Color.Yellow);
                        TimeSpan delay = new TimeSpan(0, ((ArchwitchEvent.FullWalkEnergy - ArchwitchEvent.CurrentWalkEnergy) * 5), 0);
                        VCBotScript.nextOnline = DateTime.Now + delay;
                        Variables.ScriptLog("Estimate online time is " + VCBotScript.nextOnline, Color.Lime);
                        BotCore.KillGame(VCBotScript.game);
                        BotCore.Delay(delay);
                        PrivateVariable.Instance.InEventScreen = false;
                        PrivateVariable.Instance.InMainScreen = false;
                        ArchwitchEvent.CurrentBossEnergy = ArchwitchEvent.FullBossEnergy;
                        ArchwitchEvent.CurrentWalkEnergy = ArchwitchEvent.FullWalkEnergy;
                        BotCore.StartGame(VCBotScript.game + VCBotScript.activity);
                        return;
                    }
                    BotCore.SendTap(buttons.Value.X + rnd.Next(430, 845), buttons.Value.Y + rnd.Next(370, 420));
                    BotCore.Delay(2000, 3000);
                    continue;
                }
                buttons = BotCore.FindImage(VCBotScript.image, Img.Return, true);
                if(buttons != null)
                {
                    BotCore.SendTap(buttons.Value);
                    BotCore.Delay(1000, 1500);
                    continue;
                }
                buttons = BotCore.FindImage(VCBotScript.image, Img.Close2, true);
                if (buttons != null)
                {
                    BotCore.SendTap(buttons.Value);
                    BotCore.Delay(1000, 1500);
                    continue;
                }
                buttons = BotCore.FindImage(crop, Img.Red_Button, false);
                if (buttons != null)
                {
                    ArchwitchEvent.CheckBossEnergy();
                    BotCore.SendTap(buttons.Value.X + rnd.Next(430, 845), buttons.Value.Y + rnd.Next(370, 420));
                    BotCore.Delay(2000, 3000);
                    PrivateVariable.Instance.Battling = true;
                    VCBotScript.Battle();
                    continue;
                }
                buttons = BotCore.FindImage(VCBotScript.image, Img.ShopKeeper, true);
                if(buttons != null)
                {
                    BotCore.SendTap(770, 640);
                    Variables.ScriptLog("Shop keeper found! Getting in and see what to buy!", Color.White);
                    BotCore.Delay(3000);
                    VCBotScript.image = Screenshot.ImageCapture();
                    if(BotCore.FindImage(VCBotScript.image, Img.MisteryBox, true)!= null)
                    {
                        Variables.ScriptLog("Mistory Box found! Purchasing all products!", Color.Lime);
                        for(int x = 0; x < 3; x++)
                        {
                            switch (x)
                            {
                                case 0:
                                    BotCore.SendTap(1030, 220);
                                    break;
                                case 1:
                                    BotCore.SendTap(1030, 390);
                                    break;
                                case 2:
                                    BotCore.SendTap(1030, 550);
                                    break;
                            }
                            BotCore.Delay(3000);
                            Point? greenButton;
                            do
                            {
                                BotCore.SendTap(2,2);
                                BotCore.Delay(300);
                                VCBotScript.image = Screenshot.ImageCapture();
                                greenButton = BotCore.FindImage(VCBotScript.image, Img.GreenButton, false);
                            }
                            while (greenButton == null);
                            BotCore.SendTap(greenButton.Value);
                            for(int y = 0; y < 10; y++)
                            {
                                BotCore.SendTap(2,2);
                                BotCore.Delay(500);
                            }
                        }
                    }
                    BotCore.SendTap(1110, 875);
                    BotCore.Delay(500);
                    continue;
                }
                if(BotCore.FindImage(VCBotScript.image, Img.ArchwitchHunt, true) != null)
                {
                    //Stage completed
                    return;
                }
                error++;
                if(error > 30)
                {
                    Screenshot.Decompress(VCBotScript.image).Save("Profiles\\Logs\\error.png");
                    //UnhandledException = true;
                    Variables.ScriptLog("Unhandled exception. Contact PoH98 for fix!", Color.Red);
                    return;
                }
                else
                {
                    BotCore.Delay(1000,1500);
                }
            }
            while (true);
        }
    }
}
