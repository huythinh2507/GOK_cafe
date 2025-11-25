using GOKCafe.Web.Services.Implementations;
using GOKCafe.Web.Services.Interfaces;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace GOKCafe.Web.Composers
{
    public class RegisterServicesComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            // Register services
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();

            // Add AutoMapper if needed (uncomment when profiles are created)
            // builder.Services.AddAutoMapper(typeof(RegisterServicesComposer));

            // Add FluentValidation if needed (uncomment when validators are created)
            // builder.Services.AddValidatorsFromAssemblyContaining<RegisterServicesComposer>();

            // Add logging
            builder.Services.AddLogging();
        }
    }
}
