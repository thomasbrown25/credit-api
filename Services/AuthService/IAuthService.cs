using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using financing_api.Dtos.User;

namespace web_api_netcore_project.Data
{
    public interface IAuthService
    {
        Task<ServiceResponse<string>> Register(User user, string password);
        Task<ServiceResponse<string>> Login(string email, string password);
        Task<bool> UserExists(string email);
        Task<ServiceResponse<LoadUserDto>> LoadUser(string email);
    }
}