using Common.Mvvm;
namespace Client.UI.Displays.Tactical.MasterToolbar.VideoMaps;

public class VideoMapToggleState : ViewModelBase
{
    private bool regions;
    public bool Regions
    {
        get => regions;
        set
        {
            regions = value;
            OnPropertyChanged();
        }
    }

    private bool airspace;
    public bool Airspace
    {
        get => airspace;
        set
        {
            airspace = value;
            OnPropertyChanged();
        }
    }
}
