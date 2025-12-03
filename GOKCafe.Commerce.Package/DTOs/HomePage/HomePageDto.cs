using GOKCafe.Application.DTOs.Banner;
using GOKCafe.Application.DTOs.Category;
using GOKCafe.Application.DTOs.ContactInfo;
using GOKCafe.Application.DTOs.InfoCard;
using GOKCafe.Application.DTOs.Mission;
using GOKCafe.Application.DTOs.Partner;
using GOKCafe.Application.DTOs.Product;
using GOKCafe.Application.DTOs.ServiceFeature;
using GOKCafe.Application.DTOs.TeaAttribute;

namespace GOKCafe.Application.DTOs.HomePage;

public class HomePageDto
{
    public HomePageSectionDto OffersSection { get; set; } = new();
    public HomePageSectionDto CoffeeSection { get; set; } = new();
    public HomePageSectionDto TeaSection { get; set; } = new();
    public HomePageSectionDto MissionSection { get; set; } = new();
    public HomePageSectionDto PartnersSection { get; set; } = new();
    public HomePageSectionDto ContactSection { get; set; } = new();
    public List<CategoryDto> FeaturedCategories { get; set; } = new();
    public List<ProductDto> FeaturedCoffee { get; set; } = new();
    public List<ProductDto> FeaturedTea { get; set; } = new();
    public List<TeaAttributeDto> TeaAttributes { get; set; } = new();
    public List<BannerDto> ActiveBanners { get; set; } = new();
    public List<MissionDto> Missions { get; set; } = new();
    public List<InfoCardDto> InfoCards { get; set; } = new();
    public List<PartnerDto> Partners { get; set; } = new();
    public ContactInfoDto? ContactInfo { get; set; }
    public List<ServiceFeatureDto> ServiceFeatures { get; set; } = new();
}

public class HomePageSectionDto
{
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
}
