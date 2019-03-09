using System;
using System.IO.Compression;
using System.IO;
using System.Net;
using UI;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

namespace Installer
{
    class Program
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        const int SW_SHOW = 5;
        static void Main(string[] args)
        {
            var handle = GetConsoleWindow(); 
            if (args.Length > 0)
            {
               
                try
                {
                    if (args[0].Contains("exe"))
                    {
                        ShowWindow(handle, SW_SHOW);
                        Console.WriteLine("Process Downloading...");
                        var result = Downloader.Download("http://dl.memuplay.com/download/backup/Memu-Setup-3.7.0.0.exe", Environment.CurrentDirectory, 8);
                        Console.WriteLine("Completed! Used time " + result.TimeTaken);
                    }

                    else
                    {
                        WebClientOverride wc = new WebClientOverride();
                        wc.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/60.0.3112.113 Safari/537.36");
                        Regex pattern = new Regex("[\n\t\r ]");
                        string newest = pattern.Replace(wc.DownloadString("https://raw.githubusercontent.com/PoH98/Bot/master/version.txt"), "");
                        Console.WriteLine("The online version is " + newest);
                        if (newest != args[0])
                        {
                            ShowWindow(handle, SW_SHOW);
                            Console.WriteLine("Process Downloading...");
                            var result = Downloader.Download("https://github.com/PoH98/Bot/releases/download/v" + newest + "/default.exe", Environment.CurrentDirectory, 8);
                            Console.WriteLine("Completed! Used time " + result.TimeTaken);
                        }
                    }
                }
                catch
                {

                }
            }
                
        }
    }
}
