using System.Numerics;
using Client.Models;
using Client.Views;
using Newtonsoft.Json.Linq;

namespace Client.Network;

public class TcpCallbackHandler
{
	public static readonly Dictionary<string, Action<TcpClientHandler, MainWindowView, JObject>> CommandMap = new()
	{
		{ "OnConnectionEstablished", OnConnectionEstablished },
		{ "OnGlobalContactExport", OnGlobalContactExport }
	};

	public static void OnConnectionEstablished(TcpClientHandler handler, MainWindowView mainWindowView, JObject jObject)
	{
		Logger.Debug("TcpCallbackHandler.OnConnectionEstablished", "Connection established");
		handler.connectionEstablished = true;
	}

	public static void OnGlobalContactExport(TcpClientHandler handler, MainWindowView mainWindowView, JObject jObject)
	{
		foreach (JObject contact in (JArray)jObject["contacts"])
		{
			var name = (string)contact["name"];
			var player = (bool)contact["player"];
			var side = (string)contact["side"];
			var lat = (double)contact["lat"];
			var lon = (double)contact["lon"];
			var alt = (double)contact["alt"];
			var wind = (JObject)contact["wind"];
			var heading = (double)contact["heading"];
			var velocity = (JObject)contact["velocity"];
			var type = (string)contact["type"];
			var airborne = (bool)contact["airborne"];
			if (!Radar.Contacts.ContainsKey(name))
			{
				Logger.Debug("TcpCallbackHandler.OnGlobalContactExport", $"adding new: {name}");
				Contact newContact = new()
				{
					Latitude = lat,
					Longitude = lon,
					Heading = heading,
					Wind = new Vector3((float)wind["x"], (float)wind["y"], (float)wind["z"]),
					Velocity = new Vector3((float)velocity["x"], (float)velocity["y"], (float)velocity["z"])
				};
				Radar.Contacts[name] = newContact;
			}
			else
			{
				Logger.Debug("TcpCallbackHandler.OnGlobalContactExport", $"updating existing: {name}");
				var existingContact = Radar.Contacts[name];
				existingContact.Latitude = lat;
				existingContact.Longitude = lon;
				existingContact.Heading = heading;
				existingContact.Wind = new Vector3((float)wind["x"], (float)wind["y"], (float)wind["z"]);
				existingContact.Velocity =
					new Vector3((float)velocity["x"], (float)velocity["y"], (float)velocity["z"]);
			}
		}

		mainWindowView.InvalidateCanvas();
		Logger.Debug("TcpCallbackHandler.OnGlobalContactExport", "Contact export recieved!");
	}
}