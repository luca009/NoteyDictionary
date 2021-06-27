using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Net;

namespace NoteyDictionary
{
    /// <summary>
    /// Interaction logic for Downloader.xaml
    /// </summary>
    public partial class Downloader : Window
    {
        public Downloader(string downloadURL, string filePath)
        {
            InitializeComponent();

            Download(downloadURL, filePath);
        }

        private async void Download(string downloadURL, string filePath)
        {
            await Task.Run(() =>
            {
                using (WebClient webClient = new WebClient())
                {
                    webClient.DownloadFile(new Uri(downloadURL, UriKind.Absolute), filePath);
                }
            });
            this.Close();
        }
    }
}
