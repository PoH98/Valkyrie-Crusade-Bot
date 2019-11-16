using System;
using System.Collections.Generic;
using System.Net;
using System.Windows.Forms;
using System.Xml;

namespace UI
{
    class GetEventXML
    {
        private static string url = "http://www-valkyriecrusade.nubee.com/";
        public static string Eventlink, RandomImage, GuildwarLink;
        public static DateTime guildwar = DateTime.MinValue;
 
        public static void LoadXMLEvent()
        {
            List<string> Imagelink = new List<string>();
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(url);
                //get a list of the Definition nodes in the document
                var temp = doc.GetElementsByTagName("Contents");
                DateTime newest = DateTime.MinValue;
                DateTime banner = DateTime.MinValue;
                int index = 0, newestindex = 0;
                foreach (XmlNode n in temp)
                {
                    if (n.InnerText.Contains("_event.html"))
                    {
                        var date = Convert.ToDateTime(n.InnerXml.Substring(n.InnerXml.IndexOf("<LastModified xmlns=\"http://s3.amazonaws.com/doc/2006-03-01/\">")).Replace("<LastModified xmlns=\"http://s3.amazonaws.com/doc/2006-03-01/\">", "").Remove(10));
                        if (newest < date)
                        {
                            newest = date;
                            newestindex = index;
                        }
                    }
                    else if (n.InnerText.Contains("img/info_card"))
                    {
                        Imagelink.Add(n.InnerText.Substring(n.InnerText.IndexOf("event")).Remove(n.InnerText.IndexOf(".png")) + ".png");
                    }
                    else if (n.InnerText.Contains("GuildBattle"))
                    {
                        var date = Convert.ToDateTime(n.InnerXml.Substring(n.InnerXml.IndexOf("<LastModified xmlns=\"http://s3.amazonaws.com/doc/2006-03-01/\">")).Replace("<LastModified xmlns=\"http://s3.amazonaws.com/doc/2006-03-01/\">", "").Remove(10));
                        if (guildwar < date)
                        {
                            GuildwarLink = n.InnerText.Remove(n.InnerText.IndexOf(".html"));
                            guildwar = date;
                        }
                    }
                    index++;
                }
                var eventlink = temp[newestindex].InnerText.Remove(temp[newestindex].InnerText.IndexOf(".html"));
                int eventnum = 0;
                while (true)
                {
                    try
                    {
                        string convert = eventlink.Substring(eventlink.LastIndexOf('/')).Replace("_event", "").Replace("/", "");
                        eventnum = Convert.ToInt32(convert);
                        eventnum = eventnum +1;
                        eventlink = eventlink.Remove(eventlink.LastIndexOf('/')) +"/"+ eventnum + "_event";
                        HttpWebRequest request = HttpWebRequest.Create(url + eventlink + ".html") as HttpWebRequest;
                        HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                        //Returns TRUE if the Status code == 200
                        response.Close();
                    }
                    catch
                    {
                        string convert = eventlink.Substring(eventlink.LastIndexOf('/')).Replace("_event", "").Replace("/", "");
                        eventnum = Convert.ToInt32(convert);
                        eventnum = eventnum - 1;
                        eventlink = eventlink.Remove(eventlink.LastIndexOf('/')) + "/" + eventnum + "_event";
                        Eventlink = eventlink;
                        break;
                    }
                }
                Random rnd = new Random();
                int imindex = rnd.Next(0, Imagelink.Count);
                RandomImage = Imagelink[imindex];
            }
            catch
            {
                Eventlink = "";
            }
            
        }
    }
}
