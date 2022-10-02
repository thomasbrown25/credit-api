using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace financing_api.Dtos.Transaction
{
    public class GetRecurringDto
    {
        public List<InflowStreamsDto> InflowStream { get; set; }
        public List<OutflowStreamsDto> OutflowStream { get; set; }

    }
}