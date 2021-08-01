using System.Threading.Tasks;

namespace ArtemisCompanionV2.API
{
    public interface IOsHandler
    {
        Task RebootOs();
        Task SyncFs();
    }
}
