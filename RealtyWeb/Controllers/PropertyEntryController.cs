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

namespace RealtyWeb.Controllers
{
    [Authorize]
    [RoutePrefix("PropertyEntry")]
    public class PropertyEntryController : Controller
    {
        private RealtyWebContext db = new RealtyWebContext();

        // GET: /PropertyEntry/
        [Route("Listing")]
        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId<int>();
            var loginInfo = db.AppUsers.Find(userId);

            var constants = db.Constants;
            var defaultPropType = db.Properties.First().PropertyId;
            var defaultPropInd = db.PropertyIndicators.First().PropIndicatorId;

            var propertyEntries = db.PropertyEntries.Where(x => x.ContactPerson == loginInfo.Id && x.Status == "A").Include(p => p.Property).Include(p => p.AppUser);
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

            var filterModel = new SearchFilter();
            filterModel.LocationName = "Philippines";

            ViewBag.OrderByItems = orderByItems;
            ViewBag.OrderByName = "Default";
            ViewBag.OrderById = null;
            ViewBag.ViewCompanyInfo = "Yes";
            ViewBag.ViewRightPane = true;

            var propertyList = propertyEntries.OrderByDescending(x => x.UpdatedDate);
            
            int pageSize = 10;
            int pageNumber = 1;
            ViewBag.PropertyList = propertyList.ToPagedList(pageNumber, pageSize);

            return View(filterModel);
        }

        // GET: /PropertyEntry/Details/5
        public ActionResult Details(long? id)
        {
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
            return View(propertyentry);
        }

        // GET: /PropertyEntry/Create
        public ActionResult Create()
        {
            PropertyEntry property = new PropertyEntry();
            property.PointOfInterests = new List<PointOfInterest>();
            ViewBag.PropertyId = new SelectList(db.Properties, "PropertyId", "PropertyName");
            ViewBag.PropIndicatorId = new SelectList(db.PropertyIndicators, "PropIndicatorId", "PropIndicatorName");

            return View(property);
        }

