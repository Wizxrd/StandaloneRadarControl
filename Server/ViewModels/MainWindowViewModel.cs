using System.IO;
using System.Windows;
using Newtonsoft.Json.Linq;
using Server.Models;
using Server.Network;

namespace Server.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
	public JObject Config;
	
	public TcpServerHandler TcpServerHandler;
	public UdpServerHandler UdpServerHandler;

	public int ClientPort;
	
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
		return (tcpActive, udpActive);
	}

	public async Task StopServer()
	{
		await TcpServerHandler.StopAsync();
		UdpServerHandler.Stop();
	}

	public async Task ExitApplication()
	{
		await TcpServerHandler.StopAsync();
		UdpServerHandler.Stop();
		Application.Current.Shutdown();
	}
}