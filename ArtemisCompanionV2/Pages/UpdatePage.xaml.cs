using ArtemisCompanionV2.API;
using System;
using System.IO;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ArtemisCompanionV2.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class UpdatePage
    {
        public static string ImgPath { get; } = Path.Combine(Xamarin.Essentials.FileSystem.AppDataDirectory, "boot.img");

        public UpdatePage() =>
            InitializeComponent();

        private async void DownloadButton_OnPressed(object sender, EventArgs e)
        {
            var downloader = DependencyService.Get<IDownloader>();

            DownloadButton.IsEnabled = false;

            Helpers.HelperMethods.ShowToast("File started downloading! Please don't exit the app!");

            File.Delete(ImgPath);

            var urlBuilder = new StringBuilder("https://0x0.st/");
            urlBuilder.Append(PinEntry.Text);
            urlBuilder.Append(".img");
            try
            {
                downloader.DownloadFile(urlBuilder.ToString(), ImgPath);
            }
            catch
            {
                await Application.Current.MainPage.DisplayAlert("Error", "File couldn't be downloaded!", "Exit");
                DownloadButton.IsEnabled = true;
            }
        }
    }
}