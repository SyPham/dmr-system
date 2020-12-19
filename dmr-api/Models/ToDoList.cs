using DMR_API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API.Models
{
    public class ToDoList
    {
        public int ID { get; set; }
        public int PlanID { get; set; } // Primary key xac dinh mixingInfo
        public Plan Plan { get; set; }
        public GlueName GlueLibrary { get; set; }
        public int MixingInfoID { get; set; }
        public int GlueID { get; set; } // Primary key xac dinh mixingInfo
        public int GlueNameID { get; set; }
        public int BuildingID { get; set; } // Primary key xac dinh mixingInfo

        public int LineID { get; set; }
        public int BPFCID { get; set; }
        public string LineName { get; set; }
        public string GlueName { get; set; }
        public string Supplier { get; set; }
        public bool Status { get; set; }
        public bool AbnormalStatus { get; set; }

        public DateTime? StartMixingTime { get; set; }
        public DateTime? FinishMixingTime { get; set; }

        public DateTime? StartStirTime { get; set; }
        public DateTime? FinishStirTime { get; set; }

        public DateTime? StartDispatchingTime { get; set; }
        public DateTime? FinishDispatchingTime { get; set; }

        public DateTime? PrintTime { get; set; }

        public double StandardConsumption { get; set; } // Luon Co gia tri
        public double MixedConsumption { get; set; }
        public double DeliveredConsumption { get; set; }

        public DateTime EstimatedStartTime { get; set; } // Primary key xac dinh mixingInfo
        public DateTime EstimatedFinishTime { get; set; }

        public bool IsDelete { get; set; }
        public DateTime DeleteTime { get; set; }
        public int DeleteBy { get; set; }

    }
}