        // POST: /PropertyEntry/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PropertyEntry propertyEntry)
        {
            if (ModelState.IsValid)
            {
                var option = new TransactionOptions();
                option.IsolationLevel = IsolationLevel.ReadCommitted;
                option.Timeout = TimeSpan.FromMinutes(5);

                try
                {
                    using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
                    {
                        var request = Request.Files.Count;
                        using (var context = new RealtyWebContext())
                        {
                            try
                            {
                                var userId = User.Identity.GetUserId<int>();
                                var loginInfo = context.AppUsers.Find(userId);

                                var currentDate = DateTime.Now;
                                propertyEntry.ContactPerson = loginInfo.Id;
                                propertyEntry.CreatedBy = loginInfo.Id;
                                propertyEntry.CreatedDate = currentDate;
                                propertyEntry.UpdatedBy = loginInfo.Id;
                                propertyEntry.UpdatedDate = currentDate;
                                propertyEntry.Status = "A";

                                context.PropertyEntries.Add(propertyEntry);
                                context.SaveChanges();

                                if (propertyEntry.PointOfInterests != null && propertyEntry.PointOfInterests.Count > 0)
                                {
                                    foreach (PointOfInterest poi in propertyEntry.PointOfInterests)
                                    {
                                        PointOfInterest newPoi = new PointOfInterest();
                                        newPoi.PoiTypeId = poi.PoiTypeId;
                                        newPoi.Name = poi.Name;
                                        newPoi.Distance = poi.Distance;
                                        newPoi.PropertyEntryId = propertyEntry.PropertyEntryId;
                                        newPoi.CreatedBy = loginInfo.Id;
                                        newPoi.CreatedDate = currentDate;
                                        newPoi.UpdatedBy = loginInfo.Id;
                                        newPoi.UpdatedDate = currentDate;

                                        context.PointOfInterests.Add(newPoi);
                                        context.SaveChanges();
                                    }
                                }

                                if (propertyEntry.UploadImages != null && propertyEntry.UploadImages.Count() > 0)
                                {
                                    foreach (HttpPostedFileBase file in propertyEntry.UploadImages)
                                    {
                                        WebImage photo = WebImage.GetImageFromRequest("uploadImages");

                                        string filextension = string.Empty; //file.FileName.Substring(file.FileName.LastIndexOf('.')).ToLower();
                                        switch (photo.ImageFormat)
                                        {
                                            case "jpeg": filextension = ".jpg"; break;
                                            case "png": filextension = ".png"; break;
                                            case "bmp": filextension = ".bmp"; break;
                                            case "gif": filextension = ".gif"; break;
                                            default: filextension = ".jpg"; break;
                                        }

                                        string filename = propertyEntry.PropertyEntryId.ToString() + "_" + Guid.NewGuid().ToString() + filextension;
                                        string filePath = "~/Gallery/" + filename;
                                        //file.SaveAs(Server.MapPath(filePath)); //Saving Image to Server Local

                                        if (photo != null)
                                        {
                                            photo.Resize(width: 500, height: 400, preserveAspectRatio: true, preventEnlarge: true);
                                            photo.Crop(1, 1, 0, 0);
                                            photo.Save(filePath);
                                        }

                                        ImageGallery imageGallery = new ImageGallery();
                                        imageGallery.TranId = propertyEntry.PropertyEntryId;
                                        imageGallery.ImagePath = filePath;
                                        context.ImageGalleries.Add(imageGallery);
                                        context.SaveChanges(); //Saving path to Database
                                    }
                                }

                                scope.Complete();
                                TempData["CreateSuccessful"] = true;
                            }
                            catch (DbEntityValidationException)
                            {
                                Transaction.Current.Rollback();
                                throw new Exception("Validation Error");
                            }
                            catch (Exception ex)
                            {
                                Transaction.Current.Rollback();
                                throw ex;
                            }
                        }
                    }
                }
                catch(Exception)
                {
                    TempData["CreateSuccessful"] = false;
                    return View(propertyEntry);
                }
                finally
                {
                    ViewData["PoiTypes"] = new SelectList(db.PointOfInterestTypes, "PoiTypeId", "PoiTypeName");
                    ViewBag.PropertyId = new SelectList(db.Properties, "PropertyId", "PropertyName", propertyEntry.PropertyId);
                    ViewBag.PropIndicatorId = new SelectList(db.PropertyIndicators, "PropIndicatorId", "PropIndicatorName", propertyEntry.PropIndicatorId);
                    propertyEntry.PointOfInterests = propertyEntry.PropertyEntryId > 0 ? db.PointOfInterests.Where(x => x.PropertyEntryId == propertyEntry.PropertyEntryId).ToList() : new List<PointOfInterest>();
                    ViewBag.ViewCompanyInfo = "No";
                }
            }

            return RedirectToAction("Edit", new { id = propertyEntry.PropertyEntryId });
        }

        // GET: /PropertyEntry/Edit/5
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            PropertyEntry propertyEntry = db.PropertyEntries.Find(id);
            if (propertyEntry == null)
            {
                return HttpNotFound();
            }

