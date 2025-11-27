using Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Models
{
    public class ServerBookmark
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public DateTime LastUsedAt { get; set; }
        public string Address { get; set; } = string.Empty;
        public string Port { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Callsign { get; set; } = string.Empty;
    }
}
