using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using RealtyWeb.Models;
using Microsoft.AspNet.Identity;
using System.Data.Entity.Validation;
using System.Transactions;
using PagedList;
using System.Web.Script.Services;
using System.Web.Services;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;
using RealtyWeb.Common;

namespace RealtyWeb.Controllers
{
    public class HomeController : Controller
    {
        private RealtyWebContext db = new RealtyWebContext();
        private const int RecordsPerPage = 5;

        public HomeController()
        {
            ViewBag.RecordsPerPage = RecordsPerPage;
        }

        public ActionResult Index()
        {
            int loginId = User.Identity.GetUserId<int>();
            var loginInfo = db.AppUsers.Find(loginId);

            var constants = db.Constants;
            var propertyInfo = db.Properties.First();
            var defaultPropType = propertyInfo.PropertyId;
            var defaultPropIndInfo = db.PropertyIndicators.First();
            var defaultPropInd = defaultPropIndInfo.PropIndicatorId;

            var propertyEntries = db.PropertyEntries.Where(x => x.Status == "A").OrderByDescending(x => x.UpdatedDate).Take(60).Include(p => p.Property).Include(p => p.AppUser).Include(x => x.PropertyIndicator);
            var indicators = new SelectList(db.PropertyIndicators, "PropIndicatorId", "PropIndicatorName", defaultPropInd.ToString());
            var propertyTypes = new SelectList(db.Properties, "PropertyId", "PropertyName");
            var minPrices = new SelectList(constants.Where(x => x.ConstantCd == "MINPRICE"), "ConstantValue", "ConstantValue");
            var maxPrices = new SelectList(constants.Where(x => x.ConstantCd == "MAXPRICE"), "ConstantValue", "ConstantValue");
            var baths = new SelectList(constants.Where(x => x.ConstantCd == "BATHROOMS"), "ConstantValue", "ConstantValue");
            var beds = new SelectList(constants.Where(x => x.ConstantCd == "BEDROOMS"), "ConstantValue", "ConstantValue");
            var orderByItems = constants.Where(x => x.ConstantCd == "ORDERBYITEMS").ToList();

            ViewBag.MinPrices = minPrices;
            ViewBag.MaxPrices = maxPrices;
            ViewBag.Baths = baths;
            ViewBag.Beds = beds;
            ViewBag.PropertyId = propertyTypes;
            ViewBag.PropIndicatorId = indicators;
            ViewBag.DeaultPropIndId = defaultPropIndInfo.PropIndicatorId;
            ViewBag.DefaultPropIndName = defaultPropIndInfo.PropIndicatorName;
            ViewBag.PropertyTypeName = propertyInfo.PropertyName;
            ViewBag.PropertyIndName = defaultPropIndInfo.PropIndicatorName;
            ViewBag.CurrentMenu = RealtyWeb.Common.Menu.Home;

            ViewBag.OrderByItems = orderByItems;
            ViewBag.OrderByName = "Default";
            ViewBag.OrderById = null;
            ViewBag.ViewCompanyInfo = "Yes";
            ViewBag.ViewRightPane = true;
            Session["LoginInfo"] = loginInfo;
            ViewBag.PropertyList = propertyEntries.ToList();

            var filterModel = new SearchFilter();
            filterModel.PropIndicatorId = defaultPropInd;

            return View(filterModel);
        }

