using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API.Models
{
    public class LunchTime
    {
        public int ID { get; set; }
        public int BuildingID { get; set; }
        [ForeignKey("BuildingID")]
        public Building Building { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
