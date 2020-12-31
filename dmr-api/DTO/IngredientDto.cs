﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dmr_api.Models;
using DMR_API.Helpers;

namespace DMR_API.DTO
{
    public class IngredientDto
    {
        public IngredientDto()
        {
            this.Name = this.Name.ToSafetyString().Trim();
        }

        public int ID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string CreatedDate { get; set; }
        public string Supplier { get; set; }
        public string Position { get; set; }
        public string MaterialNO { get; set; }

  
        public DateTime ManufacturingDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        public bool Status { get; set; }
        public bool isShow { get; set; }

        public int CreateBy { get; set; }
        public int SupplierID { get; set; }
        public int? GlueTypeID { get; set; }
        public int ModifiedBy { get; set; }

        public double ReplacementFrequency { get; set; }
        public double ExpiredTime { get; set; }
        public double PrepareTime { get; set; }
        public double Allow { get; set; }
        public double Percentage { get; set; }
        public double VOC { get; set; }
        public double Unit { get; set; }
        public double Real { get; set; }
        public double CBD { get; set; }
        public double DaysToExpiration { get; set; }
        public GlueType GlueType { get; set; }
    }
}
