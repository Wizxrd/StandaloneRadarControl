using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Client.Models;

namespace Client.Views;

/// <summary>
///   Interaction logic for NewProfileView.xaml
/// </summary>
public partial class NewProfileView : Window
{
	public NewProfileView()
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
			CreateProfileButton.IsEnabled = false;
			CreateProfileButton.Background = (Brush)Application.Current.Resources["GrayBackgroundDark"];
			return;
		}

		CreateProfileButton.IsEnabled = true;
		CreateProfileButton.Background = (Brush)Application.Current.Resources["GrayBackgroundLight"];
	}

	private void ProfileNameTextBoxKeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.Enter) CreateProfileButtonClick(sender, e);
	}

	private async void CreateProfileButtonClick(object sender, RoutedEventArgs e)
	{
		if (ProfileNameTextBox.Text == string.Empty) return;
		await Profile.New(ProfileNameTextBox.Text, Owner);
		Close();
	}
}