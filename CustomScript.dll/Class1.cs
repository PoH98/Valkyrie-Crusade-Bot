using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Drawing;
using UI;
using System.Windows.Forms;
using ImageProcessor;
using System.IO;
using System.Diagnostics;

namespace CustomScript
{
    public class CustomScript : BattleScript
    {
        private static string[] script;
        private static byte[] crop;

        public void Attack()
        {
            foreach(var line in script)
            {
                string[] text = line.Split(',');
                string key = text[0];
                List<string> value = text.ToList();
                value.Remove(key);
                ConvertScript(key, value);
            }
        }

        private static void ConvertScript(string key, List<string> value)
        {
            switch (key)
            {
                case "技能发动检查":
                    if(value.Count > 0)
                    {
                        Variables.ScriptLog.Add("正在检查" + value[0] + "技能");
                        switch (value[0])
                        {
                            case "最左":
                                crop = EmulatorController.CropImage(Script.image, new Point(176, 356), new Point(330, 611));
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
                                break;
                            case "左边":
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
                            case "右边":
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
                                Variables.ScriptLog.Add("难不成你想要帮敌人发动技能不成？");
                                break;
                        }
                    }
                    break;
                case "发呆":
                    if(value.Count > 1)
                    {
                        try
                        {
                            int num = 1;
                            switch (value[1])
                            {
                                case "秒":
                                    num = 1000;
                                    break;
                                case "毫秒":
                                    break;
                                default:
                                    Variables.ScriptLog.Add("我去，你还想发呆多久？嫌弃秒和毫秒都太短了吗？");
                                    return;
                            }
                            int sleep = Convert.ToInt32(value[0]) * num;
                            if (sleep > 60000)
                            {
                                Variables.ScriptLog.Add("我去，你还想发呆多久？嫌弃秒和毫秒都太短了吗？");
                            }
                            else
                            {
                                Thread.Sleep(sleep);
                            }
                        }
                        catch
                        {

                        }
                    }
                    break;
                case "攻击":
                    if (Script.clickLocation != null)
                    {
                        EmulatorController.SendTap(Script.clickLocation.Value);
                    }
                    else
                    {
                        EmulatorController.SendTap(640, 156);
                        EmulatorController.SendTap(462, 176);
                        EmulatorController.SendTap(820, 187);
                        EmulatorController.SendTap(311, 190);
                        EmulatorController.SendTap(955, 189);
                    }
                    break;

            }
        }

        public Control[] CreateUI()
        {
            Button test = new Button();
            test.Text = "测试脚本";
            test.Click += Test_Click;
            test.Location = new Point(10, 10);
            test.Width = 210;
            test.Height = 40;
            Button create = new Button();
            create.Text = "修改/创建脚本";
            create.Location = new Point(290, 10);
            create.Width = 210;
            create.Height = 40;
            create.Click += Create_Click;
            RichTextBox script = new RichTextBox();
            script.Location = new Point(10, 70);
            script.ReadOnly = true;
            script.Width = 510;
            script.Height = 200;
            CheckBox usethis = new CheckBox();
            usethis.Text = "使用这个脚本！";
            usethis.Location = new Point(10, 300);
            usethis.AutoSize = true;
            usethis.CheckedChanged += Usethis_CheckedChanged;
            Control[] c = { test,create,script,usethis };
            return c;
        }

        private void Usethis_CheckedChanged(object sender, EventArgs e)
        {
            if((sender as CheckBox).Checked)
            PrivateVariable.Selected_Script = PrivateVariable.BattleScript.IndexOf(this);
        }

        private void Create_Click(object sender, EventArgs e)
        {
            if (!File.Exists("Battle.csv"))
            {
                string[] contents = {"技能发动检查,最左", "技能发动检查,左边", "技能发动检查,中间","技能发动检查,右边", "技能发动检查,最右","发呆，10毫秒","攻击" };
                File.WriteAllLines("Battle.csv",contents,Encoding.UTF8);
            }
            Process.Start("Battle.csv");
        }

        private void Test_Click(object sender, EventArgs e)
        {
            Thread t = new Thread(Attack);
            t.Start();
        }

        public void ReadConfig()
        {
            if (File.Exists("Battle.csv"))
            {
                script = File.ReadAllLines("Battle.csv");
            }
        }

        public string ScriptName()
        {
            return "User Custom Script";
        }
    }
}
