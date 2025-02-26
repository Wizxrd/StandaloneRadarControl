using System.Windows;
using System.Windows.Input;
using Server.Models;
using Server.Network;

namespace Server.Views;

/// <summary>
///   Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindowView : Window
{
	private readonly TcpServerHandler tcpServerHandler;
	private readonly UdpServerHandler udpServerHandler;

	public MainWindowView()
	{
		InitializeComponent();
		Logger.Wipe();
		tcpServerHandler = new TcpServerHandler(this);
		udpServerHandler = new UdpServerHandler(this);
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
		await tcpServerHandler.StopAsync();
		udpServerHandler.Stop();
		Application.Current.Shutdown();
	}

	private async void StartServerButtonClick(object sender, RoutedEventArgs e)
	{
		if (StartServerButton.Tag.ToString() == "-1")
		{
			var tcpServerStarted = tcpServerHandler.Start();
			var udpServerStarted = udpServerHandler.Start();
			if (tcpServerStarted && udpServerStarted)
			{
				StartServerButton.Tag = "1";
				StartServerButton.Content = "STOP";
			}
		}
		else
		{
			StartServerButton.Tag = "-1";
			StartServerButton.Content = "START";
			await tcpServerHandler.StopAsync();
			udpServerHandler.Stop();
			UpdateClientPortTextBox(string.Empty);
		}
	}

	public void UpdateClientPortTextBox(string port)
	{
		PortTextBlock.Text = $"Port: {port}";
	}
}