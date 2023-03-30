using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using financing_api.Data;
using financing_api.Dtos.Transaction;
using Microsoft.EntityFrameworkCore;

namespace financing_api.DAL
{
    public class TransactionDAL
    {
        private readonly DataContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;

        public TransactionDAL(
             DataContext context,
            IHttpContextAccessor httpContextAccessor,
            IMapper mapper
        )
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
        }

        public async Task<List<Recurring>> GetDbRecurrings(User user)
        {
            return await _context.Recurrings
                                .Where(r => r.UserId == user.Id)
                                .ToListAsync();
        }

        public async Task<Recurring> GetDbRecurring(int transactionId)
        {
            return await _context.Recurrings
                    .FirstOrDefaultAsync(r => r.Id == transactionId);
        }

        public List<RecurringDto> GetIncomes(List<Recurring> dbRecurrings)
        {
            return dbRecurrings
                    .Where(r => r.Type == Enum.GetName<EType>(EType.Income))
                    .Where(r => r.IsActive == true)
                    .Select(r => _mapper.Map<RecurringDto>(r))
                    .OrderByDescending(r => r.LastAmount)
                    .ToList();
        }

        public async Task<Recurring> GetIncome(User user, string incomeId)
        {
            return await _context.Recurrings
                    .Where(r => r.UserId == user.Id)
                    .Where(r => r.StreamId == incomeId)
                    .SingleOrDefaultAsync();
        }

        public List<RecurringDto> GetRecurringTransactions(List<Recurring> dbRecurrings)
        {
            return dbRecurrings
                    .Where(r => r.IsActive == true)
                    .Select(r => _mapper.Map<RecurringDto>(r))
                    .OrderByDescending(r => r.DueDate).ToList();
        }

        public List<RecurringDto> GetExpenses(List<Recurring> dbRecurrings)
        {
            return dbRecurrings
                    .Where(r => r.Type == Enum.GetName<EType>(EType.Expense))
                    .Where(r => r.IsActive == true)
                    .Select(r => _mapper.Map<RecurringDto>(r))
                    .OrderBy(r => r.DueDate)
                    .ToList();
        }

        public async Task<List<Transaction>> GetDbTransactions(User user)
        {
            return await _context.Transactions
                    .Where(r => r.UserId == user.Id)
                    .OrderByDescending(r => r.Date)
                    .ToListAsync();
        }

        public List<TransactionDto> GetTransactions(List<Transaction> dbTransactions)
        {
            return dbTransactions.Select(c => _mapper.Map<TransactionDto>(c)).ToList();
        }

        public List<TransactionDto> GetAccountTransactions(List<Transaction> dbTransactions, string accountId)
        {
            return dbTransactions
                    .Where(t => t.AccountId == accountId)
                    .Select(t => _mapper.Map<TransactionDto>(t))
                    .ToList();
        }

        public List<TransactionDto> GetTodaysTransactions(List<Transaction> dbTransactions, string accountId)
        {
            return dbTransactions
                    .Where(t => t.AccountId == accountId)
                    .Where(t => t.Date == DateTime.Today)
                    .Select(t => _mapper.Map<TransactionDto>(t))
                    .ToList();
        }
    }
}