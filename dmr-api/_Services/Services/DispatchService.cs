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
using EC_API._Services.Interface;
using EC_API._Repositories;
using dmr_api.Models;

namespace DMR_API._Services.Services
{
    public class DispatchService : IDispatchService
    {

        private readonly IDispatchRepository _repoDispatch;
        private readonly IMapper _mapper;
        private readonly MapperConfiguration _configMapper;
        public DispatchService(IDispatchRepository repoDispatch, IMapper mapper, MapperConfiguration configMapper)
        {
            _configMapper = configMapper;
            _mapper = mapper;
            _repoDispatch = repoDispatch;

        }

        public async Task<bool> Add(Dispatch model)
        {
            _repoDispatch.Add(model);
            return await _repoDispatch.SaveAll();
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
    }
}