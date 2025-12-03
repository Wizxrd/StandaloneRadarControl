using Client.Models;
using Client.UI.Common.Connect;
using Client.UI.Common.LoadProfile;
using Client.UI.Common.Messages;
using Client.UI.Common.NewProfile;
using Client.UI.Displays.Tactical;
using Client.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Common.Utils;
namespace Client.Managers;

public static class ViewManager
{
    public static NewProfileView NewProfileView { get; set; }
    public static ConnectView ConnectView { get; set; }
    public static MessagesView? MessagesView { get; private set; }
    public static MessagesViewModel MessagesViewModel { get; } = new();

    public static bool? OpenNewProfileView()
    {
        if (NewProfileView != null) return true;
        NewProfileView = new();
        NewProfileView.Owner = Application.Current.MainWindow;
        NewProfileView.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        NewProfileView.Closing += (_, __) => NewProfileView = null;
        return NewProfileView.ShowDialog();
    }

    public static async Task<bool?> OpenConnectView()
    {
        if (SignalRClient.Connection != null && SignalRClient.Connection?.State == Microsoft.AspNetCore.SignalR.Client.HubConnectionState.Connected)
        {
            bool confirmed = Message.Confirm("Disconnect from the server?", "Confirm");
            if (confirmed)
            {
                await SignalRClient.AsyncDisconnect();
                return false;
            }
        }
        if (ConnectView != null) return true;
        ConnectView = new();
        ConnectView.Owner = Application.Current.MainWindow;
        ConnectView.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        ConnectView.Closing += (_, __) => ConnectView = null;
        return ConnectView.ShowDialog();
    }

    public static void OpenMessagesView()
    {
        if (!Application.Current.Dispatcher.CheckAccess())
        {
            Application.Current.Dispatcher.Invoke(OpenMessagesView);
            return;
        }

        if (MessagesView != null)
        {
            MessagesView.Close();
            return;
        }

        MessagesView = new MessagesView
        {
            Owner = Application.Current.MainWindow,
            DataContext = MessagesViewModel
        };

        MessagesView.Closed += (_, __) => MessagesView = null;

        MessagesView.Show();
    }

    public static void InitLoadProfile(Profile profile)
    {
        var loadProfileView = Application.Current.MainWindow as LoadProfileView;

        App.Profile = profile;

        for (int i = 0; i < profile.DisplayWindows.Count; i++)
        {
            DisplayWindow window = profile.DisplayWindows[i];

            if (window.DisplaySettings.Type == "Tactical")
            {
                var tacticalView = new TacticalView
                {
                    Id = i
                };
                Application.Current.MainWindow = tacticalView;
                tacticalView.Title.Text = $"SRC : TAC : {i+1}";
                App.Displays.Add(tacticalView.ViewModel);
                tacticalView.Show();
            }
        }
        loadProfileView?.Close();
    }
}
