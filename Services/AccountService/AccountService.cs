using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using financing_api.Data;
using financing_api.Dtos.Account;
using financing_api.Utils;
using Going.Plaid;

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
            try
            {
                response.Data = new GetAccountsDto();
                response.Data.Accounts = new List<AccountDto>();

                // Get user for accessToken
                var user = Utilities.GetCurrentUser(_context, _httpContextAccessor);

                if (user == null || user.AccessToken == null)
                {
                    response.Success = false;
                    response.Message = "User does not have access token";
                    return response;
                }

                var request = new Going.Plaid.Accounts.AccountsBalanceGetRequest()
                {
                    ClientId = _configuration["PlaidClientId"],
                    Secret = _configuration["PlaidSecret"],
                    AccessToken = user.AccessToken,
                };

                // Create plaid client
                var client = new PlaidClient(Going.Plaid.Environment.Development);

                var result = await client.AccountsBalanceGetAsync(request);

                foreach (var account in result.Accounts)
                {
                    var accountDto = new AccountDto
                    {
                        Id = account.AccountId,
                        Name = account.Name,
                        Mask = account.Mask,
                        OfficialName = account.OfficialName,
                        Type = account.Type,
                        Subtype = account.Subtype,
                    };

                    var accountBalance = new AccountBalanceDto
                    {
                        Current = account.Balances.Current,
                        Available = account.Balances.Available,
                        Limit = account.Balances.Limit,
                        IsoCurrencyCode = account.Balances.IsoCurrencyCode
                    };

                    accountDto.Balance = accountBalance;

                    response.Data.Accounts.Add(accountDto);
                }
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
