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
        [HttpGet("create-link-token")] // Creates a link token in Plaid and returns the link token
        public async Task<ActionResult<ServiceResponse<string>>> CreateLinkToken()
        {
            var response = await _plaidService.CreateLinkToken();

            if (!response.Success)
            { // need to set this to server error
                return BadRequest(response);
            }
            return Ok(response);
        }

        [Authorize]
        [HttpPost("public-token-exchange")] // Exchanges the public token for the access token
        public async Task<ActionResult<ServiceResponse<string>>> PublicTokenExchange(
            [FromBody] string publicToken
        )
        {
            var response = await _plaidService.PublicTokenExchange(publicToken);

            if (!response.Success)
            { // need to set this to server error
                return BadRequest(response);
            }
            return Ok(response);
        }
    }
}
