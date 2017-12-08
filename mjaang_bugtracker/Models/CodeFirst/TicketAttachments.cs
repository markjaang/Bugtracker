using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace mjaang_bugtracker.Models
{
    public class TicketAttachments
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        //public IEnumerable<HttpPostedFileBase> FilePath { get; set; }
        public string Description { get; set; }
        public string UserId { get; set; }
        public string FileUrl { get; set; }
        public string CreatedName { get; set; }

        public DateTimeOffset Created { get; set; }
        public DateTimeOffset? Updated { get; set; }

        public virtual Tickets Ticket { get; set; }
    }
}