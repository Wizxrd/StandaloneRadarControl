using Common.Mvvm;
using System.Windows;
using System.Windows.Input;

namespace Client.UI.Displays.Tactical.MasterToolbar.VideoMaps;

public class VideoMapsViewModel : ViewModelBase, ICloneable
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


    public VideoMapsViewModel()
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

    public object Clone()
    {
        return new VideoMapsViewModel
        {
            BorderVisibility = Visibility.Collapsed
        };
    }
}
