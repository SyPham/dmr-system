using System.Collections.Generic;
using System.Threading.Tasks;
using DMR_API.DTO;
using DMR_API.Helpers;
using DMR_API.Models;

namespace DMR_API._Services.Interface
{
    public interface IMapModelService : IECService<MapModelDto>
    {
        Task<List<ModelNoForMapModelDto>> GetModelNos(int modelNameID);
        Task<bool> MapMapModel(MapModel mapModel);
        Task<bool> Delete(int modelNameId, int modelNoId);
    }
}