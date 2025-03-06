using System.Net.Sockets;
using Newtonsoft.Json.Linq;
using Server.Models.Utils;

namespace Server.Models.Network
{
    public class TcpCallbackHandler
    {
        public static readonly Dictionary<string, Func<List<CoalitionDetail>, TcpClient, JObject, Task>> CommandMap = new()
        {
            { "OnAsyncTryConnect", OnAsyncTryConnect }
        };

        public static async Task OnAsyncTryConnect(List<CoalitionDetail> coalitionDetail, TcpClient client, JObject jObject)
        {
            foreach (CoalitionDetail data in coalitionDetail)
            {
                string coalition = data.COALITION.ToString();
                string password = data.PASSWORD;
                if (password == jObject["password"]?.ToString())
                {
                    JObject connected = new JObject
                    {
                        {"callback", "OnConnectionEstablished"}
                    };
                    NetworkStream stream = client.GetStream();
                    TcpClients.clientStreams.Add(client.Client.RemoteEndPoint.ToString(), stream);
                    await TcpClientSender.SendToClient(stream, connected);
                    return;
                }
            }
            client.Close();
            Logger.Info("TcpCallbackHandler.OnAsyncTryConnect", "Client disconnected due to invalid password.");
        }
    }
}
