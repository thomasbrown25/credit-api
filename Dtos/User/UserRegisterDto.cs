using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace web_api_netcore_project.Dtos.User
{
    public class UserRegisterDto
    {
        public string Firstname { get; set; } = string.Empty;
        public string Lastname { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}