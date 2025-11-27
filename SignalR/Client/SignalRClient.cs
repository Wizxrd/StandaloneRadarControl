using AdonisUI.Controls;
using Microsoft.AspNetCore.SignalR.Client;
using System.Data.Common;
using System.Diagnostics;
using System.Windows.Threading;
namespace SignalR.Client;

public class SignalRClient
{
    public static HubConnection? Connection { get; set; }

    public static async Task<bool> AsyncConnect(string address, string port, string password, string callsign)
    {
        if (Connection == null || Connection.State == HubConnectionState.Disconnected)
        {
            var url = $"http://{address}:{port}/SRCServer";
            Connection = new HubConnectionBuilder()
                .WithUrl(url)
                .WithAutomaticReconnect()
                .Build();
            try
            {
                await Connection.StartAsync();

                bool authenticated = await Connection.InvokeAsync<bool>("AuthenticatePassword", password);

                if (!authenticated)
                {
                    await Connection.StopAsync();
                    Connection = null;
                    MessageBox.Show("Incorrect password. Please try again.");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not connect: {ex.Message}");
            }
        }
        return false;
    }

    public static async Task<bool> AsyncDisconnect()
    {
        if (Connection == null)
            return true;

        try
        {
            if (Connection.State == HubConnectionState.Connected ||
                Connection.State == HubConnectionState.Connecting ||
                Connection.State == HubConnectionState.Reconnecting)
            {
                await Connection.StopAsync();
            }

            await Connection.DisposeAsync();
            Connection = null;
            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error while disconnecting: {ex.Message}");
            return false;
        }
    }
}