        public ActionResult Main()
        {
            int loginId = User.Identity.GetUserId<int>();
            var loginInfo = db.AppUsers.Find(loginId);

            var constants = db.Constants;
            var defaultPropType = db.Properties.First().PropertyId;
            var defaultPropIndInfo = db.PropertyIndicators.First();
            var defaultPropInd = defaultPropIndInfo.PropIndicatorId;

            var propertyEntries = db.PropertyEntries.Take(60).Include(p => p.Property).Include(p => p.AppUser).Include(x => x.PropertyIndicator);
            var indicators = new SelectList(db.PropertyIndicators, "PropIndicatorId", "PropIndicatorName", defaultPropInd.ToString());
            var propertyTypes = new SelectList(db.Properties, "PropertyId", "PropertyName");
            var minPrices = new SelectList(constants.Where(x => x.ConstantCd == "MINPRICE"), "ConstantValue", "ConstantValue");
            var maxPrices = new SelectList(constants.Where(x => x.ConstantCd == "MAXPRICE"), "ConstantValue", "ConstantValue");
            var baths = new SelectList(constants.Where(x => x.ConstantCd == "BATHROOMS"), "ConstantValue", "ConstantValue");
            var beds = new SelectList(constants.Where(x => x.ConstantCd == "BEDROOMS"), "ConstantValue", "ConstantValue");
            var orderByItems = constants.Where(x => x.ConstantCd == "ORDERBYITEMS").ToList();

            ViewBag.MinPrices = minPrices;
            ViewBag.MaxPrices = maxPrices;
            ViewBag.Baths = baths;
            ViewBag.Beds = beds;
            ViewBag.PropertyId = propertyTypes;
            ViewBag.PropIndicatorId = indicators;
            ViewBag.DeaultPropIndId = defaultPropIndInfo.PropIndicatorId;
            ViewBag.DefaultPropIndName = defaultPropIndInfo.PropIndicatorName;

            ViewBag.OrderByItems = orderByItems;
            ViewBag.OrderByName = "Default";
            ViewBag.OrderById = null;
            ViewBag.ViewCompanyInfo = "Yes";
            ViewBag.ViewRightPane = true;
            Session["LoginInfo"] = loginInfo;
            ViewBag.PropertyList = propertyEntries.OrderByDescending(x => x.UpdatedDate).ToList();
            ViewBag.CurrentMenu = RealtyWeb.Common.Menu.Main;

            var filterModel = new SearchFilter();
            filterModel.PropIndicatorId = defaultPropInd;

            return View(filterModel);
        }

        public ActionResult Details(long? id)
        {
            var userId = User.Identity.GetUserId<int>();
            var userInfo = db.AppUsers.Find(userId);

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PropertyEntry propertyentry = db.PropertyEntries.Find(id);
            if (propertyentry == null)
            {
                return HttpNotFound();
            }

            ViewData["PointOfInterestGroup"] = db.PointOfInterests.Where(x => x.PropertyEntryId == propertyentry.PropertyEntryId).GroupBy(x => x.PointOfInterestType.PoiTypeName);
            ViewBag.LoginInfo = userInfo ?? new AppUser();

            return View(propertyentry);
        }

