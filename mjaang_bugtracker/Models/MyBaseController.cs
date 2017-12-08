using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace mjaang_bugtracker.Models
{
    public class MyBaseController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = db.Users.Find(User.Identity.GetUserId());
                ViewBag.Notifications = user.TicketNotifications.OrderByDescending(u => u.Created).ToList();
                ViewBag.FirstName = user.FirstName;
                ViewBag.LastName = user.LastName;
                base.OnActionExecuting(filterContext);
            }
        }
    }
}
