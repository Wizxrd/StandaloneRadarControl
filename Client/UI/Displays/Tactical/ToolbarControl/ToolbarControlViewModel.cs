using Common.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
namespace Client.UI.Displays.Tactical.ToolbarControl;

public class ToolbarControlViewModel : ViewModelBase
{
    private Visibility borderVisibility = Visibility.Collapsed;

    public Visibility BorderVisibility
    {
        get => borderVisibility;
        set
        {
            borderVisibility = value;
            OnPropertyChanged();
        }
    }

    public ICommand CommandTest { get; }

    public ToolbarControlViewModel()
    {
        CommandTest = new RelayCommand(OnCommandTest);
    }


    private void OnCommandTest()
    {
        if (BorderVisibility == Visibility.Visible)
            BorderVisibility = Visibility.Collapsed;
        else
            BorderVisibility = Visibility.Visible;
    }
}
