using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace financing_api.Dtos.Account
{
    public class AccountDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string? Mask { get; set; }
        public string? OfficialName { get; set; }
        public string? Type { get; set; }
        public Going.Plaid.Entity.AccountSubtype? Subtype { get; set; }
        public AccountBalanceDto Balance { get; set; }
    }
}
