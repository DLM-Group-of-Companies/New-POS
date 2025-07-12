using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Models;
using NLI_POS.Models.Base;
using static NLI_POS.Pages.Orders.NewModel;

namespace NLI_POS.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var username = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "SYSTEM";
            var now = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.EncodeDate = now;
                    entry.Entity.EncodedBy = username;
                }

                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdateDate = now;
                    entry.Entity.UpdatedBy = username;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Order>()
                .HasMany(o => o.Payments)
                .WithOne(p => p.Order)
                .HasForeignKey(p => p.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // ... other configurations
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
        public DbSet<ApplicationUser> AppUsers { get; set; }
        public DbSet<UserOfficeAccess> UserOfficesAccess { get; set; }
        public DbSet<PaymentMethod> PaymentMethods { get; set; }
        public DbSet<ProductPrice> ProductPrices { get; set; }
        public DbSet<OrderPayment> OrderPayments { get; set; }
        public DbSet<ProductItem> ProductItems { get; set; }
        public DbSet<OrderDetails> OrderDetails { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
    }
}
