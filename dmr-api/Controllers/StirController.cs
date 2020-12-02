using DMR_API._Services.Interface;
using DMR_API.DTO;
using DMR_API.Helpers;
using DMR_API.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class StirController : ControllerBase
    {
        private readonly IMixingInfoService _mixingInfoService;
        public StirController(IMixingInfoService mixingInfoService)
        {
            _mixingInfoService = mixingInfoService;
        }
        [HttpGet("{glueName}")]
        public async Task<IActionResult> GetStirInfo(string glueName)
        {
            return Ok(await _mixingInfoService.Stir(glueName));
        }
        [HttpGet("{stirID}")]
        public async Task<IActionResult> GetRPM(int stirID)
        {
            return Ok(await _mixingInfoService.GetRPM(stirID));
        }
        [HttpGet("{mixingInfoID}/{building}/{start}/{end}")]
        public async Task<IActionResult> GetRPM(int mixingInfoID, string building, string start, string end)
        {
            return Ok(await _mixingInfoService.GetRPM(mixingInfoID, building, start, end));
        }
        [HttpGet("{mixingInfoID}/{start}/{end}")]
        public IActionResult GetRawData(int mixingInfoID, string start, string end)
        {
            return Ok( _mixingInfoService.GetRawData(mixingInfoID, start, end));
        }
        [HttpGet("{machineCode}/{start}/{end}")]
        public async Task<IActionResult> GetRPMByMachineCode(string machineCode, string start, string end)
        {
            return Ok(await _mixingInfoService.GetRPMByMachineCode(machineCode, start, end));
        }
    }
}
