using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;
using MonitorApp.DataAccess.Services;
using MonitorApp.Domain.Models;
using MonitorApp.Helpers;

namespace MonitorApp.ViewModels;

public partial class ShellViewModel : ObservableObject, IShell, IDisposable
{
    private readonly int _currentSessionId;
    private readonly IProcessHelper _processHelper;
    private readonly IAppMonitorDbService _dbService;
    private readonly Timer _monitorTimer;

    [ObservableProperty] private ISnackbarMessageQueue _snackbarMessageQueue;
    [ObservableProperty] private ObservableCollection<AppToMonitor> _allRunningApps = new();
    [ObservableProperty] private ObservableCollection<AppToMonitor> _allRunningAppsShown = new();
    [ObservableProperty] private ObservableCollection<AppToMonitor> _monitoringApps = new();
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isMonitoringOn;
    [ObservableProperty] private bool _showAppsOnly;
    [ObservableProperty] private bool _showForCurrentUserOnly;
    [ObservableProperty] private bool _isSettingsDialogOpened;
    [ObservableProperty] private IAppSettingsViewModel _appSettingsViewModel;

    public ShellViewModel()
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public ShellViewModel(IProcessHelper processHelper, IAppMonitorDbService dbService,
        ISnackbarMessageQueue snackbarMessageQueue, IAppSettingsViewModel appSettingsViewModel)
    {
        _processHelper = processHelper;
        _dbService = dbService;
        _snackbarMessageQueue = snackbarMessageQueue;
        _appSettingsViewModel = appSettingsViewModel;
        _currentSessionId = processHelper.GetCurrentProcessSessionId();

        IsLoading = false;
        ShowAppsOnly = false;
        IsSettingsDialogOpened = false;
        IsMonitoringOn = true;

        _monitorTimer = new Timer(5000);
        _monitorTimer.Elapsed += MonitorTimerOnElapsed;

        LoadAllRunningApps();
        LoadAllMonitoringApps();
    }

    /// <summary>
    /// Timer which runs and checks running apps.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void MonitorTimerOnElapsed(object? sender, ElapsedEventArgs e)
    {
        await Task.Run(CheckIfMonitoringAppStillRunning);
    }

    /// <summary>
    /// Check if all apps that are being monitored are still up and running
    /// </summary>
    /// <returns>Task</returns>
    private Task CheckIfMonitoringAppStillRunning()
    {
        foreach (var app in MonitoringApps)
        {
            var found = _processHelper.Get(app);
            if (found != null)
            {
                if (found.PID != app.PID)
                {
                    app.PID = found.PID; //means app is running but PID changed
                }
            }
            else
            {
                SnackbarMessageQueue.Enqueue($"{app.ProcessName} Has stopped running!");
            }
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Load all apps list that are being monitored
    /// </summary>
    [RelayCommand]
    public void LoadAllMonitoringApps()
    {
        IsLoading = true;

        MonitoringApps.Clear();
        MonitoringApps = new ObservableCollection<AppToMonitor>(_dbService.GetAll());

        IsLoading = false;
        SnackbarMessageQueue.Enqueue("Apps monitoring list loading completed!");
    }

    /// <summary>
    /// Load all running apps on windows
    /// </summary>
    [RelayCommand]
    public void LoadAllRunningApps()
    {
        IsLoading = true;
        AllRunningApps = new ObservableCollection<AppToMonitor>();

        foreach (var app in _processHelper.GetAllRunning())
        {
            AllRunningApps.Add(app);
        }

        ApplyFilter();

        IsLoading = false;
        SnackbarMessageQueue.Enqueue("System apps loading completed!");
    }

    /// <summary>
    /// Add an app to monitoring list
    /// </summary>
    /// <param name="app"></param>
    [RelayCommand]
    public void MonitorApp(AppToMonitor? app)
    {
        if (app is not null)
        {
            if (MonitoringApps.FirstOrDefault(x => x.ProcessName == app.ProcessName) != null)
            {
                SnackbarMessageQueue.Enqueue("This app is already being monitored!");
                return;
            }

            app.StartedAt = DateTime.Now;
            app.Status = AppStatus.Monitoring;

            var savedId = _dbService.Save(app);
            if (savedId > 0)
            {
                app.Id = savedId;
                MonitoringApps.Add(app);
                SnackbarMessageQueue.Enqueue("App is successfully added to the Monitoring list.");
            }
            else
            {
                SnackbarMessageQueue.Enqueue("FAILED: Something happened at server side. Please try again...");
            }
        }
    }

    /// <summary>
    /// Remove and app from monitoring list
    /// </summary>
    /// <param name="app"></param>
    [RelayCommand]
    public void StopMonitoringApp(AppToMonitor? app)
    {
        if (app is not null)
        {
            if (_dbService.Remove(app.Id))
            {
                MonitoringApps.Remove(app);
                SnackbarMessageQueue.Enqueue("App is successfully removed from the Monitoring list.");
            }
            else
            {
                SnackbarMessageQueue.Enqueue("FAILED: Something happened at server side. Please try again...");
            }
        }
    }

    /// <summary>
    /// Apply filtering on running apps.
    /// Either show only the apps that has a window or show apps for current user only.
    /// </summary>
    [RelayCommand]
    public void ApplyFilter()
    {
        IEnumerable<AppToMonitor> query = AllRunningApps;
        if (ShowAppsOnly)
        {
            query = query.Where(x => !string.IsNullOrEmpty(x.AppName));
        }

        if (ShowForCurrentUserOnly)
        {
            query = query.Where(x => x.SessionId == _currentSessionId);
        }

        AllRunningAppsShown = new ObservableCollection<AppToMonitor>(query);
    }

    /// <summary>
    /// Opens dialog box that shows monitoring settings for the app.
    /// </summary>
    [RelayCommand]
    public void OpenAppSettings(AppToMonitor app)
    {
        AppSettingsViewModel!.Settings = _dbService.GetSettings(app.Id);
        if (AppSettingsViewModel.Settings != null)
        {
            IsSettingsDialogOpened = true;
        }
        else
        {
            SnackbarMessageQueue.Enqueue("FAILED: Unable to fetch settings! Try again...");
        }
    }

    /// <summary>
    /// Turn monitoring on Or off
    /// </summary>
    [RelayCommand]
    public void ToggleMonitoringOnOff()
    {
        if (IsMonitoringOn)
        {
            IsMonitoringOn = false;
            _monitorTimer.Stop();
            SnackbarMessageQueue.Enqueue("Stopped Monitoring!");
        }
        else
        {
            IsMonitoringOn = true;
            _monitorTimer.Start();
            SnackbarMessageQueue.Enqueue("Started Monitoring!");
        }
    }

    public void Dispose()
    {
        _monitorTimer.Dispose();
    }
}