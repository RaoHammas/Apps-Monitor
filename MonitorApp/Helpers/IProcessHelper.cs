using System.Collections.Generic;
using MonitorApp.Domain.Models;

namespace MonitorApp.Helpers;

public interface IProcessHelper
{
    /// <summary>
    /// Gets all running processes and applications
    /// </summary>
    /// <returns>IEnumerable of AppToMonitor object</returns>
    IEnumerable<AppToMonitor> GetAllRunning();

    /// <summary>
    /// Gets a running Process
    /// </summary>
    /// <returns>Returns running process or null if not found</returns>
    AppToMonitor? Get(AppToMonitor process);

    /// <summary>
    /// Get current session Id
    /// </summary>
    /// <returns>Session id of current process</returns>
    int GetCurrentProcessSessionId();
}