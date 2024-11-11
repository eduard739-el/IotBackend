using IotCloudServices.Common.JWT;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Text;

namespace IotCloudServices.Common
{
    public static class WebApplicationAppExtensions
    {
        public static WebApplication InitializeCommonComponents(this WebApplication app)
        {
            // Register the custom JWT token validation middleware
            app.UseMiddleware<JwtTokenValidationMiddleware>();

            // Return the app instance to enable method chaining
            return app;
        }

    }
}

