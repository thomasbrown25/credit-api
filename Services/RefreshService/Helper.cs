using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using financing_api.Data;
using financing_api.Dtos.Account;
using financing_api.Dtos.Transaction;
using Going.Plaid.Entity;
using Microsoft.EntityFrameworkCore;

namespace financing_api.Services.RefreshService
{
    public class Helper
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

        public static RecurringDto MapPlaidStream(RecurringDto recurring, TransactionStream stream, User user, EType type)
        {
            try
            {
                recurring.UserId = user.Id;
                recurring.StreamId = stream.StreamId;
                recurring.AccountId = stream.AccountId;
                recurring.Type = Enum.GetName<EType>(type);
                recurring.Category = stream.Category.Count > 1 ? stream.Category[1] : stream.Category[0];
                recurring.Description = stream.Description;
                recurring.MerchantName = stream.MerchantName;
                recurring.FirstDate = stream.FirstDate.ToDateTime(TimeOnly.Parse("00:00:00"));
                recurring.LastDate = stream.LastDate.ToDateTime(TimeOnly.Parse("00:00:00"));
                recurring.Frequency = stream.Frequency.ToString();
                recurring.LastAmount = type == EType.Income ? stream.LastAmount.Amount * -1 : stream.LastAmount.Amount;
                recurring.IsActive = stream.IsActive;
                recurring.Status = stream.Status.ToString();

                if (stream.Category.Count > 1 && stream.Category[1].ToLower() == "internal account transfer")
                {
                    recurring.InternalTransfer = true;
                }

                // Add days to the last date to have an accurate due date... :refactor later
                if (stream.Frequency.ToString().ToLower() == "monthly")
                {
                    recurring.DueDate = stream.LastDate.ToDateTime(TimeOnly.Parse("00:00:00")).AddMonths(1);
                }
                else if (stream.Frequency.ToString().ToLower() == "biweekly" || stream.Frequency.ToString().ToLower() == "semimonthly")
                {
                    recurring.DueDate = stream.LastDate.ToDateTime(TimeOnly.Parse("00:00:00")).AddDays(14);
                }
                else if (stream.Frequency.ToString().ToLower() == "weekly")
                {
                    recurring.DueDate = stream.LastDate.ToDateTime(TimeOnly.Parse("00:00:00")).AddDays(7);
                }
                else
                {
                    recurring.DueDate = stream.LastDate.ToDateTime(TimeOnly.Parse("00:00:00")).AddMonths(1);
                }

                return recurring;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public static TransactionDto MapPlaidStream(TransactionDto transactionDto, Going.Plaid.Entity.Transaction transaction, User user)
        {
            try
            {
                transactionDto.UserId = user.Id;
                transactionDto.TransactionId = transaction.TransactionId;
                transactionDto.AccountId = transaction.AccountId;
                transactionDto.Name = transaction.Name;
                transactionDto.MerchantName = transaction.MerchantName;
                transactionDto.Amount = transaction.Amount;
                transactionDto.Pending = transaction.Pending;
                transactionDto.Date = transaction.Date.ToString();
                transactionDto.Category = transaction.Category.Count > 1 ? transaction.Category[1] : transaction.Category[0];

                return transactionDto;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public static void AddStreams(IReadOnlyList<Going.Plaid.Entity.TransactionStream> streams, DataContext context, IMapper mapper, User user, EType type)
        {
            foreach (var stream in streams)
            {
                if (!stream.Category.Contains("Internal Account Transfer") && (!stream.Category.Contains("Interest")))
                {
                    var dbRecurring = context.Recurrings
                        .FirstOrDefault(r => r.StreamId == stream.StreamId);

                    if (dbRecurring is null)
                    {
                        var recurring = Helper.MapPlaidStream(new RecurringDto(), stream, user, type);

                        // Map recurring with recurringDto db
                        Recurring recurringDb = mapper.Map<Recurring>(recurring);
                        context.Recurrings.Add(recurringDb);
                    }
                }
            }
        }
    }
}