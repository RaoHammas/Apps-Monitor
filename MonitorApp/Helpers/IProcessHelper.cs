using System.Collections.Generic;
using MonitorApp.Domain.Models;

namespace MonitorApp.Helpers;

public interface IProcessHelper
{
    /// <summary>
    /// Gets all running processes and applications
    /// </summary>
    /// <returns></returns>
    IEnumerable<AppToMonitor> GetAllRunningApplications();

    /// <summary>
    /// Get current session Id
    /// </summary>
    /// <returns></returns>
    int GetCurrentProcessId();
}