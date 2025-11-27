using Client.Managers;
using Client.Models;
using Client.Services;
using Client.Services.Interfaces;
using Common.Mvvm;
using Common.Utils;
using SignalR.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

namespace Client.UI.Common.Connect
{
    public class ConnectViewModel : ViewModelBase
    {
        private IServerBookmarkService bookmarkService = new ServerBookmarkService();

        private string searchQuery = string.Empty;
        private string address = string.Empty;
        private string port = string.Empty;
        private string password = string.Empty;
        private string callsign = string.Empty;

        private bool connectEnabled;
        private ServerBookmark selectedBookmark;
        private bool isBookmarkSelected;
        private bool newBookmarkEnabled;
        private bool saveBookmarkEnabled = false;

        private DateTime lastClickTime = DateTime.MinValue;
        private readonly TimeSpan doubleClickTimeSpan = TimeSpan.FromMilliseconds(250);
        private int selectedIndex = -1;

        public ObservableCollection<BookmarkViewModel> Bookmarks { get; } = new();
        public ObservableCollection<BookmarkViewModel> FilteredBookmarks { get; } = new();

        public BookmarkViewModel LastSelectedBookmarkVM { get; set; }
        public BookmarkViewModel? LastSelectedBookmark { get; set; }

        public string SearchQuery
        {
            get => searchQuery;
            set
            {
                if (searchQuery != value)
                {
                    searchQuery = value;
                    OnPropertyChanged();
                    FilterBookmarks();
                }
            }
        }

        public string Address
        {
            get => address;
            set
            {
                address = value;
                OnPropertyChanged();
                UpdateConnectEnabled();
            }
        }

        public string Port
        {
            get => port;
            set
            {
                port = value;
                OnPropertyChanged();
                UpdateConnectEnabled();
            }
        }

        public string Password
        {
            get => password;
            set
            {
                password = value;
                OnPropertyChanged();
                UpdateConnectEnabled();
            }
        }

        public string Callsign
        {
            get => callsign;
            set
            {
                callsign = value;
                OnPropertyChanged();
                UpdateConnectEnabled();
            }
        }

        public bool ConnectEnabled
        {
            get => connectEnabled;
            private set
            {
                if (connectEnabled == value) return;

                connectEnabled = value;
                OnPropertyChanged();
            }
        }

        public ServerBookmark SelectedBookmark
        {
            get => selectedBookmark;
            set
            {
                selectedBookmark = value;
                OnPropertyChanged();
            }
        }

        public bool IsBookmarkSelected
        {
            get => isBookmarkSelected;
            set
            {
                isBookmarkSelected = value;
                OnPropertyChanged();
            }
        }

        public bool NewBookmarkEnabled
        {
            get => newBookmarkEnabled;
            set
            {
                newBookmarkEnabled = value;
                OnPropertyChanged();
            }
        }
        public bool SaveBookmarkEnabled
        {
            get => saveBookmarkEnabled;
            set
            {
                if (SelectedBookmark == null) return;
                saveBookmarkEnabled = value;
                OnPropertyChanged();
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

                    if (selectedIndex >= 0 && selectedIndex < FilteredBookmarks.Count)
                        HandleBookmarkSelection(FilteredBookmarks[selectedIndex], userInitiated: false);
                }
            }
        }

        public ICommand SelectBookmarkCommand { get; set; }
        public ICommand NewBookmarkCommand { get; set; }
        public ICommand RenameBookmarkCommand { get; set; }
        public ICommand StopRenamingCommand { get; set; }
        public ICommand CopyBookmarkCommand { get; set; }
        public ICommand DeleteBookmarkCommand { get; set; }
        public ICommand SaveBookmarkCommand { get; set; }

        public ICommand ConnectCommand { get; set; }
        public ICommand CancelCommand { get; set; }

        public ConnectViewModel()
        {
            SelectBookmarkCommand = new RelayCommand(OnBookmarkSelected);
            NewBookmarkCommand = new RelayCommand(OnNewBookmarkCommand);
            RenameBookmarkCommand = new RelayCommand(OnRenameBookmarkCommand);
            StopRenamingCommand = new RelayCommand(OnStopRenamingCommand);
            CopyBookmarkCommand = new RelayCommand(OnCopyBookmarkCommand);
            DeleteBookmarkCommand = new RelayCommand(OnDeleteBookmarkCommand);
            SaveBookmarkCommand = new RelayCommand(OnSaveBookmarkCommand);

            ConnectCommand = new RelayCommand(OnConnectCommand);
            CancelCommand = new RelayCommand(OnCancelCommand);

            LoadBookmarks();
        }

        private void OnBookmarkSelected(object obj)
        {
            if (obj is BookmarkViewModel selected)
                HandleBookmarkSelection(selected, userInitiated: true);
        }

        private async void OnNewBookmarkCommand()
        {
            await bookmarkService.New(Address, Port, Password, Callsign);
            LoadBookmarks();
        }

        private async void OnRenameBookmarkCommand()
        {
            if (LastSelectedBookmarkVM == null || !LastSelectedBookmarkVM.IsRenaming)
            {
                LastSelectedBookmarkVM?.BeginRename();
                return;
            }

            LastSelectedBookmarkVM.IsRenaming = false;

            if (LastSelectedBookmarkVM.Name != LastSelectedBookmarkVM.OriginalName)
            {
                await bookmarkService.Rename(LastSelectedBookmarkVM.OriginalName, LastSelectedBookmarkVM.Name);
                LoadBookmarks();
            }
        }

