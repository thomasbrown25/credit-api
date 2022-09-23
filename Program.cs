global using financing_api.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Principal;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using financing_api.Data;
using financing_api.Services.CharacterService;
using financing_api.Services.WeaponService;
using financing_api.Services.PlaidService;
using Azure.Identity;

var builder = WebApplication.CreateBuilder(args);
var configBuilder = new ConfigurationBuilder();
var services = builder.Services;
var configuration = builder.Configuration;
var allowMyOrigins = "AllowMyOrigins";

if (builder.Environment.IsDevelopment())
{
    var connectionString = builder.Configuration.GetConnectionString("AzureAppConfiguration");
    configBuilder.AddAzureAppConfiguration(connectionString);
}
else
{
    var endpoint = builder.Configuration.GetSection("AppConfigEndpoint").Value;
    var credentials = new ManagedIdentityCredential();
    configBuilder.AddAzureAppConfiguration(options =>
    {
        options.Connect(new Uri(endpoint), credentials);
    });
}

var config = configBuilder.Build();

// Add logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add services to the container.
services.AddDbContext<DataContext>(options => options.UseSqlServer(config["DbConnectionString"]));
services.AddControllers();

// Turn off claim mapping for Microsoft middleware
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
services.AddEndpointsApiExplorer();
services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition(
        "oauth2",
        new OpenApiSecurityScheme
        {
            Description =
                "Standard Authorization header using the Bearer scheme, e.g. \"bearer {token} \"",
            In = ParameterLocation.Header,
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey
        }
    );
    c.OperationFilter<SecurityRequirementsOperationFilter>();
});
services.AddSwaggerGenNewtonsoftSupport();
services.AddAutoMapper(typeof(Program).Assembly);
services.AddScoped<IAuthService, AuthService>();
services.AddScoped<IPlaidService, PlaidService>();
services.AddScoped<ICharacterService, CharacterService>();
services.AddScoped<IWeaponService, WeaponService>();

// Authentication
services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(config["AppSettings:Key"])
            ),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    })
    .AddGoogle(googleOptions =>
    {
        googleOptions.ClientId = configuration["AppSettings:Google:ClientId"];
        googleOptions.ClientSecret = configuration["AppSettings:Google:ClientSecret"];
    });

services.AddHttpContextAccessor();
services.AddTransient<IPrincipal>(
    provider => provider.GetService<IHttpContextAccessor>().HttpContext.User
);

services.AddCors(options =>
{
    options.AddPolicy(
        allowMyOrigins,
        builder =>
        {
            builder
                .WithOrigins("http://localhost:3000", "https://financing-app.azurewebsites.net")
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    );
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(allowMyOrigins);

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
