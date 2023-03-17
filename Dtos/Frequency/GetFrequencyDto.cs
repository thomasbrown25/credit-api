using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace financing_api.Dtos.Frequency
{
    public class GetFrequencyDto
    {
        public int Id { get; set; }
        public List<FrequencyDto> Frequencies { get; set; }
    }
}