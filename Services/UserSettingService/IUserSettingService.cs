using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using financing_api.Dtos.UserSetting;

namespace financing_api.Services.UserSettingService
{
    public interface IUserSettingService
    {
        Task<ServiceResponse<SettingsDto>> GetSettings();
    }
}