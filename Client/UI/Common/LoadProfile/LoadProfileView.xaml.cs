using AdonisUI.Controls;
namespace Client.UI.Common.LoadProfile;

public partial class LoadProfileView : AdonisWindow
{
    public LoadProfileView()
    {
        InitializeComponent();
        DataContext = new LoadProfileViewModel();
    }
}
