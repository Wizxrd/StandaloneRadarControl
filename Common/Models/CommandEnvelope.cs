using System.Text.Json;

namespace Common.Models;

public sealed class CommandEnvelope
{
    public string Command { get; set; }
    public object Payload { get; set; }
}
