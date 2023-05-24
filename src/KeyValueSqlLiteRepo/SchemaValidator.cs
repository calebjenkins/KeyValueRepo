
using Microsoft.Data.Sqlite;
using System.Data;
using System.Diagnostics;

namespace Calebs.Data.KeyValueRepo.SqlLite;

public class SchemaValidator
{
    KeyValueSqlLiteOptions _options;
    ILogger<SchemaValidator> _logger;
    SqliteConnection _db;

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

    public static string createDefaultTableSql(KeyValueSqlLiteOptions opt)
    {
        var createTableSql = $@"
            CREATE TABLE {opt.DefaultTableName} (
                    {opt.ColumnPrefix + opt.KeyColumnName} TEXT PRIMARY KEY,
                    {opt.ColumnPrefix + opt.TypeValueColumnName} TEXT PRIMARY KEY,
                    {opt.ColumnPrefix + opt.ValueColumnName} TEXT ";

        if (opt.UseAuditFields)
        {
            createTableSql += $@"
                    , {opt.ColumnPrefix + opt.CreateByColumnName} TEXT,
                    {opt.ColumnPrefix + opt.CreateOnColumnName} INTEGER,
                    {opt.ColumnPrefix + opt.UpdatedByColumnName} TEXT,
                    {opt.ColumnPrefix + opt.UpdatedOnColumnName} INTEGER
                ";
        }

        createTableSql += $@" ); ";

        return createTableSql;
    }

}