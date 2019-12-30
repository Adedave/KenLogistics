using KenLogistics.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KenLogistics.Web.Areas.Admin.Models
{
    public class RoleEditModel
    {
        public UserRole Role { get; set; }

        public List<ApplicationUser> Members { get; set; }
        public List<ApplicationUser> NonMembers { get; set; }
    }
}
