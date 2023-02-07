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
    public int Save(AppToMonitor app)
    {
        using IDbConnection con = new SqliteConnection(_connectionString);

        const string query = @"REPLACE INTO AppsToMonitor (PID, SessionId, AppName, ProcessName, Status, StartedAt, StoppedAt)  
            VALUES 
            (@PID, @SessionId, @AppName, @ProcessName, @Status, @StartedAt, @StoppedAt); 
            
            SELECT last_insert_rowid();";


        var insertedId = con.QuerySingle<int>(query, app);

        return insertedId;
    }

    ///<inheritdoc />
    public bool Remove(int id)
    {
        using IDbConnection con = new SqliteConnection(_connectionString);
        string query = @"DELETE FROM AppsToMonitor WHERE Id = @Id;";
        var rowsAffected = con.Execute(query, new
        {
            Id = id
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