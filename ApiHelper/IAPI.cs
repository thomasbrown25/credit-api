using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace financing_api.ApiHelper
{
    public interface IAPI
    {
        Task<Going.Plaid.Accounts.AccountsGetResponse> GetAccounts(User user);
    }
}