using ImageProcessor;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UI
{
    public interface BattleScript
    {
        /// <summary>
        /// You have to read your script how to translate if you let users to customize script!
        /// </summary>
        void ReadConfig();
        /// <summary>
        /// Then you have to set how the attack will be done! Tips: remember to add Script.clickLocation for reading enemies location or click away UI!
        /// </summary>
        void Attack();
        /// <summary>
        /// You need to create bunch of UIs! Else how users enable it?
        /// </summary>
        Control[] CreateUI();

        string ScriptName();
    }

    public class defaultScript : BattleScript
    {
        public void Attack()
        {
            for (int x = 0; x < 10; x++)
            {
                EmulatorController.SendTap(1, 1);
            }
            byte[] crop = EmulatorController.CropImage(Script.image, new Point(176, 356), new Point(330, 611));
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
        }

        public Control[] CreateUI()
        {
            Label text = new Label();
            text.Text = "Default Script by PoH98";
            text.Width = 500;
            text.Name = "lbl";
            text.Location = new Point(10,10);
            text.TabIndex = 1000;
            RichTextBox txtBox = new RichTextBox();
            txtBox.Location = new Point(10, 40);
            txtBox.Height = 200;
            txtBox.Width = 510;
            txtBox.Text = "这个是默认的脚本战斗系统，将会自动应用到所有的战斗。如果想要自行创建脚本，请期待未来更新 CustomScript.dll 插件，或者到www.github.com/PoH98/Bot/了解如何自己创建脚本插件！";
            txtBox.ReadOnly = true;
            txtBox.TabIndex = 1001;
            txtBox.BackColor = Color.Black;
            txtBox.ForeColor = Color.Lime;
            txtBox.Font = new Font("Microsoft Sans Serif", 9.75F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            txtBox.Name = "Description";
            CheckBox chk = new CheckBox();
            chk.Text = "使用脚本";
            chk.Checked = true;
            chk.EnabledChanged += Chk_EnabledChanged;
            chk.Location = new Point(10,250);
            chk.AutoSize = true;
            if(PrivateVariable.BattleScript.Count == 1)
            {
                chk.Enabled = false;
            }
            Button Create = new Button();
            Create.Location = new Point(10, 270);
            Create.Click += Create_Click;
            Create.Text = "创建脚本 (C#语言）";
            Create.Width = 510;
            Create.Height = 50;

            Control[] thingsToReturn = { text, txtBox ,chk, Create};
            return thingsToReturn;
        }

        private void Create_Click(object sender, EventArgs e)
        {
            MessageBox.Show("网络上有挺多教学的，学会后再到www.github.com/PoH98/Bot/查看格式。\n打开编程软件\n导入这个exe到Reference\n增加using UI;\n在class后面增加:BattleScript\n根据Github教学编写\n包装dll\n丢到Battle_Script里面\n打开挂机就能看见你写的脚本被加载进入挂机内！", "如何编写C# dll库");

        }

        private void Chk_EnabledChanged(object sender, EventArgs e)
        {
            if((sender as CheckBox).Checked)
            {
                PrivateVariable.Selected_Script = PrivateVariable.BattleScript.IndexOf(this);
            }

        }

        public void ReadConfig()
        {
            
        }

        public string ScriptName()
        {
            return "Default Script";
        }
    }
}
