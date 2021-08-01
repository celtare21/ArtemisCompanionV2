using System.Threading.Tasks;
using ArtemisCompanionV2.API;
using ArtemisCompanionV2.Droid.API;
using Java.Lang;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: Dependency(typeof(SuHandler))]

namespace ArtemisCompanionV2.Droid.API
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class SuHandler : ProcessBase, ISuHandler
    {
        public async Task KillSu(bool shouldExit = false)
        {
            if (shouldExit)
                return;

            if (await TestMagizz())
            {
                await SetEnforce(true);
                return;
            }

            if (!await TestSu())
                return;

            using (var builder = new ProcessBuilder("/system/bin/su", "-c",
                "/system/bin/echo 1 > /dev/userland_listener-0"))
            {
                await StartProcessAsync(builder);
            }
        }

        public async Task StartSu(bool shouldExit = false)
        {
            if (shouldExit)
                return;

            if (await TestMagizz())
            {
                await SetEnforce(false);
                return;
            }

            if (await TestSu())
                return;

            using (var builder = new ProcessBuilder("/system/bin/stub"))
            {
                await StartProcessAsync(builder);
            }
        }

        public async Task<bool> TestSu()
        {
            using (var builder = new ProcessBuilder("/system/bin/test", "-f",
                "/system/bin/su"))
            {
                using (var process = builder.Start())
                {
                    if (process == null)
                        return await Task.FromResult(false);

                    return await process.WaitForAsync() != 1;
                }
            }
        }

        private static async Task<bool> TestMagizz()
        {
            using (var builder = new ProcessBuilder("/system/bin/test", "-d",
                "/data/adb/magisk"))
            {
                using (var process = builder.Start())
                {
                    if (process == null)
                        return await Task.FromResult(false);

                    return await process.WaitForAsync() != 1;
                }
            }
        }

        private static async Task SetEnforce(bool value)
        {
            using (var builder = new ProcessBuilder("/system/bin/setenforce", value ? "1" : "0"))
            {
                await StartProcessAsync(builder);
            }
        }
    }
}