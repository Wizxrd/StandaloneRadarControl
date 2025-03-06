using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Server.Models.Utils;
using Server.ViewModels;

namespace Server.Models.Network;

public class TcpExportHandler : IDataExporterHandler
{
    // External Stuff (Always a Property.)
    private MainWindowViewModel ViewModel { get; init; }
    
    // Internal Stuff
    public bool HandlerActive { get; private set; }

    public string SrcHostName { get; init; } = "localhost"; // Unimplemented
    public int SrcExternalPort { get; init; }
    
    private CancellationTokenSource? cancellationTokenSource;
    private TcpListener? connectionListener;
    private Task? connectionTask;



    public TcpExportHandler(MainWindowViewModel mainWindowViewModel)
    {
        this.ViewModel = mainWindowViewModel;
        SrcExternalPort = ViewModel.Config.SERVER_CLIENT_PORT;
    }

    public bool StartHandler()
    {
        try
        {
            cancellationTokenSource = new CancellationTokenSource();
            connectionListener = new TcpListener(IPAddress.Any, SrcExternalPort);
            connectionListener.Start();
            connectionTask = Task.Run(() => ClientConnectionListener(cancellationTokenSource.Token));
            Logger.Info("TcpServerHandler.Start", "Server started");
            HandlerActive = true;
            return HandlerActive;
        }
        catch (Exception ex)
        {
            Logger.Error("TcpServerHandler.Start", ex.ToString());
            HandlerActive = false;
            return HandlerActive;
        }
    }

    public bool StopHandler()
    {
        _ = StopAsync();
        HandlerActive = false;
        return HandlerActive;
    }
    
    public async Task StopAsync()
    {
        try
        {
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
            }

            connectionListener?.Stop();

            if (connectionTask != null)
            {
                await connectionTask;
            }
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
                TcpClient client = await connectionListener!.AcceptTcpClientAsync(token);

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
        string clientEndPoint = client.Client.RemoteEndPoint?.ToString() ?? "Unknown";
        Logger.Info("TcpServerHandler.ClientListener", $"Client connected: {clientEndPoint}");

        try
        {
            using NetworkStream clientStream = client.GetStream();
            byte[] receivedBytes = new byte[10240];

            while (!token.IsCancellationRequested)
            {
                int bytesRead = await clientStream.ReadAsync(receivedBytes, 0, receivedBytes.Length, token);
                if (bytesRead == 0)
                {
                    break; // Client disconnected
                }

                string receivedMessage = Encoding.UTF8.GetString(receivedBytes, 0, bytesRead);
                var receivedJson = JsonConvert.DeserializeObject<dynamic>(receivedMessage);
                if (receivedJson is JObject jsonObject)
                {
                    string? callback = jsonObject["callback"]?.ToString();
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
    
    public Task SendDataToAllClients(JObject data)
    {
        return TcpClientSender.SendToClients(data);
    }
}


