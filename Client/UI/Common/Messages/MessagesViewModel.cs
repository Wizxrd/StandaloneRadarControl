using Client.Managers;
using Common.Mvvm;
using SignalR.Client;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Client.UI.Common.Messages;

public enum MessageChannel
{
    All,
    Allies
}

public class ChatMessage
{
    public MessageChannel Channel { get; }
    public DateTime Timestamp { get; }
    public string Text { get; }
    public Brush Foreground { get; }

    public ChatMessage(MessageChannel channel, string text, Brush foreground)
    {
        Channel = channel;
        Text = text;
        Timestamp = DateTime.UtcNow;
        Foreground = foreground;
    }

    public string DisplayText => $"[{Timestamp:HH:mm:ss}] {Text}";
}

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
        messages = new();
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

    private void OnSendMessageCommand(object _)
    {
        if (string.IsNullOrWhiteSpace(MessageText))
            return;

        AddMessage(MessageText, SelectedChannel, Brushes.Cyan);
        MessageText = string.Empty;
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
        if (SignalRClient.Connection == null || SignalRClient.Connection.State == Microsoft.AspNetCore.SignalR.Client.HubConnectionState.Disconnected)
        {
            text = "[ERROR] Not connected to server";
            messages.Add(new ChatMessage(MessageChannel.All, text, Brushes.Red));
            if (SelectedChannel != MessageChannel.All)
                HasUnreadAll = true;
            return;
        }

        messages.Add(new ChatMessage(channel, text, foreground));

        if (channel == MessageChannel.All && SelectedChannel != MessageChannel.All)
            HasUnreadAll = true;

        if (channel == MessageChannel.Allies && SelectedChannel != MessageChannel.Allies)
            HasUnreadAllies = true;
    }

    public void AddInfoMessage(string text)
    {
        AddMessage(text, MessageChannel.All, Brushes.Yellow);
    }

    public void AddMessageFromATC(string text, MessageChannel channel)
    {
        AddMessage($"[ATC] {text}", channel, Brushes.LimeGreen);
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

