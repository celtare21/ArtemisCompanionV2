using System.Threading.Tasks;
using ArtemisCompanionV2.API;
using ArtemisCompanionV2.Droid.API;
using Java.Lang;
using Xamarin.Forms;

[assembly: Dependency(typeof(FlasherHandler))]

namespace ArtemisCompanionV2.Droid.API
{
    public class FlasherHandler : ProcessBase, IFlasherHandler
    {
        public async Task FlashImage()
        {
            await FlashPartition("a");
            await FlashPartition("b").ConfigureAwait(false);
        }

        private static async Task FlashPartition(string partition)
        {
            using (var builder = new ProcessBuilder("/system/bin/su", "-c",
                $"/system/bin/dd if=/data/user/0/com.kaname.artemiscompanion/files/boot.img of=/dev/block/bootdevice/by-name/boot_{partition}"))
            {
                await StartProcessAsync(builder);
            }
        }
    }
}