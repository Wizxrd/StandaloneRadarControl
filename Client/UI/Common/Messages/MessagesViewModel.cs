using Client.Managers;
using Client.SignalR;
using Common.Mvvm;
using Common.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Common.Utils;
using System.Windows;
namespace Client.UI.Common.Messages;

public class MessagesViewModel : ViewModelBase
{
    private readonly ObservableCollection<ChatMessage> messages;
    public ICollectionView FilteredMessages { get; }

    private string messageText;
    public string MessageText
    {
        get => messageText;
        set
        {
            if (messageText == value) return;
            messageText = value;
            OnPropertyChanged();
        }
    }

    private MessageChannel selectedChannel = MessageChannel.All;
    public MessageChannel SelectedChannel
    {
        get => selectedChannel;
        set
        {
            if (selectedChannel == value) return;
            selectedChannel = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsAllSelected));
            OnPropertyChanged(nameof(IsAlliesSelected));
            switch (selectedChannel)
            {
                case MessageChannel.All:
                    HasUnreadAll = false;
                    break;
                case MessageChannel.Allies:
                    HasUnreadAllies = false;
                    break;
            }

            FilteredMessages.Refresh();
        }
    }

    public bool IsAllSelected
    {
        get => SelectedChannel == MessageChannel.All;
        set
        {
            if (value)
                SelectedChannel = MessageChannel.All;
        }
    }

    public bool IsAlliesSelected
    {
        get => SelectedChannel == MessageChannel.Allies;
        set
        {
            if (value)
                SelectedChannel = MessageChannel.Allies;
        }
    }

    private bool hasUnreadAll;
    public bool HasUnreadAll
    {
        get => hasUnreadAll;
        set
        {
            if (hasUnreadAll == value) return;
            hasUnreadAll = value;
            OnPropertyChanged();
        }
    }

    private bool hasUnreadAllies;
    public bool HasUnreadAllies
    {
        get => hasUnreadAllies;
        set
        {
            if (hasUnreadAllies == value) return;
            hasUnreadAllies = value;
            OnPropertyChanged();
        }
    }

    public ICommand SendMessageCommand { get; }
    public ICommand ShowAllCommand { get; }
    public ICommand ShowAlliesCommand { get; }
    public ICommand CloseMessagesCommand { get; }
    public ICommand OpenConnectCommand { get; }

    public MessagesViewModel()
    {
        messages = new ObservableCollection<ChatMessage>();
        FilteredMessages = CollectionViewSource.GetDefaultView(messages);
        FilteredMessages.Filter = FilterMessage;

        SendMessageCommand = new RelayCommand(OnSendMessageCommand, CanSendMessage);
        ShowAllCommand = new RelayCommand(_ => SelectedChannel = MessageChannel.All);
        ShowAlliesCommand = new RelayCommand(_ => SelectedChannel = MessageChannel.Allies);
        CloseMessagesCommand = new RelayCommand(_ => ViewManager.MessagesView?.Close());
        OpenConnectCommand = new RelayCommand(_ => ViewManager.OpenConnectView());
    }

    private bool CanSendMessage(object _)
    {
        return !string.IsNullOrWhiteSpace(MessageText);
    }

    private async void OnSendMessageCommand(object _)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(MessageText))
                return;
            await SignalRClient.AsyncSendCommand("AtcChat", new ChatMessage(SelectedChannel, App.ServerBookmark.Callsign, MessageText, Brushes.LimeGreen));
            MessageText = string.Empty;
        }
        catch (Exception ex)
        {
            Logger.Error("MessagesViewModel.OnSendMessageCommand", ex.ToString());
        }
    }

    private bool FilterMessage(object obj)
    {
        if (obj is not ChatMessage msg)
            return false;

        return SelectedChannel switch
        {
            MessageChannel.All => msg.Channel == MessageChannel.All,
            MessageChannel.Allies => msg.Channel == MessageChannel.Allies,
            _ => true
        };
    }

    public void AddMessage(string text, MessageChannel channel, Brush foreground)
    {
        if (!Application.Current.Dispatcher.CheckAccess())
        {
            Application.Current.Dispatcher.Invoke(() => AddMessage(text, channel, foreground));
            return;
        }

        if (SignalRClient.Connection == null ||
            SignalRClient.Connection.State ==
            Microsoft.AspNetCore.SignalR.Client.HubConnectionState.Disconnected)
        {
            text = "[ERROR] Not connected to server";
            messages.Add(new ChatMessage(
                MessageChannel.All,
                App.ServerBookmark?.Callsign ?? string.Empty,
                text,
                Brushes.Red));

            if (SelectedChannel != MessageChannel.All)
                HasUnreadAll = true;

            FilteredMessages.Refresh();
            return;
        }

        messages.Add(new ChatMessage(channel, string.Empty, text, foreground));

        if (channel == MessageChannel.All && SelectedChannel != MessageChannel.All)
            HasUnreadAll = true;

        if (channel == MessageChannel.Allies && SelectedChannel != MessageChannel.Allies)
            HasUnreadAllies = true;

        FilteredMessages.Refresh();
    }

    public void AddInfoMessage(string text)
    {
        AddMessage(text, MessageChannel.All, Brushes.Yellow);
    }

    public void AddMessageFromATC(string sender, string text, MessageChannel channel)
    {
        Brush foreground = Brushes.LimeGreen;
        if (sender == App.ServerBookmark.Callsign) foreground = Brushes.Cyan;
        AddMessage($"[ATC] [{sender}] {text}", channel, foreground);
    }

    public void AddMessageFromServer(string text)
    {
        AddMessage($"[SERVER] {text}", MessageChannel.All, Brushes.LightSalmon);
    }

    public void AddErrorMessage(string text)
    {
        AddMessage($"[ERROR] {text}", MessageChannel.All, Brushes.Red);
    }
}

