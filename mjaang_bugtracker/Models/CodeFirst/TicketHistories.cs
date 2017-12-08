using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace mjaang_bugtracker.Models
{
    public class TicketHistories
    {
        public int Id { get; set; }
        public int TicketId { get; set; }

        public string Property { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public string Changed { get; set; }
        public string UserId { get; set; }

        public DateTimeOffset? Updated { get; set; }

        public virtual Tickets Ticket { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}