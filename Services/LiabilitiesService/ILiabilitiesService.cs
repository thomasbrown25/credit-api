using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using financing_api.Dtos.Liabilities;

namespace financing_api.Services.LiabilitiesService
{
    public interface ILiabilitiesService
    {
        Task<ServiceResponse<GetLiabilitiesDto>> GetLiabilities();
    }
}