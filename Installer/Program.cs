using System;
using System.IO.Compression;
using System.IO;
using System.Net;
using UI;
using System.Windows.Forms;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

namespace Installer
{
    class Program
    {
        static long file;
        static double percentage;
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 4;

        static void Main(string[] args)
        {
            var handle = GetConsoleWindow();
            Console.CursorVisible = false;
            if (args.Length > 0)
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                Console.WriteLine("QQ: 809233738");
                try
                {
                    if (args[0].Contains("apk"))
                    {
                        ShowWindow(handle, SW_SHOW);
                        Console.WriteLine("Downloading Apk...");
                        WebClientOverride wc = new WebClientOverride();
                        wc.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/60.0.3112.113 Safari/537.36");
                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(args[0]);
                        // MAGIC LINE GOES HERE \/
                        request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                        using (var strLocal = new FileStream("ValkyrieCrusade.apk", FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            // Assign the response object of HttpWebRequest to a HttpWebResponse variable.
                            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                            {
                                long fileSize = Convert.ToInt64(response.Headers["Content-Length"]);
                                using (Stream streamResponse = response.GetResponseStream())
                                {
                                    int bytesSize = 0;
                                    // A buffer for storing and writing the data retrieved from the server
                                    byte[] downBuffer = new byte[2048];

                                    // Loop through the buffer until the buffer is empty
                                    while ((bytesSize = streamResponse.Read(downBuffer, 0, downBuffer.Length)) > 0)
                                    {
                                        // Write the data from the buffer to the local hard drive
                                        strLocal.Write(downBuffer, 0, bytesSize);
                                        UpdateProgress(strLocal.Length, fileSize);
                                        Console.WriteLine("Downloaded " + percentage + " %");
                                        if (percentage != 100)
                                        {
                                            Console.SetCursorPosition(0, Console.CursorTop - 1);
                                        }
                                    }
                                }
                                MessageBox.Show("下载完毕！");
                            }

                        }
                    }
                    else if (args[0].Contains("exe"))
                    {
                        ShowWindow(handle, SW_SHOW);
                        Console.WriteLine("Downloading MEmu...");
                        WebClientOverride wc = new WebClientOverride();
                        wc.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/60.0.3112.113 Safari/537.36");
                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(args[0]);
                        // MAGIC LINE GOES HERE \/
                        request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                        using (var strLocal = new FileStream("MEmu.exe", FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            // Assign the response object of HttpWebRequest to a HttpWebResponse variable.
                            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                            {
                                long fileSize = Convert.ToInt64(response.Headers["Content-Length"]);
                                using (Stream streamResponse = response.GetResponseStream())
                                {
                                    int bytesSize = 0;
                                    // A buffer for storing and writing the data retrieved from the server
                                    byte[] downBuffer = new byte[2048];

                                    // Loop through the buffer until the buffer is empty
                                    while ((bytesSize = streamResponse.Read(downBuffer, 0, downBuffer.Length)) > 0)
                                    {
                                        // Write the data from the buffer to the local hard drive
                                        strLocal.Write(downBuffer, 0, bytesSize);
                                        UpdateProgress(strLocal.Length, fileSize);
                                        Console.WriteLine("Downloaded " + percentage + " %");
                                        if (percentage != 100)
                                        {
                                            Console.SetCursorPosition(0, Console.CursorTop - 1);
                                        }
                                    }
                                }
                                foreach (var p in Process.GetProcessesByName("神女控强力挂机"))
                                {
                                    p.Kill();
                                }
                                Process.Start("temp.exe");
                            }

                        }
                    }
                    else
                    {
                        Console.WriteLine("Checking Updates");
                        WebClientOverride wc = new WebClientOverride();
                        wc.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/60.0.3112.113 Safari/537.36");
                        Regex pattern = new Regex("[\n\t\r ]");
                        string newest = pattern.Replace(wc.DownloadString("https://raw.githubusercontent.com/PoH98/Bot/master/version.txt"), "");
                        Console.WriteLine("The online version is " + newest);
                        if (newest != args[0])
                        {
                            ShowWindow(handle, SW_SHOW);
                            Console.WriteLine("Newest Version Found! Downloading...");
                            Uri url = new Uri(@"https://github.com/PoH98/Bot/releases/download/v" + newest + "/default.exe");

                            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

                            // MAGIC LINE GOES HERE \/
                            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                            using (var strLocal = new FileStream("temp.exe", FileMode.Create, FileAccess.Write, FileShare.None))
                            {
                                // Assign the response object of HttpWebRequest to a HttpWebResponse variable.
                                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                                {
                                    long fileSize = Convert.ToInt64(response.Headers["Content-Length"]);
                                    using (Stream streamResponse = response.GetResponseStream())
                                    {
                                        int bytesSize = 0;
                                        // A buffer for storing and writing the data retrieved from the server
                                        byte[] downBuffer = new byte[2048];

                                        // Loop through the buffer until the buffer is empty
                                        while ((bytesSize = streamResponse.Read(downBuffer, 0, downBuffer.Length)) > 0)
                                        {
                                            // Write the data from the buffer to the local hard drive
                                            strLocal.Write(downBuffer, 0, bytesSize);
                                            UpdateProgress(strLocal.Length, fileSize);
                                            Console.WriteLine("Downloaded " + percentage + " %");
                                            if (percentage != 100)
                                            {
                                                Console.SetCursorPosition(0, Console.CursorTop - 1);
                                            }
                                        }
                                    }
                                    foreach (var p in Process.GetProcessesByName("神女控强力挂机"))
                                    {
                                        p.Kill();
                                    }
                                    Process.Start("temp.exe");
                                }
                            }
                        }

                    }
                }
                catch
                {

                }
            }
                
        }

        private static void UpdateProgress(Int64 BytesRead, Int64 TotalBytes)
        {
            // Calculate the download progress in percentages
            percentage = (BytesRead * 100) / TotalBytes;
            if (BytesRead == TotalBytes)
            {
                file = 0;
            }
            else
            {
                file = BytesRead;
            }
        }
    }
}
