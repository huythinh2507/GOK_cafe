using GOKCafe.Domain.Entities;

namespace GOKCafe.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<User> Users { get; }
    IRepository<Category> Categories { get; }
    IRepository<Product> Products { get; }
    IRepository<ProductImage> ProductImages { get; }
    IRepository<Order> Orders { get; }
    IRepository<OrderItem> OrderItems { get; }
    IRepository<Offer> Offers { get; }
    IRepository<Partner> Partners { get; }
    IRepository<ContactMessage> ContactMessages { get; }
    IRepository<TeaAttribute> TeaAttributes { get; }
    IRepository<Banner> Banners { get; }
    IRepository<Mission> Missions { get; }
    IRepository<InfoCard> InfoCards { get; }
    IRepository<ContactInfo> ContactInfos { get; }
    IRepository<ServiceFeature> ServiceFeatures { get; }
    IRepository<FlavourProfile> FlavourProfiles { get; }
    IRepository<Equipment> Equipments { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
