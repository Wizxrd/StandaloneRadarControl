using System.Windows;
using Client.Views;

namespace Client;

public partial class App : Application
{
	protected override void OnStartup(StartupEventArgs e)
	{
		base.OnStartup(e);
		var splashWindowView = new SplashScreenView();
		splashWindowView.TextBlockTitle.Text = "SRC";
		splashWindowView.TextBlockLoading.Text = "Initializing Client";
		splashWindowView.Show();
		// TO BE REPLACED WITH CHECKING AUTO UPDATER **TEMPORARY**
		Task.Delay(1000).ContinueWith(_ =>
		{
			splashWindowView.Dispatcher.Invoke(() =>
			{
				splashWindowView.TextBlockLoading.Text = "Checking for updates";
			});
		});
		Task.Delay(2000).ContinueWith(_ =>
		{
			splashWindowView.Dispatcher.Invoke(() =>
			{
				splashWindowView.TextBlockLoading.Text = "Launching Client";
			});
		});
		Task.Delay(3000).ContinueWith(_ =>
		{
			splashWindowView.Dispatcher.Invoke(() =>
			{
				var mainWindow = new MainWindowView();
				mainWindow.Visibility = Visibility.Hidden;
				mainWindow.Show();
				var loadProfileWindow = new LoadProfileView(mainWindow.navigate)
				{
					WindowStartupLocation = WindowStartupLocation.CenterScreen
				};
				splashWindowView.Close();
				Task.Delay(100).ContinueWith(_ =>
				{
					loadProfileWindow.Dispatcher.Invoke(() => { loadProfileWindow.ShowDialog(); });
				});
			});
		});
	}
}