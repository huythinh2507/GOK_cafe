using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GOKCafe.Application.DTOs.Category;
using GOKCafe.Application.DTOs.Common;
using GOKCafe.Domain.Entities;
using GOKCafe.Tests.Integration.Infrastructure;

namespace GOKCafe.Tests.Integration.Controllers;

public class CategoriesControllerTests : IntegrationTestBase
{
    public CategoriesControllerTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetCategories_ShouldReturnAllActiveCategories()
    {
        // Act
        var response = await GetAsync("/api/Categories");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<CategoryDto>>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeEmpty();
        result.Data.Should().Contain(c => c.Name == "Coffee");
        result.Data.Should().Contain(c => c.Name == "Tea");
    }

    [Fact]
    public async Task GetCategoryById_ShouldReturnCategory_WhenExists()
    {
        // Arrange
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Coffee",
            Description = "Coffee products",
            Slug = "coffee",
            IsActive = true,
            DisplayOrder = 1
        };

        SeedDatabase(db =>
        {
            db.Categories.Add(category);
        });

        // Act
        var response = await GetAsync($"/api/Categories/{category.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<CategoryDto>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data!.Name.Should().Be("Coffee");
    }

    [Fact]
    public async Task GetCategoryBySlug_ShouldReturnCategory_WhenExists()
    {
        // Arrange
        SeedDatabase(db =>
        {
            db.Categories.Add(new Category
            {
                Id = Guid.NewGuid(),
                Name = "Coffee",
                Description = "Coffee products",
                Slug = "coffee",
                IsActive = true,
                DisplayOrder = 1
            });
        });

        // Act
        var response = await GetAsync("/api/Categories/slug/coffee");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<CategoryDto>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data!.Slug.Should().Be("coffee");
    }

    [Fact]
    public async Task CreateCategory_ShouldCreateNewCategory()
    {
        // Arrange
        var createDto = new CreateCategoryDto
        {
            Name = "New Category",
            Description = "Test description",
            ImageUrl = "https://example.com/image.jpg",
            DisplayOrder = 1
        };

        // Act
        var response = await PostAsync("/api/Categories", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<CategoryDto>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data!.Name.Should().Be("New Category");
    }

    [Fact]
    public async Task UpdateCategory_ShouldUpdateExistingCategory()
    {
        // Arrange
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Old Name",
            Description = "Old description",
            Slug = "old-name",
            IsActive = true,
            DisplayOrder = 1
        };

        SeedDatabase(db =>
        {
            db.Categories.Add(category);
        });

        var updateDto = new UpdateCategoryDto
        {
            Name = "Updated Name",
            Description = "Updated description",
            ImageUrl = "https://example.com/updated.jpg",
            DisplayOrder = 2,
            IsActive = true
        };

        // Act
        var response = await PutAsync($"/api/Categories/{category.Id}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<CategoryDto>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data!.Name.Should().Be("Updated Name");
    }

    [Fact]
    public async Task DeleteCategory_ShouldDeleteCategory_WhenNoProducts()
    {
        // Arrange
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Coffee",
            Description = "Coffee products",
            Slug = "coffee",
            IsActive = true,
            DisplayOrder = 1
        };

        SeedDatabase(db =>
        {
            db.Categories.Add(category);
        });

        // Act
        var response = await DeleteAsync($"/api/Categories/{category.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }
}
