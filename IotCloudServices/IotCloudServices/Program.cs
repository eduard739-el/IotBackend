using IotCloudServices.Authentication.Utils;
using IotCloudServices.Common;
using IotCloudServices.Common.JWT;
using IotCloudServices.Services.Authentication.DB;
using IotCloudServices.Services.Authentication.DB.Redis;
using IotCloudServices.Services.Authentication.DB.Redis.Models;
using IotCloudServices.Services.Authentication.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

//initialize common 
CommonWebApplicationBuilderExtensions.InitializeCommonComponents(builder);

// Register TokenService
builder.Services.AddSingleton<TokenService>(); 
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
builder.Services.AddSingleton<RefreshTokenSessionManager>();
builder.Services.AddSingleton<RedisConnectionManager>();
builder.Services.AddSingleton<RedisRefreshTokenManager>();

builder.Services.Configure<RedisSettings>(builder.Configuration.GetSection("RedisSettings"));

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddDbContext<MyAppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection")));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
WebApplicationAppExtensions.InitializeCommonComponents(app);
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
