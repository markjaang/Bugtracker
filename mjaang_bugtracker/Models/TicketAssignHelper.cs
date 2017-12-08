//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;

//namespace mjaang_bugtracker.Models
//{
//    public class TicketAssignHelper
//    {
//        private ApplicationDbContext db = new ApplicationDbContext();

//        // boolean- check to see if a user is assined to ticket
//        public bool IsUserOnTicket(string userId, int ticketId)
//        {
//            var ticket = db.Ticket.Find(ticketId);
//            var flag = ticket.AssignedTo.Equals(userId);
//            return (flag);
//        }
        
//        // assign a user if not already assigned
//        public void AddUserToTicket(string userId, int ticketId)
//        {
//            ApplicationUser user = db.Users.Find(userId);
//            Tickets ticekts = db.Ticket.Find(ticketId);
//            ticket
                 
//        }
       
//        // remove from project if already assigned
//        public void RemoveUserFromTicket(string userId, int ticketId)
//        {
//            ApplicationUser user = db.Users.Find(userId);
//            Tickets tickets = db.Ticket.Find(ticketId);
//            tickets.CreatedBy.Remove(user);
//            db.SaveChanges();
//        }

//        // list all projects for a given user
//        public List<Projects> ListUserProjects(string userId)
//        {
//            ApplicationUser user = db.Users.Find(userId);
//            return user.Project.ToList();
//        }

//        // list all users for a given project
//        public List<ApplicationUser> ListUsersOnProject(int projectId)
//        {
//            Projects project = db.Project.Find(projectId);
//            return project.User.ToList();
//        }

//        // list all users NOT on a project
//        public List<ApplicationUser> ListUsersNotOnProject(int projectId)
//        {
//            Projects project = db.Project.Find(projectId);
//            var userIDs = project.User;
//            return project.User.Where(u => !userIDs.Contains(u)).ToList();
//        }
//    }
//}
