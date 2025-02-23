using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls.Primitives;
using Client.Commands;
using Client.Models;

namespace Client.ViewModels
{
    public class MainControlButtonViewModel : ViewModelBase
    {
        [DllImport("user32.dll")]
        private static extern bool ClipCursor(ref RECT lpRect);
        [DllImport("user32.dll")]
        private static extern bool ClipCursor(IntPtr lpRect);  // For unclipping the cursor
        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        public RelayCommand ClipCommand => new RelayCommand(ClipButton);

        public MainControlButtonViewModel()
        {
            ZoomRange = 10;
        }

        private int zoomRange;
        public int ZoomRange
        {
            get { return zoomRange; }
            set
            {
                if (zoomRange != value)
                {
                    zoomRange = value;
                    OnPropertyChanged(nameof(ZoomRange));
                }
            }
        }

        private void ClipButton(object parameter)
        {
            var button = parameter as UIElement;
            if (button == null) return;
            var toggleButton = button as ToggleButton;
            if (toggleButton == null) return;
            if (toggleButton.IsChecked == false)
            {
                ClipCursor(IntPtr.Zero); return;
            }
            var buttonPosition = button.PointToScreen(new Point(0, 0));
            var buttonRect = new RECT
            {
                Left = (int)buttonPosition.X,
                Top = (int)buttonPosition.Y,
                Right = (int)(buttonPosition.X + button.RenderSize.Width),
                Bottom = (int)(buttonPosition.Y + button.RenderSize.Height)
            };
            ClipCursor(ref buttonRect);
        }
    }
}