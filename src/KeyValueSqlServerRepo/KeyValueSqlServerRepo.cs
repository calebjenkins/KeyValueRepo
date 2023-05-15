using Microsoft.Extensions.Logging;
using System.Data.SqlClient;
using System.Net.NetworkInformation;

namespace Calebs.Data.KeyValueRepo.SqlServer;

public class KeyValueSqlServerRepo : IKeyValueRepo
{
    private readonly KeyValueSqlServerOptions _options;
    private string _connString;
    ILogger<KeyValueSqlServerRepo> _logger; 

    public KeyValueSqlServerRepo(KeyValueSqlServerOptions options, ILogger<KeyValueSqlServerRepo> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options ?? throw new ArgumentNullException(nameof(options)); 
        _connString = _options?.ConnString ?? throw new ArgumentNullException("options.ConnString");

        if(_options.ValidateDataSchemaOnStart)
        {
            var result = validateConnection();
            _logger.LogInformation( $"DataSchema Validated. Results: {result}");
        }
    }
    private bool validateConnection()
    {
        _logger.LogInformation("Validating Connection");
        _logger.LogDebug("About to open SQL Connection");
        var _conn = new SqlConnection(_connString);
        _conn.Open();

        _logger.LogDebug("Connection is open");

        _conn.Close();

        _logger.LogDebug("SqlConnection is closed");

        return true;
    }

    public static void CreateSqlDatabaseExpressFile(string filename)
    {
        string databaseName = Path.GetFileNameWithoutExtension(filename);
        const string ExpressMasterDB = $"Data Source=.\\\\SqlExpress; Initial Catalog=master; Integrated Security=true; User Instance=True;";

        using (var connection = new SqlConnection(ExpressMasterDB))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText =
                    String.Format("CREATE DATABASE {0} ON PRIMARY (NAME={0}, FILENAME='{1}')", databaseName, filename);
                command.ExecuteNonQuery();

                command.CommandText =
                    String.Format("EXEC sp_detach_db '{0}', 'true'", databaseName);
                command.ExecuteNonQuery();
            }
        }

        //Connect to the local, default instance of SQL Server.   
        //Microsoft.SqlServer.Server.  Server srv = new Server();

        //// Define a Database object variable by supplying the server and the 
        //// database name arguments in the constructor.   
        //Database db = new Database(srv, "Test_SMO_Database");

        ////Create the database on the instance of SQL Server.   
        //db.Create();

        ////Reference the database and display the date when it was created.   
        //db = srv.Databases["Test_SMO_Database"];
        //Console.WriteLine(db.CreateDate);
    }

    public static bool TryFileNameFromConnectionString(string connectionString, out string FilePath)
    {
        if (!connectionString.Contains(".mdf", StringComparison.OrdinalIgnoreCase))
        {
            FilePath = string.Empty;
            return false;
        }
        try
        {
            int mdbIndex = 4 + connectionString.IndexOf(".mdf", StringComparison.OrdinalIgnoreCase);
            int attachIndex = 17 + connectionString.IndexOf("AttachDbFilename=", StringComparison.OrdinalIgnoreCase);
            int length = mdbIndex - attachIndex;

            string filePath = connectionString.Substring(attachIndex, length);
            FilePath = filePath;
            return true;
        }
        catch (Exception ex)
        {
            FilePath = $"ERROR: {ex.Message}";
            return false;
        }
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
