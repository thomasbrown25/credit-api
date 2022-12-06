global using financing_api.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Principal;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using financing_api.Data;
using financing_api.Services.CharacterService;
using financing_api.Services.WeaponService;
using financing_api.Services.PlaidService;
using Azure.Identity;
using financing_api;
using financing_api.Services.TransactionsService;
using financing_api.Services.AccountService;
using Going.Plaid;
using financing_api.Shared;
using Microsoft.Extensions.Configuration.Yaml;

var builder = WebApplication.CreateBuilder(args);
var configBuilder = new ConfigurationBuilder();
var services = builder.Services;
var allowMyOrigins = "AllowMyOrigins";

builder.Logging.ClearProviders();



if (builder.Environment.IsDevelopment())
{
    Console.WriteLine("env is dev");

    var connectionString = builder.Configuration.GetConnectionString("AzureAppConfiguration");
    configBuilder.AddAzureAppConfiguration(connectionString);
}
else
{
    Console.WriteLine("env is prod");

    // var endpoint = builder.Configuration.GetSection("AppConfigEndpoint").Value;
    // Console.WriteLine("Got the endpoint: " + endpoint);
    // var credentials = new ManagedIdentityCredential();
    // configBuilder.AddAzureAppConfiguration(options =>
    // {
    //     options.Connect(new Uri(endpoint), credentials);
    // });

    var connectionString = builder.Configuration.GetConnectionString("AzureAppConfiguration");
    configBuilder.AddAzureAppConfiguration("Endpoint=https://financing-api-configs.azconfig.io;Id=GY7h-l4-s0:q/fWVEhyDj2NGi27622K;Secret=DVJDrGSpbMI0o7di/9FteiwHuUhQHhmZgrMJdhaO7jQ=");
}



// You can put your plaid secrets here. But really you can put them
// configBuilder.AddYamlFile("secrets.yaml", optional: true);

var configuration = configBuilder.Build();


// Add Going.Plaid services
services.AddHttpClient();
// services.Configure<PlaidCredentials>(configuration.GetSection(PlaidOptions.SectionKey));
// services.Configure<PlaidOptions>(configuration.GetSection(PlaidOptions.SectionKey));
services.AddSingleton<PlaidClient>();
// services.AddSingleton<ContextContainer>(new ContextContainer() { RunningOnServer = true });

var client = new PlaidClient(Going.Plaid.Environment.Development);

// Add logging
builder.Logging.AddConsole();
builder.Logging.AddEventSourceLogger();

// Add services to the container.
services.AddDbContext<DataContext>(
    options => options.UseSqlServer(configuration["DbConnectionString"])
);
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
services.AddScoped<ITransactionsService, TransactionsService>();
services.AddScoped<IAccountService, AccountService>();

// Authentication
services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(configuration["AppSettings:Key"])
            ),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

// .AddGoogle(googleOptions =>
// {
//     // googleOptions.ClientId = configuration["AppSettings:Google:ClientId"];
//     // googleOptions.ClientSecret = configuration["AppSettings:Google:ClientSecret"];
// });

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
                .WithOrigins("http://localhost:3000", "https://financing-app.azurewebsites.net", "https://money-clarity.azurewebsites.net")
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
