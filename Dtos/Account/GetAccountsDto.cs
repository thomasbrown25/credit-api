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
    }
}
