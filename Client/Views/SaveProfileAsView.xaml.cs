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
using Client.Models;

namespace Client.Views
{
    /// <summary>
    /// Interaction logic for SaveProfileAsView.xaml
    /// </summary>
    public partial class SaveProfileAsView : Window
    {
        public SaveProfileAsView()
        {
            InitializeComponent();
            ProfileNameTextBox.Focus();
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

        private void ProfileNameTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            if (ProfileNameTextBox.Text == string.Empty)
            {
                SaveProfileButton.IsEnabled = false;
                SaveProfileButton.Background = (Brush)Application.Current.Resources["GrayBackgroundDark"];
                return;
            }
            SaveProfileButton.IsEnabled = true;
            SaveProfileButton.Background = (Brush)Application.Current.Resources["GrayBackgroundLight"];
        }

        private void ProfileNameTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SaveProfileAsButtonClick(sender, e);
            }
        }

        private async void SaveProfileAsButtonClick(object sender, RoutedEventArgs e)
        {
            if (ProfileNameTextBox.Text == string.Empty) return;
            await Profile.New(ProfileNameTextBox.Text, this.Owner);
            this.Close();
        }
    }
}
