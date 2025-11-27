using AdonisUI.Controls;
using System.Globalization;
using System.Windows;
namespace Server.UI.MainWindow;

public partial class MainWindowView : AdonisWindow
{
    public MainWindowView()
    {
        InitializeComponent();
        LoadWindowSettings();
        DataContext = new MainWindowViewModel();
        SizeChanged += OnSizeChanged;
        LocationChanged += OnLocationChanged;
        StateChanged += OnStateChanged;
    }

    private void LoadWindowSettings()
    {
        double[] parts = App.Settings.WindowSettings.Bounds.Split(',').Select(s => double.Parse(s, CultureInfo.InvariantCulture)).ToArray();
        Left = parts[0];
        Top = parts[1];
        if (parts[2] == -1) Width = double.NaN;
        else Width = parts[2];
        if (parts[3] == -1) Height = double.NaN;
        else Height = parts[3];
        if (App.Settings.WindowSettings.IsMaximized) WindowState = WindowState.Maximized;
        else
        {
            WindowStyle = WindowStyle.SingleBorderWindow;
        }
        if (App.Settings.WindowSettings.ShowTitleBar) WindowStyle = WindowStyle.SingleBorderWindow;
        else WindowStyle = WindowStyle.None;
    }

    private void OnSizeChanged(object sender, EventArgs e)
    {
        App.Settings.WindowSettings.Bounds = $"{this.Left},{this.Top},{this.Width},{this.Height}";
    }

    private void OnLocationChanged(object? sender, EventArgs e)
    {
        App.Settings.WindowSettings.Bounds = $"{this.Left},{this.Top},{this.Width},{this.Height}";
    }

    private void OnStateChanged(object? sender, EventArgs e)
    {
        App.Settings.WindowSettings.IsMaximized = (WindowState == WindowState.Maximized);
    }
}
