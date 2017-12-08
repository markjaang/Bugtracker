using Microsoft.AspNet.Identity;
using mjaang_bugtracker.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;


namespace mjaang_bugtracker.Controllers
{
    public class TicketsController : MyBaseController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Tickets/
        [Authorize]
        public ActionResult Index(int? id)
        {
            var user = db.Users.Find(User.Identity.GetUserId()); //Find the current user
            IEnumerable<Tickets> pList;                          //receiving all the tickets and store them in the variable pList
            if (id != null)                                      //if you go through the project id
            {
                var project = db.Project.Find(id);               // Find all the projects



                pList = project.Ticket.ToList();                 //get all the tickets only under the specific project and store them in the pList
            }
            else
            {                                              // if not going throgh the projcet(going to the ticket index page directly)
                if(User.IsInRole("Admin"))                       
                {
                    pList = db.Ticket;                      //get all the tickets
                } else 
                {
                pList = user.Project.SelectMany(p => p.Ticket);

                }
            }
               
                
            if (User.IsInRole("Submitter") || User.IsInRole("Developer"))       //now checking for the role for submitter and developer
            {
                pList = pList.Where(s => s.AssignedToId == user.Id || s.CreatedById == user.Id).ToList();    //get assigned and created tickets. submitters don't get the tickets assigned so it's fine to use for both
            }

            return View(pList.ToList());

        }

        // GET: Tickets/Details/5
        [Authorize]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tickets tickets = db.Ticket.Find(id);

