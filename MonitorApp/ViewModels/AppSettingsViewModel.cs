using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;
using MonitorApp.DataAccess.Services;
using MonitorApp.Domain.Models;

namespace MonitorApp.ViewModels;

public partial class AppSettingsViewModel : ObservableObject, IAppSettingsViewModel
{
    private readonly IAppMonitorDbService _dbService;
    [ObservableProperty] private AppMonitorSettings _settings;
    [ObservableProperty] private ISnackbarMessageQueue _snackbarMessageQueue;

    public AppSettingsViewModel(IAppMonitorDbService dbService, ISnackbarMessageQueue snackbarMessageQueue)
    {
        _settings = new AppMonitorSettings();
        _dbService = dbService;
        _snackbarMessageQueue = snackbarMessageQueue;
    }

    [RelayCommand]
    public void SaveSettings()
    {
        Settings.UpdatedDateTime = DateTime.Now;
        SnackbarMessageQueue.Enqueue(_dbService.SaveSettings(Settings) > 0
            ? "Settings Saved successfully!"
            : "Settings Failed to save. Try again...");
    }
}