using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using financing_api.Data;
using System.Security.Claims;
using financing_api.Dtos.Transaction;
using Going.Plaid;
using AutoMapper;
using Going.Plaid.Transactions;
using Microsoft.Extensions.Options;
using financing_api.Shared;
using financing_api.Dtos.Account;
using financing_api.Utils;
using System.Collections;
using financing_api.PlaidInterface;
using financing_api.DAL;
using financing_api.Logger;
using financing_api.DbLogger;

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
        private readonly IPlaidApi _plaidApi;
        private readonly TransactionDAL _transactionDal;
        private readonly ILogging _logging;

        public TransactionsService(
            DataContext context,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            IOptions<PlaidCredentials> credentials,
            PlaidClient client,
            IMapper mapper,
            IPlaidApi plaidApi,
            TransactionDAL transactionDAL,
            ILogging logging
        )
        {
            _context = context;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _credentials = credentials.Value;
            _client = new PlaidClient(Going.Plaid.Environment.Development);
            _mapper = mapper;
            _plaidApi = plaidApi;
            _transactionDal = transactionDAL;
            _logging = logging;
        }

        public async Task<ServiceResponse<GetTransactionsDto>> GetTransactions()
        {
            var response = new ServiceResponse<GetTransactionsDto>();

            try
            {
                response.Data = new GetTransactionsDto();

                var user = Utilities.GetCurrentUser(_context, _httpContextAccessor);

                var dbTransactions = _transactionDal.GetDbTransactions(user).Result;

                response.Data.Transactions = _transactionDal.GetTransactions(dbTransactions);

            }
            catch (System.Exception ex)
            {
                _logging.LogException(ex);
                response.Success = false;
                response.Message = ex.Message;
                return response;
            }
            return response;
        }

        public async Task<ServiceResponse<GetRecurringDto>> GetRecurringTransactions()
        {
            var response = new ServiceResponse<GetRecurringDto>();
            response.Data = new GetRecurringDto();

            try
            {
                // Get user for accessToken
                var user = Utilities.GetCurrentUser(_context, _httpContextAccessor);

                response.Data.Transactions = _transactionDal.GetRecurringTransactions(user).Result;

                response.Data.Incomes = _transactionDal.GetIncomes(user).Result;

                response.Data.Expenses = _transactionDal.GetExpenses(user).Result;

                response.Data.TotalIncome = Helper.GetTotalIncome(response.Data.Incomes);

                response.Data.Tithes = Helper.GetTithes(response.Data.Incomes);

            }
            catch (System.Exception ex)
            {
                _logging.LogException(ex);
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

                response.Data.Expenses = _transactionDal.GetExpenses(user).Result;

            }
            catch (System.Exception ex)
            {
                _logging.LogException(ex);
                response.Success = false;
                response.Message = ex.Message;
                return response;
            }

            return response;
        }

        public async Task<ServiceResponse<GetRecurringDto>> AddRecurringTransaction(AddRecurringDto newRecurring)
        {
            var response = new ServiceResponse<GetRecurringDto>();
            response.Data = new GetRecurringDto();

            try
            {
                var user = Utilities.GetCurrentUser(_context, _httpContextAccessor);

                Recurring recurring = _mapper.Map<Recurring>(newRecurring);

                recurring.UserId = user.Id;

                _context.Recurrings.Add(recurring);

                await _context.SaveChangesAsync();

                response.Data.Transactions = _transactionDal.GetRecurringTransactions(user).Result;

                response.Data.Incomes = _transactionDal.GetIncomes(user).Result;

                response.Data.Expenses = _transactionDal.GetExpenses(user).Result;

                response.Data.TotalIncome = Helper.GetTotalIncome(response.Data.Incomes);

                response.Data.Tithes = Helper.GetTithes(response.Data.Incomes);

            }
            catch (System.Exception ex)
            {
                _logging.LogException(ex);
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

                var dbRecurring = await _context.Recurrings
                    .FirstOrDefaultAsync(r => r.Id == updatedRecurring.Id);

                // confirm that current user is owner
                if (dbRecurring.UserId == user.Id)
                {
                    _mapper.Map<UpdateRecurringDto, Recurring>(updatedRecurring, dbRecurring);

                    await _context.SaveChangesAsync();

                    response.Data.Expenses = _transactionDal.GetExpenses(user).Result;
                }

            }
            catch (System.Exception ex)
            {
                _logging.LogException(ex);
                response.Success = false;
                response.Message = ex.Message;
                return response;
            }

            return response;
        }

        public async Task<ServiceResponse<GetRecurringDto>> UpdateIncome(UpdateRecurringDto updatedRecurring)
        {
            var response = new ServiceResponse<GetRecurringDto>();
            response.Data = new GetRecurringDto();

            try
            {
                var user = Utilities.GetCurrentUser(_context, _httpContextAccessor);

                //var dbRecurring = _transactionDal.GetDbRecurring(updatedRecurring.Id).Result;

                var dbRecurring = await _context.Recurrings
                    .FirstOrDefaultAsync(r => r.Id == updatedRecurring.Id);

                // confirm that current user is owner
                if (dbRecurring.UserId == user.Id)
                {
                    _mapper.Map<UpdateRecurringDto, Recurring>(updatedRecurring, dbRecurring);

                    await _context.SaveChangesAsync();

                    response.Data.Incomes = _transactionDal.GetIncomes(user).Result;
                }

            }
            catch (System.Exception ex)
            {
                _logging.LogException(ex);
                response.Success = false;
                response.Message = ex.Message;
                return response;
            }

            return response;
        }

        public async Task<ServiceResponse<GetRecurringDto>> DisableRecurringTransaction(int transactionId)
        {
            var response = new ServiceResponse<GetRecurringDto>();
            response.Data = new GetRecurringDto();

            try
            {
                var user = Utilities.GetCurrentUser(_context, _httpContextAccessor);

                var dbRecurring = _transactionDal.GetDbRecurring(transactionId).Result;

                // confirm that current user is owner
                if (dbRecurring.UserId == user.Id)
                {
                    dbRecurring.IsActive = false;
                    dbRecurring.UpdatedDate = DateTime.Now;

                    await _context.SaveChangesAsync();

                    response.Data.Expenses = _transactionDal.GetExpenses(user).Result;
                }

            }
            catch (System.Exception ex)
            {
                _logging.LogException(ex);
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

                var dbTransactions = _transactionDal.GetDbTransactions(user).Result;

                response.Data.Transactions = _transactionDal.GetAccountTransactions(dbTransactions, accountId);

                var todayTransactions = _transactionDal.GetTodaysTransactions(dbTransactions, accountId);

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
                _logging.LogException(ex);
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

                var dbIncome = _transactionDal.GetIncome(user, incomeId).Result;

                _context.Recurrings.Remove(dbIncome);

                await _context.SaveChangesAsync();

                response.Data.Incomes = _transactionDal.GetIncomes(user).Result;
            }
            catch (System.Exception ex)
            {
                _logging.LogException(ex);
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
                    .FirstOrDefaultAsync();

                dbIncome.IsActive = recurringDto.IsActive;

                await _context.SaveChangesAsync();

                response.Data.Incomes = _transactionDal.GetIncomes(user).Result;

            }
            catch (System.Exception ex)
            {
                _logging.LogException(ex);
                response.Success = false;
                response.Message = ex.Message;
                return response;
            }

            return response;
        }


    }
}
