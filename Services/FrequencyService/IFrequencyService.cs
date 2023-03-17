using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using financing_api.Dtos.Frequency;

namespace financing_api.Services.FrequencyService
{
    public interface IFrequencyService
    {
        Task<ServiceResponse<GetFrequencyDto>> GetFrequencies();
        Task<ServiceResponse<GetFrequencyDto>> AddFrequency(AddFrequencyDto frequency);
    }
}