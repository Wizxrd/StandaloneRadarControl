using System.Windows.Media;

namespace Common.Models;

public class ChatMessage
{
    public MessageChannel Channel { get; }
    public DateTime Timestamp { get; }
    public string Sender { get; }
    public string Text { get; }
    public Brush Foreground { get; }

    public ChatMessage(MessageChannel channel, string sender, string text, Brush foreground)
    {
        Channel = channel;
        Sender = sender;
        Text = text;
        Timestamp = DateTime.UtcNow;
        Foreground = foreground;
    }

    public string DisplayText => $"[{Timestamp:HH:mm:ss}] {Text}";
}
