using System.Threading.Tasks;
using ArtemisCompanionV2.API;
using ArtemisCompanionV2.Droid.API;
using Java.Lang;
using Xamarin.Forms;

[assembly: Dependency(typeof(GammaHandler))]

namespace ArtemisCompanionV2.Droid.API
{
    public class GammaHandler : ProcessBase, IGammaHandler
    {
        public async Task EnableGammaHack()
        {
            using (var builder = new ProcessBuilder("/system/bin/su", "-c",
                "/system/bin/echo 2 > /dev/userland_listener-0"))
            {
                await StartProcessAsync(builder);
            }
        }

        public async Task DisableGammaHack()
        {
            using (var builder = new ProcessBuilder("/system/bin/su", "-c",
                "/system/bin/echo 3 > /dev/userland_listener-0"))
            {
                await StartProcessAsync(builder);
            }
        }
    }
}