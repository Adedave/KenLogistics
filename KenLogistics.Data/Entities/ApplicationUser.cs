using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace KenLogistics.Data.Entities
{
    public class ApplicationUser : IdentityUser<string>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsDeactivated { get; set; } = false;
    }
}
