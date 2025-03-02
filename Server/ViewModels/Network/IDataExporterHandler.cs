using Newtonsoft.Json.Linq;

namespace Server.ViewModels.Network;

/// <summary>
/// Handler for Data being sent to Standalone Radar Control Clients.
/// </summary>
public interface IDataExporterHandler
{
	bool HandlerActive { get; }
	
	string SrcHostName { get; init; }
	int SrcExternalPort { get; init; }
	
	public bool StartHandler();
	public bool StopHandler();

	public Task SendDataToAllClients(JObject data);
}