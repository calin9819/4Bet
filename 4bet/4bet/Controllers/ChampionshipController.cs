using _4bet.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace _4bet.Controllers
{
    public class ChampionshipController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();
        [Authorize(Roles = "User,Administrator")]
        public ActionResult Index()
        {
            var championships = db.Championships;
            var isAdmin = User.IsInRole("Administrator");
            ViewBag.Championships = championships;
            ViewBag.isAdmin = isAdmin;
            return View();
        }

        [Authorize(Roles = "User,Administrator")]
        public ActionResult Show(int id)
        {
            var championship = db.Championships.Find(id);
            var isAdmin = User.IsInRole("Administrator");
            var now = DateTime.Now;
            ViewBag.Championship = championship;
            ViewBag.Teams = championship.Teams;
            ViewBag.Matches = db.FootballMatches.Where(x => x.ChampionshipId == id && (x.Outcome == "0" && x.ResolveTime > now)).ToList();
            ViewBag.PastMatches = db.FootballMatches.Where(x => x.ChampionshipId == id && !(x.Outcome == "0" && x.ResolveTime > now)).ToList();
            ViewBag.HasMatches = ViewBag.Matches.Count > 0;
            ViewBag.HasHistory = ViewBag.PastMatches.Count > 0;
            ViewBag.isAdmin = isAdmin;
            return View();
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult New()
        {
            return View();
        }


        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public ActionResult New(string championshipName, string country, HttpPostedFileBase file)
        {

            try
            {
                if (ModelState.IsValid)
                {
                    string fileName = null;
                    if (file != null)
                    {
                        if (file.ContentLength <= 0)
                        {
                            throw new Exception("Error while uploading");
                        }

                        fileName = Path.GetFileName(file.FileName);
                        string path = Path.Combine(Server.MapPath("~/Assets"), fileName);
                        file.SaveAs(path);
                    }

                    var championship = new Championship
                    {
                        ChampionshipName = championshipName,
                        Country = country,
                        PhotoFileName = fileName
                    };
                    db.Championships.Add(championship);
                    db.SaveChanges();
                    TempData["message"] = "Championship was saved!";
                    return RedirectToAction("Index");
                }
                else
                {
                    return RedirectToAction("New");
                }
            }
            catch (Exception e)
            {
                return View();
            }
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult Edit(int id)
        {
            var championship = db.Championships.Find(id);
            return View(championship);
        }

        [HttpPut]
        [Authorize(Roles = "Administrator")]
        public ActionResult Edit(int id, Championship requestChampionship)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Championship championship = db.Championships.Find(id);
                    if (TryUpdateModel(championship))
                    {
                        championship.ChampionshipName = requestChampionship.ChampionshipName;
                        championship.Country = requestChampionship.Country;
                        TempData["message"] = "Championship data was changed!";
                        db.SaveChanges();
                    }
                    return RedirectToAction("Index");
                }
                else
                {
                    return View(requestChampionship);
                }
            }
            catch (Exception e)
            {
                return View(requestChampionship);
            }
        }

        [HttpDelete]
        [Authorize(Roles = "Administrator")]
        public ActionResult Delete(int id)
        {
            Championship championship = db.Championships.Find(id);
            
            db.Championships.Remove(championship);
            TempData["message"] = "Match was deleted!";
            db.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}