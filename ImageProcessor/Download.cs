using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BotFramework
{
    /// <summary>
    /// Download File UI
    /// </summary>
    public partial class Download : Form
    {
        /// <summary>
        /// Form for downloading data
        /// </summary>
        public Download()
        {
            InitializeComponent();
        }
        private static WebClient webClient;
        private void DownloadFile(string urlAddress, string location)
        {
            using (webClient = new WebClient())
            {
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
                webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);

                // The variable that will be holding the url address (making sure it starts with http://)
                Uri URL = new Uri(urlAddress);

                try
                {
                    // Start downloading the file
                    webClient.DownloadFileAsync(URL, location);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        // The event that will fire whenever the progress of the WebClient is changed
        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            try
            {
                progressBar1.Value = e.ProgressPercentage;

                // Show the percentage on our label.
                label1.Text = e.ProgressPercentage.ToString() + "%";

                // Update the label with how much data have been downloaded so far and the total size of the file we are currently downloading
                this.Text = string.Format("{0} MB's / {1} MB's",
                    (e.BytesReceived / 1024d / 1024d).ToString("0.00"),
                    (e.TotalBytesToReceive / 1024d / 1024d).ToString("0.00"));
            }
            catch
            {

            }
        }

        // The event that will trigger when the WebClient is completed
        private void Completed(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled == true)
            {
                MessageBox.Show("Download has been canceled.");
            }
            else
            {
                Close();
            }
        }
        /// <summary>
        /// Url and file save path
        /// </summary>
        public string url, path;

        private void Download_Shown(object sender, EventArgs e)
        {
            DllImport.EnableMenuItem(DllImport.GetSystemMenu(this.Handle, false), 0xF060, 1);
            Thread t = new Thread(() => DownloadFile(url,path));
            t.Start();
        }
    }
}
