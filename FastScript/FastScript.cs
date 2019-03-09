using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageProcessor;
using UI;
using System.Drawing;
using ImgXml;

using System.Windows.Forms;
using System.IO;

namespace FastScript
{
    public class FastScript : BattleScript
    {
        public void Attack()
        {
            for (int x = 0; x < 10; x++)
            {
                EmulatorController.SendTap(1, 1);
            }
            if (PrivateVariable.Battling == false)
            {
                return;
            }
            EmulatorController.SendTap(new Point(263, 473));
            for (int x = 0; x < 5; x++)
            {
                EmulatorController.SendTap(1, 1);
            }
            Point? p = EmulatorController.FindImage(Script.image, Img.GreenButton,true);
            if(p != null)
            {
                EmulatorController.SendTap(p.Value);
            }
            if (PrivateVariable.Battling == false)
            {
                return;
            }
            EmulatorController.SendTap(new Point(448, 492));
            for (int x = 0; x < 5; x++)
            {
                EmulatorController.SendTap(1, 1);
            }
            p = EmulatorController.FindImage(Script.image, Img.GreenButton, true);
            if (p != null)
            {
                EmulatorController.SendTap(p.Value);
            }
            if (PrivateVariable.Battling == false)
            {
                return;
            }
            EmulatorController.SendTap(new Point(641, 473));
            for (int x = 0; x < 5; x++)
            {
                EmulatorController.SendTap(1, 1);
            }
            p = EmulatorController.FindImage(Script.image, Img.GreenButton, true);
            if (p != null)
            {
                EmulatorController.SendTap(p.Value);
            }
            if (PrivateVariable.Battling == false)
            {
                return;
            }
            EmulatorController.SendTap(new Point(834, 483));
            for (int x = 0; x < 5; x++)
            {
                EmulatorController.SendTap(1, 1);
            }
            p = EmulatorController.FindImage(Script.image, Img.GreenButton, true);
            if (p != null)
            {
                EmulatorController.SendTap(p.Value);
            }
            if (PrivateVariable.Battling == false)
            {
                return;
            }
            EmulatorController.SendTap(new Point(1017, 470));
            for (int x = 0; x < 5; x++)
            {
                EmulatorController.SendTap(1, 1);
            }
            p = EmulatorController.FindImage(Script.image, Img.GreenButton, true);
            if (p != null)
            {
                EmulatorController.SendTap(p.Value);
            }
        }

        public Control[] CreateUI()
        {
            Label text = new Label();
            text.Text = "Default Script by PoH98";
            text.AutoSize = true;
            text.Name = "lbl";
            text.Location = new Point(10, 10);
            text.TabIndex = 1000;
            RichTextBox txtBox = new RichTextBox();
            txtBox.Location = new Point(10, 40);
            txtBox.Height = 200;
            txtBox.Width = 400;
            txtBox.Text = "这个是默认的脚本战斗系统，将会自动应用到所有的战斗。如果想要自行创建脚本，请期待未来更新 CustomScript.dll 插件，或者到www.github.com/PoH98/Bot/了解如何自己创建脚本插件！";
            txtBox.ReadOnly = true;
            txtBox.TabIndex = 1001;
            txtBox.BackColor = Color.Black;
            txtBox.ForeColor = Color.Lime;
            txtBox.Font = new Font("Microsoft Sans Serif", 9.75F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            txtBox.Name = "Description";
            Button Create = new Button();
            Create.Location = new Point(10, 370);
            Create.Click += Create_Click;
            Create.Text = "创建脚本 (C#语言）";
            Create.Width = 380;
            Create.Height = 50;
            Control[] thingsToReturn = { text, txtBox, Create };
            return thingsToReturn;
        }
        private void Create_Click(object sender, EventArgs e)
        {
            MessageBox.Show("网络上有挺多教学的，学会后再到www.github.com/PoH98/Bot/查看格式。\n打开编程软件\n导入这个exe到Reference\n增加using UI;\n在class后面增加:BattleScript\n根据Github教学编写\n包装dll\n丢到Battle_Script里面\n打开挂机就能看见你写的脚本被加载进入挂机内！", "如何编写C# dll库");
        }

        public void ReadConfig()
        {

        }

        public string ScriptName()
        {
            return "快速脚本（测试版）";
        }
    }
}
