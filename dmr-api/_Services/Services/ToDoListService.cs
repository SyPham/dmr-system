using AutoMapper;
using DMR_API._Repositories.Interface;
using DMR_API._Services.Interface;
using DMR_API.DTO;
using DMR_API.Helpers;
using DMR_API.Models;
using DMR_API._Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace DMR_API._Services.Services
{
    public class ToDoListService : IToDoListService
    {
        private readonly IToDoListRepository _repoToDoList;
        private readonly IGlueRepository _repoGlue;
        private readonly IMixingInfoRepository _repoMixingInfo;
        private readonly IBuildingRepository _repoBuilding;
        private readonly IPlanRepository _repoPlan;
        private readonly IDispatchRepository _repoDispatch;
        private readonly IJWTService _jwtService;
        private readonly IMapper _mapper;
        private readonly MapperConfiguration _configMapper;
        public ToDoListService(
            IToDoListRepository repoToDoList,
            IGlueRepository repoGlue,
            IMixingInfoRepository repoMixingInfo,
            IBuildingRepository repoBuilding,
            IPlanRepository repoPlan,
            IDispatchRepository repoDispatch,
            IJWTService jwtService,
            IMapper mapper,
            MapperConfiguration configMapper
            )
        {
            _mapper = mapper;
            _configMapper = configMapper;
            _repoToDoList = repoToDoList;
            _repoGlue = repoGlue;
            _repoBuilding = repoBuilding;
            _repoPlan = repoPlan;
            _repoMixingInfo = repoMixingInfo;
            _repoDispatch = repoDispatch;
            _jwtService = jwtService;
        }


        public async Task<bool> AddRange(List<ToDoList> toDoList)
        {
            _repoToDoList.AddRange(toDoList);
            try
            {
                return await _repoToDoList.SaveAll();
            }
            catch (Exception)
            {
                return false;
                throw;
            }
        }

        public async Task<bool> CancelRange(List<ToDoListForCancelDto> todolistList)
        {
            var flag = new List<bool>();
            foreach (var todolist in todolistList)
            {
                var model = _repoToDoList.FindAll(x => x.ID == todolist.ID && todolist.LineNames.Contains(x.LineName)).ToList();
                if (model is null) flag.Add(false);
                _repoToDoList.RemoveMultiple(model);
                flag.Add(await _repoToDoList.SaveAll());
            }
            return flag.All(x => x is true);
        }
        public async Task<bool> Cancel(ToDoListForCancelDto todolist)
        {
            var model = _repoToDoList.FindAll(x => x.ID == todolist.ID && todolist.LineNames.Contains(x.LineName)).ToList();
            if (model is null) return false;
            _repoToDoList.RemoveMultiple(model);
            return await _repoToDoList.SaveAll();
        }

        public async Task<List<ToDoListDto>> Done(int buildingID)
        {
            var currentDate = DateTime.Now.Date;
            var currentTime = DateTime.Now;
            var model = await _repoToDoList.FindAll(x => x.EstimatedStartTime.Date == currentDate && x.EstimatedFinishTime.Date == currentDate && x.BuildingID == buildingID).ToListAsync();

            var groupBy = model.GroupBy(x => new { x.EstimatedStartTime, x.EstimatedFinishTime, x.GlueName });
            var todolist = new List<ToDoListDto>();
            foreach (var todo in groupBy)
            {

                var item = todo.FirstOrDefault();
                var lineList = todo.Select(x => x.LineName).ToList();
                var stdTotal = todo.Select(x => x.StandardConsumption).Sum();
                var stddeliver = todo.Select(x => x.DeliveredConsumption).Sum();

                var itemTodolist = new ToDoListDto();
                itemTodolist.ID = item.ID;
                itemTodolist.PlanID = item.PlanID;
                itemTodolist.MixingInfoID = item.MixingInfoID;
                itemTodolist.GlueID = item.GlueID;
                itemTodolist.LineID = item.LineID;
                itemTodolist.LineName = item.LineName;
                itemTodolist.GlueName = item.GlueName;
                itemTodolist.Supplier = item.Supplier;
                itemTodolist.Status = item.Status;

                itemTodolist.StartMixingTime = item.StartMixingTime;
                itemTodolist.FinishMixingTime = item.FinishMixingTime;

                itemTodolist.StartStirTime = item.StartStirTime;
                itemTodolist.FinishStirTime = item.FinishStirTime;

                itemTodolist.StartDispatchingTime = item.StartDispatchingTime;
                itemTodolist.FinishDispatchingTime = item.FinishDispatchingTime;

                itemTodolist.PrintTime = item.PrintTime;

                itemTodolist.MixedConsumption = Math.Round(item.MixedConsumption);
                itemTodolist.DeliveredConsumption = Math.Round(stddeliver, 2);
                itemTodolist.StandardConsumption = Math.Round(stdTotal, 2);

                itemTodolist.EstimatedStartTime = item.EstimatedStartTime;
                itemTodolist.EstimatedFinishTime = item.EstimatedFinishTime;

                itemTodolist.AbnormalStatus = item.AbnormalStatus;

                itemTodolist.LineNames = lineList;
                itemTodolist.BuildingID = item.BuildingID;
                todolist.Add(itemTodolist);
            }
            return todolist.Where(x => x.EstimatedFinishTime.Date == currentDate && x.PrintTime != null).OrderBy(x => x.EstimatedStartTime).ToList();

        }

        public async Task<List<ToDoListDto>> ToDo(int buildingID)
        {

            var currentTime = DateTime.Now;
            var currentDate = currentTime.Date;

            var model = await _repoToDoList.FindAll(x => x.IsDelete == false && x.EstimatedStartTime.Date == currentDate && x.EstimatedFinishTime.Date == currentDate && x.BuildingID == buildingID).ToListAsync();
            var groupBy = model.GroupBy(x => new { x.EstimatedStartTime, x.EstimatedFinishTime, x.GlueNameID });
            var todolist = new List<ToDoListDto>();
            foreach (var todo in groupBy)
            {
                var item = todo.FirstOrDefault();
                var lineList = todo.Select(x => x.LineName).ToList();
                var stdTotal = todo.Select(x => x.StandardConsumption).Sum();
                var stddeliver = todo.Select(x => x.DeliveredConsumption).Sum();

                var itemTodolist = new ToDoListDto();
                itemTodolist.ID = item.ID;
                itemTodolist.PlanID = item.PlanID;
                itemTodolist.MixingInfoID = item.MixingInfoID;
                itemTodolist.GlueID = item.GlueID;
                itemTodolist.LineID = item.LineID;
                itemTodolist.LineName = item.LineName;
                itemTodolist.GlueName = item.GlueName;
                itemTodolist.Supplier = item.Supplier;
                itemTodolist.Status = item.Status;

                itemTodolist.StartMixingTime = item.StartMixingTime;
                itemTodolist.FinishMixingTime = item.FinishMixingTime;

                itemTodolist.StartStirTime = item.StartStirTime;
                itemTodolist.FinishStirTime = item.FinishStirTime;

                itemTodolist.StartDispatchingTime = item.StartDispatchingTime;
                itemTodolist.FinishDispatchingTime = item.FinishDispatchingTime;

                itemTodolist.PrintTime = item.PrintTime;

                itemTodolist.MixedConsumption = Math.Round(item.MixedConsumption, 2);
                itemTodolist.DeliveredConsumption = Math.Round(stddeliver, 2);
                itemTodolist.StandardConsumption = stdTotal;

                itemTodolist.EstimatedStartTime = item.EstimatedStartTime;
                itemTodolist.EstimatedFinishTime = item.EstimatedFinishTime;

                itemTodolist.AbnormalStatus = item.AbnormalStatus;
                itemTodolist.IsDelete = item.IsDelete;

                itemTodolist.LineNames = lineList;
                itemTodolist.BuildingID = item.BuildingID;
                todolist.Add(itemTodolist);
            }
            return todolist.Where(x => x.EstimatedFinishTime.Date == currentDate && x.PrintTime is null).OrderBy(x => x.EstimatedStartTime).OrderBy(x=> x.EstimatedStartTime).ThenBy(x=> x.Supplier).ToList();
        }

        public async Task<MixingInfo> Mix(MixingInfoForCreateDto mixing)
        {
            try
            {
                var item = _mapper.Map<MixingInfoForCreateDto, MixingInfo>(mixing);
                item.Code = CodeUtility.RandomString(8);
                item.CreatedTime = DateTime.Now;
                var glue = await _repoGlue.FindAll().FirstOrDefaultAsync(x => x.isShow == true && x.ID == mixing.GlueID);
                item.ExpiredTime = DateTime.Now.AddMinutes(glue.ExpiredTime);
                _repoMixingInfo.Add(item);
                await _repoMixingInfo.SaveAll();
                // await _repoMixing.AddOrUpdate(item.ID);
                return item;
            }
            catch
            {
                return new MixingInfo();
            }
        }

        public void UpdateDispatchTimeRange(ToDoListForUpdateDto model)
        {
            var dispatch = _repoDispatch.FindAll(x => x.MixingInfoID == model.MixingInfoID && x.LineID == model.LineID).Select(x => x.Amount).ToList();
            var total = dispatch.Sum();
            var list = _repoToDoList.FindAll(x => x.EstimatedStartTime == model.EstimatedStartTime && x.EstimatedFinishTime == model.EstimatedFinishTime && x.GlueName == model.GlueName && x.LineID == model.LineID).ToList();
            list.ForEach(x =>
            {
                x.FinishDispatchingTime = model.FinishTime;
                x.StartDispatchingTime = model.StartTime;
                x.DeliveredConsumption = total;
            });
            _repoToDoList.UpdateRange(list);
            _repoToDoList.Save();
        }

        public void UpdateMixingTimeRange(ToDoListForUpdateDto model)
        {
            var list = _repoToDoList.FindAll(x => x.EstimatedStartTime == model.EstimatedStartTime && x.EstimatedFinishTime == model.EstimatedFinishTime && x.GlueName == model.GlueName).ToList();
            list.ForEach(x =>
            {
                x.FinishMixingTime = model.FinishTime;
                x.StartMixingTime = model.StartTime;
                x.MixedConsumption = model.Amount;
                x.MixingInfoID = model.MixingInfoID;
            });
            _repoToDoList.UpdateRange(list);
            _repoToDoList.Save();
        }

        public void UpdateStiringTimeRange(ToDoListForUpdateDto model)
        {
            var list = _repoToDoList.FindAll(x => x.EstimatedStartTime == model.EstimatedStartTime && x.EstimatedFinishTime == model.EstimatedFinishTime && x.GlueName == model.GlueName).ToList();
            list.ForEach(x =>
            {
                x.FinishStirTime = model.FinishTime;
                x.StartStirTime = model.StartTime;
            });
            _repoToDoList.UpdateRange(list);
            _repoToDoList.Save();
        }
        public MixingInfo PrintGlue(int mixingInfoID)
        {
            var mixing = _repoMixingInfo.FindById(mixingInfoID);
            if (mixing is null) return new MixingInfo();
            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    var printTime = DateTime.Now;
                    mixing.Status = printTime <= mixing.EstimatedFinishTime;
                    mixing.PrintTime = printTime;
                    _repoMixingInfo.Update(mixing);
                    _repoMixingInfo.Save();
                    var todolist = _repoToDoList.FindAll(x => x.GlueName == mixing.GlueName && x.EstimatedStartTime == mixing.EstimatedStartTime && x.EstimatedFinishTime == mixing.EstimatedFinishTime).ToList();
                    todolist.ForEach(item =>
                    {
                        item.Status = mixing.Status;
                        item.PrintTime = mixing.PrintTime;

                    });
                    _repoToDoList.UpdateRange(todolist);
                    _repoToDoList.Save();
                    scope.Complete();
                    return mixing;
                }
                catch
                {
                    scope.Dispose();
                    return new MixingInfo();
                }
            }
        }
        public MixingInfo FindPrintGlue(int mixingInfoID)
        {
            var mixing = _repoMixingInfo.FindById(mixingInfoID);
            if (mixing is null) return new MixingInfo();
            return mixing;
        }

        public async Task<object> Dispatch(DispatchParams todolistDto)
        {
            var todolist = await _repoToDoList.FindAll(x =>
            todolistDto.Lines.Contains(x.LineName)
            && x.EstimatedFinishTime == todolistDto.EstimatedFinishTime
            && x.EstimatedStartTime == todolistDto.EstimatedStartTime
            && x.GlueName == todolistDto.Glue
            && todolistDto.Lines.Contains(x.LineName))
                .ToListAsync();
            var dispatches = await _repoDispatch.FindAll(x => x.CreatedTime.Date == todolistDto.EstimatedFinishTime.Date && todolist.Select(x => x.MixingInfoID).Contains(x.MixingInfoID)).ToListAsync();

            var result = (from a in todolist
                          join b in dispatches on a.LineID equals b.LineID
                          into ab
                          from c in ab.DefaultIfEmpty()
                          select new DispatchTodolistDto
                          {
                              ID = c == null ? 0 : c.ID,
                              Glue = a.GlueName,
                              Line = a.LineName,
                              LineID = a.LineID,
                              MixingInfoID = a.MixingInfoID,
                              Real = c == null ? 0 : c.Amount * 1000,
                              StandardAmount = c == null ? 0 : c.StandardAmount,
                              MixedConsumption = Math.Round(a.MixedConsumption, 2)
                          });
            return result.OrderBy(x => x.Line).ToList();
        }

        public bool UpdateStartStirTimeByMixingInfoID(int mixingInfoID)
        {
            var mixing = _repoMixingInfo.FindById(mixingInfoID);
            if (mixing == null) return false;
            var lines = _repoBuilding.FindAll(x => x.ParentID == mixing.BuildingID).Select(x => x.ID).ToList();
            if (lines.Count == 0) return false;
            try
            {
                var list = _repoToDoList.FindAll(x => x.EstimatedStartTime == mixing.EstimatedStartTime && x.EstimatedFinishTime == mixing.EstimatedFinishTime && x.GlueName == mixing.GlueName && lines.Contains(x.LineID)).ToList();
                list.ForEach(x =>
                {
                    x.StartStirTime = DateTime.Now;
                });
                _repoToDoList.UpdateRange(list);
                _repoToDoList.Save();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool UpdateFinishStirTimeByMixingInfoID(int mixingInfoID)
        {
            var mixing = _repoMixingInfo.FindById(mixingInfoID);
            if (mixing == null) return false;
            var lines = _repoBuilding.FindAll(x => x.ParentID == mixing.BuildingID).Select(x => x.ID).ToList();
            try
            {
                var list = _repoToDoList.FindAll(x => x.EstimatedStartTime == mixing.EstimatedStartTime && x.EstimatedFinishTime == mixing.EstimatedFinishTime && x.GlueName == mixing.GlueName && lines.Contains(x.LineID)).ToList();
                list.ForEach(x =>
                {
                    x.FinishStirTime = DateTime.Now;
                });
                _repoToDoList.UpdateRange(list);
                _repoToDoList.Save();
                return true;
            }
            catch
            {
                return false;
            }
        }
     
        public async Task<bool> GenerateToDoList(List<int> plans)
        {
            if (plans.Count == 0) return false;
            var currentTime = DateTime.Now;
            var currentDate = currentTime.Date;
            var plansModel = await _repoPlan.FindAll(x => plans.Contains(x.ID))
                .Include(x => x.BPFCEstablish)
                    .ThenInclude(x => x.Glues)
                    .ThenInclude(x => x.GlueName)
                .Include(x => x.BPFCEstablish)
                    .ThenInclude(x => x.Glues)
                    .ThenInclude(x => x.GlueIngredients)
                    .ThenInclude(x => x.Ingredient)
                    .ThenInclude(x => x.Supplier)
                    .SelectMany(x => x.BPFCEstablish.Glues.Where(x=> x.isShow), (plan, glue) => new
                    {
                        plan.WorkingHour,
                        plan.HourlyOutput,
                        plan.FinishWorkingTime,
                        plan.StartWorkingTime,
                        plan.DueDate,
                        plan.Building,
                        PlanID = plan.ID,
                        BPFCID = plan.BPFCEstablishID,
                        plan.CreatedDate,
                        glue.Consumption,
                        GlueID = glue.ID,
                        glue.GlueName,
                        ChemicalA = glue.GlueIngredients.FirstOrDefault(x => x.Position == "A").Ingredient,
                    }).ToListAsync();

            if (plansModel.Count == 0) return false;
            var value = plansModel.FirstOrDefault();

            var line = await _repoBuilding.FindAll(x => x.ID == value.Building.ID).FirstOrDefaultAsync();
            if (line is null) return false;

            var building = await _repoBuilding.FindAll(x => x.ID == line.ParentID)
               .Include(x => x.LunchTime).FirstOrDefaultAsync();
            if (building is null) return false;

            var startLunchTimeBuilding = building.LunchTime.StartTime;
            var endLunchTimeBuilding = building.LunchTime.EndTime;
          

            var todolist = new List<ToDoListDto>();
            var glues = plansModel.GroupBy(x => x.GlueName).ToList();
            foreach (var glue in glues)
            {
               
                foreach (var item in glue)
                {
                    var startLunchTime = item.DueDate.Date.Add(new TimeSpan(startLunchTimeBuilding.Hour, startLunchTimeBuilding.Minute, 0));
                    var endLunchTime = item.DueDate.Date.Add(new TimeSpan(endLunchTimeBuilding.Hour, endLunchTimeBuilding.Minute, 0));

                    var finishWorkingTime = item.FinishWorkingTime;
                    var checmicalA = item.ChemicalA;

                    int prepareTime = checmicalA.PrepareTime;
                    double replacementFrequency = checmicalA.ReplacementFrequency;
                    var kgPair = item.Consumption.ToDouble() / 1000;

                    if (item.CreatedDate.Date != currentDate)
                    {
                        var estimatedTime = item.DueDate.Date.Add(new TimeSpan(7, 30, 00)) - TimeSpan.FromMinutes(prepareTime);
                        var startWorkingTimeTemp = estimatedTime;

                        // Neu tao tu ngay hom truoc thi lay moc thoi gian la 7:30
                        while (true)
                        {
                            if (startWorkingTimeTemp <= finishWorkingTime)
                            {
                                var todo = new ToDoListDto();
                                todo.GlueName = glue.Key.Name;
                                todo.GlueID = item.GlueID;
                                todo.PlanID = item.PlanID;
                                todo.LineID = item.Building.ID;
                                todo.LineName = item.Building.Name;
                                todo.PlanID = item.PlanID;
                                todo.BPFCID = item.BPFCID;
                                todo.Supplier = item.ChemicalA.Supplier.Name;
                                todo.PlanID = item.PlanID;
                                todo.GlueNameID = item.GlueName.ID;
                                todo.BuildingID = building.ID;
                                var estimatedFinishTime = startWorkingTimeTemp.AddMinutes(prepareTime);
                                if (startWorkingTimeTemp >= startLunchTime && estimatedFinishTime <= endLunchTime)
                                {
                                    // neu nam trong gio an trua thi lay tu khoang EndLunchTime
                                    estimatedFinishTime = endLunchTime.AddMinutes(prepareTime);
                                    startWorkingTimeTemp = endLunchTime;
                                    todo.EstimatedStartTime = startWorkingTimeTemp;
                                    todo.EstimatedFinishTime = estimatedFinishTime;
                                }
                                else
                                {
                                    replacementFrequency = estimatedFinishTime >= finishWorkingTime ? (finishWorkingTime - startWorkingTimeTemp).TotalHours : replacementFrequency;
                                    todo.EstimatedStartTime = startWorkingTimeTemp;
                                    var standardConsumption = kgPair * (double)item.HourlyOutput * replacementFrequency;
                                    todo.StandardConsumption = standardConsumption;
                                    todo.EstimatedFinishTime = estimatedFinishTime;
                                }
                                todolist.Add(todo);
                                startWorkingTimeTemp = startWorkingTimeTemp.AddHours(checmicalA.ReplacementFrequency);

                            }
                            else
                            {
                                break;
                            }

                        }
                    }
                    else
                    {
                        var startWorkingTimeTemp = item.StartWorkingTime;

                        // Them task trong ngay
                        while (true)
                        {
                            if (startWorkingTimeTemp <= finishWorkingTime)
                            {
                                var todo = new ToDoListDto();
                                todo.GlueName = glue.Key.Name;
                                todo.GlueID = item.GlueID;
                                todo.PlanID = item.PlanID;
                                todo.LineID = item.Building.ID;
                                todo.LineName = item.Building.Name;
                                todo.PlanID = item.PlanID;
                                todo.BPFCID = item.BPFCID;
                                todo.Supplier = item.ChemicalA.Supplier.Name;
                                todo.PlanID = item.PlanID;
                                todo.GlueNameID = item.GlueName.ID;
                                todo.BuildingID = building.ID;

                                var estimatedFinishTime = startWorkingTimeTemp.AddMinutes(prepareTime);
                                if (startWorkingTimeTemp >= startLunchTime && estimatedFinishTime <= endLunchTime)
                                {
                                    // neu nam trong gio an trua thi lay tu khoang EndLunchTime
                                    estimatedFinishTime = endLunchTime.AddMinutes(prepareTime);
                                    startWorkingTimeTemp = endLunchTime;
                                    todo.EstimatedStartTime = startWorkingTimeTemp;
                                    todo.EstimatedFinishTime = estimatedFinishTime;

                                }
                                else
                                {

                                    replacementFrequency = estimatedFinishTime >= finishWorkingTime ? (finishWorkingTime - startWorkingTimeTemp).TotalHours : replacementFrequency;
                                    todo.EstimatedStartTime = startWorkingTimeTemp;
                                    var standardConsumption = kgPair * (double)item.HourlyOutput * replacementFrequency;
                                    todo.StandardConsumption = standardConsumption;
                                    todo.EstimatedFinishTime = estimatedFinishTime;
                                }
                                todolist.Add(todo);
                                startWorkingTimeTemp = startWorkingTimeTemp.AddHours(checmicalA.ReplacementFrequency);

                            }
                            else
                            {
                                break;
                            }

                        }
                    }
                }
            }
            try
            {
                var model = _mapper.Map<List<ToDoList>>(todolist);
                _repoToDoList.AddRange(model);
                _repoToDoList.Save();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
