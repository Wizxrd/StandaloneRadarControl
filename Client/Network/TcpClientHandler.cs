using System;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Client.Models;
using System.Net.Http;
using System.Windows;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.IO;
using Newtonsoft.Json;
using System.Threading;
using Client.Views;

namespace Client.Network
{
    public class TcpClientHandler
    {
        private TcpClient? tcpClient;
        private NetworkStream? clientStream;
        private Task? clientTask;
        private MainWindowView mainWindowView;
        CancellationTokenSource? cancellationTokenSource;

        public bool connectionEstablished = false;
        private bool connectionRefused = false;

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
                string ip = jObject["ip"]?.ToString() ?? string.Empty;
                int port = jObject["port"]?.Value<int>() ?? -1;
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
                        CancellationToken token = cancellationTokenSource.Token;
                        clientTask = Task.Run(() => ListenToServerAsync(token));
                        await TcpClientSender.AsyncSendJsonToServer(clientStream, jObject);
                        Logger.Info("TcpClientHandler.TcpConnectAsync", $"Attempting to establish connection to {ip}:{port}");
                        bool success = await WaitForConnectionAsync(TimeSpan.FromSeconds(10));
                        if (success)
                        {
                            Logger.Info("TcpClientHandler.TcpConnectAsync", $"Connection successfully established to {ip}:{port}");
                        }
                        else
                        {
                            HandleConnectionFailure("Connection timeout");
                        }
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
            DateTime startTime = DateTime.Now;
            while ((DateTime.Now - startTime) < timeout)
            {
                if (connectionEstablished)
                {
                    return true;
                }
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
            byte[] receivedBytes = new byte[64000];
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (clientStream == null)
                    {
                        Logger.Info("TcpClientHandler.ListenToServerAsync", "Client stream is null");
                        return;
                    }
                    int bytesRead = await clientStream.ReadAsync(receivedBytes, 0, receivedBytes.Length, cancellationToken);
                    if (bytesRead == 0)
                    {
                        Logger.Info("TcpClientHandler.ListenToServerAsync", "Server closed the connection");
                        break;
                    }

                    string receivedMessage = Encoding.UTF8.GetString(receivedBytes, 0, bytesRead);
                    var receivedJson = JsonConvert.DeserializeObject<dynamic>(receivedMessage);
                    if (receivedJson == null)
                    {
                        Logger.Warning("TcpClientHandler.ListenToServerAsync", "Received null or invalid JSON");
                        continue;
                    }

                    if (receivedJson is JObject jsonObject)
                    {
                        string? callback = jsonObject["callback"]?.ToString();
                        if (!string.IsNullOrEmpty(callback) && TcpCallbackHandler.CommandMap.TryGetValue(callback, out var f))
                        {
                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                f(this, mainWindowView, jsonObject);
                            });
                        }
                    }
                    else
                    {
                        Logger.Warning("TcpClientHandler.ListenToServerAsync", "Received JSON is not a valid object");
                    }
                }
                catch (IOException ex)
                {
                    Logger.Info("TcpClientHandler.ListenToServerAsync", $"Connection closed or error reading data: {ex.Message}");
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
    }
}
