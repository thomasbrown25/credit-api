using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace financing_api.Services.PlaidService
{
    public interface IPlaidService
    {
        Task<ServiceResponse<string>> GetLinkToken(string email);
    }
}