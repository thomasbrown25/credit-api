using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using financing_api.Dtos.Liabilities;
using financing_api.Services.LiabilitiesService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace financing_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LiabilitiesController : ControllerBase
    {
        private readonly ILiabilitiesService _liabilitiesService;

        public LiabilitiesController(ILiabilitiesService liabilitiesService)
        {
            _liabilitiesService = liabilitiesService;
        }

        [Authorize]
        [HttpGet("")]
        public async Task<ActionResult<ServiceResponse<GetLiabilitiesDto>>> GetLiabilities()
        {
            var response = await _liabilitiesService.GetLiabilities();

            if (!response.Success)
            { // need to set this to server error
                return BadRequest(response);
            }
            return Ok(response);
        }
    }
}