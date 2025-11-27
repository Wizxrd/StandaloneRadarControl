using Client.Managers;
using Client.Models;
using Client.UI.Controls.Display;
using Common.Mvvm;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using vFalcon.Engines;
using Size = System.Drawing.Size;
namespace Client.UI.Displays.Tactical;

public class TacticalViewModel : ViewModelBase
{
    private RenderView renderView { get; set; }

    public SkiaEngine SkiaEngine { get; set; }

    public DisplayState DisplayState { get; set; } = new();

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

    public TacticalViewModel(RenderView renderView)
    {
        DisplayStatus = "NOT RECEIVING SURVEILLANCE DATA";
        App.DisplayState = DisplayState;

        this.renderView = renderView;
        SkiaEngine = renderView.SkiaEngine;

        SkiaEngine.SizeChanged += OnSizeChanged;
        SkiaEngine.MouseDown += OnMouseDown;
        SkiaEngine.MouseUp += OnMouseUp;
        SkiaEngine.MouseMove += OnMouseMove;
        SkiaEngine.MouseWheel += OnMouseWheel;
        SkiaEngine.PaintSurface += PaintSurface;

        OpenConnectCommand = new RelayCommand(OnOpenConnectCommand);
        OpenMessagesCommand = new RelayCommand(OnOpenMessagesCommand);
    }

    private void OnSizeChanged(double width, double height)
    {
    }

    private void OnMouseDown(object sender, SKPoint point, MouseButton button)
    {
    }

    private void OnMouseMove(object sender, SKPoint point)
    {
    }

    private void OnMouseUp(object sender, SKPoint point, MouseButton button)
    {
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
    }

    private async void OnOpenConnectCommand()
    {
        await ViewManager.OpenConnectView();
        DisplayStatus = string.Empty;
    }

    private void OnOpenMessagesCommand()
    {
        ViewManager.OpenMessagesView();
    }
}
