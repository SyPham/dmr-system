using DMR_API._Services.Interface;
using DMR_API.Data;
using DMR_API.DTO;
using DMR_API.Helpers;
using DMR_API.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TodolistScheduleService
{
    public class Todolist : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly DataContext _context;
        private HubConnection _connection;
        private bool _flag = true;
        private List<TodolistDto> _model = new List<TodolistDto>();

        public Todolist(ILogger<Worker> logger, DataContext context)
        {
            _connection = new HubConnectionBuilder()
              .WithUrl("http://10.4.4.224:1002/ec-hub")
              .Build();
            Console.WriteLine($"Hub State: {_connection.State}");
            _logger = logger;
            _context = context;
        }
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogError($"Todolist Service StopAsync at: { DateTimeOffset.Now}");
            _context.DisposeAsync();
            return _connection.DisposeAsync();
        }
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Loop is here to wait until the server is running
            while (true)
            {
                _logger.LogInformation($"Hub: {_connection.State}");

                try
                {
                    await _connection.StartAsync(stoppingToken);
                    await CheckTodolist();
                    break;
                }
                catch
                {
                    await Task.Delay(1000);
                }
            }

            _logger.LogInformation($"Hub State: {_connection.State}");
            _connection.On<int>("ReceiveTodolist", (building) =>
           {
               _logger.LogInformation($"ReceiveTodolist building: {building}");
           });
            _connection.On("ReceiveCreatePlan", async () =>
            {
                _logger.LogInformation($"ReceiveCreatePlan");
                _flag = true;
                await CheckTodolist();
            });
            _connection.Closed += async (error) =>
            {
                _flag = false;
                _logger.LogError(error.Message);
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await _connection.StartAsync();
            };
            _connection.Reconnecting += (error) =>
           {
               _logger.LogError(error.Message);
               _logger.LogInformation($"Singnalr Reconnecting: { DateTimeOffset.Now} ---- Flag: {_flag}");

               return Task.CompletedTask;
           };
            _connection.Reconnected += async (connectionId) =>
           {
               _flag = true;
               _logger.LogInformation($"{connectionId} reconnected at: { DateTimeOffset.Now} ---- Flag: {_flag}");
               await CheckTodolist();
           };
            while (true)
            {
                if (_connection.State == HubConnectionState.Disconnected)
                {
                    try
                    {
                        await _connection.StartAsync(stoppingToken);
                        if (_connection.State == HubConnectionState.Connected)
                        {
                            _logger.LogInformation($"Hub: {_connection.State}");
                            _flag = true;
                            await CheckTodolist();
                        }
                    }
                    catch
                    {
                        await Task.Delay(3000);
                    }
                }

            }
        }
        async Task CheckTodolist()
        {

            var currentTime = DateTime.Now.Subtract(new TimeSpan(00, 00, 30));
            _model = await CheckTodolistAllBuilding();
            _logger.LogInformation($"Get all todolist: { DateTimeOffset.Now} ---- Flag: {_flag} -- Start loop -- Count: {_model.Count}");
            var model = _model.GroupBy(x => new { x.BuildingID, x.EstimatedTime }).Where(x => x.Key.EstimatedTime.Subtract(new TimeSpan(00, 00, 30)) == currentTime).ToList();
            _logger.LogInformation($"So luong todolist can load: { model.Count()}");

            while (_flag)
            {
                if (_connection.State == HubConnectionState.Disconnected)
                {
                    _flag = false;
                    _logger.LogInformation($"Hub Disconnected at: { DateTimeOffset.Now} ---- Flag: {_flag} -- Stop loop");
                }
                //_logger.LogInformation($"Todolist loop at: { DateTimeOffset.Now} ---- Flag: {_flag}");

                foreach (var item in model)
                {
                    try
                    {
                        await _connection.InvokeAsync("Todolist", item.Key.BuildingID);
                        _logger.LogInformation("Todolist InvokeAsync at: {time}", DateTimeOffset.Now);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Todolist error at: {time} {err}", DateTimeOffset.Now, ex.Message);
                    }
                }
                await Task.Delay(30000);
            }
        }
        async Task<List<TodolistDto>> CheckTodolistAllBuilding()
        {
            var currentTime = DateTime.Now;
            var currentDate = DateTime.Now.Date;
            var buildingModel = await _context.Buildings
                .Include(x => x.LunchTime).ToListAsync();

            var buildings = buildingModel.Where(x => x.Level == 2 && x.LunchTime != null).ToList();
            var lunchTimes = buildings.Select(x => x.LunchTime).ToList();
            var buildingIDList = buildings.Select(x => x.ID).ToList();
            var lines = new List<int>();
            lines = buildingModel.Where(x => x.ParentID != null && buildingIDList.Contains(x.ParentID.Value)).Select(x => x.ID).ToList();

            var model = from b in _context.BPFCEstablishes
                        join p in _context.Plans
                                            .Include(x => x.Building)
                                            .Where(x => x.DueDate.Date == currentDate && lines.Contains(x.BuildingID))
                                            .Select(x => new
                                            {
                                                x.HourlyOutput,
                                                x.WorkingHour,
                                                x.ID,
                                                x.BPFCEstablishID,
                                                Building = x.Building,

                                            })
                        on b.ID equals p.BPFCEstablishID
                        join g in _context.Glues.Where(x => x.isShow)
                                           .Include(x => x.MixingInfos)
                                           .Include(x => x.GlueIngredients)
                                               .ThenInclude(x => x.Ingredient)
                                               .ThenInclude(x => x.Supplier)
                                            .Select(x => new
                                            {
                                                x.ID,
                                                x.BPFCEstablishID,
                                                x.Name,
                                                x.Consumption,
                                                MixingInfos = x.MixingInfos.Select(x => new MixingInfoTodolistDto { ID = x.ID, Status = x.Status, EstimatedStartTime = x.StartTime, EstimatedFinishTime = x.EndTime }).ToList(),
                                                Ingredients = x.GlueIngredients.Select(x => new { x.Position, x.Ingredient.PrepareTime, Supplier = x.Ingredient.Supplier.Name, x.Ingredient.ReplacementFrequency })
                                            })
                       on b.ID equals g.BPFCEstablishID
                        select new
                        {
                            GlueID = g.ID,
                            GlueName = g.Name,
                            g.Ingredients,
                            g.MixingInfos,
                            Plans = p,
                            p.HourlyOutput,
                            p.WorkingHour,
                            g.Consumption,
                            Line = p.Building.Name,
                            LineID = p.Building.ID,
                            BuildingID = p.Building.ParentID,
                        };


            var mixingInfoModel = await _context.MixingInfos
                .Include(x => x.MixingInfoDetails)
           .Where(x => x.CreatedTime.Date == currentDate)
           .ToListAsync();
            var dispatchModel = await _context.Dispatches
          .Where(x => x.CreatedTime.Date == currentDate).ToListAsync();

            var test = await model.ToListAsync();
            var groupByGlueName = test.GroupBy(x => x.GlueName).Distinct().ToList();
            var resAll = new List<TodolistDto>();
            foreach (var building in buildings)
            {
                var linesList = buildingModel.Where(x => x.ParentID == building.ID).Select(x => x.ID).ToList();

                var startLunchTimeBuilding = building.LunchTime.StartTime;
                var endLunchTimeBuilding = building.LunchTime.EndTime;

                var startLunchTime = currentDate.Add(new TimeSpan(startLunchTimeBuilding.Hour, startLunchTimeBuilding.Minute, 0));
                var endLunchTime = currentDate.Add(new TimeSpan(endLunchTimeBuilding.Hour, endLunchTimeBuilding.Minute, 0));

                var plans = test.Where(x => linesList.Contains(x.LineID)).ToList();
                var groupBy = test.GroupBy(x => x.GlueName).Distinct().ToList();
                var res = new List<TodolistDto>();
                foreach (var glue in groupBy)
                {
                    var itemTodolist = new TodolistDto();
                    itemTodolist.Lines = glue.Select(x => x.Line).ToList();
                    itemTodolist.Glue = glue.Key;
                    double standardConsumption = 0;

                    foreach (var item in glue)
                    {
                        itemTodolist.GlueID = item.GlueID;
                        itemTodolist.BuildingID = item.BuildingID ?? 0;
                        var supplier = string.Empty;
                        var checmicalA = item.Ingredients.ToList().FirstOrDefault(x => x.Position == "A");
                        double prepareTime = 0;
                        if (checmicalA != null)
                        {
                            supplier = checmicalA.Supplier;
                            prepareTime = checmicalA.PrepareTime;
                        }
                        itemTodolist.Supplier = supplier;
                        itemTodolist.MixingInfoTodolistDtos = item.MixingInfos;
                        itemTodolist.DeliveredActual = "-";
                        itemTodolist.Status = false;
                        var estimatedTime = currentDate.Add(new TimeSpan(7, 30, 00)) - TimeSpan.FromMinutes(prepareTime);
                        itemTodolist.EstimatedTime = estimatedTime;
                        var estimatedTimes = new List<DateTime>();
                        estimatedTimes.Add(estimatedTime);
                        double cycle = 8 / checmicalA.ReplacementFrequency;
                        for (int i = 1; i <= cycle; i++)
                        {
                            var estimatedTimeTemp = estimatedTimes.Last().AddHours(checmicalA.ReplacementFrequency);
                            if (estimatedTimeTemp >= startLunchTime && estimatedTimeTemp <= endLunchTime)
                            {
                                estimatedTimes.Add(endLunchTime);
                            }
                            else
                            {
                                estimatedTimes.Add(estimatedTimeTemp);
                            }
                        }
                        itemTodolist.EstimatedTimes = estimatedTimes;

                        var kgPair = item.Consumption.ToDouble() / 1000;
                        standardConsumption += kgPair * (double)item.HourlyOutput * checmicalA.ReplacementFrequency;

                    }
                    itemTodolist.StandardConsumption = Math.Round(standardConsumption, 2);

                    res.Add(itemTodolist);
                    standardConsumption = 0;
                }
                var res2 = new List<TodolistDto>();
                foreach (var item in res)
                {
                    foreach (var estimatedTime in item.EstimatedTimes)
                    {
                        var mixing = mixingInfoModel.Where(x => x.EstimatedTime == estimatedTime && x.GlueName == item.Glue).FirstOrDefault();
                        var deliverAndActual = string.Empty;
                        if (mixing == null) deliverAndActual = "0kg/0kg";
                        else
                        {
                            var buildingGlue = dispatchModel.Where(x => x.MixingInfoID == mixing.ID).Select(x => x.Amount).ToList();
                            var deliver = buildingGlue.Sum();
                            deliverAndActual = $"{Math.Round(deliver / 1000, 2)}kg/{Math.Round(CalculateGlueTotal(mixing), 2)}";
                        }
                        var itemTodolist = new TodolistDto();
                        itemTodolist.Supplier = item.Supplier;
                        itemTodolist.GlueID = item.GlueID;
                        itemTodolist.BuildingID = item.BuildingID;
                        itemTodolist.ID = mixing == null ? 0 : mixing.ID;
                        itemTodolist.EstimatedStartTime = mixing == null ? DateTime.MinValue : mixing.StartTime;
                        itemTodolist.EstimatedFinishTime = mixing == null ? DateTime.MinValue : mixing.EndTime;
                        itemTodolist.MixingInfoTodolistDtos = item.MixingInfoTodolistDtos;
                        itemTodolist.Lines = item.Lines;
                        itemTodolist.Glue = item.Glue;
                        itemTodolist.StandardConsumption = item.StandardConsumption;
                        itemTodolist.DeliveredActual = deliverAndActual;
                        itemTodolist.Status = mixing == null ? false : mixing.Status;
                        itemTodolist.EstimatedTime = estimatedTime;
                        res2.Add(itemTodolist);
                    }
                }
                res2 = res2.OrderBy(x => x.Glue).Where(x => x.EstimatedTime >= currentTime && x.Status == false).ToList();
                resAll.AddRange(res2);
            }

            return resAll;
        }
        double CalculateGlueTotal(MixingInfo mixingInfo)
        {
            return mixingInfo.MixingInfoDetails.Sum(x => x.Amount);
        }
    }
}
