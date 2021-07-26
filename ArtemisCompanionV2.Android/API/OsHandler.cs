using ArtemisCompanionV2.API;
using ArtemisCompanionV2.Droid.API;
using Java.Lang;
using Xamarin.Forms;

[assembly: Dependency(typeof(OsHandler))]

namespace ArtemisCompanionV2.Droid.API
{
    public class OsHandler : ProcessBase, IOsHandler
    {
        public void RebootOs()
        {
            using (var builder = new ProcessBuilder("/system/bin/su", "-c",
                "/system/bin/reboot"))
            {
                StartProcess(builder);
            }
        }

        public void SyncFs()
        {
            using (var builder = new ProcessBuilder("/system/bin/su", "-c",
                "/system/bin/sync"))
            {
                StartProcess(builder);
            }
        }
    }
}