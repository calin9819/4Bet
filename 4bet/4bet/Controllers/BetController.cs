using _4bet.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace _4bet.Controllers
{
    public class BetController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        [Authorize(Roles = "User,Administrator")]
        public ActionResult Index()
        {
            var user = User.Identity.GetUserId();
            var bets = db.Bets.Where(x => (x.UserId == user && x.Committed == false && x.FootballMatch.Outcome == "0"));
            var potentialWinnings = 0.0;
            var totalSpent = 0.0;
            var notEnoughBetcoins = false;
            List<Bet> betList = new List<Bet>();
            foreach (var bet in bets)
            {
                var match = db.FootballMatches.Find(bet.MatchId);
                if (match.Outcome == "0")
                {
                    betList.Add(bet);
                    potentialWinnings = potentialWinnings + bet.Amount * bet.Odd;
                    totalSpent = totalSpent + bet.Amount;
                }
            }
            var profile = db.Profiles.Find(user);
            var betcoins = profile.Betcoins;
            if (totalSpent > betcoins)
            {
                notEnoughBetcoins = true;
            }
            ViewBag.isEmpty = true;
            if (bets.Count() > 0)
            {
                ViewBag.isEmpty = false;
            }
            ViewBag.Bets = betList;
            ViewBag.TotalSpent = totalSpent;
            ViewBag.PotentialWinnings = potentialWinnings;
            ViewBag.NotEnoughBetcoins = notEnoughBetcoins;
            ViewBag.User = user;
            return View();
        }

        [Authorize(Roles = "User,Administrator")]
        public ActionResult FootballMatch(int id)
        {
            FootballMatch match = db.FootballMatches.Find(id);
            ViewBag.Match = match;
            var user = db.Profiles.Find(User.Identity.GetUserId());
            ViewBag.User = user;
            var matches = db.FootballMatches.Where(x => x.Outcome != "0");
            matches = matches.Where(x => (x.FirstTeamId == match.FirstTeamId && x.SecondTeamId == match.SecondTeamId) || (x.FirstTeamId == match.SecondTeamId && x.SecondTeamId == match.FirstTeamId));
            ViewBag.PastMatches = matches;
            ViewBag.HasHistory = matches.Count() > 0;
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "User,Administrator")]
        public ActionResult FootballMatch(int id, int Amount)
        {

            try
            {
                if (ModelState.IsValid)
                {
                    var oddChoice = Request.Form["OddChoice"];
                    Bet bet = new Bet();
                    var userId = User.Identity.GetUserId();
                    FootballMatch footballMatch = db.FootballMatches.Find(id);
                    bet.UserId = userId;
                    bet.MatchId = id;
                    switch (oddChoice)
                    {
                        case "1":
                            bet.Odd = footballMatch.FirstTeamWinsOdd;
                            break;
                        case "2":
                            bet.Odd = footballMatch.SecondTeamWinsOdd;
                            break;
                        case "3":
                            bet.Odd = footballMatch.DrawOdd;
                            break;
                    }
                    bet.OutcomeChoice = int.Parse(oddChoice);
                    bet.Amount = Amount;
                    bet.FirstPlayer = footballMatch.FirstTeam.TeamName;
                    bet.SecondPlayer = footballMatch.SecondTeam.TeamName;
                    bet.Committed = false;
                    bet.MatchedResolved = false;
                    bet.ResolveTime = footballMatch.ResolveTime;
                    bet.FootballMatch = footballMatch;
                    db.Bets.Add(bet);
                    db.SaveChanges();
                    TempData["message"] = "Bet was added!";
                    return RedirectToAction("Index", "Match");
                }
                else
                {
                    return View(id);
                }
            }
            catch (Exception e)
            {
                return View(id);
            }
        }

        [HttpDelete]
        [Authorize(Roles = "User,Administrator")]
        public ActionResult Delete(int id)
        {
            Bet bet = db.Bets.Find(id);
            if (bet.Committed == false || bet.MatchedResolved == true)
            {
                db.Bets.Remove(bet);
                TempData["message"] = "Bet was deleted!";
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpPut]
        [Authorize(Roles = "User,Administrator")]
        public void commitBet(int id)
        {
            if (ModelState.IsValid)
            {
                Bet bet = db.Bets.Find(id);
                if (TryUpdateModel(bet))
                {
                    bet.Committed = true;
                    db.SaveChanges();
                }
            }
        }

        [HttpPut]
        [Authorize(Roles = "User,Administrator")]
        public void spendBetcoins(double amount)
        {
            if (ModelState.IsValid)
            {
                var user = User.Identity.GetUserId();
                Profile profile = db.Profiles.Find(user);
                if (TryUpdateModel(profile))
                {
                    profile.Betcoins = profile.Betcoins - amount;
                    profile.AmountBetSoFar = profile.AmountBetSoFar + amount;
                    db.SaveChanges();
                }
            }
        }


        [Authorize(Roles = "User,Administrator")]
        public ActionResult Payment()
        {
            var user = User.Identity.GetUserId();
            var profile = db.Profiles.Find(user);
            var bets = db.Bets.Where(x => (x.UserId == user && x.Committed == false && x.FootballMatch.Outcome == "0"));
            var totalSpent = 0.0;
            foreach (var bet in bets)
            {
                totalSpent = totalSpent + bet.Amount;
            }
            var betcoins = profile.Betcoins;
            if (totalSpent > betcoins)
            {
                return RedirectToAction("Index");
            }

            IList<Bet> betList = bets.ToList();
            foreach (Bet bet in betList)
            {
                var match = db.FootballMatches.Find(bet.MatchId);
                if (match.Outcome == "0")
                {
                    commitBet(bet.BetId);
                }
            }

            spendBetcoins(totalSpent);


            return RedirectToAction("Index", "Match");
        }

        [Authorize(Roles = "User,Administrator")]
        public ActionResult History()
        {
            var user = User.Identity.GetUserId();
            var betsWait = db.Bets.Where(x => (x.UserId == user && x.Committed == true && x.MatchedResolved == false));
            ViewBag.BetsInWaiting = betsWait;
            var betsDone = db.Bets.Where(x => (x.UserId == user && x.MatchedResolved == true));
            ViewBag.BetsConcluded = betsDone;
            return View();
        }
    }
}