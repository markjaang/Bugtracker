using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace mjaang_bugtracker.Models
{
    public class ProjectAssignHelper
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // boolean- check to see if a user is assined to project
        public bool IsUserOnProject(string userId, int projectId)
        {
            var project = db.Project.Find(projectId);
            var flag = project.User.Any(u => u.Id == userId);
            return (flag);
        }
        
        // assign a user if not already assigned
        public void AddUserToProject(string userId, int projectId)
        {
            ApplicationUser user = db.Users.Find(userId);
            Projects project = db.Project.Find(projectId);
            project.User.Add(user);
            db.SaveChanges();          
        }
       
        // remove from project if already assigned
        public void RemoveUserFromProject(string userId, int projectId)
        {
            ApplicationUser user = db.Users.Find(userId);
            Projects project = db.Project.Find(projectId);
            project.User.Remove(user);
            db.SaveChanges();
        }

        // list all projects for a given user
        public List<Projects> ListUserProjects(string userId)
        {
            ApplicationUser user = db.Users.Find(userId);
            return user.Project.ToList();
        }

        // list all users for a given project
        public List<ApplicationUser> ListUsersOnProject(int projectId)
        {
            Projects project = db.Project.Find(projectId);
            return project.User.ToList();
        }

        // list all users NOT on a project
        public List<ApplicationUser> ListUsersNotOnProject(int projectId)
        {
            Projects project = db.Project.Find(projectId);
            var userIDs = project.User;
            return project.User.Where(u => !userIDs.Contains(u)).ToList();
        }
    }
}
