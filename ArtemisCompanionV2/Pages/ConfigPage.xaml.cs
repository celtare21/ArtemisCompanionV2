using System;
using System.IO;
using ArtemisCompanionV2.API;
using Plugin.FilePicker;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ArtemisCompanionV2.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ConfigPage
    {
        public static bool BlurEnabled { get; private set; }
        private static bool _flashValue;

        public ConfigPage() =>
            InitializeComponent();

        protected override void OnAppearing()
        {
            DnsPick.Title = "DNS Profile";
            DnsPick.Margin = new Thickness(33, 15, 20, 0);
            DnsPick.SelectedIndex = Convert.ToInt32(File.ReadAllText(FileHandler.DnsFile));

            FlashToggle.IsToggled = Convert.ToBoolean(Convert.ToInt32(File.ReadAllText(FileHandler.FlashFile)));
            BlurToggle.IsToggled = Convert.ToBoolean(Convert.ToInt32(File.ReadAllText(FileHandler.BlurFile)));

            BootImage.Text = File.Exists(FileHandler.BootFile) ? "Boot.img found!" : "File not found!";

            try
            {
                DependencyService.Get<IListener>().TestSu();
            }
            catch
            {
                KillSuButton.IsEnabled = false;
                StartSuButton.IsEnabled = true;
            }

            FlashToggle.IsToggled = _flashValue;
        }

        private async void DnsPick_OnSelectedIndexChanged(object sender, EventArgs e) =>
            await File.WriteAllTextAsync(FileHandler.DnsFile, DnsPick.SelectedIndex.ToString());

        private async void FlashToggle_OnToggled(object sender, ToggledEventArgs e)
        {
            if (FlashToggle.IsToggled && !File.Exists(FileHandler.BootFile))
            {
                FlashToggle.IsToggled = false;
                await DisplayAlert("Error", "Please choose a file before toggling this on!", "Exit");
                return;
            }

            await File.WriteAllTextAsync(FileHandler.FlashFile, Convert.ToInt32(e.Value).ToString());

            if (e.Value)
                Helpers.HelperMethods.ShowToast("Please reboot now!");
        }

        private async void PickButton_OnPressed(object sender, EventArgs e)
        {
            var userFile = await CrossFilePicker.Current.PickFile();

            if (userFile == null)
                return;

            var filePath = userFile.FilePath;

            if (filePath.Contains("content://"))
            {
                LabelFilePicker.Text = "No file selected!";
                await DisplayAlert("Error!", "Please select a file from internal storage!", "OK");
                return;
            }

            if (!userFile.FileName.Contains(".img"))
            {
                LabelFilePicker.Text = "No file selected!";
                await DisplayAlert("Error!", "Please select a file with the .img extention!", "OK");
                return;
            }

            File.Delete(FileHandler.BootFile);
            File.Copy(filePath, FileHandler.BootFile);

            LabelFilePicker.Text = "File selected!";
            BootImage.Text = "Boot.img found!";
        }

        private async void BlurToggle_OnToggled(object sender, ToggledEventArgs e)
        {
            if (BackupPage.BackupWorkEnabled)
            {
                await DisplayAlert("Error", "Can't enable blue while backup work is queued!", "Ok");
                return;
            }

            await File.WriteAllTextAsync(FileHandler.BlurFile, Convert.ToInt32(e.Value).ToString());
            BlurEnabled = e.Value;
        }

        private void StartSuButton_OnPressed(object sender, EventArgs e)
        {
            DependencyService.Get<IListener>().StartSu();

            StartSuButton.IsEnabled = false;
            KillSuButton.IsEnabled = true;
        }

        private void KillSuButton_OnPressed(object sender, EventArgs e)
        {
            DependencyService.Get<IListener>().KillSu();

            KillSuButton.IsEnabled = false;
            StartSuButton.IsEnabled = true;
        }

        public static async void FlashOn()
        {
            await File.WriteAllTextAsync(FileHandler.FlashFile, "1");

            _flashValue = true;

            await Application.Current.MainPage.DisplayAlert("Artemis", "Please reboot!", "OK");
        }
    }
}