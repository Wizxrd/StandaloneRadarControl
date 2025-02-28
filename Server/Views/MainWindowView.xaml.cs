using System.Windows;
using System.Windows.Input;
using Server.ViewModels;

namespace Server.Views;

/// <summary>
///   Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindowView : Window
{
	private readonly MainWindowViewModel viewModel = new();
	
	public MainWindowView()
	{
		InitializeComponent();
		this.DataContext = viewModel;
	}

	private void TitleBarMouseDown(object sender, MouseButtonEventArgs e)
	{
		if (e.ChangedButton == MouseButton.Left) DragMove();
	}

	private void MinimizeButtonClick(object sender, RoutedEventArgs e)
	{
		WindowState = WindowState.Minimized;
	}

	private async void CloseButtonClick(object sender, RoutedEventArgs e)
	{
		try
		{
			await viewModel.ExitApplication();
		}
		catch (Exception exception)
		{
			throw;
		}
	}

	private async void StartServerButtonClick(object sender, RoutedEventArgs e)
	{
		if (StartServerButton.Tag.ToString() == "-1")
		{
			var (tcpStarted, udpStarted) = viewModel.StartServer();
			if (!tcpStarted || !udpStarted) return;
			StartServerButton.Tag = "1";
			StartServerButton.Content = "STOP";
		}
		else
		{			
			await viewModel.StopServer();
			StartServerButton.Tag = "-1";
			StartServerButton.Content = "START";
		}
	}
}