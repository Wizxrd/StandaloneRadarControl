using Client.Managers;
using Common.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Client.UI.Common.Messages;

// Which "chat" a message belongs to
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
        messages.Add(new ChatMessage(channel, text, foreground));
    }

    public void AddInfoMessage(string text)
    {
        text = $"{text}";
        AddMessage(text, MessageChannel.All, Brushes.Yellow);
    }

    public void AddMessageFromATC(string text, MessageChannel channel)
    {
        text = $"[ATC] {text}";
        AddMessage(text, channel, Brushes.LimeGreen);
    }

    public void AddMessageFromServer(string text)
    {
        text = $"[SERVER] {text}";
        AddMessage(text, MessageChannel.All, Brushes.LightSalmon);
    }

    public void AddErrorMessage(string text)
    {
        text = $"[ERROR] {text}";
        AddMessage(text, MessageChannel.All, Brushes.Red);
    }
}
