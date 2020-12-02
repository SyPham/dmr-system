using dmr_api.Models;
using DMR_API._Services.Interface;
using DMR_API.DTO;
using DMR_API.Helpers;
using EC_API._Services.Interface;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class DispatchController : ControllerBase
    {
        private readonly IDispatchService _dispatchService;
        public DispatchController(IDispatchService dispatchService)
        {
            _dispatchService = dispatchService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var role = await _dispatchService.GetAllAsync();
            return Ok(role);
        }
        [HttpPost]
        public async Task<IActionResult> Add(Dispatch dispatch)
        {
            var res = await _dispatchService.Add(dispatch);
            return Ok(res);
        }
        [HttpPut]
        public async Task<IActionResult> Update(Dispatch dispatch)
        {
            var res = await _dispatchService.Add(dispatch);
            return Ok(res);
        }
        [HttpDelete("{ID}")]
        public async Task<IActionResult> Delete(int ID)
        {
            var res = await _dispatchService.Delete(ID);
            return Ok(res);
        }
    }
}
