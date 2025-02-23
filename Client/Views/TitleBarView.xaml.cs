using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Client.Views
{
    /// <summary>
    /// Interaction logic for TitleBar.xaml
    /// </summary>
    public partial class TitleBarView : UserControl
    {

        private DateTime titleBorderDoubleClickTime = DateTime.MinValue;
        private int doubleClickSpeed = 250;

        public TitleBarView()
        {
            InitializeComponent();
        }

        public string TitleText
        {
            get => TitleBarTextBlock.Text;
            set => TitleBarTextBlock.Text = value;
        }

        private void MenuButtonClick(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            if (window is MainWindowView mainWindow)
            {
                mainWindow.MenuPopup.IsOpen =! mainWindow.MenuPopup.IsOpen;
            }
        }

        private void TitleBarMouseDown(object sender, MouseButtonEventArgs e)
        {
            var window = Window.GetWindow(this);
            if (window is MainWindowView mainWindow)
            {
                mainWindow.TitleBarMouseDown(sender, e);
                var currentTime = DateTime.Now;
                var timeSpan = currentTime - titleBorderDoubleClickTime;

                if (timeSpan.TotalMilliseconds <= doubleClickSpeed)
                {
                    MaximizeButtonClick(sender, e);
                }
                titleBorderDoubleClickTime = currentTime;
            }
        }

        private void MinimizeButtonClick(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            if (window is MainWindowView mainWindow)
            {
                mainWindow.WindowState = WindowState.Minimized;
            }
        }

        private void MaximizeButtonClick(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            if (window is MainWindowView mainWindow)
            {
                if (window.WindowState == WindowState.Maximized)
                {
                    MaximizeButton.ToolTip = "Maximize";
                    mainWindow.MainWindowGrid.Margin = new Thickness(0);
                    MaximizeButton.Tag = "pack://application:,,,/Images/Maximize.png";
                    window.WindowState = WindowState.Normal;
                }
                else
                {
                    MaximizeButton.ToolTip = "Restore";
                    mainWindow.MainWindowGrid.Margin = new Thickness(8,8,8,0);
                    MaximizeButton.Tag = "pack://application:,,,/Images/MaximizeMinimize.png";
                    window.WindowState = WindowState.Maximized;
                }
            }
        }

        private void CloseButtonClick(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
    }
}
