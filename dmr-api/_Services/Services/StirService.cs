using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DMR_API.Helpers;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using DMR_API._Repositories.Interface;
using DMR_API._Services.Interface;
using DMR_API.DTO;
using DMR_API.Models;
using Microsoft.EntityFrameworkCore;
using DMR_API.Data;

namespace DMR_API._Services.Services
{
    public class StirService : IStirService
    {
        private readonly IMapper _mapper;
        private readonly IScaleMachineService _scaleMachineService;
        private readonly ISettingService _settingService;
        private readonly IToDoListRepository _repoTodolist;
        private readonly IStirRepository _repoStir;
        private readonly IMongoRepository<Data.MongoModels.RawData> _repoRawData;
        private readonly MapperConfiguration _configMapper;

        public StirService(IMapper mapper,
            IScaleMachineService scaleMachineService,
            ISettingService settingService,
            IToDoListRepository repoTodolist,
            IStirRepository repoStir,
            IMongoRepository<DMR_API.Data.MongoModels.RawData> repoRawData,
            MapperConfiguration configMapper)
        {
            _mapper = mapper;
            _scaleMachineService = scaleMachineService;
            _settingService = settingService;
            _repoTodolist = repoTodolist;
            _repoStir = repoStir;
            _repoRawData = repoRawData;
            _configMapper = configMapper;
        }

        public async Task<bool> Add(StirDTO model)
        {
            var item = _mapper.Map<Stir>(model);
            item.StartScanTime = DateTime.Now;
            var stirModel = await _repoStir.FindAll(x => x.MixingInfoID == model.MixingInfoID).CountAsync();

            if (stirModel == 0)
            {
                item.StartStiringTime = DateTime.Now;
            }
            _repoStir.Add(item);
            return await _repoStir.SaveAll();
        }

        public async Task<bool> Delete(object id)
        {
            var item = _repoStir.FindById(id);
            _repoStir.Remove(item);
            return await _repoStir.SaveAll();
        }

        public async Task<List<StirDTO>> GetAllAsync()
        {
            return await _repoStir.FindAll().ProjectTo<StirDTO>(_configMapper).OrderByDescending(x => x.ID).ToListAsync();
        }

        public StirDTO GetById(object id)
        {
            return _mapper.Map<Stir, StirDTO>(_repoStir.FindById(id));
        }

        public async Task<List<StirDTO>> GetStirByMixingInfoID(int MixingInfoID)
        {
            return await _repoStir.FindAll(x => x.MixingInfoID == MixingInfoID)
                .Include(x => x.MixingInfo)
                .ThenInclude(x => x.Glue)
                .ThenInclude(x => x.GlueIngredients)
                .ThenInclude(x => x.Ingredient)
                .ThenInclude(x => x.GlueType)
                .ProjectTo<StirDTO>(_configMapper).OrderBy(x => x.CreatedTime).ToListAsync();

        }

        public async Task<PagedList<StirDTO>> GetWithPaginations(PaginationParams param)
        {
            var lists = _repoStir.FindAll().ProjectTo<StirDTO>(_configMapper).OrderByDescending(x => x.ID);
            return await PagedList<StirDTO>.CreateAsync(lists, param.PageNumber, param.PageSize);
        }

        public async Task<Setting> ScanMachine(int buildingID, string scanValue)
        {
            var setting = await _settingService.CheckMachine(buildingID, scanValue);
            if (setting is null)
            {
                return new Setting();
            }
            return setting;
        }

        public async Task<PagedList<StirDTO>> Search(PaginationParams param, object text)
        {
            var lists = _repoStir.FindAll().ProjectTo<StirDTO>(_configMapper)
           .Where(x => x.GlueName.Contains(text.ToString()))
           .OrderByDescending(x => x.ID);
            return await PagedList<StirDTO>.CreateAsync(lists, param.PageNumber, param.PageSize);
        }

        public async Task<bool> Update(StirDTO model)
        {
            var currentTime = DateTime.Now;
            var item = await _repoStir.FindAll(x => x.ID == model.ID).Include(x => x.Setting).FirstOrDefaultAsync();

            var machineID = item.Setting.MachineCode.ToInt();
            var end = item.StartScanTime.AddMinutes(model.GlueType.Minutes);
            var start = item.StartScanTime;

            var rawDataModel = _repoRawData
                .AsQueryable()
                .Where(x => x.MachineID == machineID && x.CreatedDateTime >= start && x.CreatedDateTime <= end)
                .Select(x => new { x.RPM, x.CreatedDateTime, x.Sequence })
                .OrderByDescending(x => x.CreatedDateTime).ToArray();

            var sequence = rawDataModel[rawDataModel.Length / 2].Sequence;
            var rawData = _repoRawData
               .AsQueryable()
               .Where(x => x.MachineID == machineID && sequence == x.Sequence)
               .Select(x => new { x.RPM, x.CreatedDateTime, x.Sequence }).OrderByDescending(x => x.CreatedDateTime).ToList();
            int temp = 0;
            int standardDuration = item.StandardDuration == 0 ? (int)model.GlueType.Minutes * 60 : item.StandardDuration;
            if (rawData.Count == 0) return false;
            foreach (var raw in rawData)
            {
                if (raw.RPM >= model.GlueType.RPM)
                {
                    temp++;
                }
            }
            // Neu khuay du thoi gian va du toc do thi pass
            if (standardDuration <= temp)
            {
                item.StandardDuration = standardDuration;
                item.ActualDuration = temp;
                item.StartTime = rawData.FirstOrDefault().CreatedDateTime;
                item.EndTime = rawData.LastOrDefault().CreatedDateTime;
                item.FinishStiringTime = currentTime;
                item.Status = true;
                _repoStir.Update(item);
                var stirList = await _repoStir.FindAll(x => x.MixingInfoID == model.MixingInfoID).OrderBy(x => x.CreatedTime).ToListAsync();

                var todolist = await _repoTodolist.FindAll(x => x.MixingInfoID == model.MixingInfoID).ToListAsync();
                todolist.ForEach(todo =>
                {
                    todo.StartStirTime = stirList.FirstOrDefault().StartStiringTime;
                    todo.FinishStirTime = item.FinishStiringTime;
                });
                return await _repoStir.SaveAll();
            }
            else // nguoc lai thi khuay them
            {
                item.StandardDuration = standardDuration;
                item.ActualDuration = temp;
                item.EndTime = rawData.FirstOrDefault().CreatedDateTime;
                item.StartTime = rawData.LastOrDefault().CreatedDateTime;
                item.Status = false;
                _repoStir.Update(item);
                await _repoStir.SaveAll();
                var stir = new Stir()
                {
                    MixingInfoID = model.MixingInfoID,
                    StandardDuration = standardDuration - temp,
                    Status = false,
                    CreatedTime = DateTime.Now,
                    GlueName = model.GlueName,
                    SettingID = model.SettingID,
                    MachineID = item.MachineID
                };
                _repoStir.Add(stir);
                return await _repoStir.SaveAll();
            }
        }

        public async Task<bool> UpdateStartScanTime(int mixingInfoID)
        {
            var item = await _repoStir.FindAll(x => x.MixingInfoID == mixingInfoID && x.StartScanTime == DateTime.MinValue).FirstOrDefaultAsync();
            item.StartScanTime = DateTime.Now;
            _repoStir.Update(item);
            return await _repoStir.SaveAll();
        }
    }
}