            ViewData["PoiTypes"] = new SelectList(db.PointOfInterestTypes, "PoiTypeId", "PoiTypeName");
            ViewBag.PropertyId = new SelectList(db.Properties, "PropertyId", "PropertyName", propertyEntry.PropertyId);
            ViewBag.PropIndicatorId = new SelectList(db.PropertyIndicators, "PropIndicatorId", "PropIndicatorName", propertyEntry.PropIndicatorId);
            propertyEntry.PointOfInterests = db.PointOfInterests.Where(x => x.PropertyEntryId == propertyEntry.PropertyEntryId).ToList();
            return View(propertyEntry);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(PropertyEntry propertyEntry)
        {
            if (ModelState.IsValid)
            {
                var request = Request.Files.Count;

                var option = new TransactionOptions();
                option.IsolationLevel = IsolationLevel.ReadCommitted;
                option.Timeout = TimeSpan.FromMinutes(5);

                try
                {
                    using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
                    {
                        try
                        {
                            using (var context = new RealtyWebContext())
                            {

                                var userId = User.Identity.GetUserId<int>();
                                var loginInfo = context.AppUsers.Find(userId);

                                var currentDate = DateTime.Now;
                                propertyEntry.UpdatedBy = loginInfo.Id;
                                propertyEntry.UpdatedDate = currentDate;

                                context.Entry(propertyEntry).State = EntityState.Modified;
                                context.SaveChanges();

                                List<PointOfInterest> poiBackUp = context.PointOfInterests.Where(x => x.PropertyEntryId == propertyEntry.PropertyEntryId).ToList();

                                if (propertyEntry.PointOfInterests != null)
                                {
                                    foreach (PointOfInterest poi in propertyEntry.PointOfInterests)
                                    {
                                        PointOfInterest poiOnDB = context.PointOfInterests.Find(poi.PoiId);

                                        if (poiOnDB != null)
                                        {
                                            poiOnDB.PoiTypeId = poi.PoiTypeId;
                                            poiOnDB.Name = poi.Name;
                                            poiOnDB.Distance = poi.Distance;
                                            poiOnDB.UpdatedBy = loginInfo.Id;
                                            poiOnDB.UpdatedDate = currentDate;

                                            context.Entry(poiOnDB).State = EntityState.Modified;
                                            context.SaveChanges();
                                        }
                                        else
                                        {
                                            PointOfInterest newPoi = new PointOfInterest();
                                            newPoi.PoiTypeId = poi.PoiTypeId;
                                            newPoi.Name = poi.Name;
                                            newPoi.Distance = poi.Distance;
                                            newPoi.PropertyEntryId = propertyEntry.PropertyEntryId;
                                            newPoi.CreatedBy = loginInfo.Id;
                                            newPoi.CreatedDate = currentDate;
                                            newPoi.UpdatedBy = loginInfo.Id;
                                            newPoi.UpdatedDate = currentDate;

                                            newPoi = context.PointOfInterests.Add(newPoi);
                                            context.SaveChanges();
                                            poi.PoiId = newPoi.PoiId;
                                        }
                                        EntityState state = db.Entry<PointOfInterest>(poi).State;
                                    }
                                }

                                List<PointOfInterest> poiListOnTran = propertyEntry.PointOfInterests != null ? propertyEntry.PointOfInterests.ToList() : new List<PointOfInterest>();
                                List<PointOfInterest> poiListDeleted = poiBackUp.Except<PointOfInterest>(poiListOnTran, new PointOfIntrestComparer()).ToList();

                                if (poiListDeleted.Count() > 0)
                                {
                                    context.PointOfInterests.RemoveRange(poiListDeleted);
                                    context.SaveChanges();
                                }
                            }

                            TempData["EditSuccessful"] = true;
                            scope.Complete();
                        }
                        catch (DbEntityValidationException)
                        {
                            Transaction.Current.Rollback();
                            TempData["EditSuccessful"] = false;
                            throw new Exception("Validation Errors");
                        }
                        catch (Exception ex)
                        {
                            Transaction.Current.Rollback();
                            TempData["EditSuccessful"] = false;
                            throw ex;
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
                finally
                {
                    ViewData["PoiTypes"] = new SelectList(db.PointOfInterestTypes, "PoiTypeId", "PoiTypeName");
                    ViewBag.PropertyId = new SelectList(db.Properties, "PropertyId", "PropertyName", propertyEntry.PropertyId);
                    ViewBag.PropIndicatorId = new SelectList(db.PropertyIndicators, "PropIndicatorId", "PropIndicatorName", propertyEntry.PropIndicatorId);
                    propertyEntry.PointOfInterests = db.PointOfInterests.Where(x => x.PropertyEntryId == propertyEntry.PropertyEntryId).ToList();
                }
            }

            PropertyEntry updatedPropertyEntry = db.PropertyEntries.Find(propertyEntry.PropertyEntryId);
            updatedPropertyEntry.PointOfInterests = db.PointOfInterests.Where(x => x.PropertyEntryId == propertyEntry.PropertyEntryId).ToList();

            return View(updatedPropertyEntry);
        }

        // GET: /PropertyEntry/Delete/5
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PropertyEntry propertyentry = db.PropertyEntries.Find(id);
            if (propertyentry == null)
            {
                return HttpNotFound();
            }
            return PartialView("_Delete", propertyentry);
        }

        // GET: /PropertyEntry/Delete/
        [HttpPost]
        public void Delete()
        {
            long propertyEntryId = Convert.ToInt64(Request.Form["propertyEntryId"]);

            var propertyentry = db.PropertyEntries.Find(propertyEntryId);
            var imageGalleries = db.ImageGalleries.Where(x => x.TranId == propertyentry.PropertyEntryId).ToList();

            foreach (var image in imageGalleries)
            {
                db.ImageGalleries.Remove(image);
                db.SaveChanges();

                string imagePath = Request.MapPath(image.ImagePath);

                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            propertyentry.Status = "C";
            propertyentry.UpdatedBy = User.Identity.GetUserId<int>();
            propertyentry.UpdatedDate = DateTime.Now;
            db.SaveChanges();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public ActionResult UploadImage(int id)
        {
            ViewData.Add("propertyEntryId", id);

            var imageList = db.ImageGalleries.Where(x => x.TranId == id).ToList();
            return View(imageList);
        }

        [HttpPost]
        public ActionResult UploadImageAction()
        {
            if (Request.Files.Count != 0)
            {
                long propertyEntryId = Convert.ToInt64(Request.Form["propertyEntryId"]);

                for (int i = 0; i < Request.Files.Count; i++)
                {
                    HttpPostedFileBase file = Request.Files[i];

                    WebImage photo = WebImage.GetImageFromRequest(file.FileName);

                    string filextension = string.Empty;//file.FileName.Substring(file.FileName.LastIndexOf('.')).ToLower();

                    switch (photo.ImageFormat)
                    {
                        case "jpeg": filextension = ".jpg"; break;
                        case "png": filextension = ".png"; break;
                        case "bmp": filextension = ".bmp"; break;
                        case "gif": filextension = ".gif"; break;
                        default: filextension = ".jpg"; break;
                    }

                    string filename = propertyEntryId.ToString() + "_" + Guid.NewGuid().ToString() + filextension;
                    string filePath = "~/Gallery/" + filename;
                    //file.SaveAs(Server.MapPath(filePath)); //Saving Image to Server Local
                    
                    if (photo != null)
                    {
                        photo.Resize(width: 500, height: 400, preserveAspectRatio: true, preventEnlarge: true);
                        photo.Crop(1, 1, 0, 0);
                        photo.Save(filePath);
                    }

                    ImageGallery imageGallery = new ImageGallery();
                    imageGallery.TranId = Convert.ToInt64(Request.Form["propertyEntryId"]);
                    imageGallery.ImagePath = filePath;
                    db.ImageGalleries.Add(imageGallery);
                    db.SaveChanges(); //Saving path to Database
                }

                var model = db.ImageGalleries.Where(x => x.TranId == propertyEntryId).ToList();

                return PartialView("_ImageList", model);
            }

            return Content("Error");
        }

        public PartialViewResult AddNewPOI()
        {
            var model = new PointOfInterest();

            ViewData["PoiTypes"] = new SelectList(db.PointOfInterestTypes, "PoiTypeId", "PoiTypeName");
            return PartialView("_PointOfIntestList", model);
        }

        [HttpPost]
        public ActionResult DeleteImage(int imageId, int tranId)
        {
            var imageOnDB = db.ImageGalleries.Find(imageId);
            db.ImageGalleries.Remove(imageOnDB);
            db.SaveChanges();

            string imagePath = Request.MapPath(imageOnDB.ImagePath);

            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }

            var model = db.ImageGalleries.Where(x => x.TranId == tranId).ToList();
            return PartialView("_ImageList", model);
        }

        public ActionResult SearchPropertyPerUser(SearchFilter filter)
        {
            var loginInfo = db.AppUsers.Find(User.Identity.GetUserId<int>());
            var userId = loginInfo != null ? loginInfo.Id : 0;
            var orderByInfo = filter.OrderMethod != null ? db.Constants.Find(filter.OrderMethod) : null;
            var propIndInfo = filter.PropIndicatorId == null ? db.PropertyIndicators.First() : db.PropertyIndicators.Find(filter.PropIndicatorId);
            var locationInfo = db.Locations.Find((filter.LocationId ?? 0) > 0 ? filter.LocationId : filter.LocationIdDefault);
            var locationName = locationInfo != null ? locationInfo.LocationName.TrimEnd() : string.Empty;

            decimal propertyEntryId = 0;
            string searchQuery = string.IsNullOrEmpty(filter.SearchTerm) ? string.Empty : filter.SearchTerm;
            decimal minPrice = string.IsNullOrEmpty(filter.MinPrices) ? 0 : Convert.ToDecimal(filter.MinPrices);
            decimal maxPrice = string.IsNullOrEmpty(filter.MaxPrices) ? 0 : !filter.MaxPrices.Contains('+') ? Convert.ToDecimal(filter.MaxPrices) : Convert.ToDecimal(filter.MaxPrices.Substring(0, filter.MaxPrices.Length - 2));
            decimal bed = string.IsNullOrEmpty(filter.Beds) ? 0 : Convert.ToDecimal(filter.Beds[0].ToString());
            decimal bath = string.IsNullOrEmpty(filter.Baths) ? 0 : Convert.ToDecimal(filter.Baths[0].ToString());
            bool maxAboveLimit = string.IsNullOrEmpty(filter.MaxPrices) ? false : filter.MaxPrices.Contains('+');

            if (decimal.TryParse(filter.SearchTerm, out propertyEntryId)) { }
            var propertyEntries = db.PropertyEntries.Where(x => (x.ContactPerson == loginInfo.Id) && x.Status == "A" &&
                                                                 (propertyEntryId > 0 ? x.PropertyEntryId == propertyEntryId : //Filter by PropertyEntryId first if any. . .
                                                                 ((filter.PropertyId != null ? x.PropertyId == filter.PropertyId : true) &&
                                                                 (filter.PropIndicatorId != null ? x.PropIndicatorId == filter.PropIndicatorId : true) &&
                                                                 (minPrice != 0 ? x.Price >= minPrice : true) &&
                                                                 (maxPrice == 0 || maxAboveLimit ? true : x.Price <= maxPrice) &&
                                                                 (bed != 0 ? x.Bedrooms >= bed : true) &&
                                                                 (bath != 0 ? x.Baths >= bath : true) &&
                                                                 (x.Address1.Contains(searchQuery) ||
                                                                 x.Address2.Contains(searchQuery) ||
                                                                 x.Address3.Contains(searchQuery) ||
                                                                 x.Address4.Contains(searchQuery)) &&
                                                                 (x.Address1.Contains(locationName) ||
                                                                  x.Address2.Contains(locationName) ||
                                                                  x.Address3.Contains(locationName) ||
                                                                  x.Address4.Contains(locationName)
                                                                 )))).Include(p => p.Property).Include(p => p.AppUser);
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

            filter.OrderMethod = orderByInfo != null ? orderByInfo.ConstantId : 0;
            filter.LocationName = string.IsNullOrEmpty(locationName) ? "Philippines" : locationName;
            filter.LocationIdDefault = (filter.LocationId ?? 0) > 0 ? filter.LocationId : filter.LocationIdDefault;

            IOrderedQueryable<PropertyEntry> propertyList = null;

            if (orderByInfo != null)
            {
                switch (orderByInfo.ConstantId)
                {
                    case 50: propertyList = propertyEntries.OrderBy(x => x.Price);break;
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

            int pageSize = 10;
            int pageNumber = (filter.Page ?? 1);

            ViewBag.PropertyList = propertyList.ToPagedList(pageNumber, pageSize);
            ViewBag.ViewCompanyInfo = "Yes";

            return View("Index", filter);
        }

        [AllowAnonymous]
        public ActionResult GetLocations(string query)
        {
            return Json(SearchCommon.GetLocations(query), JsonRequestBehavior.AllowGet);
        }
    }
}
