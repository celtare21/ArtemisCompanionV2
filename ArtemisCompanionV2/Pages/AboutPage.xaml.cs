using Plugin.Clipboard;
using System;
using Xamarin.Forms.Xaml;

namespace ArtemisCompanionV2.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AboutPage
    {
        public AboutPage() =>
            InitializeComponent();

        private void CopyButton_OnPressed(object sender, EventArgs e) =>
            CrossClipboard.Current.SetText("pkg install -y tsu tar sed zstd openssl-tool pv curl");
    }
}