using Microsoft.AspNet.Identity;
using mjaang_bugtracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace mjaang_bugtracker.Controllers
{
    public class AdminController : MyBaseController
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Admin
        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            List<AdminUserListModel> users = new List<AdminUserListModel>();
            UserRolesHelper helper = new UserRolesHelper(db);
            foreach (var user in db.Users.ToList())
            {
                var eachUser = new AdminUserListModel();
                eachUser.roles = new List<string>();
                eachUser.user = user;
                eachUser.roles = helper.ListUserRoles(user.Id).ToList();

                users.Add(eachUser);
            }
            return View(users);
        }

        //GET: UserRoleAdmin
        [Authorize(Roles = "Admin")]
        public ActionResult UserRole(string id)
        {
            var user = db.Users.Find(id);
            var AdminModel = new UserRoleViewModel();
            UserRolesHelper helper = new UserRolesHelper(db);
            AdminModel.SelectedRoles = helper.ListUserRoles(id).ToArray();
            AdminModel.Roles = new MultiSelectList(db.Roles, "Name", "Name", AdminModel.SelectedRoles);
            AdminModel.Id = user.Id;
            AdminModel.FirstName = user.FirstName;
            AdminModel.LastName = user.LastName;

            return View(AdminModel);
        }
        //POST: UserRoleAdmin
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult UserRole(UserRoleViewModel model)
        {
            var user = db.Users.Find(model.Id);
            UserRolesHelper helper = new UserRolesHelper(db);
            foreach (var role in db.Roles.Select(r => r.Name).ToList())
            {
                helper.RemoveUserFromRole(user.Id, role);
            }

            foreach (var role in model.SelectedRoles)
            {
                helper.AddUserToRole(user.Id, role);
            }
            return RedirectToAction("Index", "Admin");
        }


        // POST: Tickets/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteUser()
        {
            ApplicationUser user = db.Users.Find(User.Identity.GetUserId());
            db.Users.Remove(user);
            db.SaveChanges();
            return RedirectToAction("Index", "Admin");

        }
    }
}
