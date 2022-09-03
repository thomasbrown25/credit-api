global using web_api_netcore_project.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Principal;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using web_api_netcore_project.Data;
using web_api_netcore_project.Services.CharacterService;
using web_api_netcore_project.Services.WeaponService;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

// Add logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add services to the container.
services.AddDbContext<DataContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("MainConnection")));
services.AddControllers();

// Turn off claim mapping for Microsoft middleware 
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
services.AddEndpointsApiExplorer();
services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Description = "Standard Authorization header using the Bearer scheme, e.g. \"bearer {token} \"",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    c.OperationFilter<SecurityRequirementsOperationFilter>();

});
services.AddSwaggerGenNewtonsoftSupport();
services.AddAutoMapper(typeof(Program).Assembly);
services.AddScoped<ICharacterService, CharacterService>();
services.AddScoped<IAuthService, AuthService>();
services.AddScoped<IWeaponService, WeaponService>();

// Authentication
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(builder.Configuration.GetSection("AppSettings:Key").Value)),
        ValidateIssuer = false,
        ValidateAudience = false
    };
}).AddGoogle(googleOptions =>
    {
        googleOptions.ClientId = configuration["AppSettings:Google:ClientId"];
        googleOptions.ClientSecret = configuration["AppSettings:Google:ClientSecret"];
    });

services.AddHttpContextAccessor();
services.AddTransient<IPrincipal>(provider => provider.GetService<IHttpContextAccessor>().HttpContext.User);

services.AddCors(options =>
            {
                options.AddPolicy("AllowFinancingAppClient",
                  builder =>
                  {
                      builder
                      .WithOrigins("http://localhost:3000")
                      .AllowAnyHeader()
                      .AllowAnyMethod();
                  });
            });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFinancingAppClient");

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
