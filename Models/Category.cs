using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace financing_api.Models
{
    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum Category
    {
        [EnumMember(Value = "Travel")]
        Travel,

        [EnumMember(Value = "Payment")]
        Payment,

        [EnumMember(Value = "Food and Drink")]
        FoodAndDrink,

        [EnumMember(Value = "Transfer")]
        Transfer
    }

    public class ECategory
    {
        public void LogCategory()
        {
            string Json = JsonSerializer.Serialize(Category.FoodAndDrink);

            Console.WriteLine("logging category: " + Json);
        }
    }
}
