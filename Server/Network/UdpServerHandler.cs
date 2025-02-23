using Newtonsoft.Json.Linq;
using Server.Views;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Server.Models;

namespace Server.Network
{
    public class UdpServerHandler
    {
        private CancellationTokenSource? cancellationTokenSource;
        private UdpClient? udpClient;
        private Task? udpTask;
        private readonly MainWindowView mainWindowView;
        public JObject config;

        public UdpServerHandler(MainWindowView mainWindowView)
        {
            this.mainWindowView = mainWindowView;
            config = JObject.Parse(File.ReadAllText(LoadFile.Load("Config", "Config.json")));
        }

        public bool Start()
        {
            try
            {
                int port = config["DCS_TO_SERVER_PORT"]?.Value<int>() ?? -1;
                if (port == -1) { throw new Exception("Could not get config DCS_TO_SERVER_PORT"); }

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
                {
                    try
                    {
                        if (udpClient == null) return;

                        UdpReceiveResult result = await udpClient.ReceiveAsync();
                        string json = Encoding.UTF8.GetString(result.Buffer);

                        if (string.IsNullOrWhiteSpace(json))
                            continue;

                        JObject? receivedJson = JObject.Parse(json);

                        if (receivedJson.TryGetValue("callback", out JToken? callbackToken) &&
                            callbackToken?.Type == JTokenType.String)
                        {
                            string callback = callbackToken.ToString();
                            if (callback == "OnGlobalContactExport")
                            {
                                await TcpClientSender.SendToClients(receivedJson);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Debug("UdpServerHandler.DCSListener", ex.ToString());
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Logger.Debug("UdpServerHandler.DCSListener", "Listener stopped.");
            }
        }
    }
}
