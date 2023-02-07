using CommunityToolkit.Mvvm.ComponentModel;
using MonitorApp.Domain.Models;

namespace MonitorApp.ViewModels;

public partial class AppSettingsViewModel : ObservableObject, IAppSettingsViewModel
{
    [ObservableProperty] private AppMonitorSettings? _settings;

    public AppSettingsViewModel()
    {
    }
}