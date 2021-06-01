using ArtemisCompanionV2.API;
using System;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms.Xaml;

// ReSharper disable InvertIf

namespace ArtemisCompanionV2.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BackupPage
    {
        public static bool BackupWorkEnabled { get; private set; }

        public BackupPage() =>
            InitializeComponent();

        protected override void OnAppearing()
        {
            base.OnAppearing();

            TermuxInstalled.Text = Helpers.HelperMethods.IsAppInstalled("com.termux") ? "App installed!" : "App not installed!";

            try
            {
                Status.Text = File.ReadAllText(FileHandler.StatusFile) switch
                {
                    "1" => "Completed",
                    "-1" => "Failure",
                    var _ => Status.Text
                };
            }
            catch (FileNotFoundException)
            {
                Status.Text = "Idle";
            }
        }

        private async void BackupButton_OnClicked(object sender, EventArgs e)
        {
            if (!await BackupChecksPass())
                return;

            File.Delete(FileHandler.StatusFile);

            await File.WriteAllTextAsync(FileHandler.BackupPass, PassEntry.Text);
            await File.WriteAllTextAsync(FileHandler.BackupFile, "1");
            Status.Text = "Backup";

            BackupWorkEnabled = true;

            Helpers.HelperMethods.ShowToast("Please reboot your phone! Come back to see if the status is successful.");
        }

        private async void RestoreButton_OnClicked(object sender, EventArgs e)
        {
            if (!await BackupChecksPass())
                return;

            File.Delete(FileHandler.StatusFile);

            await File.WriteAllTextAsync(FileHandler.BackupPass, PassEntry.Text);
            await File.WriteAllTextAsync(FileHandler.BackupFile, "2");

            Status.Text = "Restore";

            BackupWorkEnabled = true;

            Helpers.HelperMethods.ShowToast("Please reboot your phone! Come back to see if the status is successful.");
        }

        private async Task<bool> BackupChecksPass()
        {
            if (ConfigPage.BlurEnabled)
            {
                await DisplayAlert("Error", "Please disable blur before using this feature!", "Exit");
                return false;
            }

            if (!Helpers.HelperMethods.IsAppInstalled("com.termux"))
            {
                await DisplayAlert("Error", "Please install Termux!", "Exit");
                return false;
            }

            if (DepsCheck.IsChecked == false)
            {
                await DisplayAlert("Error", "Please make sure you downloaded the termux deps and checked the box!", "Exit");
                return false;
            }

            if (PassEntry.Text == null)
            {
                await DisplayAlert("Error", "Please enter a password!", "Exit");
                return false;
            }

            if (PassEntry.Text != PassEntryConfirm.Text)
            {
                await DisplayAlert("Error", "Please check your passwords!", "Exit");
                return false;
            }

            return true;
        }
    }
}