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
        using var transaction = con.BeginTransaction();
        try
        {
            const string query =
                @"REPLACE INTO AppsToMonitor (PID, SessionId, AppName, ProcessName, Status, StartedAt, StoppedAt)  
                        VALUES 
                        (@PID, @SessionId, @AppName, @ProcessName, @Status, @StartedAt, @StoppedAt); 
                        
                        SELECT last_insert_rowid();";

            var insertedAppId = con.QuerySingle<int>(query, app, transaction);

            const string querySettings =
                @"REPLACE INTO AppMonitorSettings (AppId, MonitorProcessName, MonitorWindowName, MonitorPID, TryRestarting, RestartingAttempts, SendAlertEmail, EmailAddress, UpdatedDateTime)  
            VALUES 
            (@AppId, @MonitorProcessName, @MonitorWindowName, @MonitorPID, @TryRestarting, @RestartingAttempts, @SendAlertEmail, @EmailAddress, @UpdatedDateTime); 
            
            SELECT last_insert_rowid();";


            var insertedSettingsId = con.QuerySingle<int>(querySettings, new AppMonitorSettings
            {
                AppId = insertedAppId,
                UpdatedDateTime = DateTime.Now,
                TryRestarting = true,
                SendAlertEmail = false,
                RestartingAttempts = 5,
                MonitorWindowName = true,
                MonitorProcessName = true,
                MonitorPID = true,
                EmailAddress = "",
                Id = 0
            }, transaction);


            transaction.Commit();
            if (insertedAppId < 1 || insertedSettingsId < 1)
            {
                transaction.Rollback();
            }

            return insertedAppId;
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }


        return 0;
    }

    ///<inheritdoc />
    public bool Remove(int id)
    {
        using IDbConnection con = new SqliteConnection(_connectionString);
        using var transaction = con.BeginTransaction();
        try
        {
            var query = @"DELETE FROM AppsToMonitor WHERE Id = @Id;";
            var rowsAffected = con.Execute(query, new { Id = id }, transaction);

            var querySettings = @"DELETE FROM AppMonitorSettings WHERE AppId = @AppId;";
            var rowsAffectedSettings = con.Execute(querySettings, new { AppId = id }, transaction);

            transaction.Commit();
            if (rowsAffected < 1 || rowsAffectedSettings < 1)
            {
                transaction.Rollback();
                return false;
            }

            return true;
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }
    }

    ///<inheritdoc />
    public AppToMonitor Get(int id)
    {
        using IDbConnection con = new SqliteConnection(_connectionString);
        var query = "SELECT * FROM AppsToMonitor WHERE Id = @Id;";

        return con.QueryFirstOrDefault<AppToMonitor>(query, new
        {
            Id = id
        });
    }

    ///<inheritdoc />
    public IEnumerable<AppToMonitor> GetAll()
    {
        using IDbConnection con = new SqliteConnection(_connectionString);
        var query = "SELECT * FROM AppsToMonitor;";

        return con.Query<AppToMonitor>(query);
    }


    ///<inheritdoc />
    public AppMonitorSettings GetSettings(int appId)
    {
        using IDbConnection con = new SqliteConnection(_connectionString);
        var query = "SELECT * FROM AppMonitorSettings WHERE AppId = @AppId;";

        return con.QueryFirstOrDefault<AppMonitorSettings>(query, new
        {
            AppId = appId
        });
    }

    ///<inheritdoc />
    public int SaveSettings(AppMonitorSettings settings)
    {
        using IDbConnection con = new SqliteConnection(_connectionString);

        const string query =
            @"REPLACE INTO AppMonitorSettings (AppId, MonitorProcessName, MonitorWindowName, MonitorPID, TryRestarting, RestartingAttempts, SendAlertEmail, EmailAddress, UpdatedDateTime)  
            VALUES 
            (@AppId, @MonitorProcessName, @MonitorWindowName, @MonitorPID, @TryRestarting, @RestartingAttempts, @SendAlertEmail, @EmailAddress, @UpdatedDateTime); 
            
            SELECT last_insert_rowid();";

        var insertedId = con.QuerySingle<int>(query, settings);

        return insertedId;
    }
}