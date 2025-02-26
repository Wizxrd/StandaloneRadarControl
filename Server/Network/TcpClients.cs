using System.Net.Sockets;

namespace Server.Network;

public class TcpClients
{
	public static Dictionary<string, NetworkStream> clientStreams = new();
}