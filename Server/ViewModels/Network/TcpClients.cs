using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Server.Models;

namespace Server.ViewModels.Network
{
    public class TcpClients
    {
        public static Dictionary<string, NetworkStream> clientStreams = new Dictionary<string, NetworkStream>();
    }
}
