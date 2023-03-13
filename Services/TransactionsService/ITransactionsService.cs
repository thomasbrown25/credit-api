using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using financing_api.Dtos.Transaction;

namespace financing_api.Services.TransactionsService
{
    public interface ITransactionsService
    {
        Task<ServiceResponse<GetTransactionsDto>> GetTransactions();
        Task<ServiceResponse<GetTransactionsDto>> RefreshTransactions();
        Task<ServiceResponse<GetTransactionsDto>> GetAccountTransactions(string accountId);
        Task<ServiceResponse<GetRecurringDto>> GetRecurringTransactions();
        Task<ServiceResponse<GetRecurringDto>> GetExpenses();
        Task<ServiceResponse<GetRecurringDto>> RefreshRecurringTransactions();
        Task<ServiceResponse<List<RecurringDto>>> AddRecurringTransaction(AddRecurringDto newRecurring);
        Task<ServiceResponse<GetRecurringDto>> UpdateRecurringTransaction(UpdateRecurringDto updatedRecurring);
        Task<ServiceResponse<GetRecurringDto>> DisableRecurringTransaction(int transactionId);
        Task<ServiceResponse<GetRecurringDto>> DeleteIncome(string incomeId);
        Task<ServiceResponse<GetRecurringDto>> SetIncomeActive(string incomeId, UpdateRecurringDto recurringDto);
    }
}
