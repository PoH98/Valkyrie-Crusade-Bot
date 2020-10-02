using BotFramework;
using Newtonsoft.Json.Linq;
using System.Drawing;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace UI
{
    class CheckVersion
    {
        public static string UpdateText;

        public static string[] BufferUpdateText;

        public static string currentVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public static void CheckUpdate()
        {
            try
            {
                WebClientOverride wc = new WebClientOverride();
                var rawdata = wc.DownloadData("https://api.github.com/repos/PoH98/Valkyrie-Crusade-Bot/releases/latest");
                var api = Encoding.UTF8.GetString(rawdata);
                JObject data = JObject.Parse(api);
                string latestversion = data["tag_name"].ToString();
                BufferUpdateText = data["body"].ToString().Split('\n');
                JObject assets = JObject.Parse(data["assets"][0].ToString());
                string download = assets["browser_download_url"].ToString();
                if (Regex.Match(currentVersion.Replace(".",""),@"\d+").Value != Regex.Match(latestversion.Replace(".", ""), @"\d+").Value)
                {
                    if (Variables.FindConfig("General", "AlertUpdate", out string output))
                    {
                        if (bool.Parse(output))
                        {
                            string formatedhtml = "";
                            foreach(var line in BufferUpdateText)
                            {
                                if(line.Contains("# "))
                                {
                                    formatedhtml += line.Replace("# ", "<h2>") + "</h2>";
                                }
                                else
                                {
                                    formatedhtml += line.Replace("*","<li>")+"</li><br>";
                                }
                            }
                            UpdateText = Properties.Resources.html.Replace("<!data!>", formatedhtml).Replace("<!download!>", "<a href=\"" + download + "\">Download</a>");
                        }
                    }
                    else
                    {
                        Variables.ModifyConfig("General", "AlertUpdate", "true");
                        string formatedhtml = "";
                        foreach (var line in BufferUpdateText)
                        {
                            if (line.Contains("# "))
                            {
                                formatedhtml += line.Replace("# ", "<h2>") + "</h2>";
                            }
                            else
                            {
                                formatedhtml += line.Replace("*", "<li>") + "</li><br>";
                            }
                        }
                        UpdateText = Properties.Resources.html.Replace("<!data!>", formatedhtml).Replace("<!download!>", "<a href=\"" + download + "\">Download</a>");

                    }
                }
            }
            catch
            {

            }
            
        }
    }
}
