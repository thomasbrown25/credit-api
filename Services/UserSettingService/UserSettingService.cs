using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using financing_api.Data;
using financing_api.Dtos.UserSetting;
using financing_api.Utils;
using Microsoft.EntityFrameworkCore;

namespace financing_api.Services.UserSettingService
{
    public class UserSettingService : IUserSettingService
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;

        public UserSettingService(
            DataContext context,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            IMapper mapper
        )
        {
            _context = context;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
        }

        public async Task<ServiceResponse<SettingsDto>> GetSettings()
        {
            var response = new ServiceResponse<SettingsDto>();

            try
            {
                response.Data = new SettingsDto();

                var user = Utilities.GetCurrentUser(_context, _httpContextAccessor);

                var dbSettings = await _context.UserSettings
                                   .FirstOrDefaultAsync(s => s.UserId == user.Id);

                response.Data = _mapper.Map<SettingsDto>(dbSettings);
            }
            catch (System.Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                return response;
            }
            return response;
        }

        public async Task<ServiceResponse<SettingsDto>> SaveSettings(SettingsDto newSettings)
        {
            var response = new ServiceResponse<SettingsDto>();

            try
            {
                response.Data = new SettingsDto();

                var user = Utilities.GetCurrentUser(_context, _httpContextAccessor);

                var dbSettings = await _context.UserSettings
                                   .FirstOrDefaultAsync(s => s.UserId == user.Id);

                _mapper.Map<SettingsDto, UserSettings>(newSettings, dbSettings);

                await _context.SaveChangesAsync();

                response.Data = newSettings;
            }
            catch (System.Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                return response;
            }
            return response;
        }
    }
}