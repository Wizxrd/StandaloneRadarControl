using Server.UI.Controls.ExportSettings.Basic;
using System.Windows.Controls;
namespace Server.UI.Views.Controls;

public partial class BasicSettingsView : UserControl
{
    public BasicSettingsView()
    {
        InitializeComponent();
        DataContext = new BasicSettingsViewModel();
    }
}
