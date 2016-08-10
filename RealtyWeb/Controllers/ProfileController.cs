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
using System.Web.Helpers;
using RealtyWeb.Common;

namespace RealtyWeb.Controllers
{
    public class ProfileController : Controller
    {
        private RealtyWebContext db = new RealtyWebContext();

        private const int RecordsPerPage = 5;

        public ProfileController()
        {
            ViewBag.RecordsPerPage = RecordsPerPage;
        }

        [Authorize]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Info(int agentId)
        {
            int userId = User.Identity.GetUserId<int>();
            var agentInfo = db.AppUsers.Find(agentId);
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
            ViewBag.ViewRightPane = false;
            ViewBag.ProfileInfo = agentInfo;
            ViewBag.DefaultPropIndName = propIndInfo.PropIndicatorName;
            ViewBag.CurrentMenu = RealtyWeb.Common.Menu.Realty;
            ViewBag.UserLoginId = userId;

            var filterModel = new SearchFilter();
            filterModel.LocationName = "Philippines";

            var propertyEntries = db.PropertyEntries.Where(x => x.ContactPerson == agentId && x.Status == "A").Include(p => p.Property).Include(p => p.AppUser).Include(p => p.UserFavorite);
            var propertyList = propertyEntries.OrderByDescending(x => x.UpdatedDate);

            var pageNum = 0;
            ViewBag.IsEndOfRecords = false;
            LoadAllPropertiesToSession(propertyList);
            ViewBag.PropertyList = GetRecordsForPage(pageNum);

            return View(filterModel);
        }

