using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace financing_api.Dtos.Liabilities
{
    public class GetLiabilitiesDto
    {
        public List<string> Liabilities { get; set; }
    }
}