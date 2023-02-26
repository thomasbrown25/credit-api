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
using AutoMapper;
using Going.Plaid.Transactions;
using Microsoft.Extensions.Options;
using financing_api.Shared;
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
        private readonly IMapper _mapper;

        public TransactionsService(
            DataContext context,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            IOptions<PlaidCredentials> credentials,
            PlaidClient client,
            IMapper mapper
        )
        {
            _context = context;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _credentials = credentials.Value;
            _client = new PlaidClient(Going.Plaid.Environment.Development);
            _mapper = mapper;
        }

        public async Task<ServiceResponse<GetTransactionsDto>> GetTransactions()
        {
            var response = new ServiceResponse<GetTransactionsDto>();

            try
            {
                response.Data = new GetTransactionsDto();
                response.Data.Transactions = new List<TransactionDto>();
                response.Data.RecentTransactions = new List<TransactionDto>();
                response.Data.Expenses = new List<TransactionDto>();
                response.Data.Incomes = new List<TransactionDto>();
                response.Data.CashAccounts = new List<AccountDto>();
                response.Data.CreditAccounts = new List<AccountDto>();
                response.Data.CashAmount = new decimal?();

                // Get user for accessToken
                var user = Utilities.GetCurrentUser(_context, _httpContextAccessor);

                var startDate = DateTime.Today.AddMonths(-4);

                var request = new Going.Plaid.Transactions.TransactionsGetRequest()
                {
                    Options = new Going.Plaid.Entity.TransactionsGetRequestOptions()
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

                int i = 0;
                foreach (var transaction in result.Transactions)
                {
                    var transactionDto = Helper.MapPlaidStream(new TransactionDto(), transaction, user);

                    if (transaction.Category.Count > 1 && transaction.Category[1] == "Internal Account Transfer")
                        continue;

                    if (transaction.Amount > 0)
                    {
                        response.Data.Expenses.Add(transactionDto);
                    }
                    else
                    {
                        response.Data.Incomes.Add(transactionDto);
                    }
                    i++;
                }

                var dbTransactions = await _context.Transactions
                                   .Where(c => c.UserId == user.Id)
                                   .ToListAsync();

                response.Data.Transactions = dbTransactions.Select(c => _mapper.Map<TransactionDto>(c)).ToList();

                decimal? cashAmount = 0;
                decimal? creditAmount = 0;

                foreach (var account in result.Accounts)
                {
                    var accountDto = new AccountDto();

                    accountDto.AccountId = account.AccountId;
                    accountDto.Name = account.Name;
                    accountDto.Mask = account.Mask;
                    accountDto.OfficialName = account.OfficialName;
                    accountDto.Type = account.Subtype?.ToString();
                    accountDto.BalanceAvailable = account.Balances.Available;
                    accountDto.BalanceCurrent = account.Balances.Current;
                    accountDto.BalanceLimit = account.Balances.Limit;

                    if (account.Subtype == Going.Plaid.Entity.AccountSubtype.CreditCard)
                    {
                        creditAmount = creditAmount + account.Balances.Current;
                        response.Data.CreditAccounts.Add(accountDto);
                    }
                    else
                    {
                        cashAmount = cashAmount + account.Balances.Available;
                        response.Data.CashAccounts.Add(accountDto);
                    }
                }
                response.Data.CashAmount = cashAmount;
                response.Data.CreditAmount = creditAmount;
                cashAmount = 0;
                creditAmount = 0;
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

        public async Task<ServiceResponse<GetTransactionsDto>> RefreshTransactions()
        {
            var response = new ServiceResponse<GetTransactionsDto>();
            try
            {
                response.Data = new GetTransactionsDto();
                response.Data.Transactions = new List<TransactionDto>();

                // Get user for accessToken6
                var user = Utilities.GetCurrentUser(_context, _httpContextAccessor);

                var startDate = DateTime.Today.AddMonths(-4);

                var request = new Going.Plaid.Transactions.TransactionsGetRequest()
                {
                    Options = new Going.Plaid.Entity.TransactionsGetRequestOptions()
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

                var dbTransactions = await _context.Transactions
                                   .Where(c => c.UserId == user.Id)
                                   .ToListAsync();

                response.Data.Transactions = dbTransactions.Select(c => _mapper.Map<TransactionDto>(c)).ToList();

            }
            catch (System.Exception ex)
            {
                Console.WriteLine("Recurring Transactions failed: " + ex.Message);
                response.Success = false;
                response.Message = ex.Message + " --------- Inner Exception: " + ex.InnerException.Message;
                return response;
            }

            return response;
        }

        // Get Current Spend for the month from Plaid API
        public async Task<ServiceResponse<CurrentMonthDto>> GetCurrentSpendForMonth()
        {
            var response = new ServiceResponse<CurrentMonthDto>();
            response.Data = new CurrentMonthDto();

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
                if (transaction.Category.Count > 1 && transaction.Category[1] == "Internal Account Transfer")
                    continue;

                if (transaction.Amount > 0)
                {
                    response.Data.Expense += transaction.Amount;
                }
                else
                {
                    response.Data.Income += transaction.Amount;
                }
            }

            response.Data.Expense = response.Data.Expense * -1;
            response.Data.Income = response.Data.Income * -1;

            return response;
        }

        // Get Recurring Transactions
        public async Task<ServiceResponse<GetRecurringDto>> GetRecurringTransactions()
        {
            var response = new ServiceResponse<GetRecurringDto>();
            response.Data = new GetRecurringDto();

            try
            {
                // Get user for accessToken
                var user = Utilities.GetCurrentUser(_context, _httpContextAccessor);

                var dbRecurrings = await _context.Recurrings
                    .Where(r => r.UserId == user.Id)
                    .ToListAsync();

                response.Data.Transactions = dbRecurrings.Select(r => _mapper.Map<RecurringDto>(r)).OrderByDescending(r => r.DueDate).ToList();

                response.Data.Incomes = dbRecurrings
                                            .Where(r => r.Type == Enum.GetName<EType>(EType.Income))
                                            .Where(r => r.IsActive == true)
                                            .Select(r => _mapper.Map<RecurringDto>(r))
                                            .OrderBy(r => r.DueDate)
                                            .ToList();

                response.Data.Expenses = dbRecurrings
                                            .Where(r => r.Type == Enum.GetName<EType>(EType.Expense))
                                            .Where(r => r.IsActive == true)
                                            .Select(r => _mapper.Map<RecurringDto>(r))
                                            .OrderBy(r => r.DueDate)
                                            .ToList();

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


        public async Task<ServiceResponse<GetRecurringDto>> GetExpenses()
        {
            var response = new ServiceResponse<GetRecurringDto>();
            response.Data = new GetRecurringDto();

            try
            {
                // Get user for accessToken
                var user = Utilities.GetCurrentUser(_context, _httpContextAccessor);

                var dbRecurrings = await _context.Recurrings
                    .Where(r => r.UserId == user.Id)
                    .ToListAsync();

                response.Data.Expenses = dbRecurrings
                                            .Where(r => r.Type == Enum.GetName<EType>(EType.Expense))
                                            .Where(r => r.IsActive == true)
                                            .Select(r => _mapper.Map<RecurringDto>(r))
                                            .OrderBy(r => r.DueDate)
                                            .ToList();

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

        public async Task<ServiceResponse<List<RecurringDto>>> RefreshRecurringTransactions()
        {
            var response = new ServiceResponse<List<RecurringDto>>();
            try
            {
                response.Data = new List<RecurringDto>();

                // Get user for accessToken6
                var user = Utilities.GetCurrentUser(_context, _httpContextAccessor);

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
                    if (!inflowStream.Category.Contains("Internal Account Transfer"))
                    {
                        var dbRecurring = await _context.Recurrings
                            .FirstOrDefaultAsync(r => r.StreamId == inflowStream.StreamId);

                        if (dbRecurring is null)
                        {
                            var recurring = Helper.MapPlaidStream(new RecurringDto(), inflowStream, user, EType.Income);

                            // Map recurring with recurringDto db
                            Recurring recurringDb = _mapper.Map<Recurring>(recurring);
                            _context.Recurrings.Add(recurringDb);
                        }
                    }
                }

                foreach (var outflowStream in RecurringResponse.OutflowStreams)
                {
                    if (!outflowStream.Category.Contains("Internal Account Transfer") && !outflowStream.Category.Contains("Payroll"))
                    {


                        var dbRecurring = await _context.Recurrings
                            .FirstOrDefaultAsync(r => r.StreamId == outflowStream.StreamId);

                        if (dbRecurring is null)
                        {
                            var recurring = Helper.MapPlaidStream(new RecurringDto(), outflowStream, user, EType.Expense);

                            // Map recurring with recurringDto db
                            Recurring recurringDb = _mapper.Map<Recurring>(recurring);
                            _context.Recurrings.Add(recurringDb);
                        }
                    }
                }

                // save to Db
                await _context.SaveChangesAsync();

                var dbRecurrings = await _context.Recurrings
                    .Where(c => c.UserId == user.Id)
                    .ToListAsync();

                // get data from DB
                response.Data = dbRecurrings.Select(c => _mapper.Map<RecurringDto>(c)).ToList();

            }
            catch (System.Exception ex)
            {
                Console.WriteLine("Recurring Transactions failed: " + ex.Message);
                response.Success = false;
                response.Message = ex.Message + " --------- Inner Exception: " + ex.InnerException.Message;
                return response;
            }

            return response;
        }

        public async Task<ServiceResponse<List<RecurringDto>>> AddRecurringTransaction(AddRecurringDto newRecurring)
        {
            var response = new ServiceResponse<List<RecurringDto>>();

            try
            {
                // Map recurring with recurringDto
                Recurring recurring = _mapper.Map<Recurring>(newRecurring);

                // Set the current user Id
                recurring.UserId = Utilities.GetUserId(_httpContextAccessor);

                // Add recurring and save to DB
                _context.Recurrings.Add(recurring);
                await _context.SaveChangesAsync();

                response.Data = await _context.Recurrings
                    .Where(c => c.UserId == Utilities.GetUserId(_httpContextAccessor))
                    .Select(c => _mapper.Map<RecurringDto>(c))
                    .ToListAsync();

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

        public async Task<ServiceResponse<RecurringDto>> UpdateRecurringTransaction(UpdateRecurringDto updatedRecurring)
        {
            var response = new ServiceResponse<RecurringDto>();

            try
            {
                Recurring recurring = await _context.Recurrings
                    .FirstOrDefaultAsync(c => c.Id == updatedRecurring.Id);

                // confirm that current user is owner
                if (recurring.UserId == Utilities.GetUserId(_httpContextAccessor))
                {
                    _mapper.Map<UpdateRecurringDto, Recurring>(updatedRecurring, recurring);


                    await _context.SaveChangesAsync();
                    response.Data = _mapper.Map<RecurringDto>(recurring);
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

        public async Task<ServiceResponse<GetTransactionsDto>> GetAccountTransactions(string accountId)
        {
            var response = new ServiceResponse<GetTransactionsDto>();

            try
            {
                response.Data = new GetTransactionsDto();
                response.Data.Transactions = new List<TransactionDto>();

                // Get user for accessToken
                var user = Utilities.GetCurrentUser(_context, _httpContextAccessor);

                var dbTransactions = await _context.Transactions
                    .Where(r => r.UserId == user.Id)
                    .ToListAsync();

                response.Data.Transactions = dbTransactions
                                            .Where(t => t.AccountId == accountId)
                                            .Select(t => _mapper.Map<TransactionDto>(t))
                                            .ToList();

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
