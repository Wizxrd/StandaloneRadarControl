using Common.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
namespace Server.Socket;

public class UdpSender
{
    public static UdpClient UdpClient = new();

    public static async Task SendToDCS(JObject jObject)
    {
        if (jObject == null)
        {
            throw new ArgumentNullException(nameof(jObject), "The JSON object to send cannot be null.");
        }
        try
        {
            string serialized = JsonConvert.SerializeObject(jObject, Formatting.Indented);
            byte[] buffer = Encoding.UTF8.GetBytes(serialized);

            if (UdpClient == null)
            {
                throw new InvalidOperationException("UDP client is not initialized.");
            }

            if (buffer.Length == 0)
            {
                throw new InvalidOperationException("Serialized JSON object is empty, nothing to send.");
            }

            int bytesSent = await UdpClient.SendAsync(buffer, buffer.Length, "127.0.0.1", App.Settings.GeneralSettings.Ports.DcsUdpReceive);

            if (bytesSent != buffer.Length)
            {
                Logger.Warning("SendToDCS", $"Partial send detected. Expected: {buffer.Length}, Sent: {bytesSent}");
            }
        }
        catch (SocketException ex)
        {
            Logger.Error("UdpSender.SendToDCS", $"SocketException: {ex.Message}. ErrorCode: {ex.SocketErrorCode}");
        }
        catch (InvalidOperationException ex)
        {
            Logger.Error("UdpSender.SendToDCS", $"InvalidOperationException: {ex.Message}");
        }
        catch (JsonException ex)
        {
            Logger.Error("UdpSender.SendToDCS", $"JsonException during serialization: {ex.Message}");
        }
        catch (Exception ex)
        {
            Logger.Error("UdpSender.SendToDCS", $"Unexpected exception: {ex.Message}");
            throw;
        }
    }
}
