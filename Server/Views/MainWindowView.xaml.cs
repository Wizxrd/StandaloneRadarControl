using System.Windows;
using System.Windows.Input;
using Server.Models.Utils;
using Server.ViewModels;

namespace Server.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindowView : Window
    {
        private readonly MainWindowViewModel viewModel = new();
        
        public MainWindowView()
        {
            InitializeComponent();
            this.DataContext = viewModel;
            
            Logger.Wipe();
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
        private void CloseButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                viewModel.ExitApplicaiton();
            }
            catch (Exception exception)
            {
                throw;
            }
        }

        private void StartServerButtonClick(object sender, RoutedEventArgs e)
        {
            if (!viewModel.ServerRunning)
            {
                viewModel.StartServer(); 
                Console.WriteLine("Starting server...");
                return;
            }
            viewModel.StopServer();
            Console.WriteLine("Stopping server...");
        }
    }
}
