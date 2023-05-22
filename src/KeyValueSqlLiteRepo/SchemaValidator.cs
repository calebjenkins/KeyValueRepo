
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

    public async Task<bool> TablesExists(KeyValueSqlLiteOptions Options, SqliteConnection DbConnection)
    {
        DbConnection.ConfirmOpen();
        var defaultTable = Options.DefaultTableName;

        var createSqlTable = $"";

        return true;
    }

    public static string checkDefaultTableSql(KeyValueSqlLiteOptions opt)
    {
        return "";
    }
    public static string createDefaultTableSql(KeyValueSqlLiteOptions opt)
    {
        var createSqlTable = $@"
            CREATE TABLE {opt.DefaultTableName} (
                    {opt.KeyColumnName} TEXT PRIMARY KEY,
                    {opt.TypeValueColumnName} TEXT PRIMARY KEY,
                    {opt.ValueColumnName} TEXT ";

        if (opt.UseAuditFields)
        {
            createSqlTable += $@"
                    , {opt.CreateByColumnName} TEXT,
                    {opt.CreateOnColumnName} INTEGER,
                    {opt.UpdatedByColumnName} TEXT,
                    {opt.UpdatedOnColumnName} INTEGER
                ";
        }

        createSqlTable += $@" ); ";

        return createSqlTable;
    }

}