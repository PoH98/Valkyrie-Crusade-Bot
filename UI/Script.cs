using System.Drawing;
using System.IO;
using System.Threading;
using System;
using ImageProcessor;
using System.Diagnostics;
using ImgXml;
using System.Windows.Forms;
using System.Media;
using System.Reflection;
using System.Linq;
using System.Net;
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
            pushed = false;
            Debug_.WriteLine();
            Thread errorhandler = new Thread(ScriptErrorHandler.ErrorHandle);
            errorhandler.Start();
            while (PrivateVariable.Run)
            {
                if (!CloseEmu)
                {
                    Thread.Sleep(10);
                    if (Variables.Controlled_Device != null) //The Emulator is running
                    {
                        while (Variables.Proc == null)//But not registred on our Proc value
                        {
                            Debug_.WriteLine("Variables.Proc is null");
                            //so go on and find the emulator!
                            EmulatorController.ConnectAndroidEmulator(string.Empty, string.Empty, MainScreen.MEmu);
                            //MEmu found!
                            if (Variables.Proc != null)
                            {
                                break;
                            }
                            //Maybe something is wrong, no process is same name as MEmu!
                            EmulatorController.StartEmulator();
                        }
                    }
                    else //The Emulator is not exist!
                    {
                        EmulatorController.StartEmulator(); //Start our fxxking Emulator!!
                        Thread.Sleep(10000); //Wait
                        continue; //Back to start of the loop
                    }
                    if (!EmulatorController.StartAdb())
                    {
                        MessageBox.Show("Unable to start adb!");
                        Environment.Exit(0);
                    }
                    int error = 0;
                    Thread.Sleep(10);
                    while (image == null) //Weird problem happens, we still cannot receive any image capture!
                    {
                        if (!PrivateVariable.Run)
                        {
                            return;
                        }
                        Thread.Sleep(1000); //Wait forever?
                        Variables.ScriptLog.Add("Waiting for first tons of image buffer");
                        error++;
                        if (error > 30) //Nah, we only wait for 30 sec
                        {
                            MessageBox.Show("无法截图！出现怪异错误！");
                            Environment.Exit(0);
                        }
                    }
                    Thread.Sleep(10);
                    if (Variables.Instance.Length < 5)
                    {
                        Variables.Instance = "MEmu";
                    }
                    string filename = EmulatorController.SHA256(Variables.AdbIpPort);
                    if (!Directory.Exists("C:\\ProgramData\\" + filename))
                    {
                        Directory.CreateDirectory("C:\\ProgramData\\" + filename);
                    }

                    if (!File.Exists("C:\\ProgramData\\" + filename+"\\" +filename+".xml"))
                    {
                        if (!EmulatorController.Pull("/data/data/com.nubee.valkyriecrusade/shared_prefs/NUBEE_ID.xml", "C:\\ProgramData\\" + filename + "\\" + filename + ".xml"))
                        {
                            Variables.ScriptLog.Add("Pull files failed");
                        }
                        else
                        {
                            Variables.ScriptLog.Add("Backup saved");
                        }
                    }
                    else
                    {
                        if (!pushed)
                        {
                            EmulatorController.Push("C:\\ProgramData\\" + filename + "\\" + filename + ".xml", "/data/data/com.nubee.valkyriecrusade/shared_prefs/NUBEE_ID.xml", 660);
                            pushed = true;
                            Variables.ScriptLog.Add("Restored backup xml");
                            Thread.Sleep(1000);
                        }
                    }
                    Image img = EmulatorController.Decompress(Script.image);
                    if (img.Height != 720 || img.Width != 1280)
                    {
                        Debug_.WriteLine("Image size not correct: " + img.Width + "*" + img.Height);
                        if (!PrivateVariable.Run)
                        {
                            return;
                        }
                        if (Variables.Proc != null)
                        {
                            Variables.ScriptLog.Add("Emulator's screen size is not 1280*720! Detected size is " + img.Width + "*" + img.Height);
                            Variables.Proc.Kill();
                            Variables.Proc = null;
                        }
                        ProcessStartInfo server = new ProcessStartInfo();
                        string path = "";
                        Variables.Configure.TryGetValue("Path", out path);
                        path = path.Replace("MEmu\\MEmu.exe", "MEmuHyperv\\cmd.bat");
                        string text = "MEmuManage.exe guestproperty set MEmu resolution_height 720\nMEmuManage.exe guestproperty set MEmu resolution_width 1280";
                        File.WriteAllText(path, text);
                        server.FileName = path;
                        server.UseShellExecute = true;
                        server.WorkingDirectory = path.Replace("\\cmd.bat", "");
                        server.CreateNoWindow = true;
                        server.WindowStyle = ProcessWindowStyle.Hidden;
                        Process p = Process.Start(server);
                        while (!p.HasExited)
                        {
                            Thread.Sleep(200);
                        }
                        Variables.ScriptLog.Add("Restarting Emulator after setting size");
                        EmulatorController.StartEmulator();
                        Thread.Sleep(30000);
                        continue;
                    }
                }
                if (Stuck)
                {
                    StuckRune();
                    Stuck = false;
                    continue;
                }
                Thread.Sleep(10);
                if (!EmulatorController.GameIsForeground("com.nubee.valkyriecrusade"))
                {
                    Variables.ScriptLog.Add("Starting Game");
                    EmulatorController.StartGame(Img.Icon, image);
                    Thread.Sleep(1000);
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
                            Thread.Sleep(5000);
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
                                        Variables.ScriptLog.Add("Unknown error occur, unable to detect event type.");
                                        return;
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
            Thread.Sleep(1000);
            PrivateVariable.InMainScreen = false;
            Point? point = null;
            if (!PrivateVariable.Run)
            {
                return;
            }
            Thread.Sleep(100);
            point = EmulatorController.FindImage(image, Img.Locate, true);
            if (point == null)
            {
                if (!PrivateVariable.Run)
                {
                    return;
                }
                Variables.ScriptLog.Add("Main Screen not visible");
                point = EmulatorController.FindImage(image, Img.Start_Game, true);
                if (point != null)
                {
                    Variables.ScriptLog.Add("Start Game Button Located!");
                    EmulatorController.SendTap(point.Value);
                    return;
                }
                if (!PrivateVariable.Run)
                {
                    return;
                }
                Thread.Sleep(100);
                point = EmulatorController.FindImage(image, Img.Update_Complete, true);
                if (point != null)
                {
                    EmulatorController.SendTap(point.Value);
                    return;
                }
                if (!PrivateVariable.Run)
                {
                    return;
                }
                Thread.Sleep(100);
                point = EmulatorController.FindImage(image, Img.Close2, true);
                if (point != null)
                {
                    EmulatorController.SendTap(point.Value);
                    return;
                }
                if (!PrivateVariable.Run)
                {
                    return;
                }
                point = EmulatorController.FindImage(image, Img.Close, true);
                if (point != null)
                {
                    EmulatorController.SendTap(point.Value);
                    return;
                }
                if (!PrivateVariable.Run)
                {
                    return;
                }
                Thread.Sleep(100);
                point = EmulatorController.FindImage(image, Img.Login_Reward, true);
                if (point != null)
                {
                    for (int x = 0; x < 4; x++)
                    {
                        EmulatorController.SendTap(new Point(600, 350));
                        Thread.Sleep(1000);
                    }
                    return;
                }
                if (!PrivateVariable.Run)
                {
                    return;
                }
                Thread.Sleep(100);
                point = EmulatorController.FindImage(image, Img.Back_to_Village, true);
                if (point != null)
                {
                    Variables.ScriptLog.Add("Going back to Main screen");
                    EmulatorController.SendTap(point.Value);
                    PrivateVariable.InMainScreen = true;
                    Variables.ScriptLog.Add("Screen Located");
                }
                if (!PrivateVariable.Run)
                {
                    return;
                }
                Thread.Sleep(100);
                point = EmulatorController.FindImage(image, Img.Menu, true);
                if (point == null)
                {
                    if (error < 30)
                    {
                        if(error == 0)
                        {
                            Variables.ScriptLog.Add("Waiting for Main screen");
                        }
                        Thread.Sleep(1000);
                        error++;
                    }
                    else
                    {
                        EmulatorController.KillGame("com.nubee.valkyriecrusade");
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
                    EmulatorController.SendTap(point.Value);
                    Thread.Sleep(1000);
                    Variables.ScriptLog.Add("Returning main screen");
                    EmulatorController.SendTap(942, 630);
                    Thread.Sleep(5000);
                }
                Thread.Sleep(100);
                if (!PrivateVariable.Run)
                {
                    return;
                }
                point = EmulatorController.FindImage(image, Img.GreenButton, true);
                if (point != null)
                {
                    EmulatorController.SendTap(point.Value);
                    Variables.ScriptLog.Add("Green Button Found!");
                }
            }
            else
            {
                Retry++;
                if(Retry > 5)
                {
                    PrivateVariable.InMainScreen = true;
                    Variables.ScriptLog.Add("Screen Located");
                    //Collect();
                    Retry = 0;
                }
                else
                {
                    Thread.Sleep(1000);
                    if(Retry == 1)
                    {
                        Variables.ScriptLog.Add("Waiting for Login Bonus");
                    }
                }
            }

        }
        //Collect
        private static void Collect()
        {
            Variables.ScriptLog.Add("Collecting Resources");
            for(int x = 0; x < 4; x++)
            {
                switch (x)
                {
                    case 0:
                        EmulatorController.SendSwipe(new Point(925, 576), new Point(614, 26),1000);
                        break;
                    case 1:
                        EmulatorController.SendSwipe(new Point(231, 562), new Point(877, 127), 1000);
                        break;
                    case 2:
                        EmulatorController.SendSwipe(new Point(226, 175), new Point(997, 591), 1000);
                        break;
                    case 3:
                        EmulatorController.SendSwipe(new Point(969, 128), new Point(260, 545), 1000);
                        break;
                }
                var crop = EmulatorController.CropImage(image, new Point(0, 0), new Point(1020, 720));
                //Find image and collect
                foreach (var img in Directory.GetFiles("Img\\Resources\\", "*.png"))
                {
                    Point? p = EmulatorController.FindImage(crop, img, false);
                    if (p != null)
                    {
                        EmulatorController.SendTap(p.Value);
                        Thread.Sleep(100);
                    }
                }
                Thread.Sleep(500);

            }

        }
        //Treasure hunt!
        public static void TreasureHunt()
        {
            Debug_.WriteLine();
            Point? p = null;
            p = EmulatorController.FindImage(Script.image, Img.TreasureHunt, true);
            //Find for treasure hunt building!
            for (int find = 0; find < 5; find++)
            {
                if (!PrivateVariable.Run)
                {
                    return;
                }
                if (p == null)
                {
                    p = EmulatorController.FindImage(Script.image, Img.TreasureHunt2, true);
                    if (p == null)
                    {
                        switch (find)
                        {
                            case 0:
                                EmulatorController.SendSwipe(new Point(300, 500), new Point(1100, 50), 500);
                                break;
                            case 1:
                                EmulatorController.SendSwipe(new Point(1100, 600), new Point(100, 100), 500);
                                break;
                            case 2:
                                EmulatorController.SendSwipe(new Point(1100, 275), new Point(500, 500), 500);
                                break;
                            case 3:
                                EmulatorController.SendSwipe(new Point(400, 50), new Point(900, 600), 500);
                                break;
                            case 4:
                                EmulatorController.SendSwipe(new Point(200, 500), new Point(500, 500), 500);
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
                Thread.Sleep(1000);
            }
            if(p == null)
            {
                Variables.ScriptLog.Add("No Treasure Hunt Building found!");
                return;
            }
            Variables.ScriptLog.Add("Treasure hunting...");
            EmulatorController.SendTap(p.Value);
            Thread.Sleep(1000);
            //Enter Treasure hunt
            if (!PrivateVariable.Run)
            {
                return;
            }
            EmulatorController.SendTap(879, 642);
            Thread.Sleep(30000);
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
                if (EmulatorController.RGBComparer(Script.image,new Point(973, 344), Color.FromArgb(130, 0, 0),0))
                {
                    Variables.ScriptLog.Add("Already in hunting, exit now!");
                    //Exit loop
                    EmulatorController.SendTap(1222, 56);
                    Thread.Sleep(5000);
                    break;
                }
                else
                {
                    p = EmulatorController.FindImage(Script.image, Img.Red_Button, true);
                    if (p != null)
                    {
                        //Finished hunt, collect rewards
                        EmulatorController.SendTap(p.Value);
                        Thread.Sleep(5000);
                        EmulatorController.SendTap(960, 621);
                        Thread.Sleep(7000);
                        p = EmulatorController.FindImage(Script.image, Img.Map, true);
                        //if found treasure map
                        if (p != null)
                        {
                            //Just ignore that fxxking thing
                            EmulatorController.SendTap(789, 626);
                            Thread.Sleep(10000);
                            EmulatorController.SendTap(310, 137);
                        }
                    }
                }
                Thread.Sleep(10000);
                //Back to top
                EmulatorController.SendSwipe(new Point(600, 200), new Point(600, 600), 1000);
                Thread.Sleep(3000);
                //Tap and start another hunt
                EmulatorController.SendTap(998, 340);
                Thread.Sleep(1000);
                EmulatorController.SendTap(771, 453);
                //Next Treasure hunt
                EmulatorController.SendTap(1031, 50);
                Thread.Sleep(3000);
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
                        EmulatorController.SendTap(170, 630);
                        Thread.Sleep(5000);
                        for (int x = 0; x < 5; x++)
                        {
                            Point? located = EmulatorController.FindImage(image, Environment.CurrentDirectory + "\\Img\\LocateEventSwitch.png", true);
                            if (located == null)
                            {
                                x = x - 1;
                                Thread.Sleep(1000);
                                if (error > 10)
                                {
                                    ScriptErrorHandler.Reset("Unable to locate Event Switch screen! Returning main screen!");
                                    error = 0;
                                    return;
                                }
                                error++;
                                continue;
                            }
                            Variables.ScriptLog.Add("Finding Event.png on screen");
                            point = EmulatorController.FindImage(image, Environment.CurrentDirectory + "\\Img\\Event.png", true);
                            if (point !=null)
                            {
                                Variables.ScriptLog.Add("Image matched");
                                EmulatorController.SendTap(point.Value);
                                break;
                            }
                        }
                        if (point == null)
                        {
                            MessageBox.Show("Event.png可能有问题，请确保截图是正确的！");
                            if (EmulatorController.handle != null && Variables.Proc != null)
                            {
                                DllImport.SetParent(EmulatorController.handle, IntPtr.Zero);
                                DllImport.MoveWindow(EmulatorController.handle, PrivateVariable.EmuDefaultLocation.X, PrivateVariable.EmuDefaultLocation.Y, 1318,752, true);
                            }
                            Environment.Exit(0);
                        }

                    }
                    else
                    {
                        EmulatorController.SendTap(130, 350);
                    }
                }
                else
                {
                    EmulatorController.SendTap(130, 520);
                }
            }
            else
            {
                EmulatorController.SendTap(130, 520);
            }
            Thread.Sleep(10000);
            error = 0;
            do
            {
                if (!EmulatorController.GameIsForeground("com.nubee.valkyriecrusade"))
                {
                    return;
                }
                if (!PrivateVariable.Run)
                {
                    return;
                }
                byte[] crop = EmulatorController.CropImage(image, new Point(125, 0), new Point(900, 510));
                if (EmulatorController.FindImage(crop, Img.GreenButton, false) != null)
                {
                    EmulatorController.SendTap(point.Value);
                    return;
                }
                if (!PrivateVariable.Run)
                {
                    return;
                }
                if (EmulatorController.FindImage(image, Img.Locate_Tower, true) != null)
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
                if (EmulatorController.FindImage(image, Img.Archwitch_Rec, true) != null)
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
                if (EmulatorController.FindImage(image, Img.Demon_InEvent, true) != null)
                {
                    PrivateVariable.EventType = 2;
                    PrivateVariable.InEventScreen = true;
                    return;
                }
                if (!PrivateVariable.Run)
                {
                    return;
                }
                if (EmulatorController.FindImage(image, Img.HellLoc, true) != null)
                {
                    PrivateVariable.EventType = 2;
                    PrivateVariable.InEventScreen = true;
                    return;
                }
                if (!PrivateVariable.Run)
                {
                    return;
                }
                if (EmulatorController.FindImage(image, Img.Locate, true) != null)
                {
                    Variables.ScriptLog.Add("Rare error happens, still in main screen!");
                    PrivateVariable.InMainScreen = false;
                    return;
                }
                if (!PrivateVariable.Run)
                {
                    return;
                }
                if (EmulatorController.FindImage(image, Img.Red_Button, false) != null)
                {
                    Variables.ScriptLog.Add("Battle Screen found. Starting battle!");
                    PrivateVariable.Battling = true;
                    PrivateVariable.InEventScreen = true;
                    return;
                }
                if (error > 30)
                {
                    EmulatorController.KillGame("com.nubee.valkyriecrusade");
                    ScriptErrorHandler.Reset("Critical error found! Trying to restart game!");
                    error = 0;
                    return;
                }
                error++;
            }
            while (point == null);
        }
        //Tower Event
        private static void Tower()
        {
            Debug_.WriteLine();
            Thread.Sleep(1000);
            RuneBoss = false;
            Point? point = null;
            //Nope, we are in the tower event main screen! So go on!
            point = EmulatorController.FindImage(image, Img.Close2, false);
            if (point != null)
            {
                EmulatorController.SendTap(new Point(point.Value.X , point.Value.Y));
                Thread.Sleep(1000);
            }
            if (!PrivateVariable.Run)
            {
                return;
            }
            Variables.ScriptLog.Add("Locating Tower Event UI!");
            if (EmulatorController.FindImage(image, Img.Locate_Tower, true) != null)
            {
                Tower_Floor = OCR.OcrImage(EmulatorController.CropImage(image, new Point(280, 110), new Point(440, 145)),"eng");
                Tower_Rank = OCR.OcrImage(EmulatorController.CropImage(image, new Point(280, 140), new Point(410, 170)), "eng");
                Variables.ScriptLog.Add("Tower Event Found!");
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
            Variables.ScriptLog.Add("Current have " + energy + " energy and " + runes + " runes");
            if (!PrivateVariable.Run)
            {
                return;
            }
            if (energy == 0)
            {
                Variables.ScriptLog.Add("Waiting for energy");
                if (PrivateVariable.TakePartInNormalStage)
                {
                    EmulatorController.SendTap(1218, 662);
                    Thread.Sleep(500);
                    EmulatorController.SendTap(744, 622);
                }
                else
                {
                    if (PrivateVariable.Use_Item)
                    {
                        if(runes == 5)
                        {
                            Variables.ScriptLog.Add("Use item as it is now rune!");
                        }
                        else
                        {
                            Variables.ScriptLog.Add("Close game and wait for energy because of no energy left");
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
                        Variables.ScriptLog.Add("Close game and wait for energy because of no energy left");
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
            Variables.ScriptLog.Add("Entering Stage!");
            switch (MainScreen.Level)
            {
                case 0:
                    EmulatorController.SendTap(196, 648);
                    break;
                case 1:
                    EmulatorController.SendTap(391, 648);
                    break;
                case 2:
                    EmulatorController.SendTap(581, 646);
                    break;
                case 3:
                    EmulatorController.SendTap(741, 623);
                    break;
                case 4:
                    EmulatorController.SendTap(921, 620);
                    break;
            }
            Thread.Sleep(3000);
            do
            {
                if (!EmulatorController.GameIsForeground("com.nubee.valkyriecrusade"))
                {
                    return;
                }
                if (!PrivateVariable.Run)
                {
                    return;
                }
                if (PrivateVariable.Use_Item && energy == 0 && runes ==5)
                {
                    if(EmulatorController.GetPixel(new Point(798,313),image) != Color.FromArgb(27, 95, 22))
                    {
                        Thread.Sleep(1000);
                        continue;
                    }
                    EmulatorController.SendTap(798, 313);
                    Point? p  = EmulatorController.FindImage(image, Img.GreenButton,false);
                    while(p == null)
                    {
                        if (!EmulatorController.GameIsForeground("com.nubee.valkyriecrusade"))
                        {
                            return;
                        }
                        Thread.Sleep(500);
                        p = EmulatorController.FindImage(image, Img.GreenButton, false);
                    }
                    EmulatorController.SendTap(p.Value);
                    energy = 5;
                    Thread.Sleep(5000);
                }
                if (EmulatorController.RGBComparer(image, new Point(959, 656), 31, 102, 26, 4))
                {
                    Variables.ScriptLog.Add("Start battle");
                    EmulatorController.SendTap(new Point(959, 656));
                    Thread.Sleep(7000);
                    EmulatorController.SendTap(640, 400); //Tap away Round Battle Text
                    Thread.Sleep(2000);
                    stop.Start();
                    PrivateVariable.Battling = true;
                    energy--; //Calculate Energy used
                    Thread.Sleep(1000);
                    break;
                }
                else
                {
                    var crop = EmulatorController.CropImage(image, new Point(125, 600), new Point(1270, 10));
                    point = EmulatorController.FindImage(crop, Img.Red_Button, false);
                    if (point != null)
                    {
                        Variables.ScriptLog.Add("Rune boss found!");
                        EmulatorController.SendTap(new Point(point.Value.X + 125, point.Value.Y));
                        RuneBoss = true;
                        Thread.Sleep(10000);
                    }
                }
            }
            while (!PrivateVariable.Battling);

        }

        //Amalgamation Event
        private static void Demon_Realm()
        {
            Debug_.WriteLine();
            Point? point = EmulatorController.FindImage(image, Img.Close2, false);
            if (point != null)
            {
                EmulatorController.SendTap(new Point(point.Value.X, point.Value.Y));
                Thread.Sleep(1000);
            }
            if (!PrivateVariable.Run)
            {
                return;
            }
            point = null;
            int error = 0;
            while (point == null)
            {
                if (!EmulatorController.GameIsForeground("com.nubee.valkyriecrusade"))
                {
                    return;
                }
                if (!PrivateVariable.Run)
                {
                    return;
                }
                if (EmulatorController.RGBComparer(image, new Point(415, 678), Color.FromArgb(223, 192, 63), 10))
                {
                    PrivateVariable.EventType = 2;
                    PrivateVariable.InEventScreen = true;
                    DemonStage_Enter();
                    return;
                }
                point = EmulatorController.FindImage(image, Img.HellLoc, true);
                Variables.ScriptLog.Add("Locating Demon Realm Event UI!");
                if (point != null)
                {
                    Tower_Floor = OCR.OcrImage(EmulatorController.CropImage(image, new Point(300, 115), new Point(484, 142)), "eng");
                    Tower_Rank = OCR.OcrImage(EmulatorController.CropImage(image, new Point(300, 150), new Point(458, 170)), "eng");
                    Variables.ScriptLog.Add("Demon Realm Event Found!");
                    PrivateVariable.InEventScreen = true;
                    energy = GetEnergy();
                    runes = GetRune();
                }
                else
                {
                    Thread.Sleep(1000);
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
                Variables.ScriptLog.Add("Waiting for energy");
                Variables.ScriptLog.Add("Close game and wait for energy because of no energy left");
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
            if(runes == 3 && energy < 4)
            {
                StuckRune();
                return;
            }
            Variables.ScriptLog.Add("Enterting Stage");
            switch (MainScreen.Level)
            {
                case 0:
                    EmulatorController.SendTap(250, 284);
                    break;
                case 1:
                    EmulatorController.SendTap(362, 283);
                    break;
                case 2:
                    EmulatorController.SendTap(214, 370);
                    break;
                case 3:
                    EmulatorController.SendTap(353, 371);
                    break;
                case 4:
                    EmulatorController.SendTap(353, 371);
                    break;
            }
            bool EnteredStage = false;
            do
            {
                if (!PrivateVariable.Run)
                {
                    return;
                }
                if (EmulatorController.RGBComparer(image, new Point(959, 656), 31, 102, 26, 4))
                {
                    Variables.ScriptLog.Add("Start battle");
                    EmulatorController.SendTap(new Point(959, 656));
                    Thread.Sleep(2000);
                    EmulatorController.SendTap(new Point(758, 566));
                    Thread.Sleep(7000);
                    EmulatorController.SendTap(640, 400); //Tap away Round Battle Text
                    Thread.Sleep(2000);
                    stop.Start();
                    energy--; //Calculate Energy used
                    EnteredStage = true;
                    Thread.Sleep(5000);
                    break;
                }
                else
                {
                    Thread.Sleep(200);
                }
                if (!EmulatorController.GameIsForeground("com.nubee.valkyriecrusade"))
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
            while (!EmulatorController.RGBComparer(image, new Point(415, 678), Color.FromArgb(223, 192, 63), 10))
            {
                if (!EmulatorController.GameIsForeground("com.nubee.valkyriecrusade"))
                {
                    ScriptErrorHandler.Reset("Game close, restarting...");
                    return;
                }
                error++;
                if (error > 10)
                {
                    ScriptErrorHandler.Reset("Event Locate Failed!");
                    EmulatorController.KillGame("com.nubee.valkyriecrusade");
                    return;
                }
                Thread.Sleep(1000);
            }
            error = 0;
            Variables.ScriptLog.Add("Demon Realm Event Located");
            List<Point> BlackListedLocation = new List<Point>();
            Point? p = null;
            while (error < 20 && p == null)
            {
                if (!EmulatorController.GameIsForeground("com.nubee.valkyriecrusade"))
                {
                    return;
                }
                if (!PrivateVariable.Run)
                {
                    return;
                }
                Variables.ScriptLog.Add("Fetching stage images");
                List<Image> Stage = new List<Image>();
                foreach(var file in Directory.GetFiles("Img\\DemonRealm","*.png").OrderBy(f => f))
                {
                    Stage.Add(Image.FromFile(file));
                }
                byte[] crop = EmulatorController.CropImage(image, new Point(0, 0), new Point(1280, 615));
                Variables.ScriptLog.Add("Trying to find stages to enter");
                foreach (var stage in Stage)
                {
                    p = EmulatorController.FindImage(crop, new Bitmap(stage), false);
                    if (p != null)
                    {
                        if (!BlackListedLocation.Contains(p.Value))
                        {
                            Variables.ScriptLog.Add("Stage found!");
                            EmulatorController.SendTap(p.Value);
                            Thread.Sleep(1000);
                            EmulatorController.SendTap(768, 536);
                            Thread.Sleep(5000);
                            if (EmulatorController.FindImage(image, Img.Red_Button, false) != null)
                            {
                                Variables.ScriptLog.Add("Ops, looks like the stage is not able to enter!");
                                BlackListedLocation.Add(p.Value);
                                p = null;
                                continue;
                            }
                            EmulatorController.SendTap(969, 614);
                            Thread.Sleep(2000);
                            EmulatorController.SendTap(753, 423);
                            break;
                        }

                    }

                }
                Point? p2 = EmulatorController.FindImage(image, Img.GreenButton, false);
                if (p2 != null)
                {
                    EmulatorController.SendTap(p2.Value);
                    Thread.Sleep(1000);
                    continue;
                }
                error++;
            }
            if(error > 18)
            {
                Variables.ScriptLog.Add("Looks like we are in the trouble!");
                error = 0;
                ScriptErrorHandler.Reset("Restarting game as unable to detect stages properly!");
            }
            Thread.Sleep(5000);
            Point? point = EmulatorController.FindImage(image, Img.Red_Button, false);
            while(point == null)
            {
                if (!EmulatorController.GameIsForeground("com.nubee.valkyriecrusade"))
                {
                    return;
                }
                point = EmulatorController.FindImage(image, Img.Red_Button, false);
                Thread.Sleep(1000);
            }
            EmulatorController.SendTap(point.Value);
            PrivateVariable.Battling = true;
            stop.Start();
        }

        //Fighting and locate UI
        private static void LocateEnemy()
        {
            Debug_.WriteLine();
            Variables.ScriptLog.Add("Locating Enemies & UI");
            Point? point = EmulatorController.FindImage(image, Img.Close2, false);
            if (point != null)
            {
                EmulatorController.SendTap(point.Value);
                Thread.Sleep(500);
            }
            if (EmulatorController.FindImage(image, Img.NoEnergy, true) != null)
            {
                ScriptErrorHandler.Reset("No Energy Left!");
                NoEnergy();
                Attackable = false;
                return;
            }
            if (!PrivateVariable.Battling)
            {
                return;
            }
            if (EmulatorController.FindImage(image, Img.Demon_Start, true) != null)
            {
                PrivateVariable.Battling = false;
                Variables.ScriptLog.Add("Battle Ended!");
                stop.Stop();
                Variables.ScriptLog.Add("Battle used up " + stop.Elapsed);
                stop.Reset();
                EmulatorController.SendTap(1076, 106);
                Attackable = false;
                return;
            }
            if (!PrivateVariable.Battling)
            {
                return;
            }
            if (EmulatorController.FindImage(image, Img.Locate_Tower, true) != null)
            {
                PrivateVariable.Battling = false;
                Variables.ScriptLog.Add("Battle Ended!");
                stop.Stop();
                Variables.ScriptLog.Add("Battle used up " + stop.Elapsed);
                stop.Reset();
                Attackable = false;
                return;
            }
            if (!PrivateVariable.Battling)
            {
                return;
            }
            if (EmulatorController.FindImage(image,Img.Demon_InEvent, true) != null)
            {
                PrivateVariable.Battling = false;
                Variables.ScriptLog.Add("Battle Ended!");
                stop.Stop();
                Variables.ScriptLog.Add("Battle used up " + stop.Elapsed);
                stop.Reset();
                Attackable = false;
                return;
            }
            if (!PrivateVariable.Battling)
            {
                return;
            }
            byte[] crop = EmulatorController.CropImage(image, new Point(125, 0), new Point(1280, 720));
            point = EmulatorController.FindImage(crop, Img.GreenButton, false);
            if (point != null)
            {
                Variables.ScriptLog.Add("Green Button Found!");
                if (PrivateVariable.EventType == 0)
                {
                    Point? temp = EmulatorController.FindImage(image, Img.TowerFinished, true);
                    if (temp != null && RuneBoss && runes >= 3 && runes != 4)
                    {
                        PrivateVariable.InEventScreen = false;
                        PrivateVariable.InMainScreen = false;
                        PrivateVariable.Battling = false;
                        Stuck = true;
                        Variables.ScriptLog.Add("Battle Ended!");
                        stop.Stop();
                        Variables.ScriptLog.Add("Battle used up " + stop.Elapsed);
                        stop.Reset();
                        Attackable = false;
                        return;
                    }
                    else
                    {
                        EmulatorController.SendTap(point.Value.X + 125, point.Value.Y);
                        Attackable = false;
                        return;
                    }
                }
                else if (PrivateVariable.EventType == 2)
                {
                    crop = EmulatorController.CropImage(image, new Point(147, 234), new Point(613, 299));
                    if (EmulatorController.FindImage(crop, Img.DemonEnd, true) != null)
                    {
                        PrivateVariable.InEventScreen = false;
                        PrivateVariable.InMainScreen = false;
                        PrivateVariable.Battling = false;
                        EmulatorController.SendTap(point.Value.X + 125, point.Value.Y);
                        Variables.ScriptLog.Add("Battle Ended!");
                        stop.Stop();
                        Variables.ScriptLog.Add("Battle used up " + stop.Elapsed);
                        stop.Reset();
                        Attackable = false;
                        return;
                    }
                    else
                    {
                        EmulatorController.SendTap(point.Value.X + 125, point.Value.Y);
                        Attackable = false;
                        return;
                    }
                }
                else
                {
                    var pt = EmulatorController.FindImage(crop, Img.PT, true);
                    if (pt != null)
                    {
                        Variables.ScriptLog.Add("Battle Ended!");
                        EmulatorController.SendTap(point.Value.X + 125, point.Value.Y);
                        Thread.Sleep(400);
                        EmulatorController.SendTap(point.Value.X + 125, point.Value.Y);
                        Thread.Sleep(400);
                        EmulatorController.SendTap(point.Value.X + 125, point.Value.Y);
                        for (int x = 0; x < 5; x++)
                        {
                            EmulatorController.SendTap(0, 0);
                        }
                        PrivateVariable.Battling = false;
                        PrivateVariable.InEventScreen = true;
                        stop.Stop();
                        Variables.ScriptLog.Add("Battle used up " + stop.Elapsed);
                        stop.Reset();
                        Attackable = false;
                        return;
                    }
                    else
                    {
                        EmulatorController.SendTap(point.Value.X + 125, point.Value.Y);
                        Attackable = false;
                        return;
                    }
                }
            }
            crop = EmulatorController.CropImage(image, new Point(125, 0), new Point(1280, 720));
            point = EmulatorController.FindImage(crop, Img.Red_Button, false);
            if (point != null)
            {
                if (PrivateVariable.EventType == 2)
                {
                    if (EmulatorController.RGBComparer(image, new Point(133, 35), Color.FromArgb(30, 30, 30), 10))
                    {
                        PrivateVariable.Battling = false;
                        Variables.ScriptLog.Add("Battle Ended!");
                        stop.Stop();
                        Variables.ScriptLog.Add("Battle used up " + stop.Elapsed);
                        stop.Reset();
                        Attackable = false;
                        return;
                    }
                }
                Variables.ScriptLog.Add("Starting Battle");
                EmulatorController.SendTap(point.Value.X + 125, point.Value.Y);
                PrivateVariable.Battling = true;
                Thread.Sleep(900);
                crop = EmulatorController.CropImage(image, new Point(682, 544), new Point(905, 589));
                if (EmulatorController.RGBComparer(crop, Color.FromArgb(29, 98, 24)))
                {
                    EmulatorController.SendTap(793, 565);
                    Thread.Sleep(1000);
                }
                Attackable = false;
                return;
            }
            if (!PrivateVariable.Battling)
            {
                return;
            }
            point = EmulatorController.FindImage(image, Img.GarbageMessage, true);
            if (point != null)
            {
                EmulatorController.SendTap(point.Value);
                Attackable = false;
                return;
            }
            if (!PrivateVariable.Battling)
            {
                return;
            }
            point = EmulatorController.FindImage(image, Img.Love, true);
            if (point != null)
            {
                for (int x = 0; x < 10; x++)
                {
                    EmulatorController.SendTap(point.Value);
                    Thread.Sleep(100);
                }
                Attackable = false;
                return;
            }
            if (!PrivateVariable.Battling)
            {
                return;
            }
            if (EmulatorController.RGBComparer(image, new Point(959, 656), 31, 102, 26, 4))
            {
                Variables.ScriptLog.Add("Start battle");
                EmulatorController.SendTap(959,656);
                PrivateVariable.Battling = true;
                Attackable = false;
                return;
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
                byte[] enemy = EmulatorController.CropImage(image, new Point(582, 258), new Point(715, 308));
                if (EmulatorController.RGBComparer(enemy, Color.FromArgb(33, 106, 159)) || EmulatorController.RGBComparer(enemy, Color.FromArgb(171, 0, 21)))
                {
                    EmulatorController.SendTap(640, 156); //Boss在中间，打Boss
                    Variables.ScriptLog.Add("Found Boss at center!");
                }
                else
                {
                    if (!PrivateVariable.Battling)
                    {
                        return;
                    }
                    //找不到Boss血量条，可能剩下一丝血，所以先打中间试试水，再打小怪
                    EmulatorController.SendTap(640, 156);//中间
                    EmulatorController.SendTap(462, 176);//中左边
                    EmulatorController.SendTap(820, 187);//中右边
                    EmulatorController.SendTap(342, 183);//最左边
                    EmulatorController.SendTap(955, 189);//最右边
                    Variables.ScriptLog.Add("Boss not found, trying to hit others");
                }
            }
            Attackable = true;
        }
        //Click on enemy
        private static void Battle()
        {
            do
            {
                Debug_.WriteLine();
                if (!PrivateVariable.Battling)
                {
                    return;
                }
                Thread locate = new Thread(LocateEnemy);
                locate.Start();
                if (PrivateVariable.BattleScript.Count > 0)
                {
                    PrivateVariable.BattleScript[PrivateVariable.Selected_Script].Attack();
                }
                CheckEnemy();
                if (!EmulatorController.GameIsForeground("com.nubee.valkyriecrusade"))
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
                if (EmulatorController.RGBComparer(Script.image, new Point(417, 535), energy, 10))
                {
                    num++;
                }
                if (!PrivateVariable.Run)
                {
                    return 0;
                }
                if (EmulatorController.RGBComparer(Script.image, new Point(481, 535), energy, 10))
                {
                    num++;
                }
                if (!PrivateVariable.Run)
                {
                    return 0;
                }
                if (EmulatorController.RGBComparer(Script.image, new Point(546, 535), energy, 10))
                {
                    num++;
                }
                if (!PrivateVariable.Run)
                {
                    return 0;
                }
                if (EmulatorController.RGBComparer(Script.image, new Point(613, 535), energy, 10))
                {
                    num++;
                }
                if (!PrivateVariable.Run)
                {
                    return 0;
                }
                if (EmulatorController.RGBComparer(Script.image, new Point(677, 535), energy, 10))
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
                if (EmulatorController.RGBComparer(image, new Point(208, 445), energy, 10))
                {
                    num++;
                }
                if (!PrivateVariable.Run)
                {
                    return 0;
                }
                if (EmulatorController.RGBComparer(image, new Point(253, 441), energy, 10))
                {
                    num++;
                }
                if (!PrivateVariable.Run)
                {
                    return 0;
                }
                if (EmulatorController.RGBComparer(image, new Point(315, 445), energy, 10))
                {
                    num++;
                }
                if (!PrivateVariable.Run)
                {
                    return 0;
                }
                if (EmulatorController.RGBComparer(image, new Point(351, 449), energy, 10))
                {
                    num++;
                }
                if (!PrivateVariable.Run)
                {
                    return 0;
                }
                if (!EmulatorController.RGBComparer(image, new Point(410, 458), Color.FromArgb(27,24,29), 10))
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
                if (EmulatorController.RGBComparer(Script.image, new Point(945, 207), 118, 117, 118, 10))
                {
                    num--;
                }
                if (!PrivateVariable.Run)
                {
                    return 0;
                }
                if (EmulatorController.RGBComparer(Script.image, new Point(979, 308), 114, 114, 114, 10))
                {
                    num--;
                }
                if (!PrivateVariable.Run)
                {
                    return 0;
                }
                if (EmulatorController.RGBComparer(Script.image, new Point(1088, 309), 118, 117, 118, 10))
                {
                    num--;
                }
                if (!PrivateVariable.Run)
                {
                    return 0;
                }
                if (EmulatorController.RGBComparer(Script.image, new Point(1121, 204), 113, 113, 113, 10))
                {
                    num--;
                }
                if (!PrivateVariable.Run)
                {
                    return 0;
                }
                if (EmulatorController.RGBComparer(Script.image, new Point(1033, 140), 116, 115, 115, 10))
                {
                    num--;
                }
                return num;
            }
            else
            {
                int num = 0;
                if (EmulatorController.RGBComparer(image, new Point(966, 164), 154, 135, 110, 10))
                {
                    num++;
                }
                if (EmulatorController.RGBComparer(image, new Point(1071, 173), 195, 178, 145, 10))
                {
                    num++;
                }
                if (EmulatorController.RGBComparer(image, new Point(980, 240), 142, 119, 97, 10))
                {
                    num++;
                }
                if (!EmulatorController.RGBComparer(image, new Point(1067, 243), 56, 40, 45, 10))
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
                Variables.ScriptLog.Add("Close game and stuck rune!");
                nextOnline = DateTime.Now.AddMilliseconds(wait);
                Variables.ScriptLog.Add("Estimate online time is " + nextOnline);
                EmulatorController.KillGame("com.nubee.valkyriecrusade");
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
                    EmulatorController.CloseEmulator("MEmuManage.exe");
                    MessageBox.Show("正在卡符文！下次上线时间为" + nextOnline + "!");
                    Environment.Exit(0);
                }
                if (PrivateVariable.CloseEmulator)
                {
                    EmulatorController.CloseEmulator("MEmuManage.exe");
                }
                Thread.Sleep(wait - 60000);
            }
            else
            {
                Debug_.WriteLine();
                int el = 5 - energy;
                int wait = el * 2600000;
                Variables.ScriptLog.Add("Close game and stuck treasure map!");
                nextOnline = DateTime.Now.AddMilliseconds(wait);
                Variables.ScriptLog.Add("Estimate online time is " + nextOnline);
                EmulatorController.KillGame("com.nubee.valkyriecrusade");
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
                    EmulatorController.CloseEmulator("MEmuManage.exe");
                    MessageBox.Show("已存3宝藏图碎片！下次上线时间为" + nextOnline + "!");
                    Environment.Exit(0);
                }
                if (PrivateVariable.CloseEmulator)
                {
                    EmulatorController.CloseEmulator("MEmuManage.exe");
                }
                Thread.Sleep(wait - 60000);
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
                Variables.ScriptLog.Add("Estimate online time is " + nextOnline);
                EmulatorController.KillGame("com.nubee.valkyriecrusade");
                ScriptErrorHandler.PauseErrorHandler = true;
                if (PrivateVariable.CloseEmulator)
                {
                    CloseEmu = true;
                    EmulatorController.CloseEmulator("MEmuManage.exe");
                }
                Thread.Sleep(wait - 70000);
                ScriptErrorHandler.PauseErrorHandler = false;
                CloseEmu = false;
            }
            else if(PrivateVariable.EventType == 1)
            {
                ScriptErrorHandler.PauseErrorHandler = true;
                EmulatorController.KillGame("com.nubee.valkyriecrusade");
                nextOnline = DateTime.Now.AddMilliseconds(900000);
                Variables.ScriptLog.Add("Estimate online time is " + nextOnline);
                Thread.Sleep(900000);
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
