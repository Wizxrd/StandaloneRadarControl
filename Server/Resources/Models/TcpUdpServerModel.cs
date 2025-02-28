using Server.Network;
using Server.Resources.Interfaces;
using Server.ViewModels;

namespace Server.Resources.Models;

public class TcpUdpServerModel : ModelBase, IServerModel
{
	MainWindowViewModel ViewModel;
	
	private int clientCount;
	public int ClientCount
	{
		get => clientCount;
		set
		{
			clientCount = value; 
			OnPropertyChanged();
		}
	}
	
	private float pingInterval;
	public float PingInterval	{
		get => pingInterval;
		set
		{
			pingInterval = value; 
			OnPropertyChanged();
		}
	}
	
	private int updatesPerSecond;
	public int UpdatesPerSecond	{
		get => updatesPerSecond;
		set
		{
			updatesPerSecond = value; 
			OnPropertyChanged();
		}
	}
	
	private int activePort;
	public int ActivePort	{
		get => activePort;
		set
		{
			activePort = value; 
			OnPropertyChanged();
		}
	}
	
	private TcpServerHandler TcpServerHandler { get; init; }

	private UdpServerHandler UdpServerHandler { get; init; }
	
	public TcpUdpServerModel(MainWindowViewModel viewModel)
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