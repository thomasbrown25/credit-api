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
using financing_api.ApiHelper;

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
        private readonly IAPI _api;

        public TransactionsService(
            DataContext context,
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

        public async Task<ServiceResponse<GetTransactionsDto>> GetTransactions()
        {
            var response = new ServiceResponse<GetTransactionsDto>();

            try
            {
                response.Data = new GetTransactionsDto();

                var user = Utilities.GetCurrentUser(_context, _httpContextAccessor);

                var dbTransactions = await _context.Transactions
                                   .Where(c => c.UserId == user.Id)
                                   .OrderByDescending(c => c.Date)
                                   .ToListAsync();

                response.Data.Transactions = dbTransactions.Select(c => _mapper.Map<TransactionDto>(c)).ToList();
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

                var user = Utilities.GetCurrentUser(_context, _httpContextAccessor);

                var result = await _api.GetTransactionsRequest(user);

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
                Console.WriteLine("Refresh Transactions failed: " + ex.Message);
                response.Success = false;
                response.Message = ex.Message + " --------- Inner Exception: " + ex.InnerException.Message;
                return response;
            }

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

                response.Data.Transactions = dbRecurrings
                                                .Where(r => r.IsActive == true)
                                                .Select(r => _mapper.Map<RecurringDto>(r))
                                                .OrderByDescending(r => r.DueDate).ToList();

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

                response.Data.TotalIncome = Helper.GetTotalIncome(response.Data.Incomes);

                response.Data.Tithes = Helper.GetTithes(response.Data.Incomes);

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

        public async Task<ServiceResponse<GetRecurringDto>> RefreshRecurringTransactions()
        {
            var response = new ServiceResponse<GetRecurringDto>();

            try
            {
                response.Data = new GetRecurringDto();

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

                var recurringResponse = _api.GetRecurringTransactionsRequest(user, accountResponse);

                if (recurringResponse.Result.Error is not null)
                {
                    Console.WriteLine(recurringResponse.Result.Error.ErrorMessage);
                    response.Success = false;
                    response.Message = recurringResponse.Result.Error.ErrorMessage;
                    return response;
                }

                // add streams to context 
                Helper.AddStreams(recurringResponse.Result.InflowStreams, _context, _mapper, user);
                Helper.AddStreams(recurringResponse.Result.OutflowStreams, _context, _mapper, user);

                // save streams to context
                await _context.SaveChangesAsync();

                // get all recurrings
                var dbRecurrings = await _context.Recurrings
                    .Where(c => c.UserId == user.Id)
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

        public async Task<ServiceResponse<GetRecurringDto>> UpdateRecurringTransaction(UpdateRecurringDto updatedRecurring)
        {
            var response = new ServiceResponse<GetRecurringDto>();
            response.Data = new GetRecurringDto();

            try
            {
                var user = Utilities.GetCurrentUser(_context, _httpContextAccessor);

                Recurring recurring = await _context.Recurrings
                    .FirstOrDefaultAsync(c => c.Id == updatedRecurring.Id);

                // confirm that current user is owner
                if (recurring.UserId == user.Id)
                {
                    _mapper.Map<UpdateRecurringDto, Recurring>(updatedRecurring, recurring);

                    var dbRecurrings = await _context.Recurrings
                                        .Where(r => r.UserId == user.Id)
                                        .ToListAsync();

                    await _context.SaveChangesAsync();

                    response.Data.Expenses = dbRecurrings
                                            .Where(r => r.Type == Enum.GetName<EType>(EType.Expense))
                                            .Where(r => r.IsActive == true)
                                            .Select(r => _mapper.Map<RecurringDto>(r))
                                            .OrderBy(r => r.DueDate)
                                            .ToList();
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
                    .OrderByDescending(r => r.Date)
                    .ToListAsync();

                response.Data.Transactions = dbTransactions
                                            .Where(t => t.AccountId == accountId)
                                            .Select(t => _mapper.Map<TransactionDto>(t))
                                            .ToList();

                var todayTransactions = dbTransactions
                                            .Where(t => t.AccountId == accountId)
                                            .Where(t => t.Date == DateTime.Today)
                                            .Select(t => _mapper.Map<TransactionDto>(t))
                                            .ToList();

                decimal totalAmount = 0;
                foreach (var transaction in todayTransactions)
                {
                    if (transaction.Amount > 0)
                    {
                        totalAmount = totalAmount + transaction.Amount;
                    }
                }
                response.Data.TodaySpendAmount = totalAmount;

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

        public async Task<ServiceResponse<GetRecurringDto>> DeleteIncome(string incomeId)
        {
            var response = new ServiceResponse<GetRecurringDto>();

            try
            {
                response.Data = new GetRecurringDto();
                response.Data.Incomes = new List<RecurringDto>();

                // Get user for accessToken
                var user = Utilities.GetCurrentUser(_context, _httpContextAccessor);

                var dbIncome = await _context.Recurrings
                    .Where(r => r.UserId == user.Id)
                    .Where(r => r.StreamId == incomeId)
                    .SingleOrDefaultAsync();

                _context.Recurrings.Remove(dbIncome);

                await _context.SaveChangesAsync();

                var dbRecurrings = await _context.Recurrings
                    .Where(r => r.UserId == user.Id)
                    .ToListAsync();

                response.Data.Incomes = dbRecurrings
                    .Where(r => r.Type == Enum.GetName<EType>(EType.Income))
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

        public async Task<ServiceResponse<GetRecurringDto>> SetIncomeActive(string incomeId, UpdateRecurringDto recurringDto)
        {
            var response = new ServiceResponse<GetRecurringDto>();

            try
            {
                response.Data = new GetRecurringDto();
                response.Data.Incomes = new List<RecurringDto>();

                // Get user for accessToken
                var user = Utilities.GetCurrentUser(_context, _httpContextAccessor);

                var dbIncome = await _context.Recurrings
                    .Where(r => r.UserId == user.Id)
                    .Where(r => r.StreamId == incomeId)
                    .SingleOrDefaultAsync();

                if (dbIncome is not null)
                {
                    dbIncome.IsActive = recurringDto.IsActive;
                }

                await _context.SaveChangesAsync();

                var dbRecurrings = await _context.Recurrings
                    .Where(r => r.UserId == user.Id)
                    .ToListAsync();

                response.Data.Incomes = dbRecurrings
                    .Where(r => r.Type == Enum.GetName<EType>(EType.Income))
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


    }
}
