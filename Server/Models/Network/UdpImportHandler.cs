using System.IO;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json.Linq;
using Server.Models.Utils;
using Server.ViewModels;

namespace Server.Models.Network
{
    public class UdpImportHandler : IDataImportHandler
    {
        // External Stuff (Always a Property.)
        private MainWindowViewModel ViewModel { get; init; }
        public IDataExporterHandler ExporterHandler { get; init; }
        
        // Internal Stuff
        public bool HandlerActive { get; private set; }
        public float UpdatesPerSecond { get; } //Unimplimented
        public string DcsHostName { get; init; } //Unimplimented
        public int SrcToDcsPort { get; init; } //Undeclared
        public int DcsToSrcPort { get; init; }
        
        private CancellationTokenSource? cancellationTokenSource;
        private UdpClient? udpClient;
        private Task? udpTask;
        
        public UdpImportHandler(MainWindowViewModel mainWindowViewModel)
        {
            this.ViewModel = mainWindowViewModel;

            ExporterHandler = ViewModel.DataExportHandler;
            
            UpdatesPerSecond = 0;
            DcsHostName = "localhost";
            
            DcsToSrcPort = ViewModel.Config.DCS_SERVER_SETTINGS.DCS_TO_SRC_PORT;
        }

        public bool StartHandler()
        {
            try
            {
                cancellationTokenSource = new CancellationTokenSource();
                udpClient = new UdpClient(DcsToSrcPort);

                udpTask = Task.Run(() => DCSListener(cancellationTokenSource.Token));

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("UdpServerHandler.Start", ex.ToString());
                return false;
            }
        }

        public bool StopHandler()
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
                return false;
            }

            return true;
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
                                await ExporterHandler.SendDataToAllClients(receivedJson);
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
