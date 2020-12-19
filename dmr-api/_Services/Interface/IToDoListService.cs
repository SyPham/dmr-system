using DMR_API.DTO;
using DMR_API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API._Services.Interface
{
    public interface IToDoListService
    {
        Task<List<ToDoListDto>> Done(int buildingID);
        Task<bool> GenerateToDoList(List<int> plans);
        Task<List<ToDoListDto>> ToDo(int buildingID);
        Task<bool> AddRange(List<ToDoList> toDoList);
        Task<MixingInfo> Mix(MixingInfoForCreateDto mixForToDoListDto);
        void UpdateDispatchTimeRange(ToDoListForUpdateDto model);
        void UpdateMixingTimeRange(ToDoListForUpdateDto model);
        void UpdateStiringTimeRange(ToDoListForUpdateDto model);
        MixingInfo PrintGlue(int mixingInfoID);
        MixingInfo FindPrintGlue(int mixingInfoID);
        Task<object> Dispatch(DispatchParams todolistDto);
        Task<bool> Cancel(ToDoListForCancelDto todolistID);
        Task<bool> CancelRange(List<ToDoListForCancelDto> todolistIDList);
        bool UpdateStartStirTimeByMixingInfoID(int mixingInfoID);
        bool UpdateFinishStirTimeByMixingInfoID(int mixingInfoID);


    }
}
