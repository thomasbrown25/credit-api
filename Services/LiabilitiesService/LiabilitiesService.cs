using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using financing_api.Data;
using financing_api.Dtos.Liabilities;
using financing_api.Utils;
using Going.Plaid;

namespace financing_api.Services.LiabilitiesService
{
    public class LiabilitiesService : ILiabilitiesService
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly PlaidClient _client;

        public LiabilitiesService(
            DataContext context,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            PlaidClient client
        )
        {
            _context = context;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _client = new PlaidClient(Going.Plaid.Environment.Development);
        }

        public async Task<ServiceResponse<GetLiabilitiesDto>> GetLiabilities()
        {
            var response = new ServiceResponse<GetLiabilitiesDto>();
            try
            {
                response.Data = new GetLiabilitiesDto();
                response.Data.Liabilities = new List<string>();

                // Get user for accessToken
                var user = Utilities.GetCurrentUser(_context, _httpContextAccessor);

                if (user == null || user.AccessToken == null)
                {
                    response.Success = false;
                    response.Message = "User does not have access token";
                    return response;
                }

                var request = new Going.Plaid.Liabilities.LiabilitiesGetRequest()
                {
                    ClientId = _configuration["PlaidClientId"],
                    Secret = _configuration["PlaidSecret"],
                    AccessToken = user.AccessToken,
                    // Options = new Going.Plaid.Entity.LiabilitiesGetRequestOptions()
                    // {
                    //     AccountIds = new List<string>() { "oNnb8R4NeBhkgm6nZzykcvKV3y7k4mT4oBQPg" }
                    // }
                };

                var result = await _client.LiabilitiesGetAsync(request);

                if (result is not null && result.Error is not null)
                {
                    Console.WriteLine("Plaid Error: " + result.Error.ErrorMessage);
                    response.Success = false;
                    response.Message = result.Error.ErrorMessage;
                    return response;
                }

                // foreach (var account in result.Accounts)
                // {
                //     var accountDto = new AccountDto
                //     {
                //         Id = account.AccountId,
                //         Name = account.Name,
                //         Mask = account.Mask,
                //         OfficialName = account.OfficialName,
                //         Type = account.Subtype?.ToString(),
                //         Subtype = account.Subtype,
                //     };

                //     var accountBalance = new AccountBalanceDto
                //     {
                //         Current = account.Balances.Current,
                //         Available = account.Balances.Available,
                //         Limit = account.Balances.Limit,
                //         IsoCurrencyCode = account.Balances.IsoCurrencyCode
                //     };

                //     accountDto.Balance = accountBalance;

                //     response.Data.Accounts.Add(accountDto);
                // }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("Account Balances failed");
                response.Success = false;
                response.Message = ex.Message;
                return response;
            }

            return response;
        }
    }
}