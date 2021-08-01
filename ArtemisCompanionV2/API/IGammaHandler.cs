using System.Threading.Tasks;

namespace ArtemisCompanionV2.API
{
    public interface IGammaHandler
    {
        Task EnableGammaHack();
        Task DisableGammaHack();
    }
}
