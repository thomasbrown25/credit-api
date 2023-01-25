using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using financing_api.Data;
using financing_api.Dtos.Liability;
using financing_api.Utils;
using Going.Plaid;

namespace financing_api.Services.LiabilityService
{
    public class LiabilityService : ILiabilityService
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly PlaidClient _client;

        public LiabilityService(
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

        public async Task<ServiceResponse<GetLiabilityDto>> GetLiabilities()
        {
            var response = new ServiceResponse<GetLiabilityDto>();
            try
            {
                response.Data = new GetLiabilityDto();
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
                };

                var result = await _client.LiabilitiesGetAsync(request);

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