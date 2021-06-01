using ArtemisCompanionV2.API;
using System;
using System.ComponentModel;
using System.Net;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: Dependency(typeof(AndroidDownloader))]

namespace ArtemisCompanionV2.API
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class AndroidDownloader : IDownloader
    {
        public event EventHandler<DownloadEventArgs> OnFileDownloaded;

        public void DownloadFile(string url, string path)
        {
            try
            {
                WebClient webClient = new WebClient();

                webClient.DownloadFileCompleted += Completed;
                webClient.DownloadFileAsync(new Uri(url), path);
            }
            catch
            {
                try
                {
                    OnFileDownloaded?.Invoke(this, new DownloadEventArgs(false));
                }
                catch
                {
                    Helpers.HelperMethods.ShowAlert("Couldn't download file!");
                }
            }
        }

        private void Completed(object sender, AsyncCompletedEventArgs e)
        {
            try
            {
                OnFileDownloaded?.Invoke(this,
                    e.Error != null ? new DownloadEventArgs(false) : new DownloadEventArgs(true));
            }
            catch
            {
                Helpers.HelperMethods.ShowAlert("Couldn't download file!");
            }
        }
    }
}