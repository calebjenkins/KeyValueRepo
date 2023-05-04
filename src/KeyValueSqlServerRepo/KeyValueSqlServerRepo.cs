using Microsoft.Extensions.Logging;
using System.Data.SqlClient;

namespace Calebs.Data.KeyValueRepo.SqlServer;

public class KeyValueSqlServerRepo : IKeyValueRepo
{
    private readonly KeyValueSqlServerOptions _options;
    private string _connString;
    SqlConnection _conn;
    ILogger<KeyValueSqlServerRepo> _logger; 

    public KeyValueSqlServerRepo(KeyValueSqlServerOptions options, ILogger<KeyValueSqlServerRepo> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options ?? throw new ArgumentNullException(nameof(options)); 
        _connString = _options?.ConnString ?? throw new ArgumentNullException("options.ConnString");

        if(_options.ValidateDataSchemaOnStart)
        {
            var result = validateConnection();
            _logger.LogInformation("DataSchema Validated");
        }
    }
    private bool validateConnection()
    {
        _logger.LogInformation("Validating Connection");
        _logger.LogDebug("About to open SQL Connection");
        _conn = new SqlConnection(_connString);
        _conn.Open();

        _logger.LogDebug("Connection is open");

        _conn.Close();

        _logger.LogDebug("SqlConnection is closed");

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
