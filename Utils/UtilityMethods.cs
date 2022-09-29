using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using financing_api.Data;

namespace financing_api.Utils
{
    public static class UtilityMethods
    {
        public static User GetCurrentUser(
            DataContext _context,
            IHttpContextAccessor _httpContextAccessor
        )
        {
            try
            {
                string email = _httpContextAccessor.HttpContext.User.FindFirstValue(
                    ClaimTypes.NameIdentifier
                );

                if (email == null)
                    return null;

                // Get current user from sql db
                User user = _context.Users.FirstOrDefault(
                    u => u.Email.ToLower().Equals(email.ToLower())
                );

                return user;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
    }
}
