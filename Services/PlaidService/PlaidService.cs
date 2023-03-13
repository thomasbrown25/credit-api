using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using financing_api.Data;
using System.Security.Claims;
using financing_api.Utils;
using financing_api.Dtos.Plaid;
using Going.Plaid;
using Going.Plaid.Entity;
using Going.Plaid.Link;
using financing_api.ApiHelper;

namespace financing_api.Services.PlaidService
{
    public class PlaidService : IPlaidService
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly PlaidClient _client;
        private readonly IAPI _api;

        public PlaidService(
            DataContext context,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            PlaidClient client,
             IAPI api
        )
        {
            _context = context;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _client = new PlaidClient(Going.Plaid.Environment.Development);
            _api = api;
        }

        public async Task<ServiceResponse<string>> CreateLinkToken()
        {
            ServiceResponse<string> response = new ServiceResponse<string>();

            try
            {
                // Get current user from sql db
                User user = Utilities.GetCurrentUser(_context, _httpContextAccessor);

                var linkResponse = _api.CreateLinkTokenRequest(user);

                if (linkResponse.Result.Error is not null)
                {
                    Console.WriteLine(linkResponse.Result.Error.ErrorMessage);
                    response.Success = false;
                    response.Error = new Error();
                    response.Error.ErrorCode = linkResponse.Result.Error.ErrorCode.ToString();
                    response.Error.ErrorMessage = linkResponse.Result.Error.ErrorMessage;
                    return response;
                }

                response.Data = linkResponse.Result.LinkToken;

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

                var linkResponse = _api.UpdateLinkTokenRequest(user);

                if (linkResponse.Result.Error is not null)
                {
                    Console.WriteLine(linkResponse.Result.Error.ErrorMessage);
                    response.Success = false;
                    response.Error = new Error();
                    response.Error.ErrorCode = linkResponse.Result.Error.ErrorCode.ToString();
                    response.Error.ErrorMessage = linkResponse.Result.Error.ErrorMessage;
                    return response;
                }

                response.Data = linkResponse.Result.LinkToken;

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

            // Exchange publicToken for accessToken
            var exchangeResponse = _api.PublicTokenExchangeRequest(publicToken);

            // Save accessToken to SQL DB
            var user = Utilities.GetCurrentUser(_context, _httpContextAccessor);
            user.AccessToken = exchangeResponse.Result.AccessToken;

            await _context.SaveChangesAsync();

            response.Data = exchangeResponse.Result.AccessToken;

            return response;
        }
    }
}
