using System;
using System.Security.Claims;
using System.Threading.Tasks;
using DMR_API.Helpers;
using DMR_API._Services.Interface;
using DMR_API.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Org.BouncyCastle.Crypto.Tls;
using DMR_API.SignalrHub;
using Microsoft.AspNetCore.SignalR;

namespace DMR_API.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class ToDoListController : ControllerBase
    {
        private readonly IToDoListService _toDoList;
        private readonly IHubContext<ECHub> _hubContext;
        public ToDoListController(IToDoListService toDoList, IHubContext<ECHub> hubContext)
        {
            _toDoList = toDoList;
            _hubContext = hubContext;
        }

        [HttpPut("{building}")]
        public IActionResult UpdateFinishStirTimeByMixingInfoID(int building)
        {
            if (_toDoList.UpdateFinishStirTimeByMixingInfoID(building))
            {
                return NoContent();
            }
            else
            {
                return BadRequest();
            }
        }
        [HttpPut("{building}")]
        public IActionResult UpdateStartStirTimeByMixingInfoID(int building)
        {
            if (_toDoList.UpdateStartStirTimeByMixingInfoID(building))
            {
                return NoContent();
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpGet("{building}")]
        public async Task<IActionResult> ToDo(int building)
        {
            var batchs = await _toDoList.ToDo(building);
            return Ok(batchs);
        }
        [HttpGet("{building}")]
        public async Task<IActionResult> Delay(int building)
        {
            var batchs = await _toDoList.Delay(building);
            return Ok(batchs);
        }
        [HttpGet("{building}")]
        public async Task<IActionResult> Done(int building)
        {
            var batchs = await _toDoList.Done(building);
            return Ok(batchs);
        }
        [HttpPost]
        public async Task<IActionResult> Dispatch(DispatchParams todolistDto)
        {
            var batchs = await _toDoList.Dispatch(todolistDto);
            return Ok(batchs);
        }
        [HttpGet("{mixingInfoID}")]
        public IActionResult PrintGlue(int mixingInfoID)
        {
            var batchs = _toDoList.PrintGlue(mixingInfoID);
            return Ok(batchs);
        }
        [HttpGet("{mixingInfoID}")]
        public IActionResult FindPrintGlue(int mixingInfoID)
        {
            var batchs = _toDoList.FindPrintGlue(mixingInfoID);
            return Ok(batchs);
        }
        [HttpPost]
        public IActionResult Cancel([FromBody] ToDoListForCancelDto todolistID)
        {
            var batchs = _toDoList.Cancel(todolistID);
            return Ok(batchs);
        }

        [HttpPost]
        public async Task<IActionResult> GenerateToDoList(List<int> plans)
        {
            var status = await _toDoList.GenerateToDoList(plans);
            return Ok(status);

        }
        [HttpGet("{buildingID}")]
        public async Task<IActionResult> ExportExcel(int buildingID)
        {
            var bin = await _toDoList.ExportExcelToDoListByBuilding(buildingID);
            return File(bin, "application/octet-stream", "doneListReport.xlsx");
        }
        [HttpPost]
        public IActionResult GetMixingDetail(MixingDetailParams obj)
        {
            var bin = _toDoList.GetMixingDetail(obj);
            return Ok(bin);
        }
        [HttpPost]
        public IActionResult CancelRange(List<ToDoListForCancelDto> todolistIDList)
        {
            var batchs = _toDoList.CancelRange(todolistIDList);
            return Ok(batchs);
        }


    }
}