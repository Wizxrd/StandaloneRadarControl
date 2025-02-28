namespace Server.Resources.Interfaces;

public interface IServerModel
{
	public int ClientCount { get; set; }
	public float PingInterval { get; set; }
	public int UpdatesPerSecond { get; set; }
	public int ActivePort { get; set; }
	public bool StartServer();
	public bool StopServer();
}