using AdonisUI.Controls;
namespace Client.UI.Common.NewProfile;

public partial class NewProfileView : AdonisWindow
{
    public NewProfileView()
    {
        InitializeComponent();
        DataContext = new NewProfileViewModel();
    }
}
