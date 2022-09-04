using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using financing_api.Data;
using Acklann.Plaid;
using Acklann.Plaid.Management;
using System.Security.Claims;

namespace financing_api.Services.PlaidService
{
    public class PlaidService : IPlaidService
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PlaidService(DataContext context, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ServiceResponse<string>> CreateLinkToken()
        {
            ServiceResponse<string> response = new ServiceResponse<string>();

            try
            {
                // Get current user from sql db
                User user = GetCurrentUser();

                if (user == null)
                {
                    response.Success = false;
                    response.Message = "User not found.";
                    return response;
                }

                // Create plaid client
                var client = new PlaidClient(Acklann.Plaid.Environment.Sandbox);

                // Create plaid user with user id 
                var plaidUser = new Acklann.Plaid.Management.CreateLinkTokenRequest.UserInfo()
                {
                    ClientUserId = user.Id.ToString()
                };

                var result = await client.CreateLinkToken(new CreateLinkTokenRequest()
                {
                    ClientId = _configuration.GetSection("AppSettings:Plaid:ClientId").Value,
                    Secret = _configuration.GetSection("AppSettings:Plaid:Secret").Value,
                    ClientName = "Financing Api",
                    Language = "en",
                    CountryCodes = new string[] { "US" },
                    User = plaidUser,
                    Products = new string[] { "auth" }
                });

                response.Data = result.LinkToken;

                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Exception = ex.InnerException;
            }
            return response;

        }

        public async Task<ServiceResponse<string>> PublicTokenExchange(string publicToken)
        {
            ServiceResponse<string> response = new ServiceResponse<string>();

            if (publicToken == null)
            {
                response.Success = false;
                response.Message = "No public token in request";
                return response;
            }

            // Create plaid client
            var client = new PlaidClient(Acklann.Plaid.Environment.Sandbox);

            // Exchange publicToken for accessToken
            var result = await client.ExchangeTokenAsync(new ExchangeTokenRequest()
            {
                ClientId = _configuration.GetSection("AppSettings:Plaid:ClientId").Value,
                Secret = _configuration.GetSection("AppSettings:Plaid:Secret").Value,
                PublicToken = publicToken
            });

            // Save accessToken to SQL DB
            var user = GetCurrentUser();
            user.AccessToken = result.AccessToken;
            await _context.SaveChangesAsync();

            response.Data = result.AccessToken;

            return response;
        }



        // Utility Methods
        private string GetUserEmail() => _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

        private User GetCurrentUser()
        {
            try
            {
                string email = GetUserEmail();

                if (email == null)
                    return null;

                // Get current user from sql db
                User user = _context.Users.FirstOrDefault(u => u.Email.ToLower().Equals(email.ToLower()));

                return user;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }

        }
    }
}