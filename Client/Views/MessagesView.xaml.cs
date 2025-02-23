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
    /// Interaction logic for MessagesView.xaml
    /// </summary>
    public partial class MessagesView : Window
    {
        public MessagesView()
        {
            InitializeComponent();
            MessageSendTextBox.Focus();
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

        private void MessageSendTextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void MessageSendKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (MessageSendTextBox.Text != string.Empty)
                {
                    MessageReceiveTextBox.Text += $"[{DateTime.Now.ToString("HH:mm:ss")}] {MessageSendTextBox.Text}\n";
                    MessageSendTextBox.Text = string.Empty;
                }
            }
        }
    }
}
