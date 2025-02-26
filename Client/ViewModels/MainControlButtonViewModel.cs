using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls.Primitives;
using Client.Commands;

namespace Client.ViewModels;

public class MainControlButtonViewModel : ViewModelBase
{
	private int zoomRange;

	public MainControlButtonViewModel()
	{
		ZoomRange = 10;
	}

	public RelayCommand ClipCommand => new(ClipButton);

	public int ZoomRange
	{
		get => zoomRange;
		set
		{
			if (zoomRange != value)
			{
				zoomRange = value;
				OnPropertyChanged();
			}
		}
	}

	[DllImport("user32.dll")]
	private static extern bool ClipCursor(ref RECT lpRect);

	[DllImport("user32.dll")]
	private static extern bool ClipCursor(IntPtr lpRect); // For unclipping the cursor

	private void ClipButton(object parameter)
	{
		var button = parameter as UIElement;
		if (button == null) return;
		var toggleButton = button as ToggleButton;
		if (toggleButton == null) return;
		if (toggleButton.IsChecked == false)
		{
			ClipCursor(IntPtr.Zero);
			return;
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

	[StructLayout(LayoutKind.Sequential)]
	private struct RECT
	{
		public int Left;
		public int Top;
		public int Right;
		public int Bottom;
	}
}