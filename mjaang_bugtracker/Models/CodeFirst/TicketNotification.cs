using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace mjaang_bugtracker.Models
{
    public class TicketNotification
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int TicketId { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public DateTimeOffset Created { get; set; }

        public virtual ApplicationUser User { get; set; }
        public virtual Tickets Ticket { get; set; }

    }
}