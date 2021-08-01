using System.Threading.Tasks;

namespace ArtemisCompanionV2.API
{
    public interface ISuHandler
    {
        Task KillSu(bool shouldExit = false);
        Task StartSu(bool shouldExit = false);
        Task<bool> TestSu();
    }
}
