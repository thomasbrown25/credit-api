using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using financing_api.Data;
using System.Security.Claims;
using financing_api.Utils;
using financing_api.Dtos.Plaid;
using Going.Plaid;
using Going.Plaid.Entity;
using Going.Plaid.Link;

namespace financing_api.Services.PlaidService
{
    public class PlaidService : IPlaidService
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly PlaidClient _client;

        public PlaidService(
            DataContext context,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            PlaidClient client
        )
        {
            _context = context;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _client = new PlaidClient(Going.Plaid.Environment.Development);
        }

        public async Task<ServiceResponse<string>> CreateLinkToken()
        {
            ServiceResponse<string> response = new ServiceResponse<string>();

            try
            {
                // Get current user from sql db
                User user = Utilities.GetCurrentUser(_context, _httpContextAccessor);

                // Create plaid user with user id
                var plaidUser = new LinkTokenCreateRequestUser()
                {
                    ClientUserId = user.Id.ToString()
                };

                var result = await _client.LinkTokenCreateAsync(
                    new()
                    {
                        ClientId = _configuration["PlaidClientId"],
                        Secret = _configuration["PlaidSecret"],
                        ClientName = "Financing Api",
                        Language = Language.English,
                        CountryCodes = new CountryCode[] { CountryCode.Us },
                        User = plaidUser,
                        Products = new Products[] { Products.Transactions, Products.Liabilities }
                    }
                );

                // var linkResponse = _api.LinkTokenRequest(user);

                // if (linkResponse.Result.Error is not null)
                // {
                //     Console.WriteLine(linkResponse.Result.Error.ErrorMessage);
                //     response.Success = false;
                //     response.Error = new Error();
                //     response.Error.ErrorCode = linkResponse.Result.Error.ErrorCode.ToString();
                //     response.Error.ErrorMessage = linkResponse.Result.Error.ErrorMessage;
                //     return response;
                // }

                // response.Data = linkResponse.Result.LinkToken;

                if (result.Error is not null)
                {
                    Console.WriteLine(result.Error.ErrorMessage);
                    response.Success = false;
                    response.Error = new Error();
                    response.Error.ErrorCode = result.Error.ErrorCode.ToString();
                    response.Error.ErrorMessage = result.Error.ErrorMessage;
                    return response;
                }

                response.Data = result.LinkToken;

                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }


        public async Task<ServiceResponse<string>> UpdateLinkToken()
        {
            ServiceResponse<string> response = new ServiceResponse<string>();

            // try
            // {
            //     // Get current user from sql db
            //     User user = Utilities.GetCurrentUser(_context, _httpContextAccessor);

            //     if (user == null)
            //     {
            //         response.Success = false;
            //         response.Message = "User not found.";
            //         return response;
            //     }

            //     // Create plaid user with user id
            //     var plaidUser = new LinkTokenCreateRequestUser()
            //     {
            //         ClientUserId = user.Id.ToString()
            //     };

            //     var result = await _client.CreateLinkToken(
            //         new Acklann.Plaid.Management.CreateLinkTokenRequest()
            //         {
            //             ClientId = _configuration["PlaidClientId"],
            //             Secret = _configuration["PlaidSecret"],
            //             AccessToken = user.AccessToken,
            //             ClientName = "Financing Api",
            //             Language = "en",
            //             CountryCodes = new string[] { "US" },
            //             User = plaidUser,
            //             Products = new string[] { "auth", "transactions" }
            //         }
            //     );

            //     if (result.Exception is not null)
            //     {
            //         response.Success = false;
            //         response.Message = result.Exception.ErrorMessage;
            //     }

            //     response.Data = result.LinkToken;

            //     return response;
            // }
            // catch (Exception ex)
            // {
            //     response.Success = false;
            //     response.Message = ex.Message;
            // }
            return response;
        }

        public async Task<ServiceResponse<string>> PublicTokenExchange(string publicToken)
        {
            ServiceResponse<string> response = new ServiceResponse<string>();

            if (publicToken == null)
            {
                response.Success = false;
                response.Message = "No public token in request";
                return response;
            }

            // Exchange publicToken for accessToken
            var result = await _client.ItemPublicTokenExchangeAsync(
                new()
                {
                    ClientId = _configuration["PlaidClientId"],
                    Secret = _configuration["PlaidSecret"],
                    PublicToken = publicToken
                }
            );

            // Save accessToken to SQL DB
            var user = Utilities.GetCurrentUser(_context, _httpContextAccessor);
            user.AccessToken = result.AccessToken;
            await _context.SaveChangesAsync();

            response.Data = result.AccessToken;

            return response;
        }

        // Get Recurring Transactions
        public async Task<ServiceResponse<GetRecurringDto>> GetRecurringTransactions()
        {
            var response = new ServiceResponse<GetRecurringDto>();
            try
            {
                response.Data = new GetRecurringDto();
                response.Data.InflowStream = new List<InflowStreamsDto>();
                response.Data.OutflowStream = new List<OutflowStreamsDto>();

                // Get user for accessToken6
                var user = Utilities.GetCurrentUser(_context, _httpContextAccessor);

                if (user == null || user.AccessToken == null)
                {
                    response.Success = false;
                    response.Message = "User does not have access token";
                    return response;
                }

                // Get Account IDs 
                var getAccountRequest = new Going.Plaid.Accounts.AccountsGetRequest()
                {
                    ClientId = _configuration["PlaidClientId"],
                    Secret = _configuration["PlaidSecret"],
                    AccessToken = user.AccessToken
                };

                var accountResponse = await _client.AccountsGetAsync(getAccountRequest);

                if (accountResponse.Error is not null)
                {
                    Console.WriteLine(accountResponse.Error.ErrorMessage);
                    response.Success = false;
                    response.Message = accountResponse.Error.ErrorMessage;
                    return response;
                }


                var getRecurringRequest = new Going.Plaid.Transactions.TransactionsRecurringGetRequest()
                {
                    ClientId = _configuration["PlaidClientId"],
                    Secret = _configuration["PlaidSecret"],
                    AccessToken = user.AccessToken,
                    AccountIds = Utilities.GetAccountIds(accountResponse.Accounts),

                };



                var RecurringResponse = await _client.TransactionsRecurringGetAsync(getRecurringRequest);

                if (RecurringResponse.Error is not null)
                {
                    Console.WriteLine(RecurringResponse.Error.ErrorMessage);
                    response.Success = false;
                    response.Message = RecurringResponse.Error.ErrorMessage;
                    return response;
                }

                foreach (var inflowStream in RecurringResponse.InflowStreams)
                {
                    var inflowStreamsDto = new InflowStreamsDto();

                    inflowStreamsDto.AccountId = inflowStream.AccountId;
                    inflowStreamsDto.AverageAmount = inflowStream.AverageAmount;
                    inflowStreamsDto.Categories = inflowStream.Category;
                    inflowStreamsDto.Description = inflowStream.Description;
                    inflowStreamsDto.FirstDate = inflowStream.FirstDate.ToDateTime(TimeOnly.Parse("00:00:00"));
                    inflowStreamsDto.Frequency = inflowStream.Frequency.ToString();
                    inflowStreamsDto.IsActive = inflowStream.IsActive;
                    inflowStreamsDto.LastAmount = inflowStream.LastAmount;
                    inflowStreamsDto.LastDate = inflowStream.LastDate.ToDateTime(TimeOnly.Parse("00:00:00"));
                    inflowStreamsDto.MerchantName = inflowStream.MerchantName;
                    inflowStreamsDto.Status = inflowStream.Status;
                    inflowStreamsDto.StreamId = inflowStream.StreamId;

                    response.Data.InflowStream.Add(inflowStreamsDto);
                }


                for (var i = 0; i < 10; i++)
                {
                    var outflowStream = RecurringResponse.OutflowStreams;
                    var outflowStreamsDto = new OutflowStreamsDto();

                    outflowStreamsDto.AccountId = RecurringResponse.OutflowStreams[i].AccountId;
                    outflowStreamsDto.AverageAmount = RecurringResponse.OutflowStreams[i].AverageAmount;
                    outflowStreamsDto.Categories = RecurringResponse.OutflowStreams[i].Category;
                    outflowStreamsDto.Description = RecurringResponse.OutflowStreams[i].Description;
                    outflowStreamsDto.FirstDate = RecurringResponse.OutflowStreams[i].FirstDate.ToDateTime(TimeOnly.Parse("00:00:00"));
                    outflowStreamsDto.Frequency = RecurringResponse.OutflowStreams[i].Frequency.ToString();
                    outflowStreamsDto.IsActive = RecurringResponse.OutflowStreams[i].IsActive;
                    outflowStreamsDto.LastAmount = RecurringResponse.OutflowStreams[i].LastAmount;
                    outflowStreamsDto.LastDate = RecurringResponse.OutflowStreams[i].LastDate.ToDateTime(TimeOnly.Parse("00:00:00"));
                    outflowStreamsDto.MerchantName = RecurringResponse.OutflowStreams[i].MerchantName;
                    outflowStreamsDto.Status = RecurringResponse.OutflowStreams[i].Status;
                    outflowStreamsDto.StreamId = RecurringResponse.OutflowStreams[i].StreamId;

                    response.Data.OutflowStream.Add(outflowStreamsDto);
                }

            }
            catch (System.Exception ex)
            {
                Console.WriteLine("Recurring Transactions failed: " + ex.Message);
                response.Success = false;
                response.Message = ex.Message;
                return response;
            }

            return response;
        }
    }
}
