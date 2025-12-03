using Client.Engines;
using Client.Managers;
using Client.Models;
using Client.Renderables;
using Client.Renderables.Interfaces;
using Client.Services;
using Client.UI.Controls.RenderDisplay;
using Client.Utils;
using Common.Mvvm;
using Microsoft.VisualBasic.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System;
using System.Collections.Generic;
using System.IO;
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


    public Dictionary<string, List<ProcessedFeature>> Features { get; set; } = new();
    public TacticalViewModel(RenderDisplayView renderDisplayView)
    {
        //DisplayStatus = "NOT RECEIVING SURVEILLANCE DATA";
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

        SetDisplayState();
    }

    List<ProcessedFeature> RenderableFeatures { get; set; } = new();
    private int ZoomIndex = 43;
    public async void SetDisplayState()
    {
        //42.894097161564794, 40.60800841102051 CAUCASUS
        DisplayState.IsReady = true;
        DisplayState.Center.Lat = 42.894097161564794;
        DisplayState.Center.Lon = 40.60800841102051;
        ZoomLevels = Zoom.BuildLevels();
        ScaleMap = Zoom.BuildScale(DisplayState, ZoomLevels);
        DisplayState.Width = 1300;
        DisplayState.Height = 870;
        DisplayState.Scale = ScaleMap[ZoomIndex];
        DisplayState.PanOffset = CenterAtCoordinates(DisplayState.Width, DisplayState.Height, DisplayState.Scale, DisplayState.PanOffset, DisplayState.Center);

        JArray list = new JArray();
        list.Add("Airspace");
        list.Add("Countries");
        Features = await GeoJson.GetEramFacilityFeatures(list);
        foreach (var kvp in Features)
        {
            var filtered = kvp.Value.Where(f =>
            {
                if (f.AppliedAttributes.TryGetValue("tdmOnly", out var tdmVal))
                {
                    bool isTdmOnly = Convert.ToBoolean(tdmVal);
                    if (isTdmOnly && !false)
                        return false;
                }
                return true;
            });
            RenderableFeatures.AddRange(filtered);
        }
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
        DisplayState.Width = (int)width;
        DisplayState.Height = (int)height;
        ScaleMap = Zoom.BuildScale(DisplayState, ZoomLevels);
        DisplayState.Scale = ScaleMap[43];
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
        if (!DisplayState.IsReady) return;
        int direction = delta > 0 ? 1 : -1;
        int step = DisplayState.DoubleZoom ? 4 : 1;
        int newIndex = Math.Clamp(ZoomIndex + direction * step, 0, ZoomLevels.Count - 1);
        if (newIndex == ZoomIndex) return;

        SKPoint referencePoint = DisplayState.ZoomOnMouse ? point : new SKPoint(DisplayState.Width / 2f, DisplayState.Height / 2f);

        var before = new SKPoint(
            (referencePoint.X - DisplayState.PanOffset.X - DisplayState.Width / 2f) / (float)DisplayState.Scale,
            (referencePoint.Y - DisplayState.PanOffset.Y - DisplayState.Height / 2f) / (float)DisplayState.Scale
        );

        ZoomIndex = newIndex;

        DisplayState.Scale = ScaleMap[ZoomIndex];

        var after = new SKPoint(
            (referencePoint.X - DisplayState.PanOffset.X - DisplayState.Width / 2f) / (float)DisplayState.Scale,
            (referencePoint.Y - DisplayState.PanOffset.Y - DisplayState.Height / 2f) / (float)DisplayState.Scale
        );

        var diff = after - before;
        DisplayState.PanOffset = new SKPoint(
            DisplayState.PanOffset.X + diff.X * (float)DisplayState.Scale,
            DisplayState.PanOffset.Y + diff.Y * (float)DisplayState.Scale
        );
        if (!DisplayState.ZoomOnMouse && !DisplayState.IsPanning)
        {
            DisplayState.PanOffset = CenterAtCoordinates(DisplayState.Width, DisplayState.Height, DisplayState.Scale, DisplayState.PanOffset, DisplayState.Center);
        }
        else
        {
            var newCenter = ScreenMap.ScreenToCoordinate(
                new System.Drawing.Size(DisplayState.Width, DisplayState.Height),
                DisplayState.Scale,
                DisplayState.PanOffset,
                new SKPoint(DisplayState.Width / 2f, DisplayState.Height / 2f)
            );

            DisplayState.Center = new Coordinate { Lat = newCenter.Lat, Lon = newCenter.Lon };
            var pOffset = DisplayState.PanOffset;
            DisplayState.PanOffset = pOffset;
        }
        SkiaEngine.RequestRender();
    }

    private void PaintSurface(SKPaintSurfaceEventArgs e)
    {
        SKCanvas canvas = e.Surface.Canvas;
        canvas.Clear();
        SKImageInfo info = e.Info;
        DisplayState.Size = new Size(info.Width, info.Height);
        SkiaEngine.Renderables.Clear();
        SkiaEngine.RenderEngine.Canvas = canvas;
        if (SkiaEngine.RenderEngine.Size != new Size(DisplayState.Width, DisplayState.Height)) SkiaEngine.RenderEngine.Size = new Size(DisplayState.Width, DisplayState.Height);
        if (SkiaEngine.RenderEngine.Scale != DisplayState.Scale) SkiaEngine.RenderEngine.Scale = DisplayState.Scale;
        if (SkiaEngine.RenderEngine.PanOffset != DisplayState.PanOffset) SkiaEngine.RenderEngine.PanOffset = DisplayState.PanOffset;

        List<IRenderable> videoMapRenderables = Renderer.FromFeatures(DisplayState, RenderableFeatures);
        SkiaEngine.Renderables.AddRange(videoMapRenderables);

        List<IRenderable> airplaneRenderables = Renderer.FromAirplanes(DisplayState, SurveillanceService.Airplanes);
        SkiaEngine.Renderables.AddRange(airplaneRenderables);

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
