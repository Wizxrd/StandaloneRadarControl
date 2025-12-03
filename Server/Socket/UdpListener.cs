using Common.Models;
using Common.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Server.SignalR;
using System.Data;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
namespace Server.Socket;

public class UdpListener
{
    public UdpClient UdpClient;
    public Thread UdpThread;

    public UdpListener()
    {
        UdpClient = new(new IPEndPoint(IPAddress.Any, App.Settings.GeneralSettings.Ports.ServerUdpReceive));
        UdpThread = new Thread(AsyncListener);
        UdpThread.IsBackground = true;
    }

    public void Start()
    {
        UdpThread.Start();
    }

    public void Stop()
    {
        UdpThread.Abort(); //FIXME?
    }

    public void Dispose()
    {
        Stop();
        UdpThread = null;
    }

    private async void AsyncListener()
    {
        while (true)
        {
            try
            {
                if (UdpClient != null)
                {
                    var result = await UdpClient.ReceiveAsync();
                    string json = Encoding.UTF8.GetString(result.Buffer);
                    JObject receivedJson = JsonConvert.DeserializeObject<JObject>(json);
                    CommandEnvelope envelope = new CommandEnvelope
                    {
                        Command = (string)receivedJson["callback"],
                        Payload = (object)receivedJson["airplanes"]
                    };
                    string envelopeJson = JsonConvert.SerializeObject(envelope);
                    await CommandHub.SendToAll(envelopeJson);
                    Logger.Info("UdpListener.AsyncListener", $"Received UDP packet from {result.RemoteEndPoint.Address}:{result.RemoteEndPoint.Port} - {json}");
                }
            }
            catch (ObjectDisposedException ex)
            {
                Logger.Error("UdpListener.AsyncListener", $"ObjectDisposedException: {ex}");
                break;
            }
            catch (Exception ex)
            {
                Logger.Error("UdpListener.AsyncListener", $"Unexpected exception: {ex}");
            }
        }
    }
}
