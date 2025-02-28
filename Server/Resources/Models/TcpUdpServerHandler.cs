using System.ComponentModel;
using System.Runtime.CompilerServices;
using Server.Network;
using Server.ViewModels;

namespace Server.Resources.Models;

public class TcpUdpServerHandler : IServerHandler
{
	MainWindowViewModel ViewModel;
	
	private int clientCount;
	public int ClientCount { get; set; }
	
	private float pingInterval;
	public float PingInterval { get; set; }
	
	private int updatesPerSecond;
	public int UpdatesPerSecond { get; set; }
	
	private int activePort;
	public int ActivePort { get; set; }
	
	public TcpServerHandler TcpServerHandler { get; set; }
	public UdpServerHandler UdpServerHandler { get; set; }

	public TcpUdpServerHandler(MainWindowViewModel viewModel)
	{
		ViewModel = viewModel;
		
		TcpServerHandler = new TcpServerHandler(this, viewModel.Config);
		UdpServerHandler = new UdpServerHandler(this, viewModel.Config);
	}
	
	public bool StartServer()
	{
		bool tcpActive = TcpServerHandler.Start(); 
		bool udpActive = UdpServerHandler.Start();
		Console.WriteLine($"Server Starting | Port: {ActivePort}, tcpActive: {tcpActive}, udpActive: {udpActive}");
		return tcpActive && udpActive;
	}

	public bool StopServer()
	{
		_ = TcpServerHandler.StopAsync();
		UdpServerHandler.Stop();
		Console.WriteLine($"Server Stopping | Port: {ActivePort}");
		return false;
	}
}