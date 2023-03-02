using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using financing_api.Dtos.User;

namespace financing_api.Data
{
    public interface IAuthService
    {
        Task<ServiceResponse<LoadUserDto>> Register(User user, string password);
        Task<ServiceResponse<LoadUserDto>> Login(string email, string password);
        Task<bool> UserExists(string email);
        Task<ServiceResponse<LoadUserDto>> LoadUser();
        Task<ServiceResponse<string>> DeleteUser();
    }
}