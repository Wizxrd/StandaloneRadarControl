using System.Net.Sockets;

namespace Server.Models.Network
{
    public class TcpClients
    {
        public static Dictionary<string, NetworkStream> clientStreams = new Dictionary<string, NetworkStream>();
    }
}