        private async void OnStopRenamingCommand(object obj)
        {
            if (obj is not TextBox textBox)
                return;

            var bookmark = Bookmarks.FirstOrDefault(p => p.IsRenaming);
            if (bookmark == null) return;

            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                bookmark.IsRenaming = false;
                bookmark.Name = bookmark.OriginalName;
                return;
            }

            if (bookmark.Name == bookmark.OriginalName)
            {
                bookmark.IsRenaming = false;
                return;
            }

            await bookmarkService.Rename(bookmark.OriginalName, textBox.Text);
            bookmark.IsRenaming = false;
            LoadBookmarks();
        }

        private async void OnCopyBookmarkCommand()
        {
            if (SelectedBookmark == null) return;
            await bookmarkService.Copy(SelectedBookmark);
            LoadBookmarks();
        }

        private async void OnDeleteBookmarkCommand(object obj)
        {
            if (SelectedBookmark == null) return;

            var confirmed = Message.Confirm($"Are you sure you want to delete bookmark: \"{SelectedBookmark.Name}\"");
            if (!confirmed) return;

            await bookmarkService.Delete(SelectedBookmark);
            LoadBookmarks();
        }
        
        private async void OnSaveBookmarkCommand()
        {
            if (SelectedBookmark == null) return;

            SelectedBookmark.Address = Address;
            SelectedBookmark.Port = Port;
            SelectedBookmark.Password = Password;
            SelectedBookmark.Callsign = Callsign;

            await bookmarkService.Save(SelectedBookmark);
            LoadBookmarks();
        }

        private async void OnConnectCommand()
        {
            ViewManager.MessagesViewModel.AddInfoMessage($"Connecting to http://{Address}:{Port}");
            bool connected = await SignalRClient.AsyncConnect(Address, Port, Password, Callsign);
            if (connected)
            {
                ViewManager.MessagesViewModel.AddInfoMessage($"Connection established");
                ViewManager.ConnectView?.Close();
                return;
            }
            ViewManager.MessagesViewModel.AddErrorMessage($"Connection failed");
        }

        private void OnCancelCommand()
        {
            ViewManager.ConnectView?.Close();
        }

        private void UpdateConnectEnabled()
        {
            bool allFilled =
                !string.IsNullOrWhiteSpace(Address) &&
                !string.IsNullOrWhiteSpace(Port) &&
                !string.IsNullOrWhiteSpace(Password) &&
                !string.IsNullOrWhiteSpace(Callsign);

            ConnectEnabled = allFilled;
            NewBookmarkEnabled = allFilled;
            SaveBookmarkEnabled = allFilled;
        }

        private void LoadBookmarks()
        {
            SaveBookmarkEnabled = false;
            Bookmarks.Clear();
            FilteredBookmarks.Clear();
            UnselectBookmarks();
            LastSelectedBookmarkVM = null;

            List<ServerBookmark> bookmarks = bookmarkService.GetBookmarks();
            if (!bookmarks.Any())
            {
                Address = string.Empty;
                Port = string.Empty;
                Password = string.Empty;
                Callsign = string.Empty;
                return;
            }

            foreach (ServerBookmark bookmark in bookmarks)
                Bookmarks.Add(new BookmarkViewModel(bookmark));

            var sorted = Bookmarks.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase).ToList();
            Bookmarks.Clear();
            foreach (var p in sorted)
                Bookmarks.Add(p);

            FilterBookmarks();

            int mostRecentIndex = -1;
            DateTime latest = DateTime.MinValue;

            for (int i = 0; i < FilteredBookmarks.Count; i++)
            {
                DateTime when = FilteredBookmarks[i].Model.LastUsedAt;
                if (when > latest)
                {
                    latest = when;
                    mostRecentIndex = i;
                }
            }

            if (mostRecentIndex >= 0)
            {
                SelectedIndex = mostRecentIndex;
                HandleBookmarkSelection(FilteredBookmarks[mostRecentIndex], false);
            }
        }

        private void UnselectBookmarks()
        {
            IsBookmarkSelected = false;
            foreach (BookmarkViewModel bookmark in FilteredBookmarks)
                bookmark.IsSelected = false;
        }

        private void HandleBookmarkSelection(BookmarkViewModel selected, bool userInitiated)
        {
            DateTime now = DateTime.Now;

            UnselectBookmarks();
            foreach (BookmarkViewModel bookmark in FilteredBookmarks)
                bookmark.IsSelected = false;

            selected.IsSelected = true;
            IsBookmarkSelected = true;
            SelectedBookmark = selected.Model;
            LastSelectedBookmarkVM = selected;
            Address = SelectedBookmark.Address;
            Port = SelectedBookmark.Port;
            Password = SelectedBookmark.Password;
            Callsign = SelectedBookmark.Callsign;
            SaveBookmarkEnabled = false;
            if (userInitiated && (now - lastClickTime) <= doubleClickTimeSpan)
                OnConnectCommand();

            lastClickTime = now;
        }

        private void FilterBookmarks()
        {
            FilteredBookmarks.Clear();
            string query = SearchQuery?.ToLower() ?? string.Empty;

            foreach (var bookmark in Bookmarks)
            {
                if (string.IsNullOrWhiteSpace(query) || bookmark.Name.ToLower().Contains(query))
                    FilteredBookmarks.Add(bookmark);
            }
        }
    }
}
