using _4bet.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace _4bet.Controllers
{
    public class TeamController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();
        [Authorize(Roles = "User,Administrator")]
        public ActionResult Index()
        {
            var teams = db.Teams;
            var isAdmin = User.IsInRole("Administrator");
            ViewBag.Teams = teams;
            ViewBag.isAdmin = isAdmin;
            return View();
        }

        [Authorize(Roles = "User,Administrator")]
        public ActionResult Show(int id)
        {
            var team = db.Teams.Find(id);
            var isAdmin = User.IsInRole("Administrator");
            var now = DateTime.Now;
            var matches = db.FootballMatches.Where(x => x.FirstTeamId == id || x.SecondTeamId == id);
            var currentMatches = matches.Where(x => (x.ResolveTime > now && x.Outcome == "0"));
            var pastMatches = matches.Where(x => !(x.ResolveTime > now && x.Outcome == "0"));
            ViewBag.HasMatches = currentMatches.Count() > 0;
            ViewBag.HasHistory = pastMatches.Count() > 0;
            ViewBag.CurrentMatches = currentMatches;
            ViewBag.PastMatches = pastMatches;
            ViewBag.Team = team;
            ViewBag.isAdmin = isAdmin;
            return View();
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult New()
        {
            var championships = db.Championships.ToList();
            ViewBag.Championships = championships;
            return View();
        }


        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public ActionResult New(string teamName, int championshipId, HttpPostedFileBase file)
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

                    var team = new Team
                    {
                        TeamName = teamName,
                        ChampionshipId = championshipId,
                        PhotoFileName = fileName
                    };
                    var championship = db.Championships.Find(championshipId);
                    championship.Teams.Add(team);
                    db.Teams.Add(team);
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
            var team = db.Teams.Find(id);
            var championships = db.Championships.ToList();
            ViewBag.Championships = championships;
            return View(team);
        }

        [HttpPut]
        [Authorize(Roles = "Administrator")]
        public ActionResult Edit(int id, Team requestTeam)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Team team = db.Teams.Find(id);
                    if (TryUpdateModel(team))
                    {
                        team.TeamName = requestTeam.TeamName;
                        team.ChampionshipId = requestTeam.ChampionshipId;
                        TempData["message"] = "Championship data was changed!";
                        db.SaveChanges();
                    }
                    return RedirectToAction("Index");
                }
                else
                {
                    return View(requestTeam);
                }
            }
            catch (Exception e)
            {
                return View(requestTeam);
            }
        }

        [HttpDelete]
        [Authorize(Roles = "Administrator")]
        public ActionResult Delete(int id)
        {
            Team team = db.Teams.Find(id);

            db.Teams.Remove(team);
            TempData["message"] = "Match was deleted!";
            db.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
