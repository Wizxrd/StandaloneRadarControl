using Common.Models;
namespace Client.Models;

public class DisplayWindow
{
    public string Id { get; set; } = string.Empty;
    public WindowSettings WindowSettings { get; set; } = new(0,0,1300,900);
    public DisplaySettings DisplaySettings { get; set; } = new();
}
