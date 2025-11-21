using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using GOKCafe.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;

namespace GOKCafe.Tests.Integration.Infrastructure;

public class IntegrationTestBase : IClassFixture<CustomWebApplicationFactory>, IDisposable
{
    protected readonly HttpClient Client;
    protected readonly CustomWebApplicationFactory Factory;
    private readonly IServiceScope _scope;

    protected IntegrationTestBase(CustomWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
        _scope = factory.Services.CreateScope();
    }

    protected async Task<HttpResponseMessage> GetAsync(string url)
    {
        return await Client.GetAsync(url);
    }

    protected async Task<T?> GetAsync<T>(string url)
    {
        var response = await Client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>();
    }

    protected async Task<HttpResponseMessage> PostAsync<T>(string url, T data)
    {
        var content = new StringContent(
            JsonSerializer.Serialize(data),
            Encoding.UTF8,
            "application/json");
        return await Client.PostAsync(url, content);
    }

    protected async Task<HttpResponseMessage> PutAsync<T>(string url, T data)
    {
        var content = new StringContent(
            JsonSerializer.Serialize(data),
            Encoding.UTF8,
            "application/json");
        return await Client.PutAsync(url, content);
    }

    protected async Task<HttpResponseMessage> PatchAsync<T>(string url, T data)
    {
        var content = new StringContent(
            JsonSerializer.Serialize(data),
            Encoding.UTF8,
            "application/json");
        return await Client.PatchAsync(url, content);
    }

    protected async Task<HttpResponseMessage> DeleteAsync(string url)
    {
        return await Client.DeleteAsync(url);
    }

    protected void SeedDatabase(Action<ApplicationDbContext> seedAction)
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        seedAction(dbContext);
        dbContext.SaveChanges();
    }

    public void Dispose()
    {
        _scope?.Dispose();
        Client?.Dispose();
    }
}
