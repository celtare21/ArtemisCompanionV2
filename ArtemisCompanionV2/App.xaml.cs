using Acr.UserDialogs;
using ArtemisCompanionV2.API;
using ArtemisCompanionV2.Helpers;
using ArtemisCompanionV2.Pages;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using PermissionStatus = Plugin.Permissions.Abstractions.PermissionStatus;

namespace ArtemisCompanionV2
{
    public partial class App
    {
        public App()
        {
            InitializeComponent();

            MainPage = new MainPage();
        }

        protected override async void OnStart()
        {
            await CheckPermissionsAsync();
            await WaitDriverAsync();
            await FirstLaunchInfo();
            DependencyService.Get<IDownloader>().OnFileDownloaded += App_OnFileDownloaded;
        }

        protected override void OnSleep() =>
            DependencyService.Get<IDownloader>().OnFileDownloaded -= App_OnFileDownloaded;

        protected override void OnResume() =>
            DependencyService.Get<IDownloader>().OnFileDownloaded += App_OnFileDownloaded;

        private static async Task WaitDriverAsync()
        {
            UserDialogs.Instance.ShowLoading("Please Wait...");

            while (!Directory.Exists("/proc/userland/"))
                await Task.Delay(500);

            UserDialogs.Instance.HideLoading();
        }

        private static async Task CheckPermissionsAsync()
        {
            var permissionStatus = await CrossPermissions.Current.CheckPermissionStatusAsync<StoragePermission>();

            if (permissionStatus != PermissionStatus.Granted)
            {
                var shouldRequest = await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Permission.Storage);

                if (shouldRequest || VersionTracking.IsFirstLaunchEver)
                    permissionStatus = await CrossPermissions.Current.RequestPermissionAsync<StoragePermission>();

                if (permissionStatus != PermissionStatus.Granted)
                    HelperMethods.ShowAlertKill("Permissions weren't granted!");
            }
        }

        private static async Task FirstLaunchInfo()
        {
            if (!VersionTracking.IsFirstLaunchEver)
                return;

            await Current.MainPage.DisplayAlert("Artemis",
                "Please read the 'About Features' section before doing anything!", "OK");
            await Current.MainPage.DisplayAlert("Artemis",
                "Please reboot after you make a change to take effect.", "OK").ConfigureAwait(false);
        }

        private static void App_OnFileDownloaded(object sender, DownloadEventArgs e)
        {
            if (e.FileSaved)
            {
                ConfigPage.FlashOn();
                HelperMethods.ShowAlert("File downloaded succesfully!");
            }
            else
            {
                HelperMethods.ShowAlert("There's been an error while downloading the file!");
            }
        }
    }
}
