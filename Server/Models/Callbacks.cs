using Newtonsoft.Json.Linq;
namespace Server.Models;

public static class Callbacks
{
    public static JObject ExportAllAirplanes = new()
    {
        ["callback"] = "onExportAllAirplanes"
    };
}
