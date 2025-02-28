using System.Net.Sockets;
using Newtonsoft.Json.Linq;
using Server.Models;
using Server.Resources.Models;

namespace Server.Network;

public class TcpCallbackHandler
{
	public static readonly Dictionary<string, Func<Config, TcpClient, JObject, Task>> CommandMap = new()
	{
		{ "OnAsyncTryConnect", OnAsyncTryConnect }
	};

	public static async Task OnAsyncTryConnect(Config config, TcpClient client, JObject clientJObject)
	{
		foreach (var coalitionDetailsValue in config.CoalitionDetails)
		{
			if (coalitionDetailsValue.Password == clientJObject["password"]?.ToString())
			{
				var connected = new JObject
				{
					{ "callback", "OnConnectionEstablished" }
				};
				var stream = client.GetStream();
				TcpClients.clientStreams.Add(client.Client.RemoteEndPoint.ToString(), stream);
				await TcpClientSender.SendToClient(stream, connected);
				return;
			}
		}

		client.Close();
		Logger.Info("TcpCallbackHandler.OnAsyncTryConnect", "Client disconnected due to invalid password.");
	}
}