using System.Windows;
using System.Windows.Input;

namespace Client.Views;

/// <summary>
///   Interaction logic for GeneralSettingsView.xaml
/// </summary>
public partial class GeneralSettingsView : Window
{
	public GeneralSettingsView()
	{
		InitializeComponent();
	}

	private void BorderMouseDown(object sender, MouseButtonEventArgs e)
	{
		if (e.ChangedButton == MouseButton.Left) DragMove();
	}

	private void CloseButtonClick(object sender, RoutedEventArgs e)
	{
		Close();
	}
}