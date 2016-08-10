using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using RealtyWeb.Models;

namespace RealtyWeb.Controllers
{
    public class PointOfInterestTypeController : Controller
    {
        private RealtyWebContext db = new RealtyWebContext();

        // GET: /PointOfInterestType/
        [Authorize]
        public ActionResult Index()
        {
            return View(db.PointOfInterestTypes.ToList());
        }

        // GET: /PointOfInterestType/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PointOfInterestType pointofinteresttype = db.PointOfInterestTypes.Find(id);
            if (pointofinteresttype == null)
            {
                return HttpNotFound();
            }
            return View(pointofinteresttype);
        }

        // GET: /PointOfInterestType/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: /PointOfInterestType/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PointOfInterestType pointofinteresttype)
        {
            if (ModelState.IsValid)
            {
                db.PointOfInterestTypes.Add(pointofinteresttype);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(pointofinteresttype);
        }

        // GET: /PointOfInterestType/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PointOfInterestType pointofinteresttype = db.PointOfInterestTypes.Find(id);
            if (pointofinteresttype == null)
            {
                return HttpNotFound();
            }
            return View(pointofinteresttype);
        }

        // POST: /PointOfInterestType/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(PointOfInterestType pointofinteresttype)
        {
            if (ModelState.IsValid)
            {
                db.Entry(pointofinteresttype).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(pointofinteresttype);
        }

        // GET: /PointOfInterestType/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PointOfInterestType pointofinteresttype = db.PointOfInterestTypes.Find(id);
            if (pointofinteresttype == null)
            {
                return HttpNotFound();
            }
            return View(pointofinteresttype);
        }

        // POST: /PointOfInterestType/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            PointOfInterestType pointofinteresttype = db.PointOfInterestTypes.Find(id);
            db.PointOfInterestTypes.Remove(pointofinteresttype);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
