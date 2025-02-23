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

namespace Client.Views
{
    /// <summary>
    /// Interaction logic for DeleteProfileView.xaml
    /// </summary>
    public partial class DeleteProfileView : Window
    {

        public bool DeletionConfirmed = false;

        public DeleteProfileView()
        {
            InitializeComponent();
            NoButton.Focus();
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

        private void YesButtonClick(object sender, RoutedEventArgs e)
        {
            DeletionConfirmed = true;
            this.Close();
        }

        public void SetMessageTextBlock(string message, string profileName)
        {
            if (profileName == string.Empty)
            {
                MessageTextBlock.Text = message;
            }
            else
            {
                MessageTextBlock.Text = $"{message} \"{profileName}\"";
            }
            MessageTextBlock.Measure(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity));
            if (MessageTextBlock.DesiredSize.Width > this.Width)
            {
                this.Width = MessageTextBlock.DesiredSize.Width;
            }
        }
    }
}
