using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace mjaang_bugtracker.Models
{ 
    public class TicketComments
    {
        public int Id { get; set; }
        [AllowHtml]
        public string Comment { get; set; }

        public int TicketId { get; set; }
        public string UserId { get; set; }

        public DateTimeOffset Created { get; set; }
        public DateTimeOffset? Updated { get; set; }

        public virtual ApplicationUser User { get; set; }
        public virtual Tickets Ticket { get; set; }
    }
}