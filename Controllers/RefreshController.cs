using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using financing_api.Dtos.Refresh;
using financing_api.Services.RefreshService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace financing_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RefreshController : ControllerBase
    {
        private readonly IRefreshService _refreshService;

        public RefreshController(IRefreshService refreshervice)
        {
            _refreshService = refreshervice;
        }

        [HttpPost("all")]
        public async Task<ActionResult<ServiceResponse<RefreshDto>>> RefreshAll()
        {
            var response = await _refreshService.RefreshAll();

            if (!response.Success)
            { // need to set this to server error
                return BadRequest(response);
            }
            return Ok(response);
        }
    }
}