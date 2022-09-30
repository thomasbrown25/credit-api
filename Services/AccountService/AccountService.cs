using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Acklann.Plaid;
using financing_api.Data;
using financing_api.Dtos.Account;
using financing_api.Utils;

namespace financing_api.Services.AccountService
{
    public class AccountService : IAccountService
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AccountService(
            DataContext context,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor
        )
        {
            _context = context;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ServiceResponse<GetAccountsDto>> GetAccountsBalance()
        {
            var response = new ServiceResponse<GetAccountsDto>();
            response.Data = new GetAccountsDto();
            response.Data.Accounts = new List<AccountDto>();

            // Get user for accessToken
            var user = UtilityMethods.GetCurrentUser(_context, _httpContextAccessor);

            if (user == null || user.AccessToken == null)
            {
                response.Success = false;
                response.Message = "User does not have access token";
                return response;
            }

            // Create plaid client
            var client = new PlaidClient(Acklann.Plaid.Environment.Development);

            var result = await client.FetchAccountBalanceAsync(
                new Acklann.Plaid.Balance.GetBalanceRequest
                {
                    ClientId = _configuration.GetSection("AppSettings:Plaid:ClientId").Value,
                    Secret = _configuration.GetSection("AppSettings:Plaid:Secret").Value,
                    AccessToken = user.AccessToken,
                }
            );

            foreach (var account in result.Accounts)
            {
                var accountDto = new AccountDto
                {
                    Id = account.Id,
                    Name = account.Name,
                    Mask = account.Mask,
                    OfficialName = account.OfficialName,
                    Type = account.Type,
                    Subtype = account.SubType,
                };

                var accountBalance = new AccountBalanceDto
                {
                    Current = account.Balance.Current,
                    Available = account.Balance.Available,
                    Limit = account.Balance.Limit,
                    IsoCurrencyCode = account.Balance.ISOCurrencyCode
                };

                accountDto.Balance = accountBalance;

                response.Data.Accounts.Add(accountDto);
            }

            return response;
        }
    }
}
