using AdonisUI.Controls;
using Client.Models;
using Client.Services.Interfaces;
using Common.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
namespace Client.Services;

public class ServerBookmarkService : IServerBookmarkService
{
    public List<ServerBookmark> GetBookmarks()
    {
        List<ServerBookmark> list = new List<ServerBookmark>();
        try
        {
            string path = Path.Combine(PathFinder.GetAppDirectory(), "GeneralSettings.json");
            string json = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(json)) return list;
            GeneralSettings generalSettings = JsonConvert.DeserializeObject<GeneralSettings>(json);
            if (generalSettings != null )
                list = generalSettings.ServerBookmarks;
        }
        catch (Exception ex)
        {
            Logger.Error("ServerBookmarkService.GetProfiles", ex.ToString());
        }
        return list;
    }

    public async Task New(string address, string port, string password, string callsign)
    {
        string path = Path.Combine(PathFinder.GetAppDirectory(), "GeneralSettings.json");
        string json = File.ReadAllText(path);
        GeneralSettings generalSettings = JsonConvert.DeserializeObject<GeneralSettings>(json);
        ServerBookmark bookmark = new()
        {
            Id = UniqueHash.Generate(),
            Name = "New Bookmark",
            LastUsedAt = DateTime.UtcNow,
            Address = address,
            Port = port,
            Password = password,
            Callsign = callsign
        };
        generalSettings.ServerBookmarks.Add(bookmark);
        string serialized = JsonConvert.SerializeObject(generalSettings, Formatting.Indented);
        await File.WriteAllTextAsync(path, serialized);
    }

    public async Task Rename(string oldName, string newName)
    {
        try
        {
            string path = Path.Combine(PathFinder.GetAppDirectory(), "GeneralSettings.json");
            string json = File.ReadAllText(path);
            GeneralSettings generalSettings = JsonConvert.DeserializeObject<GeneralSettings>(json);
            foreach (ServerBookmark bookmark in generalSettings.ServerBookmarks)
            {
                if (bookmark.Name == oldName)
                {
                    bookmark.Name = newName;
                    string serialized = JsonConvert.SerializeObject(generalSettings, Formatting.Indented);
                    await File.WriteAllTextAsync(path, serialized);
                    Logger.Info("ServerBookmarkService.Rename", $"Renamed \"{oldName}\" to \"{newName}\"");
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error("ServerBookmarkService.Rename", ex.ToString());
        }
    }

    public async Task Copy(ServerBookmark bookmark)
    {
        try
        {
            ServerBookmark copy = bookmark;
            copy.Id = UniqueHash.Generate();
            copy.Name = $"{bookmark.Name} - Copy";


            string path = Path.Combine(PathFinder.GetAppDirectory(), "GeneralSettings.json");
            string json = File.ReadAllText(path);
            GeneralSettings generalSettings = JsonConvert.DeserializeObject<GeneralSettings>(json);
            generalSettings.ServerBookmarks.Add(copy);

            string serialized = JsonConvert.SerializeObject(generalSettings, Formatting.Indented);
            await Task.Run(() => File.WriteAllTextAsync(path, serialized));
            Logger.Debug("ProfileService.Rename", $"Copied profile: \"{bookmark.Name}\" as: \"{copy.Name}\"");
        }
        catch (Exception ex)
        {
            Logger.Error("ProfileService.Copy", ex.ToString());
        }
    }

    public async Task Delete(ServerBookmark bookmark)
    {
        string path = Path.Combine(PathFinder.GetAppDirectory(), "GeneralSettings.json");
        string json = File.ReadAllText(path);
        GeneralSettings generalSettings = JsonConvert.DeserializeObject<GeneralSettings>(json);
        foreach (ServerBookmark bm in generalSettings.ServerBookmarks)
        {
            try
            {
                if (bookmark.Name == bm.Name)
                {
                    generalSettings.ServerBookmarks.Remove(bm);
                    string serialized = JsonConvert.SerializeObject(generalSettings, Formatting.Indented);
                    await File.WriteAllTextAsync(path, serialized);
                    Logger.Info("ServerBookmarkService.Delete", $"Deleted bookmark: \"{bookmark.Name}\"");
                    return;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("ServerBookmarkService.Delete", $"Error with {bm}: {ex.Message}");
            }
        }
    }

    public async Task Save(ServerBookmark bookmark)
    {
        string path = Path.Combine(PathFinder.GetAppDirectory(), "GeneralSettings.json");
        string json = File.ReadAllText(path);
        GeneralSettings generalSettings = JsonConvert.DeserializeObject<GeneralSettings>(json);
        foreach (ServerBookmark bm in generalSettings.ServerBookmarks)
        {
            try
            {
                if (bookmark.Name == bm.Name)
                {
                    bm.Address = bookmark.Address;
                    bm.Port = bookmark.Port;
                    bm.Password = bookmark.Password;
                    bm.Callsign = bookmark.Callsign;
                    bm.Coalition = bookmark.Coalition;
                    bm.LastUsedAt = DateTime.UtcNow;
                    string serialized = JsonConvert.SerializeObject(generalSettings, Formatting.Indented);
                    await File.WriteAllTextAsync(path, serialized);
                    Logger.Info("ServerBookmarkService.Save", $"Saved bookmark: \"{bookmark.Name}\"");
                    return;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("ServerBookmarkService.Save", $"Error with {bm}: {ex.Message}");
            }
        }
    }
}
