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
            response.Data = new GetTransactionsDto();

            // Get user for accessToken
            var user = Utilities.GetCurrentUser(_context, _httpContextAccessor);

            if (user == null || user.AccessToken == null)
            {
                response.Success = false;
                response.Message = "User does not have access token";
                return response;
            }

            var startDate = DateTime.Today.AddDays(-30);

            var request = new Going.Plaid.Transactions.TransactionsGetRequest()
            {
                Options = new TransactionsGetRequestOptions()
                {
                    Count = 15
                },
                ClientId = _configuration.GetSection("AppSettings:Plaid:ClientId").Value,
                Secret = _configuration.GetSection("AppSettings:Plaid:Secret").Value,
                AccessToken = user.AccessToken,
                StartDate = new DateOnly(startDate.Year, startDate.Month, startDate.Day),
                EndDate = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day)
            };

            var result = await _client.TransactionsGetAsync(request);

            if (result.Error is not null)
                Console.WriteLine(result.Error.ErrorMessage);

            foreach (var transaction in result.Transactions)
            {
                var transactionDto = new TransactionDto();

                transactionDto.AccountId = transaction.AccountId;
                transactionDto.Name = transaction.Name;
                transactionDto.Amount = transaction.Amount;
                transactionDto.Pending = transaction.Pending;
                transactionDto.Date = transaction.Date.ToDateTime(TimeOnly.Parse("00:00:00"));
                transactionDto.Categories = transaction.Category;

                response.Data.Transactions.Add(transactionDto);
            }

            return response;
        }

        // Get Recent Transactions
        public async Task<ServiceResponse<GetTransactionsDto>> GetRecentTransactions(uint count)
        {
            var response = new ServiceResponse<GetTransactionsDto>();
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

            var request = new Going.Plaid.Transactions.TransactionsGetRequest()
            {
                Options = new TransactionsGetRequestOptions()
                {
                    Count = 15
                },
                ClientId = _configuration.GetSection("AppSettings:Plaid:ClientId").Value,
                Secret = _configuration.GetSection("AppSettings:Plaid:Secret").Value,
                AccessToken = user.AccessToken,
                StartDate = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1), // gets the first day of the month
                EndDate = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day)
            };

            var result = await _client.TransactionsGetAsync(request);

            if (result.Error is not null)
                Console.WriteLine("Plaid Error: " + result.Error.ErrorMessage);

            foreach (var transaction in result.Transactions)
            {
                var transactionDto = new TransactionDto();

                transactionDto.AccountId = transaction.AccountId;
                transactionDto.Name = transaction.Name;
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
                    ClientId = _configuration.GetSection("AppSettings:Plaid:ClientId").Value,
                    Secret = _configuration.GetSection("AppSettings:Plaid:Secret").Value,
                    AccessToken = user.AccessToken,
                    StartDate = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1), // gets the first day of the month
                    EndDate = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day)
                }
            );

            if (result.Error is not null)
                Console.WriteLine(result.Error.ErrorMessage);

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

                // Get user for accessToken
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
                    ClientId = _configuration.GetSection("AppSettings:Plaid:ClientId").Value,
                    Secret = _configuration.GetSection("AppSettings:Plaid:Secret").Value,
                    AccessToken = user.AccessToken
                };

                var accountResponse = await _client.AccountsGetAsync(getAccountRequest);

                if (accountResponse.Error is not null)
                    Console.WriteLine(accountResponse.Error.ErrorMessage);



                var getRecurringRequest = new Going.Plaid.Transactions.TransactionsRecurringGetRequest()
                {
                    ClientId = _configuration.GetSection("AppSettings:Plaid:ClientId").Value,
                    Secret = _configuration.GetSection("AppSettings:Plaid:Secret").Value,
                    AccessToken = user.AccessToken,
                    AccountIds = Utilities.GetAccountIds(accountResponse.Accounts)
                };



                var RecurringResponse = await _client.TransactionsRecurringGetAsync(getRecurringRequest);

                if (RecurringResponse.Error is not null)
                    Console.WriteLine(RecurringResponse.Error.ErrorMessage);

                foreach (var inflowStream in RecurringResponse.InflowStreams)
                {
                    var inflowStreamsDto = new InflowStreamsDto();

                    inflowStreamsDto.AccountId = inflowStream.AccountId;
                    inflowStreamsDto.AverageAmount = inflowStream.AverageAmount;
                    inflowStreamsDto.Categories = inflowStream.Category;
                    inflowStreamsDto.Description = inflowStream.Description;
                    inflowStreamsDto.FirstDate = inflowStream.FirstDate.ToDateTime(TimeOnly.Parse("00:00:00"));
                    inflowStreamsDto.Frequency = inflowStream.Frequency;
                    inflowStreamsDto.IsActive = inflowStream.IsActive;
                    inflowStreamsDto.LastAmount = inflowStream.LastAmount;
                    inflowStreamsDto.LastDate = inflowStream.LastDate.ToDateTime(TimeOnly.Parse("00:00:00"));
                    inflowStreamsDto.MerchantName = inflowStream.MerchantName;
                    inflowStreamsDto.Status = inflowStream.Status;
                    inflowStreamsDto.StreamId = inflowStream.StreamId;

                    response.Data.InflowStream.Add(inflowStreamsDto);
                }

                foreach (var outflowStream in RecurringResponse.OutflowStreams)
                {
                    var outflowStreamsDto = new OutflowStreamsDto();

                    outflowStreamsDto.AccountId = outflowStream.AccountId;
                    outflowStreamsDto.AverageAmount = outflowStream.AverageAmount;
                    outflowStreamsDto.Categories = outflowStream.Category;
                    outflowStreamsDto.Description = outflowStream.Description;
                    outflowStreamsDto.FirstDate = outflowStream.FirstDate.ToDateTime(TimeOnly.Parse("00:00:00"));
                    outflowStreamsDto.Frequency = outflowStream.Frequency;
                    outflowStreamsDto.IsActive = outflowStream.IsActive;
                    outflowStreamsDto.LastAmount = outflowStream.LastAmount;
                    outflowStreamsDto.LastDate = outflowStream.LastDate.ToDateTime(TimeOnly.Parse("00:00:00"));
                    outflowStreamsDto.MerchantName = outflowStream.MerchantName;
                    outflowStreamsDto.Status = outflowStream.Status;
                    outflowStreamsDto.StreamId = outflowStream.StreamId;

                    response.Data.OutflowStream.Add(outflowStreamsDto);
                }

            }
            catch (System.Exception)
            {
                Console.WriteLine("Recurring Transactions failed");
            }

            return response;
        }
    }
}
