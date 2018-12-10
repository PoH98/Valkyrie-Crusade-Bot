using System.Drawing;
using System.IO;
using System.Threading;
using System;
using ImageProcessor;
using System.Diagnostics;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Media;

namespace UI
{
    public class Script
    {
        public static Stopwatch stop = new Stopwatch();
        private static bool RuneBoss, Stuck;
        public static int runes, energy;
        private static Point? clickLocation;
        public static int TreasureHuntIndex = -1;
        public static byte[] image = null;
        public static List<Bitmap> errorImages = new List<Bitmap>();
        public static DateTime nextOnline;
        //Main Loop
        public static void Bot()
        {
            Thread errorhandler = new Thread(ErrorHandle);
            errorhandler.Start();
            while (PrivateVariable.Run)
            {
                Thread.Sleep(10);
                string[] device = Variables.Devices_Connected.ConvertAll(x => x.ToString()).ToArray();
                if (Array.IndexOf(device, PrivateVariable.Adb_IP) > -1) //The Emulator is running
                {
                    while (Variables.Proc == null)//But not registred on our Proc value
                    {
                        //so go on and find the emulator!
                        string temp = "";
                        if (Variables.Instance.Length > 0)
                        {
                            temp = Variables.Instance;
                        }
                        else
                        {
                            Variables.Configure.TryGetValue("Emulator", out temp);
                        }
                        foreach (var proc in Process.GetProcessesByName("MEmu"))
                        {
                            Thread.Sleep(10);
                            if (proc.MainWindowTitle == temp) //Yup, found the same name
                            {
                                Variables.Proc = proc; //Register it you fxxker
                                continue;
                            }
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
                    error++;
                    if(error > 60) //Nah, we only wait for 1 minute
                    {
                        MessageBox.Show("无法截图！出现怪异错误！");
                        Environment.Exit(0);
                    }
                }
                Thread.Sleep(10);
                Image img = EmulatorController.Decompress(Script.image);
                if (img.Height != 720 || img.Width != 1280)
                {
                    if (!PrivateVariable.Run)
                    {
                        return;
                    }
                    if (Variables.Proc != null)
                    {
                        Variables.ScriptLog.Add("Emulator's screen size is not 1280*720! Detected size is " + img.Width + "*" + img.Height);
                        Variables.Proc.Kill();
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
                    Variables.Proc = null;
                    Variables.ScriptLog.Add("Restarting Emulator after setting size");
                    EmulatorController.StartEmulator();
                    Thread.Sleep(30000);
                    continue;
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
                    img = Image.FromFile("CustomImg\\Icon.png");
                    Variables.ScriptLog.Add("Starting Game");
                    if (!EmulatorController.StartGame(new Bitmap(img), Script.image))
                    {
                        Variables.ScriptLog.Add("Unable to start game");
                        EmulatorController.StartGame("com.nubee.valkyriecrusade");
                    }
                }
                else
                {
                    if (!PrivateVariable.InMainScreen && !PrivateVariable.InEventScreen && !PrivateVariable.Battling)
                    {
                        LocateMainScreen();
                        //Collect_Resource();
                        if(TreasureHuntIndex > -1)
                        {
                            TreasureHunt();
                        }
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
                                        Archwitch();
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
        private static void LocateMainScreen()
        {
            Thread.Sleep(1000);
            PrivateVariable.InMainScreen = false;
            int errors = 0;
            Point? point = null;
            if (!PrivateVariable.Run)
            {
                return;
            }
            Thread.Sleep(100);
            point = EmulatorController.FindImage(Script.image, "Img\\Locate.png", true);
            if (point == null)
            {
                if (!PrivateVariable.Run)
                {
                    return;
                }
                Variables.ScriptLog.Add("Main Screen not visible");
                point = EmulatorController.FindImage(Script.image, "Img\\Start_Game.png", true);
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
                point = EmulatorController.FindImage(Script.image, "Img\\Update_Complete.png", true);
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
                point = EmulatorController.FindImage(Script.image, "Img\\Close2.png", true);
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
                point = EmulatorController.FindImage(Script.image, "Img\\Login_Reward.png", true);
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
                point = EmulatorController.FindImage(Script.image, "Img\\Back_to_Village.png", true);
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
                point = EmulatorController.FindImage(Script.image, "Img\\Menu.png", true);
                if (point == null)
                {
                    if (errors < 60)
                    {
                        Variables.ScriptLog.Add("Waiting for Main screen");
                        Thread.Sleep(1000);
                        errors++;
                    }
                    else
                    {
                        PrivateVariable.Run = false;
                        Variables.ScriptLog.Add("Tried 60 times but still unable to locate main screen. Stopping bot");
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

            }
            else
            {
                PrivateVariable.InMainScreen = true;
                Variables.ScriptLog.Add("Screen Located");
            }

        }
        //Collect Resources
        private static void Collect_Resource()
        {
            //Find imsge and collect
            Point? p = EmulatorController.FindImage(Script.image, "Img\\Gold.png", true);
            if (p != null)
            {
                EmulatorController.SendTap(p.Value);
            }
            p = EmulatorController.FindImage(Script.image, "Img\\Elixir.png", true);
            if (p != null)
            {
                EmulatorController.SendTap(p.Value);
            }
            p = EmulatorController.FindImage(Script.image, "Img\\Metal.png", true);
            if (p != null)
            {
                EmulatorController.SendTap(p.Value);
            }
            p = EmulatorController.FindImage(Script.image, "Img\\BlueStone.png", true);
            if (p != null)
            {
                EmulatorController.SendTap(p.Value);
            }
        }
        //Treasure hunt!
        private static void TreasureHunt()
        {
            Point? p = null;
            p = EmulatorController.FindImage(Script.image, "Img\\TreasureHunt.png", true);
            //Find for treasure hunt building!
            for (int find = 0; find < 5; find++)
            {
                if (!PrivateVariable.Run)
                {
                    return;
                }
                if (p == null)
                {
                    p = EmulatorController.FindImage(Script.image, "Img\\TreasureHunt2.png", true);
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
                p = EmulatorController.FindImage(Script.image, "Img\\Start_Battle.png", true);
                if (p != null)
                {
                    //Finished hunt, collect rewards
                    EmulatorController.SendTap(p.Value);
                    Thread.Sleep(5000);
                    EmulatorController.SendTap(960, 621);
                    Thread.Sleep(7000);
                    p = EmulatorController.FindImage(Script.image, "Img\\Map.png", true);
                    //if found treasure map
                    if (p != null)
                    {
                        //Just ignore that fxxking thing
                        EmulatorController.SendTap(789, 626);
                        Thread.Sleep(10000);
                        EmulatorController.SendTap(310, 137);
                    }
                }
                Thread.Sleep(10000);
                //Back to top
                EmulatorController.SendSwipe(new Point(600, 200), new Point(600, 600), 1000);
                Thread.Sleep(1000);
                //Tap and start another hunt
                EmulatorController.SendTap(998, 340);
                Thread.Sleep(1000);
                EmulatorController.SendTap(771, 453);
                //Next Treasure hunt
                EmulatorController.SendTap(1031, 50);
                Thread.Sleep(1000);
            }
            while (true);

        }
        //Check Event
        private static void CheckEvent()
        {
            Point? point = null;
            if (!PrivateVariable.Run)
            {
                return;
            }
            string Special;
            if(Variables.Configure.TryGetValue("Double_Event",out Special))
            {
                if(Special == "true")
                {
                    EmulatorController.SendTap(130, 350);
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
            int error = 0;
            do
            {
                Thread.Sleep(1000);
                if (!PrivateVariable.Run)
                {
                    return;
                }
                if (PrivateVariable.EventType == 0)
                {
                    point = EmulatorController.FindImage(Script.image, "Img\\Locate_Tower.PNG", true);
                    Variables.ScriptLog.Add("Locating Tower Event UI!");
                    if (point == null)
                    {
                        Variables.ScriptLog.Add("Unable to locate tower event! Retrying...");
                        error++;
                        Thread.Sleep(1000);
                    }
                    else
                    {
                        //Is Tower Event
                        PrivateVariable.EventType = 0;
                        PrivateVariable.InEventScreen = true;
                        break;
                    }
                }
                else if (PrivateVariable.EventType == 1)
                {
                    point = EmulatorController.FindImage(Script.image, "Img\\Archwitch_Rec.png", true);
                    if (point != null)
                    {
                        //Is Archwitch
                        PrivateVariable.EventType = 1;
                        PrivateVariable.InEventScreen = true;
                        break;
                    }
                    else
                    {
                        for (int y = 0; y < 10; y++)
                        {
                            EmulatorController.SendTap(1, 1);
                        }
                        Variables.ScriptLog.Add("Unable to locate archwitch event! Retrying...");
                        error++;
                    }
                }
                else
                {
                    if(DateTime.Now.Day < 16)
                    {
                        PrivateVariable.EventType = 0;
                    }
                    else
                    {
                        PrivateVariable.EventType = 1;
                    }
                }
                point = EmulatorController.FindImage(Script.image, "Img\\Locate.png", true);
                if (point != null)
                {
                    Variables.ScriptLog.Add("Rare error happens, still in main screen!");
                    PrivateVariable.InMainScreen = false;
                    return;
                }
                if (error > 60)
                {
                    PrivateVariable.Run = false;
                    Variables.ScriptLog.Add("Looks like your event is not detectable! Stopping script!");
                    return;
                }

            }
            while (point == null);
        }
        //Tower Event
        private static void Tower()
        {
            Thread.Sleep(1000);
            RuneBoss = false;
            Point? point = null;
            clickLocation = null;
            //Here will handle all the battle errors such as exited while battle
            CheckEnemy();
            if (clickLocation != null) //It is in battle, so go on to battle!
            {
                PrivateVariable.Battling = true;
                return;
            }
            byte[] crop = EmulatorController.CropImage(Script.image, new Point(125, 0), new Point(1280, 720));
            point = EmulatorController.FindImage(crop, "Img\\Start_Battle.png", false);
            if(point != null) //It is also in battle!
            {
                PrivateVariable.Battling = true;
                return;
            }
            //Nope, we are in the tower event main screen! So go on!
            point = EmulatorController.FindImage(Script.image, "Img\\Close2.png", true);
            if (point != null)
            {
                EmulatorController.SendTap(new Point(point.Value.X + 125, point.Value.Y));
                Thread.Sleep(1000);
            }
            if (!PrivateVariable.Run)
            {
                return;
            }
            point = EmulatorController.FindImage(Script.image, "Img\\Locate_Tower.PNG", true);
            Variables.ScriptLog.Add("Locating Tower Event UI!");
            if (point != null)
            {
                Variables.ScriptLog.Add("Tower Event Found!");
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
                    NormalStage();
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
            }
            Thread.Sleep(3000);
            do
            {
                if (!PrivateVariable.Run)
                {
                    return;
                }
                if (EmulatorController.RGBComparer(Script.image, new Point(959, 656), 31, 102, 26, 4))
                {
                    Variables.ScriptLog.Add("Start battle");
                    EmulatorController.SendTap(new Point(959, 656));
                    Thread.Sleep(7000);
                    EmulatorController.SendTap(640, 400); //Tap away Round Battle Text
                    Thread.Sleep(2000);
                    stop.Start();
                    PrivateVariable.Battling = true;
                    Thread.Sleep(1000);
                    break;
                }
                else
                {
                    crop = EmulatorController.CropImage(Script.image, new Point(125, 600), new Point(1270, 10));
                    point = EmulatorController.FindImage(crop, "Img\\Start_Battle.png", false);
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
        //Archwitch Event
        private static void Archwitch()
        {
            if (!PrivateVariable.InMap)
            {
                Variables.ScriptLog.Add("Locating Archwitch Event");
                Point? IsInEvent = EmulatorController.FindImage(Script.image, "Img\\Archwitch_Rec.png", true);
                while (IsInEvent == null)
                {
                    Thread.Sleep(1000);
                    IsInEvent = EmulatorController.FindImage(Script.image, "Img\\GreenButton.png", false);
                }
                PrivateVariable.InMap = true;
                Variables.ScriptLog.Add("Archwitch Event located");
                Variables.ScriptLog.Add("Reading stages");
                EmulatorController.SendSwipe(100, 300, 1000, 400, 800);
                byte[] crop = EmulatorController.CropImage(Script.image, new Point(0, 0), new Point(1140, 720));
                Image normal = null, New = null;
                Image Boss = null;
                if (PrivateVariable.TakePartInNormalStage)
                {
                    //Tower Event but fighting normal stage
                    normal = Image.FromFile("Img\\Archwitch\\NormalStage.png");
                    Boss = Image.FromFile("Img\\Archwitch\\NormalBoss.png");
                    New = Image.FromFile("Img\\Archwitch\\New.png");
                }
                else
                {
                    //Real Archwitch event
                    normal = Image.FromFile("Img\\Archwitch\\ArchwitchStage.png");
                    //Boss = Image.FromFile("Img\\Archwitch\\ArchwitchBoss.png");
                    New = Image.FromFile("Img\\Archwitch\\New.png");
                }
                Point[] temp = null, boss = null, temp2 = null, boss2 = null;
                Point? newtemp = null;
                if (normal != null)
                {
                    temp = EmulatorController.FindMultiple(Script.image, new Bitmap(normal));
                }
                if (Boss != null)
                {
                    boss = EmulatorController.FindMultiple(Script.image, new Bitmap(Boss));
                }
                if (New != null)
                {
                    newtemp = EmulatorController.FindImage(Script.image, new Bitmap(New), true);
                }
                bool SecondPage = false;
                EmulatorController.SendSwipe(1000, 300, 100, 400, 800);
                crop = EmulatorController.CropImage(Script.image, new Point(170, 0), new Point(1280, 720));
                if (normal != null)
                {
                    temp2 = EmulatorController.FindMultiple(crop, new Bitmap(normal));
                }
                if (Boss != null)
                {
                    boss2 = EmulatorController.FindMultiple(crop, new Bitmap(Boss));
                }
                if (newtemp == null)
                {
                    newtemp = EmulatorController.FindImage(crop, new Bitmap(New), true);
                    SecondPage = true;
                }
                if (temp2 != null && temp != null)
                {
                    Array.Resize(ref PrivateVariable.NormalStage, temp2.Length + temp.Length);
                    temp.CopyTo(PrivateVariable.NormalStage, 0);
                    temp2.CopyTo(PrivateVariable.NormalStage, temp.Length);
                }
                else if (temp2 != null)
                {
                    Array.Resize(ref PrivateVariable.NormalStage, temp2.Length);
                    temp2.CopyTo(PrivateVariable.NormalStage, 0);
                }
                else if (temp != null)
                {
                    Array.Resize(ref PrivateVariable.NormalStage, temp.Length);
                    temp.CopyTo(PrivateVariable.NormalStage, 0);
                }
                else
                {
                    PrivateVariable.NormalStage = null;
                }
                if (boss != null && boss2 != null)
                {
                    Array.Resize(ref PrivateVariable.BossStage, boss2.Length + boss.Length);
                    boss.CopyTo(PrivateVariable.BossStage, 0);
                    boss2.CopyTo(PrivateVariable.BossStage, boss.Length);
                }
                else if (boss != null)
                {
                    Array.Resize(ref PrivateVariable.BossStage, boss.Length);
                    boss.CopyTo(PrivateVariable.BossStage, 0);
                }
                else if (boss2 != null)
                {
                    Array.Resize(ref PrivateVariable.BossStage, boss2.Length);
                    boss2.CopyTo(PrivateVariable.BossStage, 0);
                }
                else
                {
                    PrivateVariable.BossStage = null;
                }
                if (temp != null)
                {
                    PrivateVariable.FirstPageStageNum = temp.Length;
                }
                if (boss != null)
                {
                    PrivateVariable.FirstPageBossNum = boss.Length;
                }
                Variables.ScriptLog.Add("Read stage completed!");
                if (newtemp != null)
                {
                    Variables.ScriptLog.Add("New stage found!");
                }
                if (PrivateVariable.BossStage != null)
                {
                    Variables.ScriptLog.Add("Boss stage total " + PrivateVariable.BossStage.Length);
                }
                if (PrivateVariable.NormalStage != null)
                {
                    Variables.ScriptLog.Add("Normal stage total " + PrivateVariable.NormalStage.Length);
                }
                SelectStage(newtemp, SecondPage);
            }
            
            StartMove();
        }
        //Normal stage enterence
        private static void NormalStage()
        {
            Stopwatch time = new Stopwatch();
            time.Start();
            int swiped = 0;
                while (true)
                {
                    Point? point = EmulatorController.FindImage(Script.image, "Img\\Normal.png", true);
                    if (point == null && swiped < 5)
                    {
                        EmulatorController.SendSwipe(500, 300, 1000, 300, 500);
                        Thread.Sleep(500);
                    }
                    else if (point == null)
                    {
                        EmulatorController.SendSwipe(1000, 300, 500, 300, 500);
                        Thread.Sleep(500);
                    }
                    else
                    {
                        Variables.ScriptLog.Add("Entering Normal Stages");
                        EmulatorController.SendTap(640, 305);
                        Thread.Sleep(10000);
                        break;
                    }
                    if (swiped > 10)
                    {
                        break;
                    }
                }
                if (swiped > 10)
                {
                    Reset("Unable to locate normal stage! Returning!");
                    return;
                }
                Point? p = EmulatorController.FindImage(Script.image, "Img\\SelectStage.png", true);
                if (p != null)
                {
                    EmulatorController.SendTap(p.Value);
                    Thread.Sleep(1000);
                }
                else
                {
                    Reset("Something wrong occurs! Unable to locate switching stage");
                    return;
                }
                p = EmulatorController.FindImage(Script.image, "Img\\SelectWorld.png", true);
                if (p != null)
                {
                    if (PrivateVariable.NormalStageNum == 1)
                    {
                        EmulatorController.SendTap(620, 215);
                        Thread.Sleep(10000);
                    }
                    else
                    {
                        EmulatorController.SendTap(620, 355);
                        Thread.Sleep(10000);
                    }
                }
                else
                {
                    Reset("Unable to choose stage! Stage number not visible！");
                    return;
                }
                Archwitch();
            } 
        //Reset back to just started the script
        public static void Reset(string log)
        {
            PrivateVariable.InMainScreen = false;
            PrivateVariable.InEventScreen = false;
            Variables.ScriptLog.Add(log);
        }
        //Click away all error messages
        private static void ErrorHandle()
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
                
                Thread.Sleep(2000);
                if(Variables.Proc != null)
                {
                    try
                    {
                        foreach (var e in errorImages)
                        {
                            Thread.Sleep(10);
                            Point? p = EmulatorController.FindImage(image, e, true);
                            if (p != null)
                            {
                                EmulatorController.SendTap(p.Value);
                                Reset("Error message found!");
                            }
                        }
                    }
                    catch
                    {

                    }
                }
                
            }
        }
        //Fighting
        private static void LocateEnemy()
        {

            Variables.ScriptLog.Add("Locating Enemies");
            clickLocation = null;
            CheckEnemy();
            if (clickLocation != null)
            {
                Variables.ScriptLog.Add("Enemies position is at " + clickLocation.Value);
            }
            else
            {
                Point? point = null;
                if (PrivateVariable.EventType == 0)
                {
                    point = EmulatorController.FindImage(Script.image, "Img\\Locate_Tower.PNG", true);
                    if (point != null)
                    {
                        Variables.ScriptLog.Add("Battle Ended!");
                        PrivateVariable.Battling = false;
                        PrivateVariable.InEventScreen = true;
                        stop.Stop();
                        Variables.ScriptLog.Add("Battle used up " + stop.Elapsed);
                        stop.Reset();
                    }
                }
                if (!PrivateVariable.Battling)
                {
                    return;
                }
                point = null;
                byte[] crop = EmulatorController.CropImage(Script.image, new Point(125, 0), new Point(1280, 720));
                point = EmulatorController.FindImage(crop, "Img\\GreenButton.png", false);
                if (point != null)
                {
                    Variables.ScriptLog.Add("Green Button Found!");
                    if (PrivateVariable.EventType == 0)
                    {
                        Point? temp = EmulatorController.FindImage(Script.image, "Img\\TowerFinished.png", true);
                        if (temp != null && RuneBoss && runes >= 3)
                        {
                            PrivateVariable.InEventScreen = false;
                            PrivateVariable.InMainScreen = false;
                            PrivateVariable.Battling = false;
                            Stuck = true;
                            return;
                        }
                        else
                        {
                            clickLocation = new Point(2000, 2000);
                            EmulatorController.SendTap(new Point(point.Value.X + 125, point.Value.Y));
                            return;
                        }
                    }
                    else
                    {
                        var pt = EmulatorController.FindImage(crop, "Img\\PT.PNG", true);
                        if (pt != null)
                        {
                            Variables.ScriptLog.Add("Battle Ended!");
                            for (int x = 0; x < 5; x++)
                            {
                                if (!PrivateVariable.Battling)
                                {
                                    return;
                                }
                                EmulatorController.SendTap(point.Value.X + 125, point.Value.Y);
                                Thread.Sleep(1000);
                            }
                            PrivateVariable.Battling = false;
                            PrivateVariable.InEventScreen = true;
                            stop.Stop();
                            Variables.ScriptLog.Add("Battle used up " + stop.Elapsed);
                            stop.Reset();
                        }
                        else
                        {
                            EmulatorController.SendTap(point.Value.X + 125, point.Value.Y);
                        }
                    }
                }

                if (!PrivateVariable.Battling)
                {
                    return;
                }
                point = null;
                point = EmulatorController.FindImage(crop, "Img\\Start_Battle.png", false);
                if (point != null)
                {
                    Variables.ScriptLog.Add("Starting Battle");
                    clickLocation = new Point(point.Value.X + 125, point.Value.Y);
                    PrivateVariable.Battling = true;

                }
                if (!PrivateVariable.Battling)
                {
                    return;
                }
                point = null;
                point = EmulatorController.FindImage(Script.image, "Img\\GarbageMessage.png", true);
                if (point != null)
                {
                    clickLocation = point.Value;

                }
                point = null;
                if (!PrivateVariable.Battling)
                {
                    return;
                }
                point = EmulatorController.FindImage(Script.image, "Img\\Love.png", true);
                if (point != null)
                {
                    for (int x = 0; x < 10; x++)
                    {
                        EmulatorController.SendTap(point.Value);
                        Thread.Sleep(100);
                    }
                }
                if (!PrivateVariable.Battling)
                {
                    return;
                }
                if (EmulatorController.RGBComparer(Script.image, new Point(959, 656), 31, 102, 26, 4))
                {
                    Variables.ScriptLog.Add("Start battle");
                    clickLocation = new Point(959, 656);
                    PrivateVariable.Battling = true;
                }
                else
                {
                    Variables.ScriptLog.Add("Unable to locate enemy. ");
                }
            }

        }
        /// <summary>
        /// Check is there any HP bar in game
        /// </summary>
        private static void CheckEnemy()
        {
            if (EmulatorController.RGBComparer(Script.image, new Point(1185, 25), Color.FromArgb(1, 67, 200), 1))
            {
                clickLocation = new Point(640, 156);
            }
            else
            {
                byte[] enemy = EmulatorController.CropImage(Script.image, new Point(582, 258), new Point(715, 308));
                if (EmulatorController.RGBComparer(enemy, Color.FromArgb(33, 106, 159)) || EmulatorController.RGBComparer(enemy, Color.FromArgb(171, 0, 21)))
                {
                    clickLocation = new Point(640, 156);
                }
                else
                {
                    enemy = EmulatorController.CropImage(Script.image, new Point(409, 255), new Point(534, 307));
                    if (EmulatorController.RGBComparer(enemy, Color.FromArgb(33, 106, 159)) || EmulatorController.RGBComparer(enemy, Color.FromArgb(171, 0, 21)))
                    {
                        clickLocation = new Point(462, 176);
                    }
                    else
                    {
                        enemy = EmulatorController.CropImage(Script.image, new Point(771, 260), new Point(889, 307));
                        if (EmulatorController.RGBComparer(enemy, Color.FromArgb(33, 106, 159)) || EmulatorController.RGBComparer(enemy, Color.FromArgb(171, 0, 21)))
                        {
                            clickLocation = new Point(820, 187);
                        }
                        else
                        {
                            enemy = EmulatorController.CropImage(Script.image, new Point(276, 263), new Point(388, 306));
                            if (EmulatorController.RGBComparer(enemy, Color.FromArgb(33, 106, 159)) || EmulatorController.RGBComparer(enemy, Color.FromArgb(171, 0, 21)))
                            {
                                clickLocation = new Point(311, 190);
                            }
                            else
                            {
                                enemy = EmulatorController.CropImage(Script.image, new Point(908, 258), new Point(1039, 309));
                                if (EmulatorController.RGBComparer(enemy, Color.FromArgb(33, 106, 159)) || EmulatorController.RGBComparer(enemy, Color.FromArgb(171, 0, 21)))
                                {
                                    clickLocation = new Point(955, 189);
                                }
                                else
                                {
                                    clickLocation = null;
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void Battle()
        {
            BattleScript.ReadScript();
            do
            {
                if (!PrivateVariable.Battling)
                {
                    return;
                }
                Thread.Sleep(200);
                byte[] crop = EmulatorController.CropImage(Script.image, new Point(176, 356), new Point(330, 611));
                clickLocation = null;
                Thread locate = new Thread(LocateEnemy);
                locate.Start();
                for (int x = 0; x < 10; x++)
                {
                    EmulatorController.SendTap(1, 1);
                }
                foreach (var f in PrivateVariable.Skills)
                {
                    if (!PrivateVariable.Run)
                    {
                        return;
                    }
                    try
                    {
                        Point? p = EmulatorController.FindImage(crop, new Bitmap(EmulatorController.Decompress(f)), false);
                        if (p != null)
                        {
                            Variables.ScriptLog.Add("Skill actived");
                            EmulatorController.SendSwipe(new Point(263, 473), new Point(264, 474), 1200);
                            Thread.Sleep(1000);
                            break;
                        }
                    }
                    catch
                    {

                    }
                }
                crop = EmulatorController.CropImage(Script.image, new Point(357, 356), new Point(543, 610));
                foreach (var f in PrivateVariable.Skills)
                {

                    if (!PrivateVariable.Run)
                    {
                        return;
                    }
                    try
                    {
                        Point? p = EmulatorController.FindImage(crop, new Bitmap(EmulatorController.Decompress(f)), false);
                        if (p != null)
                        {
                            Variables.ScriptLog.Add("Skill actived");
                            EmulatorController.SendSwipe(new Point(448, 492), new Point(449, 493), 1200);
                            Thread.Sleep(1000);
                            break;
                        }
                    }
                    catch
                    {

                    }
                }
                crop = EmulatorController.CropImage(Script.image, new Point(546, 376), new Point(724, 597));
                foreach (var f in PrivateVariable.Skills)
                {
                    if (!PrivateVariable.Run)
                    {
                        return;
                    }
                    try
                    {
                        Point? p = EmulatorController.FindImage(crop, new Bitmap(EmulatorController.Decompress(f)), false);
                        if (p != null)
                        {
                            Variables.ScriptLog.Add("Skill actived");
                            EmulatorController.SendSwipe(new Point(641, 473), new Point(642, 474), 1200);
                            Thread.Sleep(1000);
                            break;
                        }
                    }
                    catch
                    {

                    }
                }
                crop = EmulatorController.CropImage(Script.image, new Point(761, 356), new Point(921, 613));
                foreach (var f in PrivateVariable.Skills)
                {
                    if (!PrivateVariable.Run)
                    {
                        return;
                    }
                    try
                    {
                        Point? p = EmulatorController.FindImage(crop, new Bitmap(EmulatorController.Decompress(f)), false);
                        if (p != null)
                        {
                            Variables.ScriptLog.Add("Skill actived");
                            EmulatorController.SendSwipe(new Point(834, 483), new Point(835, 484), 1200);
                            Thread.Sleep(1000);
                            break;
                        }
                    }
                    catch
                    {

                    }
                }
                crop = EmulatorController.CropImage(Script.image, new Point(934, 356), new Point(1090, 578));
                foreach (var f in PrivateVariable.Skills)
                {
                    if (!PrivateVariable.Run)
                    {
                        return;
                    }
                    try
                    {
                        Point? p = EmulatorController.FindImage(crop, new Bitmap(EmulatorController.Decompress(f)), false);
                        if (p != null)
                        {
                            Variables.ScriptLog.Add("Skill actived");
                            EmulatorController.SendSwipe(new Point(1017, 470), new Point(1018, 471), 1200);
                            Thread.Sleep(1000);
                            break;
                        }
                    }
                    catch
                    {

                    }
                }
                if (clickLocation != null)
                {
                    EmulatorController.SendTap(clickLocation.Value);
                }
                else
                {
                    EmulatorController.SendTap(640, 156);
                    EmulatorController.SendTap(462, 176);
                    EmulatorController.SendTap(820, 187);
                    EmulatorController.SendTap(311, 190);
                    EmulatorController.SendTap(955, 189);
                }
            }
            while (PrivateVariable.Battling);
        }

        public static int GetEnergy()
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

        private static int GetRune()
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

        private static void SelectStage(Point? newtemp, bool SecondPage)
        {

            if (PrivateVariable.AlwaysAttackNew)
            {
                if (newtemp == null)
                {
                    Variables.ScriptLog.Add("No new stage found, selecting randomly");
                    Random rnd = new Random();
                    if(PrivateVariable.BossStage != null && PrivateVariable.NormalStage != null)
                    {
                        PrivateVariable.UserSelectedStage = rnd.Next(1, PrivateVariable.BossStage.Length + PrivateVariable.NormalStage.Length);
                    }
                    else if(PrivateVariable.NormalStage != null)
                    {
                        PrivateVariable.UserSelectedStage = rnd.Next(1, PrivateVariable.NormalStage.Length);
                    }
                    else if(PrivateVariable.BossStage != null)
                    {
                        PrivateVariable.UserSelectedStage = rnd.Next(1, PrivateVariable.BossStage.Length);
                    }
                    else
                    {
                        Variables.ScriptLog.Add("Unable to detect any stage!");
                        PrivateVariable.Run = false;
                        return;
                    }
                }
                else
                {
                    Variables.ScriptLog.Add("Going to new stage and start battle!");
                    if (!SecondPage)
                    {
                        //Back to first page
                        EmulatorController.SendSwipe(100, 300, 1000, 400, 800);
                    }
                    EmulatorController.SendTap(newtemp.Value);
                    Point? p = EmulatorController.FindImage(Script.image, "Img\\GreenButton.png", false);
                    while (p == null)
                    {
                        Thread.Sleep(10);
                        p = EmulatorController.FindImage(Script.image, "Img\\GreenButton.png", false);
                    }
                    EmulatorController.SendTap(p.Value);
                }
            }
            int divided = PrivateVariable.UserSelectedStage % 3;
            int divide = PrivateVariable.UserSelectedStage / 3;
            if (divided != 0)
            {
                // Yes it is not dividable by 3 so it is a normal stage
                // Because of the array's index, we need to minus 1 after each 2 values
                int index = divide * 2 + divided;
                if (index <= PrivateVariable.FirstPageStageNum)
                {
                    //It is in the first page!
                    EmulatorController.SendSwipe(100, 300, 1000, 400, 800);
                    //Selected stage
                    Point stage = PrivateVariable.NormalStage[index];
                    EmulatorController.SendTap(stage);
                    Point? p = EmulatorController.FindImage(Script.image, "Img\\GreenButton.png", false);
                    while (p == null)
                    {
                        p = EmulatorController.FindImage(Script.image, "Img\\GreenButton.png", false);
                    }
                    EmulatorController.SendTap(p.Value);
                }
                else if (index < PrivateVariable.NormalStage.Length)
                {
                    //The stage is in second page!
                    Point stage = PrivateVariable.NormalStage[index];
                    EmulatorController.SendTap(stage);
                    Point? p = EmulatorController.FindImage(Script.image, "Img\\GreenButton.png", false);
                    while (p == null)
                    {
                        p = EmulatorController.FindImage(Script.image, "Img\\GreenButton.png", false);
                    }
                    EmulatorController.SendTap(p.Value);
                }
            }
            else
            {
                if(PrivateVariable.BossStage != null)
                {
                    int index = divide;
                    if (index <= PrivateVariable.FirstPageBossNum)
                    {
                        //It is in the first page!
                        EmulatorController.SendSwipe(100, 300, 1000, 400, 800);
                        //Selected stage
                        Point stage = PrivateVariable.BossStage[index];
                        EmulatorController.SendTap(stage);
                        Point? p = EmulatorController.FindImage(Script.image, "Img\\GreenButton.png", false);
                        while (p == null)
                        {
                            p = EmulatorController.FindImage(Script.image, "Img\\GreenButton.png", false);
                        }
                        EmulatorController.SendTap(p.Value);
                    }
                    else if (index < PrivateVariable.BossStage.Length)
                    {
                        //The stage is in second page!
                        Point stage = PrivateVariable.BossStage[index];
                        EmulatorController.SendTap(stage);
                        Point? p = EmulatorController.FindImage(Script.image, "Img\\GreenButton.png", false);
                        while (p == null)
                        {
                            p = EmulatorController.FindImage(Script.image, "Img\\GreenButton.png", false);
                        }
                        EmulatorController.SendTap(p.Value);
                    }
                }
                
            }
            StartMove();
        }

        private static void StartMove()
        {
            Point? point = null;
            bool Move = true;
            while (Move)
            {
                Variables.ScriptLog.Add("Waiting for auto movement...");
                Thread.Sleep(1000);
                point = EmulatorController.FindImage(Script.image, "Img\\Start_Battle.png", false);
                if(point != null)
                {
                    EmulatorController.SendTap(point.Value);
                    Move = false;
                    PrivateVariable.Battling = true;
                    break;
                }
                else
                {
                    byte[] crop = EmulatorController.CropImage(Script.image, new Point(896, 360), new Point(1045, 430));
                    if (EmulatorController.RGBComparer(crop, Color.FromArgb(21, 73, 17)))
                    {
                        EmulatorController.SendTap(930, 387);
                    }
                    EmulatorController.SendTap(652, 139);
                }
            }
        }

        private static void StuckRune()
        {
            int el = 5 - energy - 1;
            int wait = el * 2600000;
            Variables.ScriptLog.Add("Close game and stuck rune!");
            nextOnline = DateTime.Now.AddMilliseconds(wait);
            Variables.ScriptLog.Add("Estimate online time is " + nextOnline);
            EmulatorController.KillGame("com.nubee.valkyriecrusade");
            if (!PrivateVariable.EnterRune)
            {
                PrivateVariable.Run = false;
                if(Directory.Exists(Environment.CurrentDirectory + "\\Audio\\"))
                {
                    string[] path = Directory.GetFiles(Environment.CurrentDirectory + "\\Audio\\", "*.mp3", SearchOption.TopDirectoryOnly);
                    if(path.Length > 0)
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

        private static void NoEnergy()
        {
            int el = 5 - energy;
            int wait = el * 2600000;
            nextOnline = DateTime.Now.AddMilliseconds(wait);
            Variables.ScriptLog.Add("Estimate online time is " + nextOnline);
            EmulatorController.KillGame("com.nubee.valkyriecrusade");
            if (PrivateVariable.CloseEmulator)
            {
                EmulatorController.CloseEmulator("MEmuManage.exe");
            }
            Thread.Sleep(wait - 70000);
        }
    }
}
