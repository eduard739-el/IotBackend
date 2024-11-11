using IotCloudServices.Common.Models;
using IotCloudServices.Services.Authentication.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IotCloudServices.Services.Authentication.DB
{
    public class MyAppDbContext : DbContext
    {
        
        public MyAppDbContext(DbContextOptions<MyAppDbContext> options) : base(options) 
        {
            
        }

        // Define DbSet properties for your tables, for example:
        public DbSet<AuthUser> MyAuthUsers{ get; set; }


    }


}
  
