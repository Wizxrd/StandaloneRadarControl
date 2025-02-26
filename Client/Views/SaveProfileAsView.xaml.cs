using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Client.Models;

namespace Client.Views;

/// <summary>
///   Interaction logic for SaveProfileAsView.xaml
/// </summary>
public partial class SaveProfileAsView : Window
{
	public SaveProfileAsView()
	{
		InitializeComponent();
		ProfileNameTextBox.Focus();
	}

	private void BorderMouseDown(object sender, MouseButtonEventArgs e)
	{
		if (e.ChangedButton == MouseButton.Left) DragMove();
	}

	private void CloseButtonClick(object sender, RoutedEventArgs e)
	{
		Close();
	}

	private void ProfileNameTextBoxTextChanged(object sender, TextChangedEventArgs e)
	{
		if (ProfileNameTextBox.Text == string.Empty)
		{
			SaveProfileButton.IsEnabled = false;
			SaveProfileButton.Background = (Brush)Application.Current.Resources["GrayBackgroundDark"];
			return;
		}

		SaveProfileButton.IsEnabled = true;
		SaveProfileButton.Background = (Brush)Application.Current.Resources["GrayBackgroundLight"];
	}

	private void ProfileNameTextBoxKeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.Enter) SaveProfileAsButtonClick(sender, e);
	}

	private async void SaveProfileAsButtonClick(object sender, RoutedEventArgs e)
	{
		if (ProfileNameTextBox.Text == string.Empty) return;
		await Profile.New(ProfileNameTextBox.Text, Owner);
		Close();
	}
}