using GOKCafe.Application.DTOs.Banner;
using GOKCafe.Application.DTOs.Category;
using GOKCafe.Application.DTOs.Common;
using GOKCafe.Application.DTOs.ContactInfo;
using GOKCafe.Application.DTOs.HomePage;
using GOKCafe.Application.DTOs.InfoCard;
using GOKCafe.Application.DTOs.Mission;
using GOKCafe.Application.DTOs.Partner;
using GOKCafe.Application.DTOs.Product;
using GOKCafe.Application.DTOs.ServiceFeature;
using GOKCafe.Application.DTOs.TeaAttribute;
using GOKCafe.Application.Services.Interfaces;
using GOKCafe.Domain.Entities;
using GOKCafe.Domain.Interfaces;

namespace GOKCafe.Application.Services;

public class HomeService : IHomeService
{
    private readonly IUnitOfWork _unitOfWork;

    public HomeService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<HomePageDto>> GetHomePageDataAsync()
    {
        try
        {
            // Get featured categories (limit to 3 for the offers section)
            var categories = await _unitOfWork.Categories.GetAllAsync();
            var featuredCategories = categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .Take(3)
                .Select(c => MapCategoryToDto(c))
                .ToList();

            // Get featured coffee products (limit to 4)
            var products = await _unitOfWork.Products.GetAllAsync();
            var featuredCoffee = products
                .Where(p => p.IsActive && p.IsFeatured)
                .OrderBy(p => p.DisplayOrder)
                .Take(4)
                .Select(p => MapProductToDto(p))
                .ToList();

            // Get tea attributes
            var teaAttributes = await _unitOfWork.TeaAttributes.GetAllAsync();
            var teaAttributeList = teaAttributes
                .Where(ta => ta.IsActive)
                .OrderBy(ta => ta.DisplayOrder)
                .Select(ta => MapTeaAttributeToDto(ta))
                .ToList();

            // Get featured tea products (limit to 4)
            var teaCategory = categories.FirstOrDefault(c => c.Name.ToLower().Contains("tea"));
            var featuredTea = products
                .Where(p => p.IsActive && teaCategory != null && p.CategoryId == teaCategory.Id)
                .OrderBy(p => p.DisplayOrder)
                .Take(4)
                .Select(p => MapProductToDto(p))
                .ToList();

            // Get active banners
            var banners = await _unitOfWork.Banners.GetAllAsync();
            var activeBanners = banners
                .Where(b => b.IsActive)
                .OrderBy(b => b.DisplayOrder)
                .Select(b => MapBannerToDto(b))
                .ToList();

            // Get missions
            var missions = await _unitOfWork.Missions.GetAllAsync();
            var missionList = missions
                .Where(m => m.IsActive)
                .OrderBy(m => m.DisplayOrder)
                .Select(m => MapMissionToDto(m))
                .ToList();

            // Get info cards
            var infoCards = await _unitOfWork.InfoCards.GetAllAsync();
            var infoCardList = infoCards
                .Where(ic => ic.IsActive)
                .OrderBy(ic => ic.DisplayOrder)
                .Select(ic => MapInfoCardToDto(ic))
                .ToList();

            // Get partners
            var partners = await _unitOfWork.Partners.GetAllAsync();
            var partnerList = partners
                .Where(p => p.IsActive)
                .OrderBy(p => p.DisplayOrder)
                .Select(p => MapPartnerToDto(p))
                .ToList();

            // Get contact info (first active one)
            var contactInfos = await _unitOfWork.ContactInfos.GetAllAsync();
            var contactInfo = contactInfos
                .Where(ci => ci.IsActive)
                .Select(ci => MapContactInfoToDto(ci))
                .FirstOrDefault();

            // Get service features
            var serviceFeatures = await _unitOfWork.ServiceFeatures.GetAllAsync();
            var serviceFeatureList = serviceFeatures
                .Where(sf => sf.IsActive)
                .OrderBy(sf => sf.DisplayOrder)
                .Select(sf => MapServiceFeatureToDto(sf))
                .ToList();

            var homePageData = new HomePageDto
            {
                OffersSection = new HomePageSectionDto
                {
                    Title = "OUR DELICIOUS OFFER",
                    Subtitle = "EXPERIENCE OUR BEST"
                },
                CoffeeSection = new HomePageSectionDto
                {
                    Title = "OUR COFFEE",
                    Subtitle = "TOP PICKS FOR YOU"
                },
                TeaSection = new HomePageSectionDto
                {
                    Title = "OUR TEA RANGE",
                    Subtitle = "EXPERIENCE THE TEA ZEN"
                },
                MissionSection = new HomePageSectionDto
                {
                    Title = "OUR MISSIONS",
                    Subtitle = "YOU'RE THE REASON WE'RE HERE"
                },
                PartnersSection = new HomePageSectionDto
                {
                    Title = "OUR PARTNERS",
                    Subtitle = "GROW WITH US"
                },
                ContactSection = new HomePageSectionDto
                {
                    Title = "WHERE TO FIND US",
                    Subtitle = "COFFEE, TEA, FOOD AND MORE"
                },
                FeaturedCategories = featuredCategories,
                FeaturedCoffee = featuredCoffee,
                FeaturedTea = featuredTea,
                TeaAttributes = teaAttributeList,
                ActiveBanners = activeBanners,
                Missions = missionList,
                InfoCards = infoCardList,
                Partners = partnerList,
                ContactInfo = contactInfo,
                ServiceFeatures = serviceFeatureList
            };

            return ApiResponse<HomePageDto>.SuccessResult(homePageData);
        }
        catch (Exception ex)
        {
            return ApiResponse<HomePageDto>.FailureResult(
                "An error occurred while retrieving homepage data",
                new List<string> { ex.Message });
        }
    }

