using System.IO;
using Xamarin.Forms.Xaml;

// ReSharper disable InvertIf

namespace ArtemisCompanionV2.Droid.API
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public static class AssetHandler
    {
        public static void CopyAssets()
        {
            var appFolder = Xamarin.Essentials.FileSystem.AppDataDirectory;
            var assetsFolder = Path.Combine(appFolder, "assets");

            Directory.CreateDirectory(assetsFolder);

            using (var androidAssets = Android.App.Application.Context.Assets)
            {
                if (!File.Exists(Path.Combine(assetsFolder, "cbackup.sh")))
                {
                    using (var source = new StreamReader(androidAssets?.Open("cbackup.sh") ?? Stream.Null))
                    {
                        using (var fileStream = File.Create(Path.Combine(assetsFolder, "cbackup.sh")))
                        {
                            source.BaseStream.CopyTo(fileStream);
                        }
                    }
                }

                if (!File.Exists(Path.Combine(assetsFolder, "resetprop")))
                {
                    using (var source = new StreamReader(androidAssets?.Open("resetprop") ?? Stream.Null))
                    {
                        using (var fileStream = File.Create(Path.Combine(assetsFolder, "resetprop")))
                        {
                            source.BaseStream.CopyTo(fileStream);
                        }
                    }
                }
            }
        }
    }
}