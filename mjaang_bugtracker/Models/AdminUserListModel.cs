using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace mjaang_bugtracker.Models
{
    public class AdminUserListModel
    {
        public ApplicationUser user { get; set; }
        public List<string> roles { get; set; }
    }
}