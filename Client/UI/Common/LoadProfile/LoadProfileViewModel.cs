using Client.Managers;
using Client.Models;
using Client.Services.Interfaces;
using Common.Mvvm;
using Common.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using vFalcon.Services;
namespace Client.UI.Common.LoadProfile;

public class LoadProfileViewModel : ViewModelBase
{
    public IProfileService ProfileService = new ProfileService();

    private DateTime lastClickTime = DateTime.MinValue;
    private readonly TimeSpan doubleClickTimeSpan = TimeSpan.FromMilliseconds(250);
    private string searchQuery = string.Empty;
    private Profile selectedProfile;
    private int selectedIndex = -1;
    private bool isProfileSelected;

    public ObservableCollection<ProfileViewModel> Profiles { get; } = new();
    public ObservableCollection<ProfileViewModel> FilteredProfiles { get; } = new();
    public ProfileViewModel? LastSelectedProfileVM { get; set; }
    public Profile? LastSelectedProfile { get; set; }


    public bool IsProfileSelected
    {
        get => isProfileSelected;
        set { isProfileSelected = value; OnPropertyChanged(); }
    }

    public Profile SelectedProfile
    {
        get => selectedProfile;
        set { selectedProfile = value; OnPropertyChanged(); }
    }

    public string SearchQuery
    {
        get => searchQuery;
        set
        {
            if (searchQuery != value)
            {
                searchQuery = value;
                OnPropertyChanged();
                FilterProfiles();
            }
        }
    }

    public int SelectedIndex
    {
        get => selectedIndex;
        set
        {
            if (selectedIndex != value)
            {
                selectedIndex = value;
                OnPropertyChanged();

                if (selectedIndex >= 0 && selectedIndex < FilteredProfiles.Count)
                    HandleProfileSelection(FilteredProfiles[selectedIndex], userInitiated: false);
            }
        }
    }

    public ICommand SelectProfileCommand { get; set; }
    public ICommand NewProfileCommand { get; set; }
    public ICommand ImportProfileCommand { get; set; }
    public ICommand LoadProfileCommand { get; set; }
    public ICommand RenameProfileCommand { get; set; }
    public ICommand StopRenamingCommand { get; set; }
    public ICommand CopyProfileCommand { get; set; }
    public ICommand ExportProfileCommand { get; set; }
    public ICommand DeleteProfileCommand { get; set; }

    public LoadProfileViewModel()
    {
        SelectProfileCommand = new RelayCommand(OnProfileSelected);
        NewProfileCommand = new RelayCommand(OnNewProfileCommand);
        ImportProfileCommand = new RelayCommand(OnImportProfileCommand);
        LoadProfileCommand = new RelayCommand(OnLoadProfileCommand);
        RenameProfileCommand = new RelayCommand(OnRenameProfileCommand);
        StopRenamingCommand = new RelayCommand(OnStopRenamingCommand);
        CopyProfileCommand = new RelayCommand(OnCopyProfileCommand);
        ExportProfileCommand = new RelayCommand(OnExportProfile);
        DeleteProfileCommand = new RelayCommand(OnDeleteProfile);

        LoadProfiles();
    }

    private void OnProfileSelected(object obj)
    {
        if (obj is ProfileViewModel selected)
            HandleProfileSelection(selected, userInitiated: true);
    }

    private void OnNewProfileCommand()
    {
        ViewManager.OpenNewProfileView();
        LoadProfiles();
    }

    private async void OnImportProfileCommand()
    {
        bool imported = await ProfileService.Import();
        if (imported) LoadProfiles();
    }

    private async void OnLoadProfileCommand()
    {
        SelectedProfile.LastUsedAt = DateTime.UtcNow;
        await ProfileService.SaveAsync(SelectedProfile);
        ViewManager.InitLoadProfile(SelectedProfile);
        //FIXME load profile here
    }

