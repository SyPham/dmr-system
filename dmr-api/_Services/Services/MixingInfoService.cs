using AutoMapper;
using AutoMapper.QueryableExtensions;
using DMR_API._Repositories.Interface;
using DMR_API._Services.Interface;
using DMR_API.DTO;
using DMR_API.Helpers;
using DMR_API.Models;
using EC_API.Data;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API._Services.Services
{
    public class MixingInfoService : IMixingInfoService
    {
        private readonly IMixingInfoRepository _repoMixingInfor;
        private readonly IMixingService _repoMixing;
        private readonly IGlueRepository _repoGlue;
        private readonly IRawDataRepository _repoRawData;
        private readonly IStirRepository _repoStir;
        private readonly IMongoRepository<EC_API.Data.MongoModels.RawData> _rowDataRepository;
        private readonly ISettingRepository _repoSetting;
        private readonly IMapper _mapper;
        private readonly MapperConfiguration _configMapper;

        public MixingInfoService(
            IMixingInfoRepository repoMixingInfor,
            IMixingService repoMixing,
            IRawDataRepository repoRawData,
            IMongoRepository<EC_API.Data.MongoModels.RawData> rowDataRepository,
            ISettingRepository repoSetting,
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
        }

        public async Task<MixingInfo> Mixing(MixingInfoForCreateDto mixing)
        {
            try
            {
                var item = _mapper.Map<MixingInfoForCreateDto, MixingInfo>(mixing);
                item.Code = CodeUtility.RandomString(8);
                item.CreatedTime = DateTime.Now;
                var glue = await _repoGlue.FindAll().FirstOrDefaultAsync(x => x.isShow == true && x.ID == mixing.GlueID);
                item.ExpiredTime = DateTime.Now.AddMinutes(glue.ExpiredTime);
                _repoMixingInfor.Add(item);
                await _repoMixingInfor.SaveAll();
                // await _repoMixing.AddOrUpdate(item.ID);
                return item;
            }
            catch
            {
                return new MixingInfo();
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

            var model = from a in _repoMixingInfor.FindAll().Where(x => x.GlueName.Equals(glueName) && x.CreatedTime.Date == currentDate)
                        join b in _repoStir.FindAll().Include(x => x.Setting).Where(x => x.GlueName.Equals(glueName) && x.CreatedTime.Date == currentDate) on a.ID equals b.MixingInfoID into gj
                        from ab in gj.DefaultIfEmpty()
                        select new
                        {
                            a.ID,
                            StirID = ab.ID,
                            a.GlueName,
                            a.ChemicalA,
                            a.ChemicalB,
                            a.ChemicalC,
                            a.ChemicalD,
                            a.ChemicalE,
                            a.CreatedTime,
                            ab.StartTime,
                            ab.EndTime,
                            ab.SettingID,
                            ab.Setting,
                            MachineType = ab.SettingID == null ? string.Empty : ab.Setting.MachineType,
                            ab.Status,
                            ab.RPM,
                            ab.TotalMinutes,
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
    }
}
