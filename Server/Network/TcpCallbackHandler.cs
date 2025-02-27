using System.Net.Sockets;
using Newtonsoft.Json.Linq;
using Server.Models;

namespace Server.Network;

public class TcpCallbackHandler
{
	public static readonly Dictionary<string, Func<JObject, TcpClient, JObject, Task>> CommandMap = new()
	{
		{ "OnAsyncTryConnect", OnAsyncTryConnect }
	};

	public static async Task OnAsyncTryConnect(JObject config, TcpClient client, JObject jObject)
	{
		JObject passwords = config["PASSWORDS"] as JObject;
		if (passwords != null)
		{
			foreach (var data in passwords)
			{
				var coalition = data.Key;
				var password = data.Value.ToString();
				if (password == jObject["password"]?.ToString())
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
		else
		{
			Logger.Error("TcpCallbackHandler.OnAsyncTryConnect", "PASSWORDS is not a valid JObject.");
		}
	}
}