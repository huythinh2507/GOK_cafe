using Umbraco.Cms.Core.Strings;

namespace GOKCafe.Web.Models.ViewModels
{
    public class HomepageViewModel
    {
        // Static content from Umbraco
        public List<HeroBanner> HeroBanners { get; set; } = new();
        public string FeaturedSectionTitle { get; set; } = string.Empty;
        public string FeaturedSectionSubtitle { get; set; } = string.Empty;
        public string AboutTitle { get; set; } = string.Empty;
        public IHtmlEncodedString? AboutDescription { get; set; }
        public string AboutImageUrl { get; set; } = string.Empty;
        public List<PromotionBanner> PromotionBanners { get; set; } = new();

        // Dynamic data from services
        public List<DTOs.ProductDto> FeaturedProducts { get; set; } = new();
        public List<DTOs.CategoryDto> Categories { get; set; } = new();

        // Settings
        public SiteSettings Settings { get; set; } = new();
    }

    public class HeroBanner
    {
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string CtaText { get; set; } = string.Empty;
        public string CtaUrl { get; set; } = string.Empty;
    }

    public class PromotionBanner
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string LinkUrl { get; set; } = string.Empty;
    }

    public class SiteSettings
    {
        public string SiteName { get; set; } = "GOK Cafe";
        public string LogoUrl { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string FacebookUrl { get; set; } = string.Empty;
        public string InstagramUrl { get; set; } = string.Empty;
        public string TwitterUrl { get; set; } = string.Empty;
        public string CopyrightText { get; set; } = string.Empty;
    }
}
