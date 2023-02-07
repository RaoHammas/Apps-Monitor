namespace MonitorApp.Domain.Models;

public class AppMonitorSettings
{
    public int Id { get; set; }
    public int AppId { get; set; }
    public bool MonitorProcessName { get; set; }
    public bool MonitorWindowName { get; set; }
    public bool MonitorPID { get; set; }
    public bool TryRestarting { get; set; }
    public int RestartingAttempts { get; set; }
    public bool SendAlertEmail { get; set; }
    public string EmailAddress { get; set; } = string.Empty;
    public DateTime UpdatedDateTime { get; set; }
}