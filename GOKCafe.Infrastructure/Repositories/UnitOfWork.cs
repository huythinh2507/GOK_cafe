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
