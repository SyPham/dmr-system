using System;
using System.ComponentModel.DataAnnotations.Schema;
using DMR_API.Models;

namespace dmr_api.Models
{
    public class Dispatch
    {
        public int ID { get; set; }
        public double Amount { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime EstimatedTime { get; set; }
        public int LineID { get; set; }
        [ForeignKey("LineID")]
        public Building Building { get; set; }
        public int MixingInfoID { get; set; }
        [ForeignKey("MixingInfoID")]
        public MixingInfo MixingInfo { get; set; }
        
    }
}