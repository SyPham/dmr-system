using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API.DTO
{
    public class ToDoListDto
    {
        public int ID { get; set; }
        public int PlanID { get; set; }
        public int MixingInfoID { get; set; }
        public int GlueID { get; set; }
        public int BuildingID { get; set; }
        public int LineID { get; set; }
        public int BPFCID { get; set; }
        public int GlueNameID { get; set; }
        public string LineName { get; set; }
        public string GlueName { get; set; }
        public string Supplier { get; set; }
        public bool Status { get; set; }
        public bool AbnormalStatus { get; set; }
        public bool IsDelete { get; set; }

        public List<string> LineNames { get; set; }
        public DateTime? StartMixingTime { get; set; }
        public DateTime? FinishMixingTime { get; set; }

        public DateTime? StartStirTime { get; set; }
        public DateTime? FinishStirTime { get; set; }

        public DateTime? StartDispatchingTime { get; set; }
        public DateTime? FinishDispatchingTime { get; set; }

        public DateTime? PrintTime { get; set; }

        public double StandardConsumption { get; set; }
        public double MixedConsumption { get; set; }
        public double DeliveredConsumption { get; set; }

        public DateTime EstimatedStartTime { get; set; }
        public DateTime EstimatedFinishTime { get; set; }
    }
    public class ToDoListForReturnDto
    {
        public ToDoListForReturnDto(List<ToDoListDto> data, int doneTotal = 0, int todoTotal = 0, int delayTotal = 0, int total = 0)
        {
            Data = data;
            DoneTotal = doneTotal;
            Total = total;
            TodoTotal = todoTotal;
            DelayTotal = delayTotal;
            var val = Math.Round(((double)doneTotal / total) * 100, 0);
            PercentageOfDone =  Double.IsNaN(val) ? 0 : val;
        }
        public List<ToDoListDto> Data { get; set; }
        public double Total { get; set; }
        public double DoneTotal { get; set; }
        public double TodoTotal { get; set; }
        public double DelayTotal { get; set; }
        public double PercentageOfDone { get; set; }
    }
}
