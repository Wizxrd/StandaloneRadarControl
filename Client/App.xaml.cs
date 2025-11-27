using Client.Models;
using Common.Utils;
using Newtonsoft.Json;
using System.Configuration;
using System.Data;
using System.IO;
using System.Threading;
using System.Windows;
namespace Client;

public partial class App : Application
{
    public static string Version { get; } = "0.0.1";
    public static GeneralSettings? GeneralSettings { get; set; }
    public static DisplayState? DisplayState { get; set; }
    public App()
    {
        Logger.DebugMode = true;
        Logger.LogLevelThreshold = LogLevel.Trace;
        Logger.Info("App", $"Launching SRC Client v{Version}");
        string path = Path.Combine(PathFinder.GetAppDirectory(), "GeneralSettings.json");
        string json = File.ReadAllText(path);
        if (json == string.Empty)
        {
            GeneralSettings = new();
            SaveGeneralSettings();
        }
        else
        {
            GeneralSettings = JsonConvert.DeserializeObject<GeneralSettings>(json);
        }

    }

    public static void SaveGeneralSettings()
    {
        string file = Path.Combine(PathFinder.GetAppDirectory(), "GeneralSettings.json");
        string serialized = JsonConvert.SerializeObject(GeneralSettings, Formatting.Indented);
        File.WriteAllText(file, serialized);
    }
}
