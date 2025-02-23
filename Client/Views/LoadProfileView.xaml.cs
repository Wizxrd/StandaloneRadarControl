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
using System.Windows.Shapes;
using Client.ViewModels;
using Client.Models;
using System.ComponentModel;

namespace Client.Views
{
    /// <summary>
    /// Interaction logic for LoadProfileView.xaml
    /// </summary>
    public partial class LoadProfileView : Window
    {
        Navigate navigate;
        public LoadProfileView(Navigate navigate)
        {
            InitializeComponent();
            this.navigate = navigate;
            this.DataContext = new LoadProfileViewModel(navigate);
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
            Application.Current.Shutdown();
        }

        private void SearchBoxGotFocus(object sender, RoutedEventArgs e)
        {
            PlaceholderText.Visibility = Visibility.Hidden;
        }

        private void SearchBoxLostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(SearchBox.Text))
            {
                PlaceholderText.Visibility = Visibility.Visible;
            }
        }

    }
}
