using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using financing_api.Data;
using System.Security.Claims;
using AutoMapper;
using Microsoft.Extensions.Options;
using financing_api.Utils;
using System.Collections;
using financing_api.DbLogger;
using financing_api.Dtos.ManagedBill;

namespace financing_api.Services.ManagedBillsService
{
    public class ManagedBillsService : IManagedBillsService
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;
        private readonly ILogging _logging;

        public ManagedBillsService(
            DataContext context,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            IMapper mapper,
            ILogging logging
        )
        {
            _context = context;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
            _logging = logging;
        }

        public async Task<ServiceResponse<GetBillsDto>> GetBills()
        {
            var response = new ServiceResponse<GetBillsDto>();

            try
            {
                response.Data = new GetBillsDto();

                var user = Utilities.GetCurrentUser(_context, _httpContextAccessor);

                var dbBills = await _context.ManagedBills
                                   .OrderBy(b => b.Name)
                                   .ToListAsync();

                decimal totalAmounts = 0;
                decimal totalMinMonthly = 0;
                foreach (var dbBill in dbBills)
                {
                    totalAmounts += dbBill.TotalAmount;
                    totalMinMonthly += dbBill.MonthlyMin;
                }

                response.Data.Bills = dbBills.Select(b => _mapper.Map<BillDto>(b)).ToList();
                response.Data.TotalAmounts = totalAmounts;
                response.Data.TotalMinMonthly = totalMinMonthly;
            }
            catch (System.Exception ex)
            {
                _logging.LogException(ex);
                response.Success = false;
                response.Message = ex.Message;
                return response;
            }
            return response;
        }

        public async Task<ServiceResponse<GetBillsDto>> AddBill(AddBillDto bill)
        {
            var response = new ServiceResponse<GetBillsDto>();

            try
            {
                response.Data = new GetBillsDto();

                var user = Utilities.GetCurrentUser(_context, _httpContextAccessor);

                bill.UserId = user.Id;

                ManagedBill newBill = _mapper.Map<ManagedBill>(bill);

                _context.ManagedBills.Add(newBill);

                await _context.SaveChangesAsync();

                var dbBills = await _context.ManagedBills
                                   .OrderBy(c => c.Name)
                                   .ToListAsync();

                response.Data.Bills = dbBills.Select(c => _mapper.Map<BillDto>(c)).ToList();
            }
            catch (System.Exception ex)
            {
                _logging.LogException(ex);
                response.Success = false;
                response.Message = ex.Message;
                return response;
            }
            return response;
        }

        public async Task<ServiceResponse<GetBillsDto>> UpdateBill(UpdateBillDto updatedBill)
        {
            var response = new ServiceResponse<GetBillsDto>();
            response.Data = new GetBillsDto();

            try
            {
                var user = Utilities.GetCurrentUser(_context, _httpContextAccessor);

                var dbManagedBill = await _context.ManagedBills
                    .FirstOrDefaultAsync(b => b.Id == updatedBill.Id);

                // confirm that current user is owner
                if (dbManagedBill.UserId == user.Id)
                {
                    _mapper.Map<UpdateBillDto, ManagedBill>(updatedBill, dbManagedBill);

                    await _context.SaveChangesAsync();

                    var dbBills = await _context.ManagedBills
                               .OrderBy(c => c.Name)
                               .ToListAsync();

                    response.Data.Bills = dbBills.Select(c => _mapper.Map<BillDto>(c)).ToList();
                }

            }
            catch (System.Exception ex)
            {
                _logging.LogException(ex);
                response.Success = false;
                response.Message = ex.Message;
                return response;
            }

            return response;
        }

        public async Task<ServiceResponse<GetBillsDto>> DeleteBill(int billId)
        {
            var response = new ServiceResponse<GetBillsDto>();

            try
            {
                response.Data = new GetBillsDto();

                // Get user for accessToken
                var user = Utilities.GetCurrentUser(_context, _httpContextAccessor);

                var dbManagedBill = await _context.ManagedBills
                    .FirstOrDefaultAsync(b => b.Id == billId);

                if (dbManagedBill is not null)
                {
                    _context.ManagedBills.Remove(dbManagedBill);
                    await _context.SaveChangesAsync();
                }

                var dbBills = await _context.ManagedBills
                               .OrderBy(c => c.Name)
                               .ToListAsync();

                response.Data.Bills = dbBills.Select(c => _mapper.Map<BillDto>(c)).ToList();
            }
            catch (System.Exception ex)
            {
                _logging.LogException(ex);
                response.Success = false;
                response.Message = ex.Message;
                return response;
            }

            return response;
        }
    }
}