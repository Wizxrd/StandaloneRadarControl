using Client.Models;
using Client.Network;
using Client.ViewModels;
using SkiaSharp;
using System.Windows;
using System.Windows.Input;


namespace Client.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindowView : Window
    {

        private MainWindowViewModel viewModel = new();
        private CommandAreaViewModel commandAreaViewModel;
        private Command command;
        public Navigate navigate;
        public Profile? CurrentProfile;
        public Radar radar;

        public MainWindowView()
        {
            InitializeComponent();
            radar = new Radar(this);
            navigate = new(this);
            DataContext = viewModel;
            commandAreaViewModel = (CommandAreaViewModel)CommandArea.DataContext;
            command = new(commandAreaViewModel);
        }

        private void RadarMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {
                Point mousePos = e.GetPosition(this);
                radar.MouseDown(mousePos);
                viewModel.CustomCursor = System.Windows.Input.Cursors.None;
                RadarCanvas.CaptureMouse();
                InvalidateCanvas();
            }
        }

        private void RadarMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Released)
            {
                radar.MouseUp(sender, e);
                viewModel.CustomCursor = new System.Windows.Input.Cursor(
                System.Windows.Application.GetResourceStream(new Uri($"pack://application:,,,/Cursors/Cross1.cur", UriKind.Absolute)).Stream);
                RadarCanvas.ReleaseMouseCapture();
                InvalidateCanvas();
            }
        }

        private void RadarMouseMove(object sender, MouseEventArgs e)
        {
            Point mousePos = e.GetPosition(this);
            radar.MouseMove(mousePos);
            InvalidateCanvas();
        }

        private void RadarMouseWheel(object sender, MouseWheelEventArgs e)
        {
            bool invert = false;
            double range = radar.MouseWheel(e.Delta, invert);
            if (MainControlButtons.DataContext is MainControlButtonViewModel vm)
            {
                vm.ZoomRange = (int)range;
            }
            InvalidateCanvas();
        }

        public void InvalidateCanvas()
        {
            RadarCanvas.InvalidateVisual();
        }

        private void OnPaintSurface(object sender, SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs e)
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
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void MainWindowViewKeyDown(object sender, KeyEventArgs e)
        {
            bool shiftPressed = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
            bool ctrlPressed = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);

            if (ctrlPressed & shiftPressed)
            {
                switch (e.Key)
                {
                    case Key.S:
                        Navigate.OpenSaveProfileAsView(navigate);
                        break;
                }
            }
            else if (ctrlPressed)
            {
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
                        this.Hide();
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
            }
            else
            {
                command.Handle(ctrlPressed, shiftPressed, e.Key);
            }
        }
    }
}