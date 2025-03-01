using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Server.Models;
using System.Windows;
using Server.ViewModels.Utils;

namespace Server.ViewModels.Network
{
    public class TcpCallbackHandler
    {
        public static readonly Dictionary<string, Func<TcpServerHandler, TcpClient, JObject, Task>> CommandMap = new()
        {
            { "OnAsyncTryConnect", OnAsyncTryConnect }
        };

        public static async Task OnAsyncTryConnect(TcpServerHandler tcpServerHandler, TcpClient client, JObject jObject)
        {
            var passwords = tcpServerHandler.config["PASSWORDS"] as JObject;
            if (passwords != null)
            {
                foreach (var data in passwords)
                {
                    string coalition = data.Key;
                    string password = data.Value.ToString();
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
            else
            {
                Logger.Error("TcpCallbackHandler.OnAsyncTryConnect", "PASSWORDS is not a valid JObject.");
            }
        }
    }
}
