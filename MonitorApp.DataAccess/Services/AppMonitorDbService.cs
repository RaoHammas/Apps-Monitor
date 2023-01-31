using System.Data;
using Dapper;
using Microsoft.Data.Sqlite;
using MonitorApp.Domain.Models;

namespace MonitorApp.DataAccess.Services;

/// <summary>
/// AppMonitorDbService Class
/// </summary>
public class AppMonitorDbService : IAppMonitorDbService
{
    private readonly string _connectionString;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="connectionHelper"></param>
    public AppMonitorDbService(IConnectionHelper connectionHelper)
    {
        _connectionString = connectionHelper.GetConnectionString();
    }

    ///<inheritdoc />
    public bool Save(AppToMonitor app)
    {
        using IDbConnection con = new SqliteConnection(_connectionString);

        string query =
            @"Insert into AppsToMonitor (PID, SessionId, AppName, ProcessName, Status, StartedAt, StoppedAt)  
                values (@PID, @SessionId, @AppName, @ProcessName, @Status, @StartedAt, @StoppedAt)";
        if (app.Id > 0)
        {
            query =
                @"UPDATE AppsToMonitor SET PID = @PID, SessionId = @SessionId, AppName = @AppName, ProcessName = @ProcessName, Status = @Status
                  StartedAt = @StartedAt, StoppedAt = @StoppedAt 
                  WHERE Id = @Id;";
        }

        var rowsAffected = con.Execute(query, app);

        if (rowsAffected > 0)
        {
            return true;
        }

        return false;
    }

    ///<inheritdoc />
    public bool Remove(AppToMonitor app)
    {
        using IDbConnection con = new SqliteConnection(_connectionString);
        string query = @"DELETE FROM AppsToMonitor WHERE Id = @Id;";
        var rowsAffected = con.Execute(query, new
        {
            app.Id
        });

        if (rowsAffected > 0)
        {
            return true;
        }

        return false;
    }

    ///<inheritdoc />
    public AppToMonitor Get(int id)
    {
        using IDbConnection con = new SqliteConnection(_connectionString);
        string query = "SELECT * FROM AppsToMonitor;";

        return con.QueryFirstOrDefault<AppToMonitor>(query, new DynamicParameters());
    }

    ///<inheritdoc />
    public IEnumerable<AppToMonitor> GetAll()
    {
        using IDbConnection con = new SqliteConnection(_connectionString);
        string query = "SELECT * FROM AppsToMonitor;";

        return con.Query<AppToMonitor>(query, new DynamicParameters());
    }
}