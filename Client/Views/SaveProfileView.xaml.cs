using System.Windows;
using System.Windows.Input;
using Client.Models;

namespace Client.Views;

/// <summary>
///   Interaction logic for SaveProfileView.xaml
/// </summary>
public partial class SaveProfileView : Window
{
	private readonly Navigate navigate;

	public SaveProfileView(Navigate navigate)
	{
		InitializeComponent();
		this.navigate = navigate;
		MessageTextBlock.Text = $"Save Profile: {navigate.mainWindowView.CurrentProfile?.Name ?? string.Empty}";
		YesButton.Focus();
	}

	private void BorderMouseDown(object sender, MouseButtonEventArgs e)
	{
		if (e.ChangedButton == MouseButton.Left) DragMove();
	}

	private async void YesButtonClick(object sender, RoutedEventArgs e)
	{
		if (navigate.mainWindowView.CurrentProfile == null) return;
		await Profile.Save(navigate.mainWindowView.CurrentProfile.Name, navigate.mainWindowView);
		Close();
	}

	private void CloseButtonClick(object sender, RoutedEventArgs e)
	{
		Close();
	}
}