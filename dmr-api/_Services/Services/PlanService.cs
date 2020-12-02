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
using System.Dynamic;
using Newtonsoft.Json;
using System.Collections.Immutable;
using DMR_API.SignalrHub;
using Microsoft.AspNetCore.SignalR;
using Google.Protobuf.WellKnownTypes;
using OfficeOpenXml;
using System.IO;
using OfficeOpenXml.Style;
using System.Drawing;
using Microsoft.AspNetCore.Http;
using EC_API.DTO;
using EC_API.Enums;
using EC_API._Repositories;

namespace DMR_API._Services.Services
{
    public class PlanService : IPlanService
    {
        private readonly int LINE_LEVEL = 3;
        private readonly IPlanRepository _repoPlan;
        private readonly IPlanDetailRepository _repoPlanDetail;
        private readonly IGlueRepository _repoGlue;
        private readonly IGlueIngredientRepository _repoGlueIngredient;
        private readonly IIngredientRepository _repoIngredient;
        private readonly IIngredientInfoRepository _repoIngredientInfo;

        private readonly IBuildingRepository _repoBuilding;
        private readonly IBPFCEstablishRepository _repoBPFC;
        private readonly IMixingInfoRepository _repoMixingInfo;
        private readonly IBuildingGlueRepository _repoBuildingGlue;
        private readonly IDispatchRepository _repoDispatch;
        private readonly IModelNameRepository _repoModelName;
        private readonly IHubContext<ECHub> _hubContext;
        private readonly IMapper _mapper;
        private readonly MapperConfiguration _configMapper;
        public PlanService(
            IPlanRepository repoPlan,
            IDispatchRepository repoDispatch,
            IPlanDetailRepository repoPlanDetail,
            IGlueRepository repoGlue,
            IGlueIngredientRepository repoGlueIngredient,
            IIngredientRepository repoIngredient,
            IBuildingRepository repoBuilding,
            IBPFCEstablishRepository repoBPFC,
            IIngredientInfoRepository repoIngredientInfo,
            IMixingInfoRepository repoMixingInfo,
            IModelNameRepository repoModelName,
            IBuildingGlueRepository repoBuildingGlue,
            IHubContext<ECHub> hubContext,
            IHttpContextAccessor _httpContextAccessor,
            IMapper mapper,
            MapperConfiguration configMapper)
        {
            _configMapper = configMapper;
            _mapper = mapper;
            _repoGlue = repoGlue;
            _repoGlueIngredient = repoGlueIngredient;
            _repoIngredient = repoIngredient;
            _repoIngredientInfo = repoIngredientInfo;
            _repoPlan = repoPlan;
            _repoPlanDetail = repoPlanDetail;
            _repoBuilding = repoBuilding;
            _repoModelName = repoModelName;
            _hubContext = hubContext;
            _repoBPFC = repoBPFC;
            _repoMixingInfo = repoMixingInfo;
            _repoBuildingGlue = repoBuildingGlue;
            _repoDispatch = repoDispatch;
        }


        public async Task<object> GetBatchByIngredientID(int ingredientID)
        {
            try
            {
                var item = (await _repoIngredientInfo.FindAll().Where(x => x.IngredientID == ingredientID).ToListAsync()).Select(x => new BatchDto
                {
                    ID = x.ID,
                    BatchName = x.Batch
                }).DistinctBy(x => x.BatchName);

                return item;
            }
            catch
            {
                throw;
            }

        }

        public async Task<object> TroubleShootingSearch(string value, string batchValue)
        {
            try
            {
                var ingredientName = value.ToSafetyString();
                var from = DateTime.Now.Date.AddDays(-3).Date;
                var to = DateTime.Now.Date.Date;
                var plans = _repoPlan.FindAll()
                    .Include(x => x.Building)
                    .Include(x => x.BPFCEstablish)
                        .ThenInclude(x => x.Glues)
                        .ThenInclude(x => x.GlueIngredients)
                        .ThenInclude(x => x.Ingredient)
                    .Include(x => x.BPFCEstablish)
                        .ThenInclude(x => x.ModelName)
                        .ThenInclude(x => x.ModelNos)
                        .ThenInclude(x => x.ArticleNos)
                        .ThenInclude(x => x.ArtProcesses)
                        .ThenInclude(x => x.Process)
                    .Where(x => x.DueDate.Date >= from && x.DueDate.Date <= to)
                    .Select(x => new
                    {
                        x.BPFCEstablish.Glues,
                        ModelName = x.BPFCEstablish.ModelName.Name,
                        ModelNo = x.BPFCEstablish.ModelNo.Name,
                        ArticleNo = x.BPFCEstablish.ArticleNo.Name,
                        Process = x.BPFCEstablish.ArtProcess.Process.Name,
                        Line = x.Building.Name,
                        LineID = x.Building.ID,
                        x.DueDate
                    });
                var troubleshootings = new List<TroubleshootingDto>();

                foreach (var plan in plans)
                {
                    // lap nhung bpfc chua ingredient search
                    foreach (var glue in plan.Glues.Where(x => x.isShow == true))
                    {
                        foreach (var item in glue.GlueIngredients.Where(x => x.Ingredient.Name.Trim().Contains(ingredientName)))
                        {
                            var buildingGlue = await _repoBuildingGlue.FindAll().Where(x => x.BuildingID == plan.LineID && x.CreatedDate.Date == plan.DueDate.Date).OrderByDescending(x => x.CreatedDate).FirstOrDefaultAsync();
                            var mixingID = 0;
                            if (buildingGlue != null)
                            {
                                mixingID = buildingGlue.MixingInfoID;
                            }
                            var mixingInfo = _repoMixingInfo.FindById(mixingID);
                            var batch = "";
                            var mixDate = new DateTime();
                            if (mixingInfo != null)
                            {
                                switch (item.Position)
                                {
                                    case "A":
                                        batch = mixingInfo.BatchA;
                                        break;
                                    case "B":
                                        batch = mixingInfo.BatchB;
                                        break;
                                    case "C":
                                        batch = mixingInfo.BatchC;
                                        break;
                                    case "D":
                                        batch = mixingInfo.BatchD;
                                        break;
                                    case "E":
                                        batch = mixingInfo.BatchE;
                                        break;
                                    default:
                                        break;
                                }
                                mixDate = mixingInfo.CreatedTime;
                            }
                            var detail = new TroubleshootingDto
                            {
                                Ingredient = item.Ingredient.Name,
                                GlueName = item.Glue.Name,
                                ModelName = plan.ModelName,
                                ModelNo = plan.ModelNo,
                                ArticleNo = plan.ArticleNo,
                                Process = plan.Process,
                                Line = plan.Line,
                                DueDate = plan.DueDate.Date,
                                Batch = batch,
                                MixDate = mixDate
                            };
                            troubleshootings.Add(detail);
                        }
                    }
                }
                return troubleshootings.Where(x => x.Batch.Equals(batchValue)).OrderByDescending(x => x.MixDate).DistinctBy(x => x.Line).ToList();
            }
            catch
            {
                return new List<TroubleshootingDto>();
            }
        }

        public async Task<bool> Add(PlanDto model)
        {
            var checkExist = await _repoPlan.FindAll().AnyAsync(x => x.BuildingID == model.BuildingID && x.BPFCEstablishID == model.BPFCEstablishID && x.DueDate.Date == model.DueDate.Date);
            if (!checkExist)
            {
                var plan = _mapper.Map<Plan>(model);
                plan.CreatedDate = DateTime.Now;
                plan.BPFCEstablishID = model.BPFCEstablishID;
                _repoPlan.Add(plan);
                var result = await _repoPlan.SaveAll();
                await _hubContext.Clients.All.SendAsync("summaryRecieve", "ok");
                return result;
            }
            else
            {
                return false;
            }
        }
        //Lấy danh sách Plan và phân trang
        public async Task<PagedList<PlanDto>> GetWithPaginations(PaginationParams param)
        {
            var lists = _repoPlan.FindAll().ProjectTo<PlanDto>(_configMapper).OrderByDescending(x => x.ID);
            return await PagedList<PlanDto>.CreateAsync(lists, param.PageNumber, param.PageSize);
        }

        //Tìm kiếm Plan
        public Task<PagedList<PlanDto>> Search(PaginationParams param, object text)
        {
            throw new System.NotImplementedException();

        }
        //Xóa Plan
        public async Task<bool> Delete(object id)
        {
            var Plan = _repoPlan.FindById(id);
            _repoPlan.Remove(Plan);
            await _hubContext.Clients.All.SendAsync("summaryRecieve", "ok");
            return await _repoPlan.SaveAll();
        }

        //Cập nhật Plan
        public async Task<bool> Update(PlanDto model)
        {
            var plan = _mapper.Map<Plan>(model);
            plan.CreatedDate = DateTime.Now;
            _repoPlan.Update(plan);
            var result = await _repoPlan.SaveAll();
            await _hubContext.Clients.All.SendAsync("summaryRecieve", "ok");
            return result;
        }

        //Lấy toàn bộ danh sách Plan 
        public async Task<List<PlanDto>> GetAllAsync()
        {
            var min = DateTime.Now.Date;
            var max = DateTime.Now.AddDays(15).Date;
            var r = await _repoPlan.FindAll()
                .Include(x => x.Building)
                .Include(x => x.BPFCEstablish)
                .ThenInclude(x => x.Glues)
                .Include(x => x.BPFCEstablish)
                .ThenInclude(x => x.ModelName)
                .Include(x => x.BPFCEstablish)
                .ThenInclude(x => x.ModelNo)
                .Include(x => x.BPFCEstablish)
                .ThenInclude(x => x.ArticleNo)
                .Include(x => x.BPFCEstablish)
                .ThenInclude(x => x.ArtProcess)
                .ThenInclude(x => x.Process)
                .ProjectTo<PlanDto>(_configMapper)
                .OrderByDescending(x => x.ID)
                .ToListAsync();
            return r;
        }
        public async Task<List<GlueCreateDto1>> GetGlueByBuilding(int buildingID)
        {
            var item = _repoBuilding.FindById(buildingID);
            var lineList = await _repoBuilding.FindAll().Where(x => x.ParentID == item.ID).Select(x => x.ID).ToListAsync();
            List<int> modelNameID = _repoPlan.FindAll().Where(x => lineList.Contains(x.BuildingID)).Select(x => x.BPFCEstablishID).ToList();
            var lists = await _repoGlue.FindAll().Where(x => x.isShow == true).Where(x => x.isShow == true).ProjectTo<GlueCreateDto1>(_configMapper).Where(x => modelNameID.Contains(x.BPFCEstablishID)).OrderByDescending(x => x.ID).Select(x => new GlueCreateDto1
            {
                ID = x.ID,
                Name = x.Name,
                GlueID = x.GlueID,
                Code = x.Code,
                ModelNo = x.ModelNo,
                CreatedDate = x.CreatedDate,
                BPFCEstablishID = x.BPFCEstablishID,
                PartName = x.PartName,
                PartNameID = x.PartNameID,
                MaterialNameID = x.MaterialNameID,
                MaterialName = x.MaterialName,
                Consumption = x.Consumption,
                Chemical = new GlueDto1 { ID = x.GlueID, Name = x.Name }
            }).ToListAsync();
            return lists.DistinctBy(x => x.Name).ToList();
        }
        public async Task<List<GlueCreateDto1>> GetGlueByBuildingModelName(int buildingID, int bpfc)
        {
            var item = _repoBuilding.FindById(buildingID);
            var lineList = await _repoBuilding.FindAll().Where(x => x.ParentID == item.ID).Select(x => x.ID).ToListAsync();
            List<int> modelNameID = _repoPlan.FindAll().Where(x => lineList.Contains(x.BuildingID)).Select(x => x.BPFCEstablishID).ToList();
            var lists = await _repoGlue.FindAll().Where(x => x.isShow == true).ProjectTo<GlueCreateDto1>(_configMapper).Where(x => x.BPFCEstablishID == bpfc).OrderByDescending(x => x.ID).Select(x => new GlueCreateDto1
            {
                ID = x.ID,
                Name = x.Name,
                GlueID = x.GlueID,
                Code = x.Code,
                ModelNo = x.ModelNo,
                CreatedDate = x.CreatedDate,
                BPFCEstablishID = x.BPFCEstablishID,
                PartName = x.PartName,
                PartNameID = x.PartNameID,
                MaterialNameID = x.MaterialNameID,
                MaterialName = x.MaterialName,
                Consumption = x.Consumption,
                Chemical = new GlueDto1 { ID = x.GlueID, Name = x.Name }
            }).ToListAsync();
            return lists.DistinctBy(x => x.Name).ToList();
        }
        //Lấy Plan theo Plan_Id
        public PlanDto GetById(object id)
        {
            return _mapper.Map<Plan, PlanDto>(_repoPlan.FindById(id));
        }

        public async Task<object> GetLines(int buildingID)
        {
            var item = _repoBuilding.FindById(buildingID);
            if (item == null) return new List<BuildingDto>();
            if (item.Level == 2)
            {
                var lineList = _repoBuilding.FindAll().Where(x => x.ParentID == item.ID);
                return await lineList.ProjectTo<BuildingDto>(_configMapper).ToListAsync();
            }
            else
            {
                var lineList = _repoBuilding.FindAll().Where(x => x.Level == 5);
                return await lineList.ProjectTo<BuildingDto>(_configMapper).ToListAsync();
            }

        }

