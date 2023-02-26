using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using financing_api.Dtos.Account;

namespace financing_api.Services.AccountService
{
    public static class Helper
    {
        public static AccountDto MapPlaidStream(AccountDto accountDto, Going.Plaid.Entity.Account account, User user)
        {
            accountDto.UserId = user.Id;
            accountDto.AccountId = account.AccountId;
            accountDto.Name = account.Name;
            accountDto.Mask = account.Mask;
            accountDto.OfficialName = account.OfficialName;
            accountDto.Type = account.Subtype?.ToString();
            accountDto.Subtype = account.Subtype?.ToString();
            accountDto.BalanceCurrent = account.Balances.Current;
            accountDto.BalanceAvailable = account.Balances.Available;
            accountDto.BalanceLimit = account.Balances.Limit;

            return accountDto;
        }
    }
}