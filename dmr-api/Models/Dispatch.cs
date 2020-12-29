using System;
using System.ComponentModel.DataAnnotations.Schema;
using DMR_API.Models;

namespace dmr_api.Models
{
    public class Dispatch
    {
        public int ID { get; set; }
        public int StationID { get; set; }
        public double Amount { get; set; }
        public double StandardAmount { get; set; }
        public string Unit { get; set; }
        public bool IsDelete { get; set; }
        public DateTime? StartDispatchingTime { get; set; }
        public DateTime? FinishDispatchingTime { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime? DeliveryTime { get; set; }
        public DateTime? DeleteTime { get; set; }
        public int DeleteBy { get; set; }
        public int CreateBy { get; set; }
        public int LineID { get; set; }
        [ForeignKey("LineID")]
        public Building Building { get; set; }
        public int MixingInfoID { get; set; }
        [ForeignKey("MixingInfoID")]
        public MixingInfo MixingInfo { get; set; }
       

    }
}