    private CategoryDto MapCategoryToDto(Category category)
    {
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            Slug = category.Slug,
            ImageUrl = category.ImageUrl,
            DisplayOrder = category.DisplayOrder,
            IsActive = category.IsActive,
            ProductCount = category.Products?.Count ?? 0
        };
    }

    private ProductDto MapProductToDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Slug = product.Slug,
            Price = product.Price,
            DiscountPrice = product.DiscountPrice,
            ImageUrl = product.ImageUrl,
            StockQuantity = product.StockQuantity,
            IsActive = product.IsActive,
            IsFeatured = product.IsFeatured,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name ?? string.Empty,
            Images = product.ProductImages?
                .OrderBy(pi => pi.DisplayOrder)
                .Select(pi => new ProductImageDto
                {
                    Id = pi.Id,
                    ImageUrl = pi.ImageUrl,
                    AltText = pi.AltText,
                    DisplayOrder = pi.DisplayOrder,
                    IsPrimary = pi.IsPrimary
                })
                .ToList() ?? new List<ProductImageDto>()
        };
    }

    private TeaAttributeDto MapTeaAttributeToDto(TeaAttribute teaAttribute)
    {
        return new TeaAttributeDto
        {
            Id = teaAttribute.Id,
            Title = teaAttribute.Title,
            Description = teaAttribute.Description,
            ImageUrl = teaAttribute.ImageUrl,
            DisplayOrder = teaAttribute.DisplayOrder,
            IsActive = teaAttribute.IsActive
        };
    }

    private BannerDto MapBannerToDto(Banner banner)
    {
        return new BannerDto
        {
            Id = banner.Id,
            Title = banner.Title,
            Subtitle = banner.Subtitle,
            Description = banner.Description,
            ImageUrl = banner.ImageUrl,
            ButtonText = banner.ButtonText,
            ButtonLink = banner.ButtonLink,
            ProductId = banner.ProductId,
            DisplayOrder = banner.DisplayOrder,
            IsActive = banner.IsActive,
            Type = banner.Type,
            Product = banner.Product != null ? MapProductToDto(banner.Product) : null
        };
    }

    private MissionDto MapMissionToDto(Mission mission)
    {
        return new MissionDto
        {
            Id = mission.Id,
            Title = mission.Title,
            Subtitle = mission.Subtitle,
            Description = mission.Description,
            MediaUrl = mission.MediaUrl,
            MediaType = mission.MediaType,
            ButtonText = mission.ButtonText,
            ButtonLink = mission.ButtonLink,
            DisplayOrder = mission.DisplayOrder,
            IsActive = mission.IsActive
        };
    }

    private InfoCardDto MapInfoCardToDto(InfoCard infoCard)
    {
        return new InfoCardDto
        {
            Id = infoCard.Id,
            Title = infoCard.Title,
            Description = infoCard.Description,
            ImageUrl = infoCard.ImageUrl,
            ButtonText = infoCard.ButtonText,
            ButtonLink = infoCard.ButtonLink,
            DisplayOrder = infoCard.DisplayOrder,
            IsActive = infoCard.IsActive
        };
    }

    private PartnerDto MapPartnerToDto(Partner partner)
    {
        return new PartnerDto
        {
            Id = partner.Id,
            Name = partner.Name,
            LogoUrl = partner.LogoUrl,
            WebsiteUrl = partner.WebsiteUrl,
            DisplayOrder = partner.DisplayOrder,
            IsActive = partner.IsActive
        };
    }

    private ContactInfoDto MapContactInfoToDto(ContactInfo contactInfo)
    {
        return new ContactInfoDto
        {
            Id = contactInfo.Id,
            Title = contactInfo.Title,
            Subtitle = contactInfo.Subtitle,
            Description = contactInfo.Description,
            Address = contactInfo.Address,
            Phone = contactInfo.Phone,
            Email = contactInfo.Email,
            WorkingHours = contactInfo.WorkingHours,
            ImageUrl = contactInfo.ImageUrl,
            MapUrl = contactInfo.MapUrl,
            ButtonText = contactInfo.ButtonText,
            ButtonLink = contactInfo.ButtonLink,
            IsActive = contactInfo.IsActive
        };
    }

    private ServiceFeatureDto MapServiceFeatureToDto(ServiceFeature serviceFeature)
    {
        return new ServiceFeatureDto
        {
            Id = serviceFeature.Id,
            Title = serviceFeature.Title,
            Description = serviceFeature.Description,
            IconUrl = serviceFeature.IconUrl,
            DisplayOrder = serviceFeature.DisplayOrder,
            IsActive = serviceFeature.IsActive
        };
    }
}
