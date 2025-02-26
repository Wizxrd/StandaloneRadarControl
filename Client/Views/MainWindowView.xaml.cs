using System.Windows;
using System.Windows.Input;
using Client.Models;
using Client.ViewModels;
using SkiaSharp;
using SkiaSharp.Views.Desktop;

namespace Client.Views;

/// <summary>
///   Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindowView : Window
{
	private readonly Command command;
	private readonly CommandAreaViewModel commandAreaViewModel;

	private readonly MainWindowViewModel viewModel = new();
	public Profile? CurrentProfile;
	public Navigate navigate;
	public Radar radar;

	public MainWindowView()
	{
		InitializeComponent();
		radar = new Radar(this);
		navigate = new Navigate(this);
		DataContext = viewModel;
		commandAreaViewModel = (CommandAreaViewModel)CommandArea.DataContext;
		command = new Command(commandAreaViewModel);
	}

	private void RadarMouseDown(object sender, MouseButtonEventArgs e)
	{
		if (e.RightButton == MouseButtonState.Pressed)
		{
			var mousePos = e.GetPosition(this);
			radar.MouseDown(mousePos);
			viewModel.CustomCursor = Cursors.None;
			RadarCanvas.CaptureMouse();
			InvalidateCanvas();
		}
	}

	private void RadarMouseUp(object sender, MouseButtonEventArgs e)
	{
		if (e.RightButton == MouseButtonState.Released)
		{
			radar.MouseUp(sender, e);
			viewModel.CustomCursor = new Cursor(
				Application.GetResourceStream(new Uri("pack://application:,,,/Cursors/Cross1.cur", UriKind.Absolute))
					.Stream);
			RadarCanvas.ReleaseMouseCapture();
			InvalidateCanvas();
		}
	}

	private void RadarMouseMove(object sender, MouseEventArgs e)
	{
		var mousePos = e.GetPosition(this);
		radar.MouseMove(mousePos);
		InvalidateCanvas();
	}

	private void RadarMouseWheel(object sender, MouseWheelEventArgs e)
	{
		var invert = false;
		var range = radar.MouseWheel(e.Delta, invert);
		if (MainControlButtons.DataContext is MainControlButtonViewModel vm) vm.ZoomRange = (int)range;
		InvalidateCanvas();
	}

	public void InvalidateCanvas()
	{
		RadarCanvas.InvalidateVisual();
	}

	private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
	{
		var canvas = e.Surface.Canvas;
		canvas.Clear(SKColors.Black);
		radar.Invalidate(e);
	}

	public void SetTitle(string profile)
	{
		TitleBar.TitleText = $"SRC : {profile}";
	}

	public void TitleBarMouseDown(object sender, MouseButtonEventArgs e)
	{
		if (e.ChangedButton == MouseButton.Left) DragMove();
	}

	private void MainWindowViewKeyDown(object sender, KeyEventArgs e)
	{
		var shiftPressed = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
		var ctrlPressed = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);

		if (ctrlPressed & shiftPressed)
			switch (e.Key)
			{
				case Key.S:
					Navigate.OpenSaveProfileAsView(navigate);
					break;
			}
		else if (ctrlPressed)
			switch (e.Key)
			{
				case Key.M:
					Navigate.OpenMessagesView(navigate);
					break;
				case Key.F12:
					Navigate.OpenConnectView(navigate);
					break;
				case Key.N:
					Navigate.OpenNewProfileView(navigate);
					break;
				case Key.L:
					Hide();
					Navigate.OpenLoadProfileView(navigate);
					break;
				case Key.S:
					Navigate.OpenSaveProfileView(navigate);
					break;
				case Key.G:
					Navigate.OpenGeneralSettingsView(navigate);
					break;
				case Key.D:
					Navigate.OpenDisplaySettingsView(navigate);
					break;
				case Key.H:
					Navigate.OpenHelpView(navigate);
					break;
			}
		else
			command.Handle(ctrlPressed, shiftPressed, e.Key);
	}
}