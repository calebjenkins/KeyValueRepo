
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using Calebs.Extensions;

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
        var TotalResult = await CreateTable(Options.DefaultTableName, Options, DbConnection);

        if (TotalResult)
        {
            foreach (var tableName in Options.NonDefaultTableMapping.Values)
            {
                var thisResult = await CreateTable(tableName, Options, DbConnection);
                if (!thisResult)
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
        catch (Exception ex)
        {
            _logger.LogError("Error validating Default Table: {0}", ex.Message);
            throw;
        }
    }

    public async Task<(bool HasError, IList<string> Messages)> ValidateSchema(KeyValueSqlLiteOptions Options, SqliteConnection DbConnection)
    {
        bool hasErrors = false;
        IList<string> results = new List<string>();

        try
        {
            DbConnection.ConfirmOpen();

            // Check Existance of All Tables
            foreach (var table in Options.AllTables())
            {
                var table_exists = await TablesExists(table, DbConnection);
                if (!table_exists)
                {
                    // if (Options.CreateTableIfMissing) // We always do this. 
                    results.Add($"Table {table} is missing.. attempting to add");

                    var created = await CreateTable(table, Options, DbConnection);
                    table_exists = await TablesExists(table, DbConnection);

                    if (!table_exists)
                    {
                        hasErrors = true;
                        results.Add($"Tried to create table {table} did not work.");
                    }
                    else
                    {
                        results.Add($"Succesfully created table {table}");
                    }
                }

                var validationResult = await ValidateTableSchema(table, Options, DbConnection);
                if(validationResult.HasError)
                {
                    hasErrors = true;
                }

                results.AddRange(validationResult.Messages);
            }
            return (hasErrors, results);

        }
        catch (Exception ex)
        {
            _logger.LogError($"Error validating Schema - {ex.Message}");
            return (true, results);
        }
        finally
        {
            await DbConnection.CloseAsync();
        }
    }

    private async Task<(bool HasError, IList<string> Messages)> ValidateTableSchema(string TableName, KeyValueSqlLiteOptions Options, SqliteConnection DbConnection)
    {
        bool hasErrors = false;
        IList<string> results = new List<string>();

        var sql = $@"SELECT * FROM sqlite_schema";

        try
        {
            using var connection = DbConnection;
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = sql;

            using var reader = await command.ExecuteReaderAsync();
            while (reader.Read())
            {
                var col1 = reader.GetString(0);
                var tableSql = reader.GetString(4);
                // Pause for effect.
                var columns = Options.AllColumnsWithPrefix();

                foreach (var c in columns)
                {
                    var result = CheckString(tableSql, c);
                    if (result.Missing && hasErrors == false)
                    {
                        hasErrors = true;
                    }

                    results.Add(result.Message);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error trying to validate schema for table {TableName} - {ex.Message}");
            throw;
        }
        finally
        {
            DbConnection.Close();
        }

        return (hasErrors, results);
    }

    private (bool Missing, string Message) CheckString(string SourceText, string SearchString)
    {
        bool notFound = !SourceText.Contains(SearchString, StringComparison.OrdinalIgnoreCase);
        string foundText = (notFound) ? "not " : string.Empty;
        string msg = $"{SearchString} was {foundText}found.";

        return (notFound, msg);
    }
    public static string createTableSql(string TableName, KeyValueSqlLiteOptions opt)
    {
        // var auditColumnsSql = (!opt.UseAuditFields) ? string.Empty : $@", // We always use Audit Fields
                   

        var createTableSql = $@"
            CREATE TABLE IF NOT EXISTS {TableName} ({opt.ColumnPrefix + opt.KeyColumnName} TEXT NOT NULL,
                    {opt.ColumnPrefix + opt.TypeValueColumnName} TEXT NOT NULL,
                    {opt.ColumnPrefix + opt.ValueColumnName} TEXT,
                    {opt.ColumnPrefix + opt.CreateByColumnName} TEXT,
                    {opt.ColumnPrefix + opt.CreateOnColumnName} INTEGER NOT NULL,
                    {opt.ColumnPrefix + opt.UpdatedByColumnName} TEXT,
                    {opt.ColumnPrefix + opt.UpdatedOnColumnName} INTEGER NOT NULL) ; ";
        // Primary Key ({ opt.ColumnPrefix + opt.KeyColumnName }, { opt.ColumnPrefix + opt.TypeValueColumnName }));";

        Debug.Print($"Sql: {createTableSql} ");

        return createTableSql;
    }

}