using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Models;

public class GeneralSettings
{
    public bool ConfirmDisconnect { get; set; } = true;
    public bool ConfirmExit { get; set; } = true;
    public bool PlayConnectDisconnectSound { get; set; } = true;
    public bool Callsign { get; set; } = true;
    public List<ServerBookmark> ServerBookmarks { get; set; } = new();
}
