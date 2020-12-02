using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API.Models
{
    public class Building
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
        public int? ParentID { get; set; }
        public ICollection<Plan> Plans { get; set; }
        public LunchTime LunchTime { get; set; }
        public ICollection<Setting> Settings { get; set; }
    }
}
