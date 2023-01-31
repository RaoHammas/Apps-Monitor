﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;
using MonitorApp.DataAccess.Services;
using MonitorApp.Domain.Models;
using MonitorApp.Helpers;

namespace MonitorApp.ViewModels;

public partial class ShellViewModel : ObservableObject, IShell
{
    private readonly int _currentSessionId;
    private readonly IProcessHelper _processHelper;
    private readonly IAppMonitorDbService _dbService;
    [ObservableProperty] private ISnackbarMessageQueue _snackbarMessageQueue;
    [ObservableProperty] private ObservableCollection<AppToMonitor> _allRunningApps = new();
    [ObservableProperty] private ObservableCollection<AppToMonitor> _allRunningAppsShown = new();
    [ObservableProperty] private ObservableCollection<AppToMonitor> _monitoringApps = new();
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _showAppsOnly;
    [ObservableProperty] private bool _showForCurrentUserOnly;

    /// <summary>
    /// Constructor
    /// </summary>
    public ShellViewModel(IProcessHelper processHelper, IAppMonitorDbService dbService,
        ISnackbarMessageQueue snackbarMessageQueue)
    {
        _processHelper = processHelper;
        _dbService = dbService;
        _snackbarMessageQueue = snackbarMessageQueue;
        _currentSessionId = processHelper.GetCurrentProcessId();
        IsLoading = false;
        ShowAppsOnly = false;

        LoadAllRunningAppsCommand.Execute(null);
        LoadAllMonitoringApps();
    }

    private void LoadAllMonitoringApps()
    {
        IsLoading = true;
        MonitoringApps = new ObservableCollection<AppToMonitor>();

        foreach (var app in _dbService.GetAll())
        {
            MonitoringApps.Add(app);
        }

        IsLoading = false;
        SnackbarMessageQueue.Enqueue("Apps monitoring list loading completed!");
    }

    [RelayCommand]
    private void LoadAllRunningApps()
    {
        IsLoading = true;
        ShowAppsOnly = false;
        ShowForCurrentUserOnly = false;
        AllRunningApps = new ObservableCollection<AppToMonitor>();

        foreach (var app in _processHelper.GetAllRunningApplications())
        {
            AllRunningApps.Add(app);
        }

        AllRunningAppsShown = AllRunningApps;

        IsLoading = false;
        SnackbarMessageQueue.Enqueue("System apps loading completed!");
    }

    [RelayCommand]
    public void MonitorApp(AppToMonitor? app)
    {
        if (app is not null)
        {
            if (MonitoringApps.Contains(app))
            {
                return;
            }

            if (_dbService.Save(app))
            {
                MonitoringApps.Add(app);
                SnackbarMessageQueue.Enqueue("App is successfully added to the Monitoring list.");
            }
        }
    }

    [RelayCommand]
    public void StopMonitoringApp(AppToMonitor? app)
    {
        if (app is not null)
        {
            if (_dbService.Remove(app))
            {
                MonitoringApps.Remove(app);
                SnackbarMessageQueue.Enqueue("App is successfully removed from the Monitoring list.");
            }
        }
    }

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
}