using Common.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Client.UI.Displays.Tactical.CommandComposition;

public class CommandCompositionViewModel : ViewModelBase
{
    private int zIndex = 0;
    private string raiseLowerText = "RAISE";
    private Visibility commandCompositionVisibility = Visibility.Visible;

    public int ZIndex
    {
        get => zIndex;
        set
        {
            zIndex = value;
            OnPropertyChanged();
        }
    }

    public string RaiseLowerText
    {
        get => raiseLowerText;
        set
        {
            raiseLowerText = value;
            OnPropertyChanged();
        }
    }
    public Visibility CommandCompositionVisibility
    {
        get => commandCompositionVisibility;
        set
        {
            commandCompositionVisibility = value;
            OnPropertyChanged();
        }
    }

    public ICommand CommandCompositionAreaCommand { get; set; }
    public ICommand CommandCompositionAreaRaiseLowerCommand { get; set; }

    public CommandCompositionViewModel()
    {
        CommandCompositionAreaCommand = new RelayCommand(OnCommandCompositionAreaCommand);
        CommandCompositionAreaRaiseLowerCommand = new RelayCommand(OnCommandCompositionAreaRaiseLowerCommand);
    }

    private void OnCommandCompositionAreaCommand()
    {
        if (CommandCompositionVisibility == Visibility.Collapsed)
        {
            CommandCompositionVisibility = Visibility.Visible;
        }
        else
        {
            CommandCompositionVisibility = Visibility.Collapsed;
        }
    }

    private void OnCommandCompositionAreaRaiseLowerCommand()
    {
        if (ZIndex == 0)
        {
            ZIndex = int.MaxValue;
            RaiseLowerText = "LOWER";
        }
        else
        {
            ZIndex = 0;
            RaiseLowerText = "RAISE";
        }
    }
}
