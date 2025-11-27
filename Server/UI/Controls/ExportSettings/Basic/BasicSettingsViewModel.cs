using Common.Mvvm;
using System.Windows;
using System.Windows.Input;
namespace Server.UI.Controls.ExportSettings.Basic;

public class BasicSettingsViewModel : ViewModelBase
{
    private string updateRate;
    private string maxTargets;
    private bool airplanes;
    private bool helicopters;
    private bool groundVehicles;
    private bool ships;
    private bool aaMissiles;
    private Visibility saveVisibility = Visibility.Collapsed;

    public string UpdateRate
    {
        get => updateRate;
        set
        {
            SaveVisibility = Visibility.Visible;
            updateRate = value;
            OnPropertyChanged();
        }
    }
    public string MaxTargets
    {
        get => maxTargets;
        set
        {
            SaveVisibility = Visibility.Visible;
            maxTargets = value;
            OnPropertyChanged();
        }
    }
    public bool Airplanes
    {
        get => airplanes;
        set
        {
            SaveVisibility = Visibility.Visible;
            airplanes = value;
            OnPropertyChanged();
        }
    }
    public bool Helicopters
    {
        get => helicopters;
        set
        {
            SaveVisibility = Visibility.Visible;
            helicopters = value;
            OnPropertyChanged();
        }
    }
    public bool GroundVehicles
    {
        get => groundVehicles;
        set
        {
            SaveVisibility = Visibility.Visible;
            groundVehicles = value;
            OnPropertyChanged();
        }
    }
    public bool Ships
    {
        get => ships;
        set
        {
            SaveVisibility = Visibility.Visible;
            ships = value;
            OnPropertyChanged();
        }
    }
    public bool AAMissiles
    {
        get => aaMissiles;
        set
        {
            SaveVisibility = Visibility.Visible;
            aaMissiles = value;
            OnPropertyChanged();
        }
    }
    public Visibility SaveVisibility
    {
        get => saveVisibility;
        set
        {
            saveVisibility = value;
            OnPropertyChanged();
        }
    }

    public ICommand SaveCommand { get; set; }

    public BasicSettingsViewModel()
    {
        updateRate = App.Settings.BasicExportSettings.UpdateRate.ToString();
        maxTargets = App.Settings.BasicExportSettings.MaxTargets.ToString();
        airplanes = App.Settings.BasicExportSettings.Airplanes;
        helicopters = App.Settings.BasicExportSettings.Helicopters;
        groundVehicles = App.Settings.BasicExportSettings.GroundVehicles;
        ships = App.Settings.BasicExportSettings.Ships;
        aaMissiles = App.Settings.BasicExportSettings.AAMissiles;
        SaveCommand = new RelayCommand(OnSaveCommand);
    }

    private void OnSaveCommand()
    {
        App.Settings.BasicExportSettings.UpdateRate = int.Parse(updateRate);
        App.Settings.BasicExportSettings.MaxTargets = int.Parse(maxTargets);
        App.Settings.BasicExportSettings.Airplanes = airplanes;
        App.Settings.BasicExportSettings.Helicopters = helicopters;
        App.Settings.BasicExportSettings.GroundVehicles = groundVehicles;
        App.Settings.BasicExportSettings.Ships = ships;
        App.Settings.BasicExportSettings.AAMissiles = aaMissiles;
        App.SaveSettings();
        SaveVisibility = Visibility.Collapsed;
    }
}
