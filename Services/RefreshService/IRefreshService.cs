using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using financing_api.Dtos.Refresh;

namespace financing_api.Services.RefreshService
{
    public interface IRefreshService
    {
        Task<ServiceResponse<RefreshDto>> RefreshAll();
    }
}