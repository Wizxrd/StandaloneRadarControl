using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using Client.Commands;
using Client.Models;
using Client.Views;
using Client.Controls;

namespace Client.ViewModels
{
    public class LoadProfileViewModel : ViewModelBase
    {
        private Navigate navigate;
        private ToggleButton? selectedProfile = null;

        private DateTime lastClickTime = DateTime.MinValue;
        private readonly TimeSpan doubleClickTimeSpan = TimeSpan.FromMilliseconds(500);

        private string _searchQuery = string.Empty;
        private Window? loadProfileView;

        public ObservableCollection<Profile> Profiles { get; set; } = new();
        public ICollectionView FilteredProfiles { get; }
        public ICommand ProfileSelectedCommand { get; set; }
        public ICommand RenameProfileCommand { get; set; }
        public ICommand StopRenamingCommand { get; set; }
        public ICommand CopyProfileCommand { get; set; }
        public ICommand ExportProfileCommand { get; set; }
        public ICommand DeleteProfileCommand { get; set; }
        public ICommand ImportProfileCommand { get; set; }

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (_searchQuery != value)
                {
                    _searchQuery = value;
                    OnPropertyChanged(nameof(SearchQuery));
                    FilteredProfiles.Refresh(); // Refresh the filter
                }
            }
        }

        public LoadProfileViewModel(Navigate navigate)
        {
            this.navigate = navigate;
            LoadProfiles();
            ProfileSelectedCommand = new RelayCommand(ProfileSelected);
            RenameProfileCommand = new RelayCommand(RenameProfile);
            StopRenamingCommand = new RelayCommand(StopRenaming);
            CopyProfileCommand = new RelayCommand(CopyProfile);
            ExportProfileCommand = new RelayCommand(ExportProfile);
            DeleteProfileCommand = new RelayCommand(DeleteProfile);
            ImportProfileCommand = new RelayCommand(ImportProfile);

            FilteredProfiles = CollectionViewSource.GetDefaultView(Profiles);
            FilteredProfiles.Filter = FilterProfiles;
            loadProfileView = Navigate.GetOpenDialog();
        }

        private bool FilterProfiles(object obj)
        {
            if (obj is Profile profile)
            {
                return string.IsNullOrEmpty(SearchQuery) || profile.Name.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        private void LoadProfiles()
        {
            try
            {
                Profiles.Clear();
                string folderPath = LoadFile.LoadFolder("Profiles");
                string[] files = Directory.GetFiles(folderPath, "*.json");

                var loadedProfiles = new List<Profile>();

                foreach (string file in files)
                {
                    try
                    {
                        string fileContent = File.ReadAllText(file);
                        JObject profile = JObject.Parse(fileContent);
                        string name = profile["Name"]?.ToString() ?? string.Empty;

                        if (!string.IsNullOrEmpty(name))
                        {
                            loadedProfiles.Add(new Profile { Name = name, IsRenaming = false });
                        }
                    }
                    catch (Exception fileEx)
                    {
                        Logger.Error("LoadProfileViewModel.LoadProfiles", $"Failed to load profile from {file}: {fileEx.Message}");
                    }
                }
                var sortedProfiles = loadedProfiles.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase).ToList();
                foreach (var profile in sortedProfiles)
                {
                    Profiles.Add(profile);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("LoadProfileViewModel.LoadProfiles", ex.ToString());
            }
        }

        private async void ProfileSelected(object parameter)
        {
            if (parameter is not ToggleButton toggleButton) return;
            DateTime now = DateTime.Now;
            if (selectedProfile == toggleButton && (now - lastClickTime) <= doubleClickTimeSpan)
            {
                if (toggleButton.DataContext is not Profile profile) return;
                lastClickTime = DateTime.MinValue;
                await Profile.Load(profile.Name, navigate.mainWindowView);
                Navigate.GetOpenDialog()?.Close();
                navigate.mainWindowView.CurrentProfile = profile;
                navigate.mainWindowView.SetTitle(profile.Name);
                navigate.mainWindowView.Show();
                return;
            }
            if (selectedProfile == null)
            {
                selectedProfile = toggleButton;
            }
            else if (toggleButton == selectedProfile)
            {
                selectedProfile.IsChecked = false;
                selectedProfile = null;
            }
            else
            {
                selectedProfile.IsChecked = false;
                selectedProfile = toggleButton;
            }

            lastClickTime = now;
        }


        private async void RenameProfile(object parameter)
        {
            if (parameter is not Profile profile) return;
            if (!profile.IsRenaming)
            {
                profile.IsRenaming = true;
                profile.OldName = profile.Name;
                return;
            }
            profile.IsRenaming = false;
            if (profile.Name != profile.OldName)
            {
                await profile.RenameAsync();
                LoadProfiles();
            }
        }

        private void StopRenaming(object parameter)
        {
            if (parameter is not TextBox textBox) return;
            var profile = Profiles.FirstOrDefault(p => p.IsRenaming);
            if (profile != null)
            {
                profile.Name = textBox.Text;
                RenameProfile(profile);
            }
        }

        private async void CopyProfile(object parameter)
        {
            if (parameter is not Profile profile) return;
            await profile.CopyAsync();
            LoadProfiles();
        }

        private void ExportProfile(object parameter)
        {
            if (parameter is not Profile profile) return;
            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = "Export Profile",
                Filter = "JSON File (*.json)|*.json",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                string path = saveFileDialog.FileName;
                profile.Export(path);
            }
        }

        private async void DeleteProfile(object parameter)
        {
            if (parameter is not Profile profile) return;

            DeleteProfileView deleteProfileView = new DeleteProfileView()
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
            };
            deleteProfileView.SetMessageTextBlock("Are you sure you want to delete profile:", profile.Name);
            deleteProfileView.ShowDialog();
            if (deleteProfileView.DeletionConfirmed)
            {
                await profile.DeleteAsync();
                LoadProfiles();
            }
        }

        private async void ImportProfile(object parameter)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Import Profile",
                Filter = "JSON File (*.json)|*.json",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };
            if (openFileDialog.ShowDialog() == true)
            {
                string file = openFileDialog.FileName;
                if (file.Contains(".json"))
                {
                    await Profile.Import(file);
                    LoadProfiles();
                }
            }
        }
    }
}
