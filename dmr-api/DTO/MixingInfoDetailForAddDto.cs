using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API.DTO
{
    public class MixingInfoDetailForAddDto
    {
        public int ID { get; set; }
        public double Amount { get; set; }
        public string Position { get; set; }
        public string Batch { get; set; }
        public int IngredientID { get; set; }
        public int MixingInfoID { get; set; }
    }

}
