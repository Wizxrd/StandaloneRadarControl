using System.Windows;
using System.Windows.Input;

namespace Client.Views;

/// <summary>
///   Interaction logic for DeleteProfileView.xaml
/// </summary>
public partial class DeleteProfileView : Window
{
	public bool DeletionConfirmed;

	public DeleteProfileView()
	{
		InitializeComponent();
		NoButton.Focus();
	}

	private void BorderMouseDown(object sender, MouseButtonEventArgs e)
	{
		if (e.ChangedButton == MouseButton.Left) DragMove();
	}

	private void CloseButtonClick(object sender, RoutedEventArgs e)
	{
		Close();
	}

	private void YesButtonClick(object sender, RoutedEventArgs e)
	{
		DeletionConfirmed = true;
		Close();
	}

	public void SetMessageTextBlock(string message, string profileName)
	{
		if (profileName == string.Empty)
			MessageTextBlock.Text = message;
		else
			MessageTextBlock.Text = $"{message} \"{profileName}\"";
		MessageTextBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
		if (MessageTextBlock.DesiredSize.Width > Width) Width = MessageTextBlock.DesiredSize.Width;
	}
}