using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace mjaang_bugtracker.Models
{
    public class TicketUserViewModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public MultiSelectList Users { get; set; }
        public string[] SelectedUsers { get; set; }
    }
}