        [Route("{propertyDetails}")]
        public ActionResult PropertyInfo(string propertyDetails)
        {
            string propertyId = propertyDetails.Substring(propertyDetails.LastIndexOf('-') + 1);

            long id = 0;

            if (!long.TryParse(propertyId, out id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var userId = User.Identity.GetUserId<int>();
            var userInfo = db.AppUsers.Find(userId);

            PropertyEntry propertyentry = db.PropertyEntries.Find(id);
            if (propertyentry == null)
            {
                return HttpNotFound();
            }

            var propIndInfo = db.PropertyIndicators.First();
            var constants = db.Constants;

            var propertyTypes = new SelectList(db.Properties, "PropertyId", "PropertyName");
            var indicators = new SelectList(db.PropertyIndicators, "PropIndicatorId", "PropIndicatorName");
            var minPrices = new SelectList(constants.Where(x => x.ConstantCd == "MINPRICE"), "ConstantValue", "ConstantValue");
            var maxPrices = new SelectList(constants.Where(x => x.ConstantCd == "MAXPRICE"), "ConstantValue", "ConstantValue");
            var baths = new SelectList(constants.Where(x => x.ConstantCd == "BATHROOMS"), "ConstantValue", "ConstantValue");
            var beds = new SelectList(constants.Where(x => x.ConstantCd == "BEDROOMS"), "ConstantValue", "ConstantValue");
            var orderByItems = constants.Where(x => x.ConstantCd == "ORDERBYITEMS").ToList();

            ViewBag.MinPrices = minPrices;
            ViewBag.MaxPrices = maxPrices;
            ViewBag.Baths = baths;
            ViewBag.Beds = beds;
            ViewBag.PropertyId = propertyTypes;
            ViewBag.PropIndicatorId = indicators;
            ViewBag.OrderByItems = orderByItems;
            ViewBag.OrderByName = "Default";
            ViewBag.OrderById = null;
            ViewBag.ViewCompanyInfo = "Yes";
            ViewBag.DefaultPropIndName = propIndInfo.PropIndicatorName;
            ViewBag.PointOfInterestGroup = db.PointOfInterests.Where(x => x.PropertyEntryId == propertyentry.PropertyEntryId).GroupBy(x => x.PointOfInterestType.PoiTypeName);
            ViewBag.LoginInfo = userInfo;

            if (propertyentry.PropertyIndicator != null)
            {
                if (propertyentry.PropertyIndicator.PropIndicatorName == EnumString.GetStringValue(PropIndicator.ForSale))
                {
                    ViewBag.CurrentMenu = RealtyWeb.Common.Menu.ForSale;
                }
                else if (propertyentry.PropertyIndicator.PropIndicatorName == EnumString.GetStringValue(PropIndicator.ForRent))
                {
                    ViewBag.CurrentMenu = RealtyWeb.Common.Menu.ForRent;
                }
                else if (propertyentry.PropertyIndicator.PropIndicatorName == EnumString.GetStringValue(PropIndicator.Development))
                {
                    ViewBag.CurrentMenu = RealtyWeb.Common.Menu.Development;
                }
                else if (propertyentry.PropertyIndicator.PropIndicatorName == EnumString.GetStringValue(PropIndicator.Foreclosed))
                {
                    ViewBag.CurrentMenu = RealtyWeb.Common.Menu.Foreclosed;
                }
            }


            var filterModel = new SearchFilter();
            filterModel.LocationName = "Philippines";

            ViewBag.PropertyInfo = propertyentry;

            return View(filterModel);
        }

        [Route("property/{title}/{filter?}")]
        public ActionResult PropertySearch(SearchFilter filter, string title)
        {
            int userId = User.Identity.GetUserId<int>();
            var orderByInfo = filter.OrderMethod != null ? db.Constants.Find(filter.OrderMethod) : null;
            var propIndInfo = filter.PropIndicatorId == null ? db.PropertyIndicators.First() : db.PropertyIndicators.Find(filter.PropIndicatorId);
            var locationInfo = db.Locations.Find((filter.LocationId ?? 0) > 0 ? filter.LocationId : filter.LocationIdDefault);
            var propTypeInfo = filter.PropertyId == null ? null : db.Properties.Find(filter.PropertyId);
            var locationName = locationInfo != null ? locationInfo.LocationName.TrimEnd() : string.Empty;

            decimal propertyEntryId = 0;
            string searchQuery = string.IsNullOrEmpty(filter.SearchTerm) ? string.Empty : filter.SearchTerm;
            decimal minPrice = string.IsNullOrEmpty(filter.MinPrices) ? 0 : Convert.ToDecimal(filter.MinPrices);
            decimal maxPrice = string.IsNullOrEmpty(filter.MaxPrices) ? 0 : !filter.MaxPrices.Contains('+') ? Convert.ToDecimal(filter.MaxPrices) : Convert.ToDecimal(filter.MaxPrices.Substring(0, filter.MaxPrices.Length - 2));
            decimal bed = string.IsNullOrEmpty(filter.Beds) ? 0 : Convert.ToDecimal(filter.Beds[0].ToString());
            decimal bath = string.IsNullOrEmpty(filter.Baths) ? 0 : Convert.ToDecimal(filter.Baths[0].ToString());
            bool maxAboveLimit = string.IsNullOrEmpty(filter.MaxPrices) ? false : filter.MaxPrices.Contains('+');

            if (decimal.TryParse(filter.SearchTerm, out propertyEntryId)) { }
            var propertyEntries = db.PropertyEntries.Where(x => x.Status == "A" && (propertyEntryId > 0 ? x.PropertyEntryId == propertyEntryId : //Filter by PropertyEntryId first if any. . .
                                                                ((filter.PropertyId != null ? x.PropertyId == filter.PropertyId : true) &&
                                                                (filter.PropIndicatorId != null ? x.PropIndicatorId == filter.PropIndicatorId : true) &&
                                                                (minPrice != 0 ? x.Price >= minPrice : true) &&
                                                                (maxPrice == 0 || maxAboveLimit ? true : x.Price <= maxPrice) &&
                                                                (bed != 0 ? x.Bedrooms >= bed : true) &&
                                                                (bath != 0 ? x.Baths >= bath : true) &&
                                                                (x.Address1.Contains(searchQuery) || searchQuery.Contains(x.Address1) ||
                                                                 x.Address2.Contains(searchQuery) || searchQuery.Contains(x.Address2) ||
                                                                 x.Address3.Contains(searchQuery) || searchQuery.Contains(x.Address3) ||
                                                                 x.Address4.Contains(searchQuery) || searchQuery.Contains(x.Address4) ||
                                                                 x.PropertyTitle.Contains(searchQuery) || searchQuery.Contains(x.PropertyTitle) ||
                                                                 x.PropertyDetails.Contains(searchQuery) || searchQuery.Contains(x.PropertyDetails)) &&
                                                                 (x.Address1.Contains(locationName) || locationName.Contains(x.Address1) ||
                                                                  x.Address2.Contains(locationName) || locationName.Contains(x.Address2) ||
                                                                  x.Address3.Contains(locationName) || locationName.Contains(x.Address3) ||
                                                                  x.Address4.Contains(locationName) || locationName.Contains(x.Address4)
                                                                 )))).Include(p => p.Property).Include(p => p.AppUser).Include(x => x.UserFavorite);
            var constants = db.Constants;
            var mins = new SelectList(constants.Where(x => x.ConstantCd == "MINPRICE"), "ConstantValue", "ConstantValue");
            var maxs = new SelectList(constants.Where(x => x.ConstantCd == "MAXPRICE"), "ConstantValue", "ConstantValue");
            var bathRooms = new SelectList(constants.Where(x => x.ConstantCd == "BATHROOMS"), "ConstantValue", "ConstantValue");
            var bedRooms = new SelectList(constants.Where(x => x.ConstantCd == "BEDROOMS"), "ConstantValue", "ConstantValue");
            var propIndicator = new SelectList(db.PropertyIndicators, "PropIndicatorId", "PropIndicatorName");
            var propertyId = new SelectList(db.Properties, "PropertyId", "PropertyName");
            var orderByItems = constants.Where(x => x.ConstantCd == "ORDERBYITEMS").ToList();

            ViewBag.MinPrices = mins;
            ViewBag.MaxPrices = maxs;
            ViewBag.Baths = bathRooms;
            ViewBag.Beds = bedRooms;
            ViewBag.PropertyId = propertyId;
            ViewBag.PropIndicatorId = propIndicator;
            ViewBag.OrderByItems = orderByItems;
            ViewBag.DeaultPropIndId = propIndInfo.PropIndicatorId;
            ViewBag.DefaultPropIndName = propIndInfo.PropIndicatorName;
            ViewBag.OrderByName = orderByInfo != null ? orderByInfo.ConstantValue : "Default";
            ViewBag.UserLoginId = userId;
            ViewBag.ViewCompanyInfo = "Yes";
            ViewBag.ViewRightPane = false;
            ViewBag.PropertyTypeName = propTypeInfo == null ? "Property" : propTypeInfo.PropertyName;
            ViewBag.PropertyIndName = propIndInfo.PropIndicatorName;

            filter.OrderMethod = orderByInfo != null ? orderByInfo.ConstantId : 0;
            filter.LocationName = string.IsNullOrEmpty(locationName) ? "Philippines" : locationName;
            filter.LocationIdDefault = (filter.LocationId ?? 0) > 0 ? filter.LocationId : filter.LocationIdDefault;

            IEnumerable<PropertyEntry> propertyList = null;

            if (orderByInfo != null)
            {
                switch (orderByInfo.ConstantId)
                {
                    case 50: propertyList = propertyEntries.OrderBy(x => x.Price); break;
                    case 51: propertyList = propertyEntries.OrderByDescending(x => x.Price); break;
                    case 52: propertyList = propertyEntries.OrderByDescending(x => x.UpdatedDate); break;
                    case 53: propertyList = propertyEntries.OrderBy(x => x.Property.PropertyName); break;
                    case 54: propertyList = propertyEntries.OrderBy(x => x.FloorArea); break;
                }
            }
            else
            {
                propertyList = propertyEntries.OrderByDescending(x => x.UpdatedDate);
            }

            if (propIndInfo != null)
            {
                if (propIndInfo.PropIndicatorName == EnumString.GetStringValue(PropIndicator.ForSale))
                {
                    ViewBag.CurrentMenu = RealtyWeb.Common.Menu.ForSale;
                }
                else if (propIndInfo.PropIndicatorName == EnumString.GetStringValue(PropIndicator.ForRent))
                {
                    ViewBag.CurrentMenu = RealtyWeb.Common.Menu.ForRent;
                }
                else if (propIndInfo.PropIndicatorName == EnumString.GetStringValue(PropIndicator.Development))
                {
                    ViewBag.CurrentMenu = RealtyWeb.Common.Menu.Development;
                }
                else if (propIndInfo.PropIndicatorName == EnumString.GetStringValue(PropIndicator.Foreclosed))
                {
                    ViewBag.CurrentMenu = RealtyWeb.Common.Menu.Foreclosed;
                }
            }
            
            var pageNum = 0;
            ViewBag.IsEndOfRecords = false;
            LoadAllPropertiesToSession(propertyList);
            ViewBag.PropertyList = GetRecordsForPage(pageNum);

            return View("SearchLayout", filter);
        }

        [Authorize]
        public ActionResult Favorites()
        {
            int userId = User.Identity.GetUserId<int>();
            var loginInfo = db.AppUsers.Find(userId);
            var propIndInfo = db.PropertyIndicators.First();
            var propTypeInfo = db.Properties.First();
            var locationName = string.Empty;

            var propertyEntries = db.PropertyEntries.Where(x => x.Status == "A").Include(p => p.Property).Include(p => p.AppUser).Include(p => p.UserFavorite).Where(x => x.UserFavorite.Where(y => y.UserId == loginInfo.Id).Count() > 0);
            var constants = db.Constants;
            var mins = new SelectList(constants.Where(x => x.ConstantCd == "MINPRICE"), "ConstantValue", "ConstantValue");
            var maxs = new SelectList(constants.Where(x => x.ConstantCd == "MAXPRICE"), "ConstantValue", "ConstantValue");
            var bathRooms = new SelectList(constants.Where(x => x.ConstantCd == "BATHROOMS"), "ConstantValue", "ConstantValue");
            var bedRooms = new SelectList(constants.Where(x => x.ConstantCd == "BEDROOMS"), "ConstantValue", "ConstantValue");
            var propIndicator = new SelectList(db.PropertyIndicators, "PropIndicatorId", "PropIndicatorName");
            var propertyId = new SelectList(db.Properties, "PropertyId", "PropertyName");
            var orderByItems = constants.Where(x => x.ConstantCd == "ORDERBYITEMS").ToList();

            ViewBag.MinPrices = mins;
            ViewBag.MaxPrices = maxs;
            ViewBag.Baths = bathRooms;
            ViewBag.Beds = bedRooms;
            ViewBag.PropertyId = propertyId;
            ViewBag.PropIndicatorId = propIndicator;
            ViewBag.OrderByItems = orderByItems;
            ViewBag.DeaultPropIndId = propIndInfo.PropIndicatorId;
            ViewBag.DefaultPropIndName = propIndInfo.PropIndicatorName;
            ViewBag.OrderByName = "Default";
            ViewBag.UserLoginId = userId;
            ViewBag.ViewCompanyInfo = "Yes";
            ViewBag.ViewRightPane = false;
            ViewBag.PropertyTypeName = propTypeInfo == null ? "Property" : propTypeInfo.PropertyName;
            ViewBag.PropertyIndName = propIndInfo.PropIndicatorName;

            var filter = new SearchFilter();

            filter.OrderMethod = 0;
            filter.LocationName = string.IsNullOrEmpty(locationName) ? "Philippines" : locationName;
            filter.LocationIdDefault = (filter.LocationId ?? 0) > 0 ? filter.LocationId : filter.LocationIdDefault;

            IEnumerable<PropertyEntry> propertyList = null;

           
           propertyList = propertyEntries.OrderByDescending(x => x.UpdatedDate);

            if (propIndInfo != null)
            {
                if (propIndInfo.PropIndicatorName == EnumString.GetStringValue(PropIndicator.ForSale))
                {
                    ViewBag.CurrentMenu = RealtyWeb.Common.Menu.ForSale;
                }
                else if (propIndInfo.PropIndicatorName == EnumString.GetStringValue(PropIndicator.ForRent))
                {
                    ViewBag.CurrentMenu = RealtyWeb.Common.Menu.ForRent;
                }
                else if (propIndInfo.PropIndicatorName == EnumString.GetStringValue(PropIndicator.Development))
                {
                    ViewBag.CurrentMenu = RealtyWeb.Common.Menu.Development;
                }
                else if (propIndInfo.PropIndicatorName == EnumString.GetStringValue(PropIndicator.Foreclosed))
                {
                    ViewBag.CurrentMenu = RealtyWeb.Common.Menu.Foreclosed;
                }
            }

            var pageNum = 0;
            ViewBag.IsEndOfRecords = false;
            LoadAllPropertiesToSession(propertyList);
            ViewBag.PropertyList = GetRecordsForPage(pageNum);

            return View("SearchLayout", filter);
        }

        public ActionResult GetLocations(string query)
        {
            return Json(SearchCommon.GetLocations(query), JsonRequestBehavior.AllowGet);
        }

        public ActionResult CustomGetLocations(string query, int? page)
        {
            return Json(SearchCommon.CustomGetLocations(query, page), JsonRequestBehavior.AllowGet);
        }

        public ActionResult About()
        {
            var constants = db.Constants;
            var defaultPropType = db.Properties.First().PropertyId;
            var defaultPropIndInfo = db.PropertyIndicators.First();
            var defaultPropInd = defaultPropIndInfo.PropIndicatorId;

            var indicators = new SelectList(db.PropertyIndicators, "PropIndicatorId", "PropIndicatorName", defaultPropInd.ToString());
            var propertyTypes = new SelectList(db.Properties, "PropertyId", "PropertyName");
            var minPrices = new SelectList(constants.Where(x => x.ConstantCd == "MINPRICE"), "ConstantValue", "ConstantValue");
            var maxPrices = new SelectList(constants.Where(x => x.ConstantCd == "MAXPRICE"), "ConstantValue", "ConstantValue");
            var baths = new SelectList(constants.Where(x => x.ConstantCd == "BATHROOMS"), "ConstantValue", "ConstantValue");
            var beds = new SelectList(constants.Where(x => x.ConstantCd == "BEDROOMS"), "ConstantValue", "ConstantValue");
            var orderByItems = constants.Where(x => x.ConstantCd == "ORDERBYITEMS").ToList();

            ViewBag.MinPrices = minPrices;
            ViewBag.MaxPrices = maxPrices;
            ViewBag.Baths = baths;
            ViewBag.Beds = beds;
            ViewBag.PropertyId = propertyTypes;
            ViewBag.PropIndicatorId = indicators;

            ViewBag.SiteInfo = db.Sites.FirstOrDefault();
            var postCards = db.PostCards.Where(x => x.Status == "A").ToList();
            ViewBag.Activities = postCards.Where(x => x.Type == "Activity").ToList();
            ViewBag.Recognitions = postCards.Where(x => x.Type == "Recognition").ToList();
            ViewBag.CurrentMenu = RealtyWeb.Common.Menu.About;

            var filterModel = new SearchFilter();
            filterModel.PropIndicatorId = defaultPropInd;

            return View(filterModel);
        }

        [Authorize]
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpPost]
        [Authorize]
        public PartialViewResult AddFavorite(int userId, int propertyEntryId)
        {
            bool exist = db.UserFavorites.Where(x => x.UserId == userId && x.PropertyEntryId == propertyEntryId).Count() > 0;

            if (!exist)
            {
                var newFavorite = new UserFavorite();
                newFavorite.UserId = userId;
                newFavorite.PropertyEntryId = propertyEntryId;
                newFavorite.CreatedBy = userId;
                newFavorite.CreatedDate = DateTime.Now;
                db.UserFavorites.Add(newFavorite);
                db.SaveChanges();
            }

            var propertyInfo = db.PropertyEntries.Where(x => x.PropertyEntryId == propertyEntryId).Include(x => x.UserFavorite).FirstOrDefault();
            var propertyInfoDictionary = new KeyValuePair<int, PropertyEntry>(0, propertyInfo);
            ViewBag.UserLoginId = userId;

            return PartialView("_Favorite", propertyInfoDictionary);
        }

        [HttpPost]
        public void AddUserInquiry(int propertyEntryId, string name, string email, string mobileNo, string message)
        {
            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(mobileNo) && !string.IsNullOrEmpty(message))
            {
                if (!IsValidEmail(email))
                {
                    throw new Exception("Invalid email format.");
                }

                try
                {
                    using (var context = new RealtyWebContext())
                    {
                        var newInquiry = new RealtyWeb.Models.ClientInquiry();
                        newInquiry.PropertyEntryId = propertyEntryId;
                        newInquiry.Name = name;
                        newInquiry.Email = email;
                        newInquiry.ContactNo = mobileNo;
                        newInquiry.InquiryNote = message;
                        newInquiry.CreatedBy = 1;
                        newInquiry.UpdatedBy = 1;
                        newInquiry.CreatedDate = DateTime.Now;
                        newInquiry.UpdatedDate = DateTime.Now;
                        newInquiry.Status = "A";

                        context.ClientInquiries.Add(newInquiry);
                        context.SaveChanges();
                    }
                }
                catch (DbEntityValidationException)
                {
                    Transaction.Current.Rollback();
                    throw new Exception("Validation Error. Please review required fields and value format.");
                }
                catch (Exception ex)
                {
                    Transaction.Current.Rollback();
                    throw ex;
                }

                try
                {
                    var client = new AppUser();
                    client.FirstName = name;
                    client.Email = email;
                    client.PhoneNumber = mobileNo;

                    var propertyInfo = db.PropertyEntries.Find(propertyEntryId);

                    Common.Common.SendPropertyClientInquiryEmail(message, propertyEntryId, client, propertyInfo.AppUser);
                }
                catch { }
            }
            else
            {
                throw new Exception("Please fill in required fields.");
            }
        }

