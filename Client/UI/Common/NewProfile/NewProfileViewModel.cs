using Client.Managers;
using Common.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using vFalcon.Services;
namespace Client.UI.Common.NewProfile;

public class NewProfileViewModel : ViewModelBase
{
    private ProfileService profileService = new();

    private string selectedDisplayType = string.Empty;
    private bool positionEnabled = false;
    private string selectedPosition = string.Empty;
    private string profileName = string.Empty;
    private bool profileNameEnabled = false;
    private bool saveEnabled = false;

    public ObservableCollection<string> initialPositions = new();

    public ObservableCollection<string> InitialPositions
    {
        get => initialPositions;
        set
        {
            initialPositions = value;
            OnPropertyChanged();
        }
    }
    public string SelectedDisplayType
    {
        get => selectedDisplayType;
        set
        {
            selectedDisplayType = value;
            PositionEnabled = true;
            LoadPositions();
            SelectedPosition = string.Empty;
            ProfileNameEnabled = false;
            ProfileName = string.Empty;
            OnPropertyChanged();
        }
    }
    public bool PositionEnabled
    {
        get => positionEnabled;
        set
        {
            positionEnabled = value;
            OnPropertyChanged();
        }
    }
    public string SelectedPosition
    {
        get => selectedPosition;
        set
        {
            selectedPosition = value;
            ProfileNameEnabled = true;
            OnPropertyChanged();
        }
    }
    public string ProfileName
    {
        get => profileName;
        set
        {
            profileName = value;
            if (profileName != string.Empty) SaveEnabled = true;
            else SaveEnabled = false;
            OnPropertyChanged();
        }
    }
    public bool ProfileNameEnabled
    {
        get => profileNameEnabled;
        set
        {
            profileNameEnabled = value;
            OnPropertyChanged();
        }
    }
    public bool SaveEnabled
    {
        get => saveEnabled;
        set
        {
            saveEnabled = value;
            OnPropertyChanged();
        }
    }

    public ICommand CreateProfileCommand { get; set; }
    public ICommand CancelCommand { get; set; } 

    public NewProfileViewModel()
    {
        CreateProfileCommand = new RelayCommand(OnCreateProfileCommand);
        CancelCommand = new RelayCommand(OnCancelCommand);
    }

    private async void OnCreateProfileCommand()
    {
        await profileService.New(SelectedDisplayType, SelectedPosition, ProfileName);
        ViewManager.NewProfileView.Close();
    }

    private void OnCancelCommand()
    {
        ViewManager.NewProfileView.Close();
    }

    private void LoadPositions()
    {
        InitialPositions.Clear();
        if (SelectedDisplayType == "Tactical")
        {
            InitialPositions.Add("C2");
        }
        else if (SelectedDisplayType == "Terminal")
        {
            InitialPositions.Add("Approach");
            InitialPositions.Add("Final");
            InitialPositions.Add("Departure");
        }
        else if (SelectedDisplayType == "Tower")
        {
            InitialPositions.Add("Local");
            InitialPositions.Add("Ground");
            InitialPositions.Add("Delivery");
        }
    }
}