        private async Task<object> Summary3(int building)
        {

            var currentDate = DateTime.Now.Date;
            var item = _repoBuilding.FindById(building);
            var lineList = await _repoBuilding.FindAll().Where(x => x.ParentID == item.ID).ToListAsync();
            var plans = _repoPlan.FindAll()
            .Include(x => x.BPFCEstablish)
            .ThenInclude(x => x.ModelName)
            .Where(x => x.DueDate.Date == currentDate).ToList();
            // Header
            var header = new List<HeaderForSummary> {
                  new HeaderForSummary
                {
                    field = "Supplier",
                },
                new HeaderForSummary
                {
                    field = "Chemical"
                }
            };
            var modelNameList = new List<string>();
            foreach (var line in lineList)
            {
                var itemHeader = new HeaderForSummary
                {
                    field = line.Name
                };
                var plan = plans.Where(x => x.BuildingID == line.ID).OrderByDescending(x => x.CreatedDate).FirstOrDefault();
                if (plan != null)
                {
                    modelNameList.Add(plan.BPFCEstablish.ModelName.Name);
                }
                else
                {
                    modelNameList.Add(string.Empty);
                }
                header.Add(itemHeader);
            }
            // end header

            // Data
            var model = (from glue in _repoGlue.FindAll().ToList()
                         join bpfc in _repoBPFC.FindAll().Include(x => x.ModelName).ToList() on glue.BPFCEstablishID equals bpfc.ID
                         join plan in plans on bpfc.ID equals plan.BPFCEstablishID
                         join bui in lineList on plan.BuildingID equals bui.ID
                         select new SummaryDto
                         {
                             GlueID = glue.ID,
                             BuildingID = bui.ID,
                             GlueName = glue.Name,
                             BuildingName = bui.Name,
                             Comsumption = glue.Consumption,
                             ModelNameID = bpfc.ModelNameID,
                             WorkingHour = plan.WorkingHour,
                             HourlyOutput = plan.HourlyOutput
                         }).ToList();
            var data2 = new List<object>();
            var data = new List<object>();
            var plannings = _repoPlan.FindAll().Where(x => x.DueDate.Date == currentDate && lineList.Select(x => x.ID).Contains(x.BuildingID)).Select(p => p.BPFCEstablishID);
            var glueList = _repoGlue.FindAll()
                .Where(x => x.isShow == true)
                .Include(x => x.GlueIngredients)
                    .ThenInclude(x => x.Ingredient)
                    .ThenInclude(x => x.Supplier)
                .Include(x => x.BPFCEstablish)
                    .ThenInclude(x => x.ModelName)
                .Include(x => x.BPFCEstablish)
                    .ThenInclude(x => x.Plans)
                    .ThenInclude(x => x.Building)
                .Include(x => x.MixingInfos)
                .Where(x => plannings.Contains(x.BPFCEstablishID))
                .Select(x => new
                {
                    GlueIngredients = x.GlueIngredients.Select(a => new { a.GlueID, a.Ingredient, a.Position }),
                    x.Name,
                    x.ID,
                    x.BPFCEstablishID,
                    x.BPFCEstablish.Plans,
                    x.Consumption,
                    MixingInfos = x.MixingInfos.Select(a => new { a.GlueName, a.CreatedTime, a.ChemicalA, a.ChemicalB, a.ChemicalC, a.ChemicalD, a.ChemicalE, })
                });
            var glueDistinct = glueList.DistinctBy(x => x.Name);
            var rowParents = new List<RowParentDto>();
            foreach (var glue in glueDistinct)
            {

                var itemData = new ItemData<string, object>();
                var supplier = glue.GlueIngredients.FirstOrDefault(x => x.Position.Equals("A")) == null ? "#N/A" : glue.GlueIngredients.FirstOrDefault(x => x.Position.Equals("A")).Ingredient.Supplier.Name;
                var glueInfo = new GlueInfo { GlueName = glue.Name, BPFC = "" };
                itemData.Add("Supplier", supplier);
                itemData.Add("Chemical", glueInfo);
                var listTotal = new List<double>();
                var listStandardTotal = new List<double>();
                var listWorkingHour = new List<double>();
                var listHourlyOuput = new List<double>();
                var rowRealInfo = new List<object>();
                var rowCountInfo = new List<object>();
                var delivered = await _repoBuildingGlue.FindAll()
                                        .Where(x => x.GlueName.Equals(glue.Name) && lineList.Select(a => a.ID).Contains(x.BuildingID) && x.CreatedDate.Date == currentDate)
                                        .OrderBy(x => x.CreatedDate)
                                        .Select(x => new DeliveredInfo
                                        {
                                            ID = x.ID,
                                            Qty = x.Qty,
                                            GlueName = x.GlueName,
                                            CreatedDate = x.CreatedDate,
                                            LineID = x.BuildingID
                                        })
                                        .ToListAsync();
                var deliver = delivered.Select(x => x.Qty).ToList().ConvertAll<double>(Convert.ToDouble).Sum();
                var mixingInfos = await _repoMixingInfo.FindAll().Where(x => x.GlueName.Equals(glue.Name) && x.CreatedTime.Date == currentDate).ToListAsync();
                double realTotal = 0;
                foreach (var real in mixingInfos)
                {
                    realTotal += real.ChemicalA.ToDouble() + real.ChemicalB.ToDouble() + real.ChemicalC.ToDouble() + real.ChemicalD.ToDouble() + real.ChemicalE.ToDouble();
                }
                foreach (var line in lineList.OrderBy(x => x.Name))
                {

                    var sdtCon = model.FirstOrDefault(x => x.GlueName.Equals(glue.Name) && x.BuildingID == line.ID);
                    var listBuildingGlue = delivered.Where(x => x.GlueName.Equals(glue.Name) && x.LineID == line.ID && x.CreatedDate.Date == currentDate).OrderByDescending(x => x.CreatedDate).ToList();
                    double real = 0;
                    if (listBuildingGlue.FirstOrDefault() != null)
                    {
                        real = listBuildingGlue.FirstOrDefault().Qty.ToDouble();
                    }
                    double comsumption = 0;
                    if (sdtCon != null)
                    {
                        comsumption = glue.Consumption.ToDouble() * sdtCon.WorkingHour.ToDouble() * sdtCon.HourlyOutput.ToDouble();
                        itemData.Add(line.Name, Math.Round(comsumption / 1000, 3) + "kg");
                        listTotal.Add(glue.Consumption.ToDouble());
                        listWorkingHour.Add(sdtCon.WorkingHour.ToDouble());
                        listHourlyOuput.Add(sdtCon.HourlyOutput.ToDouble());
                        listStandardTotal.Add(comsumption / 1000);
                    }
                    else
                    {
                        itemData.Add(line.Name, 0);
                    }

                    rowCountInfo.Add(new SummaryInfo
                    {
                        GlueName = glue.Name,
                        line = line.Name,
                        lineID = line.ID,
                        glueID = glue.ID,
                        value = Math.Round(real, 3),
                        count = listBuildingGlue.Count,
                        maxReal = realTotal,
                        delivered = Math.Round(deliver, 3),
                        deliveredInfos = listBuildingGlue,
                        consumption = comsumption / 1000
                    });
                    rowRealInfo.Add(new SummaryInfo
                    {
                        GlueName = glue.Name,
                        line = line.Name,
                        lineID = line.ID,
                        glueID = glue.ID,
                        value = Math.Round(real, 3),
                        count = listBuildingGlue.Count,
                        maxReal = realTotal,
                        delivered = Math.Round(deliver, 3),
                        consumption = comsumption / 1000
                    });

                }
                itemData.Add("Real", $"{Math.Round(deliver, 3)}kg / {Math.Round(realTotal, 3)}kg");
                itemData.Add("Count", glue.MixingInfos.Where(x => x.CreatedTime.Date == currentDate).Count());
                itemData.Add("rowRealInfo", rowRealInfo);
                itemData.Add("rowCountInfo", rowCountInfo);
                data.Add(itemData);

            }
            var infoList = new List<HeaderForSummary>() {
                  new HeaderForSummary
                {
                    field = "Real"
                },
                new HeaderForSummary
                {
                    field = "Count"
                }};

            header.AddRange(infoList);
            // End Data
            return new { header, data, modelNameList };

        }
        public async Task<object> Summary(int building)
        {
            try
            {
                //var currentDate = new DateTime(2020, 10, 22).Date;
                var currentDate = DateTime.Now.Date;
                var lines = await _repoBuilding.FindAll(x => x.ParentID == building).Select(x => x.ID).ToListAsync();
                var plans = await _repoPlan.FindAll()
                .Include(x => x.BPFCEstablish)
                .ThenInclude(x => x.ModelName)
                 .Include(x => x.BPFCEstablish)
                .ThenInclude(x => x.Glues)
                .ThenInclude(x => x.GlueIngredients)
                .ThenInclude(x => x.Ingredient)
                .ThenInclude(x => x.Supplier)
                .Include(x => x.Building)
                .Where(x => lines.Contains(x.BuildingID) && x.DueDate.Date == currentDate).Select(x => new
                {
                    x.BPFCEstablishID,
                    x.BPFCEstablish,
                    LineID = x.Building.ID,
                    LineName = x.Building.Name,
                    x.WorkingHour,
                    x.HourlyOutput,
                    x.BPFCEstablish.Glues
                }).ToListAsync();

                var gluesList = plans.SelectMany(x => x.Glues).Select(x => x.Name);
                var mixingInfoModel = await _repoMixingInfo.FindAll()
                    .Where(x => gluesList.Contains(x.GlueName) && x.BuildingID == building && x.CreatedTime.Date == currentDate).ToListAsync();
                var buildingGlueModel = await _repoBuildingGlue.FindAll()
                                       .Where(x => gluesList.Contains(x.GlueName) && lines.Contains(x.BuildingID) && x.CreatedDate.Date == currentDate).ToListAsync();
                // Header
                var header = new List<HeaderForSummary> {
                  new HeaderForSummary
                {
                    field = "Supplier",
                    HasRowspan = true,
                    RowspanValue = 0
                },
                new HeaderForSummary
                {
                    field = "Chemical",
                    HasRowspan = true,
                    RowspanValue = 0
                }
            };
                var modelNameHeader = new List<HeaderForSummary>();
                // Data

                var planHeaders = plans.Select(x => new
                {
                    ModelName = x.BPFCEstablish.ModelName.Name,
                    x.BPFCEstablishID,
                    LineName = x.LineName,
                    LineID = x.LineID,
                    x.WorkingHour,
                    x.HourlyOutput,
                    x.Glues,

                }).OrderBy(x => x.LineName).DistinctBy(x => new { x.LineName, x.ModelName });

                var rowParents = new List<RowParentDto>();
                var glueDistinct = planHeaders.SelectMany(x => x.Glues).ToList().Where(x => x.isShow).DistinctBy(x => x.Name);
                foreach (var plan in planHeaders)
                {
                    header.Add(new HeaderForSummary
                    {
                        field = plan.LineName,
                        ColspanValue = planHeaders.Where(x => x.LineID == plan.LineID).Count(),
                        HasColspan = true

                    });
                    modelNameHeader.Add(new HeaderForSummary
                    {
                        field = plan.ModelName,
                    });
                }
                foreach (var glue in glueDistinct)
                {
                    var rowParent = new RowParentDto();
                    var rowChild1 = new RowChildDto();
                    var rowChild2 = new RowChildDto();
                    var cellInfos = new List<CellInfoDto>();

                    var itemData = new ItemData<string, object>();
                    var supplier = glue.GlueIngredients.FirstOrDefault(x => x.Position.Equals("A")) == null ? "#N/A" : glue.GlueIngredients.FirstOrDefault(x => x.Position.Equals("A")).Ingredient.Supplier.Name;
                    // Static Left
                    rowChild1.Supplier = new CellInfoDto() { Supplier = supplier };
                    rowChild1.GlueName = new CellInfoDto() { GlueName = glue.Name };
                    rowChild1.GlueID = glue.ID;

                    rowChild2.Supplier = new CellInfoDto() { Supplier = supplier };
                    rowChild2.GlueName = new CellInfoDto() { GlueName = glue.Name };
                    rowChild2.GlueID = glue.ID;
                    // End Static Left
                    var listTotal = new List<double>();
                    var listStandardTotal = new List<double>();
                    var listWorkingHour = new List<double>();
                    var listHourlyOuput = new List<double>();


                    var mixingInfos = mixingInfoModel.Where(x => x.GlueName.Equals(glue.Name));

                    var delivered = buildingGlueModel
                                        .Where(x => x.GlueName.Equals(glue.Name))
                                        .OrderBy(x => x.CreatedDate)
                                        .Select(x => new DeliveredInfo
                                        {
                                            ID = x.ID,
                                            Qty = x.Qty,
                                            GlueName = x.GlueName,
                                            CreatedDate = x.CreatedDate,
                                            LineID = x.BuildingID
                                        });
                    double realTotal = 0;
                    double deliver = 0;
                    var deliverTotal = delivered.Select(x => x.Qty).ToList().ConvertAll<double>(Convert.ToDouble).Sum();

                    foreach (var plan in planHeaders)
                    {
                        var status = plan.Glues.Count > 0 ? plan.Glues.Select(x => x.Name).Any(x => x == glue.Name) : false;
                        realTotal = 0;
                        deliver = 0;
                        foreach (var mixingInfo in mixingInfos)
                        {
                            realTotal += mixingInfo.ChemicalA.ToDouble() + mixingInfo.ChemicalB.ToDouble() + mixingInfo.ChemicalC.ToDouble() + mixingInfo.ChemicalD.ToDouble() + mixingInfo.ChemicalE.ToDouble();
                        }

                        if (delivered.Count() > 0)
                        {
                            deliver = delivered.Where(x => x.LineID == plan.LineID).Select(x => x.Qty).ToList().ConvertAll<double>(Convert.ToDouble).Sum();
                        }

                        var listBuildingGlue = delivered.Where(x => x.GlueName.Equals(glue.Name) && x.LineID == plan.LineID && x.CreatedDate.Date == currentDate).OrderByDescending(x => x.CreatedDate);
                        double real = 0;
                        var listBuildingGlueCount = listBuildingGlue.Count();
                        if (listBuildingGlueCount > 0)
                        {
                            real = listBuildingGlue.FirstOrDefault().Qty.ToDouble();
                        }
                        double comsumption = glue.Consumption.ToDouble() * plan.WorkingHour * plan.HourlyOutput;
                        itemData.Add(plan.LineName, Math.Round(comsumption / 1000, 3) + "kg");
                        listTotal.Add(glue.Consumption.ToDouble());
                        listWorkingHour.Add(plan.WorkingHour);
                        listHourlyOuput.Add(plan.HourlyOutput);
                        listStandardTotal.Add(comsumption / 1000);

                        double deliverdTotal = 0;
                        if (listBuildingGlueCount > 0)
                        {
                            deliverdTotal = delivered.Select(x => x.Qty).ToList().ConvertAll<double>(Convert.ToDouble).Sum();
                        }
                        var dynamicCellInfo = new CellInfoDto
                        {
                            status = status,
                            modelName = plan.ModelName,
                            GlueName = glue.Name,
                            line = plan.LineName,
                            lineID = plan.LineID,
                            GlueID = rowChild1.GlueID,
                            value = real >= 1 && real < 10 ? Math.Round(real, 2) : real >= 10 ? Math.Round(real, 1) : Math.Round(real, 3),
                            count = listBuildingGlueCount,
                            maxReal = realTotal,
                            delivered = deliver >= 1 && deliver < 10 ? Math.Round(deliver, 2) : deliver >= 10 ? Math.Round(deliver, 1) : Math.Round(deliver, 3),
                            deliveredTotal = Math.Round(deliverTotal, 3),
                            deliveredInfos = listBuildingGlue.Select(x => new DeliveredInfo
                            {
                                ID = x.ID,
                                LineID = x.LineID,
                                Qty = (x.Qty.ToDouble() >= 1 && x.Qty.ToDouble() < 10 ? Math.Round(x.Qty.ToDouble(), 2) : x.Qty.ToDouble() >= 10 ? Math.Round(x.Qty.ToDouble(), 1) : Math.Round(x.Qty.ToDouble(), 3)).ToSafetyString(),
                                GlueName = x.GlueName,
                                CreatedDate = x.CreatedDate
                            }).ToList(),
                            consumption = comsumption / 1000
                        };
                        cellInfos.Add(dynamicCellInfo);
                        rowChild1.CellsCenter = cellInfos;
                        rowChild2.CellsCenter = cellInfos;

                    }
                    // Static Right
                    var actual = $"{Math.Round(deliverTotal, 3)}kg / {Math.Round(realTotal, 3)}kg";
                    var count = mixingInfos.Where(x => x.CreatedTime.Date == currentDate).Count();
                    rowChild1.Actual = new CellInfoDto() { real = actual };
                    rowChild1.Count = new CellInfoDto() { count = count };
                    rowChild2.Actual = new CellInfoDto() { real = actual };
                    rowChild2.Count = new CellInfoDto() { count = count };
                    // End Static Right
                    rowParent.Row1 = rowChild1;
                    rowParent.Row2 = rowChild2;
                    rowParents.Add(rowParent);
                }
                var infoList = new List<HeaderForSummary>() {
                  new HeaderForSummary
                {
                    field = "Real",
                    HasRowspan = true,
                    RowspanValue = 0
                },
                new HeaderForSummary
                {
                    field = "Count",
                    HasRowspan = true,
                    RowspanValue = 0
                }, new HeaderForSummary
                {
                    field = "Option",
                    HasRowspan = true,
                    RowspanValue = 0
                }
            };

                header.AddRange(infoList);
                // End Data
                return new { header = header.DistinctBy(x => x.field), rowParents, modelNameList = modelNameHeader };
            }
            catch
            {
                return new
                {
                    header = new List<HeaderForSummary> { },
                    rowParents = new List<RowParentDto> { },
                    modelNameList = new List<HeaderForSummary> { }
                };

            }

        }
        public Task<object> GetAllPlansByDate(string from, string to)
        {
            throw new System.NotImplementedException();
        }

