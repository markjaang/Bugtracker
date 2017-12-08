using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace mjaang_bugtracker.Models
{
    public class Tickets
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Slug { get; set; }

        public DateTimeOffset Created { get; set; }
        public DateTimeOffset? Updated { get; set; }

        public int ProjectId { get; set; }
        public int TicketTypeId { get; set; }
        public int TicketPriorityId { get; set; }
        public int TicketStatusId { get; set; }
        public string CreatedById { get; set; }
        public string AssignedToId { get; set; }

        public virtual Projects Project { get; set; }
        public virtual TicketTypes TicketType { get; set; }
        public virtual TicketPriorities TicketPriority { get; set; }
        public virtual TicketStatuses TicketStatus { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }
        public virtual ApplicationUser AssignedTo { get; set; }
        
        public virtual ICollection<TicketAttachments> TicketAttachment { get; set; }
        public virtual ICollection<TicketComments> TicketComment { get; set; }
        public virtual ICollection<TicketHistories> TicketHistory { get; set; }

        private int BodyLimit = 200;
        public string BodyTrimmed
        {
            get
            {
                if (this.Description.Length > this.BodyLimit)
                    return this.Description.Substring(0, this.BodyLimit) + " " + "...";
                else
                    return this.Description;
            }
        }

        private int BodyTrunc = 70;
        public string BodyClipped
        {
            get
            {
                if (this.Description.Length > this.BodyTrunc)
                    return this.Description.Substring(0, this.BodyTrunc) + " " + "...";
                else
                    return this.Description;
            }
        }
    }
}