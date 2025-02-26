using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using Client.Models;
using Client.Views;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Client.Network;

public class TcpClientHandler
{
	private readonly MainWindowView mainWindowView;
	private CancellationTokenSource? cancellationTokenSource;
	private NetworkStream? clientStream;
	private Task? clientTask;

	public bool connectionEstablished = false;
	private bool connectionRefused = false;
	private TcpClient? tcpClient;

	public TcpClientHandler(MainWindowView mainWindowView)
	{
		this.mainWindowView = mainWindowView;
	}

	public async Task AsyncTryConnect(JObject jObject)
	{
		try
		{
			Logger.Debug("TcpClientHandler.TcpConnectAsync", jObject.ToString());
			tcpClient = new TcpClient();
			var ip = jObject["ip"]?.ToString() ?? string.Empty;
			var port = jObject["port"]?.Value<int>() ?? -1;
			if (ip == string.Empty || port == -1)
			{
				MessageBox.Show("TcpClientHandler.TcpConnectAsync\nConnection Requires IP and PORT");
				return;
			}

			var connection = tcpClient.ConnectAsync(ip, port);
			if (await Task.WhenAny(connection, Task.Delay(TimeSpan.FromSeconds(10))) == connection)
			{
				if (tcpClient.Connected)
				{
					clientStream = tcpClient.GetStream();
					cancellationTokenSource = new CancellationTokenSource();
					var token = cancellationTokenSource.Token;
					clientTask = Task.Run(() => ListenToServerAsync(token));
					await TcpClientSender.AsyncSendJsonToServer(clientStream, jObject);
					Logger.Info("TcpClientHandler.TcpConnectAsync",
						$"Attempting to establish connection to {ip}:{port}");
					var success = await WaitForConnectionAsync(TimeSpan.FromSeconds(10));
					if (success)
						Logger.Info("TcpClientHandler.TcpConnectAsync",
							$"Connection successfully established to {ip}:{port}");
					else
						HandleConnectionFailure("Connection timeout");
				}
				else
				{
					HandleConnectionFailure("Server offline");
				}
			}
			else
			{
				HandleConnectionFailure("Connection timeout");
			}
		}
		catch (Exception ex)
		{
			Logger.Error("TcpClientHandler.TcpConnectAsync", ex.ToString());
		}
	}

	private async Task<bool> WaitForConnectionAsync(TimeSpan timeout)
	{
		var startTime = DateTime.Now;
		while (DateTime.Now - startTime < timeout)
		{
			if (connectionEstablished) return true;
			await Task.Delay(100);
		}

		return false;
	}

	private void HandleConnectionFailure(string reason)
	{
		tcpClient?.Close();
		clientStream?.Dispose();
		cancellationTokenSource?.Cancel();
		tcpClient = null;
		clientStream = null;
		cancellationTokenSource = null;
		Logger.Warning("TcpClientHandler.HandleConnectionFailure", $"{reason}");
	}

	private async Task ListenToServerAsync(CancellationToken cancellationToken)
	{
		var receivedBytes = new byte[64000];
		while (!cancellationToken.IsCancellationRequested)
			try
			{
				if (clientStream == null)
				{
					Logger.Info("TcpClientHandler.ListenToServerAsync", "Client stream is null");
					return;
				}

				var bytesRead = await clientStream.ReadAsync(receivedBytes, 0, receivedBytes.Length, cancellationToken);
				if (bytesRead == 0)
				{
					Logger.Info("TcpClientHandler.ListenToServerAsync", "Server closed the connection");
					break;
				}

				var receivedMessage = Encoding.UTF8.GetString(receivedBytes, 0, bytesRead);
				var receivedJson = JsonConvert.DeserializeObject<dynamic>(receivedMessage);
				if (receivedJson == null)
				{
					Logger.Warning("TcpClientHandler.ListenToServerAsync", "Received null or invalid JSON");
					continue;
				}

				if (receivedJson is JObject jsonObject)
				{
					var callback = jsonObject["callback"]?.ToString();
					if (!string.IsNullOrEmpty(callback) &&
						TcpCallbackHandler.CommandMap.TryGetValue(callback, out var f))
						Application.Current.Dispatcher.Invoke(() => { f(this, mainWindowView, jsonObject); });
				}
				else
				{
					Logger.Warning("TcpClientHandler.ListenToServerAsync", "Received JSON is not a valid object");
				}
			}
			catch (IOException ex)
			{
				Logger.Info("TcpClientHandler.ListenToServerAsync",
					$"Connection closed or error reading data: {ex.Message}");
				break;
			}
			catch (OperationCanceledException)
			{
				Logger.Info("TcpClientHandler.ListenToServerAsync", "Listener task was cancelled");
				break;
			}
			catch (Exception ex)
			{
				Logger.Error("TcpClientHandler.ListenToServerAsync", $"Unexpected error: {ex.Message}");
				break;
			}
	}
}