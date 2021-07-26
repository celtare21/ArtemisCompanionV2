using System.IO;
using Xamarin.Forms.Xaml;

// ReSharper disable InvertIf

namespace ArtemisCompanionV2.Droid.API
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public static class AssetHandler
    {
        private static readonly string AssetsFolder;

        static AssetHandler()
        {
            var appFolder = Xamarin.Essentials.FileSystem.AppDataDirectory;

            AssetsFolder = Path.Combine(appFolder, "assets");
            Directory.CreateDirectory(AssetsFolder);
        }

        public static void CopyAssets()
        {
            CopyFileToPath("cbackup.sh");
            CopyFileToPath("resetprop");
            CopyFileToPath("k_hosts");
        }

        private static void CopyFileToPath(string fileName)
        {
            using (var androidAssets = Android.App.Application.Context.Assets)
            {
                if (!File.Exists(Path.Combine(AssetsFolder, fileName)))
                {
                    using (var source = new StreamReader(androidAssets?.Open(fileName) ?? Stream.Null))
                    {
                        using (var fileStream = File.Create(Path.Combine(AssetsFolder, fileName)))
                        {
                            source.BaseStream.CopyTo(fileStream);
                        }
                    }
                }
            }
        }
    }
}