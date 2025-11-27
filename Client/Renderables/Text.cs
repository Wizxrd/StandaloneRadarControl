using SkiaSharp;
using Client.Renderables.Interfaces;
namespace Client.Renderables;

public class Text : IRenderable
{
    public string Content { get; set; }
    public SKPoint Point { get; set; }
    public SKPaint Paint { get; set; }
    public int ZIndex { get; set; }

    public Text(string text, SKPoint point, SKPaint paint, int zIndex)
    {
        Content = text;
        Point = point;
        Paint = paint;
        ZIndex = zIndex;
    }

    public void Dispose()
    {
        Paint?.Dispose();
    }
}
