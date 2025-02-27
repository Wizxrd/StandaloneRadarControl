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
	}

	public void TitleBarMouseDown(object sender, MouseButtonEventArgs e)
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
			if (tcpStarted && udpStarted)
			{
				StartServerButton.Tag = "1";
				StartServerButton.Content = "STOP";
			}
		}
		else
		{
			StartServerButton.Tag = "-1";
			StartServerButton.Content = "START";
			await viewModel.StopServer();
			UpdateClientPortTextBox(string.Empty);
		}
	}

	public void UpdateClientPortTextBox(string port)
	{
		PortTextBlock.Text = $"Port: {port}";
	}
}