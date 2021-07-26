using System.Threading.Tasks;
using Java.Lang;

namespace ArtemisCompanionV2.Droid.API
{
    public class ProcessBase
    {
        internal async Task StartProcessAsync(ProcessBuilder builder)
        {
            using (var process = builder.Start())
            {
                if (process == null)
                    return;

                _ = await process.WaitForAsync();
            }
        }

        internal void StartProcess(ProcessBuilder builder)
        {
            using (var process = builder.Start())
            {
                if (process == null)
                    return;

                _ = process.WaitFor();
            }
        }
    }
}