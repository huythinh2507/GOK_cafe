using FluentAssertions;
using GOKCafe.Application.DTOs.Category;
using GOKCafe.Application.Services;
using GOKCafe.Domain.Entities;
using GOKCafe.Domain.Interfaces;
using GOKCafe.Tests.Unit.Helpers;
using Moq;

namespace GOKCafe.Tests.Unit.Services;

public class CategoryServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IRepository<Category>> _categoryRepositoryMock;
    private readonly Mock<IRepository<Product>> _productRepositoryMock;
    private readonly CategoryService _categoryService;

    public CategoryServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _categoryRepositoryMock = new Mock<IRepository<Category>>();
        _productRepositoryMock = new Mock<IRepository<Product>>();

        _unitOfWorkMock.Setup(u => u.Categories).Returns(_categoryRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Products).Returns(_productRepositoryMock.Object);

        _categoryService = new CategoryService(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task GetAllCategoriesAsync_ShouldReturnActiveCategories_OrderedByDisplayOrder()
    {
        // Arrange
        var categories = new List<Category>
        {
            TestDataBuilder.CreateCategory("Coffee", displayOrder: 2, isActive: true),
            TestDataBuilder.CreateCategory("Tea", displayOrder: 1, isActive: true),
            TestDataBuilder.CreateCategory("Inactive", displayOrder: 3, isActive: false)
        };

        _categoryRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(categories);

        // Act
        var result = await _categoryService.GetAllCategoriesAsync();

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(2);
        result.Data![0].Name.Should().Be("Tea");
        result.Data[1].Name.Should().Be("Coffee");
    }

    [Fact]
    public async Task GetAllCategoriesAsync_ShouldReturnFailure_WhenExceptionOccurs()
    {
        // Arrange
        _categoryRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _categoryService.GetAllCategoriesAsync();

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("An error occurred while retrieving categories");
        result.Errors.Should().Contain("Database error");
    }

    [Fact]
    public async Task GetCategoryByIdAsync_ShouldReturnCategory_WhenCategoryExists()
    {
        // Arrange
        var category = TestDataBuilder.CreateCategory("Coffee");
        _categoryRepositoryMock
            .Setup(r => r.GetByIdAsync(category.Id))
            .ReturnsAsync(category);

        // Act
        var result = await _categoryService.GetCategoryByIdAsync(category.Id);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be("Coffee");
    }

    [Fact]
    public async Task GetCategoryByIdAsync_ShouldReturnFailure_WhenCategoryNotFound()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        _categoryRepositoryMock
            .Setup(r => r.GetByIdAsync(categoryId))
            .ReturnsAsync((Category?)null);

        // Act
        var result = await _categoryService.GetCategoryByIdAsync(categoryId);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Category not found");
    }

    [Fact]
    public async Task GetCategoryBySlugAsync_ShouldReturnCategory_WhenSlugExists()
    {
        // Arrange
        var category = TestDataBuilder.CreateCategory("Coffee", slug: "coffee");
        _categoryRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Category, bool>>>()))
            .ReturnsAsync(category);

        // Act
        var result = await _categoryService.GetCategoryBySlugAsync("coffee");

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Slug.Should().Be("coffee");
    }

    [Fact]
    public async Task GetCategoryBySlugAsync_ShouldReturnFailure_WhenSlugNotFound()
    {
        // Arrange
        _categoryRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Category, bool>>>()))
            .ReturnsAsync((Category?)null);

        // Act
        var result = await _categoryService.GetCategoryBySlugAsync("non-existent");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Category not found");
    }

    [Fact]
    public async Task CreateCategoryAsync_ShouldCreateCategory_WhenDataIsValid()
    {
        // Arrange
        var createDto = new CreateCategoryDto
        {
            Name = "New Category",
            Description = "Description",
            ImageUrl = "https://example.com/image.jpg",
            DisplayOrder = 1
        };

        _categoryRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Category, bool>>>()))
            .ReturnsAsync((Category?)null);

        _categoryRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Category>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(default))
            .ReturnsAsync(1);

        // Act
        var result = await _categoryService.CreateCategoryAsync(createDto);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be("New Category");
        result.Data.Slug.Should().Be("new-category");
        result.Message.Should().Be("Category created successfully");

        _categoryRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Category>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task CreateCategoryAsync_ShouldReturnFailure_WhenCategoryWithSameNameExists()
    {
        // Arrange
        var createDto = new CreateCategoryDto
        {
            Name = "Coffee",
            Description = "Description",
            DisplayOrder = 1
        };

        var existingCategory = TestDataBuilder.CreateCategory("Coffee");
        _categoryRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Category, bool>>>()))
            .ReturnsAsync(existingCategory);

        // Act
        var result = await _categoryService.CreateCategoryAsync(createDto);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("A category with this name already exists");
        _categoryRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Category>()), Times.Never);
    }

    [Fact]
    public async Task UpdateCategoryAsync_ShouldUpdateCategory_WhenCategoryExists()
    {
        // Arrange
        var category = TestDataBuilder.CreateCategory("Old Name");
        var updateDto = new UpdateCategoryDto
        {
            Name = "Updated Name",
            Description = "Updated Description",
            ImageUrl = "https://example.com/updated.jpg",
            DisplayOrder = 2,
            IsActive = true
        };

        _categoryRepositoryMock
            .Setup(r => r.GetByIdAsync(category.Id))
            .ReturnsAsync(category);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(default))
            .ReturnsAsync(1);

        // Act
        var result = await _categoryService.UpdateCategoryAsync(category.Id, updateDto);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be("Updated Name");
        result.Message.Should().Be("Category updated successfully");

        _categoryRepositoryMock.Verify(r => r.Update(It.IsAny<Category>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task UpdateCategoryAsync_ShouldReturnFailure_WhenCategoryNotFound()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var updateDto = new UpdateCategoryDto
        {
            Name = "Updated Name",
            Description = "Description",
            DisplayOrder = 1,
            IsActive = true
        };

        _categoryRepositoryMock
            .Setup(r => r.GetByIdAsync(categoryId))
            .ReturnsAsync((Category?)null);

        // Act
        var result = await _categoryService.UpdateCategoryAsync(categoryId, updateDto);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Category not found");
        _categoryRepositoryMock.Verify(r => r.Update(It.IsAny<Category>()), Times.Never);
    }

    [Fact]
    public async Task DeleteCategoryAsync_ShouldDeleteCategory_WhenCategoryHasNoProducts()
    {
        // Arrange
        var category = TestDataBuilder.CreateCategory("Coffee");
        _categoryRepositoryMock
            .Setup(r => r.GetByIdAsync(category.Id))
            .ReturnsAsync(category);

        _productRepositoryMock
            .Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>()))
            .ReturnsAsync(false);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(default))
            .ReturnsAsync(1);

        // Act
        var result = await _categoryService.DeleteCategoryAsync(category.Id);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().BeTrue();
        result.Message.Should().Be("Category deleted successfully");

        _categoryRepositoryMock.Verify(r => r.SoftDelete(It.IsAny<Category>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task DeleteCategoryAsync_ShouldReturnFailure_WhenCategoryHasProducts()
    {
        // Arrange
        var category = TestDataBuilder.CreateCategory("Coffee");
        _categoryRepositoryMock
            .Setup(r => r.GetByIdAsync(category.Id))
            .ReturnsAsync(category);

        _productRepositoryMock
            .Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>()))
            .ReturnsAsync(true);

        // Act
        var result = await _categoryService.DeleteCategoryAsync(category.Id);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Cannot delete category with existing products");
        _categoryRepositoryMock.Verify(r => r.SoftDelete(It.IsAny<Category>()), Times.Never);
    }

    [Fact]
    public async Task DeleteCategoryAsync_ShouldReturnFailure_WhenCategoryNotFound()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        _categoryRepositoryMock
            .Setup(r => r.GetByIdAsync(categoryId))
            .ReturnsAsync((Category?)null);

        // Act
        var result = await _categoryService.DeleteCategoryAsync(categoryId);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Category not found");
        _categoryRepositoryMock.Verify(r => r.SoftDelete(It.IsAny<Category>()), Times.Never);
    }
}
