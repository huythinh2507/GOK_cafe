using GOKCafe.Application.DTOs.Common;
using GOKCafe.Application.DTOs.HomePage;

namespace GOKCafe.Application.Services.Interfaces;

public interface IHomeService
{
    Task<ApiResponse<HomePageDto>> GetHomePageDataAsync();
}
