using FluentAssertions;
using GOKCafe.Application.Services;
using GOKCafe.Domain.Entities;
using GOKCafe.Domain.Interfaces;
using GOKCafe.Tests.Unit.Helpers;
using Moq;

namespace GOKCafe.Tests.Unit.Services;

public class HomeServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly HomeService _homeService;

    public HomeServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _homeService = new HomeService(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task GetHomePageDataAsync_ShouldReturnCompleteHomePageData()
    {
        // Arrange
        var categories = new List<Category>
        {
            TestDataBuilder.CreateCategory("Coffee", displayOrder: 1),
            TestDataBuilder.CreateCategory("Tea", displayOrder: 2),
            TestDataBuilder.CreateCategory("Food", displayOrder: 3)
        };

        var products = new List<Product>
        {
            TestDataBuilder.CreateProduct("Espresso", categories[0].Id, isFeatured: true),
            TestDataBuilder.CreateProduct("Green Tea", categories[1].Id, isFeatured: true)
        };

        var teaAttributes = new List<TeaAttribute>
        {
            TestDataBuilder.CreateTeaAttribute("Organic")
        };

        var banners = new List<Banner>
        {
            TestDataBuilder.CreateBanner("Welcome Banner")
        };

        var missions = new List<Mission>
        {
            TestDataBuilder.CreateMission("Our Mission")
        };

        var infoCards = new List<InfoCard>
        {
            TestDataBuilder.CreateInfoCard("Quality Coffee")
        };

        var partners = new List<Partner>
        {
            TestDataBuilder.CreatePartner("Partner 1")
        };

        var contactInfos = new List<ContactInfo>
        {
            TestDataBuilder.CreateContactInfo()
        };

        var serviceFeatures = new List<ServiceFeature>
        {
            TestDataBuilder.CreateServiceFeature("Fast Delivery")
        };

        _unitOfWorkMock.Setup(u => u.Categories).Returns(CreateMockRepository(categories).Object);
        _unitOfWorkMock.Setup(u => u.Products).Returns(CreateMockRepository(products).Object);
        _unitOfWorkMock.Setup(u => u.TeaAttributes).Returns(CreateMockRepository(teaAttributes).Object);
        _unitOfWorkMock.Setup(u => u.Banners).Returns(CreateMockRepository(banners).Object);
        _unitOfWorkMock.Setup(u => u.Missions).Returns(CreateMockRepository(missions).Object);
        _unitOfWorkMock.Setup(u => u.InfoCards).Returns(CreateMockRepository(infoCards).Object);
        _unitOfWorkMock.Setup(u => u.Partners).Returns(CreateMockRepository(partners).Object);
        _unitOfWorkMock.Setup(u => u.ContactInfos).Returns(CreateMockRepository(contactInfos).Object);
        _unitOfWorkMock.Setup(u => u.ServiceFeatures).Returns(CreateMockRepository(serviceFeatures).Object);

        // Act
        var result = await _homeService.GetHomePageDataAsync();

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.FeaturedCategories.Should().HaveCount(3);
        result.Data.TeaAttributes.Should().HaveCount(1);
        result.Data.ActiveBanners.Should().HaveCount(1);
        result.Data.Missions.Should().HaveCount(1);
        result.Data.InfoCards.Should().HaveCount(1);
        result.Data.Partners.Should().HaveCount(1);
        result.Data.ServiceFeatures.Should().HaveCount(1);
        result.Data.ContactInfo.Should().NotBeNull();
        result.Data.OffersSection.Title.Should().Be("OUR DELICIOUS OFFER");
        result.Data.CoffeeSection.Title.Should().Be("OUR COFFEE");
        result.Data.TeaSection.Title.Should().Be("OUR TEA RANGE");
    }

    [Fact]
    public async Task GetHomePageDataAsync_ShouldReturnFailure_WhenExceptionOccurs()
    {
        // Arrange
        _unitOfWorkMock.Setup(u => u.Categories)
            .Returns(CreateMockRepository<Category>(new List<Category>()).Object);

        _unitOfWorkMock.Setup(u => u.Products)
            .Throws(new Exception("Database error"));

        // Act
        var result = await _homeService.GetHomePageDataAsync();

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("An error occurred while retrieving homepage data");
        result.Errors.Should().Contain("Database error");
    }

    private Mock<IRepository<T>> CreateMockRepository<T>(List<T> data) where T : BaseEntity
    {
        var mock = new Mock<IRepository<T>>();
        mock.Setup(r => r.GetAllAsync()).ReturnsAsync(data);
        return mock;
    }
}
