using Client.Engines;
using Client.Managers;
using Client.Models;
using Client.Renderables;
using Client.Renderables.Interfaces;
using Client.UI.Controls.RenderDisplay;
using Client.Utils;
using Common.Mvvm;
using Microsoft.VisualBasic.Logging;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media.TextFormatting;
using Size = System.Drawing.Size;
namespace Client.UI.Displays.Tactical;

public class TacticalViewModel : ViewModelBase
{
    private RenderDisplayView renderDisplayView { get; set; }

    public SkiaEngine SkiaEngine { get; set; }

    public DisplayState DisplayState { get; set; } = new();
    public SKPoint MousePosition { get; set; }
    public List<double> ZoomLevels { get; set; } = new();
    public List<double> ScaleMap { get; set; } = new();

    public string DisplayStatus
    {
        get => DisplayState.Status;
        set
        {
            DisplayState.Status = value;
            OnPropertyChanged();
        }
    }

    public ICommand OpenConnectCommand { get; set; }
    public ICommand OpenMessagesCommand { get; set; }

    public TacticalViewModel(RenderDisplayView renderDisplayView)
    {
        DisplayStatus = "NOT RECEIVING SURVEILLANCE DATA";
        App.DisplayState = DisplayState;

        this.renderDisplayView = renderDisplayView;
        SkiaEngine = renderDisplayView.SkiaEngine;

        SkiaEngine.SizeChanged += OnSizeChanged;
        SkiaEngine.MouseDown += OnMouseDown;
        SkiaEngine.MouseUp += OnMouseUp;
        SkiaEngine.MouseMove += OnMouseMove;
        SkiaEngine.MouseWheel += OnMouseWheel;
        SkiaEngine.PaintSurface += PaintSurface;

        OpenConnectCommand = new RelayCommand(OnOpenConnectCommand);
        OpenMessagesCommand = new RelayCommand(OnOpenMessagesCommand);
    }

    public void SetDisplayState()
    {
        DisplayState.Center.Lat = 43.39712671678968;
        DisplayState.Center.Lon = 40.48118667054575;
        ZoomLevels = Zoom.BuildLevels();
        ScaleMap = Zoom.BuildScale(DisplayState, ZoomLevels);
        DisplayState.Scale = ScaleMap[50];
        DisplayState.PanOffset = CenterAtCoordinates(DisplayState.Width, DisplayState.Height, DisplayState.Scale, DisplayState.PanOffset, DisplayState.Center);
    }

    public SKPoint CenterAtCoordinates(int width, int height, double scale, SKPoint panOffset, Coordinate coordinate)
    {
        var screenPoint = ScreenMap.CoordinateToScreen(width, height, scale, panOffset, coordinate);
        float shiftX = (width / 2f) - screenPoint.X;
        float shiftY = (height / 2f) - screenPoint.Y;
        panOffset.X += shiftX;
        panOffset.Y += shiftY;
        return panOffset;
    }

    private void OnSizeChanged(double width, double height)
    {

        SkiaEngine.BackgroundValue = 6;
        SkiaEngine.BacklightValue = 100;
        SkiaEngine.ScaleBackgroundByBacklight();
        SkiaEngine.RequestRender();

    }

    private void OnMouseDown(object sender, SKPoint point, MouseButton button)
    {
        if (!DisplayState.IsReady) return;
        if (button == MouseButton.Right)
        {
            if (MousePosition == point) return;
            MousePosition = point;
            DisplayState.IsPanning = true;
            SkiaEngine.StartPan();
        }
    }

    private void OnMouseMove(object sender, SKPoint point)
    {
        if (!DisplayState.IsReady) return;
        if (!DisplayState.IsPanning) return;
        SKPoint delta = point - (SKPoint)MousePosition;
        DisplayState.PanOffset = new SKPoint(
            DisplayState.PanOffset.X + delta.X,
            DisplayState.PanOffset.Y + delta.Y
        );
        MousePosition = point;
        SkiaEngine.RequestRender();
    }

    private void OnMouseUp(object sender, SKPoint point, MouseButton button)
    {
        if (!DisplayState.IsReady) return;
        if (button == MouseButton.Right)
        {
            Coordinate newCenter = ScreenMap.ScreenToCoordinate(new System.Drawing.Size(DisplayState.Width, DisplayState.Height), DisplayState.Scale, DisplayState.PanOffset, new SKPoint(DisplayState.Width / 2f, DisplayState.Height / 2f));
            DisplayState.Center.Lat = newCenter.Lat;
            DisplayState.Center.Lon = newCenter.Lon;
            MousePosition = new SKPoint();
            DisplayState.IsPanning = false;
            SkiaEngine.StopPan();
        }
    }

    private void OnMouseWheel(object sender, SKPoint point, int delta)
    {
    }

    private void PaintSurface(SKPaintSurfaceEventArgs e)
    {
        SKCanvas canvas = e.Surface.Canvas;
        canvas.Clear();
        SKImageInfo info = e.Info;
        DisplayState.Size = new Size(info.Width, info.Height);
        SkiaEngine.Renderables.Clear();
        SkiaEngine.RenderEngine.Canvas = canvas;

        //FIXME

        SkiaEngine.RenderEngine.UpdateRenderables(SkiaEngine.Renderables);
        SkiaEngine.RenderEngine.Render();
    }

    private async void OnOpenConnectCommand()
    {
        await ViewManager.OpenConnectView();
    }

    private void OnOpenMessagesCommand()
    {
        ViewManager.OpenMessagesView();
    }
}
