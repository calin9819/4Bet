using _4bet.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace _4bet.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext dbEntities = new ApplicationDbContext();
        [Authorize(Roles = "User,Administrator")]
        public ActionResult Index()
        {
            var currentUser = User.Identity.GetUserId();
            var profile = dbEntities.Profiles.Find(currentUser);
            if (profile == null)
            {
                return RedirectToAction("New", "Profile");
            }
            return RedirectToAction("Index", "Profile");
        }
    }
}