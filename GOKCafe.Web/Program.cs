using GOKCafe.Web.Helpers;
using Umbraco.Commerce.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to listen on all network interfaces (for mobile access)
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(25718); // HTTP
    serverOptions.ListenAnyIP(44317, listenOptions =>
    {
        listenOptions.UseHttps(); // HTTPS
    });
});

// Allow all hosts for development
builder.Services.Configure<Microsoft.AspNetCore.HostFiltering.HostFilteringOptions>(options =>
{
    options.AllowedHosts.Clear();
});

builder.CreateUmbracoBuilder()
    .AddBackOffice()
    .AddWebsite()
    .AddComposers()
    .AddUmbracoCommerce(umbracoCommerceBuilder =>
    {
        umbracoCommerceBuilder.AddCommerceProductFeeds();
    })
    .Build();

WebApplication app = builder.Build();

await app.BootUmbracoAsync();


app.UseUmbraco()
    .WithMiddleware(u =>
    {
        u.UseBackOffice();
        u.UseWebsite();
    })
    .WithEndpoints(u =>
    {
        u.UseBackOfficeEndpoints();
        u.UseWebsiteEndpoints();
    });

// Display mobile access URLs
app.Lifetime.ApplicationStarted.Register(MobileAccessHelper.DisplayMobileAccessInfo);

await app.RunAsync();