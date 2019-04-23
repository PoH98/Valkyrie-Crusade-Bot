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

namespace UI
{
    public class Script
    {
        public static Stopwatch stop = new Stopwatch();
        public static bool RuneBoss, Stuck, EnterWitchGate, Archwitch_Repeat, DisableAutoCheckEvent, CloseEmu = false, pushed = false;
        public static int runes, energy;
        public static int Archwitch_Stage;
        public static int TreasureHuntIndex = -1;
        private static int Retry = 0, error = 0;
        public static string Tower_Floor = "", Tower_Rank = "";
        public static byte[] image = null;
        public static Point archwitch_level_location;
        public static DateTime nextOnline;
        //Main Loop
        public static void Bot()
        {
            Debug_.WriteLine();
            while (PrivateVariable.Run)
            {
                if (DateTime.Now > GetEventXML.guildwar && DateTime.Now < GetEventXML.guildwar.AddDays(10))
                {
                    var Japan = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");
                    var time = TimeZoneInfo.ConvertTime(DateTime.Now, Japan).TimeOfDay;
                    var hour = time.Hours;
                    if (hour == 8 || hour == 12 || hour == 19 || hour == 22)
                    {
                        Console.Beep();
                        ScriptErrorHandler.Reset("Guild War is running, waiting for end...");
                        double seconds = 0;
                        Console.Beep();
                        switch (hour)
                        {
                            case 8:
                                seconds = (TimeSpan.Parse("8:59:59") - time).TotalMilliseconds;
                                Variables.ScriptLog("Will start game at Japan time 8:59:59",Color.YellowGreen);
                                break;
                            case 12:
                                seconds = (TimeSpan.Parse("12:59:59") - time).TotalMilliseconds;
                                Variables.ScriptLog("Will start game at Japan Time 12:59:59",Color.YellowGreen);
                                break;
                            case 19:
                                seconds = (TimeSpan.Parse("19:59:59") - time).TotalMilliseconds;
                                Variables.ScriptLog("Will start game at Japan Time 19:59:59",Color.YellowGreen);
                                break;
                            case 22:
                                seconds = (TimeSpan.Parse("23:59:59") - time).TotalMilliseconds;
                                Variables.ScriptLog("Will start game at Japan Time 23:59:59",Color.YellowGreen);
                                break;
                        }
                        BotCore.KillGame("com.nubee.valkyriecrusade");
                        BotCore.Delay(Convert.ToInt32(seconds),true);
                    }
                }
                if (!CloseEmu)
                {
                    BotCore.Delay(10,true);
                    if (Variables.Controlled_Device != null) //The Emulator is running
                    {
                        while (Variables.Proc == null)//But not registred on our Proc value
                        {
                            Debug_.WriteLine("Variables.Proc is null");
                            //so go on and find the emulator!
                            BotCore.ConnectAndroidEmulator();
                            //MEmu found!
                            if (Variables.Proc != null)
                            {
                                break;
                            }
                            //Maybe something is wrong, no process is same name as MEmu!
                            BotCore.StartEmulator();
                            BotCore.Delay(9000,12000);
                        }
                    }
                    else //The Emulator is not exist!
                    {
                        BotCore.StartEmulator(); //Start our fxxking Emulator!!
                        BotCore.Delay(9000,12000); //Wait
                        BotCore.ConnectAndroidEmulator();
                        continue;
                    }
                    if (!BotCore.StartAdb())
                    {
                        MessageBox.Show("Unable to start adb!");
                        Environment.Exit(0);
                    }
                    int error = 0;
                    BotCore.Delay(10,true);
                    while (image == null) //Weird problem happens, we still cannot receive any image capture!
                    {
                        if (!PrivateVariable.Run)
                        {
                            return;
                        }
                        BotCore.Delay(1000,false); //Wait forever?
                        if(Variables.Controlled_Device == null) //Emulator not started, awaiting...
                        {
                            continue;
                        }
                        Variables.ScriptLog("Waiting for first tons of image buffer",Color.Yellow);
                        error++;
                        if (error > 30) //Nah, we only wait for 30 sec
                        {
                            MessageBox.Show("无法截图！出现怪异错误！");
                            Environment.Exit(0);
                        }
                    }
                    BotCore.Delay(10,true);
                    if (Variables.Instance.Length < 5)
                    {
                        Variables.Instance = "MEmu";
                    }
                    string filename = BotCore.SHA256(Variables.AdbIpPort);
                    if (!Directory.Exists("C:\\ProgramData\\" + filename))
                    {
                        Directory.CreateDirectory("C:\\ProgramData\\" + filename);
                    }

                    if (!File.Exists("C:\\ProgramData\\" + filename + "\\" + filename + ".xml"))
                    {
                        if (!BotCore.Pull("/data/data/com.nubee.valkyriecrusade/shared_prefs/NUBEE_ID.xml", "C:\\ProgramData\\" + filename + "\\" + filename + ".xml"))
                        {
                            Variables.ScriptLog("Pull files failed",Color.Red);
                        }
                        else
                        {
                            Variables.ScriptLog("Backup saved",Color.Lime);
                        }
                    }
                    else
                    {
                        if (!pushed)
                        {
                            BotCore.Push("C:\\ProgramData\\" + filename + "\\" + filename + ".xml", "/data/data/com.nubee.valkyriecrusade/shared_prefs/NUBEE_ID.xml", 660);
                            pushed = true;
                            Variables.ScriptLog("Restored backup xml",Color.Lime);
                            BotCore.Delay(1000,false);
                        }
                    }
                    Image img = BotCore.Decompress(Script.image);
                    try
                    {
                        if (img.Height != 720 || img.Width != 1280)
                        {
                            Debug_.WriteLine("Image size not correct: " + img.Width + "*" + img.Height);
                            if (!PrivateVariable.Run)
                            {
                                return;
                            }
                            Variables.ScriptLog("Emulator's screen size is not 1280*720! Detected size is " + img.Width + "*" + img.Height, Color.LightYellow);
                            BotCore.ResizeEmulator(1280, 720);
                            BotCore.Delay(20000, 30000);
                            continue;
                        }
                    }
                    catch
                    {
                        continue;
                    }

                }
                if (Stuck)
                {
                    StuckRune();
                    Stuck = false;
                    continue;
                }
                BotCore.Delay(10,true);
                if (!BotCore.GameIsForeground("com.nubee.valkyriecrusade"))
                {
                    for (int e = 0; e < 10; e++)
                    {
                        Variables.ScriptLog("Starting Game", Color.Lime);
                        BotCore.StartGame(Img.Icon, image);
                        BotCore.Delay(5000,false);
                        if (BotCore.GameIsForeground("com.nubee.valkyriecrusade"))
                        {
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
        }
        //Try Locate MainScreen
        public static void LocateMainScreen()
        {
            Debug_.WriteLine();
            BotCore.Delay(1000,false);
            PrivateVariable.InMainScreen = false;
            Point? point = null;
            if (!PrivateVariable.Run)
            {
                return;
            }
            BotCore.Delay(100,200);
            var crop = BotCore.CropImage(image, new Point(315, 150), new Point(1005, 590));
            point = BotCore.FindImage(crop, Img.GreenButton, false);
            if (point != null)
            {
                BotCore.SendTap(point.Value);
            }
            if (!BotCore.RGBComparer(image,new Point(109, 705),Color.FromArgb(130,130,130), 5) && !BotCore.RGBComparer(image, new Point(219, 705), Color.FromArgb(130, 130, 130), 5))
            {
                if (!PrivateVariable.Run)
                {
                    return;
                }
                Variables.ScriptLog("Main Screen not visible",Color.White);
                point = BotCore.FindImage(image, Img.Start_Game, true);
                if (point != null)
                {
                    Variables.ScriptLog("Start Game Button Located!",Color.Lime);
                    BotCore.SendTap(point.Value);
                    error = 0;
                    return;
                }
                if (!PrivateVariable.Run)
                {
                    return;
                }
                BotCore.Delay(100,200);
                point = BotCore.FindImage(image, Img.Update_Complete, true);
                if (point != null)
                {
                    BotCore.SendTap(point.Value);
                    return;
                }
                if (!PrivateVariable.Run)
                {
                    return;
                }
                BotCore.Delay(100,200);
                point = BotCore.FindImage(image, Img.Close2, true);
                if (point != null)
                {
                    BotCore.SendTap(point.Value);
                    return;
                }
                if (!PrivateVariable.Run)
                {
                    return;
                }
                point = BotCore.FindImage(image, Img.Close, true);
                if (point != null)
                {
                    BotCore.SendTap(point.Value);
                    return;
                }
                if (!PrivateVariable.Run)
                {
                    return;
                }
                BotCore.Delay(100,200);
                point = BotCore.FindImage(image, Img.Login_Reward, true);
                if (point != null)
                {
                    for (int x = 0; x < 4; x++)
                    {
                        BotCore.SendTap(new Point(600, 350));
                        BotCore.Delay(1000,false);
                    }
                    return;
                }
                if (!PrivateVariable.Run)
                {
                    return;
                }
                BotCore.Delay(100,200);
                point = BotCore.FindImage(image, Img.Back_to_Village, true);
                if (point != null)
                {
                    Variables.ScriptLog("Going back to Main screen",Color.Lime);
                    BotCore.SendTap(point.Value);
                    PrivateVariable.InMainScreen = true;
                    Variables.ScriptLog("Screen Located",Color.Lime);
                }
                if (!PrivateVariable.Run)
                {
                    return;
                }
                BotCore.Delay(100,200);
                point = BotCore.FindImage(image, Img.Menu, true);
                if (point == null)
                {
                    if (error < 30)
                    {
                        if(error == 0)
                        {
                            Variables.ScriptLog("Waiting for Main screen",Color.White);
                        }
                        BotCore.Delay(1000,false);
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
                    if (!PrivateVariable.Run)
                    {
                        return;
                    }
                    BotCore.SendTap(point.Value);
                    BotCore.Delay(1000,false);
                    Variables.ScriptLog("Returning main screen",Color.Lime);
                    BotCore.SendTap(942, 630);
                    BotCore.Delay(5000,false);
                }
                BotCore.Delay(100,200);
                if (!PrivateVariable.Run)
                {
                    return;
                }
                point = BotCore.FindImage(image, Img.GreenButton, false);
                if (point != null)
                {
                    BotCore.SendTap(point.Value);
                    Variables.ScriptLog("Green Button Found!",Color.Lime);
                }
                ScriptErrorHandler.ErrorHandle();
            }
            else
            {
                Retry++;
                if(Retry > 5)
                {
                    PrivateVariable.InMainScreen = true;
                    Variables.ScriptLog("Screen Located",Color.White);
                    //Collect();
                    Retry = 0;
                }
                else
                {
                    BotCore.Delay(1000,false);
                    if(Retry == 1)
                    {
                        Variables.ScriptLog("Waiting for Login Bonus",Color.DeepPink);
                    }
                }
            }

        }
        //Collect
        private static void Collect()
        {
            Variables.ScriptLog("Collecting Resources",Color.Lime);
            for(int x = 0; x < 4; x++)
            {
                switch (x)
                {
                    case 0:
                        BotCore.SendSwipe(new Point(925, 576), new Point(614, 26),1000);
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
                var crop = BotCore.CropImage(image, new Point(0, 0), new Point(1020, 720));
                //Find image and collect
                foreach (var img in Directory.GetFiles("Img\\Resources\\", "*.png"))
                {
                    Point? p = BotCore.FindImage(crop, img, false);
                    if (p != null)
                    {
                        BotCore.SendTap(p.Value);
                        BotCore.Delay(100,200);
                    }
                }
                BotCore.Delay(400,600);

            }

        }
        //Treasure hunt!
        public static void TreasureHunt()
        {
            Debug_.WriteLine();
            Point? p = null;
            p = BotCore.FindImage(Script.image, Img.TreasureHunt, true);
            //Find for treasure hunt building!
            for (int find = 0; find < 5; find++)
            {
                if (!PrivateVariable.Run)
                {
                    return;
                }
                if (p == null)
                {
                    p = BotCore.FindImage(Script.image, Img.TreasureHunt2, true);
                    if (p == null)
                    {
                        switch (find)
                        {
                            case 0:
                                BotCore.SendSwipe(new Point(300, 500), new Point(1100, 50), 500);
                                break;
                            case 1:
                                BotCore.SendSwipe(new Point(1100, 600), new Point(100, 100), 500);
                                break;
                            case 2:
                                BotCore.SendSwipe(new Point(1100, 275), new Point(500, 500), 500);
                                break;
                            case 3:
                                BotCore.SendSwipe(new Point(400, 50), new Point(900, 600), 500);
                                break;
                            case 4:
                                BotCore.SendSwipe(new Point(200, 500), new Point(500, 500), 500);
                                break;
                        }
                        
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
                BotCore.Delay(1000,false);
            }
            if(p == null)
            {
                Variables.ScriptLog("No Treasure Hunt Building found!",Color.Yellow);
                return;
            }
            Variables.ScriptLog("Treasure hunting...",Color.Lime);
            BotCore.SendTap(p.Value);
            BotCore.Delay(1000,false);
            //Enter Treasure hunt
            if (!PrivateVariable.Run)
            {
                return;
            }
            BotCore.SendTap(879, 642);
            BotCore.Delay(20000,30000);
            if (!PrivateVariable.Run)
            {
                return;
            }
            do
            {
                if (!PrivateVariable.Run)
                {
                    return;
                }
                //If already hunting
                if (BotCore.RGBComparer(Script.image,new Point(973, 344), Color.FromArgb(130, 0, 0),0))
                {
                    Variables.ScriptLog("Already in hunting, exit now!",Color.Lime);
                    //Exit loop
                    BotCore.SendTap(1222, 56);
                    BotCore.Delay(5000,false);
                    break;
                }
                else
                {
                    p = BotCore.FindImage(Script.image, Img.Red_Button, true);
                    if (p != null)
                    {
                        //Finished hunt, collect rewards
                        BotCore.SendTap(p.Value);
                        BotCore.Delay(5000,false);
                        BotCore.SendTap(960, 621);
                        BotCore.Delay(7000,false);
                        p = BotCore.FindImage(Script.image, Img.Map, true);
                        //if found treasure map
                        if (p != null)
                        {
                            //Just ignore that fxxking thing
                            BotCore.SendTap(789, 626);
                            BotCore.Delay(9000,12000);
                            BotCore.SendTap(310, 137);
                        }
                    }
                }
                BotCore.Delay(9000,12000);
                //Back to top
                BotCore.SendSwipe(new Point(600, 200), new Point(600, 600), 1000);
                BotCore.Delay(3000,false);
                //Tap and start another hunt
                BotCore.SendTap(998, 340);
                BotCore.Delay(1000,false);
                BotCore.SendTap(771, 453);
                //Next Treasure hunt
                BotCore.SendTap(1031, 50);
                BotCore.Delay(3000,false);
            }
            while (true);

        }
        //Check Event
        private static void CheckEvent()
        {
            Debug_.WriteLine();
            Point? point = null;
            if (!PrivateVariable.Run)
            {
                return;
            }
            string Special;
            int error = 0;
            if (Variables.Configure.TryGetValue("Double_Event",out Special))
            {
                if(Special == "true")
                {
                    if (File.Exists("Img\\Event.png"))
                    {
                        BotCore.SendTap(170, 630);
                        BotCore.Delay(5000,false);
                        for (int x = 0; x < 5; x++)
                        {
                            Point? located = BotCore.FindImage(image, Environment.CurrentDirectory + "\\Img\\LocateEventSwitch.png", true);
                            if (located == null)
                            {
                                x = x - 1;
                                BotCore.Delay(1000,false);
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
                            Variables.ScriptLog("Finding Event.png on screen",Color.White);
                            point = BotCore.FindImage(image, Environment.CurrentDirectory + "\\Img\\Event.png", true);
                            if (point !=null)
                            {
                                Variables.ScriptLog("Image matched",Color.Lime);
                                BotCore.SendTap(point.Value);
                                break;
                            }
                        }
                        if (point == null)
                        {
                            MessageBox.Show("Event.png可能有问题，请确保截图是正确的！");
                            if (BotCore.handle != null && Variables.Proc != null)
                            {
                                DllImport.SetParent(BotCore.handle, IntPtr.Zero);
                                DllImport.MoveWindow(BotCore.handle, PrivateVariable.EmuDefaultLocation.X, PrivateVariable.EmuDefaultLocation.Y, 1318,752, true);
                            }
                            Environment.Exit(0);
                        }

                    }
                    else
                    {
                        BotCore.SendTap(130, 350);
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
            BotCore.Delay(9000,12000);
            error = 0;
            do
            {
                if (!BotCore.GameIsForeground("com.nubee.valkyriecrusade"))
                {
                    return;
                }
                if (!PrivateVariable.Run)
                {
                    return;
                }
                var crop = BotCore.CropImage(image, new Point(125, 0), new Point(900, 510));
                if (BotCore.FindImage(crop, Img.GreenButton, false) != null)
                {
                    BotCore.SendTap(point.Value);
                    return;
                }
                if (!PrivateVariable.Run)
                {
                    return;
                }
                if (BotCore.FindImage(image, Img.Locate_Tower, true) != null)
                {
                    //Is Tower Event
                    PrivateVariable.EventType = 0;
                    PrivateVariable.InEventScreen = true;
                    return;
                }
                if (!PrivateVariable.Run)
                {
                    return;
                }
                if (BotCore.FindImage(image, Img.Archwitch_Rec, true) != null)
                {
                    //Is Archwitch
                    PrivateVariable.EventType = 1;
                    PrivateVariable.InEventScreen = true;
                    return;
                }
                if (!PrivateVariable.Run)
                {
                    return;
                }
                if (BotCore.FindImage(image, Img.Demon_InEvent, true) != null)
                {
                    PrivateVariable.EventType = 2;
                    PrivateVariable.InEventScreen = true;
                    return;
                }
                if (!PrivateVariable.Run)
                {
                    return;
                }
                if (BotCore.FindImage(image, Img.HellLoc, true) != null)
                {
                    PrivateVariable.EventType = 2;
                    PrivateVariable.InEventScreen = true;
                    return;
                }
                if (!PrivateVariable.Run)
                {
                    return;
                }
                if (BotCore.RGBComparer(image, new Point(109, 705), Color.FromArgb(130, 130, 130), 5) && BotCore.RGBComparer(image, new Point(219, 705), Color.FromArgb(130, 130, 130), 5))
                {
                    Variables.ScriptLog("Rare error happens, still in main screen!",Color.Red);
                    PrivateVariable.InMainScreen = false;
                    return;
                }
                if (!PrivateVariable.Run)
                {
                    return;
                }
                crop = BotCore.CropImage(image, new Point(140, 0), new Point(1160, 720));
                if (BotCore.FindImage(crop, Img.Red_Button, false) != null)
                {
                    Variables.ScriptLog("Battle Screen found. Starting battle!",Color.Lime);
                    PrivateVariable.Battling = true;
                    PrivateVariable.InEventScreen = true;
                    return;
                }
                if (error > 30)
                {
                    BotCore.KillGame("com.nubee.valkyriecrusade");
                    ScriptErrorHandler.Reset("Critical error found! Trying to restart game!");
                    error = 0;
                    return;
                }
                ScriptErrorHandler.ErrorHandle();
                error++;
            }
            while (point == null);
        }
        //Tower Event
        private static void Tower()
        {
            Debug_.WriteLine();
            BotCore.Delay(1000,false);

            Point? point = null;
            //Nope, we are in the tower event main screen! So go on!
            point = BotCore.FindImage(image, Img.Close2, false);
            if (point != null)
            {
                BotCore.SendTap(new Point(point.Value.X , point.Value.Y));
                BotCore.Delay(1000,false);
            }
            if (!PrivateVariable.Run)
            {
                return;
            }
            Variables.ScriptLog("Locating Tower Event UI!",Color.White);
            if (BotCore.FindImage(image, Img.Locate_Tower, true) != null)
            {
                Tower_Floor = OCR.OcrImage(BotCore.CropImage(image, new Point(280, 110), new Point(440, 145)),"eng");
                Tower_Rank = OCR.OcrImage(BotCore.CropImage(image, new Point(280, 140), new Point(410, 170)), "eng");
                Variables.ScriptLog("Tower Event Found!",Color.Lime);
                PrivateVariable.InEventScreen = true;
            }
            else
            {
                PrivateVariable.InMainScreen = false;
                PrivateVariable.InEventScreen = false;
                return;
            }
            if (!PrivateVariable.Run)
            {
                return;
            }
            runes = GetRune();
            energy = GetEnergy();
            Variables.ScriptLog("Current have " + energy + " energy and " + runes + " runes", Color.LightSkyBlue);
            if (!PrivateVariable.Run)
            {
                return;
            }
            if (energy == 0)
            {
                Variables.ScriptLog("Waiting for energy",Color.Yellow);
                if (PrivateVariable.TakePartInNormalStage)
                {
                    BotCore.SendTap(1218, 662);
                    BotCore.Delay(400,600);
                    BotCore.SendTap(744, 622);
                }
                else
                {
                    if (PrivateVariable.Use_Item)
                    {
                        if(runes == 5)
                        {
                            Variables.ScriptLog("Use item as it is now rune!",Color.White);
                        }
                        else
                        {
                            Variables.ScriptLog("Close game and wait for energy because of no energy left",Color.White);
                            if (!PrivateVariable.Run)
                            {
                                return;
                            }
                            NoEnergy();
                            PrivateVariable.InEventScreen = false;
                            PrivateVariable.InMainScreen = false;
                            PrivateVariable.Battling = false;
                            return;
                        }
                    }
                    else
                    {
                        Variables.ScriptLog("Close game and wait for energy because of no energy left",Color.White);
                        if (!PrivateVariable.Run)
                        {
                            return;
                        }
                        NoEnergy();
                        PrivateVariable.InEventScreen = false;
                        PrivateVariable.InMainScreen = false;
                        PrivateVariable.Battling = false;
                        return;
                    }
                }
            }
            Variables.ScriptLog("Entering Stage!",Color.Lime);
            switch (MainScreen.Level)
            {
                case 0:
                    BotCore.SendTap(196, 648);
                    break;
                case 1:
                    if (BotCore.RGBComparer(image, new Point(328, 621), Color.FromArgb(13, 12,12), 5))
                    {
                        Variables.ScriptLog("中级还没被解锁！自动往下挑战中！", Color.Red);
                        BotCore.SendTap(196, 648);
                        break;
                    }
                    BotCore.SendTap(391, 648);
                    break;
                case 2:
                    if (BotCore.RGBComparer(image, new Point(515, 625), Color.FromArgb(12, 11, 12), 5))
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
                    if (BotCore.RGBComparer(image, new Point(703, 622), Color.FromArgb(32, 30, 30), 5))
                    {
                        Variables.ScriptLog("超上级还没被解锁！自动往下挑战中！", Color.Red);
                        if (BotCore.RGBComparer(image, new Point(515, 625), Color.FromArgb(12, 11, 12), 5))
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
                    }
                    BotCore.SendTap(741, 623);
                    break;
                case 4:
                    if (BotCore.RGBComparer(image, new Point(885, 621), Color.FromArgb(15,14,14), 5))
                    {
                        Variables.ScriptLog("霸级还没被解锁！自动往下挑战中！", Color.Red);
                        if (BotCore.RGBComparer(image, new Point(703, 622), Color.FromArgb(32, 30, 30), 5))
                        {
                            Variables.ScriptLog("超上级还没被解锁！自动往下挑战中！", Color.Red);
                            if (BotCore.RGBComparer(image, new Point(515, 625), Color.FromArgb(12, 11, 12), 5))
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
                        }
                        BotCore.SendTap(741, 623);
                        break;
                    }
                    BotCore.SendTap(921, 620);
                    break;
            }
            BotCore.Delay(3000,false);
            do
            {
                if (!BotCore.GameIsForeground("com.nubee.valkyriecrusade"))
                {
                    return;
                }
                if (!PrivateVariable.Run)
                {
                    return;
                }
                if (PrivateVariable.Use_Item && energy == 0 && runes == 5)
                {
                    if(BotCore.GetPixel(new Point(798,313),image) != Color.FromArgb(27, 95, 22))
                    {
                        BotCore.Delay(1000,false);
                        continue;
                    }
                    BotCore.SendTap(798, 313);
                    Point? p  = BotCore.FindImage(image, Img.GreenButton,false);
                    while(p == null)
                    {
                        if (!BotCore.GameIsForeground("com.nubee.valkyriecrusade"))
                        {
                            return;
                        }
                        BotCore.Delay(400,600);
                        p = BotCore.FindImage(image, Img.GreenButton, false);
                    }
                    BotCore.SendTap(p.Value);
                    energy = 5;
                    BotCore.Delay(5000,false);
                }
                if (BotCore.RGBComparer(image, new Point(959, 656), 31, 102, 26, 4))
                {
                    Variables.ScriptLog("Start battle",Color.Lime);
                    BotCore.SendTap(new Point(959, 656));
                    BotCore.Delay(7000,false);
                    BotCore.SendTap(640, 400); //Tap away Round Battle Text
                    BotCore.Delay(2000,false);
                    stop.Start();
                    PrivateVariable.Battling = true;
                    energy--; //Calculate Energy used
                    BotCore.Delay(1000,false);
                    break;
                }
                else
                {
                    var crop = BotCore.CropImage(image, new Point(125, 600), new Point(1270, 10));
                    point = BotCore.FindImage(crop, Img.Red_Button, false);
                    if (point != null)
                    {
                        Variables.ScriptLog("Rune boss found!",Color.Yellow);
                        BotCore.SendTap(new Point(point.Value.X + 125, point.Value.Y));
                        RuneBoss = true;
                        BotCore.Delay(9000,12000);
                    }
                    else
                    {
                        ScriptErrorHandler.ErrorHandle();
                    }
                }
            }
            while (!PrivateVariable.Battling);

        }
        //Demon Event
        private static void Demon_Realm()
        {
            Debug_.WriteLine();
            Point? point = BotCore.FindImage(image, Img.Close2, false);
            if (point != null)
            {
                BotCore.SendTap(new Point(point.Value.X, point.Value.Y));
                BotCore.Delay(1000,false);
            }
            if (!PrivateVariable.Run)
            {
                return;
            }
            point = null;
            int error = 0;
            while (point == null)
            {
                if (!BotCore.GameIsForeground("com.nubee.valkyriecrusade"))
                {
                    return;
                }
                if (!PrivateVariable.Run)
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
                point = BotCore.FindImage(image, Img.HellLoc, true);
                Variables.ScriptLog("Locating Demon Realm Event UI!",Color.White);
                if (point != null)
                {
                    Tower_Floor = OCR.OcrImage(BotCore.CropImage(image, new Point(300, 115), new Point(484, 142)), "eng");
                    Tower_Rank = OCR.OcrImage(BotCore.CropImage(image, new Point(300, 150), new Point(458, 170)), "eng");
                    Variables.ScriptLog("Demon Realm Event Found!",Color.Lime);
                    PrivateVariable.InEventScreen = true;
                    energy = GetEnergy();
                    runes = GetRune();
                }
                else
                {
                    BotCore.Delay(1000,false);
                    error++;
                    if(error > 20)
                    {
                        ScriptErrorHandler.Reset("Unable to locate event. Going back to main screen");
                        return;
                    }
                }
            }
            
            if (energy == 0)
            {
                Variables.ScriptLog("Waiting for energy",Color.Yellow);
                Variables.ScriptLog("Close game and wait for energy because of no energy left",Color.Yellow);
                if (!PrivateVariable.Run)
                {
                    return;
                }
                NoEnergy();
                PrivateVariable.InEventScreen = false;
                PrivateVariable.InMainScreen = false;
                PrivateVariable.Battling = false;
                return;
            }
            if(runes == 3 && energy < 4 && Stuck)
            {
                StuckRune();
                return;
            }
            PrivateVariable.InEventScreen = true;
            Variables.ScriptLog("Enterting Stage",Color.White);
            switch (MainScreen.Level)
            {
                case 0:
                    BotCore.SendTap(250, 284);
                    break;
                case 1:
                    if(BotCore.RGBComparer(image,new Point(143, 355), Color.FromArgb(41,16, 4), 5))
                    {
                        Variables.ScriptLog("中级还没被解锁！自动往下挑战中！",Color.Red);
                        BotCore.SendTap(250, 284);
                        break;
                    }
                    BotCore.SendTap(362, 283);
                    break;
                case 2:
                    if (BotCore.RGBComparer(image, new Point(143, 355), Color.FromArgb(41, 16, 4), 5))
                    {
                        Variables.ScriptLog("上级还没被解锁！自动往下挑战中！", Color.Red);
                        if (BotCore.RGBComparer(image, new Point(324, 270), Color.FromArgb(41, 16, 4), 5))
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
                    if (BotCore.RGBComparer(image, new Point(324, 355), Color.FromArgb(41, 16, 4), 5))
                    {
                        Variables.ScriptLog("超上级还没被解锁！自动往下挑战中！", Color.Red);
                        if (BotCore.RGBComparer(image, new Point(143, 355), Color.FromArgb(41, 16, 4), 5))
                        {
                            Variables.ScriptLog("上级还没被解锁！自动往下挑战中！", Color.Red);
                            if (BotCore.RGBComparer(image, new Point(324, 270), Color.FromArgb(41, 16, 4), 5))
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
                    if (BotCore.RGBComparer(image, new Point(324, 355), Color.FromArgb(41, 16, 4), 5))
                    {
                        Variables.ScriptLog("超上级还没被解锁！自动往下挑战中！", Color.Red);
                        if (BotCore.RGBComparer(image, new Point(143, 355), Color.FromArgb(41, 16, 4), 5))
                        {
                            Variables.ScriptLog("上级还没被解锁！自动往下挑战中！", Color.Red);
                            if (BotCore.RGBComparer(image, new Point(324, 270), Color.FromArgb(41, 16, 4), 5))
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
            do
            {
                if (!PrivateVariable.Run)
                {
                    return;
                }
                if (BotCore.RGBComparer(image, new Point(959, 656), 31, 102, 26, 4))
                {
                    Variables.ScriptLog("Start battle",Color.Lime);
                    BotCore.SendTap(new Point(959, 656));
                    BotCore.Delay(2000,false);
                    BotCore.SendTap(new Point(758, 566));
                    BotCore.Delay(6000,8000);
                    BotCore.SendTap(640, 400); //Tap away Round Battle Text
                    BotCore.Delay(2000,false);
                    stop.Start();
                    energy--; //Calculate Energy used
                    EnteredStage = true;
                    BotCore.Delay(5000,false);
                    break;
                }
                else
                {
                    BotCore.Delay(200,1000);
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
                BotCore.Delay(1000,false);
            }
            error = 0;
            Variables.ScriptLog("Demon Realm Event Located",Color.Lime);
            List<Point> BlackListedLocation = new List<Point>();
            Point? p = null;
            while (error < 20 && p == null)
            {
                if (!BotCore.GameIsForeground("com.nubee.valkyriecrusade"))
                {
                    return;
                }
                if (!PrivateVariable.Run)
                {
                    return;
                }
                Variables.ScriptLog("Fetching stage images",Color.White);
                List<Image> Stage = new List<Image>();
                foreach(var file in Directory.GetFiles("Img\\DemonRealm","*.png").OrderBy(f => f))
                {
                    Stage.Add(Image.FromFile(file));
                }
                var crop = BotCore.CropImage(image, new Point(0, 0), new Point(1280, 615));
                Variables.ScriptLog("Trying to find stages to enter",Color.LightSkyBlue);
                Bitmap screen = (Bitmap)BotCore.Decompress(image);
                foreach (var stage in Stage)
                {
                    p = BotCore.FindImage(screen, (Bitmap)stage, false);
                    if (p != null)
                    {
                        if (!BlackListedLocation.Contains(p.Value))
                        {
                            Variables.ScriptLog("Stage found!", Color.Lime);
                            BotCore.SendTap(p.Value);
                            BotCore.Delay(2000,false);
                            BotCore.SendTap(768, 536);
                            BotCore.Delay(5000,false);
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
                            BotCore.Delay(2000,false);
                            BotCore.SendTap(753, 423);
                            break;
                        }
                    }
                }
                Point? p2 = BotCore.FindImage(image, Img.GreenButton, false);
                if (p2 != null)
                {
                    BotCore.SendTap(p2.Value);
                    BotCore.Delay(1000,false);
                    continue;
                }
                error++;
            }
            if(error > 18)
            {
                Variables.ScriptLog("Looks like we are in the trouble!",Color.Red);
                error = 0;
                BotCore.KillGame("com.nubee.valkyriecrusade");
                ScriptErrorHandler.Reset("Restarting game as unable to detect stages properly!");
            }
            BotCore.Delay(5000,false);
            Point? point = BotCore.FindImage(image, Img.Red_Button, false);
            for(int x = 0; x < 20; x++)
            {
                Point? p2 = BotCore.FindImage(image, Img.GreenButton, false);
                if (p2 != null)
                {
                    BotCore.SendTap(p2.Value);
                    BotCore.Delay(1000,false);
                    continue;
                }
                if (!BotCore.GameIsForeground("com.nubee.valkyriecrusade"))
                {
                    return;
                }
                point = BotCore.FindImage(image, Img.Red_Button, false);
                BotCore.Delay(1000,false);
            }
            if(point == null)
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
            if (!BotCore.RGBComparer(image, new Point(10, 27), Color.FromArgb(200, 200, 200), 5))
            {
                Debug_.WriteLine();
                Variables.ScriptLog("HP bar not found. Finding UIs",Color.Yellow);
                ScriptErrorHandler.ErrorHandle();
                Point? point = BotCore.FindImage(image, Img.Close2, false);
                if (point != null)
                {
                    BotCore.SendTap(point.Value);
                    BotCore.Delay(400,600);
                    locateUIError = 0;
                }
                if (BotCore.FindImage(image, Img.NoEnergy, true) != null)
                {
                    ScriptErrorHandler.Reset("No Energy Left!");
                    NoEnergy();
                    Attackable = false;
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
                    Variables.ScriptLog("Battle Ended!",Color.Lime);
                    stop.Stop();
                    Variables.ScriptLog("Battle used up " + stop.Elapsed,Color.Lime);
                    stop.Reset();
                    BotCore.SendTap(1076, 106);
                    Attackable = false;
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
                    Variables.ScriptLog("Battle Ended!",Color.Lime);
                    stop.Stop();
                    Variables.ScriptLog("Battle used up " + stop.Elapsed,Color.Lime);
                    stop.Reset();
                    Attackable = false;
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
                    Variables.ScriptLog("Battle Ended!",Color.Lime);
                    stop.Stop();
                    Variables.ScriptLog("Battle used up " + stop.Elapsed, Color.Lime);
                    stop.Reset();
                    Attackable = false;
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
                        if (BotCore.FindImage(image, Img.TowerFinished, true) != null && RuneBoss && runes >= 3 && runes != 5)
                        {
                            PrivateVariable.InEventScreen = false;
                            PrivateVariable.InMainScreen = false;
                            PrivateVariable.Battling = false;
                            Stuck = true;
                            Variables.ScriptLog("Battle Ended!", Color.Lime);
                            stop.Stop();
                            Variables.ScriptLog("Battle used up " + stop.Elapsed, Color.Lime);
                            stop.Reset();
                            Attackable = false;
                            locateUIError = 0;
                            return;
                        }
                        else
                        {
                            if (BotCore.FindImage(image, Img.Locate_Tower, true) != null)
                            {
                                PrivateVariable.InEventScreen = false;
                                PrivateVariable.InMainScreen = false;
                                PrivateVariable.Battling = false;
                                Variables.ScriptLog("Battle Ended!", Color.Lime);
                                stop.Stop();
                                Variables.ScriptLog("Battle used up " + stop.Elapsed, Color.Lime);
                                stop.Reset();
                                Attackable = false;
                                locateUIError = 0;
                            }
                            else
                            {
                                BotCore.SendTap(point.Value.X + 125, point.Value.Y);
                                Attackable = false;
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
                            PrivateVariable.InEventScreen = false;
                            PrivateVariable.InMainScreen = false;
                            PrivateVariable.Battling = false;
                            BotCore.SendTap(point.Value.X + 125, point.Value.Y);
                            Variables.ScriptLog("Battle Ended!", Color.Lime);
                            stop.Stop();
                            Variables.ScriptLog("Battle used up " + stop.Elapsed, Color.Lime);
                            stop.Reset();
                            Attackable = false;
                            locateUIError = 0;
                            return;
                        }
                        else
                        {
                            BotCore.SendTap(point.Value.X + 125, point.Value.Y);
                            Attackable = false;
                            locateUIError = 0;
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
                                BotCore.SendTap(0, 0);
                            }
                            PrivateVariable.Battling = false;
                            PrivateVariable.InEventScreen = true;
                            stop.Stop();
                            Variables.ScriptLog("Battle used up " + stop.Elapsed, Color.Lime);
                            stop.Reset();
                            Attackable = false;
                            locateUIError = 0;
                            return;
                        }
                        else
                        {
                            BotCore.SendTap(point.Value.X + 125, point.Value.Y);
                            Attackable = false;
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
                            Attackable = false;
                            locateUIError = 0;
                            return;
                        }
                    }
                    Variables.ScriptLog("Starting Battle", Color.Lime);
                    BotCore.SendTap(point.Value.X + 125, point.Value.Y);
                    PrivateVariable.Battling = true;
                    BotCore.Delay(900,1000);
                    crop = BotCore.CropImage(image, new Point(682, 544), new Point(905, 589));
                    if (BotCore.RGBComparer(crop, Color.FromArgb(29, 98, 24)))
                    {
                        BotCore.SendTap(793, 565);
                        BotCore.Delay(1000,false);
                    }
                    Attackable = false;
                    locateUIError = 0;
                    return;
                }
                if (!PrivateVariable.Battling)
                {
                    return;
                }
                point = BotCore.FindImage(image, Img.GarbageMessage, true);
                if (point != null)
                {
                    BotCore.SendTap(point.Value);
                    Attackable = false;
                    locateUIError = 0;
                    return;
                }
                if (!PrivateVariable.Battling)
                {
                    return;
                }
                point = BotCore.FindImage(image, Img.Love, true);
                if (point != null)
                {
                    for (int x = 0; x < 10; x++)
                    {
                        BotCore.SendTap(point.Value);
                        BotCore.Delay(100,200);
                        locateUIError = 0;
                    }
                    Attackable = false;
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
                    Attackable = false;
                    locateUIError = 0;
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
                var enemy = BotCore.CropImage(image, new Point(582, 258), new Point(715, 308));
                if (BotCore.RGBComparer(enemy, Color.FromArgb(33, 106, 159)) || BotCore.RGBComparer(enemy, Color.FromArgb(171, 0, 21)))
                {
                    BotCore.SendTap(640, 156); //Boss在中间，打Boss
                    Variables.ScriptLog("Found Boss at center!", Color.LightPink);
                }
                else
                {
                    if (!PrivateVariable.Battling)
                    {
                        return;
                    }
                    if (PrivateVariable.Battling == true && PrivateVariable.EventType == 2)
                    {
                        var point = BotCore.FindImage(Script.image, "Img\\HellLoc.png", false);
                        if (point != null)
                        {
                            PrivateVariable.Battling = false;
                            Variables.ScriptLog("Battle Ended!", Color.Lime);
                            return;
                        }
                        BotCore.Delay(100, 500);
                    }
                    //找不到Boss血量条，可能剩下一丝血，所以先打中间试试水，再打小怪
                    BotCore.SendTap(640,156);
                    BotCore.Delay(100,500);
                    BotCore.SendTap(462, 176);
                    BotCore.Delay(100, 500);
                    BotCore.SendTap(820,187);
                    BotCore.Delay(100, 500);
                    BotCore.SendTap(330, 202);
                    BotCore.Delay(100, 500);
                    BotCore.SendTap(952, 193);
                    Variables.ScriptLog("Boss not found, trying to hit others", Color.LightPink);
                }
            }
            Attackable = true;
        }
        //Click on enemy
        private static void Battle()
        {
            do
            {
                Attackable = true;
                Debug_.WriteLine();
                if (!PrivateVariable.Battling)
                {
                    return;
                }
                for(int x = 0; x < 10; x++)
                {
                    BotCore.SendTap(1, 1);
                    BotCore.Delay(100, 500);
                }
                LocateUI();
                if (Attackable)
                {
                    Variables.ScriptLog("Locating Skills and enemies",Color.Gold);
                    if (PrivateVariable.BattleScript.Count > 0)
                    {
                        PrivateVariable.BattleScript[PrivateVariable.Selected_Script].Attack();
                    }
                    CheckEnemy();
                    挂机框架.挂机核心.延时(2000,true);
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
            if (PrivateVariable.EventType == 0)
            {
                if (!PrivateVariable.Run)
                {
                    return 0;
                }
                int num = 0;
                Color energy = Color.FromArgb(50, 233, 34);
                if (BotCore.RGBComparer(Script.image, new Point(417, 535), energy, 10))
                {
                    num++;
                }
                if (!PrivateVariable.Run)
                {
                    return 0;
                }
                if (BotCore.RGBComparer(Script.image, new Point(481, 535), energy, 10))
                {
                    num++;
                }
                if (!PrivateVariable.Run)
                {
                    return 0;
                }
                if (BotCore.RGBComparer(Script.image, new Point(546, 535), energy, 10))
                {
                    num++;
                }
                if (!PrivateVariable.Run)
                {
                    return 0;
                }
                if (BotCore.RGBComparer(Script.image, new Point(613, 535), energy, 10))
                {
                    num++;
                }
                if (!PrivateVariable.Run)
                {
                    return 0;
                }
                if (BotCore.RGBComparer(Script.image, new Point(677, 535), energy, 10))
                {
                    num++;
                }
                return num;
            }
            else
            {
                if (!PrivateVariable.Run)
                {
                    return 0;
                }
                int num = 0;
                Color energy = Color.FromArgb(104, 45, 22);
                if (BotCore.RGBComparer(image, new Point(208, 445), energy, 10))
                {
                    num++;
                }
                if (!PrivateVariable.Run)
                {
                    return 0;
                }
                if (BotCore.RGBComparer(image, new Point(253, 441), energy, 10))
                {
                    num++;
                }
                if (!PrivateVariable.Run)
                {
                    return 0;
                }
                if (BotCore.RGBComparer(image, new Point(315, 445), energy, 10))
                {
                    num++;
                }
                if (!PrivateVariable.Run)
                {
                    return 0;
                }
                if (BotCore.RGBComparer(image, new Point(351, 449), energy, 10))
                {
                    num++;
                }
                if (!PrivateVariable.Run)
                {
                    return 0;
                }
                if (!BotCore.RGBComparer(image, new Point(410, 458), Color.FromArgb(27,24,29), 10))
                {
                    num++;
                }
                return num;
            }
        } 
        //Get runes
        private static int GetRune()
        {
            Debug_.WriteLine();
            if (PrivateVariable.EventType == 0)
            {
                if (!PrivateVariable.Run)
                {
                    return 0;
                }
                int num = 5;
                if (BotCore.RGBComparer(Script.image, new Point(945, 207), 118, 117, 118, 10))
                {
                    num--;
                }
                if (!PrivateVariable.Run)
                {
                    return 0;
                }
                if (BotCore.RGBComparer(Script.image, new Point(979, 308), 114, 114, 114, 10))
                {
                    num--;
                }
                if (!PrivateVariable.Run)
                {
                    return 0;
                }
                if (BotCore.RGBComparer(Script.image, new Point(1088, 309), 118, 117, 118, 10))
                {
                    num--;
                }
                if (!PrivateVariable.Run)
                {
                    return 0;
                }
                if (BotCore.RGBComparer(Script.image, new Point(1121, 204), 113, 113, 113, 10))
                {
                    num--;
                }
                if (!PrivateVariable.Run)
                {
                    return 0;
                }
                if (BotCore.RGBComparer(Script.image, new Point(1033, 140), 116, 115, 115, 10))
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
            ScriptErrorHandler.PauseErrorHandler = true;
            if (PrivateVariable.EventType == 0)
            {
                Debug_.WriteLine();
                int el = 5 - energy;
                int wait = el * 2600000;
                Variables.ScriptLog("Close game and stuck rune!",Color.DarkGreen);
                nextOnline = DateTime.Now.AddMilliseconds(wait);
                Variables.ScriptLog("Estimate online time is " + nextOnline, Color.Lime);
                BotCore.KillGame("com.nubee.valkyriecrusade");
                if (!PrivateVariable.EnterRune)
                {
                    PrivateVariable.Run = false;
                    if (Directory.Exists(Environment.CurrentDirectory + "\\Audio\\"))
                    {
                        string[] path = Directory.GetFiles(Environment.CurrentDirectory + "\\Audio\\", "*.wav");
                        if (path.Length > 0)
                        {
                            SoundPlayer player = new SoundPlayer();
                            player.SoundLocation = path[0];
                            player.PlayLooping();
                        }
                    }
                    BotCore.CloseEmulator();
                    MessageBox.Show("正在卡符文！下次上线时间为" + nextOnline + "!");
                    Environment.Exit(0);
                }
                if (PrivateVariable.CloseEmulator)
                {
                    BotCore.CloseEmulator();
                }
                BotCore.Delay(wait - 70000,wait - 50000);
            }
            else
            {
                Debug_.WriteLine();
                int el = 5 - energy;
                int wait = el * 2600000;
                Variables.ScriptLog("Close game and stuck treasure map!", Color.DarkGreen);
                nextOnline = DateTime.Now.AddMilliseconds(wait);
                Variables.ScriptLog("Estimate online time is " + nextOnline, Color.Lime);
                BotCore.KillGame("com.nubee.valkyriecrusade");
                if (!PrivateVariable.EnterRune)
                {
                    PrivateVariable.Run = false;
                    if (Directory.Exists(Environment.CurrentDirectory + "\\Audio\\"))
                    {
                        string[] path = Directory.GetFiles(Environment.CurrentDirectory + "\\Audio\\", "*.wav");
                        if (path.Length > 0)
                        {
                            SoundPlayer player = new SoundPlayer();
                            player.SoundLocation = path[0];
                            player.PlayLooping();
                        }
                    }
                    BotCore.CloseEmulator();
                    MessageBox.Show("已存3宝藏图碎片！下次上线时间为" + nextOnline + "!");
                    Environment.Exit(0);
                }
                if (PrivateVariable.CloseEmulator)
                {
                    BotCore.CloseEmulator();
                }
                BotCore.Delay(wait - 70000,wait - 50000);
            }
            ScriptErrorHandler.PauseErrorHandler = false;
        }
        //No energy left so close game
        private static void NoEnergy()
        {
            Debug_.WriteLine();
            if (PrivateVariable.EventType == 0 || PrivateVariable.EventType == 2)
            {
                int el = 5 - energy;
                int wait = el * 2500000;
                nextOnline = DateTime.Now.AddMilliseconds(wait);
                Variables.ScriptLog("Estimate online time is " + nextOnline, Color.Lime);
                BotCore.KillGame("com.nubee.valkyriecrusade");
                ScriptErrorHandler.PauseErrorHandler = true;
                if (PrivateVariable.CloseEmulator)
                {
                    CloseEmu = true;
                    BotCore.CloseEmulator();
                }
                BotCore.Delay(wait - 70000, wait - 50000);
                ScriptErrorHandler.PauseErrorHandler = false;
                CloseEmu = false;
            }
            else if(PrivateVariable.EventType == 1)
            {
                ScriptErrorHandler.PauseErrorHandler = true;
                BotCore.KillGame("com.nubee.valkyriecrusade");
                nextOnline = DateTime.Now.AddMilliseconds(900000);
                Variables.ScriptLog("Estimate online time is " + nextOnline, Color.Lime);
                BotCore.Delay(900000, 1000000);
                ScriptErrorHandler.PauseErrorHandler = false;
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
            string[] files = Directory.GetFiles("Battle_Script","*.dll");
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
                foreach(var s in PrivateVariable.BattleScript)
                {
                    s.ReadConfig();
                }
            }
        }
    }
}
