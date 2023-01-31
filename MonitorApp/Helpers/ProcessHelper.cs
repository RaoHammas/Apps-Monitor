using System;
using System.Collections.Generic;
using System.Diagnostics;
using MonitorApp.Domain.Models;

namespace MonitorApp.Helpers;

public class ProcessHelper : IProcessHelper
{
    ///<inheritdoc/>
    public IEnumerable<AppToMonitor> GetAllRunningApplications()
    {
        var processCollection = Process.GetProcesses();
        foreach (var p in processCollection)
        {
            yield return AddProcessToList(p);
        }
    }

    ///<inheritdoc/>
    public int GetCurrentProcessId()
    {
        return Process.GetCurrentProcess().SessionId;
    }


    private AppToMonitor AddProcessToList(Process process)
    {
        return new AppToMonitor
        {
            PID = process.Id,
            SessionId = process.SessionId,
            ProcessName = process.ProcessName,
            AppName = process.MainWindowTitle,
            Status = AppStatus.Running,
            StartedAt = DateTime.Now,
            StoppedAt = null
        };
    }
}