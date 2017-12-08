using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace mjaang_bugtracker.Models
{
    public class Projects
    {
        public Projects()
        {
            this.Ticket = new HashSet<Tickets>();
            this.User = new HashSet<ApplicationUser>();
        }
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        [AllowHtml]
        public string Body { get; set; }
        public string AuthorUserId { get; set; }
        public string Slug { get; set; }

        public DateTimeOffset Created { get; set; }
        public DateTimeOffset? Updated { get; set; }

        public virtual ICollection<Tickets> Ticket { get; set; }
        public virtual ICollection<ApplicationUser> User { get; set; }

        private int BodyLimit = 200;
        public string BodyTrimmed
        {
            get
            {
                if (this.Body.Length > this.BodyLimit)
                    return this.Body.Substring(0, this.BodyLimit) + " " + "...";
                else
                    return this.Body;
            }
        }

        private int BodyTrunc = 70;
        public string BodyClipped
        {
            get
            {
                if (this.Body.Length > this.BodyTrunc)
                    return this.Body.Substring(0, this.BodyTrunc) + " " + "...";
                else
                    return this.Body;
            }
        }
    }
}