using BotFramework;
using Newtonsoft.Json.Linq;
using System.Drawing;
using System.Reflection;
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
                var api = wc.DownloadString("https://api.github.com/repos/PoH98/Valkyrie-Crusade-Bot/releases/latest");
                dynamic data = JObject.Parse(api);
                string latestversion = data["tag_name"].ToString();
                BufferUpdateText = data["body"].ToString().Replace("\r\n", "\n\n");
                if (Regex.Match(currentVersion.Replace(".", ""), @"\d+").Value != Regex.Match(latestversion.Replace(".", ""), @"\d+").Value)
                {
                    if (Variables.FindConfig("General", "AlertUpdate", out string output))
                    {
                        if (bool.Parse(output))
                        {
                            UpdateText = BufferUpdateText;
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
