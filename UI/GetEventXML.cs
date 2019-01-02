using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml;

namespace UI
{
    class GetEventXML
    {
        private static string url = "http://www-valkyriecrusade.nubee.com/";
        public static string Eventlink, RandomImage;

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
                    index++;
                }
                Eventlink = temp[newestindex].InnerText.Remove(temp[newestindex].InnerText.IndexOf(".html"));
                Random rnd = new Random();
                int imindex = rnd.Next(0, Imagelink.Count);
                RandomImage = Imagelink[imindex];
            }
            catch
            {
                MessageBox.Show("无法连接神女控活动页面，造成挂机探测不了活动资料，请确保网络连接完善后再启动挂机！");
                Environment.Exit(0);
            }
            
        }
    }
}
