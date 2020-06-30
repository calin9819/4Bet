using _4bet.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace _4bet.Controllers
{
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        [Authorize(Roles = "User,Administrator")]
        public ActionResult Index()
        {
            var currentUser = User.Identity.GetUserId();
            var profile = db.Profiles.Find(currentUser);
            ViewBag.DisplayName = profile.DisplayName;
            ViewBag.Betcoins = profile.Betcoins;
            ViewBag.UserId = profile.UserId;
            var isAdmin = User.IsInRole("Administrator");
            var config = db.Configs.Find(1);
            ViewBag.AutoPlay = config.AutoPlay;
            ViewBag.isAdmin = isAdmin;
            if (config.AutoPlay)
            {
                var now = DateTime.Now;
                var matches = db.FootballMatches.Where(x => now >= x.ResolveTime && x.Outcome == "0");
                if (matches.Count() > 0)
                {
                    AutoPlay(matches);
                }
            }
            return View();
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult Users()
        {
            var users = db.Users.ToList();
            ViewBag.Users = users;
            return View();
        }

        [Authorize(Roles = "User,Administrator")]
        public ActionResult New()
        {
            var currentUser = User.Identity.GetUserId();
            var profile = db.Profiles.Find(currentUser);
            if (profile != null)
            {
                return Redirect("/Home/Index");
            }

            return View();
        }


        [HttpPost]
        public ActionResult New(Profile profile)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    profile.UserId = User.Identity.GetUserId();
                    profile.Betcoins = 0;
                    db.Profiles.Add(profile);
                    db.SaveChanges();
                    TempData["message"] = "Profile was saved!";
                    return RedirectToAction("Index");
                }
                else
                {
                    return View(profile);
                }
            }
            catch (Exception e)
            {
                return View(profile);
            }
        }

        [Authorize(Roles = "User,Administrator")]
        public ActionResult Edit(string id)
        {
            Profile profile = db.Profiles.Find(id);
            return View(profile);
        }

        [HttpPut]
        public ActionResult Edit(string id, Profile requestProfile)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Profile profile = db.Profiles.Find(id);
                    if (TryUpdateModel(profile))
                    {
                        profile.DisplayName = requestProfile.DisplayName;
                        TempData["message"] = "Profile was changed!";
                        db.SaveChanges();
                    }
                    return RedirectToAction("Index");
                }
                else
                {
                    return View(requestProfile);
                }
            }
            catch (Exception e)
            {
                return View(requestProfile);
            }
        }

        [Authorize(Roles = "User,Administrator")]
        public ActionResult AddBetcoins(string id)
        {
            var profile = db.Profiles.Find(id);
            ViewBag.Betcoins = profile.Betcoins;
            return View(profile);
        }


        [HttpPut]
        public ActionResult AddBetcoins(string id, int newBetcoins)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Profile profile = db.Profiles.Find(id);
                    if (TryUpdateModel(profile))
                    {
                        var newTotal = profile.Betcoins + newBetcoins;
                        profile.Betcoins = newTotal;
                        TempData["message"] = "Betcoins successfully added!";
                        db.SaveChanges();
                    }
                    return RedirectToAction("Index");
                } else
                {
                    return View(newBetcoins);
                }
            }
            catch (Exception e)
            {
                return View(newBetcoins);
            }
        }

        [Authorize(Roles = "User,Administrator")]
        public ActionResult CashOut(string id)
        {
            var profile = db.Profiles.Find(id);
            ViewBag.Betcoins = profile.Betcoins;
            string message = "";
            if (TempData.ContainsKey("message"))
                message = TempData["message"].ToString();
            if (message.Length > 0)
                ViewBag.Message = message;
            bool hasMessage = false;
            if (message.Length > 0)
                hasMessage = true;
            ViewBag.HasMessage = hasMessage;
            return View(profile);
        }

        [HttpPut]
        public ActionResult CashOut(string id, int amount)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Profile profile = db.Profiles.Find(id);
                    if (amount <= profile.Betcoins)
                    {
                        if (TryUpdateModel(profile))
                        {
                            var newTotal = profile.Betcoins - amount;
                            profile.Betcoins = newTotal;
                            db.SaveChanges();
                        }
                    }
                    return RedirectToAction("Index");
                }
                else
                {
                    return View(amount);
                }
            }
            catch (Exception e)
            {
                return View(amount);
            }
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult ChangeAutoplay()
        {
            Config config = db.Configs.Find(1);
            if (TryUpdateModel(config))
            {
                config.AutoPlay = !config.AutoPlay;
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "User,Administrator")]
        public ActionResult AutoPlay(IQueryable<FootballMatch> matches)
        {
            var config = db.Configs.Find(1);
            if (!config.AutoPlay)
            {
                return RedirectToAction("Index");
            }
            IList<FootballMatch> matchList = matches.ToList();
            foreach (var match in matchList)
            {
                playMatch(match);
            }
            return RedirectToAction("Index");
        }

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
                }
                else
                {
                    addToPrizePool(bet.Amount);
                }
            }
        }

        [HttpPut]
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
        public void updateMatch(int matchId, string result)
        {
            var match = db.FootballMatches.Find(matchId);
            if (TryUpdateModel(match))
            {
                match.Outcome = result;
                db.SaveChanges();
            }
        }

        public void playMatch(FootballMatch match)
        {
            var odd1 = match.FirstTeamWinsOdd;
            var odd2 = match.SecondTeamWinsOdd;
            var odd3 = match.DrawOdd;
            var oddSum = odd1 + odd2 + odd3;
            var result = "0";
            odd1 = odd1 / oddSum;
            odd2 = odd2 / oddSum;
            odd3 = odd3 / oddSum;
            odd1 = (1 - odd1) / 2;
            odd2 = (1 - odd2) / 2;
            odd3 = (1 - odd3) / 2;
            var random = new Random();
            double num = random.Next(0, 100);
            num = num / 100.00;
            if (num < odd1)
            {
                result = "1";
            }
            else if (num < odd1 + odd2)
            {
                result = "2";
            }
            else
                result = "3";
            updateMatch(match.FootballMatchId, result);
            resolveBets(match.FootballMatchId);
        }
    }

    
}