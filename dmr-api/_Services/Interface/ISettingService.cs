using DMR_API.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API._Services.Interface
{
    public interface ISettingService
    {
        Task<object> GetAllAsync();
        Task<bool> AddSetting(SettingDTO model);
        Task<bool> AddMachine(ScaleMachineDto model);
        Task<bool> Add(StirDTO model);
        Task<bool> Update(StirDTO model);
        Task<bool> DeleteSetting(int id);
        Task<bool> DeleteMachine(int id);
        Task<bool> UpdateSetting(SettingDTO model);
        Task<bool> UpdateMachine(ScaleMachineDto model);
        Task<object> GetSettingByBuilding(int buildingID);
        Task<object> GetMachineByBuilding(int buildingID);
    }
}
