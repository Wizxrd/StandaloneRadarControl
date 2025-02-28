using System.IO;
using System.Windows;
using Newtonsoft.Json;
using Server.Resources.Models;

namespace Server.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
	public readonly Config Config;
	
	private IServerHandler serverHandler;
	public IServerHandler ServerHandler
	{
		get => serverHandler;
		set
		{
			serverHandler = value;
			OnPropertyChanged();
		}
	}

	private bool serverRunning;
	public bool ServerRunning	{
		get => serverRunning;
		set
		{
			serverRunning = value;
			OnPropertyChanged();
		}
	}

	public MainWindowViewModel()
	{
		//File.WriteAllTextAsync(path:"./Config/Config.json", JsonConvert.SerializeObject(new Config(), Formatting.Indented,
		//	new Newtonsoft.Json.Converters.StringEnumConverter()));

		Config =  JsonConvert.DeserializeObject<Config>(File.ReadAllText(Path.Join("Config", "Config.json")),
			new Newtonsoft.Json.Converters.StringEnumConverter());
		
		Console.WriteLine(JsonConvert.SerializeObject(Config, Formatting.Indented));
		
		ServerHandler = new TcpUdpServerHandler(this);
	}

	public bool StartServer()
	{
		if (ServerRunning)
		{
			Console.WriteLine("Server is already running.");
			return ServerRunning;
		}
		
		ServerRunning = serverHandler.StartServer();
		return ServerRunning;
	}

	public void StopServer()
	{
		serverHandler.StopServer();
		ServerRunning = false;
	}

	public void ExitApplication()
	{
		StopServer();
		Application.Current.Shutdown();
	}
}