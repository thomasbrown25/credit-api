using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using financing_api.Dtos.Transaction;

namespace financing_api.Services.TransactionsService
{
    public interface ITransactionsService
    {
        // Get all transactions for the linked accounts
        Task<ServiceResponse<GetTransactionsDto>> GetTransactions();
        // Get recent transactions with configured count amount
        Task<ServiceResponse<GetTransactionsDto>> GetRecentTransactions(uint count);
        // Get the current spend for the month
        Task<ServiceResponse<decimal>> GetCurrentSpendForMonth();
        // Get recurring transactions
        Task<ServiceResponse<GetRecurringDto>> GetRecurringTransactions();
    }
}
