using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using financing_api.Data;
using System.Security.Claims;
using financing_api.Dtos.Transaction;
using financing_api.Logging;
using Going.Plaid;
using Going.Plaid.Transactions;
using Microsoft.Extensions.Options;
using financing_api.Shared;
using Going.Plaid.Entity;
using financing_api.Dtos.Account;
using financing_api.Utils;
using System.Collections;

namespace financing_api.Services.TransactionsService
{
    public class TransactionsService : ITransactionsService
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly PlaidCredentials _credentials;
        private readonly PlaidClient _client;

        public TransactionsService(
            DataContext context,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            IOptions<PlaidCredentials> credentials,
            PlaidClient client
        )
        {
            _context = context;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _credentials = credentials.Value;
            _client = new PlaidClient(Going.Plaid.Environment.Development); ;
        }

        // Get All Transactions from Plaid
        public async Task<ServiceResponse<GetTransactionsDto>> GetTransactions()
        {
            var response = new ServiceResponse<GetTransactionsDto>();

            try
            {
                response.Data = new GetTransactionsDto();
                response.Data.Transactions = new List<TransactionDto>();
                response.Data.Accounts = new List<AccountDto>();

                // Get user for accessToken
                var user = Utilities.GetCurrentUser(_context, _httpContextAccessor);

                if (user == null || user.AccessToken == null)
                {
                    response.Success = false;
                    response.Message = "User does not have access token";
                    return response;
                }

                var startDate = DateTime.Today.AddMonths(-4);

                var request = new Going.Plaid.Transactions.TransactionsGetRequest()
                {
                    Options = new TransactionsGetRequestOptions()
                    {
                        Count = 500
                    },
                    ClientId = _configuration["PlaidClientId"],
                    Secret = _configuration["PlaidSecret"],
                    AccessToken = user.AccessToken,
                    StartDate = new DateOnly(startDate.Year, startDate.Month, startDate.Day),
                    EndDate = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day)
                };

                var result = await _client.TransactionsGetAsync(request);

                if (result is not null && result.Error is not null)
                {
                    Console.WriteLine("Plaid Error: " + result.Error.ErrorMessage);
                    response.Success = false;
                    response.Message = result.Error.ErrorMessage;
                    return response;
                }

                foreach (var transaction in result.Transactions)
                {
                    var transactionDto = new TransactionDto();

                    transactionDto.AccountId = transaction.AccountId;
                    transactionDto.Name = transaction.Name;
                    transactionDto.MerchantName = transaction.MerchantName;
                    transactionDto.Amount = transaction.Amount;
                    transactionDto.Pending = transaction.Pending;
                    transactionDto.Date = transaction.Date.ToDateTime(TimeOnly.Parse("00:00:00"));
                    transactionDto.Categories = transaction.Category;

                    response.Data.Transactions.Add(transactionDto);
                }

                foreach (var account in result.Accounts)
                {
                    var accountDto = new AccountDto();

                    accountDto.Id = account.AccountId;
                    accountDto.Name = account.Name;
                    accountDto.Mask = account.Mask;
                    accountDto.OfficialName = account.OfficialName;
                    accountDto.Type = account.Type;

                    accountDto.Balance = new AccountBalanceDto();

                    accountDto.Balance.Available = account.Balances.Available;
                    accountDto.Balance.Current = account.Balances.Current;
                    accountDto.Balance.Limit = account.Balances.Limit;

                    response.Data.Accounts.Add(accountDto);
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("Get Transactions failed: " + ex.Message);
                response.Success = false;
                response.Message = ex.Message;
                return response;
            }
            return response;
        }

