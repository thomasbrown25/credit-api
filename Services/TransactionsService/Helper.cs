using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using financing_api.Dtos.Transaction;
using Going.Plaid.Entity;

namespace financing_api.Services.TransactionsService
{
    public static class Helper
    {
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
    }
}