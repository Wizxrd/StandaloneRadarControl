using Common.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Client.UI.Displays.Tactical.ToolbarControl;

public class ToolbarControlToggleState : ViewModelBase
{
    private bool masterToolbar = true;
    private bool masterRaise;
    private bool commandCompositionArea = true;
    private bool commandCompositionAreaRaise;

    public bool MasterToolbar
    {
        get => masterToolbar;
        set
        {
            masterToolbar = value;
            OnPropertyChanged();
        }
    }
    public bool MasterRaise
    {
        get => masterRaise;
        set
        {
            masterRaise = value;
            OnPropertyChanged();
        }
    }
    public bool CommandCompositionArea
    {
        get => commandCompositionArea;
        set
        {
            commandCompositionArea = value;
            OnPropertyChanged();
        }
    }
    public bool CommandCompositionAreaRaise
    {
        get => commandCompositionAreaRaise;
        set
        {
            commandCompositionAreaRaise = value;
            OnPropertyChanged();
        }
    }
}
