using ArtemisCompanionV2.API;
using ArtemisCompanionV2.Droid.API;
using ArtemisCompanionV2.Pages;
using Java.Lang;
using Xamarin.Forms;

[assembly: Dependency(typeof(FlasherHandler))]

namespace ArtemisCompanionV2.Droid.API
{
    public class FlasherHandler : ProcessBase, IFlasherHandler
    {
        public void FlashImage()
        {
            if (!ConfigPage.IsSuEnabled)
                DependencyService.Get<ISuHandler>().StartSu();

            FlashPartition("a");
            FlashPartition("b");
        }

        private void FlashPartition(string partition)
        {
            using (var builder = new ProcessBuilder("/system/bin/su", "-c",
                $"/system/bin/dd if=/data/user/0/com.kaname.artemiscompanion/files/boot.img of=/dev/block/bootdevice/by-name/boot_{partition}"))
            {
                StartProcess(builder);
            }
        }
    }
}