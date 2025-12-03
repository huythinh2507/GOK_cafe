using GOKCafe.Application.DTOs.Common;
using GOKCafe.Application.DTOs.Odoo;

namespace GOKCafe.Application.Services.Interfaces;

public interface IOdooService
{
    /// <summary>
    /// Fetch products from Odoo
    /// </summary>
    Task<ApiResponse<List<OdooProductDto>>> FetchProductsFromOdooAsync();

    /// <summary>
    /// Sync products from Odoo to GOKCafe database
    /// </summary>
    Task<ApiResponse<OdooSyncResultDto>> SyncProductsFromOdooAsync();
}
