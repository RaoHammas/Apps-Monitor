namespace MonitorApp.Domain.Models;

public class AppToMonitor
{
    public int Id { get; set; }
    public int PID { get; set; }
    public int SessionId { get; set; }
    public string AppName { get; set; } = string.Empty;
    public string ProcessName { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? StoppedAt { get; set; }
    public AppStatus Status { get; set; } = AppStatus.Running;
}