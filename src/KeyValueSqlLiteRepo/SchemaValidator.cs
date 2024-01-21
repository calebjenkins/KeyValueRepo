
namespace Calebs.Data.KeyValueRepo.SqlLite;

public class SchemaValidator
{
    ILogger<SchemaValidator> _logger;

    public SchemaValidator(ILogger<SchemaValidator> Logger)
    {
        _logger = Logger ?? throw new ArgumentNullException(nameof(Logger));
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
                if (validationResult.HasError)
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

        return TotalResult;
    }
    public async Task<bool> CreateTable(string TableName, KeyValueSqlLiteOptions Options, SqliteConnection DbConnection)
    {
        DbConnection.ConfirmOpen();

        var sql = createTableSql(TableName, Options);

        try
        {
            var command = DbConnection.CreateCommand();
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

    private async Task<(bool HasError, IList<string> Messages)> ValidateTableSchema(string TableName, KeyValueSqlLiteOptions Options, SqliteConnection DbConnection)
    {
        bool hasErrors = false;
        IList<string> results = new List<string>();

        var sqlSqliteSchema = $@"SELECT * FROM sqlite_schema";

        try
        {
            using var connection = DbConnection;
            connection.Open();

            // Check Table
            var schemaCommand = connection.CreateCommand();
            schemaCommand.CommandText = sqlSqliteSchema;
            using var schemaReader = await schemaCommand.ExecuteReaderAsync();

            while (schemaReader.Read())
            {
                var col1 = schemaReader.GetString(0);
                var tableSql = schemaReader.GetString(4);
                var columns = Options.AllColumnsWithPrefix();

                foreach (var c in columns)
                {
                    var result = CheckSourceForSearchString(tableSql, c);
                    if (result.IsMissing && hasErrors == false)
                    {
                        hasErrors = true;
                    }

                    results.Add(result.Message);
                }

                // Check Support for History (PK or no PK)
                // WITHOUT ROWID = PK = No History
                // No PK = ROWID = With History
                var track = (Options.TrackHistory) ? "" : "not ";
                var pkMsg = $"KeyValueRepo is { track }configured to Track History";

                var DesignedForHistory = CheckSourceForSearchString(tableSql, "WITHOUT ROWID");
                var designForHistoryNot = (DesignedForHistory.IsMissing)? "not " : "";
                var designMsg = (DesignedForHistory.IsMissing)?
                    "There are no PrimaryKeys and instead it it using Sqlite's ROWID Feature - TrackHistory should be true."
                    : "It has a composite Primary Key and was create WITHOUT ROWID - TrackHistory should be false.";

                if (DesignedForHistory.IsMissing != Options.TrackHistory)
                {
                    results.Add($"Error: History Tracking Mismatch. {pkMsg}, but {designMsg}. Try changing the configuration so that TrackHistory is false or Delete this table andre-initialize with TrackHistory set to true.");
                    hasErrors = true;
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

    private static (bool IsMissing, string Message) CheckSourceForSearchString(string SourceText, string SearchString)
    {
        bool notFound = !SourceText.Contains(SearchString, StringComparison.OrdinalIgnoreCase);
        string foundText = (notFound) ? "not " : string.Empty;
        string msg = $"{SearchString} was {foundText}found.";

        return (notFound, msg);
    }
    public static string createTableSql(string TableName, KeyValueSqlLiteOptions opt)
    {
        // Track History: No Primary Key, so we can rely on Insert
        // Not Tracking History, we use PKs since we're relying on constraints for Upserts
        var primaryKey = (opt.TrackHistory) ? $@");" : $@", Primary Key ({opt.ColumnPrefix + opt.KeyColumnName}, {opt.ColumnPrefix + opt.TypeColumnName})
               ) WITHOUT ROWID;";         

        var createTableSql = $@"
            CREATE TABLE {TableName} (
                    {opt.ColumnPrefix + opt.KeyColumnName} TEXT NOT NULL,
                    {opt.ColumnPrefix + opt.TypeColumnName} TEXT NOT NULL,
                    {opt.ColumnPrefix + opt.ValueColumnName} TEXT NOT NULL,
                    {opt.ColumnPrefix + opt.CreateByColumnName} TEXT,
                    {opt.ColumnPrefix + opt.CreateOnColumnName} INTEGER NOT NULL,
                    {opt.ColumnPrefix + opt.UpdatedByColumnName} TEXT,
                    {opt.ColumnPrefix + opt.UpdatedOnColumnName} INTEGER NOT NULL
                  {primaryKey}";

        Debug.Print($"Sql: {createTableSql} ");

        return createTableSql;
    }

}