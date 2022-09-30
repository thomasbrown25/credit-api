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
        Task<ServiceResponse<List<Acklann.Plaid.Entity.Transaction>>> GetTransactions();

        // Get recent transactions with configured count amount
        Task<ServiceResponse<List<Acklann.Plaid.Entity.Transaction>>> GetRecentTransactions(
            uint count
        );

        // Get categories
        Task<ServiceResponse<GetTransactionsDto>> GetCurrentMonthTransactions();

        // Get the current spend for the month
        Task<ServiceResponse<decimal>> GetCurrentSpendForMonth();
    }
}
