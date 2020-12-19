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
using System.Net.Http;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;

namespace DMR_API._Services.Services
{
    public class UserDetailService : IUserDetailService
    {
        private readonly IUserDetailRepository _repoUserDetail;
        private readonly IBuildingUserRepository _repoBuildingUser;
        private readonly IUserRoleRepository _repoUserRole;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly MapperConfiguration _configMapper;
        public UserDetailService(IUserDetailRepository repoBrand,
            IBuildingUserRepository repoBuildingUser,
            IUserRoleRepository repoUserRole,
            IConfiguration configuration,
             IMapper mapper,
            MapperConfiguration configMapper)
        {
            _configMapper = configMapper;
            _mapper = mapper;
            _configuration = configuration;
            _repoUserDetail = repoBrand;
            _repoBuildingUser = repoBuildingUser;
            _repoUserRole = repoUserRole;
        }

        //Thêm Brand mới vào bảng UserDetail
        public async Task<bool> Add(UserDetailDto model)
        {
            var UserDetail = _mapper.Map<UserDetail>(model);
            _repoUserDetail.Add(UserDetail);
            return await _repoUserDetail.SaveAll();
        }



        //Lấy danh sách Brand và phân trang
        public async Task<PagedList<UserDetailDto>> GetWithPaginations(PaginationParams param)
        {
            var lists = _repoUserDetail.FindAll().ProjectTo<UserDetailDto>(_configMapper).OrderByDescending(x => x.ID);
            return await PagedList<UserDetailDto>.CreateAsync(lists, param.PageNumber, param.PageSize);
        }
        //public async Task<object> GetIngredientOfUserDetail(int UserDetailid)
        //{
        //    return await _repoUserDetail.GetIngredientOfUserDetail(UserDetailid);

        //    throw new System.NotImplementedException();
        //}
        //Tìm kiếm UserDetail
        //public async Task<PagedList<UserDetailDto>> Search(PaginationParams param, object text)
        //{
        //    var lists = _repoUserDetail.FindAll().ProjectTo<UserDetailDto>(_configMapper)
        //    .Where(x => x.Name.Contains(text.ToString()))
        //    .OrderByDescending(x => x.ID);
        //    return await PagedList<UserDetailDto>.CreateAsync(lists, param.PageNumber, param.PageSize);
        //}
        //Xóa Brand
        public async Task<bool> Delete(object id)
        {
            var UserDetail = _repoUserDetail.FindById(id);
            _repoUserDetail.Remove(UserDetail);
            return await _repoUserDetail.SaveAll();
        }

        //Cập nhật Brand
        public async Task<bool> Update(UserDetailDto model)
        {
            var UserDetail = _mapper.Map<UserDetail>(model);
            _repoUserDetail.Update(UserDetail);
            return await _repoUserDetail.SaveAll();
        }

        //Lấy toàn bộ danh sách Brand 
        public async Task<List<UserDetailDto>> GetAllAsync()
        {
            return await _repoUserDetail.FindAll().ProjectTo<UserDetailDto>(_configMapper).OrderByDescending(x => x.ID).ToListAsync();
        }

        //Lấy Brand theo Brand_Id
        public UserDetailDto GetById(object id)
        {
            return _mapper.Map<UserDetail, UserDetailDto>(_repoUserDetail.FindById(id));
        }

        public Task<PagedList<UserDetailDto>> Search(PaginationParams param, object text)
        {
            throw new NotImplementedException();
        }

        public Task<List<ModelNoForMapModelDto>> GetModelNos(int modelNameID)
        {
            throw new NotImplementedException();
        }

        public Task<bool> MapUserDetailDto(UserDetailDto mapModel)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Delete(int userId, int lineID)
        {
            throw new NotImplementedException();
        }

        public async Task<object> GetAllUserInfo()
        {
            var appsettings = _configuration.GetSection("AppSettings").Get<Appsettings>();
            var DMRSystemCode = appsettings.SystemCode;
            using var client = new HttpClient();
            var response = await client.GetAsync($"{appsettings.API_AUTH_URL}Users/GetUserBySystemID/{DMRSystemCode}");
            var data = await response.Content.ReadAsStringAsync();
            var users = JsonConvert.DeserializeObject<List<UserDto>>(data);
            var userRole = await _repoUserRole.FindAll().Include(x => x.Role).ToListAsync();
            var buildingUser = await _repoBuildingUser.FindAll().Include(x => x.Building).ToListAsync();
            var result = new List<UserDto>();
            foreach (var x in users)
            {
                var userRoleItem = userRole.FirstOrDefault(a => a.UserID == x.ID);
                var buildingUserItem = buildingUser.FirstOrDefault(a => a.UserID == x.ID);
                result.Add(new UserDto
                {
                    ID = x.ID,
                    Username = x.Username,
                    Password = x.Password,
                    EmployeeID = x.EmployeeID,
                    Email = x.Email,
                    PasswordSalt = x.PasswordSalt,
                    PasswordHash = x.PasswordHash,
                    IsLock = userRoleItem != null ? userRoleItem.IsLock : false,
                    SystemID = DMRSystemCode,
                    UserRoleID = userRoleItem != null ? userRoleItem.RoleID : 0,
                    BuildingUserID = buildingUserItem != null ? buildingUserItem.BuildingID : 0,
                    Role = userRoleItem != null ? userRoleItem.Role.Name : "#N/A",
                    Building = buildingUserItem != null ? buildingUserItem.Building.Name : "#N/A",
                });
            }

            return result;
        }
    }
}