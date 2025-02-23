using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using Client.Views;
using Client.Models;

namespace Client.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private Cursor? customCursor;
        public MainControlButtonViewModel MainControlButtonViewModel { get; }

        public MainWindowViewModel()
        {
            Logger.Wipe();
            MainControlButtonViewModel = new MainControlButtonViewModel();
            InitializeCursor();
        }

        public Cursor CustomCursor
        {
            get { return customCursor; }
            set
            {
                if (customCursor != value)
                {
                    customCursor = value;
                    OnPropertyChanged(nameof(CustomCursor));
                }
            }
        }

        private void InitializeCursor()
        {
            try
            {
                var cursorUri = new Uri("pack://application:,,,/Cursors/Cross1.cur");
                var customCursor = new Cursor(Application.GetResourceStream(cursorUri).Stream);
                CustomCursor = customCursor;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
