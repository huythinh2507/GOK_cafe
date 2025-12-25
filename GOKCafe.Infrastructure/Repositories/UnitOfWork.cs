using GOKCafe.Domain.Entities;
using GOKCafe.Domain.Interfaces;
using GOKCafe.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace GOKCafe.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        Users = new Repository<User>(_context);
        Categories = new Repository<Category>(_context);
        Products = new Repository<Product>(_context);
        ProductImages = new Repository<ProductImage>(_context);
        Orders = new Repository<Order>(_context);
        OrderItems = new Repository<OrderItem>(_context);
        Offers = new Repository<Offer>(_context);
        Partners = new Repository<Partner>(_context);
        ContactMessages = new Repository<ContactMessage>(_context);
        TeaAttributes = new Repository<TeaAttribute>(_context);
        Banners = new Repository<Banner>(_context);
        Missions = new Repository<Mission>(_context);
        InfoCards = new Repository<InfoCard>(_context);
        ContactInfos = new Repository<ContactInfo>(_context);
        ServiceFeatures = new Repository<ServiceFeature>(_context);
        FlavourProfiles = new Repository<FlavourProfile>(_context);
        Equipments = new Repository<Equipment>(_context);
        Carts = new Repository<Cart>(_context);
        CartItems = new Repository<CartItem>(_context);
        RevokedTokens = new Repository<RevokedToken>(_context);
        Coupons = new Repository<Coupon>(_context);
        CouponUsages = new Repository<CouponUsage>(_context);
        Payments = new Repository<Payment>(_context);
        BankTransferConfigs = new Repository<BankTransferConfig>(_context);
        ProductTypes = new Repository<ProductType>(_context);
        ProductAttributes = new Repository<ProductAttribute>(_context);
        ProductAttributeValues = new Repository<ProductAttributeValue>(_context);
        ProductAttributeSelections = new Repository<ProductAttributeSelection>(_context);
        ProductComments = new Repository<ProductComment>(_context);
        Blogs = new Repository<Blog>(_context);
        BlogCategories = new Repository<BlogCategory>(_context);
        BlogComments = new Repository<BlogComment>(_context);
        Events = new Repository<Event>(_context);
        EventRegistrations = new Repository<EventRegistration>(_context);
        EventReviews = new Repository<EventReview>(_context);
        EventHighlights = new Repository<EventHighlight>(_context);
        EventNotificationSubscriptions = new Repository<EventNotificationSubscription>(_context);
    }

    public IRepository<User> Users { get; }
    public IRepository<Category> Categories { get; }
    public IRepository<Product> Products { get; }
    public IRepository<ProductImage> ProductImages { get; }
    public IRepository<Order> Orders { get; }
    public IRepository<OrderItem> OrderItems { get; }
    public IRepository<Offer> Offers { get; }
    public IRepository<Partner> Partners { get; }
    public IRepository<ContactMessage> ContactMessages { get; }
    public IRepository<TeaAttribute> TeaAttributes { get; }
    public IRepository<Banner> Banners { get; }
    public IRepository<Mission> Missions { get; }
    public IRepository<InfoCard> InfoCards { get; }
    public IRepository<ContactInfo> ContactInfos { get; }
    public IRepository<ServiceFeature> ServiceFeatures { get; }
    public IRepository<FlavourProfile> FlavourProfiles { get; }
    public IRepository<Equipment> Equipments { get; }
    public IRepository<Cart> Carts { get; }
    public IRepository<CartItem> CartItems { get; }
    public IRepository<RevokedToken> RevokedTokens { get; }
    public IRepository<Coupon> Coupons { get; }
    public IRepository<CouponUsage> CouponUsages { get; }
    public IRepository<Payment> Payments { get; }
    public IRepository<BankTransferConfig> BankTransferConfigs { get; }
    public IRepository<ProductType> ProductTypes { get; }
    public IRepository<ProductAttribute> ProductAttributes { get; }
    public IRepository<ProductAttributeValue> ProductAttributeValues { get; }
    public IRepository<ProductAttributeSelection> ProductAttributeSelections { get; }
    public IRepository<ProductComment> ProductComments { get; }
    public IRepository<Blog> Blogs { get; }
    public IRepository<BlogCategory> BlogCategories { get; }
    public IRepository<BlogComment> BlogComments { get; }
    public IRepository<Event> Events { get; }
    public IRepository<EventRegistration> EventRegistrations { get; }
    public IRepository<EventReview> EventReviews { get; }
    public IRepository<EventHighlight> EventHighlights { get; }
    public IRepository<EventNotificationSubscription> EventNotificationSubscriptions { get; }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
