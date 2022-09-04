using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using financing_api.Services.PlaidService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace financing_api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PlaidController : ControllerBase
    {
        private readonly IPlaidService _plaidService;

        public PlaidController(IPlaidService plaidService)
        {
            _plaidService = plaidService;
        }

        [Authorize]
        [HttpGet("create-link-token")]
        public async Task<ActionResult<ServiceResponse<string>>> GetLinkToken()
        {
            // Get the email from the ClaimsPrincipal for the current user
            string email = User?.Identity?.Name;

            var response = await _plaidService.GetLinkToken(email);

            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
    }
}