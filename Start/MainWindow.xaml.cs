using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Start
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MouseDown += Window_MouseDown;
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }
        private void 逍遥模拟器_Click(object sender, RoutedEventArgs e)
        {
            var dlls = Directory.GetFiles("Emulators");
            foreach (var dll in dlls)
            {
                if (!dll.Contains("MEmu"))
                {
                    File.Move(dll, dll.Replace(".dll", ".old"));
                }
                else
                {
                    File.Move(dll, dll.Replace(".old", ".dll"));
                }
            }
            Process.Start("VCweiyi.exe");
        }

        private void 夜神模拟器_Click(object sender, RoutedEventArgs e)
        {
            var dlls = Directory.GetFiles("Emulators");
            foreach (var dll in dlls)
            {
                if (!dll.Contains("Nox"))
                {
                    File.Move(dll, dll.Replace(".dll", ".old"));
                }
                else
                {
                    File.Move(dll, dll.Replace(".old", ".dll"));
                }
            }
            Process.Start("VCweiyi.exe");
        }

        private void 自动判断_Click(object sender, RoutedEventArgs e)
        {
            var dlls = Directory.GetFiles("Emulators");
            foreach (var dll in dlls)
            {
                File.Move(dll, dll.Replace(".old", ".dll"));
            }
            Process.Start("VCweiyi.exe");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void 木木模拟器_Click(object sender, RoutedEventArgs e)
        {
            var dlls = Directory.GetFiles("Emulators");
            foreach (var dll in dlls)
            {
                if (!dll.Contains("Nemu"))
                {
                    File.Move(dll, dll.Replace(".dll", ".old"));
                }
                else
                {
                    File.Move(dll, dll.Replace(".old", ".dll"));
                }
            }
            Process.Start("VCweiyi.exe");
        }
    }
}
