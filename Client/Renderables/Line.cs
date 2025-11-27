using SkiaSharp;
using Client.Renderables.Interfaces;
namespace Client.Renderables;

public class Line : IRenderable
{
    public SKPoint Start { get; set; }
    public SKPoint End { get; set; }
    public SKPaint Paint { get; set; }
    public int ZIndex { get; set; }

    public Line(SKPoint start, SKPoint end, SKPaint paint, int zIndex)
    {
        Start = start;
        End = end;
        Paint = paint;
        ZIndex = zIndex;
    }

    public void Dispose()
    {
        Paint?.Dispose();
    }
}
