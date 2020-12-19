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
using DMR_API._Repositories;
using dmr_api.Models;
using System.Transactions;

namespace DMR_API._Services.Services
{
    public class DispatchService : IDispatchService
    {

        private readonly IDispatchRepository _repoDispatch;
        private readonly IToDoListService _toDoListService;
        private readonly IMixingInfoRepository _repoMixingInfo;
        private readonly IMapper _mapper;
        private readonly MapperConfiguration _configMapper;
        public DispatchService(
            IDispatchRepository repoDispatch,
            IToDoListService toDoListService,
            IMixingInfoRepository repoMixingInfo,
            IMapper mapper,
            MapperConfiguration configMapper)
        {
            _configMapper = configMapper;
            _mapper = mapper;
            _repoDispatch = repoDispatch;
            _toDoListService = toDoListService;
            _repoMixingInfo = repoMixingInfo;
        }

        public async Task<bool> Add(Dispatch model)
        {
            _repoDispatch.Add(model);
            return await _repoDispatch.SaveAll();
        }

        public bool AddDispatching(Dispatch model)
        {
            try
            {
                var mixing = _repoMixingInfo.FindById(model.MixingInfoID);
                if (mixing == null) return false;
                using (TransactionScope scope = new TransactionScope())
                {
                    _repoDispatch.Add(model);
                    _repoDispatch.Save();

                    var item = new ToDoListForUpdateDto()
                    {
                        GlueName = mixing.GlueName,
                        StartTime = model.StartDispatchingTime,
                        FinishTime = model.FinishDispatchingTime,
                        EstimatedFinishTime = mixing.EstimatedFinishTime,
                        EstimatedStartTime = mixing.EstimatedStartTime,
                    };
                    _toDoListService.UpdateStiringTimeRange(item);
                    scope.Complete();
                    return true;
                }

            }
            catch (Exception)
            {
                return false;
                throw;
            }

        }
        public async Task<bool> Delete(object id)
        {
            var dispatch = _repoDispatch.FindById(id);
            _repoDispatch.Remove(dispatch);
            return await _repoDispatch.SaveAll();
        }

        public async Task<bool> Update(Dispatch model)
        {
            var dispatch = _mapper.Map<Dispatch>(model);
            _repoDispatch.Update(dispatch);
            return await _repoDispatch.SaveAll();
        }
        public async Task<List<Dispatch>> GetAllAsync()
        {
            return await _repoDispatch.FindAll().OrderBy(x => x.ID).ToListAsync();
        }


        public Task<PagedList<Dispatch>> GetWithPaginations(PaginationParams param)
        {
            throw new NotImplementedException();
        }

        public Task<PagedList<Dispatch>> Search(PaginationParams param, object text)
        {
            throw new NotImplementedException();
        }

        public Dispatch GetById(object id)
        {
            throw new NotImplementedException();
        }

        public bool AddDispatchingRange(List<Dispatch> dispatch)
        {

            if (dispatch.Count == 0) return false;
            var mixing = _repoMixingInfo.FindById(dispatch.FirstOrDefault().MixingInfoID);
            if (mixing == null) return false;

            var flags = new List<bool>();
            foreach (var model in dispatch)
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    try
                    {
                        if (model.ID == 0)
                        {
                            _repoDispatch.Add(model);
                            _repoDispatch.Save();
                        } else
                        {
                            _repoDispatch.Update(model);
                            _repoDispatch.Save();
                        }
                        var item = new ToDoListForUpdateDto()
                        {
                            GlueName = mixing.GlueName,
                            StartTime = model.StartDispatchingTime,
                            FinishTime = model.FinishDispatchingTime,
                            EstimatedFinishTime = mixing.EstimatedFinishTime,
                            EstimatedStartTime = mixing.EstimatedStartTime,
                            MixingInfoID = model.MixingInfoID,
                            Amount = model.Amount,
                            LineID = model.LineID
                        };
                        _toDoListService.UpdateDispatchTimeRange(item);
                        scope.Complete();
                        flags.Add(true);
                    }
                    catch
                    {
                        scope.Dispose();
                        flags.Add(false);
                    }
                }

            }

            return flags.All(x => x == true);

        }
    }
}