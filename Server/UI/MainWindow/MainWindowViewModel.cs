using AdonisUI;
using Common.Mvvm;
using Newtonsoft.Json;
using Server.Models;
using Server.UI.Controls.ExportSettings.Realistic;
using Server.UI.Views.Controls;
using Common.Utils;
using SignalR.Server;
using System.IO;
using System.Windows;
using System.Windows.Input;
namespace Server.UI.MainWindow;

public class MainWindowViewModel : ViewModelBase
{
    public SignalRServer Server { get; set; } = new();

    private string startStopText = "START";
    private string clients = "0";
    private string port = "-";

    private bool basicExport;
    private bool advancedExport;
    private bool realisticExport;
    private bool simultaneousConnect;
    private string maxRedControllers;
    private string maxBlueControllers;
    private string adminPassword;
    private string redPassword;
    private string bluePassword;

    private object exportSettingsContent;

    public string StartStopText
    {
        get => startStopText;
        set
        {
            startStopText = value;
            OnPropertyChanged();
        }
    }
    public string Clients => CommandHub.ClientCount.ToString();
    public string Port
    {
        get => App.Settings.GeneralSettings.Ports.SignalRServer.ToString();
        set
        {
            port = value;
            OnPropertyChanged();
        }
    }

    public bool BasicExport
    {
        get => basicExport;
        set
        {
            if (AdvancedExport == true) AdvancedExport = false;
            if (RealisticExport == true) RealisticExport = false;
            basicExport = value;            
            LoadBasicSettingsView();
            OnPropertyChanged();
        }
    }
    public bool AdvancedExport
    {
        get => advancedExport;
        set
        {
            if (BasicExport == true) BasicExport = false;
            if (RealisticExport == true) RealisticExport = false;
            advancedExport = value;
            LoadAdvancedSettingsView();
            OnPropertyChanged();
        }
    }
    public bool RealisticExport
    {
        get => realisticExport;
        set
        {
            if (BasicExport == true) BasicExport = false;
            if (AdvancedExport == true) AdvancedExport = false;
            realisticExport = value;            
            LoadRealisticSettingsView();
            OnPropertyChanged();
        }
    }
    public bool SimultaneousConnect
    {
        get => simultaneousConnect;
        set
        {
            simultaneousConnect = value;            
            OnPropertyChanged();
        }
    }
    public string MaxRedControllers
    {
        get => maxRedControllers;
        set
        {
            maxRedControllers = value;            
            OnPropertyChanged();
        }
    }
    public string MaxBlueControllers
    {
        get => maxBlueControllers;
        set
        {
            maxBlueControllers = value;            
            OnPropertyChanged();
        }
    }
    public string AdminPassword
    {
        get => adminPassword;
        set
        {
            adminPassword = value;            
            OnPropertyChanged();
        }
    }
    public string RedPassword
    {
        get => redPassword;
        set
        {
            redPassword = value;            
            OnPropertyChanged();
        }
    }
    public string BluePassword
    {
        get => bluePassword;
        set
        {
            bluePassword = value;
            OnPropertyChanged();
        }
    }
    public object ExportSettingsContent
    {
        get => exportSettingsContent;
        set
        {
            exportSettingsContent = value;
            OnPropertyChanged();
        }
    }

    public ICommand StartStopCommand { get; set; }
    public ICommand SaveCommand { get; set; }

    public MainWindowViewModel()
    {

        App.MainWindowViewModel = this;

        basicExport = App.Settings.GeneralSettings.ExportMethod.Basic;
        advancedExport = App.Settings.GeneralSettings.ExportMethod.Advanced;
        realisticExport = App.Settings.GeneralSettings.ExportMethod.Realistic;
        simultaneousConnect = App.Settings.GeneralSettings.SimultaneousConnect;
        maxRedControllers = App.Settings.GeneralSettings.MaxControllers.Red.ToString();
        maxBlueControllers = App.Settings.GeneralSettings.MaxControllers.Blue.ToString();
        adminPassword = App.Settings.GeneralSettings.Passwords.Admin;
        redPassword = App.Settings.GeneralSettings.Passwords.Red;
        bluePassword = App.Settings.GeneralSettings.Passwords.Blue;

        InitExportSettingsContent();

        CommandHub.ClientCountChanged += OnClientCountChanged;

        StartStopCommand = new RelayCommand(OnStartStopCommand);
        SaveCommand = new RelayCommand(OnSaveCommand);
    }

    public void InitExportSettingsContent()
    {
        if (App.Settings.GeneralSettings.ExportMethod.Basic) LoadBasicSettingsView();
        else if (App.Settings.GeneralSettings.ExportMethod.Advanced) LoadAdvancedSettingsView();
        else if (App.Settings.GeneralSettings.ExportMethod.Realistic) LoadRealisticSettingsView();
        else ExportSettingsContent = null;
    }

    public void LoadBasicSettingsView()
    {
        ExportSettingsContent = new BasicSettingsView();
    }

    public void LoadAdvancedSettingsView()
    {
        ExportSettingsContent = new AdvancedSettingsView();
    }

    public void LoadRealisticSettingsView()
    {
        ExportSettingsContent = new RealisticSettingsView();
    }

    private void OnClientCountChanged()
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            OnPropertyChanged(nameof(Clients));
        });
    }

    private async void OnStartStopCommand()
    {
        if (Server.Started)
        {
            Server.Stop();
            StartStopText = "START";
        }
        else
        {
            await Server.Start(App.Settings.GeneralSettings.Ports.SignalRServer);
            StartStopText = "STOP";
        }
    }

    private void OnSaveCommand()
    {
        App.Settings.GeneralSettings.ExportMethod.Basic = basicExport;
        App.Settings.GeneralSettings.ExportMethod.Advanced = advancedExport;
        App.Settings.GeneralSettings.ExportMethod.Realistic = realisticExport;
        App.Settings.GeneralSettings.SimultaneousConnect = simultaneousConnect;
        App.Settings.GeneralSettings.MaxControllers.Red = int.Parse(maxRedControllers);
        App.Settings.GeneralSettings.MaxControllers.Blue = int.Parse(maxBlueControllers);
        App.Settings.GeneralSettings.Passwords.Admin = adminPassword;
        App.Settings.GeneralSettings.Passwords.Red = redPassword;
        App.Settings.GeneralSettings.Passwords.Blue = bluePassword;
        App.SaveSettings();
    }
}
