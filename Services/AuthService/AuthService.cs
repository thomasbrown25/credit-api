using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using financing_api.Dtos.User;
using financing_api.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace financing_api.Data
{
    public class AuthService : IAuthService
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(
            DataContext context,
            IConfiguration configuration,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor
        )
        {
            _context = context;
            _configuration = configuration;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ServiceResponse<LoadUserDto>> Register(User user, string password)
        {
            var response = new ServiceResponse<LoadUserDto>();
            response.Data = new LoadUserDto();

            try
            {
                if (await UserExists(user.Email))
                {
                    response.Message = "A user with that email already exists.";
                    return response;
                }

                CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);

                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // after we save user, we create and return the jwt token
                response.Data.JWTToken = CreateToken(user);

                response.Data = _mapper.Map<LoadUserDto>(user);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<ServiceResponse<LoadUserDto>> Login(string email, string password)
        {
            var response = new ServiceResponse<LoadUserDto>();

            try
            {
                response.Data = new LoadUserDto();

                var user = await _context.Users.FirstOrDefaultAsync(
                    u => u.Email.ToLower().Equals(email.ToLower())
                );

                if (user == null)
                {
                    response.Success = false;
                    response.Message = "User not found.";
                }
                else if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
                {
                    response.Success = false;
                    response.Message = "Wrong password.";
                }
                else
                {
                    response.Data.JWTToken = CreateToken(user);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<ServiceResponse<LoadUserDto>> LoadUser()
        {
            ServiceResponse<LoadUserDto> response = new ServiceResponse<LoadUserDto>();

            try
            {
                User user = Utilities.GetCurrentUser(_context, _httpContextAccessor);

                if (user == null)
                {
                    response.Success = false;
                    response.Message = "User not found.";
                    return response;
                }

                response.Data = _mapper.Map<LoadUserDto>(user);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<ServiceResponse<string>> DeleteUser()
        {
            ServiceResponse<string> response = new ServiceResponse<string>();

            try
            {
                User user = Utilities.GetCurrentUser(_context, _httpContextAccessor);


                var dbTransactions = await _context.Transactions
                                   .Where(c => c.UserId == user.Id)
                                   .OrderByDescending(c => c.Date)
                                   .ToListAsync();

                var dbRecurrings = await _context.Recurrings
                                .Where(r => r.UserId == user.Id)
                                .ToListAsync();

                var dbAccounts = await _context.Accounts
                                .Where(a => a.UserId == user.Id)
                                .ToListAsync();

                _context.RemoveRange(dbTransactions);
                _context.RemoveRange(dbRecurrings);
                _context.RemoveRange(dbAccounts);
                _context.Remove(user);

                _context.SaveChangesAsync();

                response.Data = "User Deleted: " + user.FirstName;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<bool> UserExists(string email)
        {
            try
            {
                if (await _context.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower()))
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return false;
        }

        private void CreatePasswordHash(
            string password,
            out byte[] passwordHash,
            out byte[] passwordSalt
        )
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt))
            {
                var computeHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computeHash.SequenceEqual(passwordHash);
            }
        }

        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Email),
                new Claim(ClaimTypes.Name, user.Email)
            };

            SymmetricSecurityKey key = new SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(
                    _configuration.GetSection("AppSettings:Key").Value
                )
            );

            SigningCredentials creds = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha512Signature
            );

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddMinutes(
                    Double.Parse(_configuration["AppSettings:JWTTokenExpiration"])
                ),
                signingCredentials: creds
            );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }
    }
}
