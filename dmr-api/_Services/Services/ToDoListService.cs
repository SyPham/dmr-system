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
using System.Globalization;
using OfficeOpenXml;
using System.IO;
using OfficeOpenXml.Style;
using System.Drawing;
using DMR_API.Data;

namespace DMR_API._Services.Services
{
    public class ToDoListService : IToDoListService
    {
        private readonly IToDoListRepository _repoToDoList;
        private readonly IGlueRepository _repoGlue;
        private readonly IMixingInfoRepository _repoMixingInfo;
        private readonly IBuildingRepository _repoBuilding;
        private readonly IPlanRepository _repoPlan;
        private readonly IMongoRepository<Data.MongoModels.RawData> _repoRawData;
        private readonly IStirRepository _repoStir;
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
            IMongoRepository<DMR_API.Data.MongoModels.RawData> repoRawData,
            IStirRepository repoStir,
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
            _repoRawData = repoRawData;
            _repoStir = repoStir;
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

        public async Task<ToDoListForReturnDto> Done(int buildingID)
        {

            var currentTime = DateTime.Now.ToLocalTime();
            var currentDate = currentTime.Date;
            var value = currentTime.ToString("tt", CultureInfo.InvariantCulture);
            // var morning = "AM";
            // var start = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 12, 00, 00);
            var model = await _repoToDoList.FindAll(x =>
                  x.IsDelete == false
                  && x.EstimatedStartTime.Date == currentDate
                  && x.EstimatedFinishTime.Date == currentDate
                  && x.BuildingID == buildingID)
               .ToListAsync();

            var groupBy = model.GroupBy(x => new { x.EstimatedStartTime, x.EstimatedFinishTime, x.GlueName });
            var todolist = new List<ToDoListDto>();
            foreach (var todo in groupBy)
            {
                var item = todo.FirstOrDefault();
                var lineList = todo.Select(x => x.LineName).OrderBy(x => x).ToList();
                var stdTotal = todo.Select(x => x.StandardConsumption).Sum();
                var stddeliver = todo.FirstOrDefault().DeliveredConsumption;

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
                itemTodolist.StandardConsumption = Math.Round(stdTotal, 2);

                itemTodolist.EstimatedStartTime = item.EstimatedStartTime;
                itemTodolist.EstimatedFinishTime = item.EstimatedFinishTime;

                itemTodolist.AbnormalStatus = item.AbnormalStatus;

                itemTodolist.LineNames = lineList;
                itemTodolist.BuildingID = item.BuildingID;
                todolist.Add(itemTodolist);
            }
            var modelTemp = todolist.Where(x => x.EstimatedFinishTime.Date == currentDate)
               .OrderBy(x => x.EstimatedStartTime).GroupBy(x => new { x.EstimatedStartTime, x.EstimatedFinishTime }).ToList();
            var result = new List<ToDoListDto>();
            foreach (var item in modelTemp)
            {
                result.AddRange(item.OrderByDescending(x => x.GlueName));
            }
            //if (value == morning)
            //{
            //    result = result.Where(x => x.EstimatedFinishTime <= start).ToList();
            //}
            //else
            //{
            //    result = result.Where(x => x.EstimatedFinishTime >= start).ToList();

            //}
            var doneList = new List<ToDoListDto>();
            var doneListTemp = result.Where(x => x.FinishDispatchingTime != null).ToList();
            var groupbyTime = doneListTemp.GroupBy(x => x.EstimatedStartTime).ToList();
            foreach (var item in groupbyTime)
            {
                var doneItem = item.Where(x => x.GlueName.Contains(" + ")).OrderBy(x => x.GlueName).OrderBy(x => x.Supplier).ToList();
                var doneItem2 = item.Where(x => !x.GlueName.Contains(" + ")).OrderBy(x => x.GlueName).OrderBy(x => x.Supplier).ToList();
                var res = doneItem.Concat(doneItem2).ToList();
                doneList.AddRange(res);
            }
            var total = result.Count;
            var doneTotal = doneList.Count;

            var todoTotal = result.Where(x => x.FinishDispatchingTime is null && x.EstimatedFinishTime >= currentTime).Count();
            var delayTotal = result.Where(x => x.FinishDispatchingTime is null && x.EstimatedFinishTime < currentTime).Count();

            return new ToDoListForReturnDto(doneList, doneTotal, todoTotal, delayTotal, total);
        }

