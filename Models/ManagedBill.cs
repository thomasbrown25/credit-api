using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static financing_api.Enums.Enum;

namespace financing_api.Models
{
    public class ManagedBill
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Type { get; set; }
        public string Category { get; set; }
        public string? Description { get; set; }
        public string? MerchantName { get; set; }
        public DateTime FirstDate { get; set; }
        public DateTime LastDate { get; set; }
        public DateTime? DueDate { get; set; }
        public EFrequency? Frequency { get; set; }
        public decimal Amount { get; set; }
        public bool IsActive { get; set; }
        public DateTime? CreatedDate { get; set; } = DateTime.Now;
        public DateTime? UpdatedDate { get; set; } = DateTime.Now;
    }
}