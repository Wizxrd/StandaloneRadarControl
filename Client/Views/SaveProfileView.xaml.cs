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
    /// Interaction logic for SaveProfileView.xaml
    /// </summary>
    public partial class SaveProfileView : Window
    {
        Navigate navigate;
        public SaveProfileView(Navigate navigate)
        {
            InitializeComponent();
            this.navigate = navigate;
            MessageTextBlock.Text = $"Save Profile: {navigate.mainWindowView.CurrentProfile?.Name ?? string.Empty}";
            YesButton.Focus();
        }

        private void BorderMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private async void YesButtonClick(object sender, RoutedEventArgs e)
        {
            if (navigate.mainWindowView.CurrentProfile == null) return;
            await Profile.Save(navigate.mainWindowView.CurrentProfile.Name, navigate.mainWindowView);
            this.Close();
        }

        private void CloseButtonClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
