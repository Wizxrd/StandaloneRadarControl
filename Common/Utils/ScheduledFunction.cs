using System.Windows;
namespace Common.Utils;

public class ScheduledFunction : IDisposable
{
    private readonly Func<Task> callback;
    private CancellationTokenSource? cancellationTokenSource;
    private Task? loopTask;
    private bool callOnStart;

    public TimeSpan Interval;

    public ScheduledFunction(Func<Task> refreshCallback, int interval, bool callOnStart = false)
    {
        this.callback = refreshCallback;
        Interval = TimeSpan.FromSeconds(interval);
        this.callOnStart = callOnStart;
    }

    public void Start()
    {
        if (cancellationTokenSource != null) return;
        cancellationTokenSource = new CancellationTokenSource();

        if (callOnStart) _ = callback();

        _ = Task.Run(async () =>
        {
            var token = cancellationTokenSource.Token;
            while (!token.IsCancellationRequested)
            {
                var started = DateTime.UtcNow;
                try
                {
                    await Application.Current.Dispatcher.BeginInvoke(() => { callback(); });
                }
                catch (Exception ex)
                {
                    Logger.Error("ScheduledFunction.Start", ex.ToString());
                    break;
                }
                var elapsed = DateTime.UtcNow - started;
                var delay = Interval - elapsed;
                if (delay < TimeSpan.Zero) delay = TimeSpan.Zero;
                try
                {
                    await Task.Delay(delay, token);
                }
                catch (TaskCanceledException ex)
                {
                    Logger.Debug("ScheduledFunction.Start", "Scheduler canceled");
                    break;
                }
            }
        }, cancellationTokenSource.Token);
    }

    public void Stop()
    {
        try
        {
            if (cancellationTokenSource == null) return;
            cancellationTokenSource.Cancel();
            loopTask?.Wait();
            cancellationTokenSource.Dispose();
            cancellationTokenSource = null;
            loopTask = null;
        }
        catch (Exception ex)
        {
            Logger.Error("ScheduledFunction.Stop", ex.ToString());
        }
    }

    public void Dispose()
    {
        Stop();
    }
}

