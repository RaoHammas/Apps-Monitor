using MonitorApp.Domain.Models;

namespace MonitorApp.DataAccess.Services;

public interface IAppMonitorDbService
{
    /// <summary>
    /// Save app in database
    /// </summary>
    /// <param name="app">AppToMonitor</param>
    /// <returns>True if saved successfully else false</returns>
    bool Save(AppToMonitor app);

    /// <summary>
    /// Removes an app from monitoring table
    /// </summary>
    /// <param name="app">AppToMonitor</param>
    /// <returns>True if successfully deleted else false</returns>
    bool Remove(AppToMonitor app);

    /// <summary>
    /// Gets an app by ID
    /// </summary>
    /// <param name="id">AppToMonitor Id</param>
    /// <returns>AppToMonitor or null if not found</returns>
    AppToMonitor Get(int id);

    /// <summary>
    /// Gets all apps being monitored
    /// </summary>
    /// <returns>IEnumerable of AppToMonitor</returns>
    IEnumerable<AppToMonitor> GetAll();
}