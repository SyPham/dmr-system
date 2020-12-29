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
    public class PlanController : ControllerBase
    {
        private readonly IPlanService _planService;
        private readonly IHubContext<ECHub> _hubContext;
        public PlanController(IPlanService planService, IHubContext<ECHub> hubContext)
        {
            _planService = planService;
            _hubContext = hubContext;
        }
        [HttpPost]
        public async Task<IActionResult> GetBPFCByGlue([FromBody] TooltipParams tooltip)
        {
            var plans = await _planService.GetBPFCByGlue(tooltip);
            return Ok(plans);
        }
        [HttpGet("{ingredientName}/{batch}")]
        public async Task<IActionResult> TroubleShootingSearch(string ingredientName, string batch)
        {
            return Ok(await _planService.TroubleShootingSearch(ingredientName, batch));
        }
        [HttpGet]
        public async Task<IActionResult> GetPlans([FromQuery] PaginationParams param)
        {
            var plans = await _planService.GetWithPaginations(param);
            Response.AddPagination(plans.CurrentPage, plans.PageSize, plans.TotalCount, plans.TotalPages);
            return Ok(plans);
        }
        [HttpGet("{buildingID}")]
        public async Task<IActionResult> GetGlueByBuilding(int buildingID)
        {
            var plans = await _planService.GetGlueByBuilding(buildingID);
            return Ok(plans);
        }
        [HttpGet("{buildingID}/{modelName}")]
        public async Task<IActionResult> GetGlueByBuildingModelName(int buildingID, int modelName)
        {
            var plans = await _planService.GetGlueByBuildingModelName(buildingID, modelName);
            return Ok(plans);
        }

        [HttpGet("{IngredientID}")]
        public async Task<IActionResult> GetBatchByIngredientID(int IngredientID)
        {
            var batchs = await _planService.GetBatchByIngredientID(IngredientID);
            return Ok(batchs);
        }
        [HttpGet(Name = "GetPlans")]
        public async Task<IActionResult> GetAll()
        {
            var plans = await _planService.GetAllAsync();
            return Ok(plans);
        }
        [HttpGet("{from}/{to}")]
        public async Task<IActionResult> GetAllPlansByDate(string from, string to)
        {
            var plans = await _planService.GetAllPlansByDate(from, to);
            return Ok(plans);
        }
        //[HttpGet("{modeNameID}")]
        //public async Task<IActionResult> GetPlanByModelNameID(int modeNameID)
        //{
        //    var lists = await _planService.GetPlanByModelNameID(modeNameID);
        //    return Ok(lists);
        //}
        [HttpGet("{text}")]
        public async Task<IActionResult> Search([FromQuery] PaginationParams param, string text)
        {
            var lists = await _planService.Search(param, text);
            Response.AddPagination(lists.CurrentPage, lists.PageSize, lists.TotalCount, lists.TotalPages);
            return Ok(lists);
        }
        [HttpGet("{buildingID}")]
        public async Task<IActionResult> GetLines(int buildingID)
        {
            var lists = await _planService.GetLines(buildingID);
            return Ok(lists);
        }
        
        [HttpPost]
        public async Task<IActionResult> Create(PlanDto create)
        {
            var startTime = create.DueDate.Date.Add(new TimeSpan(create.StartTime.Hour, create.StartTime.Minute, 0)).ToRemoveSecond();
            var endTime = create.DueDate.Date.Add(new TimeSpan(create.EndTime.Hour, create.EndTime.Minute, 0)).ToRemoveSecond();

            DateTime timeNow = DateTime.Now.ToLocalTime().ToRemoveSecond();
            create.StartWorkingTime = startTime;
            create.FinishWorkingTime = endTime;

            var dateNow = DateTime.Now.Date;
            if (startTime >= endTime)
                return BadRequest("The start time must be less than or equal to the end time!<br> Thời gian bắt đầu phải nhỏ hơn hoặc bằng thời gian kết thúc! ");

            if (startTime < timeNow)
                return BadRequest("The start time must be greater than or equal to the current time!<br> Thời gian bắt đầu phải lớn hơn hoặc bằng thời gian hiện tại! ");

            if (create.DueDate.Date < dateNow) 
                return BadRequest("The duedate must be greater than or equal to the current date!<br> Ngày hết hạn phải lớn hơn hoặc bằng ngày hiện tại! ");

            if (await _planService.CheckDuplicate(create.BuildingID, create.BPFCEstablishID, create.DueDate))
                return BadRequest("Plan already exists!<br> Kế hoạch làm việc này đã tồn tại! ");

            if (_planService.GetById(create.ID) != null)
                return BadRequest("Plan ID already exists!<br> Kế hoạch làm việc này đã tồn tại!");

            if (await _planService.CheckExistTimeRange(create.BuildingID, startTime, endTime, create.DueDate))
                return BadRequest("The time range of plans already exists!<br> Khoảng thời gian này đã trùng lắp với 1 khoảng thời gian khác.!");

            var model = await _planService.Add(create);
            if (model)
                await _hubContext.Clients.All.SendAsync("ReceiveCreatePlan");
            return Ok(model);
        }

        [HttpPut]
        public async Task<IActionResult> Update(PlanDto update)
        {
            var startTime = update.DueDate.Date.Add(new TimeSpan(update.StartTime.Hour, update.StartTime.Minute, 0)).ToRemoveSecond();
            var endTime = update.DueDate.Date.Add(new TimeSpan(update.EndTime.Hour, update.EndTime.Minute, 0)).ToRemoveSecond();

            DateTime timeNow = DateTime.Now.ToLocalTime().ToRemoveSecond();
            update.StartWorkingTime = startTime;
            update.FinishWorkingTime = endTime;

            if (startTime >= endTime)
                return BadRequest("The start time must be less than or equal to the end time!<br> Thời gian bắt đầu phải nhỏ hơn hoặc bằng thời gian kết thúc! ");

            var model = await _planService.Update(update);
            if (model)
            {
                await _hubContext.Clients.All.SendAsync("ReceiveCreatePlan");
                return NoContent();
            }
            return BadRequest($"Updating model no {update.ID} failed on save");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var model = await _planService.Delete(id);

            if (model)
            {
                await _hubContext.Clients.All.SendAsync("ReceiveCreatePlan");
                return NoContent();
            }
            throw new Exception("Error deleting the model no");
        }
        [AllowAnonymous]
        [HttpGet("{buildingID}")]
        public async Task<IActionResult> Summary(int buildingID)
        {
            var model = await _planService.Summary(buildingID);
            return Ok(model);
        }

        [HttpPost]
        public async Task<IActionResult> ClonePlan(List<PlanForCloneDto> create)
        {
            var model = await _planService.ClonePlan(create);
            await _hubContext.Clients.All.SendAsync("ReceiveCreatePlan");
            return Ok(model);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteRange(List<int> delete)
        {
            var model = await _planService.DeleteRange(delete);
            await _hubContext.Clients.All.SendAsync("ReceiveCreatePlan");
            return Ok(model);
        }
        [HttpPost]
        public async Task<IActionResult> DispatchGlue(BuildingGlueForCreateDto create)
        {
            return Ok(await _planService.DispatchGlue(create));
        }
        [HttpGet("{building}/{min}/{max}")]
        public async Task<IActionResult> Search(int building, DateTime min, DateTime max)
        {
            var lists = await _planService.GetAllPlanByRange(building, min, max);
            return Ok(lists);
        }

        [HttpGet("{id}/{qty}")]
        public async Task<IActionResult> EditDelivered(int id, string qty)
        {
            var lists = await _planService.EditDelivered(id, qty);
            return Ok(lists);
        }

        [HttpGet("{id}/{qty}")]
        public async Task<IActionResult> EditQuantity(int id, int qty)
        {
            var lists = await _planService.EditQuantity(id, qty);
            return Ok(lists);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDelivered(int id)
        {
            var lists = await _planService.DeleteDelivered(id);
            return Ok(lists);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Finish(int id)
        {
            var lists = await _planService.Finish(id);
            return Ok(lists);
        }
        [HttpGet]
        public async Task<IActionResult> GetAllPlanByDefaultRange()
        {
            var lists = await _planService.GetAllPlanByDefaultRange();
            return Ok(lists);
        }
        [HttpPost]
        public async Task<IActionResult> ConsumptionByLineCase1(ReportParams reportParams)
        {
            var lists = await _planService.ConsumptionByLineCase1(reportParams);
            return Ok(lists);
        }
        [HttpPost]
        public async Task<IActionResult> ConsumptionByLineCase2(ReportParams reportParams)
        {
            var lists = await _planService.ConsumptionByLineCase2(reportParams);
            return Ok(lists);
        }
        [HttpGet("{startDate}/{endDate}")]
        public async Task<IActionResult> Report(DateTime startDate, DateTime endDate)
        {
            var bin = await _planService.Report(startDate, endDate);
            return File(bin, "application/octet-stream", "report.xlsx");
        }
        [HttpPost]
        public async Task<IActionResult> ReportConsumptionCase1(ReportParams reportParams)
        {

            var delta = reportParams.EndDate - reportParams.StartDate;
            var str = Math.Abs(delta.TotalDays);
            if (str > 31)
            {
                var error = $"Chỉ được xuất dữ liệu báo cáo trong 30 ngày!!!<br>The report data can only be exported for 30 days!!!";
                return BadRequest(error);
            }
            else
            {
                var bin = await _planService.ReportConsumptionCase1(reportParams);
                return File(bin, "application/octet-stream", "reportConsumption1.xlsx");
            }
        }
        [HttpPost]
        public async Task<IActionResult> ReportConsumptionCase2(ReportParams reportParams)
        {
            var delta = reportParams.EndDate - reportParams.StartDate;
            var str = Math.Abs(delta.TotalDays);
            if (str > 31)
            {
                var error = $"Chỉ được xuất dữ liệu báo cáo trong 30 ngày!!!<br>The report data can only be exported for 30 days!!!";
                return BadRequest(error);
            }
            else
            {
                var bin = await _planService.ReportConsumptionCase2(reportParams);
                return File(bin, "application/octet-stream", "reportConsumption2.xlsx");
            }
        }
        [HttpPost]
        public async Task<IActionResult> GetReport(GetReportDto getReportDto)
        {
            var delta = getReportDto.EndDate - getReportDto.StartDate;
            var str = Math.Abs(delta.TotalDays);
            if (str > 31)
            {
                var error = $"Chỉ được xuất dữ liệu báo cáo trong 30 ngày!!!<br>The report data can only be exported for 30 days!!!";
                return BadRequest(error);
            }
            else
            {
                var bin = await _planService.Report(getReportDto.StartDate, getReportDto.EndDate);
                return File(bin, "application/octet-stream", "report.xlsx");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Dispatch([FromBody] DispatchParams todolistDto)
        {
            var batchs = await _planService.Dispatch(todolistDto);
            return Ok(batchs);
        }
        [HttpPost]
        public IActionResult DeleteRangePlan(List<int> plans)
        {
            var batchs = _planService.DeleteRangePlan(plans);
            return Ok(batchs);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePlan(int id)
        {
            var glue = await _planService.DeletePlan(id);
            return Ok(glue);
        }
        [HttpGet("{mixingInfoID}")]
        public IActionResult PrintGlue(int mixingInfoID)
        {
            var glue = _planService.PrintGlue(mixingInfoID);
            return Ok(glue);
        }

    }
}