using System.Drawing;
using System.IO;
using System;
using BotFramework;
using System.Diagnostics;
using ImgXml;
using System.Windows.Forms;
using System.Media;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Text.RegularExpressions;

namespace UI
{
    public class VCBotScript : ScriptInterface
    {
        public static Stopwatch stop = new Stopwatch();
        public static bool RuneBoss, Stuck, EnterWitchGate, Archwitch_Repeat, DisableAutoCheckEvent, CloseEmu = false, pushed = false;
        public static int runes, energy;
        public static int Archwitch_Stage;
        public static int TreasureHuntIndex = -1;
        private static int Retry = 0, error = 0;
        public static string Tower_Floor = "", Tower_Rank = "";
        public static byte[] image = null;
        public static DateTime nextOnline;
        private static bool Collected;
        //Try Locate MainScreen
        private static void LocateMainScreen()
        {
            Debug_.WriteLine();
            BotCore.Delay(1000, false);
            PrivateVariable.InMainScreen = false;
            BotCore.Delay(100, 200);
            bool StartGame = false;
            for (int x = 0; x < 30; x++)
            {
                if (!Muted_Device && StartGame)
                {
                    for (int y = 0; y < 10; y++)
                    {
                        BotCore.SendEvent(25);
                    }
                    Muted_Device = true;
                }
                image = BotCore.ImageCapture();
                while (BotCore.RGBComparer(image, new Point(520, 355), Color.Black, 1))
                {
                    BotCore.Delay(1000, true);
                    image = BotCore.ImageCapture();
                }
                var crop = BotCore.CropImage(image, new Point(315, 150), new Point(1005, 590));
                Point? point = BotCore.FindImage(crop, Img.GreenButton, false);
                if (point != null)
                {
                    BotCore.SendTap(point.Value);
                }
                image = BotCore.ImageCapture();
                if (!BotCore.RGBComparer(image, new Point(109, 705), Color.FromArgb(130, 130, 130), 5) && !BotCore.RGBComparer(image, new Point(219, 705), Color.FromArgb(130, 130, 130), 5))
                {
                    if (!BotCore.GameIsForeground("com.nubee.valkyriecrusade"))
                    {
                        return;
                    }
                    Variables.ScriptLog("Main Screen not visible", Color.White);
                    point = BotCore.FindImage(image, Img.Start_Game, true);
                    if (point != null)
                    {
                        Variables.ScriptLog("Start Game Button Located!", Color.Lime);
                        BotCore.SendTap(point.Value);
                        error = 0;
                        StartGame = true;
                        return;
                    }
                    point = BotCore.FindImage(image, Img.Update_Complete, true);
                    if (point != null)
                    {
                        BotCore.SendTap(point.Value);
                        return;
                    }
                    point = BotCore.FindImage(image, Img.Close2, true);
                    if (point != null)
                    {
                        BotCore.SendTap(point.Value);
                        return;
                    }
                    point = BotCore.FindImage(image, Img.Close, true);
                    if (point != null)
                    {
                        BotCore.SendTap(point.Value);
                        return;
                    }
                    point = BotCore.FindImage(image, Img.Login_Reward, true);
                    if (point != null)
                    {
                        for (int y = 0; y < 4; y++)
                        {
                            BotCore.SendTap(600, 350);
                            BotCore.Delay(1000, false);
                        }
                        return;
                    }
                    point = BotCore.FindImage(crop, Img.Red_Button, false);
                    if (point != null)
                    {
                        BotCore.SendTap(point.Value);
                        Variables.ScriptLog("Found Red Button!", Color.Lime);
                    }
                    point = BotCore.FindImage(image, Img.Back_to_Village, true);
                    if (point != null)
                    {
                        Variables.ScriptLog("Going back to Main screen", Color.Lime);
                        BotCore.SendTap(point.Value);
                        PrivateVariable.InMainScreen = true;
                        Variables.ScriptLog("Screen Located", Color.Lime);
                    }
                    point = BotCore.FindImage(image, Img.Menu, true);
                    if (point == null)
                    {
                        if (error < 30)
                        {
                            if (error == 0)
                            {
                                Variables.ScriptLog("Waiting for Main screen", Color.White);
                            }
                            BotCore.Delay(1000, false);
                            error++;
                        }
                        else
                        {
                            BotCore.KillGame("com.nubee.valkyriecrusade");
                            ScriptErrorHandler.Reset("Unable to locate main screen. Restarting Game!");
                            error = 0;
                            return;
                        }
                    }
                    else
                    {
                        BotCore.SendTap(point.Value);
                        BotCore.Delay(1000, false);
                        Variables.ScriptLog("Returning main screen", Color.Lime);
                        BotCore.SendTap(942, 630);
                        BotCore.Delay(5000, false);
                    }
                    point = BotCore.FindImage(image, Img.GreenButton, false);
                    if (point != null)
                    {
                        BotCore.SendTap(point.Value);
                        Variables.ScriptLog("Green Button Found!", Color.Lime);
                    }
                    ScriptErrorHandler.ErrorHandle();
                }
                else
                {
                    Retry++;
                    if (Retry > 5)
                    {
                        PrivateVariable.InMainScreen = true;
                        Variables.ScriptLog("Screen Located", Color.White);
                        Collect();
                        Retry = 0;
                        break;
                    }
                    else
                    {
                        BotCore.Delay(1000, false);
                        if (Retry == 1)
                        {
                            Variables.ScriptLog("Waiting for Login Bonus", Color.DeepPink);
                        }
                    }
                }
                if (x > 25)
                {
                    BotCore.KillGame("com.nubee.valkyriecrusade");
                }
            }
        }
        //Collect
        private static void Collect()
        {
            if (!Collected)
            {
                Variables.ScriptLog("Collecting Resources", Color.Lime);
                for (int x = 0; x < 4; x++)
                {
                    switch (x)
                    {
                        case 0:
                            BotCore.SendSwipe(new Point(925, 576), new Point(614, 26), 1000);
                            break;
                        case 1:
                            BotCore.SendSwipe(new Point(231, 562), new Point(877, 127), 1000);
                            break;
                        case 2:
                            BotCore.SendSwipe(new Point(226, 175), new Point(997, 591), 1000);
                            break;
                        case 3:
                            BotCore.SendSwipe(new Point(969, 128), new Point(260, 545), 1000);
                            break;
                    }
                    image = BotCore.ImageCapture();
                    var crop = BotCore.CropImage(image, new Point(0, 0), new Point(1020, 720));
                    //Find image and collect
                    var p = BotCore.FindImage(crop, Img.Resource_1, false);
                    if (p != null)
                    {
                        BotCore.SendTap(p.Value);
                        BotCore.Delay(100, 200);
                    }
                    if (!BotCore.RGBComparer(image, new Point(1261, 101), Color.FromArgb(70, 130, 10), 65))
                    {
                        p = BotCore.FindImage(crop, Img.Resource_2, false);
                        if (p != null)
                        {
                            BotCore.SendTap(p.Value);
                            BotCore.Delay(100, 200);
                            
                        }
                    }
                    else
                    {
                        Variables.ScriptLog("乙醚已满，跳过收取！", Color.Lime);
                        
                    }
                    if (!BotCore.RGBComparer(image, new Point(1260, 42), Color.FromArgb(249, 173, 46), 10))
                    {
                        p = BotCore.FindImage(crop, Img.Resource_3, false);
                        if (p != null)
                        {
                            BotCore.SendTap(p.Value);
                            BotCore.Delay(100, 200);
                            
                        }
                    }
                    else
                    {
                        Variables.ScriptLog("黄金已满，跳过收取！", Color.Lime);
                        
                    }
                    if(!BotCore.RGBComparer(image,new Point(1260, 160), Color.FromArgb(125, 125, 125), 10))
                    {
                        p = BotCore.FindImage(crop, Img.Resource_4, false);
                        if (p != null)
                        {
                            BotCore.SendTap(p.Value);
                            BotCore.Delay(100, 200);
                            
                        }
                    }
                    else
                    {
                        Variables.ScriptLog("铁已满，跳过收取！", Color.Lime);
                        
                    }
                    BotCore.Delay(800, 1200);
                }
            }
            Collected = true;
        }
        //Archwitch
        private static void ArchwitchEnter()
        {
            //return mainscreen before enter
            LocateMainScreen();
            //Enter battle screen
            BotCore.SendTap(170, 630);
            BotCore.Delay(5000, false);
            for (int x = 0; x < 5; x++)
            {
                image = BotCore.ImageCapture();
                Point? located = BotCore.FindImage(image, Environment.CurrentDirectory + "\\Img\\LocateEventSwitch.png", true);
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
            if (File.Exists("Img\\ArchwitchHunt.png"))
            {
                var point = BotCore.FindImage(image, "Img\\ArchwitchHunt.png", false);
                if (point != null)
                {
                    BotCore.SendTap(point.Value);
                    //Enter event

                }
            }
            //not found, we have to exit now!
            return;
        }
        //Guild wars
        private static void GuildWar()
        {
            for(int x = 0; x < 60; x++)
            {
                image = BotCore.ImageCapture();
                var point = BotCore.FindImage(image, Img.GreenButton, false);
                if(point != null)
                {
                    BotCore.SendTap(point.Value);
                    BotCore.Delay(500);
                    x--;
                    continue;
                }
                if (BotCore.FindImage(image, "Img\\GuildWar\\Locate.png", false) != null)
                {
                    break;
                }
                if(x > 50)
                {
                    Variables.ScriptLog("Somehing is not right! No guild war locate found! Lets go back!");
                    GetEventXML.guildwar = DateTime.MinValue;
                    return;
                }
            }
            //Read energy
            if (BotCore.RGBComparer(image, new Point(877, 560), Color.FromArgb(50, 233,34), 10))
            {
                PrivateVariable.Battling = true;
                Battle();
            }
            else
            {
                BotCore.Delay(2700000);
            }
        }
        //Check Event
        private static Point[] eventlocations = new Point[] { new Point(795, 80), new Point(795, 240), new Point(795, 400)  };
        private static int selectedeventlocations = 0;
        private static void CheckEvent()
        {
            Point eventlocation = eventlocations[selectedeventlocations];
            Debug_.WriteLine();
            Point? point = null;
            int error = 0;
            if (Variables.FindConfig("General","Double_Event", out string Special))
            {
                if (Special == "true")
                {
                    BotCore.SendTap(170, 655);
                    BotCore.Delay(5000, false);
                    for (int x = 0; x < 5; x++)
                    {
                        image = BotCore.ImageCapture();
                        Point? located = BotCore.FindImage(image, "Img\\LocateEventSwitch.png", true);
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
                        else if (File.Exists("Img\\Event.png"))
                        {
                            Variables.ScriptLog("Finding Event.png on screen", Color.White);
                            point = BotCore.FindImage(image, Environment.CurrentDirectory + "\\Img\\Event.png", true);
                            if (point != null)
                            {
                                Variables.ScriptLog("Image matched", Color.Lime);
                                BotCore.SendTap(point.Value);
                                break;
                            }
                            ScriptErrorHandler.ErrorHandle();
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (point == null)
                    {
                        Variables.ScriptLog("Event.png not found, force enter event and check what event...", Color.Red);
                        BotCore.SendTap(eventlocation);
                        Variables.ScriptLog("Updating Event.png...", Color.White);
                        var save = BotCore.CropImage(image, eventlocation, new Point(eventlocation.X + 60, eventlocation.Y + 30));
                        if (File.Exists("Img\\Event.png"))
                        {
                            File.Delete(("Img\\Event.png"));
                        }
                        BotCore.Decompress(save).Save(Environment.CurrentDirectory + "\\Img\\Event.png");
                    }
                }
                else
                {
                    BotCore.SendTap(130, 520);
                }
            }
            else
            {
                BotCore.SendTap(130, 520);
            }
            BotCore.Delay(9000, 12000);
            Variables.ScriptLog("Detecting Events", Color.Green);
            error = 0;
            point = null;
            do
            {
                if (!BotCore.GameIsForeground("com.nubee.valkyriecrusade"))
                {
                    return;
                }
                image = BotCore.ImageCapture();
                var crop = BotCore.CropImage(image, new Point(125, 0), new Point(900, 510));
                if (BotCore.FindImage(image, Img.ArchwitchHunt, true) != null)
                {
                    //Is Archwitch, we will save it to another filename as we have to use it later!
                    if (File.Exists("Img\\ArchwitchHunt.png"))
                    {
                        File.Delete("Img\\ArchwitchHunt.png");
                    }
                    File.Move("Img\\Event.png", "Img\\ArchwitchHunt.png");
                    selectedeventlocations++;
                    ScriptErrorHandler.Reset("Archwitch Event found, but we don't want to run this!");
                    return;
                }
                if (BotCore.FindImage(image, Img.Archwitch, true) != null)
                {
                    File.Delete("Img\\Event.png");
                    selectedeventlocations++;
                    ScriptErrorHandler.Reset("Archwitch Event found, but we don't want to run this!");
                    return;
                }
                point = BotCore.FindImage(crop, Img.GreenButton, false);
                if (point != null)
                {
                    BotCore.SendTap(point.Value);
                    continue;
                }
                if (BotCore.FindImage(image, Img.Demon_Tutorial, true) != null || BotCore.FindImage(image, Img.Tower_Tutorial, true) != null)
                {
                    for(int x = 0; x < 10; x++)
                    {
                        BotCore.SendTap(300, 300);
                        BotCore.Delay(100);
                    }
                    continue;
                }
                if (BotCore.FindImage(image, Img.Locate_Tower, true) != null)
                {
                    //Is Tower Event
                    PrivateVariable.EventType = 0;
                    PrivateVariable.InEventScreen = true;
                    return;
                }
                if (BotCore.FindImage(image, Img.Demon_InEvent, true) != null)
                {
                    //Is Demon Event
                    PrivateVariable.EventType = 2;
                    PrivateVariable.InEventScreen = true;
                    return;
                }
                if (BotCore.FindImage(image, Img.HellLoc, true) != null)
                {
                    //Is Demon Event
                    PrivateVariable.EventType = 2;
                    PrivateVariable.InEventScreen = true;
                    return;
                }
                if (BotCore.RGBComparer(image, new Point(109, 705), Color.FromArgb(130, 130, 130), 5) && BotCore.RGBComparer(image, new Point(219, 705), Color.FromArgb(130, 130, 130), 5))
                {
                    Variables.ScriptLog("Rare error happens, still in main screen!", Color.Red);
                    PrivateVariable.InMainScreen = false;
                    return;
                }
                crop = BotCore.CropImage(image, new Point(140, 0), new Point(1160, 720));
                if (BotCore.FindImage(crop, Img.Red_Button, false) != null)
                {
                    Variables.ScriptLog("Battle Screen found. Starting battle!", Color.Lime);
                    PrivateVariable.Battling = true;
                    PrivateVariable.InEventScreen = true;
                    return;
                }
                if (error > 5)
                {
                    //The event is not readable, try to click another event!
                    if(selectedeventlocations < eventlocations.Length)
                    {
                        selectedeventlocations++;
                    }
                    else
                    {
                        MessageBox.Show("Current all event is not supported by VCBot!");
                    }
                    if (File.Exists("Img\\Event.png"))
                    {
                        File.Delete("Img\\Event.png");
                    }
                    ScriptErrorHandler.Reset("Event unable to detected, restarting...");
                    error = 0;
                    return;
                }
                ScriptErrorHandler.ErrorHandle();
                error++;
            }
            while (true);
        }
        //Tower Event
        private static void Tower()
        {
            Debug_.WriteLine();
            BotCore.Delay(1000, false);
            image = BotCore.ImageCapture();
            Point? point = BotCore.FindImage(image, Img.Close2, false);
            if (point != null)
            {
                BotCore.SendTap(new Point(point.Value.X, point.Value.Y));
                BotCore.Delay(1000, false);
            }
            image = BotCore.ImageCapture();
            Variables.ScriptLog("Locating Tower Event UI!", Color.White);
            if (BotCore.FindImage(image, Img.Locate_Tower, true) != null)
            {
                image = BotCore.ImageCapture();
                Tower_Floor = OCR.OcrImage(BotCore.CropImage(image, new Point(280, 110), new Point(440, 145)), "eng");
                Tower_Rank = OCR.OcrImage(BotCore.CropImage(image, new Point(280, 140), new Point(410, 170)), "eng");
                Variables.ScriptLog("Tower Event Found!", Color.Lime);
                PrivateVariable.InEventScreen = true;
            }
            else
            {
                PrivateVariable.InMainScreen = false;
                PrivateVariable.InEventScreen = false;
                return;
            }
            image = BotCore.ImageCapture();
            while (!BotCore.RGBComparer(image, new Point(135, 526), 13,46,74, 10))
            {
                BotCore.Delay(1000, true);
                image = BotCore.ImageCapture();
            }
            energy = GetEnergy();
            runes = GetRune();
            Variables.ScriptLog("Current have " + energy + " energy and " + runes + " runes", Color.LightSkyBlue);
            if(energy == 0)
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
                        if (runes == 5)
                        {
                            Variables.ScriptLog("Use item as it is now rune!", Color.White);
                        }
                        else
                        {
                            Variables.ScriptLog("Close game and wait for energy because of no energy left", Color.White);
                            NoEnergy();
                            PrivateVariable.InEventScreen = false;
                            PrivateVariable.InMainScreen = false;
                            PrivateVariable.Battling = false;
                            return;
                        }
                    }
                    else
                    {
                        Variables.ScriptLog("Close game and wait for energy because of no energy left", Color.White);
                        NoEnergy();
                        PrivateVariable.InEventScreen = false;
                        PrivateVariable.InMainScreen = false;
                        PrivateVariable.Battling = false;
                        return;
                    }
                }
            }
            Variables.ScriptLog("Entering Stage!", Color.Lime);
            image = BotCore.ImageCapture();
                switch (MainScreen.Level)
                {
                    case 0:
                        BotCore.SendTap(196, 648);
                        break;
                    case 1:
                        if (BotCore.RGBComparer(image, new Point(328, 621), Color.FromArgb(13, 12, 12), 35))
                        {
                            Variables.ScriptLog("中级还没被解锁！自动往下挑战中！", Color.Red);
                            BotCore.SendTap(196, 648);
                            break;
                        }
                        BotCore.SendTap(391, 648);
                        break;
                    case 2:
                        if (BotCore.RGBComparer(image, new Point(515, 625), Color.FromArgb(12, 11, 12), 35))
                        {
                            Variables.ScriptLog("上级还没被解锁！自动往下挑战中！", Color.Red);
                            if (BotCore.RGBComparer(image, new Point(328, 621), Color.FromArgb(13, 12, 12), 5))
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
                        if (BotCore.RGBComparer(image, new Point(703, 622), Color.FromArgb(32, 30, 30), 35))
                        {
                            Variables.ScriptLog("超上级还没被解锁！自动往下挑战中！", Color.Red);
                            if (BotCore.RGBComparer(image, new Point(515, 625), Color.FromArgb(12, 11, 12), 35))
                            {
                                Variables.ScriptLog("上级还没被解锁！自动往下挑战中！", Color.Red);
                                if (BotCore.RGBComparer(image, new Point(328, 621), Color.FromArgb(13, 12, 12), 35))
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
                        if (BotCore.RGBComparer(image, new Point(885, 621), Color.FromArgb(107, 100, 100), 90))
                        {
                            Variables.ScriptLog("霸级还没被解锁！自动往下挑战中！", Color.Red);
                            if (BotCore.RGBComparer(image, new Point(703, 621), Color.FromArgb(107, 100, 100), 90))
                            {
                                Variables.ScriptLog("超上级还没被解锁！自动往下挑战中！", Color.Red);
                                if (BotCore.RGBComparer(image, new Point(515, 621), Color.FromArgb(107, 100, 100), 90))
                                {
                                    Variables.ScriptLog("上级还没被解锁！自动往下挑战中！", Color.Red);
                                    if (BotCore.RGBComparer(image, new Point(328, 621), Color.FromArgb(117, 100, 100), 90))
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
                }
            BotCore.Delay(3000, false);
            do
            {
                if (!BotCore.GameIsForeground("com.nubee.valkyriecrusade"))
                {
                    return;
                }
                if (PrivateVariable.Use_Item && energy == 0 && runes == 5)
                {
                    image = BotCore.ImageCapture();
                    if (BotCore.GetPixel(new Point(798, 313), image) != Color.FromArgb(27, 95, 22))
                    {
                        BotCore.Delay(1000, false);
                        continue;
                    }
                    BotCore.SendTap(798, 313);
                    image = BotCore.ImageCapture();
                    Point? p = BotCore.FindImage(image, Img.GreenButton, false);
                    while (p == null)
                    {
                        if (!BotCore.GameIsForeground("com.nubee.valkyriecrusade"))
                        {
                            return;
                        }
                        BotCore.Delay(400, 600);
                        image = BotCore.ImageCapture();
                        p = BotCore.FindImage(image, Img.GreenButton, false);
                    }
                    BotCore.SendTap(p.Value);
                    energy = 5;
                    BotCore.Delay(5000, false);
                }
                image = BotCore.ImageCapture();
                if (BotCore.RGBComparer(image, new Point(959, 656), 31, 102, 26, 4))
                {
                    Variables.ScriptLog("Start battle", Color.Lime);
                    BotCore.SendTap(new Point(959, 656));
                    BotCore.Delay(7000, false);
                    BotCore.SendTap(640, 400); //Tap away Round Battle Text
                    BotCore.Delay(2000, false);
                    stop.Start();
                    PrivateVariable.Battling = true;
                    energy--; //Calculate Energy used
                    if(nextOnline < DateTime.Now)
                    {
                        nextOnline = DateTime.Now;
                    }
                    nextOnline = nextOnline.AddMinutes(45);
                    BotCore.Delay(1000, false);
                    break;
                }
                else
                {
                    image = BotCore.ImageCapture();
                    var crop = BotCore.CropImage(image, new Point(125, 600), new Point(1270, 10));
                    point = BotCore.FindImage(crop, Img.Red_Button, false);
                    if (point != null)
                    {
                        Variables.ScriptLog("Rune boss found!", Color.Yellow);
                        BotCore.SendTap(new Point(point.Value.X + 125, point.Value.Y));
                        RuneBoss = true;
                        BotCore.Delay(9000, 12000);
                    }
                    else
                    {
                        ScriptErrorHandler.ErrorHandle();
                    }
                }
                image = BotCore.ImageCapture();
                ScriptErrorHandler.ErrorHandle();
            }
            while (!PrivateVariable.Battling);

        }
        //Demon Event
        private static void Demon_Realm()
        {
            Debug_.WriteLine();
            Point? point = null;
            int error = 0;
            while (true)
            {
                image = BotCore.ImageCapture();
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
                if (BotCore.RGBComparer(image, new Point(415, 678), Color.FromArgb(223, 192, 63), 10))
                {
                    PrivateVariable.EventType = 2;
                    PrivateVariable.InEventScreen = true;
                    DemonStage_Enter();
                    return;
                }
                image = BotCore.ImageCapture();
                Variables.ScriptLog("Locating Demon Realm Event UI!", Color.White);
                if (BotCore.RGBComparer(image, new Point(600, 405), Color.FromArgb(59, 30, 37), 15))
                {
                    Tower_Floor = OCR.OcrImage(BotCore.CropImage(image, new Point(300, 115), new Point(484, 142)), "eng");
                    Tower_Rank = OCR.OcrImage(BotCore.CropImage(image, new Point(300, 150), new Point(458, 170)), "eng");
                    Variables.ScriptLog("Demon Realm Event Found!", Color.Lime);
                    PrivateVariable.InEventScreen = true;
                    energy = GetEnergy();
                    runes = GetRune();
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

            if (energy == 0)
            {
                Variables.ScriptLog("Waiting for energy", Color.Yellow);
                Variables.ScriptLog("Close game and wait for energy because of no energy left", Color.Yellow);
                NoEnergy();
                PrivateVariable.InEventScreen = false;
                PrivateVariable.InMainScreen = false;
                PrivateVariable.Battling = false;
                return;
            }
            Variables.ScriptLog("Enterting Stage", Color.White);
            BotCore.SendSwipe(new Point(307, 249), new Point(305, 403),300);
            BotCore.Delay(1500);
            switch (MainScreen.Level)
            {
                case 0:
                    BotCore.SendTap(250, 284);
                    break;
                case 1:
                    if (BotCore.RGBComparer(image, new Point(143, 355), Color.FromArgb(51, 16, 5), 20))
                    {
                        Variables.ScriptLog("中级还没被解锁！自动往下挑战中！", Color.Red);
                        BotCore.SendTap(250, 284);
                        break;
                    }
                    BotCore.SendTap(362, 283);
                    break;
                case 2:
                    if (BotCore.RGBComparer(image, new Point(143, 355), Color.FromArgb(51, 16, 5), 20))
                    {
                        Variables.ScriptLog("上级还没被解锁！自动往下挑战中！", Color.Red);
                        if (BotCore.RGBComparer(image, new Point(324, 270), Color.FromArgb(51, 16, 5), 20))
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
                    if (BotCore.RGBComparer(image, new Point(324, 355), Color.FromArgb(51, 16, 5), 20))
                    {
                        Variables.ScriptLog("超上级还没被解锁！自动往下挑战中！", Color.Red);
                        if (BotCore.RGBComparer(image, new Point(143, 355), Color.FromArgb(51, 16, 5), 20))
                        {
                            Variables.ScriptLog("上级还没被解锁！自动往下挑战中！", Color.Red);
                            if (BotCore.RGBComparer(image, new Point(324, 270), Color.FromArgb(51, 16, 5), 20))
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
                    if (BotCore.RGBComparer(image, new Point(324, 355), Color.FromArgb(51, 16, 5), 20))
                    {
                        Variables.ScriptLog("超上级还没被解锁！自动往下挑战中！", Color.Red);
                        if (BotCore.RGBComparer(image, new Point(143, 355), Color.FromArgb(51, 16, 5), 20))
                        {
                            Variables.ScriptLog("上级还没被解锁！自动往下挑战中！", Color.Red);
                            if (BotCore.RGBComparer(image, new Point(324, 270), Color.FromArgb(51, 16, 5), 20))
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
                image = BotCore.ImageCapture();
                if (BotCore.RGBComparer(image, new Point(959, 656), 31, 102, 26, 4))
                {
                    Variables.ScriptLog("Start battle", Color.Lime);
                    BotCore.SendTap(new Point(959, 656));
                    BotCore.Delay(2000, false);
                    if (runes == 4 && energy == 5)
                    {
                        BotCore.SendSwipe(new Point(640, 473), new Point(640, 280), 1000);
                        BotCore.Delay(500, false);
                    }
                    BotCore.SendTap(new Point(758, 566));
                    BotCore.Delay(6000, 8000);
                    BotCore.SendTap(640, 400); //Tap away Round Battle Text
                    BotCore.Delay(2000, false);
                    stop.Start();
                    energy--; //Calculate Energy used
                    nextOnline = nextOnline.AddMinutes(45);
                    EnteredStage = true;
                    BotCore.Delay(5000, false);
                    break;
                }
                else
                {
                    BotCore.Delay(1000, 1500);
                    error++;
                    if(error > 10)
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
            image = BotCore.ImageCapture();
            while (!BotCore.RGBComparer(image, new Point(415, 678), Color.FromArgb(223, 192, 63), 10))
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
                image = BotCore.ImageCapture();
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
                image = BotCore.ImageCapture();
                var crop = BotCore.CropImage(image, new Point(0, 0), new Point(1280, 615));
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
                                BotCore.Delay(2000, false);
                                BotCore.SendTap(768, 536);
                                BotCore.Delay(5000, false);
                                image = BotCore.ImageCapture();
                                if (BotCore.RGBComparer(image, new Point(1003, 658), Color.FromArgb(118, 0, 8), 5))
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
                                    p = null;
                                    continue;
                                }
                                BotCore.SendTap(970, 614);
                                BotCore.Delay(2000, false);
                                BotCore.SendTap(753, 423);
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
                            if (runes == 3 && energy != 5)
                            {
                                StuckRune();
                                return;
                            }
                            BotCore.SendTap(p.Value);
                            break;
                        }
                    }
                }
                image = BotCore.ImageCapture();
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
                image = BotCore.ImageCapture();
                var crop = BotCore.CropImage(image, new Point(0, 0), new Point(1280, 600));
                Point? p2 = BotCore.FindImage(image, Img.GreenButton, false);
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
                image = BotCore.ImageCapture();
                point = BotCore.FindImage(image, Img.Red_Button, false);
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
            stop.Start();
        }
        static int locateUIError = 0;
        //Fighting and locate UI
        private static void LocateUI()
        {
            BotCore.Delay(1000);
            if (!BotCore.RGBComparer(image, new Point(10, 27), Color.FromArgb(200, 200, 200), 5))
            {
                Variables.ScriptLog("HP bar not found. Finding UIs", Color.Yellow);
                Attackable = false;
                Random rnd = new Random();
                BotCore.SendTap(rnd.Next(5, 15), rnd.Next(5, 15));
                ScriptErrorHandler.ErrorHandle();
                Point? point = BotCore.FindImage(image, Img.Close2, false);
                if (point != null)
                {
                    BotCore.SendTap(point.Value);
                    BotCore.Delay(1000, 1200);
                    locateUIError = 0;
                    image = BotCore.ImageCapture();
                }
                if (BotCore.FindImage(image, Img.NoEnergy, true) != null)
                {
                    ScriptErrorHandler.Reset("No Energy Left!");
                    NoEnergy();
                    locateUIError = 0;
                    return;
                }
                if (!PrivateVariable.Battling)
                {
                    return;
                }
                if (BotCore.FindImage(image, Img.Demon_Start, true) != null)
                {
                    PrivateVariable.Battling = false;
                    Variables.ScriptLog("Battle Ended!", Color.Lime);
                    stop.Stop();
                    Variables.ScriptLog("Battle used up " + stop.Elapsed, Color.Lime);
                    stop.Reset();
                    BotCore.SendTap(1076, 106);
                    
                    locateUIError = 0;
                    return;
                }
                if (!PrivateVariable.Battling)
                {
                    return;
                }
                if (BotCore.FindImage(image, Img.Locate_Tower, true) != null)
                {
                    PrivateVariable.Battling = false;
                    Variables.ScriptLog("Battle Ended!", Color.Lime);
                    stop.Stop();
                    Variables.ScriptLog("Battle used up " + stop.Elapsed, Color.Lime);
                    stop.Reset();
                    
                    locateUIError = 0;
                    return;
                }
                if (!PrivateVariable.Battling)
                {
                    return;
                }
                if (BotCore.FindImage(image, Img.Demon_InEvent, true) != null)
                {
                    PrivateVariable.Battling = false;
                    Variables.ScriptLog("Battle Ended!", Color.Lime);
                    stop.Stop();
                    Variables.ScriptLog("Battle used up " + stop.Elapsed, Color.Lime);
                    stop.Reset();
                    
                    locateUIError = 0;
                    return;
                }
                if (!PrivateVariable.Battling)
                {
                    return;
                }
                var crop = BotCore.CropImage(image, new Point(125, 0), new Point(1280, 720));
                point = BotCore.FindImage(crop, Img.GreenButton, false);
                if (point != null)
                {
                    Variables.ScriptLog("Green Button Found!", Color.Lime);
                    if (PrivateVariable.EventType == 0)
                    {
                        if (RuneBoss && runes >= 3 && runes != 5 && BotCore.FindImage(image, Img.TowerFinished, true) != null)
                        {
                            PrivateVariable.InEventScreen = false;
                            PrivateVariable.InMainScreen = false;
                            PrivateVariable.Battling = false;
                            Stuck = true;
                            Variables.ScriptLog("Battle Ended!", Color.Lime);
                            stop.Stop();
                            Variables.ScriptLog("Battle used up " + stop.Elapsed, Color.Lime);
                            stop.Reset();
                            
                            locateUIError = 0;
                            RuneBoss = false;
                            return;
                        }
                        else
                        {
                            if (BotCore.FindImage(image, Img.Locate_Tower, true) != null)
                            {
                                PrivateVariable.Battling = false;
                                Variables.ScriptLog("Battle Ended!", Color.Lime);
                                stop.Stop();
                                Variables.ScriptLog("Battle used up " + stop.Elapsed, Color.Lime);
                                stop.Reset();
                                
                                locateUIError = 0;
                            }
                            else
                            {
                                BotCore.SendTap(point.Value.X + 125, point.Value.Y);
                                
                                BotCore.Delay(1000);
                                locateUIError = 0;
                                return;
                            }
                        }
                    }
                    else if (PrivateVariable.EventType == 2)
                    {
                        crop = BotCore.CropImage(image, new Point(147, 234), new Point(613, 299));
                        if (BotCore.FindImage(crop, Img.DemonEnd, true) != null)
                        {
                            PrivateVariable.Battling = false;
                            BotCore.SendTap(point.Value.X + 125, point.Value.Y);
                            Variables.ScriptLog("Battle Ended!", Color.Lime);
                            stop.Stop();
                            Variables.ScriptLog("Battle used up " + stop.Elapsed, Color.Lime);
                            stop.Reset();
                            
                            locateUIError = 0;
                            return;
                        }
                        else
                        {
                            BotCore.SendTap(point.Value.X + 125, point.Value.Y);
                            
                            locateUIError = 0;
                            BotCore.Delay(1000);
                            return;
                        }
                    }
                    else
                    {
                        var pt = BotCore.FindImage(crop, Img.PT, true);
                        if (pt != null)
                        {
                            Variables.ScriptLog("Battle Ended!", Color.Lime);
                            BotCore.SendTap(point.Value.X + 125, point.Value.Y);
                            BotCore.Delay(400, false);
                            BotCore.SendTap(point.Value.X + 125, point.Value.Y);
                            BotCore.Delay(400, false);
                            BotCore.SendTap(point.Value.X + 125, point.Value.Y);
                            for (int x = 0; x < 5; x++)
                            {
                                BotCore.SendTap(10, 10);
                            }
                            PrivateVariable.Battling = false;
                            stop.Stop();
                            Variables.ScriptLog("Battle used up " + stop.Elapsed, Color.Lime);
                            stop.Reset();
                            
                            locateUIError = 0;
                            return;
                        }
                        else
                        {
                            BotCore.SendTap(point.Value.X + 125, point.Value.Y);
                            
                            locateUIError = 0;
                            return;
                        }
                    }
                }
                crop = BotCore.CropImage(image, new Point(125, 0), new Point(1280, 720));
                point = BotCore.FindImage(crop, Img.Red_Button, false);
                if (point != null)
                {
                    if (PrivateVariable.EventType == 2)
                    {
                        if (BotCore.RGBComparer(image, new Point(133, 35), Color.FromArgb(30, 30, 30), 10))
                        {
                            PrivateVariable.Battling = false;
                            Variables.ScriptLog("Battle Ended!", Color.Lime);
                            stop.Stop();
                            Variables.ScriptLog("Battle used up " + stop.Elapsed, Color.Lime);
                            stop.Reset();
                            
                            locateUIError = 0;
                            return;
                        }
                    }
                    Variables.ScriptLog("Starting Battle", Color.Lime);
                    BotCore.SendTap(point.Value.X + 125, point.Value.Y);
                    PrivateVariable.Battling = true;
                    BotCore.Delay(900, 1000);
                    crop = BotCore.CropImage(image, new Point(682, 544), new Point(905, 589));
                    if (BotCore.RGBComparer(crop, Color.FromArgb(29, 98, 24)))
                    {
                        BotCore.SendTap(793, 565);
                        BotCore.Delay(1000, false);
                    }
                    
                    locateUIError = 0;
                    return;
                }
                if (!PrivateVariable.Battling)
                {
                    return;
                }
                point = BotCore.FindImage(image, Img.Start_Game, true);
                if (point != null)
                {
                    BotCore.SendTap(point.Value);
                    PrivateVariable.Battling = false;
                    ScriptErrorHandler.Reset("Start Game Button Located!");
                    locateUIError = 0;
                    return;
                }
                if (BotCore.RGBComparer(image, new Point(959, 656), 31, 102, 26, 4))
                {
                    Variables.ScriptLog("Start battle", Color.Lime);
                    BotCore.SendTap(959, 656);
                    PrivateVariable.Battling = true;
                    
                    locateUIError = 0;
                    return;
                }
                if (BotCore.RGBComparer(image, new Point(415, 678), Color.FromArgb(223, 192, 63), 10))
                {
                    PrivateVariable.Battling = false;
                    BotCore.Delay(9000, 12000);
                    return;
                }
                locateUIError++;
                if (locateUIError > 30)
                {
                    //Something wrong, we need to reset
                    BotCore.KillGame("com.nubee.valkyriecrusade");
                    ScriptErrorHandler.Reset("Error on finding UIs, no UI found and no battle screen found!");
                }
            }
        }
        static bool Attackable = true;


        /// <summary>
        /// Check is there any HP bar in game
        /// </summary>
        private static void CheckEnemy()
        {
            if (Attackable)
            {
                Debug_.WriteLine();
                var enemy = BotCore.CropImage(image, new Point(585, 260), new Point(715, 305));
                if (BotCore.RGBComparer(enemy, Color.FromArgb(33, 106, 159)) || BotCore.RGBComparer(enemy, Color.FromArgb(171, 0, 21)))
                {
                    BotCore.SendTap(640, 156); //Boss在中间，打Boss
                    Variables.ScriptLog("Found Boss at center!", Color.LightPink);
                }
                else
                {
                    if (PrivateVariable.Battling == true && PrivateVariable.EventType == 2)
                    {
                        var point = BotCore.FindImage(image, "Img\\HellLoc.png", false);
                        if (point != null)
                        {
                            PrivateVariable.Battling = false;
                            Variables.ScriptLog("Battle Ended!", Color.Lime);
                            return;
                        }
                        BotCore.Delay(100, 500);
                    }
                    //找不到Boss血量条，可能剩下一丝血，所以先打中间试试水，再打小怪
                    BotCore.SendTap(640, 156);
                    BotCore.Delay(100, 500);
                    BotCore.SendTap(462, 176);
                    BotCore.Delay(100, 500);
                    BotCore.SendTap(820, 187);
                    BotCore.Delay(100, 500);
                    BotCore.SendTap(330, 202);
                    BotCore.Delay(100, 500);
                    BotCore.SendTap(952, 193);
                    Variables.ScriptLog("Boss not found, trying to hit others", Color.LightPink);
                }
            }
            Attackable = true;
        }
        private static void Battle()
        {
            do
            {
                Attackable = true;
                BotCore.Delay(500);
                image = BotCore.ImageCapture();
                LocateUI();
                Debug_.WriteLine();
                if (Attackable)
                {
                    Variables.ScriptLog("Locating Skills and enemies", Color.Gold);
                    if (PrivateVariable.BattleScript.Count > 0)
                    {
                        PrivateVariable.BattleScript[PrivateVariable.Selected_Script].Attack();
                        BotCore.Delay(300);
                    }
                    CheckEnemy();
                    Stopwatch delay = Stopwatch.StartNew();
                    Random rnd = new Random();
                    for (int x = 0; x < 15; x++)
                    {
                        BotCore.Delay(100, 300);
                        BotCore.SendTap(rnd.Next(5, 15), rnd.Next(5, 15));
                        if (delay.ElapsedMilliseconds > 3000)
                        {
                            break;
                        }
                    }
                    delay.Stop();
                }
                else
                {
                    for (int x = 0; x < 15; x++)
                    {
                        BotCore.Delay(100, 300);
                        Random rnd = new Random();
                        BotCore.SendTap(rnd.Next(5, 15), rnd.Next(5, 15));
                    }
                }
                if (!BotCore.GameIsForeground("com.nubee.valkyriecrusade"))
                {
                    ScriptErrorHandler.Reset("Game is closed! Restarting all!");
                    return;
                }
            }
            while (PrivateVariable.Battling);
        }
        //Get energy
        public static int GetEnergy()
        {
            Debug_.WriteLine();
            image = BotCore.ImageCapture();
            int num = 0;
            if (PrivateVariable.EventType == 0)
            {
                    Color energy = Color.FromArgb(50, 233, 34);
                    if (BotCore.RGBComparer(image, new Point(417, 535), energy, 10))
                    {
                        num++;
                    }
                    if (BotCore.RGBComparer(image, new Point(481, 535), energy, 10))
                    {
                        num++;
                    }
                    if (BotCore.RGBComparer(image, new Point(546, 535), energy, 10))
                    {
                        num++;
                    }
                    if (BotCore.RGBComparer(image, new Point(613, 535), energy, 10))
                    {
                        num++;
                    }
                    if (BotCore.RGBComparer(image, new Point(677, 535), energy, 10))
                    {
                        num++;
                    }
                
            }
            else
            {
                Color energy = Color.FromArgb(104, 45, 22);
                if (BotCore.RGBComparer(image, new Point(208, 445), energy, 10))
                {
                    num++;
                }
                if (BotCore.RGBComparer(image, new Point(253, 441), energy, 10))
                {
                    num++;
                }
                if (BotCore.RGBComparer(image, new Point(315, 445), energy, 10))
                {
                    num++;
                }
                if (BotCore.RGBComparer(image, new Point(351, 449), energy, 10))
                {
                    num++;
                }
                if (!BotCore.RGBComparer(image, new Point(410, 458), Color.FromArgb(27, 24, 29), 10))
                {
                    num++;
                }
            }
            if(nextOnline < DateTime.Now)
            {
                var temp = 5 - num;
                nextOnline = DateTime.Now.AddMinutes(temp * 45);
            }
            return num;
        }
        //Get runes
        private static int GetRune()
        {
            Debug_.WriteLine();
            image = BotCore.ImageCapture();
            if (PrivateVariable.EventType == 0)
            {
                int num = 5;
                if (BotCore.RGBComparer(image, new Point(945, 207), 118, 117, 118, 10))
                {
                    num--;
                }
                if (BotCore.RGBComparer(image, new Point(979, 308), 114, 114, 114, 10))
                {
                    num--;
                }
                if (BotCore.RGBComparer(image, new Point(1088, 309), 118, 117, 118, 10))
                {
                    num--;
                }
                if (BotCore.RGBComparer(image, new Point(1121, 204), 113, 113, 113, 10))
                {
                    num--;
                }
                if (BotCore.RGBComparer(image, new Point(1033, 140), 116, 115, 115, 10))
                {
                    num--;
                }
                return num;
            }
            else
            {
                int num = 0;
                if (BotCore.RGBComparer(image, new Point(966, 164), 154, 135, 110, 10))
                {
                    num++;
                }
                if (BotCore.RGBComparer(image, new Point(1071, 173), 195, 178, 145, 10))
                {
                    num++;
                }
                if (BotCore.RGBComparer(image, new Point(980, 240), 142, 119, 97, 10))
                {
                    num++;
                }
                if (!BotCore.RGBComparer(image, new Point(1067, 243), 56, 40, 45, 10))
                {
                    num++;
                }
                return num;
            }
        }
        //Close Game and wait for energy in tower event for stucking rune time
        private static void StuckRune()
        {
            Debug_.WriteLine();
            Variables.ScriptLog("Close game and stuck rune!", Color.DarkGreen);
            Variables.ScriptLog("Estimate online time is " + nextOnline, Color.Lime);
            string filename = Encryption.SHA256(Variables.AdbIpPort);
            if (!Directory.Exists("C:\\ProgramData\\" + filename))
            {
                Directory.CreateDirectory("C:\\ProgramData\\" + filename);
            }
            if (!BotCore.Pull("/data/data/com.nubee.valkyriecrusade/shared_prefs/NUBEE_ID.xml", "C:\\ProgramData\\" + filename + "\\" + filename + ".xml"))
            {
                Variables.ScriptLog("Pull files failed", Color.Red);
            }
            else
            {
                Variables.ScriptLog("Backup saved", Color.Lime);
            }
            BotCore.KillGame("com.nubee.valkyriecrusade");
            Variables.FindConfig("General","Manual_Rune", out string output);
            if (output == "true")
            {
                if (Directory.Exists(Environment.CurrentDirectory + "\\Audio\\"))
                {
                    string[] path = Directory.GetFiles(Environment.CurrentDirectory + "\\Audio\\", "*.wav");
                    if (path.Length > 0)
                    {
                        SoundPlayer player = new SoundPlayer
                        {
                            SoundLocation = path[0]
                        };
                        player.PlayLooping();
                    }
                }
                BotCore.CloseEmulator();
                MessageBox.Show("正在卡符文！下次上线时间为" + nextOnline + "!");
                Environment.Exit(0);
            }
            if (Variables.FindConfig("General","Suspend_PC", out string suspend))
            {
                if (suspend == "true")
                {
                    SleepWake.SetWakeTimer(nextOnline);
                }
                else
                {
                    BotCore.Delay(Convert.ToInt32((nextOnline - DateTime.Now).TotalMilliseconds));
                }
            }
            else
            {
                BotCore.Delay(Convert.ToInt32((nextOnline - DateTime.Now).TotalMilliseconds));
            }
            energy = 5;
        }
        
        //No energy left so close game
        private static void NoEnergy()
        {
            Debug_.WriteLine();
            if (PrivateVariable.EventType == 0 || PrivateVariable.EventType == 2)
            {
                Variables.ScriptLog("Estimate online time is " + nextOnline, Color.Lime);
                BotCore.KillGame("com.nubee.valkyriecrusade");
                string filename = Encryption.SHA256(Variables.AdbIpPort);
                if (!Directory.Exists("C:\\ProgramData\\" + filename))
                {
                    Directory.CreateDirectory("C:\\ProgramData\\" + filename);
                }
                if (!BotCore.Pull("/data/data/com.nubee.valkyriecrusade/shared_prefs/NUBEE_ID.xml", "C:\\ProgramData\\" + filename + "\\" + filename + ".xml"))
                {
                    Variables.ScriptLog("Pull files failed", Color.Red);
                }
                else
                {
                    Variables.ScriptLog("Backup saved", Color.Lime);
                }
                if (DateTime.Now > GetEventXML.guildwar && DateTime.Now < GetEventXML.guildwar.AddDays(9))
                {
                    //Guild war! Online immediately!
                    if(nextOnline.Hour > 9 && nextOnline.Hour < 11)
                    {
                        nextOnline = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 7, 59, 30);
                    }
                    else if (nextOnline.Hour > 13 && nextOnline.Hour < 18)
                    {
                        nextOnline = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 11, 59, 30);
                    }
                    else if (nextOnline.Hour > 20 && nextOnline.Hour < 21)
                    {
                        nextOnline = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 18, 59, 30);
                    }
                    else if (nextOnline.Hour > 23)
                    {
                        nextOnline = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 21, 59, 30);
                    }
                }
                if (Variables.FindConfig("General","Suspend_PC", out string suspend))
                {
                    if (suspend == "true")
                    {
                        SleepWake.SetWakeTimer(nextOnline);
                    }
                    else
                    {
                        BotCore.Delay(Convert.ToInt32((nextOnline - DateTime.Now).TotalMilliseconds));
                    }
                }
                else
                {
                    BotCore.Delay(Convert.ToInt32((nextOnline - DateTime.Now).TotalMilliseconds));
                }
            }
        }
        //Read battle script plugins
        public static void Read_Plugins()
        {
            Debug_.WriteLine();
            if (!Directory.Exists("Battle_Script"))
            {
                Directory.CreateDirectory("Battle_Script");
            }
            string[] files = Directory.GetFiles("Battle_Script", "*.dll");
            if (files.Length > 0)
            {
                foreach (var f in files)
                {
                    var a = Assembly.LoadFrom(f);
                    foreach (var t in a.GetTypes())
                    {
                        if (t.GetInterface("BattleScript") != null)
                        {
                            PrivateVariable.BattleScript.Add(Activator.CreateInstance(t) as BattleScript);
                        }
                    }
                }
                foreach (var s in PrivateVariable.BattleScript)
                {
                    s.ReadConfig();
                }
            }
        }

        static bool Muted_Device = false;

        void ScriptInterface.Script()
        {
            if (Variables.FindConfig("General", "Lang", out string lang))
            {
                if (File.Exists("Profiles\\Verify.vcb"))
                {
                    FileInfo f = new FileInfo("Profiles\\Verify.vcb");
                    if((DateTime.Now - f.LastAccessTime) > new TimeSpan(1, 0, 0))
                    {
                        try
                        {
                            WebClientOverride wc = new WebClientOverride();
                            var url = "https://d2n1d3zrlbtx8o.cloudfront.net/news/info/" + lang + "/index.html";
                            MainScreen.html = wc.DownloadString(new Uri(url));
                            MainScreen.html = MainScreen.html.Replace("bgcolor=\"#000000\"　text color=\"#FFFFFF\"", "style=\"background - color:#303030; color:white\"");
                            MainScreen.html = MainScreen.html.Remove(MainScreen.html.IndexOf("<table width=\"200\">"), MainScreen.html.IndexOf("</table>") - MainScreen.html.IndexOf("<table width=\"200\">"));
                            MainScreen.html = Regex.Replace(MainScreen.html, "(\\<span class\\=\"iro4\"\\>.*</span>)", "");
                        }
                        catch
                        {
                            Variables.ScriptLog("Verification failed! Server not reachable! Request again next round!", Color.Red);
                            MainScreen.html = Img.index;
                        }
                        if (MainScreen.html != Img.index)
                        {
                            if (File.ReadAllText("Profiles\\Verify.vcb") != Encryption.SHA256(MainScreen.html))
                            {
                                Variables.ScriptLog("Verification failed, regenerate new Verify.vcb", Color.Red);
                                //Event is updated
                                if (File.Exists("Img\\Event.png"))
                                {
                                    File.Delete("Img\\Event.png");
                                }
                                File.WriteAllText("Profiles\\Verify.vcb", Encryption.SHA256(MainScreen.html));
                            }
                            else
                            {
                                Variables.ScriptLog("VCBot verified", Color.White);
                            }
                        }
                    }
                   
                }
                else
                {
                    if(MainScreen.html != Img.index)
                    {
                        Variables.ScriptLog("VCBot verified", Color.White);
                        File.WriteAllText("Profiles\\Verify.vcb", Encryption.SHA256(MainScreen.html));
                    }
                }
            }
            Debug_.WriteLine();
            var Japan = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");
            var time = TimeZoneInfo.ConvertTime(DateTime.Now, Japan).TimeOfDay;
            if (time.Hours == 12)//Event change time
            {
                GetEventXML.LoadXMLEvent();
            }
            if (DateTime.Now > GetEventXML.guildwar && DateTime.Now < GetEventXML.guildwar.AddDays(9))
            {
                
                var hour = time.Hours;
                if(hour == 8 || hour == 12 || hour == 19 || hour == 22)
                {
                    Console.Beep();
                    ScriptErrorHandler.Reset("Guild War is running, waiting for end...");
                    double seconds = 0;
                    Console.Beep();
                    switch (hour)
                    {
                        case 8:
                            seconds = (TimeSpan.Parse("8:59:59") - time).TotalMilliseconds;
                            Variables.ScriptLog("Will start game at Japan time 8:59:59", Color.YellowGreen);
                            break;
                        case 12:
                            seconds = (TimeSpan.Parse("12:59:59") - time).TotalMilliseconds;
                            Variables.ScriptLog("Will start game at Japan Time 12:59:59", Color.YellowGreen);
                            break;
                        case 19:
                            seconds = (TimeSpan.Parse("19:59:59") - time).TotalMilliseconds;
                            Variables.ScriptLog("Will start game at Japan Time 19:59:59", Color.YellowGreen);
                            break;
                        case 22:
                            seconds = (TimeSpan.Parse("23:59:59") - time).TotalMilliseconds;
                            Variables.ScriptLog("Will start game at Japan Time 23:59:59", Color.YellowGreen);
                            break;
                    }
                    BotCore.KillGame("com.nubee.valkyriecrusade");
                    BotCore.Delay(Convert.ToInt32(seconds), true);
                }
                /*switch (hour)
                {
                    case 8:
                    case 12:
                    case 19:
                    case 22:
                        ScriptErrorHandler.Reset("Guild War is running!");
                        var point = BotCore.FindImage(image, Img.GreenButton, false);
                        if (point == null)
                        {
                            BotCore.SendTap(155, 663);
                            BotCore.Delay(5000);
                            BotCore.SendTap(628, 96);
                            BotCore.Delay(5000);
                        }
                        else
                        {
                            BotCore.SendTap(point.Value);
                        }
                        do
                        {
                            Variables.ScriptLog("Entering Guild War!", Color.White);
                            GuildWar();
                        } while (hour == 8 || hour == 12 || hour == 19 || hour == 22);
                        break;
                }
                */
            }
            if (!CloseEmu)
            {
                BotCore.Delay(10, true);
                if (Variables.Controlled_Device != null) //The Emulator is running
                {
                    while (Variables.Proc == null)//But not registred on our Proc value
                    {
                        Debug_.WriteLine("Variables.Proc is null");
                        //so go on and find the emulator!
                        BotCore.ConnectAndroidEmulator();
                        //Emulator found!
                        if (Variables.Proc != null)
                        {
                            break;
                        }
                        //Maybe something is wrong, no process is connected!
                        BotCore.StartEmulator();
                    }
                }
                else //The Emulator is not exist!
                {
                    BotCore.StartEmulator(); //Start our fxxking Emulator!!
                    BotCore.ConnectAndroidEmulator();
                    return;
                }
                if (Variables.Proc.HasExited)
                {
                    Variables.Proc = null;
                    return;
                }
                int error = 0;
                BotCore.Delay(10, true);
                do
                {
                    BotCore.Delay(1200, false);
                    image = BotCore.ImageCapture();
                    if (Variables.Controlled_Device == null) //Emulator not started, awaiting...
                    {
                        BotCore.ConnectAndroidEmulator();
                        return;
                    }
                    if (Variables.Proc == null)
                    {
                        BotCore.StartEmulator();
                        BotCore.ConnectAndroidEmulator();
                        return;
                    }
                    error++;
                    if (error > 60)
                    {
                        BotCore.RestartEmulator();
                        error = 0;
                    }
                } while (image == null);
                BotCore.Delay(10, true);
                string filename = Encryption.SHA256(Variables.AdbIpPort);
                if (!Directory.Exists("C:\\ProgramData\\" + filename))
                {
                    Directory.CreateDirectory("C:\\ProgramData\\" + filename);
                }

                if (!File.Exists("C:\\ProgramData\\" + filename + "\\" + filename + ".xml"))
                {
                    if (!BotCore.Pull("/data/data/com.nubee.valkyriecrusade/shared_prefs/NUBEE_ID.xml", "C:\\ProgramData\\" + filename + "\\" + filename + ".xml"))
                    {
                        Variables.ScriptLog("Pull files failed", Color.Red);
                    }
                    else
                    {
                        Variables.ScriptLog("Backup saved", Color.Lime);
                    }
                }
                else
                {
                    if (!pushed)
                    {
                        BotCore.Push("C:\\ProgramData\\" + filename + "\\" + filename + ".xml", "/data/data/com.nubee.valkyriecrusade/shared_prefs/NUBEE_ID.xml", 660);
                        pushed = true;
                        Variables.ScriptLog("Restored backup xml", Color.Lime);
                        BotCore.Delay(1000, false);
                    }
                }
            }
            if (Stuck)
            {
                StuckRune();
                Stuck = false;
                return;
            }
            BotCore.Delay(10, true);
            if (!BotCore.GameIsForeground("com.nubee.valkyriecrusade"))
            {
                for (int e = 0; e < 10; e++)
                {
                    Variables.ScriptLog("Starting Game", Color.Lime);
                    if (PrivateVariable.biubiu)
                    {
                        BotCore.StartGame("com.njh.biubiu/com.njh.ping.core.business.LauncherActivity");
                        do
                        {
                            Variables.WinApiCapt = false;
                            image = BotCore.ImageCapture();
                            var p = BotCore.FindImage(image, Img.biubiu, false);
                            if (p != null)
                            {
                                BotCore.SendTap(p.Value.X + 25, p.Value.Y - 600);
                                BotCore.Delay(10000);
                                BotCore.SendTap(620,320);
                                break;
                            }
                            else
                            {
                                p = BotCore.FindImage(image, Img.biubiu2, false);
                                if (p != null)
                                {
                                    BotCore.SendTap(p.Value);
                                    break;
                                }
                            }
                            BotCore.Delay(10000);
                            Variables.WinApiCapt = true;
                        }
                        while (true);
                    }
                    BotCore.StartGame("com.nubee.valkyriecrusade/.GameActivity");
                    BotCore.Delay(3000);
                    if (BotCore.GameIsForeground("com.nubee.valkyriecrusade"))
                    {
                        nextOnline = DateTime.Now;
                        Collected = false;
                        break;
                    }
                    if (e == 9)
                    {
                        BotCore.RestartEmulator();
                    }
                }
            }
            else
            {
                if (!PrivateVariable.InMainScreen && !PrivateVariable.InEventScreen && !PrivateVariable.Battling)
                {
                    LocateMainScreen();
                }
                else
                {
                    if (!PrivateVariable.InEventScreen)
                    {
                        CheckEvent();
                    }
                    else
                    {
                        if (!PrivateVariable.Battling)
                        {
                            switch (PrivateVariable.EventType)
                            {
                                case 0:
                                    Tower();
                                    break;
                                case 1:
                                    //Archwitch();
                                    break;
                                case 2:
                                    Demon_Realm();
                                    break;
                                default:
                                    Variables.ScriptLog("Unknown error occur, unable to detect event type.", Color.Red);
                                    PrivateVariable.InEventScreen = false;
                                    break;
                            }
                        }
                        else
                        {
                            Battle();
                        }
                    }
                }
            }
        }

        public void ResetScript()
        {
            ScriptErrorHandler.Reset("Reset script!");
        }
    }
}

