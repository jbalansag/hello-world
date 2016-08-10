using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RealtyWeb.Models
{
    class PointOfIntrestComparer : IEqualityComparer<PointOfInterest>
    {
        // PointOfInterest are equal if their PoiId is equal.
        public bool Equals(PointOfInterest x, PointOfInterest y)
        {
            //Check whether the compared objects reference the same data.
            if (Object.ReferenceEquals(x, y)) return true;

            //Check whether any of the compared objects is null.
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            //Check whether the PointOfInterest' properties are equal.
            return x.PoiId == y.PoiId;
        }

        // If Equals() returns true for a pair of objects 
        // then GetHashCode() must return the same value for these objects.
        public int GetHashCode(PointOfInterest poi)
        {
            //Check whether the object is null
            if (Object.ReferenceEquals(poi, null)) return 0;

            //Get hash code for the PoiId field if it is not null.
            int hashPoiId = poi.PoiId.GetHashCode();

            //Calculate the hash code for the product.
            return hashPoiId;
        }
    }
}