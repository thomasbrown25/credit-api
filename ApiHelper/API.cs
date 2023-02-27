using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Going.Plaid;
using financing_api.Data;
using financing_api.Shared;
using AutoMapper;
using Microsoft.Extensions.Options;
using Going.Plaid.Entity;

namespace financing_api.ApiHelper
{
    public class API : IAPI
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly PlaidCredentials _credentials;
        private readonly PlaidClient _client;
        private readonly IMapper _mapper;
        private readonly IAPI _api;

        public API(
            DataContext context,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            IOptions<PlaidCredentials> credentials,
            PlaidClient client,
            IMapper mapper,
            IAPI api
        )
        {
            _context = context;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _credentials = credentials.Value;
            _client = new PlaidClient(Going.Plaid.Environment.Development);
            _mapper = mapper;
            _api = api;
        }

        public async Task<Going.Plaid.Accounts.AccountsGetResponse> GetAccountsRequest(User user)
        {
            var request = new Going.Plaid.Accounts.AccountsBalanceGetRequest()
            {
                ClientId = _configuration["PlaidClientId"],
                Secret = _configuration["PlaidSecret"],
                AccessToken = user.AccessToken,
            };

            var result = await _client.AccountsBalanceGetAsync(request);

            return result;
        }

        public async Task<Going.Plaid.Link.LinkTokenCreateResponse> LinkTokenRequest(User user)
        {
            var plaidUser = new LinkTokenCreateRequestUser()
            {
                ClientUserId = user.Id.ToString()
            };

            var request = new Going.Plaid.Link.LinkTokenCreateRequest()
            {
                ClientId = _configuration["PlaidClientId"],
                Secret = _configuration["PlaidSecret"],
                ClientName = "Financing Api",
                Language = Language.English,
                CountryCodes = new CountryCode[] { CountryCode.Us },
                User = plaidUser,
                Products = new Products[] { Products.Transactions, Products.Liabilities }

            };

            var result = await _client.LinkTokenCreateAsync(request);

            return result;
        }
    }
}