        public async Task<ToDoListForReturnDto> Delay(int buildingID)
        {

            var currentTime = DateTime.Now.ToLocalTime();
            var currentDate = currentTime.Date;
            var value = currentTime.ToString("tt", CultureInfo.InvariantCulture);
            // var morning = "AM";
            // var start = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 12, 00, 00);
            var model = await _repoToDoList.FindAll(x =>
                   x.IsDelete == false
                   && x.EstimatedStartTime.Date == currentDate
                   && x.EstimatedFinishTime.Date == currentDate
                   && x.BuildingID == buildingID)
               .ToListAsync();
            var groupBy = model.GroupBy(x => new { x.EstimatedStartTime, x.EstimatedFinishTime, x.GlueNameID });
            var todolist = new List<ToDoListDto>();
            foreach (var todo in groupBy)
            {
                var item = todo.FirstOrDefault();
                var lineList = todo.Select(x => x.LineName).OrderBy(x => x).ToList();
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
                itemTodolist.StandardConsumption = Math.Round(stdTotal, 2);

                itemTodolist.EstimatedStartTime = item.EstimatedStartTime;
                itemTodolist.EstimatedFinishTime = item.EstimatedFinishTime;

                itemTodolist.AbnormalStatus = item.AbnormalStatus;
                itemTodolist.IsDelete = item.IsDelete;

                itemTodolist.LineNames = lineList;
                itemTodolist.BuildingID = item.BuildingID;
                todolist.Add(itemTodolist);
            }
            var modelTemp = todolist.Where(x => x.EstimatedFinishTime.Date == currentDate)
                .OrderBy(x => x.EstimatedStartTime).GroupBy(x => new { x.EstimatedStartTime, x.EstimatedFinishTime }).ToList();
            var result = new List<ToDoListDto>();
            foreach (var item in modelTemp)
            {
                result.AddRange(item.OrderByDescending(x => x.GlueName));
            }
            var delayList = new List<ToDoListDto>();

            var delayListTemp = result.Where(x => x.FinishDispatchingTime is null && x.EstimatedFinishTime < currentTime).ToList();
            var groupbyTime = delayListTemp.GroupBy(x => x.EstimatedStartTime).ToList();
            foreach (var item in groupbyTime)
            {
                var todoItem = item.Where(x => x.GlueName.Contains(" + ")).OrderBy(x => x.GlueName).OrderBy(x => x.Supplier).ToList();
                var todoItem2 = item.Where(x => !x.GlueName.Contains(" + ")).OrderBy(x => x.GlueName).OrderBy(x => x.Supplier).ToList();
                var res = todoItem.Concat(todoItem2).ToList();
                delayList.AddRange(res);

            }
            //if (value == morning)
            //{
            //    result = result.Where(x => x.EstimatedFinishTime <= start).ToList();
            //}
            //else
            //{
            //    result = result.Where(x => x.EstimatedFinishTime >= start).ToList();

            //}
            var total = result.Count;
            var doneTotal = result.Where(x => x.FinishDispatchingTime != null).Count();
            var todoTotal = result.Where(x => x.FinishDispatchingTime is null && x.EstimatedFinishTime >= currentTime).Count();
            var delayTotal = delayList.Count();

            return new ToDoListForReturnDto(delayList, doneTotal, todoTotal, delayTotal, total);
        }

        public async Task<ToDoListForReturnDto> ToDo(int buildingID)
        {
            var currentTime = DateTime.Now.ToLocalTime();
            var currentDate = currentTime.Date;
            var value = currentTime.ToString("tt", CultureInfo.InvariantCulture);
            var morning = "AM";
            var start = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 12, 00, 00);
            var model = await _repoToDoList.FindAll(x =>
                   x.IsDelete == false
                   && x.EstimatedStartTime.Date == currentDate
                   && x.EstimatedFinishTime.Date == currentDate
                   && x.BuildingID == buildingID)
               .ToListAsync();

            var groupBy = model.GroupBy(x => new { x.EstimatedStartTime, x.EstimatedFinishTime, x.GlueNameID });
            var todolist = new List<ToDoListDto>();
            foreach (var todo in groupBy)
            {
                var item = todo.FirstOrDefault();
                var lineList = todo.Select(x => x.LineName).OrderBy(x => x).ToList();
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
                itemTodolist.StandardConsumption = Math.Round(stdTotal, 2);

                itemTodolist.EstimatedStartTime = item.EstimatedStartTime;
                itemTodolist.EstimatedFinishTime = item.EstimatedFinishTime;

                itemTodolist.AbnormalStatus = item.AbnormalStatus;
                itemTodolist.IsDelete = item.IsDelete;

                itemTodolist.LineNames = lineList;
                itemTodolist.BuildingID = item.BuildingID;
                todolist.Add(itemTodolist);
            }
            var modelTemp = todolist.Where(x => x.EstimatedFinishTime.Date == currentDate)
                .OrderBy(x => x.EstimatedStartTime).GroupBy(x => new { x.EstimatedStartTime, x.EstimatedFinishTime }).ToList();
            var result = new List<ToDoListDto>();
            foreach (var item in modelTemp)
            {
                result.AddRange(item.OrderByDescending(x => x.GlueName));
            }
            var todoList = new List<ToDoListDto>();
            var groupbyTime = result.Where(x => x.FinishDispatchingTime is null).GroupBy(x => x.EstimatedStartTime).ToList();
            foreach (var item in groupbyTime)
            {
                var todoItem = item.Where(x => x.GlueName.Contains(" + ")).OrderBy(x => x.GlueName).OrderBy(x => x.Supplier).ToList();
                var todoItem2 = item.Where(x => !x.GlueName.Contains(" + ")).OrderBy(x => x.GlueName).OrderBy(x => x.Supplier).ToList();
                var res = todoItem.Concat(todoItem2).ToList();
                todoList.AddRange(res);
            }
            var delayTotal = todoList.Where(x => x.EstimatedFinishTime < currentTime).Count();
            if (value == morning)
            {
                todoList = todoList.Where(x => x.EstimatedFinishTime <= start && x.EstimatedFinishTime >= currentTime).ToList();
            }
            else
            {
                todoList = todoList.Where(x => x.EstimatedFinishTime >= start && x.EstimatedFinishTime >= currentTime).ToList();
            }
            var total = result.Count();
            //if (value == morning)
            //{
            //    total = result.Where(x => x.EstimatedFinishTime <= start && (x.EstimatedFinishTime >= currentTime || x.EstimatedFinishTime <= currentTime)).Count();
            //}
            //else
            //{
            //    total = result.Where(x => x.EstimatedFinishTime >= start && (x.EstimatedFinishTime >= currentTime || x.EstimatedFinishTime <= currentTime)).Count();
            //}
            var doneTotal = result.Where(x => x.FinishDispatchingTime != null).Count();
            var todoTotal = todoList.Count();

