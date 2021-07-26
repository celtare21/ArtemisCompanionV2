using System;
using System.IO;
using System.Threading.Tasks;
using ArtemisCompanionV2.API;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ArtemisCompanionV2.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ConfigPage
    {
        public static bool BlurEnabled { get; private set; }
        public static bool IsSuEnabled { get; private set; } = true;

        public ConfigPage() =>
            InitializeComponent();

        protected override void OnAppearing()
        {
            DnsPick.Title = "DNS Profile";
            DnsPick.Margin = new Thickness(33, 15, 20, 0);
            DnsPick.SelectedIndex = Convert.ToInt32(File.ReadAllText(FileHandler.DnsFile));

            BlurToggle.IsToggled = Convert.ToBoolean(Convert.ToInt32(File.ReadAllText(FileHandler.BlurFile)));

            try
            {
                DependencyService.Get<ISuHandler>().TestSu();
            }
            catch
            {
                KillSuButton.IsEnabled = false;
                StartSuButton.IsEnabled = true;
                IsSuEnabled = false;
            }
        }

        private async void DnsPick_OnSelectedIndexChanged(object sender, EventArgs e) =>
            await File.WriteAllTextAsync(FileHandler.DnsFile, DnsPick.SelectedIndex.ToString()).ConfigureAwait(false);

        private async void PickButton_OnPressed(object sender, EventArgs e)
        {
            var fileResult = await FilePicker.PickAsync();

            if (fileResult == null)
                return;

            var filePath = fileResult.FullPath;

            if (!fileResult.FileName.Contains(".img"))
            {
                await DisplayAlert("Error!", "Please select a file with the .img extention!", "OK");
                return;
            }

            File.Delete(FileHandler.BootFile);
            File.Copy(filePath, FileHandler.BootFile);

            await FlashAndRebootTask().ConfigureAwait(false);
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
            DependencyService.Get<ISuHandler>().StartSu();

            StartSuButton.IsEnabled = false;
            KillSuButton.IsEnabled = true;
            IsSuEnabled = true;
        }

        private void KillSuButton_OnPressed(object sender, EventArgs e)
        {
            DependencyService.Get<ISuHandler>().KillSu();

            KillSuButton.IsEnabled = false;
            StartSuButton.IsEnabled = true;
            IsSuEnabled = false;
        }

        public static async void FlashOn() =>
            await FlashAndRebootTask().ConfigureAwait(false);

        private static async Task FlashAndRebootTask()
        {
            DependencyService.Get<IFlasherHandler>().FlashImage();
            await Task.Delay(250);
            DependencyService.Get<IOsHandler>().SyncFs();
            await Task.Delay(250);
            DependencyService.Get<IOsHandler>().RebootOs();
        }
    }
}