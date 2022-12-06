using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using financing_api.Data;
using Acklann.Plaid;
using System.Security.Claims;
using financing_api.Utils;

namespace financing_api.Services.PlaidService
{
    public class PlaidService : IPlaidService
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PlaidService(
            DataContext context,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor
        )
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
                User user = Utilities.GetCurrentUser(_context, _httpContextAccessor);

                if (user == null)
                {
                    response.Success = false;
                    response.Message = "User not found.";
                    return response;
                }

                // Create plaid client
                var client = new PlaidClient(Acklann.Plaid.Environment.Development);

                // Create plaid user with user id
                var plaidUser = new Acklann.Plaid.Management.CreateLinkTokenRequest.UserInfo()
                {
                    ClientUserId = user.Id.ToString()
                };

                var result = await client.CreateLinkToken(
                    new Acklann.Plaid.Management.CreateLinkTokenRequest()
                    {
                        ClientId = _configuration["PlaidClientId"],
                        Secret = _configuration["PlaidSecret"],
                        ClientName = "Financing Api",
                        Language = "en",
                        CountryCodes = new string[] { "US" },
                        User = plaidUser,
                        Products = new string[] { "auth", "transactions" }
                    }
                );

                if (result.Exception is not null)
                {
                    response.Success = false;
                    response.Message = result.Exception.ErrorMessage;
                }

                response.Data = result.LinkToken;

                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }


        public async Task<ServiceResponse<string>> UpdateLinkToken()
        {
            ServiceResponse<string> response = new ServiceResponse<string>();

            try
            {
                // Get current user from sql db
                User user = Utilities.GetCurrentUser(_context, _httpContextAccessor);

                if (user == null)
                {
                    response.Success = false;
                    response.Message = "User not found.";
                    return response;
                }

                // Create plaid client
                var client = new PlaidClient(Acklann.Plaid.Environment.Development);

                // Create plaid user with user id
                var plaidUser = new Acklann.Plaid.Management.CreateLinkTokenRequest.UserInfo()
                {
                    ClientUserId = user.Id.ToString()
                };

                var result = await client.CreateLinkToken(
                    new Acklann.Plaid.Management.CreateLinkTokenRequest()
                    {
                        ClientId = _configuration["PlaidClientId"],
                        Secret = _configuration["PlaidSecret"],
                        AccessToken = user.AccessToken,
                        ClientName = "Financing Api",
                        Language = "en",
                        CountryCodes = new string[] { "US" },
                        User = plaidUser,
                        Products = new string[] { "auth", "transactions" }
                    }
                );

                if (result.Exception is not null)
                {
                    response.Success = false;
                    response.Message = result.Exception.ErrorMessage;
                }

                response.Data = result.LinkToken;

                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
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
            var client = new PlaidClient(Acklann.Plaid.Environment.Development);

            // Exchange publicToken for accessToken
            var result = await client.ExchangeTokenAsync(
                new Acklann.Plaid.Management.ExchangeTokenRequest()
                {
                    ClientId = _configuration["PlaidClientId"],
                    Secret = _configuration["PlaidSecret"],
                    PublicToken = publicToken
                }
            );

            // Save accessToken to SQL DB
            var user = Utilities.GetCurrentUser(_context, _httpContextAccessor);
            user.AccessToken = result.AccessToken;
            await _context.SaveChangesAsync();

            response.Data = result.AccessToken;

            return response;
        }

        // Get recurring transactions
        // public async Task<ServiceResponse<decimal>> GetRecurringTransactions()
        // {
        //     var response = new ServiceResponse<List<Acklann.Plaid.Entity.Transaction>>();
        //     response.Data = new List<Acklann.Plaid.Entity.Transaction>();

        //     // Get user for accessToken
        //     var user = GetCurrentUser(_context, _httpContextAccessor);

        //     if (user == null || user.AccessToken == null)
        //     {
        //         response.Success = false;
        //         response.Message = "User does not have access token";
        //         //return response;
        //     }

        //     // Create plaid client
        //     var client = new PlaidClient(Acklann.Plaid.Environment.Development);

        //     var result = await client.FetchTransactionsAsync(
        //         new Acklann.Plaid.()
        //         {
        //             ClientId = _configuration["PlaidClientId"],
        //             Secret = _configuration["PlaidSecret"],
        //             AccessToken = user.AccessToken,
        //             StartDate = DateTime.Today.AddMonths(-1),
        //             EndDate = DateTime.Today
        //         }
        //     );

        //     foreach (var transaction in result.Transactions)
        //     {
        //         response.Data.Add(transaction);
        //     }

        //     return response;
        // }
    }
}
