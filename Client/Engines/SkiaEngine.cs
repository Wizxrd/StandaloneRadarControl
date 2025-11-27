using Client.Renderables.Interfaces;
using Client.UI.Controls.Display;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.WPF;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
namespace vFalcon.Engines;

public class SkiaEngine
{

    private Border DisplayElementBorder;
    private SKElement SkiaElement;
    private Cursor Cursor { get; set; } = Cursors.Arrow;
    public RenderEngine RenderEngine { get; set; }
    public List<IRenderable> Renderables { get; set; }

    public Action<double, double>? SizeChanged;
    public Action<object, SKPoint, MouseButton>? MouseDown;
    public Action<object, SKPoint, MouseButton>? MouseUp;
    public Action<object, SKPoint>? MouseMove;
    public Action<object, SKPoint, int>? MouseWheel;
    public Action<SKPaintSurfaceEventArgs>? PaintSurface;
    public Action<Key>? KeyDown;
    public Action<Key>? KeyUp;

    public int BackgroundValue { get; set; }
    public int BacklightValue { get; set; }

    public SkiaEngine(RenderView renderView)
    {
        DisplayElementBorder = renderView.DisplayElementBorder;
        SkiaElement = renderView.DisplayElement;
        SkiaElement.SizeChanged += OnSizeChanged;
        SkiaElement.MouseDown += OnMouseDown;
        SkiaElement.MouseUp += OnMouseUp;
        SkiaElement.MouseMove += OnMouseMove;
        SkiaElement.MouseWheel += OnMouseWheel;
        SkiaElement.PaintSurface += OnPaintSurface;
        SkiaElement.Unloaded += (_, __) => OnUnloaded();

        RenderEngine = new();
        Renderables = new();
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        SizeChanged?.Invoke(e.NewSize.Width, e.NewSize.Height);
    }

    private TResult Invoke<TResult>(Func<TResult> callback)
    {
        return SkiaElement.Dispatcher.Invoke(callback);
    }

    private void Invoke(Action callback)
    {
        SkiaElement.Dispatcher.Invoke(callback);
    }

    public void RequestRender()
    {
        Invoke(SkiaElement.InvalidateVisual);
    }

    public void SetBackgroundColor(SKColor color)
    {
        DisplayElementBorder.Background = new SolidColorBrush(Color.FromArgb(color.Alpha, color.Red, color.Green, color.Blue));
    }

    public void ScaleBackgroundByBacklight()
    {
        double factor = 0.65 + 0.35 * (BacklightValue / 100.0);
        double baseBlue = ((BackgroundValue / 60.0) * 127) * factor;
        byte blue = (byte)Math.Max(Math.Min(baseBlue, 255), 0);
        DisplayElementBorder.Background = new SolidColorBrush(Color.FromArgb(255, 0, 0, blue));
    }

    public static SKColor ScaleColor(SKColor color, double percent)
    {
        var f = Math.Max(0.0, Math.Min(1.0, percent * 0.01));
        byte r = (byte)Math.Round(color.Red * f);
        byte g = (byte)Math.Round(color.Green * f);
        byte b = (byte)Math.Round(color.Blue * f);
        return new SKColor(r, g, b, color.Alpha);
        //return new SKColor(color.Red, color.Green, color.Blue, (byte)(percent*(255/100))); 
    }

    public SKPoint GetCursorLocation()
    {
        Point mousePosition = Mouse.GetPosition(SkiaElement);
        if (SkiaElement.IgnorePixelScaling) return new SKPoint((float)mousePosition.X, (float)mousePosition.Y);
        var dpi = VisualTreeHelper.GetDpi(SkiaElement);
        return new SKPoint((float)(mousePosition.X * dpi.DpiScaleX), (float)(mousePosition.Y * dpi.DpiScaleY));
    }

    public void StartPan()
    {
        HideCursor();
        CaptureMouse();
    }

    public void StopPan()
    {
        ShowCursor();
        ReleaseMouse();
    }

    public void SetCursor(Cursor cursor) => SkiaElement.Cursor = cursor;

    public void ShowCursor() => SkiaElement.Cursor = Cursor;

    public void HideCursor() => SkiaElement.Cursor = Cursors.None;

    public void CaptureMouse() => Mouse.Capture(SkiaElement);
    public void ReleaseMouse() => Mouse.Capture(null);

    public static float MeasureText(SKPaint paint, string text)
    {
        return paint.MeasureText(text);
    }

    public static SKRect GetTextBounds(SKFont font, string text)
    {
        float advance = font.MeasureText(text, out SKRect bounds);
        return bounds;
    }

    public static float MeasureTextWidth(SKFont font, string text)
    {
        SKRect bounds = GetTextBounds(font, text);
        return bounds.Width;
    }

    public static float MeasureTextHeight(SKFont font, string text)
    {
        SKRect bounds = GetTextBounds(font, text);
        return bounds.Height;
    }

    public static float GetTextHeight(SKPaint paint)
    {
        SKFont t = new();
        SKFontMetrics metrics = paint.FontMetrics;
        return metrics.Bottom - metrics.Top + metrics.Leading;
    }

    private void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        Point position = e.GetPosition((IInputElement)sender);
        SKPoint location = new SKPoint((float)position.X, (float)position.Y);
        MouseButton button = e.ChangedButton;
        MouseDown?.Invoke(sender, location, button);
    }

    private void OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        var position = e.GetPosition((IInputElement)sender);
        SKPoint location = new SKPoint((float)position.X, (float)position.Y);
        MouseButton button = e.ChangedButton;
        MouseUp?.Invoke(sender, location, button);
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        var position = e.GetPosition((IInputElement)sender);
        SKPoint location = new SKPoint((float)position.X, (float)position.Y);
        MouseMove?.Invoke(sender, location);
    }

    private void OnMouseWheel(object sender, MouseWheelEventArgs e)
    {
        int delta = e.Delta > 0 ? -1 : 1;
        var position = e.GetPosition((IInputElement)sender);
        SKPoint location = new SKPoint((float)position.X, (float)position.Y);
        MouseWheel?.Invoke(sender, location, delta);
    }

    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        try
        {
            PaintSurface?.Invoke(e);
        }
        catch (AccessViolationException ex)
        {
            System.Diagnostics.Debug.WriteLine($"PaintSurface error: {ex.Message}");
        }
    }

    public void OnUnloaded()
    {
        SkiaElement.MouseDown -= OnMouseDown;
        SkiaElement.MouseUp -= OnMouseUp;
        SkiaElement.MouseMove -= OnMouseMove;
        SkiaElement.PreviewMouseWheel -= OnMouseWheel;
        SkiaElement.PaintSurface -= OnPaintSurface;
    }
}