            return new ToDoListForReturnDto(todoList, doneTotal, todoTotal, delayTotal, total);
        }

        public async Task<MixingInfo> Mix(MixingInfoForCreateDto mixing)
        {
            try
            {
                var item = _mapper.Map<MixingInfoForCreateDto, MixingInfo>(mixing);
                item.Code = CodeUtility.RandomString(8);
                item.CreatedTime = DateTime.Now;
                var glue = await _repoGlue.FindAll().FirstOrDefaultAsync(x => x.isShow == true && x.ID == mixing.GlueID);
                item.ExpiredTime = DateTime.Now.AddHours(glue.ExpiredTime);
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
            var dispatch = model.Dispatches.Select(x => x.Amount).ToList();
            var total = dispatch.Sum();
            var list = _repoToDoList.FindAll(x => x.MixingInfoID == model.MixingInfoID).ToList();
            list.ForEach(x =>
            {
                x.FinishDispatchingTime = model.FinishTime.Value.ToLocalTime();
                x.StartDispatchingTime = model.StartTime.Value.ToLocalTime();
                x.Status = model.FinishTime <= x.EstimatedFinishTime;
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
                x.FinishMixingTime = model.FinishTime.Value.ToLocalTime();
                x.StartMixingTime = model.StartTime.Value.ToLocalTime();
                x.MixedConsumption = model.Amount;
                x.MixingInfoID = model.MixingInfoID;
            });
            _repoToDoList.UpdateRange(list);
            _repoToDoList.Save();
        }

        public void UpdateStiringTimeRange(ToDoListForUpdateDto model)
        {
            var list = _repoToDoList.FindAll(x => x.MixingInfoID == model.MixingInfoID).ToList();
            list.ForEach(x =>
            {
                x.FinishStirTime = model.FinishTime.Value.ToLocalTime();
                x.StartStirTime = model.StartTime.Value.ToLocalTime();
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
                    var printTime = DateTime.Now.ToLocalTime();
                    mixing.PrintTime = printTime;
                    _repoMixingInfo.Update(mixing);
                    _repoMixingInfo.Save();
                    var todolist = _repoToDoList.FindAll(x => x.MixingInfoID == mixingInfoID).ToList();
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
            var item = _repoMixingInfo.FindAll(x => x.ID == mixingInfoID).Include(x => x.MixingInfoDetails).FirstOrDefault();
            return item;
        }

        public async Task<object> Dispatch(DispatchParams todolistDto)
        {
            var dispatches = await _repoDispatch.FindAll(x => !x.IsDelete && x.MixingInfoID == todolistDto.MixingInfoID && x.CreatedTime.Date == todolistDto.EstimatedFinishTime.Date)
                .Include(x => x.Building)
                .Select(x => new DispatchTodolistDto
                {
                    ID = x.ID,
                    LineID = x.LineID,
                    Line = x.Building.Name,
                    MixingInfoID = x.MixingInfoID,
                    Real = x.Amount,
                    StationID = x.StationID,
                    StandardAmount = x.StandardAmount,
                    CreatedTime = x.CreatedTime,
                    DeliveryTime = x.DeliveryTime
                })
                .ToListAsync();
            return dispatches;
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

        public async Task<object> GenerateToDoList(List<int> plans)
        {
            if (plans.Count == 0) return new
            {
                status = false,
                message = "Không có kế hoạch làm việc nào được gửi lên server"
            };
            var currentTime = DateTime.Now;
            var currentDate = currentTime.Date;
            var plansModel = await _repoPlan.FindAll(x => plans.Contains(x.ID))
                .Include(x => x.Building)
                .ThenInclude(x => x.LunchTime)
                .Include(x => x.BPFCEstablish)
                    .ThenInclude(x => x.Glues)
                    .ThenInclude(x => x.GlueName)
                .Include(x => x.BPFCEstablish)
                    .ThenInclude(x => x.Glues)
                    .ThenInclude(x => x.GlueIngredients)
                    .ThenInclude(x => x.Ingredient)
                    .ThenInclude(x => x.Supplier)
                    .SelectMany(x => x.BPFCEstablish.Glues.Where(x => x.isShow), (plan, glue) => new
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

            if (plansModel.Count == 0) return new
            {
                status = false,
                message = "Không có danh sách keo nào cho kế hoạch làm việc này!"
            };
            var value = plansModel.FirstOrDefault();

            //var line = await _repoBuilding.FindAll(x => x.ID == value.Building.ID).FirstOrDefaultAsync();
            //if (line is null) return new
            //{
            //    status = false,
            //    message = "Không tìm thấy tòa nhà nào trong hệ thống!"
            //};

            //var building = await _repoBuilding.FindAll(x => x.ID == line.ParentID)
            //   .Include(x => x.LunchTime).FirstOrDefaultAsync();
            //if (building is null) return new
            //{
            //    status = false,
            //    message = "Không tìm thấy tòa nhà nào trong hệ thống!"
            //};

            //if (building.LunchTime is null) return new
            //{
            //    status = false,
            //    message = $"Tòa nhà {building.Name} chưa cài đặt giờ ăn trưa!"
            //};

            //var startLunchTimeBuilding = building.LunchTime.StartTime;
            //var endLunchTimeBuilding = building.LunchTime.EndTime;


            var todolist = new List<ToDoListDto>();

            var glues = plansModel.GroupBy(x => x.GlueName).ToList();

            foreach (var glue in glues)
            {
                foreach (var item in glue)
                {
                    if (item.Building.LunchTime is null) return new
                    {
                        status = false,
                        message = $"Vui lòng thêm giờ ăn trưa cho chuyền {item.Building.Name}"
                    };
                    if (item.ChemicalA is null) return new
                    {
                        status = false,
                        message = $"Keo {item.GlueName} không có hóa chất A!"
                    };
                    var checmicalA = item.ChemicalA;
                    double replacementFrequency = checmicalA.ReplacementFrequency;
                    if (replacementFrequency == 0) return new
                    {
                        status = false,
                        message = $"Cột Replacement Frequency của hóa chất {checmicalA.Name} chưa gán giờ làm việc nên không thể tạo danh sách việc làm được!"
                    };
                    var startLunchTimeBuilding = item.Building.LunchTime.StartTime;
                    var endLunchTimeBuilding = item.Building.LunchTime.EndTime;
                    var startLunchTime = item.DueDate.Date.Add(new TimeSpan(startLunchTimeBuilding.Hour, startLunchTimeBuilding.Minute, 0));
                    var endLunchTime = item.DueDate.Date.Add(new TimeSpan(endLunchTimeBuilding.Hour, endLunchTimeBuilding.Minute, 0));

                    var finishWorkingTime = item.FinishWorkingTime;

                    double prepareTime = checmicalA.PrepareTime;

                    var kgPair = item.Consumption.ToDouble() / 1000;
                    double lunchHour = (endLunchTime - startLunchTime).TotalHours;
                    if (item.DueDate.Date != currentDate && item.CreatedDate != currentDate)
                    {
                        var estimatedTime = item.DueDate.Date.Add(new TimeSpan(7, 30, 00)) - TimeSpan.FromHours(prepareTime);
                        var estimatedStartTimeTemp = estimatedTime;
                        var fwt = new DateTime();

                        while (true)
                        {
                            fwt = estimatedStartTimeTemp.AddHours(prepareTime);
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
                            todo.BuildingID = item.Building.ParentID.Value;
                            todo.StandardConsumption = kgPair * (double)item.HourlyOutput * replacementFrequency;

                            if (estimatedStartTimeTemp >= finishWorkingTime) break;
                            // Rot vao khoang TG an trua tinh lai consumption 
                            if (estimatedStartTimeTemp >= startLunchTime && estimatedStartTimeTemp <= endLunchTime)
                            {
                                estimatedStartTimeTemp = endLunchTime;
                                todo.EstimatedStartTime = estimatedStartTimeTemp;  // SLT 13:30
                                todo.EstimatedFinishTime = estimatedStartTimeTemp.AddHours(prepareTime); // 13:30 + preparetime
                                                                                                         // 13:30 + 2 = 15:30 >= 14:00
                                var estimatedStartNextTimeTemp = estimatedStartTimeTemp.AddHours(replacementFrequency);
                                if (estimatedStartNextTimeTemp > finishWorkingTime)
                                {
                                    var recalculateReplacementFrequency = (finishWorkingTime - estimatedStartTimeTemp).TotalHours;
                                    todo.StandardConsumption = kgPair * (double)item.HourlyOutput * recalculateReplacementFrequency;
                                }

                            }
                            else
                            {
                                var estimatedStartNextTimeTemp = estimatedStartTimeTemp.AddHours(replacementFrequency);
                                todo.StandardConsumption = kgPair * (double)item.HourlyOutput * replacementFrequency;

                                if (estimatedStartNextTimeTemp > finishWorkingTime)
                                {
                                    replacementFrequency = (finishWorkingTime - estimatedStartTimeTemp).TotalHours;
                                    todo.StandardConsumption = kgPair * (double)item.HourlyOutput * replacementFrequency;

                                }
                                else if (estimatedStartTimeTemp < startLunchTime && estimatedStartNextTimeTemp < finishWorkingTime && estimatedStartNextTimeTemp <= endLunchTime && estimatedStartNextTimeTemp > startLunchTime)
                                {
                                    // neu TGBG tiep theo ma nam trong gio an trua thi gan bang TGKT an trua
                                    estimatedStartNextTimeTemp = estimatedStartNextTimeTemp <= endLunchTime && estimatedStartNextTimeTemp >= startLunchTime ? endLunchTime : estimatedStartNextTimeTemp;
                                    var recalculateReplacementFrequency = (estimatedStartNextTimeTemp - estimatedStartTimeTemp).TotalHours - (endLunchTime - startLunchTime).TotalHours;
                                    replacementFrequency = recalculateReplacementFrequency;
                                    todo.StandardConsumption = kgPair * (double)item.HourlyOutput * replacementFrequency;

                                }

                                todo.EstimatedStartTime = estimatedStartTimeTemp;
                                todo.EstimatedFinishTime = fwt;
                            }
                            replacementFrequency = checmicalA.ReplacementFrequency;
                            estimatedStartTimeTemp = estimatedStartTimeTemp.AddHours(replacementFrequency);
                            todolist.Add(todo);
                        }
                        // Neu tao tu ngay hom truoc thi lay moc thoi gian la 7:30
                    }
                    else
                    {
                        var estimatedStartTimeTemp = item.StartWorkingTime;
                        var fwt = new DateTime();

                        while (true)
                        {
                            fwt = estimatedStartTimeTemp.AddHours(prepareTime);
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
                            todo.BuildingID = item.Building.ParentID.Value;
                            todo.StandardConsumption = kgPair * (double)item.HourlyOutput * replacementFrequency;
                            if (estimatedStartTimeTemp >= finishWorkingTime) break;
                            // Rot vao khoang TG an trua tinh lai consumption 
                            if (estimatedStartTimeTemp >= startLunchTime && estimatedStartTimeTemp <= endLunchTime)
                            {
                                estimatedStartTimeTemp = endLunchTime;
                                todo.EstimatedStartTime = estimatedStartTimeTemp;  // SLT 13:30
                                todo.EstimatedFinishTime = estimatedStartTimeTemp.AddHours(prepareTime); // 13:30 + preparetime
                                                                                                         // 13:30 + 2 = 15:30 >= 14:00
                                var estimatedStartNextTimeTemp = estimatedStartTimeTemp.AddHours(replacementFrequency);
                                if (estimatedStartNextTimeTemp > finishWorkingTime)
                                {
                                    var recalculateReplacementFrequency = (finishWorkingTime - estimatedStartTimeTemp).TotalHours;
                                    todo.StandardConsumption = kgPair * (double)item.HourlyOutput * recalculateReplacementFrequency;
                                }
                            }
                            else
                            {
                                var estimatedStartNextTimeTemp = estimatedStartTimeTemp.AddHours(replacementFrequency);
                                var standardConsumption = kgPair * (double)item.HourlyOutput * replacementFrequency;
                                todo.StandardConsumption = standardConsumption;
                                if (estimatedStartNextTimeTemp > finishWorkingTime)
                                {
                                    var recalculateReplacementFrequency = (finishWorkingTime - estimatedStartTimeTemp).TotalHours;

                                    todo.StandardConsumption = kgPair * (double)item.HourlyOutput * recalculateReplacementFrequency;
                                }
                                else if (estimatedStartTimeTemp < startLunchTime && estimatedStartNextTimeTemp < finishWorkingTime && estimatedStartNextTimeTemp <= endLunchTime && estimatedStartNextTimeTemp > startLunchTime)
                                {
                                    // neu TGBG tiep theo ma nam trong gio an trua thi gan bang TGKT an trua
                                    estimatedStartNextTimeTemp = estimatedStartNextTimeTemp <= endLunchTime && estimatedStartNextTimeTemp >= startLunchTime ? endLunchTime : estimatedStartNextTimeTemp;
                                    var recalculateReplacementFrequency = (estimatedStartNextTimeTemp - estimatedStartTimeTemp).TotalHours - (endLunchTime - startLunchTime).TotalHours;
                                    todo.StandardConsumption = kgPair * (double)item.HourlyOutput * recalculateReplacementFrequency;
                                }

                                todo.EstimatedStartTime = estimatedStartTimeTemp;

                                todo.EstimatedFinishTime = fwt;
                            }
                            replacementFrequency = checmicalA.ReplacementFrequency;
                            estimatedStartTimeTemp = estimatedStartTimeTemp.AddHours(replacementFrequency);
                            todolist.Add(todo);
                        }

                    }
                }
            }
            try
            {
                var model = _mapper.Map<List<ToDoList>>(todolist);
                _repoToDoList.AddRange(model);
                _repoToDoList.Save();
                return new
                {
                    status = true,
                    message = "Tạo danh sách việc làm thành công!"
                };
            }
            catch (Exception)
            {
                return new
                {
                    status = false,
                    message = "Tạo danh sách việc làm thất bại!"
                };
            }
        }

        public Byte[] ExcelExportDoneList(List<ToDoListForExportDto> model)
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.Commercial;
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                var memoryStream = new MemoryStream();
                using (ExcelPackage p = new ExcelPackage(memoryStream))
                {
                    // đặt tên người tạo file
                    p.Workbook.Properties.Author = "Henry Pham";

                    // đặt tiêu đề cho file
                    p.Workbook.Properties.Title = "Done List";
                    //Tạo một sheet để làm việc trên đó
                    p.Workbook.Worksheets.Add("Done List");

                    // lấy sheet vừa add ra để thao tác
                    ExcelWorksheet ws = p.Workbook.Worksheets["Done List"];

                    // đặt tên cho sheet
                    ws.Name = "Done List";
                    // fontsize mặc định cho cả sheet
                    ws.Cells.Style.Font.Size = 11;
                    // font family mặc định cho cả sheet
                    ws.Cells.Style.Font.Name = "Calibri";
                    var headerArray = new List<string>()
                    {
                        "Sequence",
                        "Line",
                        "Station",
                        "Model Name",
                        "Model NO",
                        "Article NO",
                        "Supplier",
                        "Glue",
                        "SMT",
                        "FMT",
                        "SST",
                        "Stir CT",
                        "AVG. RPM",
                        "FST",
                        "PT",
                        "SDT",
                        "FDT",
                        "Std. Con.",
                        "Mixed Con.",
                        "Delivered Con.",
                        "Status",
                        "EST",
                        "EFT",
                    };
                    int headerRowIndex = 1;
                    foreach (var headerItem in headerArray.Select((value, i) => new { i, value }))
                    {
                        var headerColIndex = headerItem.i + 1;
                        var headerExcelRange = ws.Cells[headerRowIndex, headerColIndex];
                        headerExcelRange.Value = headerItem.value;
                        headerExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        headerExcelRange.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#1F4E78"));
                        headerExcelRange.Style.Font.Color.SetColor(ColorTranslator.FromHtml("#fff"));
                        headerExcelRange.Style.Font.Size = 16;

                    }

                    int bodyRowIndex = 1;
                    int bodyColIndex = 1;

                    foreach (var bodyItem in model)
                    {
                        bodyColIndex = 1;
                        bodyRowIndex++;

                        var sequenceExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                        sequenceExcelRange.Value = bodyItem.Sequence;
                        sequenceExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        sequenceExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                        var lineExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                        lineExcelRange.Value = bodyItem.Line;
                        lineExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        lineExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                        var stationExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                        stationExcelRange.Value = bodyItem.Station;
                        stationExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        stationExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                        var modelNameExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                        modelNameExcelRange.Value = bodyItem.ModelName;
                        modelNameExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        modelNameExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                        var modelNOExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                        modelNOExcelRange.Value = bodyItem.ModelNO;
                        modelNOExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        modelNOExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                        var articleNOExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                        articleNOExcelRange.Value = bodyItem.ArticleNO;
                        articleNOExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        articleNOExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                        var supplierExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                        supplierExcelRange.Value = bodyItem.Supplier;
                        supplierExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        supplierExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                        var glueNameExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                        glueNameExcelRange.Value = bodyItem.GlueName;
                        glueNameExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        glueNameExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                        var SMTExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                        SMTExcelRange.Value = bodyItem.StartMixingTime == null || !bodyItem.GlueName.Contains(" + ") ? "N/A" : bodyItem.StartMixingTime.Value.ToString("HH:mm");
                        SMTExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        SMTExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                        var FMTExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                        FMTExcelRange.Value = bodyItem.FinishMixingTime == null || !bodyItem.GlueName.Contains(" + ") ? "N/A" : bodyItem.FinishMixingTime.Value.ToString("HH:mm");
                        FMTExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        FMTExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                        var SSTExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                        SSTExcelRange.Value = bodyItem.StartStirTime == null ? "N/A" : bodyItem.StartStirTime.Value.ToString("HH:mm");
                        SSTExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        SSTExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                        var SCTExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                        SCTExcelRange.Value = bodyItem.StirCicleTime;
                        SCTExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        SCTExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                        var AVGRPMExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                        AVGRPMExcelRange.Value = bodyItem.AverageRPM;
                        AVGRPMExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        AVGRPMExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                        var FSTExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                        FSTExcelRange.Value = bodyItem.FinishStirTime == null ? "N/A" : bodyItem.FinishStirTime.Value.ToString("HH:mm");
                        FSTExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        FSTExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                        var PTExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                        PTExcelRange.Value = bodyItem.PrintTime == null ? "N/A" : bodyItem.PrintTime.Value.ToString("HH:mm");
                        PTExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        PTExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                        var SDTExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                        SDTExcelRange.Value = bodyItem.StartDispatchingTime == null ? "N/A" : bodyItem.StartDispatchingTime.Value.ToString("HH:mm");
                        SDTExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        SDTExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                        var FDTExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                        FDTExcelRange.Value = bodyItem.FinishDispatchingTime == null ? "N/A" : bodyItem.FinishDispatchingTime.Value.ToString("HH:mm");
                        FDTExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        FDTExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                        var StdConExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                        StdConExcelRange.Value = $"{Math.Round(bodyItem.StandardConsumption, 2)}kg";
                        StdConExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        StdConExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                        var MixedConExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                        MixedConExcelRange.Value = !bodyItem.GlueName.Contains(" + ") ? "N/A" : $"{Math.Round(bodyItem.MixedConsumption, 2)}kg";
                        MixedConExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        MixedConExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                        var deliverdConExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                        deliverdConExcelRange.Value = $"{Math.Round(bodyItem.DeliveredConsumption, 2)}kg";
                        deliverdConExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        deliverdConExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                        var statusRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                        statusRange.Value = bodyItem.Status;
                        statusRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        statusRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));
                        statusRange.Style.Font.Color.SetColor(bodyItem.Status == "Pass" ? ColorTranslator.FromHtml("#28a745") : ColorTranslator.FromHtml("#a30000")); // #a30000
                        var ESTExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                        ESTExcelRange.Value = bodyItem.EstimatedStartTime == null ? "N/A" : bodyItem.EstimatedStartTime.ToString("HH:mm");
                        ESTExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ESTExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));

                        var EFTExcelRange = ws.Cells[bodyRowIndex, bodyColIndex++];
                        EFTExcelRange.Value = bodyItem.EstimatedFinishTime == null ? "N/A" : bodyItem.EstimatedFinishTime.ToString("HH:mm");
                        EFTExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        EFTExcelRange.Style.Fill.BackgroundColor.SetColor(bodyItem.Sequence % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));
                    } //#BDD7EE

                    int mergeFromColIndex = 1;
                    int mergeFromRowIndex = 2;
                    int mergeToRowIndex = 1;
                    foreach (var item in model.GroupBy(x => x.Sequence))
                    {
                        mergeToRowIndex += item.Count();
                        var sequenceExcelRange = ws.Cells[mergeFromRowIndex, mergeFromColIndex, mergeToRowIndex, mergeFromColIndex];
                        sequenceExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        sequenceExcelRange.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#fff"));

                        sequenceExcelRange.Merge = true;
                        //ws.Cells[mergeFromRowIndex, mergeFromColIndex, mergeToRowIndex, mergeFromColIndex].Value = item.Key;
                        sequenceExcelRange.Style.Font.Size = 20;
                        sequenceExcelRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        sequenceExcelRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        sequenceExcelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        sequenceExcelRange.Style.Fill.BackgroundColor.SetColor(item.Key % 2 == 0 ? ColorTranslator.FromHtml("#BDD7EE") : ColorTranslator.FromHtml("#FFF"));
                        mergeFromRowIndex = mergeToRowIndex + 1;
                    }
                    //Make all text fit the cells
                    //ws.Cells[ws.Dimension.Address].AutoFitColumns();
                    ws.Cells[ws.Dimension.Address].Style.Font.Bold = true;
                    ws.Cells[ws.Dimension.Address].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ws.Cells[ws.Dimension.Address].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    //make the borders of cell F6 thick
                    ws.Cells[ws.Dimension.Address].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    ws.Cells[ws.Dimension.Address].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    ws.Cells[ws.Dimension.Address].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    ws.Cells[ws.Dimension.Address].Style.Border.Left.Style = ExcelBorderStyle.Thin;

                    ws.Cells[ws.Dimension.Address].AutoFitColumns();
                    //Lưu file lại
                    Byte[] bin = p.GetAsByteArray();
                    return bin;
                }
            }
            catch (Exception ex)
            {
                var mes = ex.Message;
                Console.WriteLine(mes);
                return new Byte[] { };
            }
        }

        public async Task<Byte[]> ExportExcelToDoListByBuilding(int buildingID)
        {
            var currentTime = DateTime.Now.ToLocalTime();
            var currentDate = currentTime.Date;
            var model = await _repoToDoList.FindAll(x =>
                   x.IsDelete == false
                   && x.EstimatedStartTime.Date == currentDate
                   && x.EstimatedFinishTime.Date == currentDate
                   && x.FinishDispatchingTime != null
                   && x.BuildingID == buildingID)
                .Include(x => x.Plan)
                    .ThenInclude(x => x.BPFCEstablish)
                    .ThenInclude(x => x.ModelName)
                .Include(x => x.Plan)
                    .ThenInclude(x => x.BPFCEstablish)
                    .ThenInclude(x => x.ModelNo)
                 .Include(x => x.Plan)
                    .ThenInclude(x => x.BPFCEstablish)
                    .ThenInclude(x => x.ArticleNo)
                .Include(x => x.Plan)
                    .ThenInclude(x => x.Stations)
                .Select(x => new
                {
                    x.Plan,
                    x.EstimatedFinishTime,
                    x.EstimatedStartTime,
                    x.FinishDispatchingTime,
                    x.StartDispatchingTime,
                    x.FinishStirTime,
                    x.StartStirTime,
                    x.StartMixingTime,
                    x.FinishMixingTime,
                    x.PrintTime,
                    x.MixingInfoID,
                    x.MixedConsumption,
                    x.DeliveredConsumption,
                    x.StandardConsumption,
                    x.LineName,
                    x.Supplier,
                    x.GlueID,
                    x.GlueName,
                    x.Status,

                    Station = x.Plan.Stations.FirstOrDefault(a => a.GlueID == x.GlueID) == null ? 0 : x.Plan.Stations.FirstOrDefault(a => a.GlueID == x.GlueID).Amount,
                    ModelName = x.Plan.BPFCEstablish.ModelName == null ? "N/A" : x.Plan.BPFCEstablish.ModelName.Name,
                    ModelNO = x.Plan.BPFCEstablish.ModelNo == null ? "N/A" : x.Plan.BPFCEstablish.ModelNo.Name,
                    ArticleNO = x.Plan.BPFCEstablish.ArticleNo == null ? "N/A" : x.Plan.BPFCEstablish.ArticleNo.Name,
                })
               .ToListAsync();
            var exportList = new List<ToDoListForExportDto>();
            var groupBy = model.GroupBy(x => new { x.EstimatedStartTime, x.GlueName })
                .OrderBy(x => x.Key.EstimatedStartTime)
                .ThenBy(x => x.Key.GlueName)
                .ToList();
            foreach (var groupByItem in groupBy.Select((value, i) => new { i, value }))
            {
                var sequence = groupByItem.i + 1;
                foreach (var item in groupByItem.value.OrderBy(x => x.LineName).ToList())
                {
                    var stirModel = await _repoStir.FindAll(a => a.MixingInfoID == item.MixingInfoID).ToListAsync();
                    var averageRPM = item.MixingInfoID == 0 || stirModel.Count == 0 ? 0 : stirModel.Average(x => x.RPM);
                    var stirCicleTime = item.MixingInfoID == 0 || stirModel.Count == 0 ? 0 : stirModel.Sum(x => x.ActualDuration);
                    var exportItem = new ToDoListForExportDto();
                    exportItem.Sequence = sequence;
                    exportItem.Line = item.LineName;
                    exportItem.Station = item.Station;
                    exportItem.Supplier = item.Supplier;
                    var modelName = item.Plan.BPFCEstablish.ModelName;
                    var modelNO = item.Plan.BPFCEstablish.ModelName;
                    var articleNO = item.Plan.BPFCEstablish.ModelName;

                    exportItem.ModelName = item.ModelName;
                    exportItem.ModelNO = item.ModelNO;
                    exportItem.ArticleNO = item.ArticleNO;
                    exportItem.GlueName = item.GlueName;

                    exportItem.StartMixingTime = item.StartMixingTime;
                    exportItem.FinishMixingTime = item.FinishMixingTime;

                    exportItem.StartStirTime = item.StartStirTime;
                    exportItem.FinishStirTime = item.FinishStirTime;

                    exportItem.StartStirTime = item.StartStirTime;
                    exportItem.FinishStirTime = item.FinishStirTime;
                    exportItem.PrintTime = item.PrintTime;
                    exportItem.StirCicleTime = stirCicleTime;
                    exportItem.AverageRPM = averageRPM;

                    exportItem.StartDispatchingTime = item.StartDispatchingTime;
                    exportItem.FinishDispatchingTime = item.FinishDispatchingTime;

                    exportItem.StandardConsumption = item.StandardConsumption;

                    exportItem.MixedConsumption = item.MixedConsumption;
                    exportItem.DeliveredConsumption = item.DeliveredConsumption;
                    exportItem.Status = item.Status ? "Pass" : "Fail";

                    exportItem.EstimatedStartTime = item.EstimatedStartTime;
                    exportItem.EstimatedFinishTime = item.EstimatedFinishTime;
                    exportList.Add(exportItem);
                }
            }
            return ExcelExportDoneList(exportList);
        }

        MixingDetailForResponse MixingDetail(string glueName, DateTime date)
        {
            var mixedModel = _repoMixingInfo.FindAll(x => x.CreatedTime.Date == date)
                         .Include(x => x.MixingInfoDetails)
                         .Include(x => x.Glue)
                             .ThenInclude(x => x.GlueName)
                         .Where(x => x.Glue.GlueName.Name == glueName).ToList();
            var count = _repoMixingInfo.FindAll()
                            .Count();
            double mixedCon = 0;
            double deliveryCon = 0;
            if (count == 0) // Chua add cai nao trong db thi return ve rong
            {
                return new MixingDetailForResponse();
            }
            else if (mixedModel.Count == 0) // neu ngay hien tai khong co thi lay cua ngay truoc do
            {
                date = date.AddDays(-1);
                return MixingDetail(glueName, date);
            }
            else // ngay hien tai co thi lay cua ngay hien tai
            {
                var mixingModel = mixedModel.FirstOrDefault();
                var deliveryModel = _repoDispatch.FindAll(x => mixedModel.Select(a => a.ID).Contains(x.MixingInfoID) && x.CreatedTime.Date == date);
                deliveryCon = deliveryModel.Count() == 0 ? 0 : deliveryModel.Average(x => x.Amount);
                mixedCon = mixedModel.SelectMany(x => x.MixingInfoDetails).Average(x => x.Amount);
                return new MixingDetailForResponse(mixedCon, mixingModel.CreatedTime, deliveryCon, mixingModel.CreatedTime);
            }


        }

        public MixingDetailForResponse GetMixingDetail(MixingDetailParams obj)
        {
            var glueName = obj.GlueName.ToSafetyString().Trim();
            var date = DateTime.Now.ToLocalTime().Date;
            return MixingDetail(glueName, date);
        }
    }
}
