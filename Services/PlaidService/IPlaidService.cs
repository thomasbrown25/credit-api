using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace financing_api.Services.PlaidService
{
    public interface IPlaidService
    {
        // Creates a link token in Plaid and returns the link token
        Task<ServiceResponse<string>> CreateLinkToken();
        // Exchanges the plaid public token for the access token
        Task<ServiceResponse<string>> PublicTokenExchange(string linkToken);
        // Get all transactions for the linked accounts
        Task<ServiceResponse<List<Acklann.Plaid.Entity.Transaction>>> GetTransactions();
        // Get all linked accounts
        Task<ServiceResponse<List<Acklann.Plaid.Entity.Account>>> GetAccountsBalance();
    }
}