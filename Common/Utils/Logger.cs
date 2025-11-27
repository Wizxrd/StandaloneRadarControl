using System.IO;
namespace Common.Utils;

public enum LogLevel
{
    Alert = 0,
    Error = 1,
    Warning = 2,
    Info = 3,
    Debug = 4,
    Trace = 5
}

public static class Logger
{
    private static readonly string LogsDirectory = PathFinder.GetFolderPath("Logs");
    private static readonly string TimestampedLogPath;
    private static readonly string DebugLogPath;
    private static string CurrentLogPath => DebugMode ? DebugLogPath : TimestampedLogPath;

    private static bool debugMode = false;
    public static bool DebugMode
    {
        get => debugMode;
        set
        {
            debugMode = value;
            if (debugMode)
            {
                try
                {
                    File.WriteAllText(DebugLogPath, string.Empty);
                }
                catch (Exception ex)
                {
                    Logger.Error("DebugMode", ex.ToString());
                }
            }
        }
    }

    public static LogLevel LogLevelThreshold { get; set; } = LogLevel.Trace;

    static Logger()
    {
        Directory.CreateDirectory(LogsDirectory);

        TimestampedLogPath = Path.Combine(LogsDirectory, $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log");
        DebugLogPath = Path.Combine(LogsDirectory, "debug.log");

        if (!debugMode)
        {
            CleanupOldLogs();
        }
    }

    private static void CleanupOldLogs()
    {
        var logFiles = new DirectoryInfo(LogsDirectory)
            .GetFiles("*.log")
            .OrderBy(f => f.CreationTimeUtc)
            .ToList();

        while (logFiles.Count >= 10)
        {
            try
            {
                logFiles[0].Delete();
                logFiles.RemoveAt(0);
            }
            catch { break; }
        }
    }

    private static string Center(string text, int width)
    {
        int padding = width - text.Length;
        int padLeft = padding / 2;
        int padRight = padding - padLeft;
        return new string(' ', padLeft) + text + new string(' ', padRight);
    }

    private static void Write(LogLevel level, string function, string message)
    {
        if (level > LogLevelThreshold)
            return;

        try
        {
            using var writer = new StreamWriter(CurrentLogPath, true);
            string levelText = Center(level.ToString().ToUpper(), 7);
            writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {levelText} | vFalcon.{function} | {message}");

        }
        catch { }
    }

    public static void Wipe()
    {
        try { File.WriteAllText(CurrentLogPath, string.Empty); } catch { }
    }

    public static void Alert(string function, string message) => Write(LogLevel.Alert, function, message);
    public static void Error(string function, string message) => Write(LogLevel.Error, function, message);
    public static void Warning(string function, string message) => Write(LogLevel.Warning, function, message);
    public static void Info(string function, string message) => Write(LogLevel.Info, function, message);
    public static void Debug(string function, string message) => Write(LogLevel.Debug, function, message);
    public static void Trace(string function, string message) => Write(LogLevel.Trace, function, message);
}
