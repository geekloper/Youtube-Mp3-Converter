using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Reflection;
using System.Threading;

/**********************************************
 * AUTHOR : ABDELLAH DERFOUFI 
 * 
 * GITHUB : http://github.com/geekloper
 **********************************************
 */

namespace Youtube_Mp3
{
    public partial class Main_Form : Form
    {

        #region Members
            private SaveFileDialog saveFileDialog1;
            private WebClient webClient;
            private DateTime lastUpdate;
            private long lastBytes = 0;
        #endregion

        public Main_Form()
        {
            InitializeComponent();

            WebBrowser1.Navigate("http://www.youtube-mp3.org/");
            WebBrowser1.ScriptErrorsSuppressed = true;

            webClient = new WebClient();
            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadFileCompleted);
            webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);

        }

        private void Down_Click(object sender, EventArgs e)
        {
            try
            {
                WebBrowser1.Document.GetElementById("youtube-url").SetAttribute("value", TextBox1.Text);
                WebBrowser1.Document.GetElementById("submit").InvokeMember("click");

                Stream myStream;

                HtmlElement download_link = WebBrowser1.Document.GetElementById("dl_link");

                HtmlElementCollection links = download_link.GetElementsByTagName("a");

                HtmlElement error = WebBrowser1.Document.GetElementById("error_text");

                if (links.Count <= 0 && this.WebBrowser1.ReadyState == WebBrowserReadyState.Complete)
                {
                    throw new Exception("Il est survenu une erreur depuis le site YouTube, nous ne pouvons pas traiter cette vidéo!\nLa plupart du temps, cette erreur provient de l’existence de droits réservés pour la vidéo ou bien de sa longueur.\nNous ne pouvons prendre en charge que des vidéos d’une durée maximum de 20 minutes.");
                }
                else
                {
                    for (int i = 0; i < links.Count; i++)
                    {
                        if (links[i].Style == null)
                        {
                            saveFileDialog1 = new SaveFileDialog();
                            saveFileDialog1.RestoreDirectory = true;

                            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                            {
                                if ((myStream = saveFileDialog1.OpenFile()) != null)
                                {
                                    myStream.Close();
                                    webClient.DownloadFileAsync(new Uri(links[i].GetAttribute("href")), saveFileDialog1.FileName + ".mp3");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
                File.Delete(saveFileDialog1.FileName);
                MessageBox.Show("Votre fichier a été téléchargé !","Félicitation !",MessageBoxButtons.OK,MessageBoxIcon.Asterisk);
                progressBar1.Value = 0;
                label1.Text = "";
        }

        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
            label1.Text = e.ProgressPercentage.ToString() + "%";

            long bytes = e.BytesReceived;

            if (lastBytes == 0)
            {
                lastUpdate = DateTime.Now;
                lastBytes = bytes;
                return;
            }

            var now = DateTime.Now;
            var timeSpan = now - lastUpdate;
            if (timeSpan.Seconds == 0)
            {
                return;
            }
            var bytesChange = bytes - lastBytes;
            var bytesPerSecond = bytesChange / timeSpan.Seconds;

            lastBytes = bytes;
            lastUpdate = now;

            label3.Text = bytesPerSecond.ToString();

        }

        private void WebBrowser1_NewWindow(object sender, CancelEventArgs e)
        {
            WebBrowser1.Navigate(WebBrowser1.StatusText);
            e.Cancel = true;
        }

        private void WebBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            WebBrowser1.Document.Window.Error += new HtmlElementErrorEventHandler(Window_Error);
        }

        private void Window_Error(object sender,HtmlElementErrorEventArgs e)
        {
            MessageBox.Show("Erreur !");
            e.Handled = true;
        }
    }
}
