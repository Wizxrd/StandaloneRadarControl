using Common.Mvvm;
namespace Client.UI.Displays.Tactical.MasterToolbar.VideoMaps;

public class VideoMapToggleState : ViewModelBase
{
    private bool countries;
    private bool airspace;

    public bool Countries
    {
        get => countries;
        set
        {
            countries = value;
            OnPropertyChanged();
        }
    }
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
