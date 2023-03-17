using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using financing_api.Dtos.Frequency;
using financing_api.Services.FrequencyService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace financing_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FrequencyController : ControllerBase
    {
        private readonly IFrequencyService _frequencyService;

        public FrequencyController(IFrequencyService frequencyService)
        {
            _frequencyService = frequencyService;
        }

        [Authorize]
        [HttpGet("")]
        public async Task<ActionResult<ServiceResponse<GetFrequencyDto>>> GetFrequencies()
        {
            var response = await _frequencyService.GetFrequencies();

            if (!response.Success)
            { // need to set this to server error
                return BadRequest(response);
            }
            return Ok(response);
        }

        [Authorize]
        [HttpPost("")]
        public async Task<ActionResult<ServiceResponse<GetFrequencyDto>>> AddFrequency(AddFrequencyDto frequency)
        {
            var response = await _frequencyService.AddFrequency(frequency);

            if (!response.Success)
            { // need to set this to server error
                return BadRequest(response);
            }
            return Ok(response);
        }
    }
}