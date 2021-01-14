using DMR_API.DTO;
using DMR_API.Models;
using DMR_API.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API._Services.Interface
{
    public interface IPlanService : IECService<PlanDto>
    {
        Task<object> GetAllPlanByDefaultRange();
        Task<object> GetAllPlanByRange(int building, DateTime min, DateTime max);
        Task<object> GetAllPlansByDate(string from, string to);
        Task<object> Summary(int building);
        Task<object> GetLines(int buildingID);
        Task<byte[]> Report(DateTime startDate, DateTime endDate);
        Task<byte[]> ReportConsumptionCase2(ReportParams reportParams);
        Task<byte[]> ReportConsumptionCase1(ReportParams reportParams);
        Task<List<GlueCreateDto1>> GetGlueByBuilding(int buildingID);
        Task<List<GlueCreateDto1>> GetGlueByBuildingModelName(int buildingID, int modelName);

        Task<object> GetBatchByIngredientID(int ingredientID);
        Task<List<PlanDto>> GetGlueByBuildingBPFCID(int buildingID, int bpfcID);
        Task<object> DispatchGlue(BuildingGlueForCreateDto obj);
        Task <object> TroubleShootingSearch(string ingredientName , string batch);
        Task<object> ClonePlan(List<PlanForCloneDto> plans);
        Task<object> DeleteRange(List<int> plansDto);
        Task<object> GetBPFCByGlue(TooltipParams tooltip);
        Task<bool> EditDelivered(int id, string qty );
        Task<bool> EditQuantity(int id, int qty );
        Task<bool> DeleteDelivered(int id);
        Task<List<ConsumtionDto>> ConsumptionByLineCase1(ReportParams reportParams);
        Task<List<ConsumtionDto>> ConsumptionByLineCase2(ReportParams reportParams);
        Task<List<TodolistDto>> CheckTodolistAllBuilding();
        Task<object> Dispatch(DispatchParams todolistDto);
        Task<MixingInfo> Print(DispatchParams todolistDto);
        MixingInfo PrintGlue(int mixingÌnoID);
        Task<object> Finish(int mixingÌnoID);
        PlanDto FindByID(int ID);
        Task<bool> DeletePlan(int id);
        Task<bool> CheckExistTimeRange(int lineID, DateTime statTime, DateTime endTime, DateTime dueDate);
        Task<bool> CheckDuplicate(int lineID, int BPFCEstablishID, DateTime dueDate);
        bool DeleteRangePlan(List<int> plans);
    }
}
