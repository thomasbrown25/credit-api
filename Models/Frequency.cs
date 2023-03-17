using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static financing_api.Enums.Enum;

namespace financing_api.Models
{
    public class Frequency
    {
        public int Id { get; set; }
        public EFrequency Name { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? UpdatedDate { get; set; }
    }
}