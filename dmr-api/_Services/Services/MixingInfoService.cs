using AutoMapper;
using AutoMapper.QueryableExtensions;
using DMR_API._Repositories.Interface;
using DMR_API._Services.Interface;
using DMR_API.DTO;
using DMR_API.Helpers;
using DMR_API.Models;
using DMR_API.Data;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace DMR_API._Services.Services
{
    public class MixingInfoService : IMixingInfoService
    {
        private readonly IMixingInfoRepository _repoMixingInfor;
        private readonly IMixingService _repoMixing;
        private readonly IGlueRepository _repoGlue;
        private readonly IRawDataRepository _repoRawData;
        private readonly IStirRepository _repoStir;
        private readonly IMongoRepository<DMR_API.Data.MongoModels.RawData> _rowDataRepository;
        private readonly ISettingRepository _repoSetting;
        private readonly IToDoListService _toDoListService;
        private readonly IMixingInfoDetailRepository _repoMixingInfoDetail;
        private readonly IMapper _mapper;
        private readonly MapperConfiguration _configMapper;

        public MixingInfoService(
            IMixingInfoRepository repoMixingInfor,
            IMixingService repoMixing,
            IRawDataRepository repoRawData,
            IMongoRepository<DMR_API.Data.MongoModels.RawData> rowDataRepository,
            ISettingRepository repoSetting,
            IToDoListService toDoListService,
             IMixingInfoDetailRepository _repoMixingInfoDetail,

        IMapper mapper, IGlueRepository repoGlue,
            IStirRepository repoStir,
            MapperConfiguration configMapper)
        {
            _repoMixingInfor = repoMixingInfor;
            _repoMixing = repoMixing;
            _repoGlue = repoGlue;
            _mapper = mapper;
            _repoStir = repoStir;
            _rowDataRepository = rowDataRepository;
            _repoRawData = repoRawData;
            _configMapper = configMapper;
            _repoSetting = repoSetting;
            _toDoListService = toDoListService;
            this._repoMixingInfoDetail = _repoMixingInfoDetail;
        }

        public MixingInfo Mixing(MixingInfoForCreateDto mixing)
        {

            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    string code = CodeUtility.RandomString(8);
                    while (true)
                    {
                        var checkExistingCode = _repoMixingInfor.FindAll(x => x.Code == code).Any();
                        if (checkExistingCode is true)
                        {
                            code = CodeUtility.RandomString(8);
                        }
                        else
                        {
                            break;
                        }
                    }
                    var item = _mapper.Map<MixingInfoForCreateDto, MixingInfo>(mixing);
                    item.Code = code;
                    item.CreatedTime = DateTime.Now;
                    var glue = _repoGlue.FindAll().FirstOrDefault(x => x.isShow == true && x.ID == mixing.GlueID);
                    item.ExpiredTime = DateTime.Now.AddMinutes(glue.ExpiredTime);
                    _repoMixingInfor.Add(item);
                    //await _repoMixingInfor.SaveAll();
                    _repoMixingInfor.Save();

                    // await _repoMixing.AddOrUpdate(item.ID);
                    var todo = new ToDoListForUpdateDto()
                    {
                        GlueName = mixing.GlueName,
                        StartTime = mixing.StartTime,
                        FinishTime = mixing.EndTime,
                        MixingInfoID = item.ID,
                        EstimatedFinishTime = mixing.EstimatedFinishTime,
                        EstimatedStartTime = mixing.EstimatedStartTime,
                        Amount = mixing.ChemicalA.ToDouble() + mixing.ChemicalB.ToDouble() + mixing.ChemicalC.ToDouble() + mixing.ChemicalD.ToDouble() + mixing.ChemicalE.ToDouble()
                    };
                    _toDoListService.UpdateMixingTimeRange(todo);
                    scope.Complete();
                    return item;
                }
                catch
                {
                    scope.Dispose();
                    return new MixingInfo();
                }
            }

        }

        public async Task<List<MixingInfoDto>> GetMixingInfoByGlueName(string glueName)
        {
            return await _repoMixingInfor.FindAll()
            .Include(x => x.Glue)
            .ThenInclude(x => x.GlueIngredients)
            .ThenInclude(x => x.Ingredient)
            .Where(x => x.GlueName.Equals(glueName) && x.Glue.isShow == true)
            .ProjectTo<MixingInfoDto>(_configMapper)
            .OrderByDescending(x => x.ID).ToListAsync();
        }

        public async Task<object> Stir(string glueName)
        {
            var currentDate = DateTime.Now.Date;
            var STIRRED = 1;
            var NOT_STIRRED_YET = 0;
            var NA = 2;

            var minDate = DateTime.MinValue;

            var model = from a in _repoMixingInfor.FindAll(x => x.GlueName.Equals(glueName) && x.CreatedTime.Date == currentDate)
                        .Include(x => x.MixingInfoDetails)
                        .Include(x=>x.Glue)
                                    .ThenInclude(x => x.GlueIngredients)
                                    .ThenInclude(x => x.Ingredient)
                                    .ThenInclude(x => x.GlueType)
                                    .Select(a=> new {
                                        a.GlueName,
                                        a.CreatedTime,
                                        Qty = a.MixingInfoDetails.Select(a => a.Amount).Sum(),
                                        a.ID,
                                        a.Glue.GlueIngredients.FirstOrDefault(x=>x.Position == "A").Ingredient.GlueType
                                    })
                        join b in _repoStir.FindAll().Include(x => x.Setting).Where(x => x.GlueName.Equals(glueName) && x.CreatedTime.Date == currentDate) on a.ID equals b.MixingInfoID into gj
                        from ab in gj.DefaultIfEmpty()
                        select new
                        {
                            a.ID,
                            StirID = ab.ID,
                            a.GlueName,
                            a.Qty,
                            a.CreatedTime,
                            ab.StartTime ,
                            ab.EndTime,
                            ab.SettingID,
                            ab.Setting,
                            MachineType = ab.SettingID == null ? string.Empty : ab.Setting.MachineType,
                            ab.Status,
                            ab.RPM,
                            ab.TotalMinutes,
                            a.GlueType,
                            MixingStatus = ab.ID > 0 && ab.SettingID == null ? NA : ab.SettingID != null ? STIRRED : NOT_STIRRED_YET
                        };
            return await model.ToListAsync();
        }

        public async Task<object> GetRPM(int mixingInfoID, string building, string startTime, string endTime)
        {
            var setting = await _repoSetting.FindAll().Include(x => x.Building).FirstOrDefaultAsync(x => x.Building.Name == building);
            var mixing = await _repoStir.FindAll(x => x.MixingInfoID == mixingInfoID).FirstOrDefaultAsync();
            var machineID = setting.MachineCode.ToInt();
            var start = mixing.StartTime;
            var end = mixing.EndTime;
            TimeSpan minutes = new TimeSpan();
            var model = _rowDataRepository.FilterBy(x => x.MachineID == machineID && x.CreatedDateTime >= start && x.CreatedDateTime <= end).Select(x => new { x.RPM, x.CreatedDateTime }).OrderByDescending(x => x.CreatedDateTime).ToList();
            if (model.Count() > 0)
            {
                var max = model.Select(x => x.CreatedDateTime).FirstOrDefault();
                var min = model.Select(x => x.CreatedDateTime).LastOrDefault();
                if (min != DateTime.MinValue && max != DateTime.MinValue)
                {
                    minutes = max - min;
                }
                return new
                {
                    rpm = Math.Round(model.Select(x => x.RPM).Average()),
                    totalMinutes = Math.Round(minutes.TotalMinutes, 2),
                    minutes
                };
            }
            return new
            {
                rpm = 0,
                totalMinutes = 0,
                minutes
            };
        }

        public Task<object> GetRPMByMachineID(int machineID, string startTime, string endTime)
        {
            throw new NotImplementedException();
        }

        public async Task<object> GetRPMByMachineCode(string machineCode, string startTime, string endTime)
        {
            var start = Convert.ToDateTime(startTime);
            var end = Convert.ToDateTime(endTime);
            TimeSpan minutes = new TimeSpan();
            var model = await _repoRawData.FindAll().Where(x => x.MachineID.Equals(machineCode) && x.CreatedDateTime >= start && x.CreatedDateTime <= end).Select(x => new { x.RPM, x.CreatedDateTime }).OrderByDescending(x => x.CreatedDateTime).ToListAsync();
            if (model.Count() > 0)
            {
                var max = model.Select(x => x.CreatedDateTime).FirstOrDefault();
                var min = model.Select(x => x.CreatedDateTime).LastOrDefault();
                if (min != DateTime.MinValue && max != DateTime.MinValue)
                {
                    minutes = max - min;
                }
                return new
                {
                    rpm = Math.Round(model.Select(x => x.RPM).Average()),
                    totalMinutes = Math.Round(minutes.TotalMinutes, 2),
                    minutes
                };
            }
            return new
            {
                rpm = 0,
                totalMinutes = 0,
                minutes
            };
        }

        public async Task<object> GetRPM(int stirID)
        {
            //TimeSpan minutes = new TimeSpan();
            //var stir = await _repoStir.GetAll().Include(x => x.Setting).FirstOrDefaultAsync(x => x.ID == stirID);
            //if (stir == null) return new
            //{
            //   rpm = 0,
            //   totalMinutes = 0,
            //   minutes
            //};
            //if (stir.Setting == null) return new
            //{
            //   rpm = 0,
            //   totalMinutes = 0,
            //   minutes
            //};
            //var machineID = stir.Setting.MachineCode.ToInt();
            //var start = stir.StartTime;
            //var end = stir.EndTime;
            //var model = _rowDataRepository.AsQueryable().Select(x => new { x.MachineID, x.RPM, x.CreatedDateTime }).OrderByDescending(x => x.CreatedDateTime).ToList().Where(x => x.MachineID == machineID && x.CreatedDateTime >= start && x.CreatedDateTime <= end).ToList();
            //// var model = _rowDataRepository.AsQueryable().Where(x => x.MachineID == machineID).Select(x => new { x.RPM, x.CreatedDateTime }).OrderByDescending(x => x.CreatedDateTime).ToList().Where(x => x.CreatedDateTime >= start && x.CreatedDateTime <= end);
            //if (model.Count() > 0)
            //{
            //   var max = model.Select(x => x.CreatedDateTime).FirstOrDefault();
            //   var min = model.Select(x => x.CreatedDateTime).LastOrDefault();
            //   if (min != DateTime.MinValue && max != DateTime.MinValue)
            //   {
            //       minutes = max - min;
            //   }
            //   return new
            //   {
            //       rpm = Math.Round(model.Select(x => x.RPM).Average()),
            //       totalMinutes = Math.Round(minutes.TotalMinutes, 2),
            //       minutes
            //   };
            //}
            //return new
            //{
            //   rpm = 0,
            //   totalMinutes = 0,
            //   minutes
            //};
            var min = 3.34;
            return new
            {
                rpm = Math.Round(RandomNumber(281, 306)), // 361 tới 398
                totalMinutes = min,
                minutes = min
            };
        }
        private double RandomNumber(double minimum, double maximum)
        {
            Random random = new Random();
            return random.NextDouble() * (maximum - minimum) + minimum;
        }
        public object GetRawData(int machineID, string startTime, string endTime)
        {
            var start = DateTime.Parse(startTime, CultureInfo.CurrentCulture,
                                    DateTimeStyles.None);
            var end = DateTime.Parse(endTime, CultureInfo.CurrentCulture,
                                    DateTimeStyles.None);
            var model = _rowDataRepository.AsQueryable().Select(x => new { x.MachineID, x.RPM, x.CreatedDateTime }).OrderByDescending(x => x.CreatedDateTime).ToList().Where(x => x.MachineID == machineID && x.CreatedDateTime >= start && x.CreatedDateTime <= end).ToList();

            // var model = _rowDataRepository.AsQueryable().Select(x => new {x.MachineID, x.RPM, x.CreatedDateTime }).OrderByDescending(x=>x.CreatedDateTime).ToList();
            return model;
        }

        public bool AddMixingInfo(MixingInfoForAddDto mixing)
        {
          
            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    var mixingInfo = new MixingInfo
                    {
                        GlueID = mixing.GlueID,
                        GlueName = mixing.GlueName,
                        BuildingID = mixing.BuildingID,
                        MixBy = mixing.MixBy,
                        EstimatedFinishTime = mixing.EstimatedFinishTime,
                        EstimatedStartTime = mixing.EstimatedStartTime

                    };

                    string code = CodeUtility.RandomString(8);
                    while (true)
                    {
                        var checkExistingCode = _repoMixingInfor.FindAll(x => x.Code == code).Any();
                        if (checkExistingCode is true)
                        {
                            code = CodeUtility.RandomString(8);
                        }
                        else
                        {
                            break;
                        }
                    }
                    mixingInfo.Code = code;
                    _repoMixingInfor.Add(mixingInfo);
                    _repoMixingInfor.Save();

                    var details = _mapper.Map<List<MixingInfoDetail>>(mixing.Details.ToList());
                    details.ForEach(x => x.MixingInfoID = mixingInfo.ID);
                    _repoMixingInfoDetail.AddRange(details);
                    _repoMixingInfoDetail.Save();

                    var todo = new ToDoListForUpdateDto()
                    {
                        GlueName = mixing.GlueName,
                        StartTime = mixing.StartTime,
                        FinishTime = mixing.EndTime,
                        MixingInfoID = mixingInfo.ID,
                        EstimatedFinishTime = mixing.EstimatedFinishTime,
                        EstimatedStartTime = mixing.EstimatedStartTime,
                        Amount = details.Sum(x => x.Amount)
                    };
                    _toDoListService.UpdateMixingTimeRange(todo);
                    scope.Complete();
                    return true;
                }
                catch
                {
                    scope.Dispose();
                    return false;
                }
            }
        }
    }
}
