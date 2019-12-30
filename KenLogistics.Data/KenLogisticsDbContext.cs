using KenLogistics.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace KenLogistics.Data
{
    public class KenLogisticsDbContext: IdentityDbContext<ApplicationUser,UserRole,string>
    {
        public KenLogisticsDbContext(DbContextOptions<KenLogisticsDbContext> options)
       : base(options) { }

       
    }
}