    private async void OnRenameProfileCommand()
    {
        if (LastSelectedProfileVM == null || !LastSelectedProfileVM.IsRenaming)
        {
            LastSelectedProfileVM?.BeginRename();
            return;
        }

        LastSelectedProfileVM.IsRenaming = false;

        if (LastSelectedProfileVM.Name != LastSelectedProfileVM.OriginalName)
        {
            await ProfileService.Rename(LastSelectedProfileVM.OriginalName, LastSelectedProfileVM.Name);
            LoadProfiles();
        }
    }

    private async void OnStopRenamingCommand(object obj)
    {
        if (obj is not TextBox textBox)
        {
            OnLoadProfileCommand();
            return;
        }
        var profile = Profiles.FirstOrDefault(p => p.IsRenaming);
        if (profile == null) return;
        Logger.Debug("3", "3");
        if (string.IsNullOrWhiteSpace(textBox.Text))
        {
            profile.IsRenaming = false;
            profile.Name = profile.OriginalName;
            return;
        }

        if (profile.Name == profile.OriginalName)
        {
            profile.IsRenaming = false;
            return;
        }

        await ProfileService.Rename(profile.OriginalName, textBox.Text);
        profile.IsRenaming = false;
        LoadProfiles();
    }

    private async void OnCopyProfileCommand()
    {
        if (SelectedProfile == null) return;
        await ProfileService.Copy(SelectedProfile);
        LoadProfiles();
    }

    private void OnExportProfile(object obj)
    {
        if (SelectedProfile == null) return;
        ProfileService.Export(SelectedProfile);
    }

    private async void OnDeleteProfile(object obj)
    {
        if (SelectedProfile == null) return;

        var confirmed = Message.Confirm($"Are you sure you want to delete profile: \"{SelectedProfile.Name}\"");
        if (!confirmed) return;

        await ProfileService.Delete(SelectedProfile);
        LoadProfiles();
    }

    public async void LoadProfiles()
    {
        Profiles.Clear();
        FilteredProfiles.Clear();
        UnselectProfiles();
        LastSelectedProfileVM = null;

        var loadedProfiles = await ProfileService.GetProfiles();
        if (!loadedProfiles.Any()) return;

        foreach (var profile in loadedProfiles)
        {
            Profiles.Add(new ProfileViewModel(profile));
        }

        var sorted = Profiles.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase).ToList();
        Profiles.Clear();
        foreach (var p in sorted)
            Profiles.Add(p);
        FilterProfiles();

        int mostRecentIndex = -1;
        DateTime latest = DateTime.MinValue;

        for (int i = 0; i < FilteredProfiles.Count; i++)
        {
            DateTime when = FilteredProfiles[i].Model.LastUsedAt;
            if (when > latest)
            {
                latest = when;
                mostRecentIndex = i;
            }
        }

        if (mostRecentIndex >= 0)
        {
            SelectedIndex = mostRecentIndex;
            HandleProfileSelection(FilteredProfiles[mostRecentIndex], false);
        }
    }

    private void UnselectProfiles()
    {
        IsProfileSelected = false;
        foreach (var profile in FilteredProfiles)
        {
            profile.IsSelected = false;
            break;
        }
    }

    public async void HandleProfileSelection(ProfileViewModel selected, bool userInitiated)
    {
        DateTime now = DateTime.Now;
        UnselectProfiles();
        foreach (var profile in FilteredProfiles)
            profile.IsSelected = false;

        selected.IsSelected = true;
        IsProfileSelected = true;
        SelectedProfile = selected.Model;
        LastSelectedProfileVM = selected;
        if (userInitiated && (now - lastClickTime) <= doubleClickTimeSpan)
            OnLoadProfileCommand();

        lastClickTime = now;
    }

    private void FilterProfiles()
    {
        FilteredProfiles.Clear();
        string query = SearchQuery?.ToLower() ?? string.Empty;

        foreach (var profile in Profiles)
        {
            if (string.IsNullOrWhiteSpace(query) || profile.Name.ToLower().Contains(query))
                FilteredProfiles.Add(profile);
        }
    }
}