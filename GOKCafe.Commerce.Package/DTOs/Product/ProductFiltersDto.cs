using GOKCafe.Application.DTOs.Equipment;
using GOKCafe.Application.DTOs.FlavourProfile;

namespace GOKCafe.Application.DTOs.Product;

public class ProductFiltersDto
{
    public List<FlavourProfileDto> FlavourProfiles { get; set; } = new();
    public List<EquipmentDto> Equipments { get; set; } = new();
    public AvailabilityDto Availability { get; set; } = new();
}

public class AvailabilityDto
{
    public int InStockCount { get; set; }
    public int OutOfStockCount { get; set; }
}
