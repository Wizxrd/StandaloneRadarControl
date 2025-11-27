using SkiaSharp;
using Client.Renderables;
using Client.Renderables.Interfaces;
using Size = System.Drawing.Size;
namespace vFalcon.Engines;

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
        Canvas.DrawCircle(circle.Center, circle.Radius, circle.Paint);
    }

    private void RenderRect(Rect rect)
    {
        Canvas.DrawRect(rect.skRect, rect.Paint);
    }
}
