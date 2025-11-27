using System.IO;
namespace Common.Utils;

public class PathFinder
{
    public static string GetAppDirectory()
    {
        try
        {
#if DEBUG
            var root = AppContext.BaseDirectory;
            var solution = Path.GetFullPath(Path.Combine(root, "..", "..", ".."));
            return solution.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
#else
            var exe = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var parent = Directory.GetParent(exe)?.FullName ?? exe;
            return parent.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
#endif
        }
        catch (Exception e)
        {
            Logger.Error("GetAppDirectory", e.ToString());
            return string.Empty;
        }
    }

    public static string GetFilePath(string folder, string file)
    {
        try
        {
            var path = ResolveFolderRoot(folder);
            return Path.Combine(path, file);
        }
        catch (Exception ex)
        {
            Logger.Error("Load", ex.ToString());
            return string.Empty;
        }
    }

    public static string GetFolderPath(string folder)
    {
        try
        {
            var path = ResolveFolderRoot(folder);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            return path;
        }
        catch (Exception ex)
        {
            Logger.Error("Load", ex.ToString());
            return string.Empty;
        }
    }

    private static string ResolveFolderRoot(string folder)
    {
#if DEBUG
        var root = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.FullName
                   ?? Directory.GetCurrentDirectory();
        var solution = Directory.GetParent(root)?.FullName ?? root;
        return Path.Combine(solution, folder);
#else
    var exe = AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    var parent = Directory.GetParent(exe)?.FullName ?? exe;
    return Path.Combine(parent, folder);
#endif
    }
}
