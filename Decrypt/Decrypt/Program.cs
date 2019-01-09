using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Decrypt
{
    class Program
    {
        static void Main(string[] args)
        {
            foreach(var file in Directory.GetFiles(Environment.CurrentDirectory, "*.log"))
            {
                File.WriteAllText(file.Replace(".log", ".txt"), "");
                using (StreamWriter s = File.AppendText(file.Replace(".log", ".txt")))
                {
                    var lines = File.ReadAllLines(file);
                    foreach (var l in lines)
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (char c in l)
                        {
                                char y = (char)(Convert.ToUInt16(c) - 14);
                                sb.Append(y);
                        }
                        string newline = sb.ToString();
                        s.WriteLine(newline);
                    }
                }
                
            }
        }
    }
}
