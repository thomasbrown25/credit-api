using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace financing_api.Dtos.Transaction
{
    public class GetRecurringDto
    {
        public List<RecurringDto> Transactions { get; set; }
        public List<RecurringDto> Income { get; set; }
        public List<RecurringDto> Expense { get; set; }
    }
}