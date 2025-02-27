using System.Windows;
using Server.Models;

namespace Server;

/// <summary>
///   Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
	public App()
	{
		// this is an intentional MVVM deviation.
		// This is the earliest we can effectively wipe the Logger, so it should happen here.
		Logger.Wipe();
	}
}