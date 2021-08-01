using System.Threading.Tasks;
using Java.Lang;
using Java.Util.Concurrent;

namespace ArtemisCompanionV2.Droid.API
{
    public class ProcessBase
    {
        internal static async Task StartProcessAsync(ProcessBuilder builder)
        {
            using (var process = builder.Start())
            {
                if (process == null)
                    return;

                await process.WaitForAsync(1000, TimeUnit.Milliseconds);
            }
        }
    }
}