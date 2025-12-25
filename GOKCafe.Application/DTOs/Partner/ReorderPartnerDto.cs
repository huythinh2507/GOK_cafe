using System.ComponentModel.DataAnnotations;

namespace GOKCafe.Application.DTOs.Partner;

public class ReorderPartnerDto
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    public int DisplayOrder { get; set; }
}
