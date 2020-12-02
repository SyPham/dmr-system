using DMR_API.DTO;
using DMR_API.Models;
using AutoMapper;
using System;

namespace DMR_API.Helpers.AutoMapper
{
    public class DtoToEfMappingProfile : Profile
    {
        private string TotalReal(MixingInfo real)
        {
            double realTotal = 0;
            realTotal = real.ChemicalA.ToDouble() + real.ChemicalB.ToDouble() + real.ChemicalC.ToDouble() + real.ChemicalD.ToDouble() + real.ChemicalE.ToDouble();
            return realTotal.ToString();
        }
        public DtoToEfMappingProfile()
        {
            CreateMap<UserForDetailDto, User>();
            CreateMap<GlueDto, Glue>();
            CreateMap<GlueCreateDto, Glue>();
            CreateMap<GlueCreateDto1, Glue>();
            CreateMap<IngredientDto, Ingredient>()
            .ForMember(d => d.VOC, o => o.MapFrom(x => x.VOC.ToDouble().ToSafetyString()))
            .ForMember(d => d.Unit, o => o.MapFrom(x => x.Unit.ToDouble().ToSafetyString()))
            .ForMember(d => d.Supplier, o => o.Ignore());
            CreateMap<IngredientForImportExcelDto, Ingredient>();
            CreateMap<IngredientDto1, Ingredient>()
            .ForMember(d => d.VOC, o => o.MapFrom(x => x.VOC.ToDouble().ToSafetyString()))
            .ForMember(d => d.Unit, o => o.MapFrom(x => x.Unit.ToDouble().ToSafetyString()));

            CreateMap<LineDto, Line>();
            CreateMap<ModelNameDto, ModelName>();
            CreateMap<PlanDto, Plan>();

            CreateMap<MapModelDto, MapModel>();
            CreateMap<ModelNoDto, ModelNo>();
            CreateMap<UserDetail, UserDetailDto>();
            CreateMap<SuppilerDto, Supplier>();
            CreateMap<PartNameDto, PartName>();
            CreateMap<PartName2Dto, PartName2>();
            CreateMap<MaterialNameDto, MaterialName>();
            CreateMap<ArticleNoDto, ArticleNo>();
            CreateMap<BuildingDto, Building>();
            CreateMap<BuildingUserDto, BuildingUser>();
            CreateMap<CommentDto, Comment>();
            CreateMap<BPFCEstablishDto, BPFCEstablish>();
            CreateMap<ArtProcessDto, ArtProcess>();
            CreateMap<ProcessDto, Process>();
            CreateMap<KindDto, Kind>();
            CreateMap<PartDto, Part>();
            CreateMap<RoleDto, Role>();
            CreateMap<MaterialDto, Material>();
            CreateMap<MixingInfoDto, MixingInfo>();
            CreateMap<MixingInfo, MixingInfoForCreateDto>();
            CreateMap<BuildingGlue, BuildingGlueForCreateDto>().ForMember(d => d.Qty, o => o.MapFrom(a => a.Qty.ToDouble().ToSafetyString()));
            CreateMap<IngredientInfo, IngredientInfoDto>();
            CreateMap<IngredientInfoReport, IngredientInfoReportDto>();
            CreateMap<Setting, SettingDTO>();
            CreateMap<Stir, StirDTO>();
            CreateMap<Plan, PlanForCloneDto>();
            CreateMap<ScaleMachine, ScaleMachineDto>();
            //CreateMap<AuditTypeDto, MES_Audit_Type_M>();
        }
    }
}