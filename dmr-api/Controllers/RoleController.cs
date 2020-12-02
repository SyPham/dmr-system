using DMR_API._Services.Interface;
using DMR_API.DTO;
using DMR_API.Helpers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _RoleService;
        public RoleController(IRoleService RoleService)
        {
            _RoleService = RoleService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var role = await _RoleService.GetAllAsync();
            return Ok(role);
        }
     
    }
}
