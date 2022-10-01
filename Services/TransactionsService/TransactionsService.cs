using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using financing_api.Data;
using Acklann.Plaid;
using System.Security.Claims;
using financing_api.Utils;
using financing_api.Dtos.Transaction;
using financing_api.Logging;

namespace financing_api.Services.TransactionsService
{
    public class TransactionsService : ITransactionsService
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TransactionsService(
            DataContext context,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor
        )
        {
            _context = context;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        // Get All Transactions from Plaid
        public async Task<ServiceResponse<List<Acklann.Plaid.Entity.Transaction>>> GetTransactions()
        {
            var response = new ServiceResponse<List<Acklann.Plaid.Entity.Transaction>>();
            response.Data = new List<Acklann.Plaid.Entity.Transaction>();

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

            var result = await client.FetchTransactionsAsync(
                new Acklann.Plaid.Transactions.GetTransactionsRequest()
                {
                    ClientId = _configuration.GetSection("AppSettings:Plaid:ClientId").Value,
                    Secret = _configuration.GetSection("AppSettings:Plaid:Secret").Value,
                    AccessToken = user.AccessToken,
                    StartDate = DateTime.Today.AddMonths(-1),
                    EndDate = DateTime.Today
                }
            );




            foreach (var transaction in result.Transactions)
            {
                response.Data.Add(transaction);
            }

            return response;
        }

        // Get Recent Transactions
        public async Task<
            ServiceResponse<List<Acklann.Plaid.Entity.Transaction>>
        > GetRecentTransactions(uint count)
        {
            var response = new ServiceResponse<List<Acklann.Plaid.Entity.Transaction>>();
            response.Data = new List<Acklann.Plaid.Entity.Transaction>();

            // Get user for accessToken
            var user = UtilityMethods.GetCurrentUser(_context, _httpContextAccessor);

            if (user == null || user.AccessToken == null)
            {
                response.Success = false;
                response.Message = "User does not have access token";
                return response;
            }

            var options = new Acklann.Plaid.Transactions.GetTransactionsRequest.PaginationOptions();
            options.Total = 15;

            // Create plaid client
            var client = new PlaidClient(Acklann.Plaid.Environment.Development);

            var result = await client.FetchTransactionsAsync(
                new Acklann.Plaid.Transactions.GetTransactionsRequest()
                {
                    ClientId = _configuration.GetSection("AppSettings:Plaid:ClientId").Value,
                    Secret = _configuration.GetSection("AppSettings:Plaid:Secret").Value,
                    AccessToken = user.AccessToken,
                    StartDate = DateTime.Today.AddMonths(-1),
                    EndDate = DateTime.Today,
                    Options = options
                }
            );

            if (result == null)
            {
                Console.WriteLine("Plaid API result is null");
            }
            else
            {
                Console.WriteLine(result);
            }

            foreach (var transaction in result.Transactions)
            {
                response.Data.Add(transaction);
            }

            return response;
        }

        public async Task<ServiceResponse<GetTransactionsDto>> GetCurrentMonthTransactions()
        {
            var response = new ServiceResponse<GetTransactionsDto>();
            response.Data = new GetTransactionsDto();
            response.Data.Transactions = new List<TransactionDto>();

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

            var result = await client.FetchTransactionsAsync(
                new Acklann.Plaid.Transactions.GetTransactionsRequest()
                {
                    ClientId = _configuration.GetSection("AppSettings:Plaid:ClientId").Value,
                    Secret = _configuration.GetSection("AppSettings:Plaid:Secret").Value,
                    AccessToken = user.AccessToken,
                    StartDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1), // gets the first day of the month
                    EndDate = DateTime.Today
                }
            );

            foreach (var transaction in result.Transactions)
            {
                var transactionDto = new TransactionDto();

                transactionDto.AccountId = transaction.AccountId;
                transactionDto.Name = transaction.Name;
                transactionDto.Date = transaction.Date;
                transactionDto.Amount = transaction.Amount;
                transactionDto.Pending = transaction.Pending;
                transactionDto.Date = transaction.Date;
                transactionDto.Categories = transaction.Categories;

                response.Data.Transactions.Add(transactionDto);

                if (transaction.Amount > 0)
                {
                    response.Data.CurrentSpendAmount += transaction.Amount;
                }
            }

            return response;
        }

        // Get Current Spend for the month from Plaid API
        public async Task<ServiceResponse<decimal>> GetCurrentSpendForMonth()
        {
            var response = new ServiceResponse<decimal>();

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

            var result = await client.FetchTransactionsAsync(
                new Acklann.Plaid.Transactions.GetTransactionsRequest()
                {
                    ClientId = _configuration.GetSection("AppSettings:Plaid:ClientId").Value,
                    Secret = _configuration.GetSection("AppSettings:Plaid:Secret").Value,
                    AccessToken = user.AccessToken,
                    StartDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1), // gets the first day of the month
                    EndDate = DateTime.Today
                }
            );

            foreach (var transaction in result.Transactions)
            {
                response.Data += transaction.Amount;
            }

            return response;
        }

        // Get Recurring Transactions
        public async Task<ServiceResponse<List<Acklann.Plaid.Entity.Transaction>>> GetRecurringTransactions()
        {
            var response = new ServiceResponse<List<Acklann.Plaid.Entity.Transaction>>();
            response.Data = new List<Acklann.Plaid.Entity.Transaction>();

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

            var result = await client.FetchTransactionsAsync(
                new Acklann.Plaid.Transactions.GetTransactionsRequest()
                {
                    ClientId = _configuration.GetSection("AppSettings:Plaid:ClientId").Value,
                    Secret = _configuration.GetSection("AppSettings:Plaid:Secret").Value,
                    AccessToken = user.AccessToken,
                    StartDate = DateTime.Today.AddMonths(-1),
                    EndDate = DateTime.Today
                }
            );

            foreach (var transaction in result.Transactions)
            {
                response.Data.Add(transaction);
            }

            return response;
        }
    }
}
