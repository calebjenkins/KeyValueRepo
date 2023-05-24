
namespace Calebs.Data.KeyValueRepo.SqlLite;

public class KeyValueSqlLiteRepo : IKeyValueRepo
{ 
    private ILogger<KeyValueSqlLiteRepo> _logger;
    private KeyValueSqlLiteOptions _options;

    private Microsoft.Data.Sqlite.SqliteConnection _db;

    public KeyValueSqlLiteRepo(ILogger<KeyValueSqlLiteRepo> Logger, KeyValueSqlLiteOptions Options = null)
    {
        _logger = Logger ?? throw new ArgumentNullException(nameof(Logger));
        _options = Options ?? new KeyValueSqlLiteOptions();

        _db = new Microsoft.Data.Sqlite.SqliteConnection(_options.ConnectionString);

        if (_options.ValidateSchemaOnStartUp)
        {
            var validate =  ValidateSchema();
            validate.Wait();
        }
    }

    public SqliteConnection DbConn { get { return _db; } }

    public string DatabaseFileName => Path.GetFullPath(_db.DataSource);


    public async Task<bool> ValidateSchema()
    {
        _logger.LogInformation("Validating KeyValue Database Schema");
        bool validSchema = false;

        await _db.OpenAsync();



        if(_options.CreateTableIfMissing)


        _db.Close();

        return true;
    }
    public async Task ReleaseForCleanUp()
    {
        await _db.CloseAsync();
        await _db.DisposeAsync();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        return;
    }

    public Task<T?> Get<T>(string key) where T : class
    {
        throw new NotImplementedException();
    }

    public Task<IList<T>> GetAll<T>() where T : class
    {
        throw new NotImplementedException();
    }

    public Task Update<T>(string key, T value) where T : class
    {
        throw new NotImplementedException();
    }
}