        [Route("profile/broker-search")]
        public ActionResult BrokerSearch(SearchFilter filter)
        {
            int userId = User.Identity.GetUserId<int>();

            var realtyList = new List<ListItem>();
            realtyList.Add(new ListItem() { Id = 2, Name = "Brokers" });
            realtyList.Add(new ListItem() { Id = 3, Name = "Agents" });
            realtyList.Add(new ListItem() { Id = 4, Name = "Agencies" });

            var propertyTypes = new SelectList(db.Properties, "PropertyId", "PropertyName");
            var indicators = new SelectList(db.PropertyIndicators, "PropIndicatorId", "PropIndicatorName");
            var agenciesBrokers = new SelectList(realtyList, "Id", "Name");
            var propIndInfo = db.PropertyIndicators.First();
            var constants = db.Constants;
            var orderByItems = constants.Where(x => x.ConstantCd == "BROKERORDER").ToList();
            var accountList = new List<AppUser>(); db.AppUsers.ToList();

            if (filter.UserEntityId != null)
            {
                accountList = db.AppUsers.Where(x => x.Roles.Any(y => y.Id == filter.UserEntityId) &&
                                                 (filter.SearchTerm != null ? (filter.SearchTerm.Contains(x.FirstName) || x.FirstName.Contains(filter.SearchTerm) ||
                                                                               filter.SearchTerm.Contains(x.LastName) || x.LastName.Contains(filter.SearchTerm) ||
                                                                               filter.SearchTerm.Contains(x.Address1) || x.Address1.Contains(filter.SearchTerm) ||
                                                                               filter.SearchTerm.Contains(x.Address2) || x.Address2.Contains(filter.SearchTerm) ||
                                                                               filter.SearchTerm.Contains(x.Address3) || x.Address3.Contains(filter.SearchTerm) ||
                                                                               filter.SearchTerm.Contains(x.Address4) || x.Address4.Contains(filter.SearchTerm)) : true)).ToList();
            }
            else
            {
                accountList = db.AppUsers.Where(x => x.Roles.Any(y => y.Name == "Agent" || y.Name == "Broker" || y.Name == "Agency") &&
                                                 (filter.SearchTerm != null ? (filter.SearchTerm.Contains(x.FirstName) || x.FirstName.Contains(filter.SearchTerm) ||
                                                                               filter.SearchTerm.Contains(x.LastName) || x.LastName.Contains(filter.SearchTerm) ||
                                                                               filter.SearchTerm.Contains(x.Address1) || x.Address1.Contains(filter.SearchTerm) ||
                                                                               filter.SearchTerm.Contains(x.Address2) || x.Address2.Contains(filter.SearchTerm) ||
                                                                               filter.SearchTerm.Contains(x.Address3) || x.Address3.Contains(filter.SearchTerm) ||
                                                                               filter.SearchTerm.Contains(x.Address4) || x.Address4.Contains(filter.SearchTerm)) : true)).ToList();
            }

            if (filter.OrderMethod != null)
            {
                switch (filter.OrderMethod)
                {
                    case 55: accountList = accountList.OrderByDescending(x => x.PropertyEntry.Count()).ToList(); break;
                    case 56: accountList = accountList.OrderBy(x => x.FirstName).ToList(); break;
                    case 57: accountList = accountList.OrderByDescending(x => x.FirstName).ToList(); break;
                }
            }
            else
            {
                accountList = accountList.OrderByDescending(x => x.PropertyEntry.Count()).ToList();
            }

            ViewBag.UserEntityId = agenciesBrokers;
            ViewBag.PropIndicatorId = indicators;
            ViewBag.OrderByItems = orderByItems;
            ViewBag.OrderByName = filter.OrderMethod == null ? orderByItems.First().ConstantValue : orderByItems.Find(x => x.ConstantId == filter.OrderMethod).ConstantValue;
            ViewBag.OrderById = null;
            ViewBag.CurrentMenu = RealtyWeb.Common.Menu.Realty;
            ViewBag.PropertyId = propertyTypes;
            ViewBag.DeaultPropIndId = propIndInfo.PropIndicatorId;
            ViewBag.DefaultPropIndName = propIndInfo.PropIndicatorName;
            ViewBag.DefaultUserEntityName = filter.UserEntityId == null ? null : realtyList.Where(x => x.Id == filter.UserEntityId).FirstOrDefault().Name;

            int pageSize = 10;
            int pageNumber = filter.Page ?? 1;
            ViewBag.AccountList = accountList.ToPagedList(pageNumber, pageSize);

            return View(filter);
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

        [Authorize]
        public ActionResult EditProfile()
        {
            AppUser userInfo = db.AppUsers.Find(User.Identity.GetUserId<int>());

            if (userInfo == null)
            {
                RedirectToAction("ReturnUnAuthorize");
            }

            ViewBag.CurrentMenu = RealtyWeb.Common.Menu.Manage;

            if (userInfo.Roles.Any(x => x.Name == "Administrator" || x.Name == "Broker" || x.Name == "Agent"))
            {
                return View("EditAgentProfile", userInfo);
            }
            else
            {
                return View("EditClientProfile", userInfo);
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult EditProfile(AppUser userInfo, HttpPostedFileBase fileUpload)
        {
            var loginId = User.Identity.GetUserId<int>();
            if (userInfo == null || userInfo.Id != loginId)
            {
                RedirectToAction("ReturnUnAuthorize");
            }

            if (ModelState.IsValid)
            {
                var request = Request.Files.Count;

                var option = new TransactionOptions();
                option.IsolationLevel = IsolationLevel.ReadCommitted;
                option.Timeout = TimeSpan.FromMinutes(5);

                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
                {
                    try
                    {
                        using (var context = new RealtyWebContext())
                        {
                            
                            if(fileUpload != null)
                            {
                                if (!string.IsNullOrEmpty(userInfo.ProfilePicLink))
                                {
                                    string imagePath = Request.MapPath(userInfo.ProfilePicLink);

                                    if (System.IO.File.Exists(imagePath))
                                    {
                                        System.IO.File.Delete(imagePath);
                                    }
                                }

                                WebImage photo = WebImage.GetImageFromRequest("fileUpload");

                                string filextension = string.Empty; //fileUpload.FileName.Substring(fileUpload.FileName.LastIndexOf('.')).ToLower();
                                switch (photo.ImageFormat)
                                {
                                    case "jpeg": filextension = ".jpg"; break;
                                    case "png": filextension = ".png"; break;
                                    case "bmp": filextension = ".bmp"; break;
                                    case "gif": filextension = ".gif"; break;
                                    default: filextension = ".jpg"; break;
                                }

                                string filename = loginId.ToString() + "_" + Guid.NewGuid().ToString() + filextension;
                                string filePath = "~/Gallery/" + filename;
                                //fileUpload.SaveAs(Server.MapPath(filePath)); //Saving Image to Server Local

                                if (photo != null)
                                {
                                    photo.Resize(width: 200, height: 200, preserveAspectRatio: true, preventEnlarge: true);
                                    photo.Crop(1, 1, 0, 0);
                                    photo.Save(filePath);
                                }

                                userInfo.ProfilePicLink = filePath;
                            }

                            context.Entry(userInfo).State = EntityState.Modified;
                            context.SaveChanges();
                        }

                        TempData["EditSuccessful"] = true;
                        scope.Complete();
                    }
                    catch (DbEntityValidationException)
                    {
                        Transaction.Current.Rollback();
                        TempData["EditSuccessful"] = false;
                    }
                    catch (Exception)
                    {
                        Transaction.Current.Rollback();
                        TempData["EditSuccessful"] = false;
                    }
                }
            }

            var userOnDb = db.AppUsers.Find(userInfo.Id);

            if (userOnDb.Roles.Any(x => x.Name == "Administrator" || x.Name == "Broker" || x.Name == "Agent"))
            {
                return View("EditAgentProfile", userOnDb);
            }
            else
            {
                return View("EditClientProfile", userOnDb);
            }
        }

        public ActionResult ReturnUnAuthorize()
        {
            return View("Unauthorized");
        }
	}
}