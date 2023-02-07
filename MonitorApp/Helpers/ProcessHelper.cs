using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MonitorApp.Domain.Models;

namespace MonitorApp.Helpers;

public class ProcessHelper : IProcessHelper
{
    ///<inheritdoc/>
    public IEnumerable<AppToMonitor> GetAllRunning()
    {
        //Same process can have different windows but they will have same PID. so take distinct!
        var processCollection = Process.GetProcesses().DistinctBy(x => x.Id);
        foreach (var p in processCollection)
        {
            yield return AddProcessToList(p);
        }
    }

    ///<inheritdoc/>
    public AppToMonitor? Get(AppToMonitor process)
    {
        Process[] foundProcesses = Process.GetProcessesByName(process.ProcessName);
        if (foundProcesses.Length < 1)
        {
            //means process is not running any more
            return null;
        }


        var sameByIdAndName =
            foundProcesses.FirstOrDefault(x => x.Id == process.PID && x.MainWindowTitle == process.AppName);
        if (sameByIdAndName != null)
        {
            // means same process is still running and nothing needs to be updated
            return process;
        }

        var sameByName =
            foundProcesses.FirstOrDefault(x => x.MainWindowTitle == process.AppName && x.Id != process.PID);
        if (sameByName != null)
        {
            //means same process by name is running but PID is changed
            process.PID = sameByName.Id;
            return process;
        }

        var sameById =
            foundProcesses.FirstOrDefault(x => x.Id == process.PID && x.MainWindowTitle != process.AppName);
        if (sameById != null)
        {
            //means same process by name is running but PID is changed
            process.AppName = sameById.MainWindowTitle;
            return process;
        }

        //means process is not running any more
        return null;
    }

    ///<inheritdoc/>
    public int GetCurrentProcessSessionId()
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