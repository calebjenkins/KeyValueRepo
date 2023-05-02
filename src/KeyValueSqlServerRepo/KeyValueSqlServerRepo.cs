using Microsoft.Extensions.Logging;
using System.Data.SqlClient;

namespace Calebs.Data.KeyValueRepo.SqlServer;

public class KeyValueSqlServerRepo : IKeyValueRepo
{
    private string _connString;
    SqlConnection _conn;
    ILogger _logger; 

    public KeyValueSqlServerRepo(string connString, ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _connString = connString ?? throw new ArgumentNullException(nameof(connString));

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
