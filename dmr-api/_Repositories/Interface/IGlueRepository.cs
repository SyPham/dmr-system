using System.Threading.Tasks;
using DMR_API.Data;
using DMR_API.Models;

namespace DMR_API._Repositories.Interface
{
    public interface IGlueRepository : IECRepository<Glue>
    {
        bool Save();
        Task<bool> CheckExists(int id);
        Task<bool> CheckBarCodeExists(string code);
        //viet them ham o day neu chua co trong ECRepository
    }
}