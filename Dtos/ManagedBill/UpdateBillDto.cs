using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace financing_api.Dtos.ManagedBill
{
    public class UpdateBillDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime DueDate { get; set; }
        public decimal MonthlyMin { get; set; }
    }
}