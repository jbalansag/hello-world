using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RealtyWeb.Models
{
    public class SearchFilter
    {
        public int? Page { get; set; }
        public int? PropertyId { get; set; }
        public int? PropIndicatorId { get; set; }
        public string SearchTerm { get; set; }
        public string MinPrices { get; set; }
        public string MaxPrices { get; set; }
        public string Beds { get; set; }
        public string Baths { get; set; }
        public int? OrderMethod { get; set; }
        public int? LocationId { get; set; }
        public int? LocationIdDefault { get; set; }
        public string LocationName { get; set; }
        public int? UserEntityId { get; set; }
    }
}