using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace financing_api.Models
{
    public class Settings
    {
        public string AppName { get; set; }
        public double Version { get; set; }
        public long RefreshRate { get; set; }
        public long FontSize { get; set; }
        public string Language { get; set; }
        public string Messages { get; set; }
        public string BackgroundColor { get; set; }
    }
}
