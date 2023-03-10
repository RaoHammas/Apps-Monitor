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
        con.Open();
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
    }

    ///<inheritdoc />
    public bool Remove(int appId)
    {
        using IDbConnection con = new SqliteConnection(_connectionString);
        con.Open();
        using var transaction = con.BeginTransaction();
        try
        {
            var query = @"DELETE FROM AppsToMonitor WHERE Id = @Id;";
            var rowsAffected = con.Execute(query, new { Id = appId }, transaction);

            var querySettings = @"DELETE FROM AppMonitorSettings WHERE AppId = @AppId;";
            var rowsAffectedSettings = con.Execute(querySettings, new { AppId = appId }, transaction);

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
    public AppToMonitor Get(int appId)
    {
        using IDbConnection con = new SqliteConnection(_connectionString);
        var query = "SELECT * FROM AppsToMonitor WHERE Id = @Id;";

        return con.QueryFirstOrDefault<AppToMonitor>(query, new
        {
            Id = appId
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


    ///<inheritdoc />
    public bool SaveSettingsForAllApps(AppMonitorSettings appSettings)
    {
        using IDbConnection con = new SqliteConnection(_connectionString);

        const string query =
            @"UPDATE AppMonitorSettings SET 
            MonitorProcessName = @MonitorProcessName, MonitorWindowName = @MonitorWindowName, MonitorPID = @MonitorPID, TryRestarting = @TryRestarting, 
            RestartingAttempts = @RestartingAttempts, SendAlertEmail = @SendAlertEmail, EmailAddress = @EmailAddress, UpdatedDateTime = @UpdatedDateTime; 
            ";

        var rowsEffected = con.Execute(query, appSettings);

        return rowsEffected > 0;
    }
}