using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Server.Models;
using Server.Views;

namespace Server.Network;

public class TcpServerHandler
{
	private readonly MainWindowView mainWindowView;
	private CancellationTokenSource? cancellationTokenSource;
	public JObject config;
	private TcpListener? connectionListener;
	private Task? connectionTask;

	public TcpServerHandler(MainWindowView mainWindowView)
	{
		this.mainWindowView = mainWindowView;
		config = JObject.Parse(File.ReadAllText(LoadFile.Load("Config", "Config.json")));
	}

	public bool Start()
	{
		try
		{
			var port = config["SERVER_CLIENT_PORT"]?.Value<int>() ?? -1;
			if (port == -1) throw new Exception("Could not get config SERVER_CLIENT_PORT");
			cancellationTokenSource = new CancellationTokenSource();
			connectionListener = new TcpListener(IPAddress.Any, port);
			connectionListener.Start();
			connectionTask = Task.Run(() => ClientConnectionListener(cancellationTokenSource.Token));
			Logger.Info("TcpServerHandler.Start", "Server started");
			mainWindowView.UpdateClientPortTextBox(port.ToString());
			return true;
		}
		catch (Exception ex)
		{
			Logger.Error("TcpServerHandler.Start", ex.ToString());
			return false;
		}
	}

	public async Task StopAsync()
	{
		try
		{
			if (cancellationTokenSource != null) cancellationTokenSource.Cancel();

			connectionListener?.Stop();

			if (connectionTask != null) await connectionTask;
		}
		catch (Exception ex)
		{
			Logger.Error("TcpServerHandler.Stop", ex.ToString());
		}
		finally
		{
			cancellationTokenSource?.Dispose();
			connectionListener = null;
			cancellationTokenSource = null;
			connectionTask = null;
		}
	}

	private async Task ClientConnectionListener(CancellationToken token)
	{
		try
		{
			while (!token.IsCancellationRequested)
			{
				var client = await connectionListener!.AcceptTcpClientAsync(token);

				if (token.IsCancellationRequested)
				{
					client.Close();
					break;
				}

				Logger.Info("TcpServerHandler.ClientConnectionListener", "Creating client connection listener");
				_ = Task.Run(() => ClientListener(client, token));
			}
		}
		catch (OperationCanceledException)
		{
			Logger.Info("TcpServerHandler.ClientConnectionListener", "Listening operation canceled.");
		}
		catch (SocketException ex)
		{
			Logger.Error("TcpServerHandler.ClientConnectionListener", $"Socket error: {ex}");
		}
		catch (Exception ex)
		{
			Logger.Error("TcpServerHandler.ClientConnectionListener", ex.ToString());
		}
	}

	private async Task ClientListener(TcpClient client, CancellationToken token)
	{
		var clientEndPoint = client.Client.RemoteEndPoint?.ToString() ?? "Unknown";
		Logger.Info("TcpServerHandler.ClientListener", $"Client connected: {clientEndPoint}");

		try
		{
			using var clientStream = client.GetStream();
			var receivedBytes = new byte[10240];

			while (!token.IsCancellationRequested)
			{
				var bytesRead = await clientStream.ReadAsync(receivedBytes, 0, receivedBytes.Length, token);
				if (bytesRead == 0) break; // Client disconnected

				var receivedMessage = Encoding.UTF8.GetString(receivedBytes, 0, bytesRead);
				var receivedJson = JsonConvert.DeserializeObject<dynamic>(receivedMessage);
				if (receivedJson is JObject jsonObject)
				{
					var callback = jsonObject["callback"]?.ToString();
					if (!string.IsNullOrEmpty(callback) &&
						TcpCallbackHandler.CommandMap.TryGetValue(callback, out var f))
					{
						Logger.Debug("TcpServerHandler.ListenToServerAsync", jsonObject.ToString());
						await f(this, client, jsonObject);
					}
				}
				else
				{
					Logger.Warning("TcpServerHandler.ListenToServerAsync", "Received JSON is not a valid object");
				}
			}
		}
		catch (OperationCanceledException)
		{
			Logger.Info("TcpServerHandler.ClientListener", "Client operation canceled.");
		}
		catch (Exception ex)
		{
			Logger.Error("TcpServerHandler.ClientListener", ex.ToString());
		}
		finally
		{
			client.Close();
			TcpClients.clientStreams.Remove(clientEndPoint);
			Logger.Info("TcpServerHandler.ClientListener", $"Client disconnected: {clientEndPoint}");
		}
	}
}