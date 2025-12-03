using Client.Renderables.Interfaces;
using SkiaSharp;

namespace Client.Renderables;

public class Arc : IRenderable
{
    public SKPoint Center { get; set; }
    public float Radius { get; set; }
    public float StartAngle { get; set; }
    public float SweepAngle { get; set; }
    public bool UseCenter { get; set; } = false;
    public SKPaint Paint { get; set; }
    public int ZIndex { get; set; }

    public Arc(SKPoint center, float radius, float startAngle, float sweepAngle, SKPaint paint, int zIndex)
    {
        Center = center;
        Radius = radius;
        StartAngle = startAngle;
        SweepAngle = sweepAngle;
        Paint = paint;
        ZIndex = zIndex;
    }

    public void Dispose()
    {
        Paint?.Dispose();
    }
}
