namespace Server.Models;

public class Config
{
	public string DCS_SAVED_GAMES { get; set; } = string.Empty;
	public string DISCORD_PRESENCE_SERVER_NAME { get; set; } = string.Empty;
	public int SERVER_CLIENT_PORT { get; set; } = 7500;
	public bool SIMULTANEOUS_CONNECTIONS_ALLOWED { get; set; } = true;
	public DcsServerSettings DCS_SERVER_SETTINGS { get; set; } = new DcsServerSettings();
	public ExportMethods EXPORT_METHODS { get; set; } = new ExportMethods();
	public List<CoalitionDetail> COALITION_DETAILS { get; set; } = new List<CoalitionDetail>();
	
	// The Enum misbehaves, so here is how you would get a "new with Defaults" Config file.
	public Config ConfigDefaults()
	{
		return new Config()
		{
			COALITION_DETAILS = new List<CoalitionDetail>()
			{
				new CoalitionDetail()
					{ COALITION = CoalitionDetail.ECoalitions.Blue, PASSWORD = "blue", CLIENT_LIMIT = -1 },
				new CoalitionDetail()
					{ COALITION = CoalitionDetail.ECoalitions.Red, PASSWORD = "red", CLIENT_LIMIT = -1 },
				new CoalitionDetail()
					{ COALITION = CoalitionDetail.ECoalitions.Admin, PASSWORD = "admin", CLIENT_LIMIT = -1 },
			}
		};
	}
}

public class DcsServerSettings
{
	public string DCS_HOST_NAME { get; set; } = "localhost";
	public int SRC_TO_DCS_PORT { get; set; } = 7600;
	public string DCS_PASSWORD { get; set; } = string.Empty;
	public int DCS_TO_SRC_PORT { get; set; } = 7700;
}

public class ExportMethods
{
	public bool GLOBAL { get; set; } = true;
	public bool SEMI_REAL { get; set; } = false;
	public bool REALISTIC { get; set; } = false;
	public bool LAND_UNITS { get; set; } = false;
	public bool NAVAL_UNITS { get; set; } = false;
}
public class CoalitionDetail
{
	public enum ECoalitions
	{
		Blue, Red, Admin
	}
	
	public ECoalitions COALITION { get; set; } = ECoalitions.Blue;
	
	public string PASSWORD { get; set; } = "blue";
	public int CLIENT_LIMIT { get; set; } = -1;
}