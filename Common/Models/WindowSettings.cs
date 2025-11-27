namespace Common.Models;

public class WindowSettings
{
    public string Bounds { get; set; }
    public bool IsOpen { get; set; } = true;
    public bool IsMaximized { get; set; } = false;
    public bool ShowTitleBar { get; set; } = true;

    public WindowSettings(int left, int top, int width, int height)
    {
        Bounds = $"{left},{top},{width},{height}";
    }
}
