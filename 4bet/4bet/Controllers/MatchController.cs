using _4bet.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace _4bet.Controllers
{
    public class MatchController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        [Authorize(Roles = "User,Administrator")]
        public ActionResult Index()
        {
            var now = DateTime.Now;
            var matches = db.FootballMatches.Where(m => (m.ResolveTime > now && m.Outcome == "0")).OrderBy(m => m.ResolveTime);
            var isAdmin = User.IsInRole("Administrator");
            ViewBag.Matches = matches;
            ViewBag.isAdmin = isAdmin;
            var config = db.Configs.Find(1);
            ViewBag.Config = config;
            return View();
        }

        [Authorize(Roles = "User,Administrator")]
        public ActionResult PastMatches()
        {
            var now = DateTime.Now;
            var matches = db.FootballMatches.Where(m => !(m.ResolveTime > now && m.Outcome == "0"));
            var isAdmin = User.IsInRole("Administrator");
            ViewBag.Matches = matches;
            ViewBag.isAdmin = isAdmin;
            return View();
        }

        [Authorize(Roles = "User,Administrator")]
        public ActionResult Show(int id)
        {
            FootballMatch match = db.FootballMatches.Find(id);
            ViewBag.Match = match;
            return View(match);
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
        public ActionResult New(int championshipId, DateTime resolveTime)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    FootballMatch match = new FootballMatch();
                    match.Outcome = "0";
                    match.ChampionshipId = championshipId;
                    match.Championship = db.Championships.Find(championshipId);
                    match.ResolveTime = resolveTime;
                    db.FootballMatches.Add(match);
                    db.SaveChanges();
                    return RedirectToAction("NewStepTwo", new { id = match.FootballMatchId });
                }
                else
                {
                    return RedirectToAction("New");
                }
            }
            catch (Exception e)
            {
                return RedirectToAction("New");
            }
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult NewStepTwo(int id)
        {
            var match = db.FootballMatches.Find(id);
            var teams = db.Teams.Where(x => x.ChampionshipId == match.ChampionshipId).ToList();
            ViewBag.Teams = teams;
            return View(match);
        }

        [HttpPut]
        [Authorize(Roles = "Administrator")]
        public ActionResult NewStepTwo(int id, FootballMatch requestMatch)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    FootballMatch match = db.FootballMatches.Find(id);
                    if (TryUpdateModel(match))
                    {
                        match.FirstTeamId = requestMatch.FirstTeamId;
                        match.SecondTeamId = requestMatch.SecondTeamId;
                        var firstTeam = db.Teams.Find(requestMatch.FirstTeamId);
                        var secondTeam = db.Teams.Find(requestMatch.SecondTeamId);
                        match.FirstTeam = firstTeam;
                        match.SecondTeam = secondTeam;
                        TempData["message"] = "Match data was changed!";
                        db.SaveChanges();
                    }
                    return RedirectToAction("NewStepThree", new { id = id });
                }
                else
                {
                    return View(requestMatch);
                }
            }
            catch (Exception e)
            {
                return View(requestMatch);
            }
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult NewStepThree(int id)
        {
            var match = db.FootballMatches.Find(id);
            var matches = db.FootballMatches.Where(x => x.Outcome != "0");
            var firstTeamMatches = matches.Where(x => (x.FirstTeamId == match.FirstTeamId && x.SecondTeamId != match.SecondTeamId) || (x.SecondTeamId == match.FirstTeamId && x.FirstTeamId != match.SecondTeamId));
            var secondTeamMatches = matches.Where(x => (x.FirstTeamId == match.SecondTeamId && x.SecondTeamId != match.FirstTeamId) || (x.SecondTeamId == match.SecondTeamId && x.FirstTeamId != match.FirstTeamId));
            var bothTeams = matches.Where(x => (x.FirstTeamId == match.FirstTeamId && x.SecondTeamId == match.SecondTeamId) || (x.FirstTeamId == match.SecondTeamId && x.SecondTeamId == match.FirstTeamId));
            ViewBag.FirstTeamMatches = firstTeamMatches;
            ViewBag.SecondTeamMatches = secondTeamMatches;
            ViewBag.BothTeamsMatches = bothTeams;
            ViewBag.FirstTeam = match.FirstTeam.TeamName;
            ViewBag.SecondTeam = match.SecondTeam.TeamName;
            ViewBag.FirstLabel = "Odd if " + ViewBag.FirstTeam + " wins";
            ViewBag.SecondLabel = "Odd if " + ViewBag.SecondTeam + " wins";
            ViewBag.FirstHistory = firstTeamMatches.Count() > 0;
            ViewBag.SecondHistory = secondTeamMatches.Count() > 0;
            ViewBag.BothHistory = bothTeams.Count() > 0;
            return View(match);
        }

        [HttpPut]
        [Authorize(Roles = "Administrator")]
        public ActionResult NewStepThree(int id, FootballMatch requestMatch)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    FootballMatch match = db.FootballMatches.Find(id);
                    if (TryUpdateModel(match))
                    {
                        match.FirstTeamWinsOdd = requestMatch.FirstTeamWinsOdd;
                        match.SecondTeamWinsOdd = requestMatch.SecondTeamWinsOdd;
                        match.DrawOdd = requestMatch.DrawOdd;
                        TempData["message"] = "Match data was changed!";
                        db.SaveChanges();
                    }
                    return RedirectToAction("Index");
                }
                else
                {
                    return View(requestMatch);
                }
            }
            catch (Exception e)
            {
                return View(requestMatch);
            }
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult Edit(int id)
        {
            var match = db.FootballMatches.Find(id);
            return View(match);
        }

        [HttpPut]
        [Authorize(Roles = "Administrator")]
        public ActionResult Edit(int id, FootballMatch footballMatch)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    FootballMatch match = db.FootballMatches.Find(id);
                    if (TryUpdateModel(match))
                    {
                        match.FirstTeam = footballMatch.FirstTeam;
                        match.SecondTeam = footballMatch.SecondTeam;
                        match.FirstTeamWinsOdd = footballMatch.FirstTeamWinsOdd;
                        match.SecondTeamWinsOdd = footballMatch.SecondTeamWinsOdd;
                        match.DrawOdd = footballMatch.DrawOdd;
                        match.ResolveTime = footballMatch.ResolveTime;
                        TempData["message"] = "Match data was changed!";
                        db.SaveChanges();
                    }
                    return RedirectToAction("Index");
                }
                else
                {
                    return View(footballMatch);
                }
            }
            catch (Exception e)
            {
                return View(footballMatch);
            }
        }

        [HttpDelete]
        [Authorize(Roles = "Administrator")]
        public ActionResult Delete(int id)
        {
            FootballMatch match = db.FootballMatches.Find(id);

            db.FootballMatches.Remove(match);
            TempData["message"] = "Match was deleted!";
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult Resolve(int id)
        {
            var match = db.FootballMatches.Find(id);
            var outcomes = new SelectList(
                new List<SelectListItem>
                {
                    new SelectListItem { Value = "1", Text = match.FirstTeam.TeamName + " wins" },
                    new SelectListItem { Value = "2", Text = match.SecondTeam.TeamName + " wins" },
                    new SelectListItem { Value = "3", Text = "Draw" }
                }, "Value", "Text"
            );
            ViewBag.Outcomes = outcomes;
            return View(match);
        }

        [Authorize(Roles = "Administrator")]
        public void resolveBets(int matchId)
        {
            FootballMatch match = db.FootballMatches.Find(matchId);
            var bets = db.Bets.Where(x => x.MatchId == matchId && x.Committed == true);
            IList<Bet> betsList = bets.ToList();
            foreach (Bet bet in betsList)
            {
                updateBet(bet.BetId, bet.OutcomeChoice.ToString() == match.Outcome);
                if (bet.OutcomeChoice.ToString() == match.Outcome)
                {
                   var amount = bet.Amount * bet.Odd;
                   awardBetcoins(amount, bet.UserId);
                } else
                {
                    addToPrizePool(bet.Amount);
                }
            }
        }

        [HttpPut]
        [Authorize(Roles = "Administrator")]
        public void addToPrizePool(double amount)
        {
            if (ModelState.IsValid)
            {
                Config config = db.Configs.Find(1);
                if (TryUpdateModel(config))
                {
                    config.PrizePool = config.PrizePool + amount;
                    db.SaveChanges();
                }
            }
        }

        [HttpPut]
        [Authorize(Roles = "Administrator")]
        public void updateBet(int id, bool isWon)
        {
            if (ModelState.IsValid)
            {
                Bet bet = db.Bets.Find(id);
                if (TryUpdateModel(bet))
                {
                    bet.MatchedResolved = true;
                    bet.Winner = isWon;
                    db.SaveChanges();
                }
            }
        }

        [HttpPut]
        [Authorize(Roles = "Administrator")]
        public void awardBetcoins(double amount, string userId)
        {
            if (ModelState.IsValid)
            {
                Profile profile = db.Profiles.Find(userId);
                if (TryUpdateModel(profile))
                {
                    profile.Betcoins = profile.Betcoins + amount;
                    db.SaveChanges();
                }
            }
        }

        [HttpPut]
        [Authorize(Roles = "Administrator")]
        public ActionResult Resolve(int id, FootballMatch requestMatch)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    FootballMatch match = db.FootballMatches.Find(id);
                    if (TryUpdateModel(match))
                    {
                        match.Outcome = requestMatch.Outcome;
                        TempData["message"] = "Match data was changed!";
                        db.SaveChanges();
                        resolveBets(id);
                    }
                    return RedirectToAction("Index");
                }
                else
                {
                    return View();
                }
            }
            catch (Exception e)
            {
                return View();
            }
        }
    }
}