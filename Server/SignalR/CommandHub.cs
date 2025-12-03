using Microsoft.AspNetCore.SignalR;

namespace Server.SignalR;

public class CommandHub : Hub
{
    private static int clientCount = 0;
    public static int ClientCount => clientCount;
    public static event Action ClientCountChanged;

    public static IHubContext<CommandHub> HubContext { get; set; }

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
        if (App.Settings.GeneralSettings.Passwords.Red == password) ok = true;
        if (App.Settings.GeneralSettings.Passwords.Blue == password) ok = true;
        return Task.FromResult(ok);
    }

    public async Task ReceiveEnvelope(string envelope)
    {
        await Clients.All.SendAsync("ReceiveEnvelope", envelope);
    }

    public static Task SendToAll(string envelope)
    {
        if (HubContext == null)
            return Task.CompletedTask;

        return HubContext.Clients.All.SendAsync("ReceiveEnvelope", envelope);
    }
}
