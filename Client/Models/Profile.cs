namespace Client.Models;

public class Profile
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime LastUsedAt { get; set; }
    public List<DisplayWindow> DisplayWindows{ get; set; } = new();
}
