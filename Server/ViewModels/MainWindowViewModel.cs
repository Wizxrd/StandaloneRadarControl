using System.IO;
using System.Windows;
using Newtonsoft.Json;
using Server.Resources.Interfaces;
using Server.Resources.Models;

namespace Server.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
	public readonly Config Config;

	private IServerModel serverModel;
	public IServerModel ServerModel
	{
		get => serverModel;
		set
		{
			serverModel = value; 
			OnPropertyChanged();
		}
	}

	private bool serverRunning;
	public bool ServerRunning{
		get => serverRunning;
		set
		{
			serverRunning = value;
			OnPropertyChanged();
		}
	}

	public MainWindowViewModel()
	{
		//File.WriteAllTextAsync(path:"./Config/Config.json", JsonConvert.SerializeObject(new Config.ConfigDefaults(), Formatting.Indented,
		//	new Newtonsoft.Json.Converters.StringEnumConverter()));

		Config =  JsonConvert.DeserializeObject<Config>(File.ReadAllText(Path.Join("Config", "Config.json")),
			new Newtonsoft.Json.Converters.StringEnumConverter());
		
		Console.WriteLine(JsonConvert.SerializeObject(Config, Formatting.Indented));
		
		ServerModel = new TcpUdpServerModel(this);
	}

	public bool StartServer()
	{
		if (ServerRunning)
		{
			Console.WriteLine("Server is already running.");
			return ServerRunning;
		}
		
		ServerRunning = ServerModel.StartServer();
		
		Console.WriteLine(ServerModel.ActivePort);
		return ServerRunning;
	}

	public void StopServer()
	{
		ServerModel.StopServer();
		ServerRunning = false;
		Console.WriteLine(ServerModel.ActivePort);
	}

	public void ExitApplication()
	{
		StopServer();
		Application.Current.Shutdown();
	}
}