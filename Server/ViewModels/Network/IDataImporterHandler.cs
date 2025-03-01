namespace Server.ViewModels.Network;

/// <summary>
/// Handler for bringing Data in from a Digital Combat Simulator Server.
/// </summary>
public interface IDataImporterHandler
{
	bool HandlerActive { get; }
	float UpdatesPerSecond { get; }
	
	string DcsHostName { get; init; }
	int SrcToDcsPort { get; init; }
	int DcsToSrcPort { get; init; }
	
	IDataExporterHandler ExporterHandler { get; init; }
	
	public bool StartDataImportHandler();
	public bool StopDataImportHandler();
}