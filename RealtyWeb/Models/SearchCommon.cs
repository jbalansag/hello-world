using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RealtyWeb.Models
{
    public static class SearchCommon
    {
        public static List<Autocomplete> GetLocations(string query)
        {
            List<Autocomplete> locations = new List<Autocomplete>();

            if (!string.IsNullOrEmpty(query))
            {
                try
                {
                    using (var db = new RealtyWebContext())
                    {
                        locations = db.Locations.Where(x => x.LocationName.Contains(query.TrimEnd())).Take(10).Select(x => new Autocomplete { Id = x.LocationId, Name = x.LocationName }).ToList();
                    }
                }
                catch (System.Data.Entity.Core.EntityCommandCompilationException eceex)
                {
                    if (eceex.InnerException != null)
                    {
                        throw eceex.InnerException;
                    }
                    throw;
                }
                catch
                {
                    throw;
                }
            }
            return locations;
        }

        public static Select2Result CustomGetLocations(string query, int? page)
        {
            Select2Result data = new Select2Result();

            if (!string.IsNullOrEmpty(query))
            {
                try
                {
                    using (var db = new RealtyWebContext())
                    {
                        var locations = db.Locations.Where(x => x.LocationName.Contains(query.TrimEnd())).Select(x => new ValuePairResult { id = x.LocationId, Name = x.LocationName }).ToList();

                        int itemsPerPage = 30;
                        int fromCount = ((page ?? 1) - 1) * itemsPerPage;
                        int to = fromCount + itemsPerPage;

                        data.items = new List<ValuePairResult>();

                        for (int i = fromCount; i < to; i++)
                        {
                            if (i < locations.Count)
                            {
                                data.items.Add(locations[i]);
                            }
                            else
                            {
                                break;
                            }
                        }

                        data.total_count = locations.Count;
                    }
                }
                catch (System.Data.Entity.Core.EntityCommandCompilationException eceex)
                {
                    if (eceex.InnerException != null)
                    {
                        throw eceex.InnerException;
                    }
                    throw;
                }
                catch
                {
                    throw;
                }
            }
            return data;
        }
    }
}