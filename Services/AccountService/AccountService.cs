using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using financing_api.Data;
using financing_api.Dtos.Account;
using financing_api.Utils;
using Going.Plaid;
using financing_api.ApiHelper;

namespace financing_api.Services.AccountService
{
    public class AccountService : IAccountService
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;
        private readonly IAPI _api;

        public AccountService(
            DataContext context,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            IMapper mapper,
            IAPI api
        )
        {
            _context = context;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
            _api = api;
        }

        public async Task<ServiceResponse<GetAccountsDto>> GetAccountsBalance()
        {
            var response = new ServiceResponse<GetAccountsDto>();
            try
            {
                response.Data = new GetAccountsDto();
                response.Data.Accounts = new List<AccountDto>();

                var user = Utilities.GetCurrentUser(_context, _httpContextAccessor);

                var dbAccounts = await _context.Accounts
                                .Where(a => a.UserId == user.Id)
                                .ToListAsync();

                response.Data.Accounts = dbAccounts.Select(a => _mapper.Map<AccountDto>(a)).ToList();
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

        public async Task<ServiceResponse<GetAccountsDto>> GetAccountBalance(string accountId)
        {
            var response = new ServiceResponse<GetAccountsDto>();
            try
            {
                response.Data = new GetAccountsDto();
                response.Data.Account = new AccountDto();

                var user = Utilities.GetCurrentUser(_context, _httpContextAccessor);

                var dbAccount = await _context.Accounts
                                .Where(a => a.UserId == user.Id)
                                .Where(a => a.AccountId == accountId)
                                .SingleOrDefaultAsync();

                response.Data.Account = _mapper.Map<AccountDto>(dbAccount);
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

        public async Task<ServiceResponse<GetAccountsDto>> RefreshAccountsBalance()
        {
            var response = new ServiceResponse<GetAccountsDto>();
            try
            {
                response.Data = new GetAccountsDto();
                response.Data.Accounts = new List<AccountDto>();

                var user = Utilities.GetCurrentUser(_context, _httpContextAccessor);

                var accountResponse = _api.GetAccountsRequest(user);

                foreach (var account in accountResponse.Result.Accounts)
                {
                    var dbAccount = await _context.Accounts
                       .FirstOrDefaultAsync(a => a.AccountId == account.AccountId);

                    if (dbAccount is null)
                    {
                        var accountDto = Helper.MapPlaidStream(new AccountDto(), account, user);

                        Account accountDb = _mapper.Map<Account>(accountDto);
                        _context.Accounts.Add(accountDb);
                    }
                }

                await _context.SaveChangesAsync();

                var dbAccounts = await _context.Accounts
                                .Where(a => a.UserId == user.Id)
                                .ToListAsync();

                response.Data.Accounts = dbAccounts.Select(a => _mapper.Map<AccountDto>(a)).ToList();

            }
            catch (System.Exception ex)
            {
                Console.WriteLine("Account Balances failed");
                response.Success = false;
                response.Message = ex.Message;
                response.InnerException = ex.InnerException.Message;
                return response;
            }

            return response;
        }
    }
}
