using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Server.Models;
using System.Net;

namespace Server.Network
{
    public class TcpClientSender
    {
        public static async Task SendToClient(NetworkStream clientStream, JObject jObject)
        {
            try
            {
                string serialized = JsonConvert.SerializeObject(jObject, Formatting.Indented);
                byte[] buffer = Encoding.UTF8.GetBytes(serialized);
                await clientStream.WriteAsync(buffer, 0, buffer.Length);
            }
            catch (IOException ex)
            {
                Logger.Error("TcpServerHandler.SendToClient", $"IOException in: {ex.Message}");
                throw;
            }
            catch (SocketException ex)
            {
                Logger.Error("TcpServerHandler.SendToClient", $"SocketException in: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error("TcpServerHandler.SendToClient", $"Exception in: {ex.Message}");
                throw;
            }
        }

        public static async Task SendToClients(JObject jObject)
        {
            var disconnectedClients = new List<string>();

            foreach (var clientPair in TcpClients.clientStreams.ToList())
            {
                string ipEndPoint = clientPair.Key; // Always assign within the loop
                var stream = clientPair.Value;

                try
                {
                    Logger.Debug("TcpClientSender.SendToClients", $"Sending to: {ipEndPoint}");
                    await SendToClient(stream, jObject);
                }
                catch (IOException ex)
                {
                    Logger.Error("TcpClientSender.SendToClients", $"IOException for client {ipEndPoint}: {ex.Message}");
                    disconnectedClients.Add(ipEndPoint);
                }
                catch (SocketException ex)
                {
                    Logger.Error("TcpClientSender.SendToClients", $"SocketException for client {ipEndPoint}: {ex.Message}");
                    disconnectedClients.Add(ipEndPoint);
                }
                catch (Exception ex)
                {
                    Logger.Error("TcpClientSender.SendToClients", $"Unexpected error for {ipEndPoint}: {ex}");
                }
            }
            foreach (string client in disconnectedClients)
            {
                TcpClients.clientStreams.Remove(client);
                Logger.Debug("TcpClientSender.SendToClients", $"Removed disconnected client: {client}");
            }
        }

    }
}
