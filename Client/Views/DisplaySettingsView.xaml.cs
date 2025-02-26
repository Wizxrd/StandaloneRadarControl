using System.Windows;
using System.Windows.Input;

namespace Client.Views;

/// <summary>
///   Interaction logic for DisplaySettingsView.xaml
/// </summary>
public partial class DisplaySettingsView : Window
{
	public DisplaySettingsView()
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