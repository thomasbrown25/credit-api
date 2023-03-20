using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using financing_api.Data;
using financing_api.Dtos.Frequency;
using financing_api.Utils;
using Microsoft.EntityFrameworkCore;

namespace financing_api.Services.FrequencyService
{
    public class FrequencyService : IFrequencyService
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;

        public FrequencyService(
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

        public async Task<ServiceResponse<GetFrequencyDto>> GetFrequencies()
        {
            var response = new ServiceResponse<GetFrequencyDto>();

            try
            {
                response.Data = new GetFrequencyDto();

                var dbFrequencies = await _context.Frequencies
                                   .ToListAsync();

                response.Data.Frequencies = dbFrequencies.Select(f => _mapper.Map<FrequencyDto>(f)).ToList();
            }
            catch (System.Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                return response;
            }
            return response;
        }

        public async Task<ServiceResponse<GetFrequencyDto>> AddFrequency(AddFrequencyDto frequency)
        {
            var response = new ServiceResponse<GetFrequencyDto>();

            try
            {
                response.Data = new GetFrequencyDto();

                var user = Utilities.GetCurrentUser(_context, _httpContextAccessor);

                Frequency newFrequency = _mapper.Map<Frequency>(frequency);

                _context.Frequencies.Add(newFrequency);

                await _context.SaveChangesAsync();

                var dbFrequencies = await _context.Frequencies
                                   .OrderBy(f => f.Name)
                                   .ToListAsync();

                response.Data.Frequencies = dbFrequencies.Select(f => _mapper.Map<FrequencyDto>(f)).ToList();
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