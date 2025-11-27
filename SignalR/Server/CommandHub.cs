using Common.Models;
using Microsoft.AspNetCore.SignalR;

namespace SignalR.Server;

public class CommandHub : Hub
{
    private static int clientCount = 0;
    public static int ClientCount => clientCount;
    public static event Action ClientCountChanged;

    public override Task OnConnectedAsync()
    {
        Interlocked.Increment(ref clientCount);
        ClientCountChanged?.Invoke();
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        Interlocked.Decrement(ref clientCount);
        ClientCountChanged?.Invoke();
        return base.OnDisconnectedAsync(exception);
    }
    public Task<bool> AuthenticatePassword(string password)
    {
        bool ok = false;
        if ("admin" == password) ok = true;
        if ("red" == password) ok = true;
        if ("blue" == password) ok = true;
        return Task.FromResult(ok);
    }

    public Task RequestPing()
    {
        return Task.CompletedTask;
    }

    public async Task SendToAll(Message message)
    {
        await Clients.All.SendAsync(message.Callback, message);
    }

    public async Task SendToClient(string connectionId, Message message)
    {
        await Clients.Client(connectionId).SendAsync(message.Callback, message);
    }
}
