using Microsoft.Extensions.Logging;
using System.Data.SqlClient;

namespace Calebs.Data.KeyValueRepo.SqlServer;

public class KeyValueSqlServerRepo : IKeyValueRepo
{
    private readonly KeyValueSqlServerOptions _options;
    private string _connString;
    SqlConnection _conn;
    ILogger _logger; 

    public KeyValueSqlServerRepo(KeyValueSqlServerOptions options, ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options ?? throw new ArgumentNullException(nameof(options)); 
        _connString = _options?.ConnString ?? throw new ArgumentNullException("options.ConnString");

        _logger.LogDebug("About to open SQL Connection");
        _conn = new SqlConnection(_connString);
        _conn.Open();

        _logger.LogDebug("Connection is open");

        _conn.Close();

        _logger.LogDebug("SqlConnection is closed");
        
    }

    public T? Get<T>(string key) where T : class
    {
        throw new NotImplementedException();
    }

    public IList<T>? GetAll<T>() where T : class
    {
        throw new NotImplementedException();
    }

    public void Update<T>(string key, T value) where T : class
    {
        throw new NotImplementedException();
    }
}
