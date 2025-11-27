using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models;

public class Ports
{
    public int SignalRServer { get; set; } = 7500;
    public int ServerUdpRecieve { get; set; } = 7600;
    public int DCSServerUdpRecieve { get; set; } = 7700;
}
