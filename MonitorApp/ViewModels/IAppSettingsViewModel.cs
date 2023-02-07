using MonitorApp.Domain.Models;

namespace MonitorApp.ViewModels;

public interface IAppSettingsViewModel
{
    public AppMonitorSettings? Settings { get; set; }
}