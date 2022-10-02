using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using financing_api.Dtos.Account;

namespace financing_api.Dtos.Transaction
{
    public class GetTransactionsDto
    {
        public int Id { get; set; }
        public List<TransactionDto> Transactions { get; set; }
        public List<AccountDto> Accounts { get; set; }
        public HashSet<string> Categories { get; set; }
        public decimal CurrentSpendAmount { get; set; }
    }
}
