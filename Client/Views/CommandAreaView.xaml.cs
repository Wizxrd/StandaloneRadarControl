using System.Windows.Controls;

namespace Client.Views;

/// <summary>
///   Interaction logic for CommandAreaView.xaml
/// </summary>
public partial class CommandAreaView : UserControl
{
	public CommandAreaView()
	{
		InitializeComponent();
		DataContext = new CommandAreaViewModel();
	}
}