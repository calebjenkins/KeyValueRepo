using Microsoft.Extensions.Logging;
using Calebs.Data.KeyValueRepo.SqlServer;
using Calebs.Extensions;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using Moq;

namespace KeyValueTests;

public class SqlServerRepoTests : InMemoryTests
{
    ILogger<KeyValueSqlServerRepo> _logger;
    KeyValueSqlServerOptions _options = new KeyValueSqlServerOptions();

    public SqlServerRepoTests()
    {
        _logger = new Mock<ILogger<KeyValueSqlServerRepo>>().Object;
        var filePath = System.IO.Path.GetFullPath("../../../Data/DB.mdf");
        _options.ConnString = $"Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename={filePath};Trusted_Connection=Yes;";
    }

    public override IKeyValueRepo GetNewInstanceOfRepoForTests()
    {
        return new KeyValueSqlServerRepo(_options, _logger);
    }

    [Fact]
    public void CanCreateLocalDbIfMissing()
    {
        var connStr = _options.ConnString;
        string filePath = "";
        var filePathInConn = KeyValueSqlServerRepo.TryFileNameFromConnectionString(connStr, out filePath);
        filePathInConn.Should().BeTrue();

        var fileExists = File.Exists(filePath);
        fileExists.Should().BeFalse();

        KeyValueSqlServerRepo.CreateSqlDatabaseExpressFile(filePath);
        fileExists = File.Exists(filePath);
        fileExists.Should().BeTrue();

        var repo = new KeyValueSqlServerRepo(_options, _logger);
        repo.Should().NotBeNull();

    }


    [Fact]
    public void CanInitalizeSqlRepoForTests()
    {
        var repo = GetNewInstanceOfRepoForTests();
        repo.Should().NotBeNull();
    }

    [Fact]
    public void CanInitializeLocalDBFile()
    {

    }
}
