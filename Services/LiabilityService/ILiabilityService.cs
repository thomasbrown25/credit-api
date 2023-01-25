using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using financing_api.Dtos.Liability;

namespace financing_api.Services.LiabilityService
{
    public interface ILiabilityService
    {
        Task<ServiceResponse<GetLiabilityDto>> GetLiabilities();
    }
}