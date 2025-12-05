using Microsoft.AspNetCore.Mvc;

namespace GOKCafe.Web.Controllers
{
    public class ProductDetailPageController : Controller
    {
        // Mock products for testing - same as ProductsGrid
        private static readonly dynamic[] MockProducts = new[]
        {
            new { Id=1,  Name = "Colombia Dark Roast", Image = "/media/tkddexzl/colombiadarkroast.png", Price = "682,000 VNĐ", Category = "Coffees", Description = "A groundbreaking 96-point Geisha from Geisha El Paraiso 82 in Hieronzuelo, Caucas, grown at high elevation and processed using innovative thermal shock paired with wine yeast fermentation. This coffee redefines complexity and elegance. Expect a dazzling cup—radiant florals interlacing with lush fruit notes and refined spice, all framed by an impeccable balance and silky texture. A true testament to innovation in microlot coffee craftsmanship." },
            new { Id=2, Name = "Swiss Water Decaf", Image = "/media/zqdncqg0/swisswaterdecaf.png", Price = "682,000 VNĐ", Category = "Coffees", Description = "Smooth decaffeinated coffee using Swiss water process. Retains full flavor without the caffeine. Perfect for evening enjoyment without compromising taste." },
            new { Id=3, Name = "Holiday Blend", Image = "/media/xiihorp0/holidayblend.png", Price = "682,000 VNĐ", Category = "Coffees", Description = "Festive blend perfect for holiday season. A warming combination of spices and rich chocolate notes that evokes the spirit of celebration." },
            new { Id=4, Name = "House Blend", Image = "/media/p3mhqekq/houseblend.png", Price = "682,000 VNĐ", Category = "Coffees", Description = "Our signature house blend. Carefully curated to provide a balanced, smooth cup that appeals to coffee enthusiasts of all levels." },
            new { Id=5, Name = "Colombia Wilton Benitez", Image = "/media/kgymu0pv/honeygreentea.png", Price = "850,000 VNĐ", Category = "Coffees", Description = "Premium Colombian single origin from renowned producer Wilton Benitez. High altitude grown beans with exceptional clarity and complexity." },
            new { Id=6, Name = "Assam Black Tea", Image = "/media/aptfsaia/assamblacktea.png", Price = "320,000 VNĐ", Category = "Bubble Tea", Description = "Classic Assam black tea with bold, malty flavor. Traditional Indian tea with a rich, full-bodied profile." },
            new { Id=7, Name = "Honey Milk Tea", Image = "/media/cvxlg5q1/honeymilktea.png", Price = "350,000 VNĐ", Category = "Bubble Tea", Description = "Sweet honey milk tea. Creamy and smooth with natural honey sweetness. A perfect balance of tea and dairy." },
            new { Id=8, Name = "Bubble Tea", Image = "/media/ydfosnna/bubbletee.png", Price = "400,000 VNĐ", Category = "Bubble Tea", Description = "Classic bubble tea. Fresh brewed tea with chewy tapioca pearls and your choice of toppings." },
            new { Id=9, Name = "Green Tea", Image = "/media/5jebon54/whiteitem.png", Price = "280,000 VNĐ", Category = "Bubble Tea", Description = "Fresh green tea. Light, refreshing, and loaded with antioxidants. A healthy choice for tea lovers." },
            new { Id=10, Name = "Honey Green Tea", Image = "/media/kgymu0pv/honeygreentea.png", Price = "320,000 VNĐ", Category = "Bubble Tea", Description = "Honey sweetened green tea. The perfect combination of fresh green tea with natural honey sweetness." },
            new { Id=11, Name = "Carribe Coffee", Image = "/media/kgymu0pv/honeygreentea.png", Price = "620,000 VNĐ", Category = "Coffees", Description = "Caribbean inspired coffee blend. Tropical notes with chocolate undertones. A taste of island paradise in every cup." },
            new { Id=12, Name = "Sweet Doughs", Image = "/media/nm0p0uau/sweetdoughs.png", Price = "180,000 VNĐ", Category = "Sweet Doughs", Description = "Fresh sweet dough pastries. Soft, fluffy, and perfect for pairing with your favorite beverage." }
        };

        public IActionResult Index(int id)
        {
            var product = MockProducts.FirstOrDefault(p => ((dynamic)p).Id == id);
            
            if (product == null)
                return NotFound();

            ViewData["Product"] = product;
            return View("~/Views/productDetail.cshtml", product);
        }
    }
}
