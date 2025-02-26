using System.IO;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json.Linq;
using Server.Models;
using Server.Views;

namespace Server.Network;

public class UdpServerHandler
{
	private readonly MainWindowView mainWindowView;
	private CancellationTokenSource? cancellationTokenSource;
	public JObject config;
	private UdpClient? udpClient;
	private Task? udpTask;

	public UdpServerHandler(MainWindowView mainWindowView)
	{
		this.mainWindowView = mainWindowView;
		config = JObject.Parse(File.ReadAllText(LoadFile.Load("Config", "Config.json")));
	}

	public bool Start()
	{
		try
		{
			var port = config["DCS_TO_SERVER_PORT"]?.Value<int>() ?? -1;
			if (port == -1) throw new Exception("Could not get config DCS_TO_SERVER_PORT");

			cancellationTokenSource = new CancellationTokenSource();
			udpClient = new UdpClient(port);

			udpTask = Task.Run(() => DCSListener(cancellationTokenSource.Token));

			return true;
		}
		catch (Exception ex)
		{
			Logger.Error("UdpServerHandler.Start", ex.ToString());
			return false;
		}
	}

	public void Stop()
	{
		try
		{
			cancellationTokenSource?.Cancel();
			udpClient?.Close();
			udpClient?.Dispose();
		}
		catch (Exception ex)
		{
			Logger.Error("UdpServerHandler.Stop", ex.ToString());
		}
	}

	private async Task DCSListener(CancellationToken token)
	{
		try
		{
			while (!token.IsCancellationRequested)
				try
				{
					if (udpClient == null) return;

					var result = await udpClient.ReceiveAsync();
					var json = Encoding.UTF8.GetString(result.Buffer);

					if (string.IsNullOrWhiteSpace(json))
						continue;

					var receivedJson = JObject.Parse(json);

					if (receivedJson.TryGetValue("callback", out var callbackToken) &&
						callbackToken?.Type == JTokenType.String)
					{
						var callback = callbackToken.ToString();
						if (callback == "OnGlobalContactExport") await TcpClientSender.SendToClients(receivedJson);
					}
				}
				catch (Exception ex)
				{
					Logger.Debug("UdpServerHandler.DCSListener", ex.ToString());
				}
		}
		catch (OperationCanceledException)
		{
			Logger.Debug("UdpServerHandler.DCSListener", "Listener stopped.");
		}
	}
}