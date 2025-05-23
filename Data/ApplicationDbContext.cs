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
        public DbSet<ProductType> ProductTypes { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<InventoryStock> InventoryStocks { get; set; }
        public DbSet<InventoryStockAuditTrail> InventoryStockAuditTrails { get; set; }
        public DbSet<ProductCombo> ProductCombos { get; set; }
        public DbSet<Order> Orders { get; set; }
    }
}
