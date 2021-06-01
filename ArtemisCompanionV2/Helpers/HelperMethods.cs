using Acr.UserDialogs;
using Android.Content.PM;
using System.Diagnostics;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ArtemisCompanionV2.Helpers
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public static class HelperMethods
    {
        public static async void ShowAlertKill(string message)
        {
            await Application.Current.MainPage.DisplayAlert("Error", message, "OK");
            Process.GetCurrentProcess().Kill();
        }

        public static void ShowToast(string message) =>
            Device.BeginInvokeOnMainThread(() =>
                UserDialogs.Instance.Toast(message));

        public static void ShowAlert(string message) =>
            Device.BeginInvokeOnMainThread(async () =>
                await Application.Current.MainPage.DisplayAlert("Artemis", message, "OK").ConfigureAwait(false));

        public static bool IsAppInstalled(string packageName)
        {
            var pm = Android.App.Application.Context.PackageManager;

            try
            {
                pm?.GetPackageInfo(packageName, PackageInfoFlags.Activities);
            }
            catch (PackageManager.NameNotFoundException)
            {
                return false;
            }

            return true;
        }
    }
}