        public async Task<List<PlanDto>> GetGlueByBuildingBPFCID(int buildingID, int bpfcID)
        {
            var lists = await _repoGlue.FindAll().Where(x => x.isShow == true && x.BPFCEstablishID == bpfcID).ProjectTo<PlanDto>(_configMapper).OrderByDescending(x => x.ID).ToListAsync();
            return lists.ToList();
        }
        public async Task<object> DispatchGlue(BuildingGlueForCreateDto obj)
        {
            try
            {
                var buildingGlue = _mapper.Map<BuildingGlue>(obj);
                var building = _repoBuilding.FindById(obj.BuildingID);
                var lastMixingInfo = await _repoMixingInfo.FindAll().Where(x => x.GlueName.Contains(obj.GlueName) && x.BuildingID == building.ParentID).OrderByDescending(x => x.CreatedTime).FirstOrDefaultAsync();
                buildingGlue.MixingInfoID = lastMixingInfo == null ? 0 : lastMixingInfo.ID;
                _repoBuildingGlue.Add(buildingGlue);
                return await _repoBuildingGlue.SaveAll();
            }
            catch
            {
                return false;
            }
        }
        public async Task<object> ClonePlan(List<PlanForCloneDto> plansDto)
        {
            var plans = _mapper.Map<List<Plan>>(plansDto);
            var flag = false;
            try
            {
                foreach (var item in plans)
                {
                    var checkExist = await _repoPlan.FindAll().AnyAsync(x => x.BuildingID == item.BuildingID && x.BPFCEstablishID == item.BPFCEstablishID && x.DueDate.Date == item.DueDate.Date);
                    if (!checkExist)
                    {
                        _repoPlan.Add(item);
                        flag = await _repoPlan.SaveAll();
                    }
                }
                return flag;
            }
            catch
            {
                return flag;
            }

        }
        public async Task<object> DeleteRange(List<int> plansDto)
        {
            var plans = await _repoPlan.FindAll().Where(x => plansDto.Contains(x.ID)).ToListAsync();
            _repoPlan.RemoveMultiple(plans);
            return await _repoBuildingGlue.SaveAll();

        }
        public async Task<object> GetAllPlanByDefaultRange()
        {
            var min = DateTime.Now.Date;
            var max = DateTime.Now.AddDays(15).Date;
            return await _repoPlan.FindAll()
                .Where(x => x.DueDate.Date >= min && x.DueDate <= max)
                .Include(x => x.Building)
                .Include(x => x.BPFCEstablish)
                .ThenInclude(x => x.Glues.Where(x => x.isShow == true))
                .Include(x => x.BPFCEstablish)
                .ThenInclude(x => x.ModelName)
                .Include(x => x.BPFCEstablish)
                .ThenInclude(x => x.ModelNo)
                .Include(x => x.BPFCEstablish)
                .ThenInclude(x => x.ArticleNo)
                .Include(x => x.BPFCEstablish)
                .ThenInclude(x => x.ArtProcess)
                .ThenInclude(x => x.Process)
                .ProjectTo<PlanDto>(_configMapper)
                .OrderByDescending(x => x.ID)
                .ToListAsync();
        }

        public async Task<object> GetAllPlanByRange(int building, DateTime min, DateTime max)
        {
            var lines = new List<int>();
            if (building == 0)
            {
                lines = await _repoBuilding.FindAll(x => x.Level == 3).Select(x => x.ID).ToListAsync();
            }
            else
            {
                lines = await _repoBuilding.FindAll(x => x.ParentID == building).Select(x => x.ID).ToListAsync();
            }
            return await _repoPlan.FindAll()
                .Where(x => x.DueDate.Date >= min.Date && x.DueDate.Date <= max.Date && lines.Contains(x.BuildingID))
                .Include(x => x.Building)
                .Include(x => x.BPFCEstablish)
                .ThenInclude(x => x.ModelName)
                .Include(x => x.BPFCEstablish)
                .ThenInclude(x => x.ModelNo)
                .Include(x => x.BPFCEstablish)
                .ThenInclude(x => x.ArticleNo)
                .Include(x => x.BPFCEstablish)
                .ThenInclude(x => x.ArtProcess)
                .ThenInclude(x => x.Process)
                .ProjectTo<PlanDto>(_configMapper)
                .OrderByDescending(x => x.ID)
                .ToListAsync();
        }

        public async Task<object> GetBPFCByGlue(TooltipParams tooltip)
        {
            var name = tooltip.Glue.Trim().ToSafetyString();
            var results = new List<string>();
            var plans = await _repoPlan.FindAll()
                 .Include(x => x.BPFCEstablish)
                     .ThenInclude(x => x.ModelName)
                     .ThenInclude(x => x.ModelNos)
                     .ThenInclude(x => x.ArticleNos)
                     .ThenInclude(x => x.ArtProcesses)
                     .ThenInclude(x => x.Process)
                 .Include(x => x.BPFCEstablish)
                     .ThenInclude(x => x.Glues)
                 .Where(x => x.DueDate.Date == DateTime.Now.Date).ToListAsync();
            foreach (var plan in plans)
            {
                foreach (var glue in plan.BPFCEstablish.Glues.Where(x => x.isShow == true && x.Name.Trim().Equals(name)))
                {
                    var bpfc = $"{plan.BPFCEstablish.ModelName.Name} -> {plan.BPFCEstablish.ModelNo.Name} -> {plan.BPFCEstablish.ArticleNo.Name} -> {plan.BPFCEstablish.ArtProcess.Process.Name}";
                    results.Add(bpfc);
                }
            }
            return results.Distinct();
        }

