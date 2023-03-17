using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static financing_api.Enums.Enum;

namespace financing_api.Dtos.Frequency
{
    public class AddFrequencyDto
    {
        public int Id { get; set; }
        public EFrequency Frequency { get; set; }
    }
}