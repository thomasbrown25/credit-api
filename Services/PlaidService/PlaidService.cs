using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using financing_api.Data;
using Acklann.Plaid;
using Acklann.Plaid.Management;

namespace financing_api.Services.PlaidService
{
    public class PlaidService : IPlaidService
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;

        public PlaidService(DataContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<ServiceResponse<string>> GetLinkToken(string email)
        {
            ServiceResponse<string> response = new ServiceResponse<string>();

            try
            {
                // Get current user from sql db
                if (email == null)
                {
                    response.Success = false;
                    response.Message = "No current user logged in.";
                    return response;
                }

                User user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower().Equals(email.ToLower()));

                if (user == null)
                {
                    response.Success = false;
                    response.Message = "User not found.";
                    return response;
                }

                // Call plaid api to get link token
                var client = new PlaidClient(Acklann.Plaid.Environment.Sandbox);

                // Create plaid user with user id 
                var plaidUser = new Acklann.Plaid.Management.CreateLinkTokenRequest.UserInfo()
                {
                    ClientUserId = user.Id.ToString()
                };

                CreateLinkTokenResponse result = await client.CreateLinkToken(new CreateLinkTokenRequest()
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
    }
}