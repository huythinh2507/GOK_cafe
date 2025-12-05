using FluentAssertions;
using GOKCafe.Application.DTOs.Product;
using GOKCafe.Application.Services;
using GOKCafe.Domain.Entities;
using GOKCafe.Domain.Interfaces;
using GOKCafe.Tests.Unit.Helpers;
using Moq;
using MockQueryable.Moq;

namespace GOKCafe.Tests.Unit.Services;

public class ProductServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IRepository<Product>> _productRepositoryMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly ProductService _productService;

    public ProductServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _productRepositoryMock = new Mock<IRepository<Product>>();
        _cacheServiceMock = new Mock<ICacheService>();

        _unitOfWorkMock.Setup(u => u.Products).Returns(_productRepositoryMock.Object);

        _productService = new ProductService(_unitOfWorkMock.Object, _cacheServiceMock.Object);
    }

    [Fact]
    public async Task GetProductsAsync_ShouldReturnPaginatedActiveProducts()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = TestDataBuilder.CreateCategory("Coffee");
        category.Id = categoryId;

        var products = new List<Product>
        {
            TestDataBuilder.CreateProduct("Product 1", categoryId, isActive: true, isFeatured: true),
            TestDataBuilder.CreateProduct("Product 2", categoryId, isActive: true, isFeatured: false),
            TestDataBuilder.CreateProduct("Product 3", categoryId, isActive: false)
        };

        products.ForEach(p => p.Category = category);

        _productRepositoryMock
            .Setup(r => r.GetQueryable())
            .Returns(products.AsQueryable());

        // Act
        var result = await _productService.GetProductsAsync(1, 10);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().HaveCount(2);
        result.Data.TotalItems.Should().Be(2);
    }

    [Fact]
    public async Task GetProductsAsync_ShouldFilterByCategory()
    {
        // Arrange
        var coffeeId = Guid.NewGuid();
        var teaId = Guid.NewGuid();
        var category = TestDataBuilder.CreateCategory("Coffee");
        category.Id = coffeeId;

        var products = new List<Product>
        {
            TestDataBuilder.CreateProduct("Coffee 1", coffeeId, isActive: true),
            TestDataBuilder.CreateProduct("Coffee 2", coffeeId, isActive: true),
            TestDataBuilder.CreateProduct("Tea 1", teaId, isActive: true)
        };

        products.ForEach(p => p.Category = category);

        _productRepositoryMock
            .Setup(r => r.GetQueryable())
            .Returns(products.AsQueryable());

        // Act - Changed to List<Guid>
        var result = await _productService.GetProductsAsync(1, 10, new List<Guid> { coffeeId });

        // Assert
        result.Success.Should().BeTrue();
        result.Data!.Items.Should().HaveCount(2);
        result.Data.Items.Should().AllSatisfy(p => p.CategoryId.Should().Be(coffeeId));
    }

    [Fact]
    public async Task GetProductsAsync_ShouldFilterByFeatured()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = TestDataBuilder.CreateCategory("Coffee");
        category.Id = categoryId;

        var products = new List<Product>
        {
            TestDataBuilder.CreateProduct("Featured 1", categoryId, isFeatured: true, isActive: true),
            TestDataBuilder.CreateProduct("Featured 2", categoryId, isFeatured: true, isActive: true),
            TestDataBuilder.CreateProduct("Normal", categoryId, isFeatured: false, isActive: true)
        };

        products.ForEach(p => p.Category = category);

        _productRepositoryMock
            .Setup(r => r.GetQueryable())
            .Returns(products.AsQueryable());

        // Act
        var result = await _productService.GetProductsAsync(1, 10, isFeatured: true);

        // Assert
        result.Success.Should().BeTrue();
        result.Data!.Items.Should().HaveCount(2);
        result.Data.Items.Should().AllSatisfy(p => p.IsFeatured.Should().BeTrue());
    }

    [Fact]
    public async Task GetProductByIdAsync_ShouldReturnProduct_WhenProductExists()
    {
        // Arrange
        var product = TestDataBuilder.CreateProduct("Espresso");
        product.Category = TestDataBuilder.CreateCategory("Coffee");

        _productRepositoryMock
            .Setup(r => r.GetQueryable())
            .Returns(new List<Product> { product }.AsQueryable());

        // Act
        var result = await _productService.GetProductByIdAsync(product.Id);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be("Espresso");
    }

    [Fact]
    public async Task GetProductByIdAsync_ShouldReturnFailure_WhenProductNotFound()
    {
        // Arrange
        var productId = Guid.NewGuid();
        _productRepositoryMock
            .Setup(r => r.GetQueryable())
            .Returns(new List<Product>().AsQueryable());

        // Act
        var result = await _productService.GetProductByIdAsync(productId);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Product not found");
    }

    [Fact]
    public async Task CreateProductAsync_ShouldCreateProduct_WhenDataIsValid()
    {
        // Arrange
        var createDto = new CreateProductDto
        {
            Name = "New Espresso",
            Description = "Strong coffee",
            Price = 3.99m,
            CategoryId = Guid.NewGuid(),
            StockQuantity = 50,
            IsFeatured = true
        };

        _productRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>()))
            .ReturnsAsync((Product?)null);

        _productRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Product>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(default))
            .ReturnsAsync(1);

        // Act
        var result = await _productService.CreateProductAsync(createDto);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be("New Espresso");
        result.Message.Should().Be("Product created successfully");

        _productRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveByPrefixAsync("products:", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateProductAsync_ShouldReturnFailure_WhenProductWithSameNameExists()
    {
        // Arrange
        var createDto = new CreateProductDto
        {
            Name = "Espresso",
            Price = 3.99m,
            CategoryId = Guid.NewGuid()
        };

        var existingProduct = TestDataBuilder.CreateProduct("Espresso");
        _productRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>()))
            .ReturnsAsync(existingProduct);

        // Act
        var result = await _productService.CreateProductAsync(createDto);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("A product with this name already exists");
        _productRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task UpdateProductAsync_ShouldUpdateProduct_WhenProductExists()
    {
        // Arrange
        var product = TestDataBuilder.CreateProduct("Old Name");
        var updateDto = new UpdateProductDto
        {
            Name = "Updated Name",
            Description = "Updated Description",
            Price = 5.99m,
            CategoryId = Guid.NewGuid(),
            StockQuantity = 100,
            IsActive = true,
            IsFeatured = true
        };

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(product.Id))
            .ReturnsAsync(product);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(default))
            .ReturnsAsync(1);

        // Act
        var result = await _productService.UpdateProductAsync(product.Id, updateDto);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be("Updated Name");
        result.Message.Should().Be("Product updated successfully");

        _productRepositoryMock.Verify(r => r.Update(It.IsAny<Product>()), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveAsync($"product:{product.Id}", It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveByPrefixAsync("products:", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProductAsync_ShouldReturnFailure_WhenProductNotFound()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var updateDto = new UpdateProductDto
        {
            Name = "Updated Name",
            Price = 5.99m,
            CategoryId = Guid.NewGuid(),
            IsActive = true
        };

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _productService.UpdateProductAsync(productId, updateDto);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Product not found");
        _productRepositoryMock.Verify(r => r.Update(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task DeleteProductAsync_ShouldDeleteProduct_WhenProductExists()
    {
        // Arrange
        var product = TestDataBuilder.CreateProduct("Espresso");
        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(product.Id))
            .ReturnsAsync(product);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(default))
            .ReturnsAsync(1);

        // Act
        var result = await _productService.DeleteProductAsync(product.Id);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().BeTrue();
        result.Message.Should().Be("Product deleted successfully");

        _productRepositoryMock.Verify(r => r.SoftDelete(It.IsAny<Product>()), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveAsync($"product:{product.Id}", It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveByPrefixAsync("products:", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteProductAsync_ShouldReturnFailure_WhenProductNotFound()
    {
        // Arrange
        var productId = Guid.NewGuid();
        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _productService.DeleteProductAsync(productId);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Product not found");
        _productRepositoryMock.Verify(r => r.SoftDelete(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task GetProductByIdAsync_ShouldReturnFromCache_WhenCacheHit()
    {
        // Arrange
        var product = TestDataBuilder.CreateProduct("Espresso");
        product.Category = TestDataBuilder.CreateCategory("Coffee");

        var cachedResponse = GOKCafe.Application.DTOs.Common.ApiResponse<GOKCafe.Application.DTOs.Product.ProductDto>
            .SuccessResult(new GOKCafe.Application.DTOs.Product.ProductDto
            {
                Id = product.Id,
                Name = "Cached Espresso",
                CategoryName = "Coffee"
            });

        _cacheServiceMock
            .Setup(c => c.GetAsync<GOKCafe.Application.DTOs.Common.ApiResponse<GOKCafe.Application.DTOs.Product.ProductDto>>(
                $"product:{product.Id}",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedResponse);

        // Act
        var result = await _productService.GetProductByIdAsync(product.Id);

        // Assert
        result.Success.Should().BeTrue();
        result.Data!.Name.Should().Be("Cached Espresso");

        // Verify database was NOT queried
        _productRepositoryMock.Verify(r => r.GetQueryable(), Times.Never);

        // Verify cache was checked
        _cacheServiceMock.Verify(c => c.GetAsync<It.IsAnyType>(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetProductByIdAsync_ShouldCacheResult_WhenCacheMiss()
    {
        // Arrange
        var product = TestDataBuilder.CreateProduct("Espresso");
        product.Category = TestDataBuilder.CreateCategory("Coffee");

        _productRepositoryMock
            .Setup(r => r.GetQueryable())
            .Returns(new List<Product> { product }.AsQueryable());

        // Act
        var result = await _productService.GetProductByIdAsync(product.Id);

        // Assert
        result.Success.Should().BeTrue();

        // Verify cache was set
        _cacheServiceMock.Verify(c => c.SetAsync(
            $"product:{product.Id}",
            It.IsAny<object>(),
            It.IsAny<TimeSpan>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
