using System.Windows;
using System.Windows.Input;
using Client.Models;
using Client.ViewModels;

namespace Client.Views;

/// <summary>
///   Interaction logic for LoadProfileView.xaml
/// </summary>
public partial class LoadProfileView : Window
{
	private Navigate navigate;

	public LoadProfileView(Navigate navigate)
	{
		InitializeComponent();
		this.navigate = navigate;
		DataContext = new LoadProfileViewModel(navigate);
	}

	private void BorderMouseDown(object sender, MouseButtonEventArgs e)
	{
		if (e.ChangedButton == MouseButton.Left) DragMove();
	}

	private void CloseButtonClick(object sender, RoutedEventArgs e)
	{
		Application.Current.Shutdown();
	}

	private void SearchBoxGotFocus(object sender, RoutedEventArgs e)
	{
		PlaceholderText.Visibility = Visibility.Hidden;
	}

	private void SearchBoxLostFocus(object sender, RoutedEventArgs e)
	{
		if (string.IsNullOrEmpty(SearchBox.Text)) PlaceholderText.Visibility = Visibility.Visible;
	}
}