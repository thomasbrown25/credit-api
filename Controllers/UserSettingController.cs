using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using financing_api.Dtos.UserSetting;
using financing_api.Services.UserSettingService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace financing_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserSettingController : ControllerBase
    {
        private readonly IUserSettingService _userSettingService;

        public UserSettingController(IUserSettingService userSettingService)
        {
            _userSettingService = userSettingService;
        }

        [Authorize]
        [HttpGet("")]
        public async Task<ActionResult<ServiceResponse<SettingsDto>>> GetSettings()
        {
            var response = await _userSettingService.GetSettings();

            if (!response.Success)
            { // need to set this to server error
                return BadRequest(response);
            }
            return Ok(response);
        }

        [Authorize]
        [HttpPost("")]
        public async Task<ActionResult<ServiceResponse<SettingsDto>>> SaveSettings(SettingsDto newSettings)
        {
            var response = await _userSettingService.SaveSettings(newSettings);

            if (!response.Success)
            { // need to set this to server error
                return BadRequest(response);
            }
            return Ok(response);
        }
    }
}