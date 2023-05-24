
using Microsoft.Data.Sqlite;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;

namespace Calebs.Data.KeyValueRepo.SqlLite;

public class SchemaValidator
{
    ILogger<SchemaValidator> _logger;

    public SchemaValidator(ILogger<SchemaValidator> Logger)
    {
        _logger = Logger ?? throw new ArgumentNullException(nameof(Logger));
    }

    public async Task<bool> TablesExists(string TableName, SqliteConnection DbConnection)
    {
        DbConnection.ConfirmOpen();
        
        var sql = $@"SELECT name FROM sqlite_master
                     WHERE type='table' AND name = $table_name;";

        try
        {
            using var connection = DbConnection;
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("$table_name", TableName);

            using var reader = await command.ExecuteReaderAsync();
            while (reader.Read())
            {
                var name = reader.GetString(0);
                Debug.Assert(name == TableName);
                if (name == TableName)
                {
                    return true;
                }
            }
            return false;
        }
        catch (SqliteException ex)
        {
            _logger.LogError("Error validating Default Table: {0}", ex.Message);
            throw;
        }
    }

    public async Task<bool> CreateAllTables(KeyValueSqlLiteOptions Options, SqliteConnection DbConnection)
    {
        var TotalResult =  await CreateTable(Options.DefaultTableName, Options , DbConnection);

        if (TotalResult)
        {
            foreach (var tableName in Options.NonDefaultTableMapping.Values)
            {
               var thisResult = await CreateTable(tableName, Options, DbConnection);
                if(!thisResult)
                {
                    TotalResult = thisResult;
                }
            }
        }

        return TotalResult;
    }

    public async Task<bool> CreateTable(string TableName, KeyValueSqlLiteOptions Options, SqliteConnection DbConnection)
    {
        DbConnection.ConfirmOpen();

        var sql = createTableSql(TableName, Options);

        try
        {
            using var connection = DbConnection;
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = sql;

            await command.ExecuteNonQueryAsync();
            return true;

        }
        catch (SqliteException ex)
        {
            _logger.LogError("Error validating Default Table: {0}", ex.Message);
            throw;
        }
        catch (DbException ex)
        {
            _logger.LogError("Error validating Default Table: {0}", ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error validating Default Table: {0}", ex.Message);
            throw;
        }
    }
    public static string createTableSql(string TableName, KeyValueSqlLiteOptions opt)
    {
        var auditColumnsSql = (!opt.UseAuditFields) ? string.Empty : $@",
                    {opt.ColumnPrefix + opt.CreateByColumnName} TEXT,
                    {opt.ColumnPrefix + opt.CreateOnColumnName} INTEGER NOT NULL,
                    {opt.ColumnPrefix + opt.UpdatedByColumnName} TEXT,
                    {opt.ColumnPrefix + opt.UpdatedOnColumnName} INTEGER NOT NULL ";

        var createTableSql = $@"
            CREATE TABLE {TableName} ({opt.ColumnPrefix + opt.KeyColumnName} TEXT NOT NULL,
                    {opt.ColumnPrefix + opt.TypeValueColumnName} TEXT NOT NULL,
                    {opt.ColumnPrefix + opt.ValueColumnName} TEXT { auditColumnsSql }  );";
                    // Primary Key ({ opt.ColumnPrefix + opt.KeyColumnName }, { opt.ColumnPrefix + opt.TypeValueColumnName }));";

        Debug.Print($"Sql: {createTableSql} ");

        return createTableSql;
    }

}