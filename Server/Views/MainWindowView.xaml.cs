using System.Windows;
using System.Windows.Input;
using Server.ViewModels;
using Server.ViewModels.Network;
using Server.ViewModels.Utils;

namespace Server.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindowView : Window
    {
        private IDataExporterHandler tcpExportHandler;
        private IDataImporterHandler udpImportHandler;
        public MainWindowView()
        {
            InitializeComponent();
            Logger.Wipe();
            tcpExportHandler = new TcpExportHandler(this);
            udpImportHandler = new UdpImportHandler(this);
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
            tcpExportHandler.StopDataExportHandler();
            udpImportHandler.StopDataImportHandler();
            Application.Current.Shutdown();
        }

        private async void StartServerButtonClick(object sender, RoutedEventArgs e)
        {
            if (StartServerButton.Tag.ToString() == "-1")
            {
                bool tcpServerStarted = tcpExportHandler.StartDataExportHandler();
                bool udpServerStarted = udpImportHandler.StartDataImportHandler();
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
                tcpExportHandler.StopDataExportHandler();
                udpImportHandler.StopDataImportHandler();
                UpdateClientPortTextBox(string.Empty);
            }
        }

        public void UpdateClientPortTextBox(string port)
        {
            PortTextBlock.Text = $"Port: {port}";
        }
    }
}
