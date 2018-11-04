using System.Drawing;
using System.IO;
using System.Threading;
using System;
using ImageProcessor;
using System.Diagnostics;

namespace UI
{
    public class Script
    {
        public static Stopwatch stop = new Stopwatch();
        public static Thread UICheck;
        public static void Bot()
        {
            Thread errorhandler = new Thread(ErrorHandle);
            errorhandler.Start();
            while (PrivateVariable.Run)
            {
                byte[]image = EmulatorController.ImageCapture();
                var point = EmulatorController.FindImage(image, "Img\\Close.png");
                if (point != null)
                {
                    EmulatorController.SendTap(point.Value);
                }
                if (!EmulatorController.GameIsForeground("com.nubee.valkyriecrusade"))
                {
                    Image img = Image.FromFile("CustomImg\\Icon.png");
                    if (!EmulatorController.StartGame(new Bitmap(img)))
                    {
                        Variables.ScriptLog.Add("Unable to start game");
                        PrivateVariable.Run = false;
                        return;
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
                                        Archwitch();
                                        break;
                                    default:
                                        Variables.ScriptLog.Add("Unknown error occur, unable to detect event type.");
                                        return;
                                }
                            }
                            else
                            {
                                UICheck = new Thread(CheckBattleUI);
                               UICheck.Start();
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
            PrivateVariable.InMainScreen = false;
            int errors = 0;
            Point? point = null;
            if (!PrivateVariable.Run)
            {
                return;
            }
            byte[]image = EmulatorController.ImageCapture();
            point = EmulatorController.FindImage(image,"Img\\Locate.png");
            if (point == null)
            {
                Variables.ScriptLog.Add("Main Screen not visible");
                point = EmulatorController.FindImage(image,"Img\\Start_Game.png");
                if (point != null)
                {
                    Variables.ScriptLog.Add("Start Game Button Located!");
                    EmulatorController.SendTap(point.Value);
                    LocateMainScreen();
                }
                point = EmulatorController.FindImage(image,"Img\\Yes.png");
                if (point != null)
                {
                    EmulatorController.SendTap(point.Value);
                    LocateMainScreen();
                }
                point = EmulatorController.FindImage(image,"Img\\Update_Complete.png");
                if (point != null)
                {
                    EmulatorController.SendTap(point.Value);
                    LocateMainScreen();
                }
                point = EmulatorController.FindImage(image, "Img\\Close2.png");
                if (point != null)
                {
                    EmulatorController.SendTap(point.Value);
                    LocateMainScreen();
                }
                point = EmulatorController.FindImage(image, "Img\\Login_Reward.png");
                if (point != null)
                {
                    for (int x = 0; x < 4; x++)
                    {
                        EmulatorController.SendTap(new Point(600, 350));
                        Thread.Sleep(1000);
                    }

                    LocateMainScreen();
                }
                point = EmulatorController.FindImage(image,"Img\\Back_to_Village.png");
                if (point != null)
                {
                    Variables.ScriptLog.Add("Going back to Main screen");
                    EmulatorController.SendTap(point.Value);
                    PrivateVariable.InMainScreen = true;
                    Variables.ScriptLog.Add("Screen Located");
                }
                point = EmulatorController.FindImage(image,"Img\\Menu.png");
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
                    EmulatorController.SendTap(point.Value);
                    LocateMainScreen();
                }

            }
            else
            {
                PrivateVariable.InMainScreen = true;
                Variables.ScriptLog.Add("Screen Located");
            }

        }
        //Check Event
        private static void CheckEvent()
        {
            Point? point = null;
            if (!PrivateVariable.Run)
            {
                return;
            }
            EmulatorController.SendTap(130, 520);
            int error = 0;
            do
            {
                if (!PrivateVariable.Run)
                {
                    return;
                }
                byte[]image = EmulatorController.ImageCapture();
                if (DateTime.Now.Day < 16)
                {
                    point = EmulatorController.FindImage(image, "Img\\Locate_Tower.PNG");
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
                else
                {
                    point = EmulatorController.FindImage(image, "");
                    if (point != null)
                    {
                        //Is Archwitch
                        PrivateVariable.EventType = 1;
                        PrivateVariable.InEventScreen = true;
                        break;
                    }
                    else
                    {
                        Variables.ScriptLog.Add("Unable to locate archwitch event! Retrying...");
                        error++;
                    }
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
            Variables.ScriptLog.Add("Tower Event Found!");
            byte[]image = EmulatorController.ImageCapture();
            Point? point = null;
            if (!PrivateVariable.Run)
            {
                return;
            }
            image = EmulatorController.ImageCapture();
            int runes = GetRune();
            int energyamount = GetEnergy();
            Variables.ScriptLog.Add("Current have " + energyamount + " energy and " + runes + " runes");
            if (energyamount == 0)
            {
                Variables.ScriptLog.Add("Waiting for energy");
                if (PrivateVariable.TakePartInNormalStage)
                {
                    EmulatorController.SendTap(1218, 662);
                    Thread.Sleep(500);
                    EmulatorController.SendTap(744, 622);
                    NormalStage();
                }
                Thread.Sleep(10000);
                Tower();
            }
            else if (runes > 3 && energyamount < 3 && runes != 5)
            {
                Variables.ScriptLog.Add("Waiting for energy");
                if (PrivateVariable.TakePartInNormalStage)
                {
                    EmulatorController.SendTap(1218, 662);
                    Thread.Sleep(500);
                    EmulatorController.SendTap(744, 622);
                    NormalStage();
                }
                Thread.Sleep(10000);
                Tower();
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
            Thread.Sleep(2000);
            do
            {
                image = EmulatorController.ImageCapture();
                if (EmulatorController.RGBComparer(image, new Point(959, 656), 31, 102, 26, 4))
                {
                    Variables.ScriptLog.Add("Start battle");
                    EmulatorController.SendTap(new Point(959, 656));
                    stop.Start();
                    PrivateVariable.Battling = true;
                }
                else
                {
                    byte[]buffer = null;
                    while (buffer == null)
                    {
                        buffer = EmulatorController.ImageCapture();
                    }
                    image = EmulatorController.CropImage(buffer, new Point(125, 600), new Point(1270, 10));
                    point = EmulatorController.FindImage(image, "Img\\Start_Battle.png");
                    if (point != null)
                    {
                        Variables.ScriptLog.Add("Starting Battle");
                        EmulatorController.SendTap(new Point(point.Value.X + 125, point.Value.Y));
                        Thread.Sleep(1000);
                    }
                    else
                    {
                        //Maybe Click wrong?
                        point = EmulatorController.FindImage(image, "Img\\Wrong_Click2.png");
                        if(point != null)
                        {
                            point = EmulatorController.FindImage(image, "Img\\Close2.png");
                            if (point != null)
                            {
                                EmulatorController.SendTap(new Point(point.Value.X + 125, point.Value.Y));
                                Thread.Sleep(1000);
                            }
                        }
                    }
                }
            }
            while (!PrivateVariable.Battling);

        }
        //Archwitch Event
        private static void Archwitch()
        {
            EmulatorController.SendSwipe(100, 300,1000, 400, 800);
            byte[] image = EmulatorController.ImageCapture();
            Image normal = null;
            if (PrivateVariable.TakePartInNormalStage)
            {
                //Tower Event but fighting normal stage
                normal = Image.FromFile("Img\\NormalStage.png");
            }
            else
            {
                //Real Archwitch event
                normal = Image.FromFile("Img\\ArchwitchStage.png");
            }
            var temp = EmulatorController.MultipleImage(image, new Bitmap(normal));
            EmulatorController.SendSwipe(1000, 300, 100, 400, 800);
            var temp2 = EmulatorController.MultipleImage(image, new Bitmap(normal));
            Array.Resize(ref PrivateVariable.NormalStage, temp2.Length + temp.Length);
            temp.CopyTo(PrivateVariable.NormalStage, 0);
            temp2.CopyTo(PrivateVariable.NormalStage, temp.Length);
            PrivateVariable.FirstPageStageNum = temp.Length;
            if (PrivateVariable.UserSelectedStage > 0)
            {
                // Archwitch event will be 2 normal stage 1 boss stage, so check if user selected stage is a normal stage
                int divided = PrivateVariable.UserSelectedStage % 3;
                int divide = PrivateVariable.UserSelectedStage / 3;
                if (divided != 0)
                {
                    // Yes it is not dividable by 3 so it is a normal stage
                    // Because of the array's index, we need to minus 1 after each 2 values
                    int index = divide * 2 + divided;
                    if(index < PrivateVariable.FirstPageStageNum)
                    {
                        //It is in the first page!
                        EmulatorController.SendSwipe(100, 300, 1000, 400, 800);
                        //Selected stage
                        Point stage = PrivateVariable.NormalStage[index];
                        EmulatorController.SendTap(stage);
                    }
                    else if(index < PrivateVariable.NormalStage.Length)
                    {
                        //The stage is in second page!


                    }
                }
            }

        }
        //Normal stage enterence
        private static void NormalStage()
        {
            Stopwatch time = new Stopwatch();
            time.Start();
            int swiped = 0;
            while(time.Elapsed.Minutes <= 45)
            {
                while (true)
                {
                    byte[] image = EmulatorController.ImageCapture();
                    Point? point = EmulatorController.FindImage(image, "Img\\Normal.png");
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
                        EmulatorController.SendTap(point.Value);
                        image = null;
                        break;
                    }
                    if(swiped > 10)
                    {
                        image = null;
                        break;
                    }
                }
                if (swiped > 10)
                {
                    Reset("Unable to locate normal stage! Returning!");
                    break;
                }
                byte[] capture = EmulatorController.ImageCapture();
                while(capture == null)
                {
                    capture = EmulatorController.ImageCapture();
                }
                Point? p = EmulatorController.FindImage(capture, "Img\\SelectStage.png");
                if(p != null)
                {
                    EmulatorController.SendTap(p.Value);
                    Thread.Sleep(1000);
                }
                else
                {
                    Reset("Something wrong occurs! Unable to locate switching stage");
                    break;
                }
                capture = EmulatorController.ImageCapture();
                p = EmulatorController.FindImage(capture, "Img\\SelectWorld.png");
                if(p!= null)
                {
                    if(PrivateVariable.NormalStageNum == 1)
                    {
                        EmulatorController.SendTap(620, 215);
                        Thread.Sleep(1000);
                    }
                    else
                    {
                        EmulatorController.SendTap(620, 355);
                        Thread.Sleep(1000);
                    }
                }
                else
                {
                    Reset("Unable to choose stage! Stage number not visible！");
                    break;
                }
                Archwitch();
            }
        }
        //Reset back to just started the script
        private static void Reset(string log)
        {
            PrivateVariable.InMainScreen = false;
            PrivateVariable.InEventScreen = false;
            Variables.ScriptLog.Add(log);
        }
        //Click away all error messages
        private static void ErrorHandle()
        {
            while (PrivateVariable.Run)
            {
                Thread.Sleep(2000);
                byte[]temp = EmulatorController.ImageCapture();
                foreach (var f in Directory.GetFiles("Img\\Errors"))
                {
                    try
                    {
                        Bitmap find = null;
                        using (Stream bmp = File.Open(f, FileMode.Open))
                        {
                            Image image = Image.FromStream(bmp);
                            find = new Bitmap(image);
                            Point? p = EmulatorController.FindImage(temp, new Bitmap(find));
                            if (p != null)
                            {
                                EmulatorController.SendTap(p.Value);
                                PrivateVariable.Battling = false;
                                PrivateVariable.InEventScreen = false;
                                PrivateVariable.InMainScreen = false;
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
        public static Point? clickLocation;
        public static void Battle()
        {
            do
            {
                if(UICheck == null)
                {
                    UICheck = new Thread(CheckBattleUI);
                    UICheck.Start();
                }
                byte[]image = null;
                while(image == null)
                {
                    image = EmulatorController.ImageCapture();
                }
                foreach (var f in PrivateVariable.Skills)
                {
                    if (!PrivateVariable.Run)
                    {
                        return;
                    }
                    try
                    {
                        Point[] p = EmulatorController.MultipleImage(image, new Bitmap(EmulatorController.Decompress(f)));
                        if (p != null)
                        {
                            foreach(var po in p)
                            {
                                EmulatorController.SendSwipe(po, new Point(po.X + 1, po.Y + 1), 1200);
                                Thread.Sleep(1000);
                            }
                            break;
                        }
                    }
                    catch
                    {

                    }
                }
                EmulatorController.SendTap(625, 177);
                EmulatorController.SendTap(477, 179);
                EmulatorController.SendTap(782, 188);
                EmulatorController.SendTap(294, 175);
                EmulatorController.SendTap(978, 179);

            }
            while (PrivateVariable.Battling);

        }

        public static void CheckBattleUI()
        {
            do
            {
                if (!PrivateVariable.Run)
                {
                    return;
                }
                Variables.ScriptLog.Add("Current Battle Time used " + stop.Elapsed);
                try
                {
                    byte[] capture = EmulatorController.ImageCapture();
                    while (capture == null)
                    {
                        capture = EmulatorController.ImageCapture();
                    }
                    Point? point = null;
                    //Color c = EmulatorController.GetPixel(new Point(665, 620), capture);
                    //File.WriteAllText(c.ToString(), "");
                    if (PrivateVariable.EventType == 0)
                    {
                        point = EmulatorController.FindImage(capture, "Img\\Locate_Tower.PNG");
                        if (point != null)
                        {
                            Variables.ScriptLog.Add("Battle Ended!");
                            PrivateVariable.Battling = false;
                            PrivateVariable.InEventScreen = true;
                            stop.Stop();
                            Variables.ScriptLog.Add("Battle used up " + stop.Elapsed);
                            
                        }
                    }

                    point = EmulatorController.FindImage(capture, "Img\\GreenButton.png");
                    if (point != null)
                    {
                        EmulatorController.SendTap(point.Value);
                        
                    }
                    //Here will always clicked twice, need to fix!
                    byte[] image = EmulatorController.CropImage(capture, new Point(125, 600), new Point(1270, 10));
                    point = EmulatorController.FindImage(image, "Img\\Start_Battle.png");
                    if (point != null)
                    {
                        Variables.ScriptLog.Add("Starting Battle");
                        EmulatorController.SendTap(new Point(point.Value.X + 125, point.Value.Y));
                        Thread.Sleep(2000);
                        PrivateVariable.Battling = true;
                        
                    }
                    image = capture;
                    point = EmulatorController.FindImage(image, "Img\\RoundBattle.png");
                    if (point != null)
                    {
                        EmulatorController.SendTap(point.Value);
                        
                    }
                    point = EmulatorController.FindImage(image, "Img\\GarbageMessage.png");
                    if (point != null)
                    {
                        EmulatorController.SendTap(point.Value);
                        
                    }
                    point = EmulatorController.FindImage(image, "Img\\Love.png");
                    if (point != null)
                    {
                        for (int x = 0; x < 10; x++)
                        {
                            EmulatorController.SendTap(point.Value);
                            Thread.Sleep(100);
                        }

                        
                    }
                    Tower_Attack();
                }
                catch
                {

                }
            }
            while (PrivateVariable.Battling);
            
        }
        private  static void Tower_Attack()
        {
            byte[]image = null;
            if (EmulatorController.RGBComparer(image, new Point(959, 656), 31, 102, 26, 4))
            {
                Variables.ScriptLog.Add("Start battle");
                EmulatorController.SendTap(new Point(959, 656));
                PrivateVariable.Battling = true;
            }
        }
        
        private static int GetEnergy()
        {
            int num = 0;
            byte[]image = EmulatorController.ImageCapture();
            Color energy = Color.FromArgb(50, 233, 34);
            if (EmulatorController.RGBComparer(image,new Point(417,535),energy, 10))
            {
                num++;
            }
            if (EmulatorController.RGBComparer(image,new Point(481,535),energy, 10))
            {
                num++;
            }
            if (EmulatorController.RGBComparer(image,new Point(546, 535), energy, 10))
            {
                num++;
            }
            if (EmulatorController.RGBComparer(image,new Point(613, 535),energy, 10))
            {
                num++;
            }
            if (EmulatorController.RGBComparer(image, new Point(677, 535), energy, 10))
            {
                num++;
            }
            return num;
        }
        private static int GetRune()
        {
            byte[]image = EmulatorController.ImageCapture();
            int num = 5;
            if (EmulatorController.RGBComparer(image,new Point(945, 207), 118,117,118, 10))
            {
                num--;
            }
            if (EmulatorController.RGBComparer(image,new Point(979, 308), 114,114,114,10))
            {
                num--;
            }
            if (EmulatorController.RGBComparer(image,new Point(1088, 309), 118,117,118,10))
            {
                num--;
            }
            if (EmulatorController.RGBComparer(image,new Point(1121, 204), 113,113,113,10))
            {
                num--;
            }
            if (EmulatorController.RGBComparer(image,new Point(1033,140), 116,115,115,10))
            {
                num--;
            }
            return num;
        }
    }
}
