using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using IotCloudServices.Common.JWT;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using System.Net.NetworkInformation;
using Microsoft.Extensions.Configuration;
using IotCloudServices.Common.Utils.Passwords;

namespace IotCloudServices.Common
{
    

        public static class CommonWebApplicationBuilderExtensions
        {
            const string JWT_SETTING_CONF_KEY = "JwtSettings";
            public static WebApplicationBuilder InitializeCommonComponents(this WebApplicationBuilder builder)
            {
                CommonConfiguration.AddCommonJsonSettings(builder.Configuration);
                builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JWT_SETTING_CONF_KEY));
            
                // Access configuration or environment if needed
                var configuration = builder.Configuration;
                var environment = builder.Environment;

            builder.Services.AddSingleton<CommonTokenService>();
            builder.Services.AddSingleton<PasswordService>();
            
            AddAutenticationToken(builder);

            /*
            // Create an instance of CommonTokenService
            var commonTokenService = new CommonTokenService();
                commonTokenService.AddAutenticationToken(builder.Services);
                builder.Services.AddSingleton(commonTokenService);*/



            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen(options =>
                {
                    options.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

                    // Add JWT Bearer authentication to Swagger
                    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        In = ParameterLocation.Header,
                        Description = "Please enter the token with **Bearer** prefix",
                        Name = "Authorization",
                        Type = SecuritySchemeType.ApiKey
                    });

                    options.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            new string[] {}
                        }
                    });
                            });

                // Return the builder to enable method chaining
                return builder;
            }

        static private void AddAutenticationToken(WebApplicationBuilder builder)
        {
            var jwtSettings = builder.Configuration.GetSection(JWT_SETTING_CONF_KEY).Get<JwtSettings>();

            var secretKey = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(secretKey)
                };
            });
        }
    }
}
