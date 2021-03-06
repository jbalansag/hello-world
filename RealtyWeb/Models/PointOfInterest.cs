//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace RealtyWeb.Models
{
    public partial class PointOfInterest
    {
        public int PoiId { get; set; }
        public long PropertyEntryId { get; set; }
        public string Name { get; set; }
        public decimal Distance { get; set; }
        public int PoiTypeId { get; set; }
        public int CreatedBy { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public int UpdatedBy { get; set; }
        public System.DateTime UpdatedDate { get; set; }
        public byte[] TimeStamp { get; set; }
    
        public virtual PointOfInterestType PointOfInterestType { get; set; }
        public virtual PropertyEntry PropertyEntry { get; set; }
    }
    
}