        [HttpPost]
        public void AddClientMessage(string name, string email, string mobileNo, string message)
        {
            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(mobileNo) && !string.IsNullOrEmpty(message))
            {
                if (!IsValidEmail(email))
                {
                    throw new Exception("Invalid email format.");
                }
                try
                {
                    using (var context = new RealtyWebContext())
                    {
                        var newMessage = new RealtyWeb.Models.ClientMessage();
                        newMessage.Name = name;
                        newMessage.Email = email;
                        newMessage.ContactNo = mobileNo;
                        newMessage.Message = message;
                        newMessage.CreatedDate = DateTime.Now;
                        newMessage.Status = "N";

                        context.ClientMessages.Add(newMessage);
                        context.SaveChanges();
                    }
                }
                catch (DbEntityValidationException)
                {
                    Transaction.Current.Rollback();
                    throw new Exception("Validation Error. Please review required fields and value format.");
                }
                catch (Exception ex)
                {
                    Transaction.Current.Rollback();
                    throw ex;
                }

                try
                {
                    var client = new AppUser();
                    client.FirstName = name;
                    client.Email = email;
                    client.PhoneNumber = mobileNo;
                    Common.Common.SendClientInquiryEmail(message, client);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            else
            {
                throw new Exception("Please fill in required fields.");
            }
        }

        public ActionResult GetProperties(int? pageNum)
        {
            pageNum = pageNum ?? 0;
            ViewBag.IsEndOfRecords = false;

            var properties = GetRecordsForPage(pageNum.Value);
            ViewBag.IsEndOfRecords = (properties.Any()) && ((pageNum.Value * RecordsPerPage) >= properties.Last().Key);
            var loginInfo = Session["LoginInfo"] as AppUser;
            ViewBag.UserLoginId = loginInfo != null ? loginInfo.Id : 0;

            return PartialView("_PropertyRow", properties);
        }

        public void LoadAllPropertiesToSession(IEnumerable<PropertyEntry> properties)
        {
            var propertyList = properties;
            int custIndex = 1;
            Session["PropertyList"] = propertyList.ToDictionary(x => custIndex++, x => x);
            ViewBag.TotalNumberProperties = propertyList.Count();
        }

        public Dictionary<int, PropertyEntry> GetRecordsForPage(int pageNum)
        {
            Dictionary<int, PropertyEntry> properties = (Session["PropertyList"] as Dictionary<int, PropertyEntry>);

            int from = (pageNum * RecordsPerPage);
            int to = from + RecordsPerPage;

            return properties
                .Where(x => x.Key > from && x.Key <= to)
                .OrderBy(x => x.Key)
                .ToDictionary(x => x.Key, x => x.Value);
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            if (filterContext.Exception == null) return;

            Type exceptionType = filterContext.Exception.GetType();

            //if (exceptionType == typeof(DbEntityValidationException))
            //{

            filterContext.ExceptionHandled = true;
            filterContext.HttpContext.Response.Clear();
            filterContext.HttpContext.Response.ContentEncoding = Encoding.UTF8;
            filterContext.HttpContext.Response.HeaderEncoding = Encoding.UTF8;
            filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
            filterContext.HttpContext.Response.StatusCode = 500;
            filterContext.HttpContext.Response.StatusDescription = filterContext.Exception.Message;
            //}
        }

        public bool IsValidEmail(string strIn)
        {
            bool invalid = false;
            if (String.IsNullOrEmpty(strIn))
                return false;

            // Use IdnMapping class to convert Unicode domain names.
            try
            {
                strIn = Regex.Replace(strIn, @"(@)(.+)$", this.DomainMapper,
                                      RegexOptions.None, TimeSpan.FromMilliseconds(200));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }

            if (invalid)
                return false;

            // Return true if strIn is in valid e-mail format.
            try
            {
                return Regex.IsMatch(strIn,
                      @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                      @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                      RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        private string DomainMapper(Match match)
        {
            IdnMapping idn = new IdnMapping();
            string domainName = match.Groups[2].Value;
            return match.Groups[1].Value + domainName;
        }

        public ActionResult ViewSubscription()
        {
            return PartialView("_Subscription");
        }

        [HttpPost]
        public void AddSubscription(string email, string mobileNo, bool? receiveAlerts)
        {
            if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(mobileNo))
            {
                if (!IsValidEmail(email))
                {
                    throw new Exception("Invalid email format.");
                }
                try
                {
                    using (var context = new RealtyWebContext())
                    {
                        var record = context.Subscriptions.Where(x => x.Email == email.TrimEnd().TrimStart()).FirstOrDefault();

                        if (record == null)
                        {
                            var newMessage = new RealtyWeb.Models.Subscription();
                            newMessage.Email = email;
                            newMessage.ContactNo = mobileNo;
                            newMessage.ReceiveAlerts = receiveAlerts ?? false;
                            newMessage.CreatedDate = DateTime.Now;

                            context.Subscriptions.Add(newMessage);
                            context.SaveChanges();
                        }
                        else
                        {
                            var existingRecord = context.Subscriptions.Find(record.Id);

                            existingRecord.ContactNo = mobileNo;
                            existingRecord.ReceiveAlerts = receiveAlerts ?? false;

                            context.Entry(existingRecord).State = EntityState.Modified;
                            context.SaveChanges();
                        }
                    }
                }
                catch (DbEntityValidationException)
                {
                    Transaction.Current.Rollback();
                    throw new Exception("Validation Error. Please input required fields and value format.");
                }
                catch (Exception ex)
                {
                    Transaction.Current.Rollback();
                    throw ex;
                }
            }
            else
            {
                throw new Exception("Please fill in required fields.");
            }
        }
    }
}