            if (tickets == null)
            {
                return HttpNotFound();
            }
            return View(tickets);
        }

        //POST:TicketAttachment/Create/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<ActionResult> CreateAttach([Bind(Include = "Id, TicketId, AssignedToId, CreatedById, TicketAttachment")] Tickets tickets, TicketAttachments file, IEnumerable<HttpPostedFileBase> upload, string description)
        {
            var oldticket = db.Ticket.Find(tickets.Id);
            if (ModelState.IsValid)
            {
                ApplicationUser user = db.Users.FirstOrDefault(u => u.UserName.Equals(User.Identity.Name));
                var userid = user.Id;
                foreach (var files in upload)
                {
                    if (files != null && files.ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(files.FileName);
                        string guidName = string.Format(Guid.NewGuid() + Path.GetExtension(files.FileName));
                        files.SaveAs(Path.Combine(Server.MapPath("/img/ticket/"), guidName));
                        file.CreatedName = fileName;
                        file.FileUrl = (Path.Combine("/img/ticket/", guidName));
                        file.TicketId = tickets.Id;
                        file.UserId = User.Identity.GetUserId();
                        file.Description = description;
                        file.Created = System.DateTimeOffset.Now;
                        db.TicketAttachment.Add(file);

                        db.SaveChanges();


                        db.Ticket.Include(t => t.AssignedTo).Include(t => t.CreatedBy).FirstOrDefault(t => t.Id == tickets.Id);

                        if (userid != oldticket.AssignedToId || userid != oldticket.CreatedById)
                        {
                            if (oldticket.AssignedToId != null || oldticket.CreatedById != null)
                            {
                                if (oldticket.CreatedById == null)
                                {
                                    var dev = oldticket.AssignedTo.Email;
                                    var owner = db.Users.Find(userid).UserName;
                                    var web = "https://mjaangbugtracker.azurewebsites.net/Tickets/Details/" + oldticket.Id;
                                    var email = new EmailService();
                                    var mail = new IdentityMessage
                                    {
                                        Subject = "Notification regarding your ticket from bug tracker",
                                        Destination = dev,
                                        Body = $"Hi {oldticket.AssignedTo.FirstName}! A attachment  was added to a ticket you are currently assigned ('{oldticket.Title}') by {owner}. Click below to view the attachment: {web}"
                                    };
                                    await email.SendAsync(mail);
                                }
                                else
                                {
                                    var dev = oldticket.CreatedBy.Email;
                                    var owner = db.Users.Find(userid).UserName;
                                    var web = "https://mjaangbugtracker.azurewebsites.net/Tickets/Details/" + oldticket.Id;
                                    var email = new EmailService();
                                    var mail = new IdentityMessage
                                    {
                                        Subject = "Notification regarding your ticket from bug tracker",
                                        Destination = dev,
                                        Body = $"Hi {oldticket.CreatedBy.FirstName}! A attachment  was added to a ticket you are currently assigned ('{oldticket.Title}') by {owner}. Click below to view the attachment: {web}"
                                    };
                                    await email.SendAsync(mail);
                                }
                            }

                            TicketNotification tf1 = new TicketNotification
                            {
                                UserId = oldticket.AssignedToId,
                                TicketId = oldticket.Id,
                                Description = "Ticket Attachment Created",
                                Created = System.DateTimeOffset.Now,
                                Type = "Attachment"
                            };
                            
                            db.TicketNotifications.Add(tf1);
                        }

                        TicketHistories th1 = new TicketHistories
                        {
                            TicketId = tickets.Id,
                            Property = "TicketAttachment",
                            NewValue = file.CreatedName,
                            Updated = file.Created,
                            UserId = userid
                        };
                        db.TicketHistory.Add(th1);

                        db.SaveChanges();
                    }
                }
            }

        return RedirectToAction("Details", "Tickets", new { id = tickets.Id });
    }

        // POST: TicketAttachment/Delete/5
        [Authorize]
        public ActionResult DeleteAttach(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var attachment = db.TicketAttachment.Find(id);
            if (attachment == null)
            {
                return HttpNotFound();
            }

                var ticket = db.Ticket.Where(u => u.TicketAttachment.Any(a => a.Id == id)).FirstOrDefault();

                db.TicketAttachment.Remove(attachment);
                db.SaveChanges();
                return RedirectToAction("Details", "Tickets", new { id = ticket.Id });
        }


        // GET: Tickets/Create
        [Authorize]
        public ActionResult Create()
        {
            if(User.IsInRole("Admin"))
            {
                ViewBag.ProjectId = new SelectList(db.Project, "Id", "Title");
                ViewBag.TicketPriorityId = new SelectList(db.TicketPriority, "Id", "Name");
                ViewBag.TicketTypeId = new SelectList(db.TicketType, "Id", "Name");
            }
            else
            {
                var projectuser = db.Users.Find(User.Identity.GetUserId());
                var userproject = projectuser.Project.OrderBy(u => u.Id).ToList();

                ViewBag.ProjectId = new SelectList(userproject, "Id", "Title");
                ViewBag.TicketPriorityId = new SelectList(db.TicketPriority, "Id", "Name");
                ViewBag.TicketTypeId = new SelectList(db.TicketType, "Id", "Name");
            }
            
            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Create([Bind(Include = "Id,Title,Description,Created,ProjectId,TicketTypeId,TicketPriorityId,TicketStatusId,CreatedById")] Tickets tickets)
        {
            ApplicationUser user = db.Users.FirstOrDefault(u => u.UserName.Equals(User.Identity.Name));
            var userid = user.Id;

            if (ModelState.IsValid)
            {
                var id = User.Identity.GetUserId();
                var status = db.TicketStatus.FirstOrDefault(s => s.Name == "Unassigned");
                tickets.CreatedById = id;
                tickets.TicketStatusId = status.Id;

                tickets.Created = System.DateTimeOffset.Now;
                db.Ticket.Add(tickets);
                db.SaveChanges();

                

                TicketHistories th1 = new TicketHistories
                {
                    TicketId = tickets.Id,
                    Property = "Description",
                    NewValue = tickets.Description,
                    Updated = tickets.Created,
                    UserId = userid
                };
                db.TicketHistory.Add(th1);
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            if (User.IsInRole("Admin"))
            {
                ViewBag.ProjectId = new SelectList(db.Project, "Id", "Title");
                ViewBag.TicketPriorityId = new SelectList(db.TicketPriority, "Id", "Name");
                ViewBag.TicketTypeId = new SelectList(db.TicketType, "Id", "Name");
            }
            else
            {
                var projectuser = db.Users.Find(User.Identity.GetUserId());
                var userproject = projectuser.Project.OrderBy(u => u.Id).ToList();

                ViewBag.ProjectId = new SelectList(userproject, "Id", "Title");
                ViewBag.TicketPriorityId = new SelectList(db.TicketPriority, "Id", "Name");
                ViewBag.TicketTypeId = new SelectList(db.TicketType, "Id", "Name");
            }

            return View(tickets);
        }


        // GET: Tickets/Edit/5
        [Authorize(Roles = "Admin, Manager, Developer, Demo")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tickets tickets = db.Ticket.Find(id);
            if (tickets == null)
            {
                return HttpNotFound();
            }

            ApplicationDbContext context = new ApplicationDbContext();

            if (User.IsInRole("Manager"))
            {
                Projects project = db.Project.Find(tickets.ProjectId);
                var projectUsers = context.Users.Where(u => u.Project.Any(p => p.Title == project.Title));

                var role = context.Roles.SingleOrDefault(u => u.Name == "Developer");
                var displayUsers = project.User.Where(u => u.Roles.Any(r => r.RoleId == role.Id));

                ViewBag.AssignedToId = new SelectList(displayUsers, "Id", "FirstName", tickets.AssignedToId);
                ViewBag.TicketPriorityId = new SelectList(db.TicketPriority, "Id", "Name", tickets.TicketPriorityId);
                ViewBag.TicketTypeId = new SelectList(db.TicketType, "Id", "Name", tickets.TicketTypeId);
                ViewBag.TicketStatusId = new SelectList(db.TicketStatus, "Id", "Name", tickets.TicketStatusId);

                tickets.Updated = System.DateTimeOffset.Now;
            }
            else
            {
                ViewBag.AssignedToId = new SelectList(db.Users, "Id", "FirstName", tickets.AssignedToId);
                ViewBag.TicketPriorityId = new SelectList(db.TicketPriority, "Id", "Name", tickets.TicketPriorityId);
                ViewBag.TicketTypeId = new SelectList(db.TicketType, "Id", "Name", tickets.TicketTypeId);
                ViewBag.TicketStatusId = new SelectList(db.TicketStatus, "Id", "Name", tickets.TicketStatusId);

                tickets.Updated = System.DateTimeOffset.Now;

            }

            return View(tickets);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize (Roles ="Admin, Manager, Developer, Demo")]
        public async Task <ActionResult> Edit([Bind(Include = "Id,Title,Description,Updated,ProjectId,TicketTypeId,TicketPriorityId,TicketStatusId,AssignedToId")] Tickets tickets)
        {
            if (ModelState.IsValid)
            {
                tickets.Updated = System.DateTimeOffset.Now;
                db.Ticket.Attach(tickets);
                var oldticket = db.Ticket.AsNoTracking().FirstOrDefault(t => t.Id == tickets.Id);
                var userid = User.Identity.GetUserId();

                db.Entry(tickets).Property("Title").IsModified = true;
                db.Entry(tickets).Property("Description").IsModified = true;
                db.Entry(tickets).Property("AssignedToId").IsModified = true;
                db.Entry(tickets).Property("TicketPriorityId").IsModified = true;
                db.Entry(tickets).Property("TicketTypeId").IsModified = true;
                db.Entry(tickets).Property("TicketStatusId").IsModified = true;
                db.Entry(tickets).Property("Updated").IsModified = true;

                db.SaveChanges();

                db.Ticket.Include(t => t.AssignedTo).Include(t => t.CreatedBy).FirstOrDefault(t => t.Id == tickets.Id);

                if (userid != oldticket.AssignedToId || userid != oldticket.CreatedById)
                {
                    if (oldticket.AssignedToId != null || oldticket.CreatedById != null)
                    {
                        if (oldticket.CreatedById == null)
                        {
                            var dev = tickets.AssignedTo.Email;
                            var user = db.Users.Find(userid).UserName;
                            var web = "https://mjaangbugtracker.azurewebsites.net/Tickets/Details/" + tickets.Id;
                            var email = new EmailService();
                            var mail = new IdentityMessage
                            {
                                Subject = "Notification regarding your ticket from bug tracker",
                                Destination = dev,
                                Body = $"Hi {tickets.AssignedTo.FirstName}! A ticket you are currently assigned ('{tickets.Title}') has been updated by {user}. Click below to view the changes: {web}"
                            };
                            await email.SendAsync(mail);
                        }
                        else
                        {
                        var dev = oldticket.CreatedBy.Email;
                        var owner = db.Users.Find(userid).UserName;
                        var web = "https://mjaangbugtracker.azurewebsites.net/Tickets/Details/" + oldticket.Id;
                        var email = new EmailService();
                        var mail = new IdentityMessage
                        {
                            Subject = "Notification regarding your ticket from bug tracker",
                            Destination = dev,
                            Body = $"Hi {oldticket.CreatedBy.FirstName}! A attachment  was added to a ticket you are currently assigned ('{oldticket.Title}') by {owner}. Click below to view the attachment: {web}"
                        };
                        await email.SendAsync(mail);
                        }
                    }
                    TicketNotification tf1 = new TicketNotification
                    {
                        UserId = tickets.AssignedToId,
                        TicketId = tickets.Id,
                        Description = "Ticket Edited",
                        Created = System.DateTimeOffset.Now,
                        Type = "TicketEdit"
                    };
                    db.TicketNotifications.Add(tf1);
                }

                if (oldticket?.Description != tickets.Description)
                {
                    TicketHistories th1 = new TicketHistories
                    {
                        TicketId = tickets.Id,
                        Property = "Description",
                        OldValue = oldticket?.Description,
                        NewValue = tickets.Description,
                        Updated = tickets.Updated,
                        UserId = userid
                    };
                    db.TicketHistory.Add(th1);
               
                }
                ApplicationUser olduser = db.Users.FirstOrDefault(u => u.Id.Equals(oldticket.AssignedToId));
                ApplicationUser newuser = db.Users.FirstOrDefault(u => u.Id.Equals(tickets.AssignedToId));

                if (olduser != null)
                {
                    if (oldticket?.AssignedToId != tickets.AssignedToId)
                    {
                        TicketHistories th2 = new TicketHistories
                        {
                            TicketId = tickets.Id,
                            Property = "AssignedTo",
                            OldValue = olduser.UserName,
                            NewValue = newuser.UserName,
                            Updated = tickets.Updated,
                            UserId = userid
                        };
                        db.TicketHistory.Add(th2);

                    }
                }
                else
                {
                    TicketHistories th2 = new TicketHistories
                    {
                        TicketId = tickets.Id,
                        Property = "AssignedTo",
                        NewValue = newuser.UserName,
                        Updated = tickets.Updated,
                        UserId = userid
                    };
                    db.TicketHistory.Add(th2);
                }

                if (oldticket?.Title != tickets.Title)
                {
                    TicketHistories th3 = new TicketHistories
                    {
                        TicketId = tickets.Id,
                        Property = "Title",
                        OldValue = oldticket?.Title,
                        NewValue = tickets.Title,
                        Updated = tickets.Updated,
                        UserId = userid
                    };
                    db.TicketHistory.Add(th3);
                 
                }

                if (oldticket?.TicketPriorityId != tickets.TicketPriorityId)
                {
                    TicketHistories th6 = new TicketHistories
                    {
                        TicketId = tickets.Id,
                        Property = "TicketPriority",
                        OldValue = oldticket?.TicketPriority.Name,
                        NewValue = db.TicketPriority.FirstOrDefault(t => t.Id == tickets.TicketPriorityId).Name,
                        Updated = tickets.Updated,
                        UserId = userid
                    };
                    db.TicketHistory.Add(th6);

                }

                if (oldticket?.TicketTypeId != tickets.TicketTypeId)
                {
                    TicketHistories th7 = new TicketHistories
                    {
                        TicketId = tickets.Id,
                        Property = "TicketType",
                        OldValue = oldticket?.TicketType.Name,
                        NewValue = db.TicketType.FirstOrDefault(t => t.Id == tickets.TicketTypeId).Name,
                        Updated = tickets.Updated,
                        UserId = userid
                    };
                    db.TicketHistory.Add(th7);

                }

                if (oldticket?.TicketStatusId != tickets.TicketStatusId)
                {
                    TicketHistories th8 = new TicketHistories
                    {
                        TicketId = tickets.Id,
                        Property = "TicketStatus",
                        OldValue = oldticket?.TicketStatus.Name,
                        NewValue = db.TicketStatus.FirstOrDefault(t => t.Id == tickets.TicketStatusId).Name,
                        Updated = tickets.Updated,
                        UserId = userid
                    };
                    db.TicketHistory.Add(th8);

                }

                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.AssignedToId = new SelectList(db.Users, "Id", "FirstName", tickets.AssignedToId);
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriority, "Id", "Name", tickets.TicketPriorityId);
            ViewBag.TicketTypeId = new SelectList(db.TicketType, "Id", "Name", tickets.TicketTypeId);
            ViewBag.TicketStatusId = new SelectList(db.TicketStatus, "Id", "Name", tickets.TicketStatusId);
            return View(tickets);


        }

        // GET: Tickets/Delete/5
        [Authorize (Roles = "Admin, Manager")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tickets tickets = db.Ticket.Find(id);
            if (tickets == null)
            {
                return HttpNotFound();
            }
            return View(tickets);
        }

        // POST: Tickets/Delete/5
        [Authorize (Roles ="Admin, Manager")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Tickets tickets = db.Ticket.Find(id);

            db.Ticket.Remove(tickets);

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

        // GET: Projects/Tickets/TicketComments/Create/5
        [Authorize]
        public ActionResult CreateComments()
        {
            return View();
        }

        //POST: Projects/Tickets/TicketComments/Create/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task <ActionResult> CreateComments(Tickets ticket, string Comment)
        {
            var oldticket = db.Ticket.Find(ticket.Id);
            if (ModelState.IsValid)
            {
                var userid = User.Identity.GetUserId();

                TicketComments comment = new TicketComments();
                comment.TicketId = ticket.Id;
                comment.UserId = User.Identity.GetUserId();
                comment.Comment = Comment;
                comment.Created = System.DateTimeOffset.Now;
                
                db.TicketComment.Add(comment);
                db.SaveChanges();

                db.Ticket.Include(t => t.AssignedTo).Include(t => t.CreatedBy).FirstOrDefault(t => t.Id == ticket.Id);

                if (userid != oldticket.AssignedToId || userid != oldticket.CreatedById)
                {
                    if (oldticket.AssignedToId != null || oldticket.CreatedById != null)
                    {
                        if (oldticket.CreatedById == null)
                        {
                            var dev = ticket.AssignedTo.Email;
                            var user = db.Users.Find(userid).UserName;
                            var web = "https://mjaangbugtracker.azurewebsites.net/Tickets/Details/" + ticket.Id;
                            var email = new EmailService();
                            var mail = new IdentityMessage
                            {
                                Subject = "Notification regarding your ticket from bug tracker",
                                Destination = dev,
                                Body = $"Hi {ticket.AssignedTo.FirstName}! A ticket you are currently assigned ('{ticket.Title}') has been updated by {user}. Click below to view the changes: {web}"
                            };
                            await email.SendAsync(mail);
                        }
                        else
                        {
                            var dev = oldticket.CreatedBy.Email;
                            var owner = db.Users.Find(userid).UserName;
                            var web = "https://mjaangbugtracker.azurewebsites.net/Tickets/Details/" + oldticket.Id;
                            var email = new EmailService();
                            var mail = new IdentityMessage
                            {
                                Subject = "Notification regarding your ticket from bug tracker",
                                Destination = dev,
                                Body = $"Hi {oldticket.CreatedBy.FirstName}! A attachment  was added to a ticket you are currently assigned ('{oldticket.Title}') by {owner}. Click below to view the attachment: {web}"
                            };
                            await email.SendAsync(mail);
                        }
                    }

                    TicketNotification tf1 = new TicketNotification
                    {
                        UserId = oldticket.AssignedToId,
                        TicketId = oldticket.Id,
                        Description = "Comment Created",
                        Created = System.DateTimeOffset.Now,
                        Type = "CommentCreate"
                    };
                    db.TicketNotifications.Add(tf1);
                    }

                    TicketHistories th1 = new TicketHistories
                    {
                        TicketId = comment.TicketId,
                        Property = "TicketComment",
                        NewValue = comment.Comment,
                        Updated = comment.Created,
                        UserId = comment.UserId,
                    };
                    db.TicketHistory.Add(th1);

                    db.SaveChanges();
                }
                return RedirectToAction("Details", "Tickets", new { id = ticket.Id });
            }


        // GET: Projects/Tickets/TicketComments/Edit/5
        [Authorize (Roles = "Admin")]
        public ActionResult EditComment(int? Id)
        {
            if (Id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TicketComments comment = db.TicketComment.Find(Id);
            if (comment == null)
            {
                return HttpNotFound();
            }
            return View(comment);
        }

        // POST: Projects/Tickets/TicketComments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize (Roles = "Admin")]
        public async Task <ActionResult> EditComment([Bind(Include = "Id, TicketId, Comment, Updated")] Tickets ticket, TicketComments commentedit)
        {
            
            if (ModelState.IsValid)
            {
                commentedit.UserId = User.Identity.GetUserId();
                commentedit.Updated = System.DateTimeOffset.Now;
                db.TicketComment.Attach(commentedit);
                db.Entry(commentedit).Property("Comment").IsModified = true;
                db.Entry(commentedit).Property("Updated").IsModified = true;

                var oldcomment = db.TicketComment.AsNoTracking().FirstOrDefault(c => c.Id == commentedit.Id);
                var userid = User.Identity.GetUserId();

                db.SaveChanges();
                var oldticket = db.Ticket.Find(ticket.Id);
                db.Ticket.Include(t => t.AssignedTo).Include(t => t.CreatedBy).FirstOrDefault(t => t.Id == ticket.Id);

                if (userid != oldticket.AssignedToId || userid != oldticket.CreatedById)
                {
                    if (oldticket.AssignedToId != null || oldticket.CreatedById != null)
                    {
                        if (oldticket.CreatedById == null)
                        {
                            var dev = ticket.AssignedTo.Email;
                            var user = db.Users.Find(userid).UserName;
                            var web = "https://mjaangbugtracker.azurewebsites.net/Tickets/Details/" + ticket.Id;
                            var email = new EmailService();
                            var mail = new IdentityMessage
                            {
                                Subject = "Notification regarding your ticket from bug tracker",
                                Destination = dev,
                                Body = $"Hi {ticket.AssignedTo.FirstName}! A ticket you are currently assigned ('{ticket.Title}') has been updated by {user}. Click below to view the changes: {web}"
                            };
                            await email.SendAsync(mail);
                        }
                        else
                        {
                            var dev = oldticket.CreatedBy.Email;
                            var owner = db.Users.Find(userid).UserName;
                            var web = "https://mjaangbugtracker.azurewebsites.net/Tickets/Details/" + oldticket.Id;
                            var email = new EmailService();
                            var mail = new IdentityMessage
                            {
                                Subject = "Notification regarding your ticket from bug tracker",
                                Destination = dev,
                                Body = $"Hi {oldticket.CreatedBy.FirstName}! A attachment  was added to a ticket you are currently assigned ('{oldticket.Title}') by {owner}. Click below to view the attachment: {web}"
                            };
                            await email.SendAsync(mail);
                        }
                    }

                    TicketNotification tf1 = new TicketNotification
                    {
                        UserId = oldticket.AssignedToId,
                        TicketId = oldticket.Id,
                        Description = "Comment Edited",
                        Created = System.DateTimeOffset.Now,
                        Type = "CommentEdit"
                    };
                    db.TicketNotifications.Add(tf1);
                }


                if (oldcomment?.Comment != commentedit.Comment)
                {
                    TicketHistories th1 = new TicketHistories
                    {
                        TicketId = commentedit.TicketId,
                        Property = "Comment",
                        OldValue = oldcomment?.Comment,
                        NewValue = commentedit.Comment,
                        Updated = commentedit.Updated,
                        UserId = userid
                    };
                    db.TicketHistory.Add(th1);
                }

                ApplicationUser olduser = db.Users.FirstOrDefault(u => u.Id.Equals(oldcomment.UserId));
                ApplicationUser newuser = db.Users.FirstOrDefault(u => u.Id.Equals(commentedit.UserId));

                if (oldcomment?.UserId != commentedit.UserId)
                {
                    TicketHistories th2 = new TicketHistories
                    {
                        TicketId = commentedit.TicketId,
                        Property = "User",
                        OldValue = olduser.FirstName,
                        NewValue = newuser.FirstName,
                        Updated = commentedit.Updated,

                    };
                    db.TicketHistory.Add(th2);
                }

                db.SaveChanges();
                
            }
            var comments = db.Ticket.Find(commentedit.TicketId);
            return RedirectToAction("Details", "Tickets", new { id = comments.Id });
        }

        // GET: Projects/Tickets/TicketComments/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteComment(int? Id)
        {
            if (Id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TicketComments commentdel = db.TicketComment.Find(Id);
            if (commentdel == null)
            {
                return HttpNotFound();
            }
            return View(commentdel);
        }

        // POST: Projects/Tickets/TicketComments/Delete/5
        [HttpPost, ActionName("DeleteComment")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult ConfirmComment(int Id)
        {
            TicketComments commentdelete = db.TicketComment.Find(Id);
            db.TicketComment.Remove(commentdelete);

            db.SaveChanges();
            var comments = db.Ticket.Find(commentdelete.TicketId);
            return RedirectToAction("Details", "Tickets", new { id = comments.Id });
        }

        //GET: Projects/Tickets/TicketNotifications/Delete/5
        [Authorize]
        public ActionResult DeleteNotification(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var notification = db.TicketNotifications.Find(id);
            if (notification == null)
            {
                return HttpNotFound();
            }

            db.TicketNotifications.Remove(notification);
            db.SaveChanges();
            return RedirectToAction("Index", "Tickets");
        }

        //POST: Projects/Tickets/TicketNotifications/DeleteAll/5
        [Authorize]
        [HttpPost]
        public ActionResult DismissAllNotification()
        {
            var userid = User.Identity.GetUserId();

            if (userid == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var noti = db.TicketNotifications.Where(n => n.UserId == userid);
            foreach (var not in noti)
            {
                db.TicketNotifications.Remove(not);
            }

            db.SaveChanges();

            return RedirectToAction("Index","Tickets");
            
        }


    }

}
