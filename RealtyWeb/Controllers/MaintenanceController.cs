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
using System.Web.Helpers;

namespace RealtyWeb.Controllers
{
    [Authorize]
    public class MaintenanceController : Controller
    {
        private RealtyWebContext db = new RealtyWebContext();

        [Route("Site/Maintenance")]
        public ActionResult Index()
        {
            var loginId = User.Identity.GetUserId<int>();
            var userInfo = db.AppUsers.Find(loginId);
            if (userInfo == null || !userInfo.Roles.Any(x => x.Name == "Administrator"))
            {
                return ReturnUnAuthorize();
            }

            ViewBag.SiteInfo = db.Sites.FirstOrDefault();
            ViewBag.Activities = db.PostCards.Where(x => x.Type == "Activity" && x.Status == "A").OrderByDescending(x => x.UpdatedDate).ToList();
            ViewBag.Recognitions = db.PostCards.Where(x => x.Type == "Recognition" && x.Status == "A").OrderByDescending(x => x.UpdatedDate).ToList();

            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateSiteInfo(Site siteInfo, HttpPostedFileBase imageUpload)
        {
            var loginId = User.Identity.GetUserId<int>();
            var userInfo = db.AppUsers.Find(loginId);
            if (userInfo == null || !userInfo.Roles.Any(x => x.Name == "Administrator"))
            {
                RedirectToAction("ReturnUnAuthorize");
            }

            if (ModelState.IsValid)
            {
                var option = new TransactionOptions();
                option.IsolationLevel = IsolationLevel.ReadCommitted;
                option.Timeout = TimeSpan.FromMinutes(5);

                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
                {
                    try
                    {
                        using (var context = new RealtyWebContext())
                        {
                            if (imageUpload != null)
                            {
                                if (!string.IsNullOrEmpty(siteInfo.ImagePath))
                                {
                                    string imagePath = Request.MapPath(siteInfo.ImagePath);

                                    if (System.IO.File.Exists(imagePath))
                                    {
                                        System.IO.File.Delete(imagePath);
                                    }
                                }

                                WebImage photo = WebImage.GetImageFromRequest("imageUpload");

                                string filextension = string.Empty; //imageUpload.FileName.Substring(imageUpload.FileName.LastIndexOf('.')).ToLower();
                                switch (photo.ImageFormat)
                                {
                                    case "jpeg": filextension = ".jpg"; break;
                                    case "png": filextension = ".png"; break;
                                    case "bmp": filextension = ".bmp"; break;
                                    case "gif": filextension = ".gif"; break;
                                    default: filextension = ".jpg"; break;
                                }

                                string filename = "Site" + "_" + Guid.NewGuid().ToString() + filextension;
                                string filePath = "~/Gallery/" + filename;

                                if (photo != null)
                                {
                                    photo.Resize(width: 400, height: 300, preserveAspectRatio: true, preventEnlarge: true);
                                    photo.Crop(1, 1, 0, 0);
                                    photo.Save(filePath);
                                }

                                siteInfo.ImagePath = filePath;
                            }

                            context.Entry(siteInfo).State = EntityState.Modified;
                            context.SaveChanges();
                        }

                        TempData["EditSuccessful"] = true;
                        scope.Complete();
                    }
                    catch (DbEntityValidationException)
                    {
                        Transaction.Current.Rollback();
                        throw new Exception("Please fill in the required fields.");
                    }
                    catch (Exception ex)
                    {
                        Transaction.Current.Rollback();
                        throw ex;
                    }
                }
            }

            return RedirectToAction("Index", "Maintenance");
        }

        public PartialViewResult AddActivity()
        {
            var model = new PostCard();
            model.Status = "A";
            model.Type = "Activity";
            return PartialView("_CreatePostCard", model);
        }

        public PartialViewResult AddRecognition()
        {
            var model = new PostCard();
            model.Status = "A";
            model.Type = "Recognition";
            return PartialView("_CreatePostCard", model);
        }

        [HttpPost]
        public PartialViewResult AddPostCard()
        {
            string postCardType = string.Empty;

            if (ModelState.IsValid)
            {
                var loginId = User.Identity.GetUserId<int>();
                var userInfo = db.AppUsers.Find(loginId);
                if (userInfo == null || !userInfo.Roles.Any(x => x.Name == "Administrator"))
                {
                    RedirectToAction("ReturnUnAuthorize");
                }

                var option = new TransactionOptions();
                option.IsolationLevel = IsolationLevel.ReadCommitted;
                option.Timeout = TimeSpan.FromMinutes(5);

                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
                {
                    try
                    {
                        using (var context = new RealtyWebContext())
                        {
                            if (Request.Files.Count != 0)
                            {
                                PostCard newPostCard = new PostCard();
                                newPostCard.Type = Request.Form["type"];
                                newPostCard.Title = Request.Form["title"];
                                newPostCard.Description = Request.Form["description"];

                                HttpPostedFileBase fileUpload = Request.Files[0];

                                if (fileUpload != null)
                                {
                                    WebImage photo = WebImage.GetImageFromRequest("fileInput");

                                    string filextension = string.Empty; fileUpload.FileName.Substring(fileUpload.FileName.LastIndexOf('.')).ToLower();
                                    switch (photo.ImageFormat)
                                    {
                                        case "jpeg": filextension = ".jpg"; break;
                                        case "png": filextension = ".png"; break;
                                        case "bmp": filextension = ".bmp"; break;
                                        case "gif": filextension = ".gif"; break;
                                        default: filextension = ".jpg"; break;
                                    }

                                    string filename = "Postcard" + "_" + Guid.NewGuid().ToString() + filextension;
                                    string filePath = "~/Gallery/" + filename;
                                    //fileUpload.SaveAs(Server.MapPath(filePath)); //Saving Image to Server Local

                                    if (photo != null)
                                    {
                                        photo.Resize(width: 400, height: 300, preserveAspectRatio: true, preventEnlarge: true);
                                        photo.Crop(1, 1, 0, 0);
                                        photo.Save(filePath);
                                    }

                                    newPostCard.ImagePath = filePath;
                                }
                                newPostCard.CreatedBy = userInfo.Id;
                                newPostCard.CreatedDate = DateTime.Now;
                                newPostCard.UpdatedBy = userInfo.Id;
                                newPostCard.UpdatedDate = DateTime.Now;
                                newPostCard.Status = "A";

                                context.PostCards.Add(newPostCard);
                                context.SaveChanges();

                                postCardType = newPostCard.Type;
                            }
                        }

                        scope.Complete();
                    }
                    catch (DbEntityValidationException)
                    {
                        Transaction.Current.Rollback();
                        throw new Exception("Please fill in required fields.");
                    }
                    catch (Exception ex)
                    {
                        Transaction.Current.Rollback();
                        throw ex;
                    }
                }
            }

            var model = new List<PostCard>();
            if (postCardType == "Activity")
            {
                model = db.PostCards.Where(x => x.Type == "Activity" && x.Status == "A").OrderByDescending(x => x.UpdatedDate).ToList();
            }
            else 
            {
                model = db.PostCards.Where(x => x.Type == "Recognition" && x.Status == "A").OrderByDescending(x => x.UpdatedDate).ToList();
            }

            return PartialView("_PostCards", model);
        }

        public PartialViewResult EditPostCard(int id)
        {
            var model = db.PostCards.Find(id);

            return PartialView("_EditPostCard", model);
        }

        [HttpPost]
        public PartialViewResult EditPostCard()
        {
            string postCardType = string.Empty;

            if (ModelState.IsValid)
            {
                var loginId = User.Identity.GetUserId<int>();
                var userInfo = db.AppUsers.Find(loginId);
                if (userInfo == null || !userInfo.Roles.Any(x => x.Name == "Administrator"))
                {
                    RedirectToAction("ReturnUnAuthorize");
                }

                var option = new TransactionOptions();
                option.IsolationLevel = IsolationLevel.ReadCommitted;
                option.Timeout = TimeSpan.FromMinutes(5);

                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
                {
                    try
                    {
                        using (var context = new RealtyWebContext())
                        {
                            int id = Convert.ToInt32(Request.Form["id"]);
                            var title = Request.Form["title"];
                            var description = Request.Form["description"];

                            var postcard = context.PostCards.Find(id);

                            if (Request.Files.Count > 0)
                            {
                                HttpPostedFileBase fileUpload = Request.Files[0];

                                if (fileUpload != null)
                                {
                                    string imagePath = Request.MapPath(postcard.ImagePath);

                                    if (System.IO.File.Exists(imagePath))
                                    {
                                        System.IO.File.Delete(imagePath);
                                    }

                                    WebImage photo = WebImage.GetImageFromRequest("fileInput");

                                    string filextension = string.Empty; //fileUpload.FileName.Substring(fileUpload.FileName.LastIndexOf('.')).ToLower();
                                    switch (photo.ImageFormat)
                                    {
                                        case "jpeg": filextension = ".jpg"; break;
                                        case "png": filextension = ".png"; break;
                                        case "bmp": filextension = ".bmp"; break;
                                        case "gif": filextension = ".gif"; break;
                                        default: filextension = ".jpg"; break;
                                    }

                                    string filename = "Postcard" + "_" + Guid.NewGuid().ToString() + filextension;
                                    string filePath = "~/Gallery/" + filename;
                                    //fileUpload.SaveAs(Server.MapPath(filePath)); //Saving Image to Server Local

                                    if (photo != null)
                                    {
                                        photo.Resize(width: 400, height: 300, preserveAspectRatio: true, preventEnlarge: true);
                                        photo.Crop(1, 1, 0, 0);
                                        photo.Save(filePath);
                                    }

                                    postcard.ImagePath = filePath;
                                }
                            }

                            postcard.Title = title;
                            postcard.Description = description;
                            postcard.UpdatedBy = userInfo.Id;
                            postcard.UpdatedDate = DateTime.Now;
                            context.Entry(postcard).State = EntityState.Modified;
                            context.SaveChanges();

                            postCardType = postcard.Type;
                        }
                        scope.Complete();
                    }
                    catch (DbEntityValidationException)
                    {
                        Transaction.Current.Rollback();
                        throw new Exception("Please fill in required fields.");
                    }
                    catch (Exception ex)
                    {
                        Transaction.Current.Rollback();
                        throw ex;
                    }
                }
            }

            var model = new List<PostCard>();

            if (postCardType == "Activity")
            {
                model = db.PostCards.Where(x => x.Type == "Activity" && x.Status == "A").OrderByDescending(x => x.UpdatedDate).ToList();
            }
            else
            {
                model = db.PostCards.Where(x => x.Type == "Recognition" && x.Status == "A").OrderByDescending(x => x.UpdatedDate).ToList();
            }

            return PartialView("_PostCards", model);
        }

        [HttpPost]
        public PartialViewResult DeletePostCard(int id)
        {
            var postCard = db.PostCards.Find(id);

            if (postCard != null)
            {
                string imagePath = Request.MapPath(postCard.ImagePath);

                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }

                db.PostCards.Remove(postCard);
                db.SaveChanges();
            }

            var model = new List<RealtyWeb.Models.PostCard>();

            if (postCard.Type == "Activity")
            {
                model = db.PostCards.Where(x => x.Type == "Activity" && x.Status == "A").OrderByDescending(x => x.UpdatedDate).ToList();
            }
            else
            {
                model = db.PostCards.Where(x => x.Type == "Recognition" && x.Status == "A").OrderByDescending(x => x.UpdatedDate).ToList();
            }

            return PartialView("_PostCards", model);
        }

        public PartialViewResult GetPostCards(string type)
        {
            var model = new List<RealtyWeb.Models.PostCard>();

            if (type == "Activity")
            {
                model = db.PostCards.Where(x => x.Type == "Activity" && x.Status == "A").OrderByDescending(x => x.UpdatedDate).ToList();
            }
            else
            {
                model = db.PostCards.Where(x => x.Type == "Recognition" && x.Status == "A").OrderByDescending(x => x.UpdatedDate).ToList();
            }
            return PartialView("_PostCards", model);
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

        public ActionResult ReturnUnAuthorize()
        {
            return View("Unauthorized");
        }
	}
}