using System.IO;
using System.Windows;
using Client.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Client.Models;

public class Profile : ViewModelBase
{
	private string name { get; set; } = string.Empty;
	private bool isRenaming { get; set; }
	public string OldName { get; set; } = string.Empty;

	public string Name
	{
		get => name;
		set
		{
			name = value;
			OnPropertyChanged();
		}
	}

	public bool IsRenaming
	{
		get => isRenaming;
		set
		{
			isRenaming = value;
			OnPropertyChanged();
		}
	}

	private static string GenerateUniqueHash()
	{
		return Guid.NewGuid().ToString();
	}

	public static JObject? GetProfileByName(string name)
	{
		var files = Directory.GetFiles(LoadFile.LoadFolder("Profiles"), "*.json");
		return files.AsParallel().Select(file => JObject.Parse(File.ReadAllText(LoadFile.Load("Profiles", file))))
			.FirstOrDefault(profile => profile["Name"]?.ToString() == name);
	}

	public static async Task New(string name, Window window)
	{
		var profile = new JObject
		{
			{ "Name", name },
			{
				"Window", new JObject
				{
					{ "Fullscreen", window.WindowState == WindowState.Maximized },
					{
						"Size", new JObject
						{
							{ "Width", window.Width },
							{ "Height", window.Height }
						}
					},
					{
						"Location", new JObject
						{
							{ "Left", window.Left },
							{ "Top", window.Top }
						}
					}
				}
			}
		};
		var serialized = JsonConvert.SerializeObject(profile, Formatting.Indented);
		await Task.Run(() => File.WriteAllText(LoadFile.Load("Profiles", $"{GenerateUniqueHash()}.json"), serialized));
		Logger.Debug("Profile.New", $"Profile: \"{name}\" Successfully Created");
	}

	public static async Task Save(string name, Window window)
	{
		try
		{
			string[] files = Directory.GetFiles(LoadFile.LoadFolder("Profiles"));
			foreach (var file in files)
				if (file.Contains(".json"))
				{
					var profile = JObject.Parse(File.ReadAllText(LoadFile.Load("Profiles", file)));
					var profileName = profile["Name"]?.ToString() ?? string.Empty;
					if (profileName != string.Empty && profileName == name)
					{
						profile["Name"] = name;
						profile["Window"]["Fullscreen"] = window.WindowState == WindowState.Maximized;
						profile["Window"]["Size"]["Width"] = window.Width;
						profile["Window"]["Size"]["Height"] = window.Height;
						profile["Window"]["Location"]["Left"] = window.Left;
						profile["Window"]["Location"]["Top"] = window.Top;
						var serialized = JsonConvert.SerializeObject(profile, Formatting.Indented);
						await Task.Run(() =>
							File.WriteAllText(LoadFile.Load("Profiles", $"{Path.GetFileName(file)}"), serialized));
						Logger.Debug("Profile.Load", $"Profile: \"{name}\" Successfully Saved");
						return;
					}
				}
		}
		catch (Exception ex)
		{
			Logger.Error("Profile.Save", ex.ToString());
		}
	}

	public static async Task SaveAs(string name, Window window)
	{
		var profile = new JObject
		{
			{ "Name", name },
			{
				"Window", new JObject
				{
					{ "Fullscreen", window.WindowState == WindowState.Maximized },
					{
						"Size", new JObject
						{
							{ "Width", window.Width },
							{ "Height", window.Height }
						}
					},
					{
						"Location", new JObject
						{
							{ "Left", window.Left },
							{ "Top", window.Top }
						}
					}
				}
			}
		};
		var serialized = JsonConvert.SerializeObject(profile, Formatting.Indented);
		await Task.Run(() => File.WriteAllText(LoadFile.Load("Profiles", $"{GenerateUniqueHash()}.json"), serialized));
		Logger.Debug("Profile.SaveAs", $"Profile: \"{name}\" Successfully Created");
	}

