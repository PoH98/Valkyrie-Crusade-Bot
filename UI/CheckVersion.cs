using BotFramework;
using Newtonsoft.Json.Linq;
using System.Drawing;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace UI
{
    class CheckVersion
    {
        public static string UpdateText;

        public static string BufferUpdateText;

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
                BufferUpdateText = data["body"].ToString().Replace("\r\n", "\n\n");
                JObject assets = JObject.Parse(data["assets"][0].ToString());
                string download = assets["browser_download_url"].ToString();
                if (Regex.Match(currentVersion.Replace(".",""),@"\d+").Value != Regex.Match(latestversion.Replace(".", ""), @"\d+").Value)
                {
                    if (Variables.FindConfig("General", "AlertUpdate", out string output))
                    {
                        if (bool.Parse(output))
                        {
                            UpdateText = BufferUpdateText + "<br><hr><a href=\""+download+"\">Download</a>";
                        }
                    }
                    else
                    {
                        Variables.ModifyConfig("General", "AlertUpdate", "true");
                        UpdateText = BufferUpdateText;
                    }
                }
            }
            catch
            {

            }
            
        }
    }
}
