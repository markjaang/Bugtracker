using mjaang_bugtracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace mjaang_bugtracker.Controllers
{
    [RequireHttps]
    public class HomeController : MyBaseController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Landing()
        {
            return View();
        }

        public ActionResult Index()
        {
            return View(db.Project.ToList());
        }

        
    }
}