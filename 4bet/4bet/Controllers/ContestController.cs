using _4bet.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace _4bet.Controllers
{
    public class ContestController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();
        [Authorize(Roles = "User,Administrator")]
        public ActionResult Index()
        {
            var config = db.Configs.Find(1);
            ViewBag.HasContest = false;
            if (config.ContestEnabled)
            {
                
                ViewBag.HasContest = true;
                var users = db.Profiles.OrderByDescending(m => m.AmountBetSoFar);
                ViewBag.Users = users;
                if (config.ContestOver)
                {
                    ViewBag.Winner = users.ToList().First();
                }
                var currentUserId = User.Identity.GetUserId();
                var currentUser = db.Profiles.Find(currentUserId);
                var userPlace = users.ToList().IndexOf(currentUser);
                ViewBag.CurrentUser = currentUser;
                ViewBag.UserPlace = userPlace + 1;
                ViewBag.PrizePool = config.PrizePool;
                ViewBag.ContestOver = config.ContestOver;
            }
            var isAdmin = User.IsInRole("Administrator");
            ViewBag.IsAdmin = isAdmin;
            return View();
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult StartContest()
        {
            Config config = db.Configs.Find(1);
            if (TryUpdateModel(config))
            {
                config.ContestEnabled = true;
                config.ContestOver = false;
                config.PrizePool = 500;
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult EndContest()
        {
            Config config = db.Configs.Find(1);
            if (TryUpdateModel(config))
            {
                config.ContestEnabled = true;
                config.ContestOver = true;
                var users = db.Profiles.OrderByDescending(m => m.AmountBetSoFar);
                var winner = users.ToList().First();
                awardWinner(winner.UserId, config.PrizePool);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult DeleteContest()
        {
            Config config = db.Configs.Find(1);
            if (TryUpdateModel(config))
            {
                config.ContestEnabled = false;;
                config.ContestOver = false;
                config.PrizePool = 0;
                resetBetcoinCounters();
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpPut]
        [Authorize(Roles = "Administrator")]
        public void awardWinner(string winnerId, double amount)
        {
            if (ModelState.IsValid)
            {
                Profile profile = db.Profiles.Find(winnerId);
                if (TryUpdateModel(profile))
                {
                    profile.Betcoins = profile.Betcoins + amount;
                    db.SaveChanges();
                }
            }
        }

        [Authorize(Roles = "Administrator")]
        public void resetBetcoinCounters()
        {
            var profiles = db.Profiles;
            IList<Profile> profilesList = profiles.ToList();
            foreach (Profile profile in profilesList)
            {
                resetCounter(profile.UserId);
            }
        }

        [HttpPut]
        [Authorize(Roles = "Administrator")]
        public void resetCounter(string userId)
        {
            if (ModelState.IsValid)
            {
                Profile profile = db.Profiles.Find(userId);
                if (TryUpdateModel(profile))
                {
                    profile.AmountBetSoFar = 0;
                    db.SaveChanges();
                }
            }
        }
    }
}