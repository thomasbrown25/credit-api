using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using financing_api.Services.PlaidService;
using Microsoft.AspNetCore.Mvc;

namespace financing_api
{
    public class Sandbox
    {
        private readonly IPlaidService _plaidService;

        public Sandbox(IPlaidService plaidService)
        {
            _plaidService = plaidService;
        }

        public async Task<ActionResult<ServiceResponse<decimal>>> GetCurrentSpendForMonth()
        {
            var response = await _plaidService.GetCurrentSpendForMonth();

            return response;
        }
    }
}
