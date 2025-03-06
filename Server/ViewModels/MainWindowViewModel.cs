using System.IO;
using System.Windows;
using Newtonsoft.Json;
using Server.Models;
using Server.Models.Network;

namespace Server.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
	public readonly Config Config;
	
	private IDataImportHandler dataImportHandler; // Field is proxied with a Property to Enable WPF Data Binding
	public IDataImportHandler DataImportHandler
	{
		get => dataImportHandler;
		set
		{
			dataImportHandler = value; 
			OnPropertyChanged();
		}
	}
	private IDataExporterHandler dataExportHandler; // Field is proxied with a Property to Enable WPF Data Binding
	public IDataExporterHandler DataExportHandler
	{
		get => dataExportHandler;
		set
		{
			dataExportHandler = value; 
			OnPropertyChanged();
		}
	}
	
	private bool serverRunning; // Field is proxied with a Property to Enable WPF Data Binding
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
		DataExportHandler = new TcpExportHandler(this);
		DataImportHandler = new UdpImportHandler(this);
		
		// Possible Null Reference. If we don't find it, make it.
		string _config_path = Path.Join("Resources/Config", "Config.json");
		try
		{
			Config =  JsonConvert.DeserializeObject<Config>(File.ReadAllText(_config_path),
				new Newtonsoft.Json.Converters.StringEnumConverter());
		}
		catch (Exception e)
		{
			Console.WriteLine($"Could not find Config.json. Making a new one. Error Message: {e.Message}");
			
			Config = new Config().ConfigDefaults();

			// indent the Json and flatten Enums to their Text values.
			string _serialized_json = JsonConvert.SerializeObject(Config, 
				Formatting.Indented, new Newtonsoft.Json.Converters.StringEnumConverter());
			File.WriteAllTextAsync(_config_path, _serialized_json);
		}
	}

	public bool StartServer()
	{
		if (ServerRunning)
		{
			Console.WriteLine("Server is already running.");
			return ServerRunning;
		}

		bool importHandler = DataImportHandler.StartHandler();
		bool exportHandler = DataExportHandler.StartHandler();

		// If one of the other fails to start, stop the other one and return a failure to start.
		if (importHandler && !exportHandler) { DataImportHandler.StopHandler(); }
		if (exportHandler && !importHandler) { DataExportHandler.StopHandler(); }
		
		ServerRunning = importHandler && exportHandler;
		return ServerRunning;
	}

	public bool StopServer()
	{
		bool importHandler = DataImportHandler.StopHandler();
		bool exportHandler = DataExportHandler.StopHandler();

		ServerRunning = importHandler && exportHandler;
		return ServerRunning;
	}

	public void ExitApplicaiton()
	{
		StopServer();
		Application.Current.Shutdown();
	}
}