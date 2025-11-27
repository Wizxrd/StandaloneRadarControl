using AdonisUI.Controls;
using Client.UI.Controls.MasterToolbar;
using Common.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Client.UI.Displays.Tactical.MasterToolbar;

public class MasterToolbarViewModel : ViewModelBase
{
    private bool deleteMode;
    private int zIndex = 0;
    private string raiseLowerText = "RAISE";
    private Visibility masterToolbarVisibility = Visibility.Visible;

    public Visibility MasterToolbarVisibility
    {
        get => masterToolbarVisibility;
        set
        {
            masterToolbarVisibility = value;
            OnPropertyChanged();
        }
    }

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

    public ICommand MasterToolbarCommand { get; set; }
    public ICommand MasterRaiseLowerCommand { get; set; }
    public ICommand DeleteCommandToggle { get; set; }

    public MasterToolbarViewModel()
    {
        MasterToolbarCommand = new RelayCommand(OnMasterToolbarCommand);
        MasterRaiseLowerCommand = new RelayCommand(OnMasterRaiseLowerCommand);
        DeleteCommandToggle = new RelayCommand(OnDeleteCommandToggle);
    }

    private void OnMasterToolbarCommand()
    {
        if (MasterToolbarVisibility == Visibility.Collapsed)
        {
            MasterToolbarVisibility = Visibility.Visible;
        }
        else
        {
            MasterToolbarVisibility = Visibility.Collapsed;
        }
    }

    private void OnMasterRaiseLowerCommand()
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

    private void OnDeleteCommandToggle()
    {
        deleteMode = !deleteMode;
        ToggleButton.DeleteModeEnabled = deleteMode;
        MenuButton.DeleteModeEnabled = deleteMode;
    }
}
