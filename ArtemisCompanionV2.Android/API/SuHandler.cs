using ArtemisCompanionV2.API;
using ArtemisCompanionV2.Droid.API;
using Java.Lang;
using Java.Util.Concurrent;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: Dependency(typeof(SuHandler))]

namespace ArtemisCompanionV2.Droid.API
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class SuHandler : ProcessBase, ISuHandler
    {
        public async void KillSu()
        {
            using (var builder = new ProcessBuilder("/system/bin/su", "-c",
                "/system/bin/echo 1 > /dev/userland_listener-0"))
            {
                await StartProcessAsync(builder);
            }
        }

        public async void StartSu()
        {
            using (var builder = new ProcessBuilder("/system/bin/sh", "-c",
                "/system/bin/stub"))
            {
                await StartProcessAsync(builder);
            }
        }

        public void TestSu()
        {
            using (var builder = new ProcessBuilder("/system/bin/su", "-c",
                "/system/bin/echo Hello"))
            {
                using (var process = builder.Start())
                {
                    if (process == null)
                        return;

                    _ = process.WaitFor(1000, TimeUnit.Milliseconds);
                }
            }
        }
    }
}