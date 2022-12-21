using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Acklann.Plaid;
using financing_api.Data;

namespace financing_api.Utils
{
    public static class Utilities
    {
        public static User GetCurrentUser(DataContext _context, IHttpContextAccessor _httpContextAccessor)
        {
            try
            {
                string email = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (email == null)
                    return null;

                // Get current user from sql db
                User user = _context.Users.FirstOrDefault(u => u.Email.ToLower().Equals(email.ToLower()));

                return user;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public static int GetUserId(IHttpContextAccessor _httpContextAccessor)
        {
            try
            {
                return int.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 0;
            }
        }

        public static string[] GetAccountIds(IReadOnlyList<Going.Plaid.Entity.Account> accounts)
        {
            string[] accountIds = new string[accounts.Count()];
            int i = 0;

            foreach (var account in accounts)
            {
                accountIds[i] = account.AccountId;
                i++;
            }

            return accountIds;
        }

        // public static T IsEmpty<T>(T result)
        // {
        //     if (result is null)
        //     {
        //         Console.WriteLine("Plaid API result is null");
        //     }
        //     else if (result.Error is not null)
        //     {
        //         Console.WriteLine(result.Error.ErrorMessage);
        //     }
        // }

        public static PlaidClient GetPlaidClient(IConfiguration _configuration)
        {
            PlaidClient client = null;

            if (_configuration["Plaid:Environment"].ToLower() == "sandbox")
            {
                client = new PlaidClient(Acklann.Plaid.Environment.Sandbox);
            }
            else if (_configuration["Plaid:Environment"].ToLower() == "development")
            {
                client = new PlaidClient(Acklann.Plaid.Environment.Development);
            }

            return client;
        }
    }
}