        // Get Recent Transactions
        public async Task<ServiceResponse<GetTransactionsDto>> GetRecentTransactions(uint count)
        {
            var response = new ServiceResponse<GetTransactionsDto>();
            try
            {
                response.Data = new GetTransactionsDto();
                response.Data.Transactions = new List<TransactionDto>();
                response.Data.Accounts = new List<AccountDto>();
                response.Data.Categories = new Dictionary<string, decimal>();
                response.Data.CategoryLabels = new HashSet<string>();
                response.Data.CategoryAmounts = new HashSet<decimal>();

                // Get user for accessToken
                var user = Utilities.GetCurrentUser(_context, _httpContextAccessor);

                if (user == null || user.AccessToken == null)
                {
                    response.Success = false;
                    response.Message = "User does not have access token";
                    return response;
                }

                Console.WriteLine("Getting plaid client id in recent transactions: " + _configuration["PlaidClientId"]);

                var request = new Going.Plaid.Transactions.TransactionsGetRequest()
                {
                    Options = new TransactionsGetRequestOptions()
                    {
                        Count = 500
                    },
                    ClientId = _configuration["PlaidClientId"],
                    Secret = _configuration["PlaidSecret"],
                    AccessToken = user.AccessToken,
                    StartDate = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1), // gets the first day of the month
                    EndDate = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day)
                };

                var result = await _client.TransactionsGetAsync(request);

                if (result.Error is not null)
                {
                    Console.WriteLine("Plaid Error: " + result.Error.ErrorMessage);
                    response.Success = false;
                    response.Message = result.Error.ErrorMessage;
                    return response;
                }

                var categoryList = new Stack<string>();

                foreach (var transaction in result.Transactions)
                {
                    var transactionDto = new TransactionDto();

                    transactionDto.AccountId = transaction.AccountId;
                    transactionDto.Name = transaction.Name;
                    transactionDto.Amount = transaction.Amount;
                    transactionDto.Pending = transaction.Pending;
                    transactionDto.Date = transaction.Date.ToDateTime(TimeOnly.Parse("00:00:00"));
                    transactionDto.Categories = transaction.Category;

                    if (!response.Data.Categories.ContainsKey(transaction.Category[0]))
                    {
                        response.Data.Categories.Add(transaction.Category[0], transaction.Amount);
                    }
                    else
                    {
                        response.Data.Categories[transaction.Category[0]] = response.Data.Categories[transaction.Category[0]] + transaction.Amount;
                    }

                    response.Data.Transactions.Add(transactionDto);
                }

