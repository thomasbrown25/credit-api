using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace financing_api.Dtos.Account
{
    public class GetAccountsDto
    {
        public int Id { get; set; }
        public List<AccountDto> Accounts { get; set; }
        public AccountDto Account { get; set; }
        public List<AccountDto> CashAccounts { get; set; }
        public List<AccountDto> CreditAccounts { get; set; }
        public List<AccountDto> LoanAccounts { get; set; }
        public decimal? CashAmount { get; set; }
        public decimal? CreditAmount { get; set; }
        public decimal? LoanAmount { get; set; }

    }
}
