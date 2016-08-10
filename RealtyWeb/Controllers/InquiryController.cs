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

namespace RealtyWeb.Controllers
{
    [Authorize]
    public class InquiryController : Controller
    {
        private RealtyWebContext db = new RealtyWebContext();

        [Route("Client/Inquiry")]
        public ActionResult Index(MessageFilter filter)
        {
            var loginId = User.Identity.GetUserId<int>();
            var userInfo = db.AppUsers.Find(loginId);
            var isAdmin = userInfo != null && userInfo.Roles.Any(x => x.Name == "Administrator");
            var isBroker = userInfo != null && userInfo.Roles.Any(x => x.Name == "Broker");
            var isAgent = userInfo != null && userInfo.Roles.Any(x => x.Name == "Agent");

            if (!isAdmin && !isBroker && !isAgent)
            {
                return ReturnUnAuthorize();
            }

            filter.Page = filter != null ? filter.Page ?? 1 : 1;
            filter.FromDate = filter != null ? filter.FromDate ?? DateTime.Now.AddDays(-5).Date : DateTime.Now.AddDays(-5).Date;
            filter.ToDate = filter != null ? filter.ToDate ?? DateTime.Now.Date.AddDays(1) : DateTime.Now.Date.AddDays(1);

            var messages = new List<Message>();

            if (isAdmin)
            {
               messages = db.ClientInquiries.Where(x => x.Status == "A" &&
                                                        x.CreatedDate >= filter.FromDate &&
                                                        x.CreatedDate <= filter.ToDate)
                                    .Select(x => new Message
                                    {
                                        PropertyEntryId = x.PropertyEntryId,
                                        Name = x.Name,
                                        Email = x.Email,
                                        ContactNo = x.ContactNo,
                                        Messages = x.InquiryNote,
                                        Status = x.Status,
                                        DateSubmitted = x.CreatedDate,
                                        BrokerAgent = x.PropertyEntry.AppUser.FirstName,
                                        PropertyAddress = x.PropertyEntry.Address1 + "," +
                                                           x.PropertyEntry.Address2 + "," +
                                                           x.PropertyEntry.Address3 + "," +
                                                           x.PropertyEntry.Address4
                                    }).Union(db.ClientMessages.Where(x => x.Status == "N" && x.CreatedDate >= filter.FromDate && x.CreatedDate <= filter.ToDate)
                                                               .Select(x => new Message
                                                               {
                                                                   PropertyEntryId = null,
                                                                   Name = x.Name,
                                                                   Email = x.Email,
                                                                   ContactNo = x.ContactNo,
                                                                   Messages = x.Message,
                                                                   Status = x.Status,
                                                                   DateSubmitted = x.CreatedDate,
                                                                   BrokerAgent = "Administrator",
                                                                   PropertyAddress = string.Empty
                                                               })).OrderByDescending(x => x.DateSubmitted).ToList();
            }
            else
            {
                messages = db.ClientInquiries.Where(x => x.Status == "A" &&
                                                        x.CreatedDate >= filter.FromDate &&
                                                        x.CreatedDate <= filter.ToDate &&
                                                        x.PropertyEntry.AppUser.Id == userInfo.Id)
                                    .Select(x => new Message
                                    {
                                        PropertyEntryId = x.PropertyEntryId,
                                        Name = x.Name,
                                        Email = x.Email,
                                        ContactNo = x.ContactNo,
                                        Messages = x.InquiryNote,
                                        Status = x.Status,
                                        DateSubmitted = x.CreatedDate,
                                        BrokerAgent = x.PropertyEntry.AppUser.FirstName,
                                        PropertyAddress = x.PropertyEntry.Address1 + "," +
                                                           x.PropertyEntry.Address2 + "," +
                                                           x.PropertyEntry.Address3 + "," +
                                                           x.PropertyEntry.Address4
                                    }).OrderByDescending(x => x.DateSubmitted).ToList();
            }

            int pageSize = 10;
            int pageNumber = filter.Page ?? 1;

            ViewBag.Messages = messages.ToPagedList(pageNumber, pageSize);

            return View(filter);
        }

        public ActionResult ReturnUnAuthorize()
        {
            return View("Unauthorized");
        }
    }

    public class Message
    {
        public long? PropertyEntryId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string ContactNo { get; set; }
        public string Messages { get; set; }
        public string Status { get; set; }
        public System.DateTime DateSubmitted { get; set; }
        public string BrokerAgent { get; set; }
        public string PropertyAddress { get; set; }
    }

    public class MessageFilter
    {
        public int? Page { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}