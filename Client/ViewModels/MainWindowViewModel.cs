using System.Windows;
using System.Windows.Input;
using Client.Models;

namespace Client.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
	private Cursor? customCursor;

	public MainWindowViewModel()
	{
		Logger.Wipe();
		MainControlButtonViewModel = new MainControlButtonViewModel();
		InitializeCursor();
	}

	public MainControlButtonViewModel MainControlButtonViewModel { get; }

	public Cursor CustomCursor
	{
		get => customCursor;
		set
		{
			if (customCursor != value)
			{
				customCursor = value;
				OnPropertyChanged();
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