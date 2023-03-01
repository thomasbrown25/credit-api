using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using financing_api.Data;
using Going.Plaid;
using AutoMapper;
using Microsoft.Extensions.Options;
using financing_api.Shared;
using financing_api.ApiHelper;
using financing_api.Dtos.Refresh;
using financing_api.Dtos.Account;
using financing_api.Utils;
using financing_api.Dtos.Transaction;

namespace financing_api.Services.RefreshService
{
    public class RefreshService : IRefreshService
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly PlaidCredentials _credentials;
        private readonly PlaidClient _client;
        private readonly IMapper _mapper;
        private readonly IAPI _api;

        public RefreshService(DataContext context,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            IOptions<PlaidCredentials> credentials,
            PlaidClient client,
            IMapper mapper,
            IAPI api
        )
        {
            _context = context;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _credentials = credentials.Value;
            _client = new PlaidClient(Going.Plaid.Environment.Development);
            _mapper = mapper;
            _api = api;
        }

        public async Task<ServiceResponse<RefreshDto>> RefreshAll()
        {
            var response = new ServiceResponse<RefreshDto>();
            response.Data = new RefreshDto();

            try
            {
                // *********** ACCOUNTS ************ //
                var user = Utilities.GetCurrentUser(_context, _httpContextAccessor);

                var accountResponse = _api.GetAccountsRequest(user);

                foreach (var account in accountResponse.Result.Accounts)
                {
                    var dbAccount = await _context.Accounts
                       .FirstOrDefaultAsync(a => a.AccountId == account.AccountId);

                    if (dbAccount is not null)
                    {
                        dbAccount = Helper.UpdateAccount(dbAccount, account, user);
                    }
                    else
                    {
                        var accountDto = Helper.MapPlaidStream(new AccountDto(), account, user);

                        Account accountDb = _mapper.Map<Account>(accountDto);
                        _context.Accounts.Add(accountDb);
                    }
                }

                await _context.SaveChangesAsync();

                // ********* TRANSACTIONS ************ //
                var result = await _api.GetTransactionsRequest(user);

                foreach (var transaction in result.Transactions)
                {
                    var dbTransaction = await _context.Transactions
                        .FirstOrDefaultAsync(t => t.TransactionId == transaction.TransactionId);

                    if (dbTransaction is null)
                    {
                        var transactionDto = Helper.MapPlaidStream(new TransactionDto(), transaction, user);

                        Transaction transactionDb = _mapper.Map<Transaction>(transactionDto);
                        _context.Transactions.Add(transactionDb);
                    }
                }

                await _context.SaveChangesAsync();

                // Get Account IDs 
                var getAccountRequest = new Going.Plaid.Accounts.AccountsGetRequest()
                {
                    ClientId = _configuration["PlaidClientId"],
                    Secret = _configuration["PlaidSecret"],
                    AccessToken = user.AccessToken
                };

                var accountResponse2 = await _client.AccountsGetAsync(getAccountRequest);

                if (accountResponse2.Error is not null)
                {
                    Console.WriteLine(accountResponse2.Error.ErrorMessage);
                    response.Success = false;
                    response.Message = accountResponse2.Error.ErrorMessage;
                    return response;
                }

                var recurringResponse = _api.GetRecurringTransactionsRequest(user, accountResponse2);

                if (recurringResponse.Result.Error is not null)
                {
                    Console.WriteLine(recurringResponse.Result.Error.ErrorMessage);
                    response.Success = false;
                    response.Message = recurringResponse.Result.Error.ErrorMessage;
                    return response;
                }

                // add streams to context 
                Helper.AddStreams(recurringResponse.Result.InflowStreams, _context, _mapper, user, EType.Income);
                Helper.AddStreams(recurringResponse.Result.OutflowStreams, _context, _mapper, user, EType.Expense);

                // save streams to context
                await _context.SaveChangesAsync();

                return response;

            }
            catch (System.Exception ex)
            {
                Console.WriteLine("Refresh Transactions failed: " + ex.Message);
                Console.WriteLine(ex.InnerException);
                Console.WriteLine(ex.StackTrace);
                response.Success = false;
                response.Message = ex.Message + " --------- Inner Exception: " + ex.InnerException.Message;
                return response;
            }

        }
    }
}