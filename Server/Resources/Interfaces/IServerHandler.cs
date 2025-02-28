using System.ComponentModel;

namespace Server;

public interface IServerHandler
{
	public int ClientCount { get; set; }
	float PingInterval { get; set; }
	int UpdatesPerSecond { get; set; }
	int ActivePort { get; set; }
	
	bool StartServer();
	bool StopServer();
}