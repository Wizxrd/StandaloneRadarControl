using Server.Models;
using Server.Network;
using System.Windows;
using System.Windows.Input;

namespace Server.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindowView : Window
    {
        private TcpServerHandler tcpServerHandler;
        private UdpServerHandler udpServerHandler;
        public MainWindowView()
        {
            InitializeComponent();
            Logger.Wipe();
            tcpServerHandler = new(this);
            udpServerHandler = new(this);
        }

        public void TitleBarMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void MinimizeButtonClick(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        private async void CloseButtonClick(object sender, RoutedEventArgs e)
        {
            await tcpServerHandler.StopAsync();
            udpServerHandler.Stop();
            Application.Current.Shutdown();
        }

        private async void StartServerButtonClick(object sender, RoutedEventArgs e)
        {
            if (StartServerButton.Tag.ToString() == "-1")
            {
                bool tcpServerStarted = tcpServerHandler.Start();
                bool udpServerStarted = udpServerHandler.Start();
                if(tcpServerStarted && udpServerStarted)
                {
                    StartServerButton.Tag = "1";
                    StartServerButton.Content = "STOP";
                }
            }
            else
            {
                StartServerButton.Tag = "-1";
                StartServerButton.Content = "START";
                await tcpServerHandler.StopAsync();
                udpServerHandler.Stop();
                UpdateClientPortTextBox(string.Empty);
            }
        }

        public void UpdateClientPortTextBox(string port)
        {
            PortTextBlock.Text = $"Port: {port}";
        }
    }
}
