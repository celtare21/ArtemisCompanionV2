using System;
using Xamarin.Forms.Xaml;

namespace ArtemisCompanionV2.API
{
    public interface IDownloader
    {
        void DownloadFile(string url, string folder);
        event EventHandler<DownloadEventArgs> OnFileDownloaded;
    }

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class DownloadEventArgs : EventArgs
    {
        public readonly bool FileSaved;

        public DownloadEventArgs(bool fileSaved) =>
            FileSaved = fileSaved;
    }
}
