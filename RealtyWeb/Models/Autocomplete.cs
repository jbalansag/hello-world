using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace RealtyWeb.Models
{
    public class Autocomplete
    {
        public int Id { get; set;}

        public string Name { get; set; }
    }

    public class ValuePairResult
    {
        public int id { get; set;}

        public string Name { get; set; }
    }

    public class Select2Result
    {
        public int total_count { get; set; }
        public List<ValuePairResult> items { get; set; }
    }
}