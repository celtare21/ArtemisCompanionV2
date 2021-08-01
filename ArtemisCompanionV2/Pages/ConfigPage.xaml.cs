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
        private bool _suEnabled;

        public ConfigPage() =>
            InitializeComponent();

        protected override async void OnAppearing()
        {
            DnsPick.Title = "DNS Profile";
            DnsPick.Margin = new Thickness(33, 15, 20, 0);
            DnsPick.SelectedIndex = Convert.ToInt32(await File.ReadAllTextAsync(FileHandler.DnsFile));

            BlurToggle.IsToggled = Convert.ToBoolean(Convert.ToInt32(await File.ReadAllTextAsync(FileHandler.BlurFile)));

            if (await DependencyService.Get<ISuHandler>().TestSu())
                return;

            KillSuButton.IsEnabled = false;
            StartSuButton.IsEnabled = true;
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

            await FlashAndRebootTask(filePath).ConfigureAwait(false);
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

        private async void StartSuButton_OnPressed(object sender, EventArgs e)
        {
            await DependencyService.Get<ISuHandler>().StartSu();

            StartSuButton.IsEnabled = false;
            KillSuButton.IsEnabled = true;
            _suEnabled = true;
        }

        private async void KillSuButton_OnPressed(object sender, EventArgs e)
        {
            await DependencyService.Get<ISuHandler>().KillSu();

            KillSuButton.IsEnabled = false;
            StartSuButton.IsEnabled = true;
            _suEnabled = false;
        }

        private async void EnableGammaHackButton_OnPressed(object sender, EventArgs e)
        {
            await DependencyService.Get<ISuHandler>().StartSu(_suEnabled);
            await DependencyService.Get<IGammaHandler>().EnableGammaHack();
            await DependencyService.Get<ISuHandler>().KillSu(_suEnabled);

            EnableGammaHackButton.IsEnabled = false;
            DisableGammaHackButton.IsEnabled = true;
        }

        private async void DisableGammaHackButton_OnPressed(object sender, EventArgs e)
        {
            await DependencyService.Get<ISuHandler>().StartSu(_suEnabled);
            await DependencyService.Get<IGammaHandler>().DisableGammaHack();
            await DependencyService.Get<ISuHandler>().KillSu(_suEnabled);

            EnableGammaHackButton.IsEnabled = true;
            DisableGammaHackButton.IsEnabled = false;
        }

        public static Task FlashOn(string path) =>
            FlashAndRebootTask(path);

        private static async Task FlashAndRebootTask(string path)
        {
            CopyNewBoot(path);

            await DependencyService.Get<ISuHandler>().StartSu();
            await DependencyService.Get<IFlasherHandler>().FlashImage();
            await DependencyService.Get<IOsHandler>().SyncFs();
            await DependencyService.Get<IOsHandler>().RebootOs();
            await DependencyService.Get<ISuHandler>().KillSu().ConfigureAwait(false);
        }

        private static void CopyNewBoot(string path)
        {
            File.Delete(FileHandler.BootFile);
            File.Copy(path, FileHandler.BootFile);
        }
    }
}