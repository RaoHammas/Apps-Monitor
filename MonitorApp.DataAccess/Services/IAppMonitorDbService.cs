using MonitorApp.Domain.Models;

namespace MonitorApp.DataAccess.Services;

public interface IAppMonitorDbService
{
    /// <summary>
    /// Save app in database
    /// </summary>
    /// <param name="app">AppToMonitor</param>
    /// <returns>Saved Id if saved successfully else 0</returns>
    int Save(AppToMonitor app);

    /// <summary>
    /// Removes an app from monitoring table
    /// </summary>
    /// <param name="id">Id of AppToMonitor object</param>
    /// <returns>True if successfully deleted else false</returns>
    bool Remove(int id);

    /// <summary>
    /// Gets an app by ID
    /// </summary>
    /// <param name="id">AppToMonitor Id</param>
    /// <returns>AppToMonitor or null if not found</returns>
    AppToMonitor Get(int id);

    /// <summary>
    /// Gets an App settings by it's ID
    /// </summary>
    /// <param name="appId">AppToMonitor Id</param>
    /// <returns>Monitoring settings for the app</returns>
    AppMonitorSettings GetSettings(int appId);

    /// <summary>
    /// Saves an app monitoring settings
    /// </summary>
    /// <param name="appSettings"> AppMonitorSettings object</param>
    /// <returns>saved settings id</returns>
    int SaveSettings(AppMonitorSettings appSettings);

    /// <summary>
    /// Gets all apps being monitored
    /// </summary>
    /// <returns>IEnumerable of AppToMonitor</returns>
    IEnumerable<AppToMonitor> GetAll();
}