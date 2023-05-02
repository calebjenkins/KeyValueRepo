﻿using Microsoft.Extensions.Logging;
using Calebs.Data.KeyValueRepo.SqlServer;
using Castle.Core.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using Moq;

namespace Tests.InMemory;

public class SqlServerTests : InMemoryTests
{
    ILogger _logger;
    KeyValueSqlServerOptions _options = new KeyValueSqlServerOptions();

    public SqlServerTests()
    {
        _logger = new Mock<ILogger>().Object;
        var filePath = System.IO.Path.GetFullPath("../../../Data/DB.mdf");
        _options.ConnString = $"Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename={filePath};Trusted_Connection=Yes;";
    }

    public override IKeyValueRepo GetNewInstanceOfRepoForTests()
    {
        return new KeyValueSqlServerRepo(_options, _logger);
    }

    [Fact]
    public void CanInitalizeSqlRepoForTests()
    {
        var repo = GetNewInstanceOfRepoForTests();
        repo.Should().NotBeNull();
    }
}
