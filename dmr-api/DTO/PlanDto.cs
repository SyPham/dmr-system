using DMR_API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API.DTO
{
    public class PlanDto
    {
        public int ID { get; set; }
        public int BuildingID { get; set; }
        public string BuildingName { get; set; }
        public string ModelName { get; set; }
        public string ModelNoName { get; set; }
        public string BPFCName { get; set; }

        public string ArticleName { get; set; }
        public string ProcessName { get; set; }
        public List<string> Glues { get; set; }
        public int BPFCEstablishID { get; set; }
        public int HourlyOutput { get; set; }
        public int WorkingHour { get; set; }
        public int ModelNameID { get; set; }
        public int ModelNoID { get; set; }
        public int Quantity { get; set; }
        public int ArticleNoID { get; set; }
        public int ArtProcessID { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime CreatedDate { get; set; }
    }
    public class ConsumtionDto
    {
        public int ID { get; set; }
        public string ModelName { get; set; }
        public string ModelNo { get; set; }
        public string ArticleNo { get; set; }
        public string Process { get; set; }
        public string Glue { get; set; }
        public float Std { get; set; }
        public int Qty { get; set; }
        public string Line { get; set; }
        public int LineID { get; set; }
        public float TotalConsumption { get; set; }
        public float RealConsumption { get; set; }
        public float Diff { get; set; }
        public float Percentage { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime MixingDate { get; set; }

    }
    public class TodolistDto
    {
        public int ID { get; set; }
        public int GlueID { get; set; }
        public int MixingInfoID { get; set; }
        public int? BuildingID { get; set; }
        public double StandardConsumption { get; set; }
        public List<string> Lines { get; set; }
        public List<string> BPFCs { get; set; }
        public string Glue { get; set; }
        public List<MixingInfoTodolistDto> MixingInfoTodolistDtos { get; set; }
        public string Supplier { get; set; }
        public string DeliveredActual { get; set; }
        public bool Status { get; set; }
        public DateTime EstimatedTime { get; set; }
        public List<DateTime> EstimatedTimes { get; set; }
        public DateTime EstimatedStartTime { get; set; }
        public DateTime EstimatedFinishTime { get; set; }

    }
    public class DispatchParams {
        public string Glue { get; set; }
        public int ID { get; set; }
        public DateTime EstimatedTime { get; set; }
        public List<string> Lines { get; set; }

    }
    public class MixingInfoTodolistDto
    {
    public int ID { get; set; }
    public string Glue { get; set; }
    public bool Status { get; set; }
    public DateTime EstimatedStartTime { get; set; }
    public DateTime EstimatedFinishTime { get; set; }

}
    public class GlueTodolistDto
    {
        public int ID { get; set; }
        public Glue Glue { get; set; }
        public int HourlyOutput { get; set; }
        public int WorkingHour { get; set; }
        public Building Building { get; set; }
        public string DeliverActual { get; set; }
        public bool Status { get; set; }
        public DateTime EstimatedTime { get; set; }
        public DateTime EstimatedStartTime { get; set; }
        public DateTime EstimatedFinishTime { get; set; }

    }
    public class DispatchTodolistDto
    {
        public int ID { get; set; }
        public double StandardAmount { get; set; }
        public string Line { get; set; }
        public string Glue { get; set; }
        public int LineID { get; set; }
        public int MixingInfoID { get; set; }
        public DateTime EstimatedTime { get; set; }
        public double Real { get; set; }

    }
}
