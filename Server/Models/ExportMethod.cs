using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models
{
    public class ExportMethod
    {
        public bool Basic { get; set; } = true;
        public bool Advanced { get; set; } = false;
        public bool Realistic { get; set; } = false;
        public bool Hardcore { get; set; } = false;
        public bool Custom { get; set; } = false;
    }
}
