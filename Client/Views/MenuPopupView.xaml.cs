using System.Windows;
using System.Windows.Controls;
using Client.Models;

namespace Client.Views;

public partial class MenuPopupView : UserControl
{
	public MenuPopupView()
	{
		InitializeComponent();
	}

	private void ConnectButtonClick(object sender, RoutedEventArgs e)
	{
		var parentWindow = Window.GetWindow(this);
		if (parentWindow == null) return;
		if (parentWindow is MainWindowView mainWindowView) Navigate.OpenConnectView(mainWindowView.navigate);
	}

	private void NewProfileButtonClick(object sender, RoutedEventArgs e)
	{
		var parentWindow = Window.GetWindow(this);
		if (parentWindow == null) return;
		if (parentWindow is MainWindowView mainWindowView) Navigate.OpenNewProfileView(mainWindowView.navigate);
	}

	private void LoadProfileButtonClick(object sender, RoutedEventArgs e)
	{
		var parentWindow = Window.GetWindow(this);
		if (parentWindow == null) return;
		if (parentWindow is MainWindowView mainWindowView)
		{
			mainWindowView.Hide();
			Navigate.OpenLoadProfileView(mainWindowView.navigate);
		}
	}

	private void SaveProfileButtonClick(object sender, RoutedEventArgs e)
	{
		var parentWindow = Window.GetWindow(this);
		if (parentWindow == null) return;
		if (parentWindow is MainWindowView mainWindowView) Navigate.OpenSaveProfileView(mainWindowView.navigate);
	}

	private void SaveProfileAsButtonClick(object sender, RoutedEventArgs e)
	{
		var parentWindow = Window.GetWindow(this);
		if (parentWindow == null) return;
		if (parentWindow is MainWindowView mainWindowView) Navigate.OpenSaveProfileAsView(mainWindowView.navigate);
	}

	private void GeneralSettingsButtonClick(object sender, RoutedEventArgs e)
	{
		var parentWindow = Window.GetWindow(this);
		if (parentWindow == null) return;
		if (parentWindow is MainWindowView mainWindowView) Navigate.OpenGeneralSettingsView(mainWindowView.navigate);
	}

	private void DisplaySettingsButtonClick(object sender, RoutedEventArgs e)
	{
		var parentWindow = Window.GetWindow(this);
		if (parentWindow == null) return;
		if (parentWindow is MainWindowView mainWindowView) Navigate.OpenDisplaySettingsView(mainWindowView.navigate);
	}

	private void ViewButtonClick(object sender, RoutedEventArgs e)
	{
		ViewPopup.IsOpen = !ViewPopup.IsOpen;
	}

	private void HelpButtonClick(object sender, RoutedEventArgs e)
	{
		var parentWindow = Window.GetWindow(this);
		if (parentWindow == null) return;
		if (parentWindow is MainWindowView mainWindowView) Navigate.OpenHelpView(mainWindowView.navigate);
	}
}