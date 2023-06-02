using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace financing_api.Dtos.ManagedBill
{
    public class GetBillsDto
    {
        public int Id { get; set; }
        public List<BillDto> Bills { get; set; }
        public decimal TotalAmounts { get; set; }
        public decimal TotalMinMonthly { get; set; }
    }
}