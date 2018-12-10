using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Drawing;
using ImageProcessor;
using System.Threading;

namespace UI
{
    class BattleScript
    {
        static string[] Scriptting;
        static string SelectedCard = "中间";
        public static void ReadScript()
        {
            if (PrivateVariable.CustomScript)
            {
                if (File.Exists("battlescript.txt"))
                {
                    Scriptting = File.ReadAllLines("battlescript.txt", Encoding.Unicode);
                }
                else
                {
                    PrivateVariable.CustomScript = false;
                }
            }

        }

        public static void TranslateAndAttack()
        {
            int x = 0;
            foreach(var l in Scriptting)
            {
                string Action = "", NewSelected = "" , Target = "" , D = "";
                try
                {
                    Action = Scriptting[x].Split('|')[0].Replace(" ", "");
                    NewSelected = Scriptting[x].Split('|')[1].Replace(" ", "");
                    Target = Scriptting[x].Split('|')[2].Replace(" ", "");
                    D = Scriptting[x].Split('|')[3].Replace(" ", "");
                }
                catch
                {

                }

                switch (Action)
                {
                    case "//":
                        break;
                    case "攻击":
                        if(SelectedCard != NewSelected)
                        {
                            switch (NewSelected)
                            {
                                case "最左":
                                    EmulatorController.SendTap(263, 473);
                                    break;
                                case "左":
                                    EmulatorController.SendTap(448, 492);
                                    break;
                                case "中间":
                                    EmulatorController.SendTap(641, 473);
                                    break;
                                case "右":
                                    EmulatorController.SendTap(834, 483);
                                    break;
                                case "最右":
                                    EmulatorController.SendTap(1017, 470);
                                    break;
                                default:
                                    Variables.ScriptLog.Add("Invalid Command");
                                    break;
                            } 
                        }
                        switch (Target)
                        {
                            case "最左":
                                EmulatorController.SendTap(311, 190);
                                break;
                            case "左":
                                EmulatorController.SendTap(462, 176);
                                break;
                            case "中间":
                                EmulatorController.SendTap(640, 156);
                                break;
                            case "右":
                                EmulatorController.SendTap(820, 187);
                                break;
                            case "最右":
                                EmulatorController.SendTap(955, 189);
                                break;
                            default:
                                Variables.ScriptLog.Add("Invalid Command");
                                break;
                        }
                        SelectedCard = NewSelected;
                        break;
                    case "发动":
                        switch (NewSelected)
                        {
                            case "最左":
                                byte[] crop = EmulatorController.CropImage(Script.image, new Point(176, 356), new Point(330, 611));
                                foreach (var f in PrivateVariable.Skills)
                                {
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
                                break;
                            case "左":
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
                                break;
                            case "中间":
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
                                break;
                            case "右":
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
                                break;
                            case "最右":
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
                                break;
                            default:
                                Variables.ScriptLog.Add("Invalid Command");
                                break;
                        }
                        break;
                    case "等待":
                        try
                        {
                            int num = Convert.ToInt32(NewSelected);
                            Thread.Sleep(num);
                        }
                        catch
                        {
                            Variables.ScriptLog.Add("Invalid Command");
                        }
                        break;
                    case "截图":
                        break;
                    case "寻找":
                        switch (NewSelected)
                        {
                            case "颜色":
                                try
                                {
                                    int R = Convert.ToInt32(Target.Split(':')[1].Split(',')[0]);
                                    int G = Convert.ToInt32(Target.Split(':')[1].Split(',')[1]);
                                    int B = Convert.ToInt32(Target.Split(':')[1].Split(',')[2]);
                                    int X = Convert.ToInt32(Target.Split(':')[0].Split(',')[0]);
                                    int Y = Convert.ToInt32(Target.Split(':')[0].Split(',')[1]);
                                    Color color = Color.FromArgb(R,G,B);
                                    if(EmulatorController.RGBComparer(Script.image,new Point(X,Y), color, 0))
                                    {
                                        if (!D.Contains("继续"))
                                        {
                                            switch (D)
                                            {
                                                case "点击":
                                                    EmulatorController.SendTap(X, Y);
                                                    break;
                                                case "滑动":
                                                    EmulatorController.SendSwipe(new Point(X, Y), new Point(X + 1, Y + 1), 1500);
                                                    break;
                                                default:
                                                    Variables.ScriptLog.Add("Invalid Command");
                                                    break;
                                            }
                                        }
                                        
                                    }
                                    else
                                    {
                                        if (D.Contains("继续"))
                                        {
                                            x = 0;
                                            NewSelected = D.Split('=')[1];
                                                while (Scriptting[x].Split('|')[0].Replace(" ", "") != "标记")
                                                {
                                                    if (x < Scriptting.Length)
                                                    {
                                                        x++;
                                                    }
                                                    else
                                                    {
                                                        Variables.ScriptLog.Add("Unable to find Marker!");
                                                        break;
                                                    }
                                                }
                                            
                                        }
                                    }
                                }
                                catch
                                {

                                }
                                break;
                            case "图片":
                                if (File.Exists(Target))
                                {
                                    var point = EmulatorController.FindImage(Script.image, Target, false);
                                    if (point != null)
                                    {
                                        switch (D)
                                        {
                                            case "点击":
                                                EmulatorController.SendTap(point.Value);
                                                break;
                                            case "滑动":
                                                EmulatorController.SendSwipe(point.Value, new Point(point.Value.X + 1, point.Value.Y + 1), 1500);
                                                break;
                                            case "继续":
                                                break;
                                            default:
                                                Variables.ScriptLog.Add("Invalid Command");
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        if (D.Contains("继续"))
                                        {
                                            x = 0;
                                                while (Scriptting[x].Split('|')[0].Replace(" ", "") != "标记")
                                                {
                                                    if (x < Scriptting.Length)
                                                    {
                                                        x++;
                                                    }
                                                    else
                                                    {
                                                        Variables.ScriptLog.Add("Unable to find Marker!");
                                                        break;
                                                    }
                                                }
                                            
                                        }
                                    }
                                }
                                break;
                        }
                        
                        break;
                    case "重返":
                        if (NewSelected == "")
                        {
                            TranslateAndAttack();
                        }
                        else
                        {
                            x = 0;
                                while (Scriptting[x].Split('|')[0].Replace(" ", "") != "标记")
                                {
                                    if (x < Scriptting.Length)
                                    {
                                        x++;
                                    }
                                    else
                                    {
                                        Variables.ScriptLog.Add("Unable to find Marker!");
                                        break;
                                    }
                                }
                        }
                        break;
                    case "标记":
                        break;
                    default:
                        Variables.ScriptLog.Add("Invalid Command");
                        break;

                }
                x++;
            }
        }
    }
}