	public static async Task Load(string name, Window window)
	{
		try
		{
			string[] files = Directory.GetFiles(LoadFile.LoadFolder("Profiles"));
			foreach (var file in files)
				if (file.Contains(".json"))
				{
					var profile = JObject.Parse(File.ReadAllText(LoadFile.Load("Profiles", file)));
					var profileName = profile["Name"]?.ToString() ?? string.Empty;
					if (profileName != string.Empty && profileName == name)
					{
						var fullscreen = false;
						var fullscreenToken = profile["Window"]?["Fullscreen"];
						if (fullscreenToken != null && fullscreenToken.Type == JTokenType.Boolean)
						{
							fullscreen = fullscreenToken.ToObject<bool>();
							if (fullscreen)
								window.WindowState = WindowState.Maximized;
							else
								window.WindowState = WindowState.Normal;
						}

						window.Width = (double)(profile["Window"]?["Size"]?["Width"] ?? 1280.0);
						window.Height = (double)(profile["Window"]?["Size"]?["Height"] ?? 720.0);
						window.Left = (double)(profile["Window"]?["Location"]?["Left"] ?? 50.0);
						window.Top = (double)(profile["Window"]?["Location"]?["Top"] ?? 50.0);
						Logger.Debug("Profile.Load", $"Profile: \"{name}\" Successfully Loaded");
						await Task.Delay(10);
						return;
					}
				}
				else
				{
					Logger.Error("LoadProfileWindow.LoadProfiles", $"{file} is not a valid config file!");
				}
		}
		catch (Exception ex)
		{
			Logger.Error("Profile.Load", ex.ToString());
		}
	}

	public async Task RenameAsync()
	{
		try
		{
			string[] files = Directory.GetFiles(LoadFile.LoadFolder("Profiles"), "*.json");
			foreach (var file in files)
			{
				var profile = JObject.Parse(File.ReadAllText(LoadFile.Load("Profiles", file)));
				var profileName = profile["Name"]?.ToString() ?? string.Empty;
				if (profileName != string.Empty && profileName == OldName)
				{
					profile["Name"] = Name;
					var serialized = JsonConvert.SerializeObject(profile, Formatting.Indented);
					await Task.Run(() =>
						File.WriteAllText(LoadFile.Load("Profiles", $"{Path.GetFileName(file)}"), serialized));
					Logger.Debug("Profile.Load", $"Profile: \"{OldName}\" Successfully Renamed To: \"{Name}\"");
					break;
				}
			}
		}
		catch (Exception ex)
		{
			Logger.Error("Profile.RenameAsync", ex.ToString());
		}
	}

	public async Task CopyAsync()
	{
		try
		{
			string[] files = Directory.GetFiles(LoadFile.LoadFolder("Profiles"), "*.json");
			foreach (var file in files)
			{
				var profile = JObject.Parse(File.ReadAllText(LoadFile.Load("Profiles", file)));
				var profileName = profile["Name"]?.ToString() ?? string.Empty;
				if (profileName != string.Empty && profileName == Name)
				{
					profile["Name"] = $"{Name} - Copy";
					var serialized = JsonConvert.SerializeObject(profile, Formatting.Indented);
					await Task.Run(() =>
						File.WriteAllText(LoadFile.Load("Profiles", $"{GenerateUniqueHash()}.json"), serialized));
					Logger.Debug("Profile.Load", $"Profile: \"{name}\" Successfully Copied");
					break;
				}
			}
		}
		catch (Exception ex)
		{
			Logger.Error("Profile.Copy", ex.ToString());
		}
	}

	public static async Task Import(string file)
	{
		var profile = JObject.Parse(File.ReadAllText(file));
		var profileName = profile["Name"]?.ToString() ?? string.Empty;
		var serialized = JsonConvert.SerializeObject(profile, Formatting.Indented);
		await Task.Run(() => File.WriteAllText(LoadFile.Load("Profiles", $"{GenerateUniqueHash()}.json"), serialized));
		Logger.Debug("Profile.Import", $"Profile: \"{profileName}\" Successfully Imported");
	}

	public void Export(string path)
	{
		var profile = GetProfileByName(Name);
		if (profile == null) return;
		var serialized = JsonConvert.SerializeObject(profile, Formatting.Indented);
		File.WriteAllText(path, serialized);
		Logger.Debug("Profile.Export", $"Profile: \"{Name}\" Successfully Exported To: {path}");
	}

	public async Task DeleteAsync()
	{
		try
		{
			string[] files = Directory.GetFiles(LoadFile.LoadFolder("Profiles"), "*.json");
			foreach (var file in files)
				try
				{
					var fileContent = await Task.Run(() => File.ReadAllText(file));
					var jObject = JObject.Parse(fileContent);
					var profileName = jObject["Name"]?.ToString() ?? string.Empty;
					if (profileName == Name)
					{
						await Task.Run(() => File.Delete(file));
						return;
					}
				}
				catch (Exception ex)
				{
					Logger.Error("LoadProfileViewModel.DeleteAsync", $"Error processing file {file}: {ex.Message}");
				}
		}
		catch (Exception ex)
		{
			Logger.Error("Profile.DeleteAsync", ex.ToString());
		}
	}
}