        public async Task<bool> EditDelivered(int id, string qty)
        {
            try
            {
                var item = _repoBuildingGlue.FindById(id);
                item.Qty = qty.ToDouble().ToSafetyString();
                return await _repoBuildingGlue.SaveAll();
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteDelivered(int id)
        {
            try
            {
                var item = _repoBuildingGlue.FindById(id);
                _repoBuildingGlue.Remove(item);
                return await _repoBuildingGlue.SaveAll();
            }
            catch
            {
                return false;
            }
        }

        public async Task<byte[]> Report(DateTime startDate, DateTime endDate)
        {
            var plans = await _repoPlan.FindAll()
                .Where(x => x.DueDate.Date >= startDate.Date && x.DueDate.Date <= endDate.Date)
                .Include(x => x.Building)
                .Include(x => x.BPFCEstablish)
                .ThenInclude(x => x.ModelName)
                 .Include(x => x.BPFCEstablish)
                .ThenInclude(x => x.ModelNo)
                .Include(x => x.BPFCEstablish)
                .ThenInclude(x => x.Glues)
                .ThenInclude(x => x.GlueIngredients)
                .ThenInclude(x => x.Ingredient)
                .Select(x => new
                {
                    Glues = x.BPFCEstablish.Glues.Where(x => x.isShow),
                    GlueIngredients = x.BPFCEstablish.Glues.Where(x => x.isShow).SelectMany(x => x.GlueIngredients),
                    ModelName = x.BPFCEstablish.ModelName.Name,
                    ModelNo = x.BPFCEstablish.ModelNo.Name,
                    x.Quantity,
                    Line = x.Building.Name,
                    LineID = x.Building.ID,
                    x.DueDate,
                    x.BPFCEstablishID
                }).OrderBy(x => x.DueDate.Date)
                .ToListAsync();

            var buildingGlues = await _repoBuildingGlue.FindAll()
                .Where(x => x.CreatedDate.Date >= startDate.Date && x.CreatedDate.Date <= endDate.Date)
                .ToListAsync();
            var buildingGlueModel = from a in buildingGlues
                                    join b in _repoGlue.FindAll().Include(x => x.GlueIngredients).ToList() on a.GlueName equals b.Name
                                    select new
                                    {
                                        a.Qty,
                                        a.BuildingID,
                                        a.CreatedDate,
                                        b.BPFCEstablishID,
                                        IngredientIDList = b.GlueIngredients.Select(x => x.IngredientID)
                                    };
            var ingredients = plans.SelectMany(x => x.GlueIngredients).Select(x => new IngredientReportDto
            {
                CBD = x.Ingredient.CBD,
                Real = x.Ingredient.Real,
                Name = x.Ingredient.Name,
                ID = x.IngredientID,
                Position = x.Position
            }).DistinctBy(x => x.Name);

            var ingredientsHeader = ingredients.Select(x => x.Name).ToList();

            var headers = new ReportHeaderDto();
            headers.Ingredients = ingredientsHeader;
            var bodyList = new List<ReportBodyDto>();
            var planModel = plans.OrderBy(x => x.DueDate.Date).ThenBy(x => x.Line).ToList();
            foreach (var plan in planModel)
            {
                var body = new ReportBodyDto
                {
                    Day = plan.DueDate.Day,
                    CBD = 0,
                    Real = 0,
                    ModelName = plan.ModelName,
                    ModelNo = plan.ModelNo,
                    Quantity = plan.Quantity,
                    Line = plan.Line,
                    LineID = plan.LineID,
                    Date = plan.DueDate.Date,
                };
                var ingredientsBody2 = new List<IngredientBodyReportDto>();
                foreach (var ingredient in ingredients)
                {
                    foreach (var glue in plan.Glues)
                    {
                        if (glue.GlueIngredients.Any(x => x.IngredientID == ingredient.ID) && plan.BPFCEstablishID == glue.BPFCEstablishID)
                        {
                            var buildingGlue = buildingGlueModel.Where(x => x.BuildingID == body.LineID && x.IngredientIDList.Contains(ingredient.ID) && x.CreatedDate.Date == plan.DueDate.Date && x.BPFCEstablishID == glue.BPFCEstablishID)
                            .Distinct().ToList();

                            var quantity = buildingGlue.Select(x => x.Qty).ToList().ConvertAll<double>(Convert.ToDouble).Sum();
                            var glueIngredients = glue.GlueIngredients.DistinctBy(x => x.Position).ToList();
                            double value = CalculateIngredientByPositon(glueIngredients, ingredient, quantity);
                            ingredientsBody2.Add(new IngredientBodyReportDto { Value = value, Name = ingredient.Name, Line = body.Line });
                        }
                    }
                }
                body.Ingredients2 = ingredientsBody2;
                var ingredientsBody = new List<double>();

                foreach (var ingredientName in ingredientsHeader)
                {
                    var model = ingredientsBody2.FirstOrDefault(x => x.Name.Equals(ingredientName));
                    if (model != null)
                    {
                        ingredientsBody.Add(model.Value);
                    }
                    else
                    {
                        ingredientsBody.Add(0);

                    }

                }
                body.Ingredients = ingredientsBody;

                bodyList.Add(body);
            }

            return ExportExcel(headers, bodyList, ingredients.ToList());
        }
        double SumProduct(double[] arrayA, double[] arrayB)
        {
            double result = 0;
            for (int i = 0; i < arrayA.Count(); i++)
                result += arrayA[i] * arrayB[i];
            return result;
        }
        private Byte[] ExportExcelConsumptionCase2(List<ConsumtionDto> consumtionDtos)
        {
            try
            {
                consumtionDtos = consumtionDtos.OrderByDescending(x => x.DueDate).OrderBy(x => x.DueDate).ThenBy(x => x.Line).ThenBy(x => x.ID).ThenByDescending(x => x.Percentage).ToList();
                ExcelPackage.LicenseContext = LicenseContext.Commercial;
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                var memoryStream = new MemoryStream();
                using (ExcelPackage p = new ExcelPackage(memoryStream))
                {
                    // đặt tên người tạo file
                    p.Workbook.Properties.Author = "Henry Pham";

                    // đặt tiêu đề cho file
                    p.Workbook.Properties.Title = "ReportConsumption";
                    //Tạo một sheet để làm việc trên đó
                    p.Workbook.Worksheets.Add("ReportConsumption");

                    // lấy sheet vừa add ra để thao tác
                    ExcelWorksheet ws = p.Workbook.Worksheets["ReportConsumption"];

                    // đặt tên cho sheet
                    ws.Name = "ReportConsumption";
                    // fontsize mặc định cho cả sheet
                    ws.Cells.Style.Font.Size = 12;
                    // font family mặc định cho cả sheet
                    ws.Cells.Style.Font.Name = "Calibri";
                    var headers = new string[]{
                        "Line", "Model Name", "Model No.", "Article No.",
                        "Process", "Qty", "Glue", "Std.(g)", "Real Consumption(g)pr.", "Diff.", "%"
                    };

                    int headerRowIndex = 1;
                    int headerColIndex = 1;
                    foreach (var header in headers)
                    {
                        int col = headerRowIndex++;
                        ws.Cells[headerColIndex, col].Value = header;
                        ws.Cells[headerColIndex, col].Style.Font.Bold = true;
                        ws.Cells[headerColIndex, col].Style.Font.Size = 12;
                    }

                    // end Style
                    int colIndex = 1;
                    int rowIndex = 1;
                    // với mỗi item trong danh sách sẽ ghi trên 1 dòng
                    foreach (var body in consumtionDtos)
                    {
                        // bắt đầu ghi từ cột 1. Excel bắt đầu từ 1 không phải từ 0 #c0514d
                        colIndex = 1;

                        // rowIndex tương ứng từng dòng dữ liệu
                        rowIndex++;


                        //gán giá trị cho từng cell                      
                        ws.Cells[rowIndex, colIndex++].Value = body.Line;
                        ws.Cells[rowIndex, colIndex++].Value = body.ModelName;
                        ws.Cells[rowIndex, colIndex++].Value = body.ModelNo;
                        ws.Cells[rowIndex, colIndex++].Value = body.ArticleNo;
                        ws.Cells[rowIndex, colIndex++].Value = body.Process;
                        ws.Cells[rowIndex, colIndex++].Value = body.Qty;
                        ws.Cells[rowIndex, colIndex++].Value = body.Glue;
                        ws.Cells[rowIndex, colIndex++].Value = body.Std;
                        ws.Cells[rowIndex, colIndex++].Value = Math.Round(body.RealConsumption, 2);
                        ws.Cells[rowIndex, colIndex++].Value = body.Diff;
                        ws.Cells[rowIndex, colIndex++].Value = body.Percentage + "%";
                    }
                    int colPatternIndex = 1;
                    int rowPatternIndex = 1;

                    int colColorIndex = 1;
                    int rowColorIndex = 1;
                    foreach (var body in consumtionDtos)
                    {
                        rowColorIndex++;
                        rowPatternIndex++;

                        if (body.Percentage > 0)
                        {
                            colPatternIndex = 7;
                            colColorIndex = 7;
                            ws.Cells[rowPatternIndex, colPatternIndex++].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            ws.Cells[rowPatternIndex, colPatternIndex++].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            ws.Cells[rowPatternIndex, colPatternIndex++].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            ws.Cells[rowPatternIndex, colPatternIndex++].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            ws.Cells[rowPatternIndex, colPatternIndex++].Style.Fill.PatternType = ExcelFillStyle.Solid;

                            ws.Cells[rowColorIndex, colColorIndex++].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#FFFF66"));
                            ws.Cells[rowColorIndex, colColorIndex++].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#FFFF66"));
                            ws.Cells[rowColorIndex, colColorIndex++].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#FFFF66"));
                            ws.Cells[rowColorIndex, colColorIndex++].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#FFFF66"));
                            ws.Cells[rowColorIndex, colColorIndex++].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#FFFF66"));
                        }
                    }
                    int mergeFromColIndex = 1;
                    int mergeToColIndex = 1;
                    int mergeFromRowIndex = 2;
                    int mergeToRowIndex = 1;
                    foreach (var item in consumtionDtos.GroupBy(x => new
                    {
                        x.ID,
                        x.Line,
                        x.ModelName,
                        x.ModelNo,
                        x.ArticleNo,
                        x.Process,
                        x.Qty
                    }))
                    {
                        mergeToRowIndex += item.Count();
                        ws.Cells[mergeFromRowIndex, mergeFromColIndex, mergeToRowIndex, mergeToColIndex].Merge = true;
                        ws.Cells[mergeFromRowIndex, mergeFromColIndex, mergeToRowIndex, mergeToColIndex].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        ws.Cells[mergeFromRowIndex, mergeFromColIndex, mergeToRowIndex, mergeToColIndex].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        ws.Cells[mergeFromRowIndex, 2, mergeToRowIndex, 2].Merge = true;
                        ws.Cells[mergeFromRowIndex, 2, mergeToRowIndex, 2].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        ws.Cells[mergeFromRowIndex, 2, mergeToRowIndex, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        ws.Cells[mergeFromRowIndex, 3, mergeToRowIndex, 3].Merge = true;
                        ws.Cells[mergeFromRowIndex, 3, mergeToRowIndex, 3].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        ws.Cells[mergeFromRowIndex, 3, mergeToRowIndex, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        ws.Cells[mergeFromRowIndex, 4, mergeToRowIndex, 4].Merge = true;
                        ws.Cells[mergeFromRowIndex, 4, mergeToRowIndex, 4].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        ws.Cells[mergeFromRowIndex, 4, mergeToRowIndex, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        ws.Cells[mergeFromRowIndex, 5, mergeToRowIndex, 5].Merge = true;
                        ws.Cells[mergeFromRowIndex, 5, mergeToRowIndex, 5].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        ws.Cells[mergeFromRowIndex, 5, mergeToRowIndex, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;


                        ws.Cells[mergeFromRowIndex, 6, mergeToRowIndex, 6].Merge = true;
                        ws.Cells[mergeFromRowIndex, 6, mergeToRowIndex, 6].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        ws.Cells[mergeFromRowIndex, 6, mergeToRowIndex, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        mergeFromRowIndex = mergeToRowIndex + 1;
                    }
                    //make the borders of cell F6 thick
                    ws.Cells[ws.Dimension.Address].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    ws.Cells[ws.Dimension.Address].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    ws.Cells[ws.Dimension.Address].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    ws.Cells[ws.Dimension.Address].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    foreach (var item in headers.Select((x, i) => new { Value = x, Index = i }))
                    {
                        var col = item.Index + 1;
                        ws.Column(col).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        ws.Column(col).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        if (col == 2 || col == 7)
                        {
                            ws.Column(col).AutoFit(30);
                        }
                        else
                        {
                            ws.Column(col).AutoFit();
                        }
                    }

                    //Lưu file lại
                    Byte[] bin = p.GetAsByteArray();
                    return bin;
                }
            }
            catch (Exception ex)
            {
                var mes = ex.Message;
                Console.Write(mes);
                return new Byte[] { };
            }
        }
        private Byte[] ExportExcelConsumptionCase1(List<ConsumtionDto> consumtionDtos)
        {
            try
            {
                consumtionDtos = consumtionDtos.OrderByDescending(x => x.Percentage).ToList();
                ExcelPackage.LicenseContext = LicenseContext.Commercial;
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                var memoryStream = new MemoryStream();
                using (ExcelPackage p = new ExcelPackage(memoryStream))
                {
                    // đặt tên người tạo file
                    p.Workbook.Properties.Author = "Henry Pham";

                    // đặt tiêu đề cho file
                    p.Workbook.Properties.Title = "ReportConsumption";
                    //Tạo một sheet để làm việc trên đó
                    p.Workbook.Worksheets.Add("ReportConsumption");

                    // lấy sheet vừa add ra để thao tác
                    ExcelWorksheet ws = p.Workbook.Worksheets["ReportConsumption"];

                    // đặt tên cho sheet
                    ws.Name = "ReportConsumption";
                    // fontsize mặc định cho cả sheet
                    ws.Cells.Style.Font.Size = 12;
                    // font family mặc định cho cả sheet
                    ws.Cells.Style.Font.Name = "Calibri";
                    var headers = new string[]{
                        "Model Name", "Model No.", "Article No.",
                        "Process", "Glue", "Std.(g)", "Glue Mixing Date", "Line", "Qty",
                        "Total Consumption(kg)", "Real Consumption(g)pr.", "Diff.", "%"
                    };

                    int headerRowIndex = 1;
                    int headerColIndex = 1;
                    foreach (var header in headers)
                    {
                        int col = headerRowIndex++;
                        ws.Cells[headerColIndex, col].Value = header;
                        ws.Cells[headerColIndex, col].Style.Font.Bold = true;
                        ws.Cells[headerColIndex, col].Style.Font.Size = 12;
                    }
                    // end Style
                    int colIndex = 1;
                    int rowIndex = 1;
                    // với mỗi item trong danh sách sẽ ghi trên 1 dòng
                    foreach (var body in consumtionDtos)
                    {
                        // bắt đầu ghi từ cột 1. Excel bắt đầu từ 1 không phải từ 0 #c0514d
                        colIndex = 1;

                        // rowIndex tương ứng từng dòng dữ liệu
                        rowIndex++;


                        //gán giá trị cho từng cell                      
                        ws.Cells[rowIndex, colIndex++].Value = body.ModelName;
                        ws.Cells[rowIndex, colIndex++].Value = body.ModelNo;
                        ws.Cells[rowIndex, colIndex++].Value = body.ArticleNo;
                        ws.Cells[rowIndex, colIndex++].Value = body.Process;
                        ws.Cells[rowIndex, colIndex++].Value = body.Glue;
                        ws.Cells[rowIndex, colIndex++].Value = body.Std;
                        ws.Cells[rowIndex, colIndex++].Value = body.MixingDate == DateTime.MinValue ? "N/A" : body.MixingDate.ToString("dd/MM/yyyy");
                        ws.Cells[rowIndex, colIndex++].Value = body.Line;
                        ws.Cells[rowIndex, colIndex++].Value = body.Qty;
                        ws.Cells[rowIndex, colIndex++].Value = Math.Round(body.TotalConsumption, 2);
                        ws.Cells[rowIndex, colIndex++].Value = Math.Round(body.RealConsumption, 2);
                        ws.Cells[rowIndex, colIndex++].Value = body.Diff;
                        ws.Cells[rowIndex, colIndex++].Value = body.Percentage + "%";
                    }

                    int colPatternIndex = 1;
                    int rowPatternIndex = 1;

                    int colColorIndex = 1;
                    int rowColorIndex = 1;
                    foreach (var body in consumtionDtos)
                    {
                        rowColorIndex++;
                        rowPatternIndex++;
                        colPatternIndex = 1;
                        colColorIndex = 1;
                        if (body.Percentage > 0)
                        {
                            ws.Cells[rowPatternIndex, colPatternIndex++].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            ws.Cells[rowPatternIndex, colPatternIndex++].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            ws.Cells[rowPatternIndex, colPatternIndex++].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            ws.Cells[rowPatternIndex, colPatternIndex++].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            ws.Cells[rowPatternIndex, colPatternIndex++].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            ws.Cells[rowPatternIndex, colPatternIndex++].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            ws.Cells[rowPatternIndex, colPatternIndex++].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            ws.Cells[rowPatternIndex, colPatternIndex++].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            ws.Cells[rowPatternIndex, colPatternIndex++].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            ws.Cells[rowPatternIndex, colPatternIndex++].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            ws.Cells[rowPatternIndex, colPatternIndex++].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            ws.Cells[rowPatternIndex, colPatternIndex++].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            ws.Cells[rowPatternIndex, colPatternIndex++].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            ws.Cells[rowColorIndex, colColorIndex++].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#FFFF66"));
                            ws.Cells[rowColorIndex, colColorIndex++].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#FFFF66"));
                            ws.Cells[rowColorIndex, colColorIndex++].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#FFFF66"));
                            ws.Cells[rowColorIndex, colColorIndex++].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#FFFF66"));
                            ws.Cells[rowColorIndex, colColorIndex++].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#FFFF66"));
                            ws.Cells[rowColorIndex, colColorIndex++].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#FFFF66"));
                            ws.Cells[rowColorIndex, colColorIndex++].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#FFFF66"));
                            ws.Cells[rowColorIndex, colColorIndex++].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#FFFF66"));
                            ws.Cells[rowColorIndex, colColorIndex++].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#FFFF66"));
                            ws.Cells[rowColorIndex, colColorIndex++].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#FFFF66"));
                            ws.Cells[rowColorIndex, colColorIndex++].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#FFFF66"));
                            ws.Cells[rowColorIndex, colColorIndex++].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#FFFF66"));
                            ws.Cells[rowColorIndex, colColorIndex++].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#FFFF66"));

                        }
                    }

                    //make the borders of cell F6 thick
                    ws.Cells[ws.Dimension.Address].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    ws.Cells[ws.Dimension.Address].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    ws.Cells[ws.Dimension.Address].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    ws.Cells[ws.Dimension.Address].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    foreach (var item in headers.Select((x, i) => new { Value = x, Index = i }))
                    {
                        var col = item.Index + 1;
                        ws.Column(col).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        ws.Column(col).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        if (col == 5 || col == 1)
                        {
                            ws.Column(col).AutoFit(30);
                        }
                        else
                        {
                            ws.Column(col).AutoFit();
                        }
                    }
                    //Lưu file lại
                    Byte[] bin = p.GetAsByteArray();
                    return bin;
                }
            }
            catch (Exception ex)
            {
                var mes = ex.Message;
                Console.Write(mes);
                return new Byte[] { };
            }
        }
        private Byte[] ExportExcel(ReportHeaderDto header, List<ReportBodyDto> bodyList, List<IngredientReportDto> ingredients)
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.Commercial;
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                var memoryStream = new MemoryStream();
                using (ExcelPackage p = new ExcelPackage(memoryStream))
                {
                    // đặt tên người tạo file
                    p.Workbook.Properties.Author = "Henry Pham";

                    // đặt tiêu đề cho file
                    p.Workbook.Properties.Title = "Report";
                    //Tạo một sheet để làm việc trên đó
                    p.Workbook.Worksheets.Add("Report");

                    // lấy sheet vừa add ra để thao tác
                    ExcelWorksheet ws = p.Workbook.Worksheets["Report"];

                    // đặt tên cho sheet
                    ws.Name = "Report";
                    // fontsize mặc định cho cả sheet
                    ws.Cells.Style.Font.Size = 11;
                    // font family mặc định cho cả sheet
                    ws.Cells.Style.Font.Name = "Calibri";



                    int ingredientRealRowIndex = 1;
                    int ingredientCBDRowIndex = 2;

                    int ingredientCBDColIndex = 8;
                    int ingredientRealColIndex = 8;

                    ws.Cells[ingredientRealRowIndex, ingredientRealColIndex++].Value = "REAL";
                    ws.Cells[ingredientCBDRowIndex, ingredientCBDColIndex++].Value = "CBD";

                    foreach (var ingredient in ingredients)
                    {
                        int cbdColumn = ingredientCBDColIndex++;
                        int realColumn = ingredientRealColIndex++;
                        ws.Cells[ingredientCBDRowIndex, cbdColumn].Value = ingredient.CBD;
                        ws.Cells[ingredientRealRowIndex, realColumn].Value = ingredient.Real;

                        ws.Cells[ingredientCBDRowIndex, cbdColumn].Style.TextRotation = 90;
                        ws.Cells[ingredientRealRowIndex, realColumn].Style.TextRotation = 90;
                        ws.Cells[ingredientCBDRowIndex, cbdColumn].AutoFitColumns(5);
                        ws.Cells[ingredientRealRowIndex, realColumn].AutoFitColumns(5);

                        ws.Cells[ingredientRealRowIndex, realColumn].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws.Cells[ingredientRealRowIndex, realColumn].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                        ws.Cells[ingredientCBDRowIndex, cbdColumn].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws.Cells[ingredientCBDRowIndex, cbdColumn].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    }

                    int headerRowIndex = 3;
                    int headerColIndex = 1;

                    int patternTypeColIndex = 1;
                    int backgroundColorColIndex = 1;

                    for (headerColIndex = 1; headerColIndex < 9; headerColIndex++)
                    {
                        ws.Cells[headerRowIndex, headerColIndex++].Value = header.Day;
                        ws.Cells[headerRowIndex, headerColIndex++].Value = header.Date;
                        ws.Cells[headerRowIndex, headerColIndex++].Value = header.ModelName;
                        ws.Cells[headerRowIndex, headerColIndex++].Value = header.ModelNo;
                        ws.Cells[headerRowIndex, headerColIndex++].Value = header.Quantity;
                        ws.Cells[headerRowIndex, headerColIndex++].Value = header.Line;
                        ws.Cells[headerRowIndex, headerColIndex++].Value = header.CBD;
                        ws.Cells[headerRowIndex, headerColIndex++].Value = header.Real;
                        // Style Header
                        ws.Cells[headerRowIndex, patternTypeColIndex++].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[headerRowIndex, patternTypeColIndex++].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[headerRowIndex, patternTypeColIndex++].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[headerRowIndex, patternTypeColIndex++].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[headerRowIndex, patternTypeColIndex++].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[headerRowIndex, patternTypeColIndex++].Style.Fill.PatternType = ExcelFillStyle.Solid;

                        ws.Cells[headerRowIndex, backgroundColorColIndex++].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#4f81bd"));
                        ws.Cells[headerRowIndex, backgroundColorColIndex++].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#4f81bd"));
                        ws.Cells[headerRowIndex, backgroundColorColIndex++].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#4f81bd"));
                        ws.Cells[headerRowIndex, backgroundColorColIndex++].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#8db5e2"));
                        ws.Cells[headerRowIndex, backgroundColorColIndex++].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#ffffff"));
                        ws.Cells[headerRowIndex, backgroundColorColIndex++].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#9bbb59"));
                    }


                    // end Style
                    int ingredientColIndex = 9;
                    foreach (var ingredient in header.Ingredients)
                    {
                        int col = ingredientColIndex++;
                        ws.Cells[headerRowIndex, col].Value = ingredient;
                        ws.Cells[headerRowIndex, col].Style.Fill.PatternType = ExcelFillStyle.Solid;

                        ws.Cells[headerRowIndex, col].Style.TextRotation = 90;
                        ws.Cells[headerRowIndex, col].Style.Font.Color.SetColor(Color.White);
                        ws.Cells[headerRowIndex, col].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#808080"));

                    }
                    int colIndex = 1;
                    int rowIndex = 3;
                    // với mỗi item trong danh sách sẽ ghi trên 1 dòng
                    foreach (var body in bodyList)
                    {
                        // bắt đầu ghi từ cột 1. Excel bắt đầu từ 1 không phải từ 0 #c0514d
                        colIndex = 1;

                        // rowIndex tương ứng từng dòng dữ liệu
                        rowIndex++;


                        //gán giá trị cho từng cell                      
                        ws.Cells[rowIndex, colIndex++].Value = body.Day;
                        ws.Cells[rowIndex, colIndex++].Value = body.Date.ToString("M/d");
                        ws.Cells[rowIndex, colIndex++].Value = body.ModelName;
                        ws.Cells[rowIndex, colIndex++].Value = body.ModelNo; ;
                        ws.Cells[rowIndex, colIndex++].Value = body.Quantity == 0 ? string.Empty : body.Quantity.ToString();
                        ws.Cells[rowIndex, colIndex++].Value = body.Line;

                        var cbds = ingredients.Select(x => x.CBD).ToArray();
                        var reals = ingredients.Select(x => x.Real).ToArray();

                        var cbdRowTotal = body.Ingredients.ToArray();
                        var realRowTotal = body.Ingredients.ToArray();
                        var value = body.Ingredients.Sum();
                        double CBD = 0, real = 0;

                        if (value > 0 && body.Quantity > 0)
                            CBD = Math.Round(SumProduct(cbdRowTotal, cbds) / body.Quantity, 3, MidpointRounding.AwayFromZero);
                        if (value > 0 && body.Quantity > 0)
                            real = Math.Round(SumProduct(realRowTotal, reals) / body.Quantity, 3, MidpointRounding.AwayFromZero);

                        ws.Cells[rowIndex, colIndex++].Value = CBD == 0 ? string.Empty : CBD.ToString();
                        ws.Cells[rowIndex, colIndex++].Value = real == 0 ? string.Empty : real.ToString();

                        foreach (var ingredient in body.Ingredients)
                        {
                            int col = colIndex++;
                            ws.Cells[rowIndex, col].Value = ingredient > 0 ? Math.Round(ingredient, 2, MidpointRounding.AwayFromZero).ToString() : string.Empty;
                            ws.Cells[rowIndex, col].Style.Font.Size = 8;
                            ws.Cells[rowIndex, col].Style.Font.Color.SetColor(Color.DarkRed);
                            ws.Cells[rowIndex, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            ws.Cells[rowIndex, col].AutoFitColumns(5);
                        }

                    }
                    int mergeFromColIndex = 1;
                    int mergeFromRowIndex = 4;
                    int mergeToRowIndex = 3;
                    foreach (var item in bodyList.GroupBy(x => x.Day))
                    {
                        mergeToRowIndex += item.Count();

                        ws.Cells[mergeFromRowIndex, 6, mergeToRowIndex, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[mergeFromRowIndex, 6, mergeToRowIndex, 8].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#9bbb59"));


                        ws.Cells[mergeFromRowIndex, 2, mergeFromRowIndex, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[mergeFromRowIndex, 2, mergeFromRowIndex, 8].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#c0514d"));

                        ws.Cells[mergeFromRowIndex, mergeFromColIndex, mergeToRowIndex, mergeFromColIndex].Merge = true;
                        ws.Cells[mergeFromRowIndex, mergeFromColIndex, mergeToRowIndex, mergeFromColIndex].Style.Font.Size = 36;
                        ws.Cells[mergeFromRowIndex, mergeFromColIndex, mergeToRowIndex, mergeFromColIndex].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        ws.Cells[mergeFromRowIndex, mergeFromColIndex, mergeToRowIndex, mergeFromColIndex].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        mergeFromRowIndex = mergeToRowIndex + 1;
                    }
                    //Make all text fit the cells
                    //ws.Cells[ws.Dimension.Address].AutoFitColumns();
                    ws.Cells[ws.Dimension.Address].Style.Font.Bold = true;

                    //make the borders of cell F6 thick
                    ws.Cells[ws.Dimension.Address].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    ws.Cells[ws.Dimension.Address].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    ws.Cells[ws.Dimension.Address].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    ws.Cells[ws.Dimension.Address].Style.Border.Left.Style = ExcelBorderStyle.Thin;

                    int dayCol = 1, dateCol = 2, modelNameCol = 3, modelNoCol = 4, qtyCol = 5, lineCol = 6, cbdCol = 7, realCol = 8;
                    ws.Column(dayCol).AutoFit(12);
                    ws.Column(dateCol).AutoFit(12);
                    ws.Column(modelNameCol).AutoFit(30);
                    ws.Column(modelNoCol).AutoFit(12);
                    ws.Column(qtyCol).AutoFit(8);
                    ws.Column(lineCol).AutoFit(8);
                    ws.Column(cbdCol).AutoFit(8);
                    ws.Column(realCol).AutoFit(10);

                    ws.Column(8).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ws.Column(8).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    ws.Column(dayCol).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Column(dateCol).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Column(modelNoCol).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Column(qtyCol).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Column(lineCol).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Column(cbdCol).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Column(realCol).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    ws.Row(1).Height = 40;
                    ws.Row(2).Height = 40;
                    //ws.Column(realCol).AutoFit(10);

                    ws.Cells[1, 1, 2, 7].Merge = true;
                    ws.Cells[1, 1, 2, 7].Style.Font.Size = 22;
                    ws.Cells[1, 1, 2, 7].Value = "Consumption-Cost Breakdown Report";
                    ws.Cells[1, 1, 2, 7].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ws.Cells[1, 1, 2, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    ws.Cells[3, 1, 3, 7].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ws.Cells[3, 1, 3, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    // freeze row and col
                    int rowCount = 5;
                    for (int i = 1; i < rowCount; i++)
                    {
                        ws.View.FreezePanes(i, dayCol);
                        ws.View.FreezePanes(i, dateCol);
                        ws.View.FreezePanes(i, modelNameCol);
                        ws.View.FreezePanes(i, modelNoCol);
                        ws.View.FreezePanes(i, qtyCol);
                        ws.View.FreezePanes(i, lineCol);
                        ws.View.FreezePanes(i, cbdCol);
                        ws.View.FreezePanes(i, realCol);
                        ws.View.FreezePanes(i, 9);
                    }

                    //Lưu file lại
                    Byte[] bin = p.GetAsByteArray();
                    return bin;
                }
            }
            catch (Exception ex)
            {
                var mes = ex.Message;
                Console.Write(mes);
                return new Byte[] { };
            }
        }
        private double CalculateIngredientByPositon(List<GlueIngredient> glueIngredients, IngredientReportDto ingredient, double quantity)
        {
            var count = glueIngredients.Count;
            switch (count)
            {
                case 1: return CalculateA(glueIngredients, ingredient, quantity);
                case 2: return CalculateAB(glueIngredients, ingredient, quantity);
                case 3: return CalculateABC(glueIngredients, ingredient, quantity);
                case 4: return CalculateABCD(glueIngredients, ingredient, quantity);
                case 5: return CalculateABCDE(glueIngredients, ingredient, quantity);
                default:
                    return 0;
            }
        }
        private double CalculateA(List<GlueIngredient> glueIngredients, IngredientReportDto ingredient, double quantity)
        {
            var valueA = quantity;
            return valueA;

        }
        private double CalculateAB(List<GlueIngredient> glueIngredients, IngredientReportDto ingredient, double quantity)
        {
            var glueIngredient = glueIngredients.FirstOrDefault(x => x.IngredientID == ingredient.ID);
            var position = glueIngredient.Position;

            double percentageB = 1 + ((double)FindPercentageByPosition(glueIngredients, "B") / 100);
            double valueA = quantity / percentageB;
            double valueB = quantity - valueA;

            switch (position)
            {
                case "A":
                    return valueA;
                case "B":
                    return valueB;
                default:
                    return 0;
            }
        }
        private int FindPercentageByPosition(List<GlueIngredient> glueIngredients, string position)
        {
            var glueIngredient = glueIngredients.FirstOrDefault(x => x.Position == position);

            return glueIngredient == null ? 0 : glueIngredient.Percentage;
        }
        private double CalculateABC(List<GlueIngredient> glueIngredients, IngredientReportDto ingredient, double quantity)
        {
            var glueIngredient = glueIngredients.FirstOrDefault(x => x.IngredientID == ingredient.ID);
            var position = glueIngredient.Position;
            var percentageB = 1 + ((double)FindPercentageByPosition(glueIngredients, "B") / 100);
            var percentageC = 1 + ((double)FindPercentageByPosition(glueIngredients, "C") / 100);

            var valueB = quantity - (quantity / percentageB);
            var valueC = quantity - valueB - (valueB / percentageB);

            var valueA = quantity - valueB - valueC;
            switch (position)
            {
                case "A": return valueA;
                case "B": return valueB;
                case "C": return valueC;
                default:
                    return 0;
            }

        }
        private double CalculateABCD(List<GlueIngredient> glueIngredients, IngredientReportDto ingredient, double quantity)
        {
            var glueIngredient = glueIngredients.FirstOrDefault(x => x.IngredientID == ingredient.ID);
            var position = glueIngredient.Position;
            var percentageB = 1 + ((double)FindPercentageByPosition(glueIngredients, "B") / 100);
            var percentageC = 1 + ((double)FindPercentageByPosition(glueIngredients, "C") / 100);
            var percentageD = 1 + ((double)FindPercentageByPosition(glueIngredients, "D") / 100);
            var valueB = quantity - (quantity / percentageB);
            var valueC = quantity - valueB - (valueB / percentageB);
            var valueD = quantity - valueB - valueC - (valueC / percentageB);
            var valueA = quantity - valueB - valueC - valueD;
            switch (position)
            {

                case "A": return valueA;
                case "B": return valueB;
                case "C": return valueC;
                case "D": return valueD;
                default:
                    return 0;
            }

        }
        private double CalculateABCDE(List<GlueIngredient> glueIngredients, IngredientReportDto ingredient, double quantity)
        {
            var glueIngredient = glueIngredients.FirstOrDefault(x => x.IngredientID == ingredient.ID);
            var position = glueIngredient.Position;
            var percentageB = 1 + ((double)FindPercentageByPosition(glueIngredients, "B") / 100);
            var percentageC = 1 + ((double)FindPercentageByPosition(glueIngredients, "C") / 100);
            var percentageD = 1 + ((double)FindPercentageByPosition(glueIngredients, "D") / 100);
            var percentageE = 1 + ((double)FindPercentageByPosition(glueIngredients, "E") / 100);
            var valueB = quantity - (quantity / percentageB);
            var valueC = quantity - valueB - (valueB / percentageB);
            var valueD = quantity - valueB - valueC - (valueC / percentageB);
            var valueE = quantity - valueB - valueC - -valueD - (valueC / percentageB);
            var valueA = quantity - valueB - valueC - valueD - valueE;
            switch (position)
            {
                case "A": return valueA;
                case "B": return valueB;
                case "C": return valueC;
                case "D": return valueD;
                case "E": return valueE;
                default:
                    return 0;
            }

        }
        public async Task<bool> EditQuantity(int id, int qty)
        {
            try
            {
                var item = _repoPlan.FindById(id);
                item.Quantity = qty;
                return await _repoPlan.SaveAll();
            }
            catch
            {
                return false;
            }
        }
        public async Task<object> OldSummary(int building)
        {

            var currentDate = DateTime.Now.Date;
            // var currentDate = new DateTime(2020,10,26);
            var item = _repoBuilding.FindById(building);
            var lineList = await _repoBuilding.FindAll().Where(x => x.ParentID == item.ID).ToListAsync();
            var plans = await _repoPlan.FindAll()
            .Include(x => x.BPFCEstablish)
            .ThenInclude(x => x.ModelName)
            .Where(x => x.DueDate.Date == currentDate && lineList.Select(x => x.ID).Contains(x.BuildingID))
            .Select(x => new
            {
                x.BPFCEstablish,
                x.BuildingID,
                x.CreatedDate,
                x.DueDate,
                x.BPFCEstablishID,
                x.WorkingHour,
                x.HourlyOutput
            })
            .ToListAsync();
            // Header
            var header = new List<HeaderForSummary> {
                  new HeaderForSummary
                {
                    field = "Supplier",
                },
                new HeaderForSummary
                {
                    field = "Chemical"
                }
            };
            var modelNameList = new List<string>();
            foreach (var line in lineList)
            {
                var itemHeader = new HeaderForSummary
                {
                    field = line.Name
                };
                var plan = plans.Where(x => x.BuildingID == line.ID).OrderByDescending(x => x.CreatedDate).FirstOrDefault();
                if (plan != null)
                {
                    modelNameList.Add(plan.BPFCEstablish.ModelName.Name);
                }
                else
                {
                    modelNameList.Add(string.Empty);
                }
                header.Add(itemHeader);
            }
            // end header

            // Data
            var model = (from glue in _repoGlue.FindAll().ToList()
                         join bpfc in _repoBPFC.FindAll().Include(x => x.ModelName).ToList() on glue.BPFCEstablishID equals bpfc.ID
                         join plan in plans on bpfc.ID equals plan.BPFCEstablishID
                         join bui in lineList on plan.BuildingID equals bui.ID
                         select new SummaryDto
                         {
                             GlueID = glue.ID,
                             BuildingID = bui.ID,
                             GlueName = glue.Name,
                             BuildingName = bui.Name,
                             Comsumption = glue.Consumption,
                             ModelNameID = bpfc.ModelNameID,
                             WorkingHour = plan.WorkingHour,
                             HourlyOutput = plan.HourlyOutput
                         }).ToList();
            var plannings = plans.Select(p => p.BPFCEstablishID).ToList();
            var glueList = await _repoGlue.FindAll()
                .Where(x => x.isShow == true)
                .Include(x => x.GlueIngredients)
                    .ThenInclude(x => x.Ingredient)
                    .ThenInclude(x => x.Supplier)
                .Include(x => x.BPFCEstablish)
                    .ThenInclude(x => x.ModelName)
                .Include(x => x.BPFCEstablish)
                    .ThenInclude(x => x.Plans)
                    .ThenInclude(x => x.Building)
                .Include(x => x.MixingInfos)
                .Where(x => plannings.Contains(x.BPFCEstablishID))
                .Select(x => new
                {
                    GlueIngredients = x.GlueIngredients.Select(a => new { a.GlueID, a.Ingredient, a.Position }),
                    x.Name,
                    x.ID,
                    x.BPFCEstablishID,
                    x.BPFCEstablish.Plans,
                    x.Consumption,
                    MixingInfos = x.MixingInfos.Select(a => new { a.GlueName, a.CreatedTime, a.ChemicalA, a.ChemicalB, a.ChemicalC, a.ChemicalD, a.ChemicalE, })
                }).ToListAsync();
            var glueDistinct = glueList.DistinctBy(x => x.Name);
            var buildingGlues = await _repoBuildingGlue.FindAll()
                                        .Where(x => lineList.Select(a => a.ID).Contains(x.BuildingID) && x.CreatedDate.Date == currentDate).ToListAsync();
            var rowParents = new List<RowParentDto>();
            foreach (var glue in glueDistinct)
            {
                var rowParent = new RowParentDto();
                var rowChild1 = new RowChildDto();
                var rowChild2 = new RowChildDto();
                var cellInfos = new List<CellInfoDto>();

                var itemData = new Dictionary<string, object>();
                var supplier = glue.GlueIngredients.FirstOrDefault(x => x.Position.Equals("A")) == null ? "#N/A" : glue.GlueIngredients.FirstOrDefault(x => x.Position.Equals("A")).Ingredient.Supplier.Name;
                // Static Left
                rowChild1.Supplier = new CellInfoDto() { Supplier = supplier };
                rowChild1.GlueName = new CellInfoDto() { GlueName = glue.Name };
                rowChild1.GlueID = glue.ID;

                rowChild2.Supplier = new CellInfoDto() { Supplier = supplier };
                rowChild2.GlueName = new CellInfoDto() { GlueName = glue.Name };
                rowChild2.GlueID = glue.ID;
                // End Static Left
                var listTotal = new List<double>();
                var listStandardTotal = new List<double>();
                var listWorkingHour = new List<double>();
                var listHourlyOuput = new List<double>();
                var delivered = buildingGlues
                                        .Where(x => x.GlueName.Equals(glue.Name))
                                        .OrderBy(x => x.CreatedDate)
                                        .Select(x => new DeliveredInfo
                                        {
                                            ID = x.ID,
                                            Qty = x.Qty,
                                            GlueName = x.GlueName,
                                            CreatedDate = x.CreatedDate,
                                            LineID = x.BuildingID
                                        })
                                        .ToList();
                var deliver = delivered.Select(x => x.Qty).ToList().ConvertAll<double>(Convert.ToDouble).Sum();
                var mixingInfos = await _repoMixingInfo.FindAll().Where(x => x.GlueName.Equals(glue.Name) && x.CreatedTime.Date == currentDate).ToListAsync();
                double realTotal = 0;
                foreach (var real in mixingInfos)
                {
                    realTotal += real.ChemicalA.ToDouble() + real.ChemicalB.ToDouble() + real.ChemicalC.ToDouble() + real.ChemicalD.ToDouble() + real.ChemicalE.ToDouble();
                }
                foreach (var line in lineList.OrderBy(x => x.Name))
                {
                    var dynamicCellInfoCenter = new CellInfoDto();
                    var sdtCon = model.FirstOrDefault(x => x.GlueName.Equals(glue.Name) && x.BuildingID == line.ID);
                    var listBuildingGlue = delivered.Where(x => x.GlueName.Equals(glue.Name) && x.LineID == line.ID && x.CreatedDate.Date == currentDate).OrderByDescending(x => x.CreatedDate).ToList();
                    double real = 0;
                    if (listBuildingGlue.FirstOrDefault() != null)
                    {
                        real = listBuildingGlue.FirstOrDefault().Qty.ToDouble();
                    }
                    double comsumption = 0;
                    if (sdtCon != null)
                    {
                        comsumption = glue.Consumption.ToDouble() * sdtCon.WorkingHour.ToDouble() * sdtCon.HourlyOutput.ToDouble();
                        itemData.Add(line.Name, Math.Round(comsumption / 1000, 3) + "kg");
                        listTotal.Add(glue.Consumption.ToDouble());
                        listWorkingHour.Add(sdtCon.WorkingHour.ToDouble());
                        listHourlyOuput.Add(sdtCon.HourlyOutput.ToDouble());
                        listStandardTotal.Add(comsumption / 1000);
                    }
                    else
                    {
                        itemData.Add(line.Name, 0);
                    }
                    double deliverdTotal = 0;
                    if (listBuildingGlue.Count > 0)
                    {
                        deliverdTotal = listBuildingGlue.Select(x => x.Qty).ToList().ConvertAll<double>(Convert.ToDouble).Sum();
                    }
                    var dynamicCellInfo = new CellInfoDto
                    {
                        GlueName = glue.Name,
                        line = line.Name,
                        lineID = line.ID,
                        value = real >= 1 && real < 10 ? Math.Round(real, 2) : real >= 10 ? Math.Round(real, 1) : Math.Round(real, 3),
                        count = listBuildingGlue.Count,
                        maxReal = realTotal,
                        delivered = Math.Round(deliver, 3),
                        deliveredTotal = deliverdTotal >= 1 && deliverdTotal < 10 ? Math.Round(deliverdTotal, 2) : deliverdTotal >= 10 ? Math.Round(deliverdTotal, 1) : Math.Round(deliverdTotal, 3),
                        deliveredInfos = listBuildingGlue.Select(x => new DeliveredInfo
                        {
                            ID = x.ID,
                            LineID = x.LineID,
                            Qty = (x.Qty.ToDouble() >= 1 && x.Qty.ToDouble() < 10 ? Math.Round(x.Qty.ToDouble(), 2) : x.Qty.ToDouble() >= 10 ? Math.Round(x.Qty.ToDouble(), 1) : Math.Round(x.Qty.ToDouble(), 3)).ToSafetyString(),
                            GlueName = x.GlueName,
                            CreatedDate = x.CreatedDate
                        }).ToList(),
                        consumption = comsumption / 1000
                    };
                    cellInfos.Add(dynamicCellInfo);

                }
                rowChild1.CellsCenter = cellInfos;
                rowChild2.CellsCenter = cellInfos;
                // Static Right
                var actual = $"{Math.Round(deliver, 3)}kg / {Math.Round(realTotal, 3)}kg";
                var count = glue.MixingInfos.Where(x => x.CreatedTime.Date == currentDate).Count();
                rowChild1.Actual = new CellInfoDto() { real = actual };
                rowChild1.Count = new CellInfoDto() { count = count };
                rowChild2.Actual = new CellInfoDto() { real = actual };
                rowChild2.Count = new CellInfoDto() { count = count };
                // End Static Right
                rowParent.Row1 = rowChild1;
                rowParent.Row2 = rowChild2;
                rowParents.Add(rowParent);

            }
            var infoList = new List<HeaderForSummary>() {
                  new HeaderForSummary
                {
                    field = "Real"
                },
                new HeaderForSummary
                {
                    field = "Count"
                }};

            header.AddRange(infoList);
            // End Data
            return new { header, rowParents, modelNameList };

        }

        public async Task<List<ConsumtionDto>> ConsumptionByLineCase1(ReportParams reportParams)
        {
            var res = await ConsumptionReportByBuilding(reportParams);
            return res.OrderByDescending(x => x.Percentage).ToList();
        }

        public async Task<List<ConsumtionDto>> ConsumptionByLineCase2(ReportParams reportParams)
        {
            var res = await ConsumptionReportByBuilding(reportParams);

            return res.OrderByDescending(x => x.DueDate).OrderBy(x => x.DueDate).ThenBy(x => x.Line).ThenBy(x => x.ID).ThenByDescending(x => x.Percentage).ToList();
        }
        private async Task<List<ConsumtionDto>> ConsumptionReportByBuilding(ReportParams reportParams)
        {
            var startDate = reportParams.StartDate.Date;
            var endDate = reportParams.EndDate.Date;
            var buildingID = reportParams.BuildingID;
            var lines = new List<int>();
            if (buildingID == 0)
            {
                lines = await _repoBuilding.FindAll(x => x.Level == LINE_LEVEL).Select(x => x.ID).ToListAsync();
            }
            else
            {
                lines = await _repoBuilding.FindAll(x => x.ParentID == buildingID).Select(x => x.ID).ToListAsync();
            }
            var buildingGlueModel = await _repoBuildingGlue.FindAll(x => x.CreatedDate.Date >= startDate && x.CreatedDate.Date <= endDate && lines.Contains(x.BuildingID)).Include(x => x.MixingInfo).ToListAsync();
            var model = await _repoPlan.FindAll()
                 .Include(x => x.BPFCEstablish)
                 .ThenInclude(x => x.Glues)
                  .Include(x => x.BPFCEstablish)
                 .ThenInclude(x => x.Plans)
                 .ThenInclude(x => x.Building)
                 .Include(x => x.BPFCEstablish)
                 .ThenInclude(x => x.ModelName)
                 .ThenInclude(x => x.ModelNos)
                 .ThenInclude(x => x.ArticleNos)
                 .ThenInclude(x => x.ArtProcesses)
                 .ThenInclude(x => x.Process)
                 .Select(x => new
                 {
                     x.BPFCEstablishID,
                     x.Quantity,
                     x.DueDate.Date,
                     x.BuildingID,
                     Line = x.Building.Name,
                     ModelName = x.BPFCEstablish.ModelName.Name,
                     ModelNo = x.BPFCEstablish.ModelNo.Name,
                     ArticleNo = x.BPFCEstablish.ArticleNo.Name,
                     Process = x.BPFCEstablish.ArtProcess.Process.Name,
                     Plans = x.BPFCEstablish.Plans,
                     Glues = x.BPFCEstablish.Glues.ToList()
                 }).Where(x => x.Plans.Any(x => lines.Contains(x.BuildingID)) && x.Date >= startDate && x.Date <= endDate)
                 .ToListAsync();
            var list = new List<ConsumtionDto>();
            foreach (var item in model)
            {
                foreach (var glue in item.Glues.Where(x => x.isShow))
                {
                    var std = glue.Consumption.ToFloat();
                    var buildingGlue = buildingGlueModel.FirstOrDefault(x => x.GlueName.Equals(glue.Name) && x.CreatedDate.Date == item.Date && item.BuildingID == x.BuildingID);
                    var totalConsumption = buildingGlue == null ? 0 : buildingGlue.Qty.ToFloat();
                    var realConsumption = totalConsumption > 0 && item.Quantity > 0 ? Math.Round(totalConsumption * 1000 / item.Quantity, 2).ToFloat() : 0;
                    var diff = std > 0 && realConsumption > 0 ? Math.Round(realConsumption - std, 2).ToFloat() : 0;
                    var percentage = std > 0 ? Math.Round((diff / std) * 100).ToFloat() : 0;
                    list.Add(new ConsumtionDto
                    {
                        ModelName = item.ModelName,
                        ModelNo = item.ModelNo,
                        ArticleNo = item.ArticleNo,
                        Process = item.Process,
                        Line = item.Line,
                        Glue = glue.Name,
                        Std = std,
                        Qty = item.Quantity,
                        TotalConsumption = totalConsumption,
                        RealConsumption = realConsumption,
                        Diff = diff,
                        ID = item.BPFCEstablishID,
                        Percentage = percentage,
                        DueDate = item.Date,
                        MixingDate = buildingGlue == null || buildingGlue.MixingInfo == null ? DateTime.MinValue : buildingGlue.MixingInfo.CreatedTime
                    });
                }

            }
            return list.ToList();
        }
        public async Task<byte[]> ReportConsumptionCase2(ReportParams reportParams)
        {
            var res = await ConsumptionReportByBuilding(reportParams);
            return ExportExcelConsumptionCase2(res);
        }
        public async Task<byte[]> ReportConsumptionCase1(ReportParams reportParams)
        {
            var res = await ConsumptionReportByBuilding(reportParams);
            return ExportExcelConsumptionCase1(res);
        }

        public async Task<object> TodolistUndone()
        {
            var currentTime = DateTime.Now;
            var currentDate = DateTime.Now.Date;
            var startLunchTime = new TimeSpan(12, 30, 0);
            var endLunchTime = new TimeSpan(13, 30, 0);

            var model = from b in _repoBPFC.GetAll()
                        join p in _repoPlan.GetAll()
                                            .Include(x => x.Building)
                                            .Where(x => x.DueDate == currentDate)
                                            .Select(x => new
                                            {
                                                x.HourlyOutput,
                                                x.WorkingHour,
                                                x.ID,
                                                x.BPFCEstablishID,
                                                Building = x.Building.Name
                                            })
                        on b.ID equals p.BPFCEstablishID
                        join g in _repoGlue.FindAll(x => x.isShow)
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
                            Line = p.Building,
                        };
            var test = await model.ToListAsync();
            var groupByGlueName = test.GroupBy(x => x.GlueName);
            var result = new List<TodolistDto>();
            foreach (var glue in groupByGlueName)
            {

                foreach (var item in glue)
                {

                    foreach (var mixingInfo in item.MixingInfos)
                    {
                        var itemTodolist = new TodolistDto();
                        double standardConsumption = 0;
                        var supplier = string.Empty;
                        var checmicalA = item.Ingredients.ToList().FirstOrDefault(x => x.Position == "A");
                        int prepareTime = 0;
                        if (checmicalA != null)
                        {
                            supplier = checmicalA.Supplier;
                            prepareTime = checmicalA.PrepareTime;
                        }
                        itemTodolist.Supplier = supplier;
                        itemTodolist.ID = item.GlueID;
                        itemTodolist.MixingInfoTodolistDtos = item.MixingInfos;
                        itemTodolist.Lines = glue.Select(x => x.Line).ToList();
                        itemTodolist.Glue = glue.Key;
                        itemTodolist.DeliveredActual = "-";
                        itemTodolist.Status = mixingInfo.Status;
                        itemTodolist.EstimatedFinishTime = mixingInfo.EstimatedFinishTime;
                        itemTodolist.EstimatedStartTime = mixingInfo.EstimatedStartTime;
                        itemTodolist.EstimatedTime = currentDate.AddHours(7).AddMinutes(30) - TimeSpan.FromMinutes(prepareTime);
                        foreach (var line in glue)
                        {
                            var kgPair = line.Consumption.ToDouble() / 1000;
                            standardConsumption += kgPair * (double)line.HourlyOutput * checmicalA.ReplacementFrequency;
                        }

                        itemTodolist.StandardConsumption = Math.Round(standardConsumption, 2);
                        result.Add(itemTodolist);
                    }
                }
            }
            return result;
        }
        public async Task<object> Todolist2(int buildingID)
        {
            if (buildingID == 0)
            {
                return new List<TodolistDto>();
            }
            var building = await _repoBuilding.FindAll(x => x.ID == buildingID).Include(x => x.LunchTime).FirstOrDefaultAsync();
            if (building == null)
            {
                return new List<TodolistDto>();
            }
            if (building.LunchTime == null)
            {
                return new List<TodolistDto>();
            }
            var lines = await _repoBuilding.FindAll(x => x.ParentID == buildingID).Select(x => x.ID).ToListAsync();
            var startLunchTimeBuilding = building.LunchTime.StartTime;
            var endLunchTimeBuilding = building.LunchTime.EndTime;
            var currentTime = DateTime.Now;
            var currentDate = DateTime.Now.Date;
            var startLunchTime = currentDate.Add(new TimeSpan(startLunchTimeBuilding.Hour, startLunchTimeBuilding.Minute, 0));
            var endLunchTime = currentDate.Add(new TimeSpan(endLunchTimeBuilding.Hour, endLunchTimeBuilding.Minute, 0));
            var model = from b in _repoBPFC.GetAll()
                        join p in _repoPlan.GetAll()
                                            .Include(x => x.Building)
                                            .Where(x => x.DueDate.Date == currentDate && lines.Contains(x.BuildingID))
                                            .Select(x => new
                                            {
                                                x.HourlyOutput,
                                                x.WorkingHour,
                                                x.ID,
                                                x.BPFCEstablishID,
                                                Building = x.Building.Name
                                            })
                        on b.ID equals p.BPFCEstablishID
                        join g in _repoGlue.FindAll(x => x.isShow)
                                           .Include(x => x.BPFCEstablish)
                                               .ThenInclude(x => x.ModelName)
                                               .ThenInclude(x => x.ModelNos)
                                               .ThenInclude(x => x.ArticleNos)
                                               .ThenInclude(x => x.ArtProcesses)
                                               .ThenInclude(x => x.Process)
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
                                                BPFCEstablish = x.BPFCEstablish.ModelName.Name + x.BPFCEstablish.ModelNo.Name + x.BPFCEstablish.ArticleNo.Name + x.BPFCEstablish.ArtProcess.Process.Name,
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
                            g.BPFCEstablish,
                            Line = p.Building,
                        };
            var test = await model.ToListAsync();
            var groupByGlueName = test.GroupBy(x => x.GlueName).Distinct().ToList();
            var result = new List<TodolistDto>();
            foreach (var glue in groupByGlueName)
            {
                var itemTodolist = new TodolistDto();
                itemTodolist.Lines = glue.Select(x => x.Line).ToList();
                itemTodolist.BPFCs = glue.Select(x => x.BPFCEstablish).ToList();
                itemTodolist.Glue = glue.Key;
                double standardConsumption = 0;

                foreach (var item in glue)
                {
                    itemTodolist.GlueID = item.GlueID;
                    var supplier = string.Empty;
                    var checmicalA = item.Ingredients.ToList().FirstOrDefault(x => x.Position == "A");
                    int prepareTime = 0;
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
                    int cycle = 8 / checmicalA.ReplacementFrequency;
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

                result.Add(itemTodolist);
                standardConsumption = 0;

            }
            var result2 = new List<TodolistDto>();

            foreach (var item in result)
            {
                foreach (var estimatedTime in item.EstimatedTimes)
                {
                    var mixing = await FindMixingInfo(item.Glue, estimatedTime);

                    var itemTodolist = new TodolistDto();
                    itemTodolist.Supplier = item.Supplier;
                    itemTodolist.GlueID = item.GlueID;
                    itemTodolist.ID = mixing == null ? 0 : mixing.ID;
                    itemTodolist.EstimatedStartTime = mixing == null ? DateTime.MinValue : mixing.StartTime;
                    itemTodolist.EstimatedFinishTime = mixing == null ? DateTime.MinValue : mixing.EndTime;
                    itemTodolist.MixingInfoTodolistDtos = item.MixingInfoTodolistDtos;
                    itemTodolist.Lines = item.Lines;
                    itemTodolist.Glue = item.Glue;
                    itemTodolist.StandardConsumption = item.StandardConsumption;
                    itemTodolist.DeliveredActual = await FindDeliver(item.Glue, estimatedTime);
                    itemTodolist.Status = mixing == null ? false : mixing.Status;
                    itemTodolist.EstimatedTime = estimatedTime;
                    result2.Add(itemTodolist);
                }
            }
            return result2.OrderBy(x => x.Glue).Where(x => x.EstimatedTime <= currentTime && x.Status == false);
        }
        public async Task<object> Todolist2ByDone(int buildingID)
        {
            //var currentTime = DateTime.Now;
            //var currentDate = DateTime.Now.Date;
            if (buildingID == 0)
            {
                return new List<TodolistDto>();
            }
            var building = await _repoBuilding.FindAll(x => x.ID == buildingID).Include(x => x.LunchTime).FirstOrDefaultAsync();
            var lines = await _repoBuilding.FindAll(x => x.ParentID == buildingID).Select(x => x.ID).ToListAsync();
            if (building == null)
            {
                return new List<TodolistDto>();
            }

            var startLunchTimeBuilding = building.LunchTime.StartTime;
            var endLunchTimeBuilding = building.LunchTime.EndTime;
            var currentTime = DateTime.Now;
            var currentDate = DateTime.Now.Date;
            var startLunchTime = currentDate.Add(new TimeSpan(startLunchTimeBuilding.Hour, startLunchTimeBuilding.Minute, 0));
            var endLunchTime = currentDate.Add(new TimeSpan(endLunchTimeBuilding.Hour, endLunchTimeBuilding.Minute, 0));
            var model = from b in _repoBPFC.GetAll()
                        join p in _repoPlan.GetAll()
                                            .Include(x => x.Building)
                                            .Where(x => x.DueDate.Date == currentDate && lines.Contains(x.BuildingID))
                                            .Select(x => new
                                            {
                                                x.HourlyOutput,
                                                x.WorkingHour,
                                                x.ID,
                                                x.BPFCEstablishID,
                                                Building = x.Building.Name
                                            })
                        on b.ID equals p.BPFCEstablishID
                        join g in _repoGlue.FindAll(x => x.isShow)
                                           .Include(x => x.BPFCEstablish)
                                               .ThenInclude(x => x.ModelName)
                                               .ThenInclude(x => x.ModelNos)
                                               .ThenInclude(x => x.ArticleNos)
                                               .ThenInclude(x => x.ArtProcesses)
                                               .ThenInclude(x => x.Process)
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
                                                BPFCEstablish = x.BPFCEstablish.ModelName.Name + x.BPFCEstablish.ModelNo.Name + x.BPFCEstablish.ArticleNo.Name + x.BPFCEstablish.ArtProcess.Process.Name,
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
                            g.BPFCEstablish,
                            Line = p.Building,
                        };
            var test = await model.ToListAsync();
            var groupByGlueName = test.GroupBy(x => x.GlueName).Distinct().ToList();
            var result = new List<TodolistDto>();
            foreach (var glue in groupByGlueName)
            {
                var itemTodolist = new TodolistDto();
                itemTodolist.Lines = glue.Select(x => x.Line).ToList();
                itemTodolist.BPFCs = glue.Select(x => x.BPFCEstablish).ToList();
                itemTodolist.Glue = glue.Key;
                double standardConsumption = 0;

                foreach (var item in glue)
                {
                    itemTodolist.GlueID = item.GlueID;
                    var supplier = string.Empty;
                    var checmicalA = item.Ingredients.ToList().FirstOrDefault(x => x.Position == "A");
                    int prepareTime = 0;
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
                    int cycle = 8 / checmicalA.ReplacementFrequency;
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

                result.Add(itemTodolist);
                standardConsumption = 0;

            }
            var result2 = new List<TodolistDto>();

            foreach (var item in result)
            {
                foreach (var estimatedTime in item.EstimatedTimes)
                {
                    var mixing = await FindMixingInfo(item.Glue, estimatedTime);

                    var itemTodolist = new TodolistDto();
                    itemTodolist.Supplier = item.Supplier;
                    itemTodolist.GlueID = item.GlueID;
                    itemTodolist.ID = mixing == null ? 0 : mixing.ID;
                    itemTodolist.EstimatedStartTime = mixing == null ? DateTime.MinValue : mixing.StartTime;
                    itemTodolist.EstimatedFinishTime = mixing == null ? DateTime.MinValue : mixing.EndTime;
                    itemTodolist.MixingInfoTodolistDtos = item.MixingInfoTodolistDtos;
                    itemTodolist.Lines = item.Lines;
                    itemTodolist.Glue = item.Glue;
                    itemTodolist.StandardConsumption = item.StandardConsumption;
                    itemTodolist.DeliveredActual = await FindDeliver(item.Glue, estimatedTime);
                    itemTodolist.Status = mixing == null ? false : mixing.Status;
                    itemTodolist.EstimatedTime = estimatedTime;
                    result2.Add(itemTodolist);
                }
            }
            return result2.OrderBy(x => x.Glue).Where(x => x.EstimatedTime <= currentTime && x.Status == true);
        }
        double CalculateGlueTotal(MixingInfo mixingInfo)
        {
            return mixingInfo.ChemicalA.ToDouble() + mixingInfo.ChemicalB.ToDouble() + mixingInfo.ChemicalC.ToDouble() + mixingInfo.ChemicalD.ToDouble() + mixingInfo.ChemicalE.ToDouble();
        }

        public async Task<object> Dispatch(DispatchParams todolistDto)
        {
            // var currentDate = DateTime.Now.Date;
            var currentDate = DateTime.Now.Date;
            var lines = await _repoBuilding.FindAll(x => todolistDto.Lines.Contains(x.Name)).Select(x => x.ID).ToListAsync();
            var dispatches = await _repoDispatch.FindAll(x => x.CreatedTime.Date == DateTime.Now.Date && lines.Contains(x.LineID)).ToListAsync();
            var mixingInfo = await _repoMixingInfo
            .FindAll(x => x.EstimatedTime == todolistDto.EstimatedTime && x.GlueName.Equals(todolistDto.Glue))
            .Include(x => x.Glue)
                .ThenInclude(x => x.GlueIngredients)
                .ThenInclude(x => x.Ingredient)
            .FirstOrDefaultAsync();
            if (mixingInfo == null) return new List<DispatchTodolistDto>();
            var glue = mixingInfo.Glue;
            var list = new List<DispatchTodolistDto>();
            var plans = _repoPlan.FindAll(x => x.DueDate.Date == currentDate)
                .Include(x => x.Building)
                .Include(x => x.BPFCEstablish)
                    .ThenInclude(x => x.Glues)
                    .ThenInclude(x => x.GlueIngredients)
                    .ThenInclude(x => x.Ingredient)
                .Where(x => x.BPFCEstablishID == glue.BPFCEstablishID || todolistDto.Lines.Contains(x.Building.Name))
                .ToList();
            if (plans.Count == 0) return new List<DispatchTodolistDto>();

            foreach (var plan in plans)
            {
                var item = new DispatchTodolistDto();
                item.Line = plan.Building.Name;
                item.LineID = plan.Building.ID;
                double standardConsumption = 0;
                foreach (var glueItem in plan.BPFCEstablish.Glues.Where(x => x.isShow && x.Name.Equals(mixingInfo.GlueName)))
                {
                    item.MixingInfoID = mixingInfo.ID;
                    item.EstimatedTime = mixingInfo.EstimatedTime;
                    item.Glue = glue.Name;
                    var checmicalA = glueItem.GlueIngredients.ToList().FirstOrDefault(x => x.Position == "A");
                    var replacementFrequency = checmicalA != null ? checmicalA.Ingredient.ReplacementFrequency : 0;
                    var kgPair = checmicalA != null ? glueItem.Consumption.ToDouble() / 1000 : 0;
                    standardConsumption = kgPair * (double)plan.HourlyOutput * replacementFrequency; // 5kg
                }
                if (standardConsumption > 3)
                {
                    while (standardConsumption > 0)
                    {
                        standardConsumption = standardConsumption - 3; // 2
                        if (standardConsumption > 3)
                        {
                            item.StandardAmount = 3;
                            list.Add(item);

                        }
                        else if (standardConsumption > 0 && standardConsumption < 3)
                        {
                            item.StandardAmount = Math.Round(standardConsumption, 2);

                            list.Add(item);

                        }
                        else
                        {
                            item.StandardAmount = 3;
                            list.Add(item);
                        }
                    }
                }
                else
                {
                    item.StandardAmount = Math.Round(standardConsumption, 2);

                    list.Add(item);

                }
            }
            var result = (from a in list
                          from b in dispatches.Where(x => a.LineID == x.LineID && x.MixingInfoID == a.MixingInfoID)
                         .DefaultIfEmpty()
                          select new DispatchTodolistDto
                          {
                              ID = b == null ? 0 : b.ID,
                              Glue = a.Glue,
                              Line = a.Line,
                              LineID = a.LineID,
                              MixingInfoID = a.MixingInfoID,
                              Real = b == null ? 0 : b.Amount,
                              StandardAmount = a.StandardAmount
                          });
            return result.OrderBy(x => x.Line).ToList();
        }
        public async Task<MixingInfo> FindMixingInfo(string glue, DateTime estimatedTime)
        {
            var mixingInfo = await _repoMixingInfo
            .FindAll(x => x.EstimatedTime == estimatedTime && x.GlueName == glue).FirstOrDefaultAsync();
            return mixingInfo;
        }
        public async Task<string> FindDeliver(string glue, DateTime estimatedTime)
        {
            var mixingInfo = await FindMixingInfo(glue, estimatedTime);
            if (mixingInfo == null) return "0kg/0kg";
            var buildingGlue = await _repoDispatch.FindAll(x => x.MixingInfoID == mixingInfo.ID).Select(x => x.Amount).ToListAsync();
            var deliver = buildingGlue.Sum();
            return $"{Math.Round(deliver / 1000, 2)}kg/{Math.Round(CalculateGlueTotal(mixingInfo), 2)}";
        }
        public async Task<MixingInfo> Print(DispatchParams todolistDto)
        {
            var mixingInfo = await _repoMixingInfo
            .FindAll(x => x.EstimatedTime == todolistDto.EstimatedTime && x.GlueName.Equals(todolistDto.Glue))
            .Include(x => x.Glue)
                .ThenInclude(x => x.GlueIngredients)
                .ThenInclude(x => x.Ingredient)
            .FirstOrDefaultAsync();
            return mixingInfo == null ? new MixingInfo() : mixingInfo;
        }

        public async Task<object> Finish(int mixingÌnoID)
        {
            var mixingInfo = await _repoMixingInfo
            .FindAll(x => x.ID == mixingÌnoID).FirstOrDefaultAsync();
            if (mixingInfo == null) return false;
            mixingInfo.Status = true;
            return await _repoMixingInfo.SaveAll();
        }

        public async Task<List<TodolistDto>> CheckTodolistAllBuilding()
        {
            var currentTime = DateTime.Now;
            var currentDate = DateTime.Now.Date;
            var buildingModel = await _repoBuilding.FindAll()
                .Include(x => x.LunchTime).ToListAsync();

            var buildings = buildingModel.Where(x => x.Level == 2 && x.LunchTime != null).ToList();
            var lunchTimes = buildings.Select(x => x.LunchTime).ToList();
            var buildingIDList = buildings.Select(x => x.ID).ToList();
            var lines = new List<int>();
            lines = buildingModel.Where(x => x.ParentID != null && buildingIDList.Contains(x.ParentID.Value)).Select(x => x.ID).ToList();

            var model = from b in _repoBPFC.GetAll()
                        join p in _repoPlan.GetAll()
                                            .Include(x => x.Building)
                                            .Where(x => x.DueDate.Date == currentDate && lines.Contains(x.BuildingID))
                                            .Select(x => new
                                            {
                                                x.HourlyOutput,
                                                x.WorkingHour,
                                                x.ID,
                                                x.BPFCEstablishID,
                                                Building = x.Building.Name,
                                                BuildingID = x.Building.ID
                                            })
                        on b.ID equals p.BPFCEstablishID
                        join g in _repoGlue.FindAll(x => x.isShow)
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
                            Line = p.Building,
                            LineID = p.BuildingID,
                        };


            var mixingInfoModel = await _repoMixingInfo
           .FindAll(x => x.CreatedTime.Date == currentDate).ToListAsync();
            var dispatchModel = await _repoDispatch
          .FindAll(x => x.CreatedTime.Date == currentDate).ToListAsync();

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
                        var supplier = string.Empty;
                        var checmicalA = item.Ingredients.ToList().FirstOrDefault(x => x.Position == "A");
                        int prepareTime = 0;
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
                        int cycle = 8 / checmicalA.ReplacementFrequency;
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
                        var buildingGlue = dispatchModel.Where(x => x.MixingInfoID == mixing.ID).Select(x => x.Amount).ToList();
                        var deliver = buildingGlue.Sum();
                        deliverAndActual = $"{Math.Round(deliver / 1000, 2)}kg/{Math.Round(CalculateGlueTotal(mixing), 2)}";

                        var itemTodolist = new TodolistDto();
                        itemTodolist.Supplier = item.Supplier;
                        itemTodolist.GlueID = item.GlueID;
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
    }
}