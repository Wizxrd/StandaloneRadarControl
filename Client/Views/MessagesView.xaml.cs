using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Client.Views;

/// <summary>
///   Interaction logic for MessagesView.xaml
/// </summary>
public partial class MessagesView : Window
{
	public MessagesView()
	{
		InitializeComponent();
		MessageSendTextBox.Focus();
	}

	private void BorderMouseDown(object sender, MouseButtonEventArgs e)
	{
		if (e.ChangedButton == MouseButton.Left) DragMove();
	}

	private void CloseButtonClick(object sender, RoutedEventArgs e)
	{
		Close();
	}

	private void MessageSendTextChanged(object sender, TextChangedEventArgs e)
	{
	}

	private void MessageSendKeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.Enter)
			if (MessageSendTextBox.Text != string.Empty)
			{
				MessageReceiveTextBox.Text += $"[{DateTime.Now.ToString("HH:mm:ss")}] {MessageSendTextBox.Text}\n";
				MessageSendTextBox.Text = string.Empty;
			}
	}
}