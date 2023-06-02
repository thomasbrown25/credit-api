using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using financing_api.Dtos.ManagedBill;

namespace financing_api.Services.ManagedBillsService
{
    public interface IManagedBillsService
    {
        Task<ServiceResponse<GetBillsDto>> GetBills();
        Task<ServiceResponse<GetBillsDto>> AddBill(AddBillDto bill);
        Task<ServiceResponse<GetBillsDto>> UpdateBill(UpdateBillDto bill);
        Task<ServiceResponse<GetBillsDto>> DeleteBill(int billId);
    }
}