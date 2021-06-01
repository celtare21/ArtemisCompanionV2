using System.IO;

namespace ArtemisCompanionV2.API
{
    public static class FileHandler
    {
        public static readonly string BackupPass, BackupFile, StatusFile;
        public static readonly string DnsFile, FlashFile, BootFile, BlurFile;

        static FileHandler()
        {
            var appFolder = Xamarin.Essentials.FileSystem.AppDataDirectory;
            var configsFolder = Path.Combine(appFolder, "configs");

            DnsFile = Path.Combine(configsFolder, "dns.txt");
            FlashFile = Path.Combine(configsFolder, "flash_boot.txt");
            BlurFile = Path.Combine(configsFolder, "blur_enable.txt");
            BootFile = Path.Combine(appFolder, "boot.img");
            BackupPass = Path.Combine(configsFolder, "pass.txt");
            BackupFile = Path.Combine(configsFolder, "backup.txt");
            StatusFile = Path.Combine(configsFolder, "status.txt");

            Directory.CreateDirectory(configsFolder);
            if (!File.Exists(DnsFile))
                File.WriteAllText(DnsFile, "0");
            if (!File.Exists(FlashFile))
                File.WriteAllText(FlashFile, "0");
            if (!File.Exists(FlashFile))
                File.WriteAllText(FlashFile, "0");
            if (!File.Exists(BlurFile))
                File.WriteAllText(BlurFile, "0");
            if (!File.Exists(BackupFile))
                File.WriteAllText(BackupFile, "0");
            File.Delete(BootFile);
        }
    }
}
