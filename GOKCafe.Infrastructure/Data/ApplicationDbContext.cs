using GOKCafe.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GOKCafe.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductImage> ProductImages { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Offer> Offers { get; set; }
    public DbSet<Partner> Partners { get; set; }
    public DbSet<ContactMessage> ContactMessages { get; set; }
    public DbSet<TeaAttribute> TeaAttributes { get; set; }
    public DbSet<Banner> Banners { get; set; }
    public DbSet<Mission> Missions { get; set; }
    public DbSet<InfoCard> InfoCards { get; set; }
    public DbSet<ContactInfo> ContactInfos { get; set; }
    public DbSet<ServiceFeature> ServiceFeatures { get; set; }
    public DbSet<FlavourProfile> FlavourProfiles { get; set; }
    public DbSet<Equipment> Equipments { get; set; }
    public DbSet<ProductFlavourProfile> ProductFlavourProfiles { get; set; }
    public DbSet<ProductEquipment> ProductEquipments { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<RevokedToken> RevokedTokens { get; set; }
    public DbSet<Coupon> Coupons { get; set; }
    public DbSet<CouponUsage> CouponUsages { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<BankTransferConfig> BankTransferConfigs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Global query filter for soft delete
        modelBuilder.Entity<User>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Category>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Product>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ProductImage>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Order>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<OrderItem>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Offer>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Partner>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ContactMessage>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<TeaAttribute>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Banner>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Mission>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<InfoCard>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ContactInfo>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ServiceFeature>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<FlavourProfile>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Equipment>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Cart>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<CartItem>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Coupon>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<CouponUsage>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Payment>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<BankTransferConfig>().HasQueryFilter(e => !e.IsDeleted);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is BaseEntity && (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entityEntry in entries)
        {
            var entity = (BaseEntity)entityEntry.Entity;

            if (entityEntry.State == EntityState.Added)
            {
                entity.CreatedAt = DateTime.UtcNow;
            }
            else
            {
                entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
