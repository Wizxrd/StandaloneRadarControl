using Common.Models;
using Newtonsoft.Json;
using Server.Models;
using Server.UI.MainWindow;
using Common.Utils;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Message = Common.Utils.Message;

namespace Server;

public partial class App : Application
{
    public static string Version { get; } = "0.0.1";
    public static Settings? Settings { get; set; }
    public static MainWindowViewModel? MainWindowViewModel { get; set; }

    private static Mutex mutex = new();
    private const string appName = "SRCServer";
    private bool createdNew;

    public App()
    {
        Logger.DebugMode = true;
        Logger.LogLevelThreshold = LogLevel.Trace;
        Logger.Info("App", $"Launching SRC Server v{Version}");
        string path = Path.Combine(PathFinder.GetAppDirectory(), "Settings.json");
        string json = File.ReadAllText(path);
        if (json == string.Empty)
        {
            Settings = new();
            SaveSettings();
        }
        else
        {
            Settings = JsonConvert.DeserializeObject<Settings>(json);
        }

        mutex = new Mutex(true, appName, out createdNew);
    }

    public static void SaveSettings()
    {
        string file = Path.Combine(PathFinder.GetAppDirectory(), "Settings.json");
        string serialized = JsonConvert.SerializeObject(Settings, Formatting.Indented);
        File.WriteAllText(file, serialized);
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        try
        {

            if (!createdNew)
            {
                Message.Warning("Another instance of vFalcon is already running!");
                Shutdown();
                return;
            }
            base.OnStartup(e);
        }
        catch (Exception ex)
        {
            Logger.Error("App.OnStartup", ex.ToString());
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        for (int i = Application.Current.Windows.Count - 1; i >= 0; i--)
        {
            var w = Application.Current.Windows[i];
            if (w == null) continue;
            if (!w.Dispatcher.CheckAccess())
                w.Dispatcher.Invoke(() => { if (w.IsLoaded) w.Close(); });
            else
                if (w.IsLoaded) w.Close();
        }

        mutex?.ReleaseMutex();
        base.OnExit(e);
    }
}
