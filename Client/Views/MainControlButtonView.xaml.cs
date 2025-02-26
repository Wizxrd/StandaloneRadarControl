using System.Windows.Controls;
using Client.ViewModels;

namespace Client.Views;

/// <summary>
///   Interaction logic for MainControlButtonView.xaml
/// </summary>
public partial class MainControlButtonView : UserControl
{
	public MainControlButtonView()
	{
		InitializeComponent();
		DataContext = new MainControlButtonViewModel();
	}
}