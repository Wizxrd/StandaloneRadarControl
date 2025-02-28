using System.IO;
using System.Windows;
using Newtonsoft.Json.Linq;
using Server.Models;
using Server.Network;

namespace Server.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
	public JObject Config;
	
	public TcpServerHandler TcpServerHandler { get; set; }
	public UdpServerHandler UdpServerHandler { get; set; }

	private int clientPort = 0;
	public int ClientPort
	{
		get => clientPort;
		set
		{
			clientPort = value;
			OnPropertyChanged();
		}
	}

	public MainWindowViewModel()
	{
		TcpServerHandler = new TcpServerHandler(this);
		UdpServerHandler = new UdpServerHandler(this);
		Config = JObject.Parse(File.ReadAllText(LoadFile.Load("Config", "Config.json")));
	}

	public (bool, bool) StartServer()
	{
		bool tcpActive = TcpServerHandler.Start(); 
		bool udpActive = UdpServerHandler.Start(); 
		
		Console.WriteLine($"Server started: {ClientPort}");
		
		return (tcpActive, udpActive);
	}

	public async Task StopServer()
	{
		await TcpServerHandler.StopAsync();
		UdpServerHandler.Stop();
		ClientPort = 0;
		Console.WriteLine($"Server stopped {ClientPort}");
	}

	public async Task ExitApplication()
	{
		await TcpServerHandler.StopAsync();
		UdpServerHandler.Stop();
		Application.Current.Shutdown();
	}
}