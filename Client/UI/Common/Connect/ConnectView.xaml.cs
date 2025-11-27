using AdonisUI.Controls;
namespace Client.UI.Common.Connect;

public partial class ConnectView : AdonisWindow
{
    public ConnectView()
    {
        InitializeComponent();
        DataContext = new ConnectViewModel();
    }
}
