using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using Client.Models;
using Client.Services.Interfaces;
using Common.Utils;
namespace vFalcon.Services;

public class ProfileService : IProfileService
{
    public async Task<List<Profile>> GetProfiles()
    {
        List<Profile> list = new List<Profile>();
        try
        {
            string profilesPath = PathFinder.GetFolderPath("Profiles");
            var files = Directory.GetFiles(profilesPath, "*.json");

            foreach (var file in files)
            {
                var json = await File.ReadAllTextAsync(file);
                var profile = JsonConvert.DeserializeObject<Profile>(json);
                if (profile == null) continue;
                list.Add(profile);
            }
        }
        catch (Exception ex)
        {
            Logger.Error("ProfileService.GetProfiles", ex.ToString());
        }
        return list;
    }

    public async Task New(string displayType, string position, string name)
    {
        Profile profile = new()
        {
            Id = UniqueHash.Generate(),
            Name = name,
            LastUsedAt = DateTime.UtcNow,
        };
        DisplayWindow displayWindow = new()
        {
            Id = UniqueHash.Generate(),
        };
        displayWindow.DisplaySettings = new()
        {
            Type = displayType,
            Position = position
        };
        profile.DisplayWindows.Add(displayWindow);
        string filePath = Path.Combine(PathFinder.GetFolderPath("Profiles"), $"{profile.Id}.json");
        string serialized = JsonConvert.SerializeObject(profile, Formatting.Indented);
        await File.WriteAllTextAsync(filePath, serialized);
    }

    public async Task<bool> Import()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = "Import Profile",
            Filter = "SRC Profile (*.srcprofile|*.srcprofile",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        };
        if (dialog.ShowDialog() == true)
        {
            try
            {
                string json = File.ReadAllText(dialog.FileName);
                Profile? imported = JsonConvert.DeserializeObject<Profile>(json);

                if (imported != null)
                {
                    imported.Id = UniqueHash.Generate();
                    string filePath = Path.Combine(PathFinder.GetFolderPath("Profiles"), $"{imported.Id}.json");
                    string serialized = JsonConvert.SerializeObject(imported, Formatting.Indented);
                    await File.WriteAllTextAsync(filePath, serialized);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("ProfileService.Import", ex.ToString());
            }
        }
        return false;
    }

    public async Task Rename(string oldName, string newName)
    {
        try
        {

            string profilesPath = PathFinder.GetFolderPath("Profiles");
            var files = Directory.GetFiles(profilesPath, "*.json");
            foreach (string file in files)
            {
                JObject profile = JObject.Parse(File.ReadAllText(file));
                if ((string?)profile["Name"] == oldName)
                {
                    profile["Name"] = newName;
                    string serialized = JsonConvert.SerializeObject(profile, Formatting.Indented);
                    await Task.Run(() => File.WriteAllText(file, serialized));
                    Logger.Info("ProfileService.Rename", $"Renamed \"{oldName}\" to \"{newName}\"");
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error("ProfileService.Rename", ex.ToString());
        }
    }

    public async Task Copy(Profile profile)
    {
        try
        {
            Profile copy = profile;
            copy.Id = UniqueHash.Generate();
            copy.Name = $"{profile.Name} - Copy";
            string serialized = JsonConvert.SerializeObject(copy, Formatting.Indented);
            await Task.Run(() => File.WriteAllText(PathFinder.GetFilePath("Profiles", $"{copy.Id}.json"), serialized));
            Logger.Debug("ProfileService.Rename", $"Copied profile: \"{profile.Name}\" as: \"{copy.Name}\"");
        }
        catch (Exception ex)
        {
            Logger.Error("ProfileService.Copy", ex.ToString());
        }
    }

    public void Export(Profile profile)
    {
        try
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = "Export Profile",
                Filter = "SRC Profile (*.srcprofile)|*.srcprofile",
                DefaultExt = ".srcprofile",
                AddExtension = true,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            if (dialog.ShowDialog() == true)
            {
                var fileName = dialog.FileName;
                if (!fileName.EndsWith(".srcprofile", StringComparison.OrdinalIgnoreCase))
                    fileName += ".srcprofile";
                string serialized = JsonConvert.SerializeObject(profile, Formatting.Indented);
                File.WriteAllText(fileName, serialized);
                Logger.Info("ProfileService.Export", $"Exported profile: \"{profile.Name}\" to: \"{dialog.FileName}\"");
            }
        }
        catch (Exception ex)
        {
            Logger.Error("ProfileService.Export", ex.ToString());
        }
    }

    public async Task Delete(Profile profile)
    {
        string profilesPath = PathFinder.GetFolderPath("Profiles");
        string[] files = Directory.GetFiles(profilesPath, "*.json");
        foreach (string file in files)
        {
            try
            {
                string content = await File.ReadAllTextAsync(file);
                JObject jObj = JObject.Parse(content);
                if ((string?)jObj["Name"] == profile.Name)
                {
                    await Task.Run(() => File.Delete(file));
                    Logger.Info("ProfileService.Delete", $"Deleted profile: \"{profile.Name}\"");
                    return;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("ProfileService.Delete", $"Error with {file}: {ex.Message}");
            }
        }
    }

    public void Save(Profile profile)
    {
        try
        {
            string filename = $"{profile.Id}.json";
            string path = PathFinder.GetFilePath("Profiles", filename);
            string serialized = JsonConvert.SerializeObject(profile, Formatting.Indented);
            File.WriteAllText(path, serialized);
            Logger.Info("ProfileService.Save", $"Saved profile: \"{profile.Name}\"");
        }
        catch (Exception ex)
        {
            Logger.Error("ProfileService.Save", ex.ToString());
        }
    }

    public async Task SaveAsync(Profile profile)
    {
        try
        {
            string filename = $"{profile.Id}.json";
            string path = PathFinder.GetFilePath("Profiles", filename);
            string serialized = JsonConvert.SerializeObject(profile, Formatting.Indented);
            await File.WriteAllTextAsync(path, serialized);
            Logger.Debug("ProfileService.Save", $"Saved profile: \"{profile.Name}\"");
        }
        catch (Exception ex)
        {
            Logger.Error("ProfileService.Save", ex.ToString());
        }
    }

    public async Task SaveAs(Profile profile, string name)
    {
        string[] files = Directory.GetFiles(PathFinder.GetFolderPath("Profiles"), "*.json");
        foreach (string file in files)
        {
            try
            {
                profile.Id = UniqueHash.Generate();
                profile.Name = name;
                string serialized = JsonConvert.SerializeObject(profile, Formatting.Indented);
                await Task.Run(() => File.WriteAllText(PathFinder.GetFilePath("Profiles", $"{profile.Id}.json"), serialized));
                Logger.Debug("ProfileService.SaveAs", $"Saved profile as: \"{profile.Name}\"");
                return;
            }
            catch (Exception ex)
            {
                Logger.Error("ProfileService.SaveAs", $"Error with {file}: {ex.Message}");
            }
        }
    }
}
