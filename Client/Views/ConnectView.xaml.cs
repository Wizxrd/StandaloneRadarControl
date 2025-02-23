using Client.Models;
using Client.Network;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Client.Views
{
    /// <summary>
    /// Interaction logic for ConnectView.xaml
    /// </summary>
    public partial class ConnectView : Window
    {
        private MainWindowView mainWindowView;
        public ConnectView(MainWindowView mainWindowView)
        {
            InitializeComponent();
            this.mainWindowView = mainWindowView;
            ServerIPPortTextBox.Focus();
        }

        private void BorderMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void CloseButtonClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void RequiredTextChanged(object sender, TextChangedEventArgs e)
        {
            if (ServerIPPortTextBox.Text != string.Empty && PasswordTextBox.Password != string.Empty)
            {
                ConnectButton.IsEnabled = true;
                ConnectButton.Background = (Brush)FindResource("GrayBackgroundLight");
            }
            else
            {
                ConnectButton.IsEnabled = false;
                ConnectButton.Background = (Brush)FindResource("GrayBackground");
            }
        }

        private void PasswordTextChanged(object sender, RoutedEventArgs e)
        {
            if (ServerIPPortTextBox.Text != string.Empty && PasswordTextBox.Password != string.Empty)
            {
                ConnectButton.IsEnabled = true;
                ConnectButton.Background = (Brush)FindResource("GrayBackgroundLight");
            }
            else
            {
                ConnectButton.IsEnabled = false;
                ConnectButton.Background = (Brush)FindResource("GrayBackground");
            }
        }

        private async void ConnectButtonClick(object sender, RoutedEventArgs e)
        {
            if (ServerIPPortTextBox.Text != string.Empty && PasswordTextBox.Password != string.Empty)
            {
                string regex = @"^(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}):(\d{1,5})$";
                Match match = Regex.Match(ServerIPPortTextBox.Text, regex);
                if (match.Success)
                {
                    string ip = match.Groups[1].Value;
                    int port = int.Parse(match.Groups[2].Value);
                    JObject jObject = new JObject
                    {
                        { "callback", "OnAsyncTryConnect" },
                        { "ip", ip },
                        { "port", port },
                        { "password", PasswordTextBox.Password }
                    };
                    TcpClientHandler tcpClientHandler = new TcpClientHandler(mainWindowView);
                    await tcpClientHandler.AsyncTryConnect(jObject);
                    if (tcpClientHandler.connectionEstablished)
                    {
                        await Sound.Play("Connected.wav");
                        this.Close();
                    }
                }
            }
        }
    }
}
