using Client.Renderables;
using Client.Renderables.Interfaces;
using Common.Utils;
using SkiaSharp;
using Size = System.Drawing.Size;
namespace Client.Engines;

public class RenderEngine
{
    public IEnumerable<IRenderable> IRenderables { get; set; }

    private SKCanvas canvas;
    private Size size;
    private double scale;
    private SKPoint panOffset;

    public SKCanvas Canvas
    {
        get => canvas;
        set
        {
            canvas = value;
        }
    }

    public Size Size
    {
        get => size;
        set
        {
            size = value;
        }
    }

    public double Scale
    {
        get => scale;
        set
        {
            scale = value;
        }
    }
    public SKPoint PanOffset
    {
        get => panOffset;
        set
        {
            panOffset = value;
        }
    }

    public RenderEngine()
    {
        IRenderables = new List<IRenderable>();
    }

    public void UpdateRenderables(List<IRenderable> renderables)
    {
        IRenderables = renderables.OrderBy(r => r.ZIndex).ToList();
    }

    public void Render()
    {
        foreach (IRenderable renderable in IRenderables)
        {
            if (renderable.GetType() == typeof(Line)) RenderLine((Line)renderable);
            else if (renderable.GetType() == typeof(Text)) RenderText((Text)renderable);
            else if (renderable.GetType() == typeof(Circle)) RenderCircle((Circle)renderable);
            else if (renderable.GetType() == typeof(Rect)) RenderRect((Rect)renderable);
            else if (renderable.GetType() == typeof(Arc)) RenderArc((Arc)renderable);
        }
    }

    public void RenderLine(Line line)
    {
        Canvas.DrawLine(line.Start, line.End, line.Paint);
    }

    public void RenderText(Text text)
    {
        Canvas.DrawText(text.Content, text.Point.X, text.Point.Y, text.Paint);
    }

    public void RenderCircle(Circle circle)
    {
        try
        {
            Canvas.DrawCircle(circle.Center, circle.Radius, circle.Paint);
        }
        catch( Exception ex)
        {
            Message.Error(ex.ToString());
        }
    }

    private void RenderRect(Rect rect)
    {
        Canvas.DrawRect(rect.skRect, rect.Paint);
    }

    private void RenderArc(Arc arc)
    {
        float r = arc.Radius;
        float heightFactor = 5f;          // how tall / skinny
        float height = heightFactor * r;

        float baseY = arc.Center.Y;       // arc should end here (where the line was)

        var rect = new SKRect(
            arc.Center.X - r,
            baseY - height/2,               // top
            arc.Center.X + r,
            baseY + height/2                      // bottom
        );

        arc.Paint.Style = SKPaintStyle.Stroke;
        arc.Paint.StrokeWidth = 1;

        Canvas.DrawArc(rect, 180f, 180f, false, arc.Paint);
    }
}
