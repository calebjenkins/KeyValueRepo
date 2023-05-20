namespace Calebs.Data.KeyValueRepo.SqlLite;

public class KeyValueSqlLiteRepo : IKeyValueRepo
{

    private string _connectionString;
    private Microsoft.Data.Sqlite.SqliteConnection _db;
    public KeyValueSqlLiteRepo(string ConnectionString)
    {
        _connectionString = ConnectionString ?? throw new ArgumentNullException(nameof(ConnectionString));

        _db = new Microsoft.Data.Sqlite.SqliteConnection(_connectionString);
    }

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
