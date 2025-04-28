using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Models;

namespace NLI_POS.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Customer> Customer { get; set; }
        public DbSet<CustClass> CustClass { get; set; }
        public DbSet<Country> Country { get; set; }
        public DbSet<OfficeCountry> OfficeCountry { get; set; }
    }
}
