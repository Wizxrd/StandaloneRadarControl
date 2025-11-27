using SkiaSharp;
using Client.Renderables.Interfaces;
namespace Client.Renderables;

public class Circle : IRenderable
{
    public SKPoint Center {  get; set; }
    public float Radius { get; set; }
    public SKPaint Paint { get; set; }
    public int ZIndex { get; set; }

    public Circle(SKPoint center, float radius, SKPaint paint, int zIndex)
    {
        Center = center;
        Radius = radius;
        Paint = paint;
        ZIndex = zIndex;
    }

    public void Dispose()
    {
        Paint?.Dispose();
    }
}
