using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;
namespace SignalR.Server;

public class SignalRServer
{
    private IHost? host { get; set; }
    public bool Started => host != null;

    public async Task Start(int port)
    {
        try
        {
            if (host != null) return;
            host = Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseKestrel();
                    webBuilder.UseUrls($"http://0.0.0.0:{port}");
                    webBuilder.ConfigureServices(services =>
                    {
                        services.AddSignalR();
                    });
                    webBuilder.Configure(app =>
                    {
                        app.UseRouting();
                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapHub<CommandHub>("/SRCServer");
                        });
                    });
                })
                .Build();
            await host.StartAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"ERROR: SignalRServer.Start() | {ex}");
        }
    }

    public async void Stop()
    {
        try
        {
            if (host != null)
            {
                await host.StopAsync();
                host.Dispose();
                host = null;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"ERROR: SignalRServer.Stop() | {ex}");
        }
    }
}
