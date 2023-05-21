
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
    }

    public string DatabaseFileName => Path.GetFullPath(_db.DataSource);


    public async Task<bool> ValidateSchema()
    {

        await _db.OpenAsync();
        _db.Close();

        return true;
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

public static class KeyValueSqlLiteRepoExtensions
{
    public static KeyValueSqlLiteRepo asKeyValueSqlLiteRepo(this IKeyValueRepo repo)
    {
        if(repo is KeyValueSqlLiteRepo)
        {
            return (KeyValueSqlLiteRepo)repo;
        }
        return null;
    }
}
