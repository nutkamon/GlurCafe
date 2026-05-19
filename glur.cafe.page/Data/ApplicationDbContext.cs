using Microsoft.EntityFrameworkCore;
using glur.cafe.page.Models;

namespace glur.cafe.page.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Service> Services => Set<Service>();
        public DbSet<Portfolio> Portfolios => Set<Portfolio>();
        public DbSet<PricingPlan> PricingPlans => Set<PricingPlan>();
        public DbSet<ProcessStep> ProcessSteps => Set<ProcessStep>();
        public DbSet<WhyUsItem> WhyUsItems => Set<WhyUsItem>();
        public DbSet<StatItem> StatItems => Set<StatItem>();
        public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();
        public DbSet<SiteSetting> SiteSettings => Set<SiteSetting>();
        public DbSet<AdminUser> AdminUsers => Set<AdminUser>();
        public DbSet<Quotation> Quotations => Set<Quotation>();
        public DbSet<QuotationItem> QuotationItems => Set<QuotationItem>();
        public DbSet<BeanType> BeanTypes => Set<BeanType>();
        public DbSet<ProductCost> ProductCosts => Set<ProductCost>();
        public DbSet<ProductCostItem> ProductCostItems => Set<ProductCostItem>();
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<CustomerInteraction> CustomerInteractions => Set<CustomerInteraction>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Service>().HasIndex(s => s.DisplayOrder);
            modelBuilder.Entity<Portfolio>().HasIndex(p => p.DisplayOrder);
            modelBuilder.Entity<PricingPlan>().HasIndex(p => p.DisplayOrder);
            modelBuilder.Entity<WhyUsItem>().HasIndex(w => w.DisplayOrder);
            modelBuilder.Entity<StatItem>().HasIndex(s => s.DisplayOrder);
            modelBuilder.Entity<SiteSetting>().HasIndex(s => s.Key).IsUnique();
            modelBuilder.Entity<AdminUser>().HasIndex(a => a.Username).IsUnique();
            modelBuilder.Entity<ContactMessage>().HasIndex(c => c.IsRead);
            modelBuilder.Entity<Quotation>().HasIndex(q => q.QuotationNumber).IsUnique();
            modelBuilder.Entity<Quotation>().HasIndex(q => q.Status);
            modelBuilder.Entity<QuotationItem>()
                .HasOne(i => i.Quotation)
                .WithMany(q => q.Items)
                .HasForeignKey(i => i.QuotationId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<BeanType>().HasIndex(b => b.Name);
            modelBuilder.Entity<ProductCost>().HasIndex(p => p.CreatedAt);
            modelBuilder.Entity<ProductCostItem>()
                .HasOne(i => i.ProductCost)
                .WithMany(p => p.Items)
                .HasForeignKey(i => i.ProductCostId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<ProductCostItem>()
                .HasOne(i => i.BeanType)
                .WithMany()
                .HasForeignKey(i => i.BeanTypeId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            modelBuilder.Entity<Customer>().HasIndex(c => c.Status);
            modelBuilder.Entity<Customer>().HasIndex(c => c.CustomerType);
            modelBuilder.Entity<Customer>().HasIndex(c => c.CreatedAt);

            modelBuilder.Entity<CustomerInteraction>()
                .HasOne(i => i.Customer)
                .WithMany(c => c.Interactions)
                .HasForeignKey(i => i.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<CustomerInteraction>().HasIndex(i => i.CustomerId);
            modelBuilder.Entity<CustomerInteraction>().HasIndex(i => i.FollowUpDate);

            modelBuilder.Entity<Quotation>()
                .HasOne(q => q.Customer)
                .WithMany(c => c.Quotations)
                .HasForeignKey(q => q.CustomerId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            modelBuilder.Entity<ContactMessage>()
                .HasOne(m => m.Customer)
                .WithMany(c => c.ContactMessages)
                .HasForeignKey(m => m.CustomerId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);
        }
    }
}
