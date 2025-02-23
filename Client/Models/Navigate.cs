using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Client.Views;

namespace Client.Models
{
    public class Navigate
    {

        public MainWindowView mainWindowView { get; set; }

        public Navigate(MainWindowView mainWindowView)
        {
            this.mainWindowView = mainWindowView;
        }

        public static Window? GetOpenDialog()
        {
            return Application.Current.Windows.OfType<Window>().FirstOrDefault(window => window.IsActive);
        }

        public static void OpenConnectView(Navigate navigate)
        {
            ConnectView connectView = new ConnectView(navigate.mainWindowView);
            connectView.Owner = navigate.mainWindowView;
            connectView.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            connectView.ShowDialog();
        }

        public static void OpenNewProfileView(Navigate navigate)
        {
            NewProfileView newProfileView = new();
            newProfileView.Owner = navigate.mainWindowView;
            newProfileView.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            newProfileView.ShowDialog();
        }

        public static void OpenLoadProfileView(Navigate navigate)
        {
            LoadProfileView loadProfileView = new(navigate);
            loadProfileView.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            loadProfileView.ShowDialog();
        }

        public static void OpenSaveProfileView(Navigate navigate)
        {
            SaveProfileView saveProfileView = new(navigate);
            saveProfileView.Owner = navigate.mainWindowView;
            saveProfileView.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            saveProfileView.ShowDialog();
        }

        public static void OpenSaveProfileAsView(Navigate navigate)
        {
            SaveProfileAsView saveProfileAsView = new();
            saveProfileAsView.Owner = navigate.mainWindowView;
            saveProfileAsView.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            saveProfileAsView.ShowDialog();
        }

        public static void OpenGeneralSettingsView(Navigate navigate)
        {
            GeneralSettingsView generalSettingsView = new();
            generalSettingsView.Owner = navigate.mainWindowView;
            generalSettingsView.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            generalSettingsView.ShowDialog();
        }

        public static void OpenDisplaySettingsView(Navigate navigate)
        {
            DisplaySettingsView displaySettingsView = new();
            displaySettingsView.Owner = navigate.mainWindowView;
            displaySettingsView.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            displaySettingsView.ShowDialog();
        }

        public static void OpenHelpView(Navigate navigate)
        {
            HelpView helpView = new();
            helpView.Owner = navigate.mainWindowView;
            helpView.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            helpView.ShowDialog();
        }

        public static void OpenMessagesView(Navigate navigate)
        {
            MessagesView messagesView = new();
            messagesView.Owner = navigate.mainWindowView;
            messagesView.WindowStartupLocation= WindowStartupLocation.CenterOwner;
            messagesView.ShowDialog();
        }
    }
}
