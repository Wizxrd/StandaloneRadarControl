namespace Client.Models;

public class ProcessedFeature
{
    public string GeometryType { get; set; }
    public object Geometry { get; set; }
    public Dictionary<string, object> AppliedAttributes { get; set; }
    public string TextContent { get; set; }
    public float FontSize { get; set; }
    public float XOffset { get; set; }
    public float YOffset { get; set; }
    public bool Underline { get; set; }
}
