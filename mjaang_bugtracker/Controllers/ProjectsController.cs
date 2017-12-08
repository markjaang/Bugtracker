using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using mjaang_bugtracker.Models;
using PagedList;
using Microsoft.AspNet.Identity;

namespace mjaang_bugtracker.Controllers
{
    public class ProjectsController : MyBaseController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Projects
        [Authorize]
        public ActionResult Index()
        {
            var projectuser = db.Users.Find(User.Identity.GetUserId());

            if (User.IsInRole("Admin"))
            {
                var adminproject = db.Project.OrderBy(a => a.Id).ToList();
                return View(adminproject);
            }
            else
            {
                var userproject = projectuser.Project.OrderBy(u => u.Id).ToList();
                return View(userproject);
            }
        }

        // GET: Projects/Details/5
        [Authorize]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Projects projects = db.Project.Find(id);
            if (projects == null)
            {
                return HttpNotFound();
            }
            return View(projects);
        }

        // GET: Projects/Create
        [Authorize (Roles = "Admin, Manager")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Projects/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize (Roles = "Admin, Manager")]
        public ActionResult Create([Bind(Include = "Id,Title,Body,Created")] Projects projects)
        {
            if (ModelState.IsValid)
            {
                var id = User.Identity.GetUserId();
                ApplicationUser projectuser = db.Users.FirstOrDefault(u => u.Id.Equals(id));

                projects.AuthorUserId = projectuser.UserName;
                projects.Created = System.DateTimeOffset.Now;
                db.Project.Add(projects);
                db.SaveChanges();

                ApplicationUser attachUser = db.Users.FirstOrDefault(u => u.UserName.Equals(User.Identity.Name));
                Projects attachProject = db.Project.Find(projects.Id);
                ProjectAssignHelper helper = new ProjectAssignHelper();
                helper.AddUserToProject(attachUser.Id, attachProject.Id);

                return RedirectToAction("Index");
            }

            return View(projects);
        }

        // GET: Projects/Edit/5
        [Authorize(Roles = "Admin, Manager")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Projects projects = db.Project.Find(id);
            if (projects == null)
            {
                return HttpNotFound();
            }

            projects.Updated = System.DateTimeOffset.Now;
            return View(projects);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize (Roles ="Admin, Manager")]
        public ActionResult Edit([Bind(Include = "Id,Title,Body,Created,Updated")] Projects projects)
        {
            if (ModelState.IsValid)
            {
                projects.Updated = System.DateTimeOffset.Now;
                db.Project.Attach(projects);
                db.Entry(projects).Property("Updated").IsModified = true;
                db.Entry(projects).Property("Title").IsModified = true;
                db.Entry(projects).Property("Body").IsModified = true;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(projects);
        }

        // GET: Projects/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Projects projects = db.Project.Find(id);
            if (projects == null)
            {
                return HttpNotFound();
            }
            return View(projects);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize (Roles ="Admin")]
        public ActionResult DeleteConfirmed(int id)
        {
            Projects projects = db.Project.Find(id);
            
            db.Project.Remove(projects);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        //GET: Project/AssignUsers
        [Authorize(Roles = "Admin, Manager")]
        public ActionResult ProjectUser(int id)
        {
            var project = db.Project.Find(id);   //Find project
            ProjectUserViewModel ProjectUser = new ProjectUserViewModel();  //construct a new object of view model
            var selected = project.User.Select(u => u.Id).ToArray();     //get selected user ids to array
            ProjectUser.Users = new MultiSelectList(db.Users.ToList(), "Id", "FirstName", selected);   // construct a new multiselect list containing id and first name of users
            ProjectUser.Id = project.Id;   //get project id and send it to view
            ProjectUser.FirstName = project.Title;   //get project title and send it to view
            ProjectUser.LastName = project.Title;

            return View(ProjectUser);   // return view action
        }

        //POST: Project/AssignUsers
        [HttpPost]
        [Authorize(Roles = "Admin, Manager")]
        public ActionResult ProjectUser(ProjectUserViewModel model)
        {
            var project = db.Project.Find(model.Id);  //find project id in the view model
            ProjectAssignHelper helper = new ProjectAssignHelper();  //construct a new object of project assign helper
            foreach (var user in db.Users.Select(r => r.Id).ToList())  // loop all users in the selected user id db and send them to list
            {
                helper.RemoveUserFromProject(user, project.Id); //remove the user from the specific project id
            }

            foreach (var user in model.SelectedUsers)   // loop all users in selected users in model
            {
                helper.AddUserToProject(user, project.Id);  // add the user to the specific project id
            }
            return RedirectToAction("Index", "Projects");
        }
    }
}