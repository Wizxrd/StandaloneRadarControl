using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;

namespace Server.Resources.Models;

public class Config
{
	// The Enum misbehaves, so here is how you would get a "new with Defaults" Config file.
	public Config ConfigDefaults()
	{
		return new Config()
		{
			CoalitionDetails = new List<CoalitionDetail>()
			{
				new CoalitionDetail()
					{ Coalition = CoalitionDetail.ECoalitions.Blue, Password = "blue", ClientLimit = -1 },
				new CoalitionDetail()
					{ Coalition = CoalitionDetail.ECoalitions.Red, Password = "red", ClientLimit = -1 },
				new CoalitionDetail()
					{ Coalition = CoalitionDetail.ECoalitions.Admin, Password = "admin", ClientLimit = -1 },
			}
		};
	}
	
	
	public string DcsSavedGames { get; set; } = string.Empty;
	public string DiscordPresenceServerName { get; set; } = string.Empty;
	public int ServerToClientPort { get; set; } = 7500;
	public int ServerToDcsPort { get; set; } = 7600;
	public int DcsToServerPort { get; set; } = 7700;
	public bool SimultaneousConnectionsAllowed { get; set; } = true;
	public ExportMethods ExportMethods { get; set; } = new ExportMethods();

	public List<CoalitionDetail> CoalitionDetails { get; set; } = new List<CoalitionDetail>();
}
public class ExportMethods
{
	public bool Global { get; set; } = true;
	public bool SemiReal { get; set; } = false;
	public bool Realistic { get; set; } = false;
	public bool LandUnits { get; set; } = false;
	public bool NavalUnits { get; set; } = false;
}
public class CoalitionDetail
{
	public enum ECoalitions
	{
		Blue, Red, Admin
	}
	
	public ECoalitions Coalition { get; set; } = ECoalitions.Blue;
	
	public string Password { get; set; } = "blue";
	public int ClientLimit { get; set; } = -1;
}
