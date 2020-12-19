using System;
using System.ComponentModel.DataAnnotations.Schema;
using DMR_API.Models;

namespace dmr_api.Models
{
    public class Dispatch
    {
        public int ID { get; set; }
        public double Amount { get; set; }
        public double StandardAmount { get; set; }
        public string Unit { get; set; }
        public DateTime CreatedTime { get; set; }
        public int LineID { get; set; }
        [ForeignKey("LineID")]
        public Building Building { get; set; }
        public int MixingInfoID { get; set; }
        [ForeignKey("MixingInfoID")]
        public MixingInfo MixingInfo { get; set; }
        public DateTime StartDispatchingTime { get; set; }
        public DateTime FinishDispatchingTime { get; set; }

    }
}