                response.Data.CategoryLabels = response.Data.Categories.Keys.ToHashSet();
                response.Data.CategoryAmounts = response.Data.Categories.Values.ToHashSet();

            }
            catch (System.Exception ex)
            {
                Console.WriteLine("Recent Transactions failed: " + ex.Message);
                response.Success = false;
                response.Message = ex.Message;
                return response;
            }


            return response;
        }

        // Get Current Spend for the month from Plaid API
        public async Task<ServiceResponse<decimal>> GetCurrentSpendForMonth()
        {
            var response = new ServiceResponse<decimal>();

            // Get user for accessToken
            var user = Utilities.GetCurrentUser(_context, _httpContextAccessor);

            if (user == null || user.AccessToken == null)
            {
                response.Success = false;
                response.Message = "User does not have access token";
                return response;
            }

            var result = await _client.TransactionsGetAsync(
                new()
                {
                    ClientId = _configuration["PlaidClientId"],
                    Secret = _configuration["PlaidSecret"],
                    AccessToken = user.AccessToken,
                    StartDate = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1), // gets the first day of the month
                    EndDate = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day)
                }
            );

            if (result.Error is not null)
            {
                Console.WriteLine(result.Error.ErrorMessage);
                response.Success = false;
                response.Error = new Error();
                response.Error.ErrorCode = result.Error.ErrorCode.ToString();
                response.Error.ErrorMessage = result.Error.ErrorMessage;
                return response;
            }

            foreach (var transaction in result.Transactions)
            {

                if (transaction.Amount > 0)
                {
                    response.Data += transaction.Amount;
                }
            }

            return response;
        }

        // Get Recurring Transactions
        public async Task<ServiceResponse<GetRecurringDto>> GetRecurringTransactions()
        {
            var response = new ServiceResponse<GetRecurringDto>();
            try
            {
                response.Data = new GetRecurringDto();
                response.Data.InflowStream = new List<InflowStreamsDto>();
                response.Data.OutflowStream = new List<OutflowStreamsDto>();

                // Get user for accessToken6
                var user = Utilities.GetCurrentUser(_context, _httpContextAccessor);

                if (user == null || user.AccessToken == null)
                {
                    response.Success = false;
                    response.Message = "User does not have access token";
                    return response;
                }

                // Get Account IDs 
                var getAccountRequest = new Going.Plaid.Accounts.AccountsGetRequest()
                {
                    ClientId = _configuration["PlaidClientId"],
                    Secret = _configuration["PlaidSecret"],
                    AccessToken = user.AccessToken
                };

                var accountResponse = await _client.AccountsGetAsync(getAccountRequest);

                if (accountResponse.Error is not null)
                {
                    Console.WriteLine(accountResponse.Error.ErrorMessage);
                    response.Success = false;
                    response.Message = accountResponse.Error.ErrorMessage;
                    return response;
                }


                var getRecurringRequest = new Going.Plaid.Transactions.TransactionsRecurringGetRequest()
                {
                    ClientId = _configuration["PlaidClientId"],
                    Secret = _configuration["PlaidSecret"],
                    AccessToken = user.AccessToken,
                    AccountIds = Utilities.GetAccountIds(accountResponse.Accounts),

                };



                var RecurringResponse = await _client.TransactionsRecurringGetAsync(getRecurringRequest);

                if (RecurringResponse.Error is not null)
                {
                    Console.WriteLine(RecurringResponse.Error.ErrorMessage);
                    response.Success = false;
                    response.Message = RecurringResponse.Error.ErrorMessage;
                    return response;
                }

                foreach (var inflowStream in RecurringResponse.InflowStreams)
                {
                    var inflowStreamsDto = new InflowStreamsDto();

                    inflowStreamsDto.AccountId = inflowStream.AccountId;
                    inflowStreamsDto.AverageAmount = inflowStream.AverageAmount;
                    inflowStreamsDto.Categories = inflowStream.Category;
                    inflowStreamsDto.Description = inflowStream.Description;
                    inflowStreamsDto.FirstDate = inflowStream.FirstDate.ToDateTime(TimeOnly.Parse("00:00:00"));
                    inflowStreamsDto.Frequency = inflowStream.Frequency.ToString();
                    inflowStreamsDto.IsActive = inflowStream.IsActive;
                    inflowStreamsDto.LastAmount = inflowStream.LastAmount;
                    inflowStreamsDto.LastDate = inflowStream.LastDate.ToDateTime(TimeOnly.Parse("00:00:00"));
                    inflowStreamsDto.MerchantName = inflowStream.MerchantName;
                    inflowStreamsDto.Status = inflowStream.Status;
                    inflowStreamsDto.StreamId = inflowStream.StreamId;

                    response.Data.InflowStream.Add(inflowStreamsDto);
                }


                for (var i = 0; i < 10; i++)
                {
                    var outflowStream = RecurringResponse.OutflowStreams;
                    var outflowStreamsDto = new OutflowStreamsDto();

                    outflowStreamsDto.AccountId = RecurringResponse.OutflowStreams[i].AccountId;
                    outflowStreamsDto.AverageAmount = RecurringResponse.OutflowStreams[i].AverageAmount;
                    outflowStreamsDto.Categories = RecurringResponse.OutflowStreams[i].Category;
                    outflowStreamsDto.Description = RecurringResponse.OutflowStreams[i].Description;
                    outflowStreamsDto.FirstDate = RecurringResponse.OutflowStreams[i].FirstDate.ToDateTime(TimeOnly.Parse("00:00:00"));
                    outflowStreamsDto.Frequency = RecurringResponse.OutflowStreams[i].Frequency.ToString();
                    outflowStreamsDto.IsActive = RecurringResponse.OutflowStreams[i].IsActive;
                    outflowStreamsDto.LastAmount = RecurringResponse.OutflowStreams[i].LastAmount;
                    outflowStreamsDto.LastDate = RecurringResponse.OutflowStreams[i].LastDate.ToDateTime(TimeOnly.Parse("00:00:00"));
                    outflowStreamsDto.MerchantName = RecurringResponse.OutflowStreams[i].MerchantName;
                    outflowStreamsDto.Status = RecurringResponse.OutflowStreams[i].Status;
                    outflowStreamsDto.StreamId = RecurringResponse.OutflowStreams[i].StreamId;

                    response.Data.OutflowStream.Add(outflowStreamsDto);
                }

            }
            catch (System.Exception ex)
            {
                Console.WriteLine("Recurring Transactions failed: " + ex.Message);
                response.Success = false;
                response.Message = ex.Message;
                return response;
            }

            return response;
        }
    }
}
