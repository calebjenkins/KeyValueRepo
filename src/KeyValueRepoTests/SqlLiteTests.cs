using System.Diagnostics;

namespace KeyValueRepoTests;

public class SqlLiteTests : InMemeoryTests
{
    ILogger<KeyValueSqlLiteRepo> _logger = new Moq.Mock<ILogger<KeyValueSqlLiteRepo>>().Object;

    public override IKeyValueRepo GetNewRepo()
    {
        var opt = new KeyValueSqlLiteOptions() { ConnectionString = "Data Source=./Db.db" };
        return new KeyValueSqlLiteRepo(_logger, opt);
    }

    [Fact]
    public void ShouldBeAbleToInstanciate()
    {
        var db = GetNewRepo();
        db.Should().NotBeNull();
    }

    [Fact]
    public async Task ShouldCreateDbInstance()
    {
        var db = GetNewRepo();
        var result = await db.asKeyValueSqlLiteRepo().ValidateSchema();
        result.Should().BeTrue();
    }

    [Fact]
    public void FileNameIsAvailable()
    {
        var db = GetNewRepo();
        var filepath = db.asKeyValueSqlLiteRepo().DatabaseFileName;
        filepath.Should().Contain("_Data\\Db.db");
    }

    [Fact]
    public void DefaultFileNameShouldBeProvided()
    {
        ILogger<KeyValueSqlLiteRepo> _logger = new Mock<ILogger<KeyValueSqlLiteRepo>>().Object;
        var db = new KeyValueSqlLiteRepo(_logger);
        var filePath = db.DatabaseFileName;
        filePath.Should().Contain("KeyValueDatabase.db");
    }

    [Fact]
    public async Task CorrectFileGetsCreated()
    {
        var db = GetNewRepo();
        var filePath = db.asKeyValueSqlLiteRepo().DatabaseFileName;
        Debug.WriteLine($"Database path: {filePath}");

        var exists = File.Exists(filePath);

        if (exists)
        {
            await db.asKeyValueSqlLiteRepo().ReleaseForCleanUp();
            File.Delete(filePath);
            File.Exists(filePath).Should().BeFalse();
        }

        var valid = await db.asKeyValueSqlLiteRepo().ValidateSchema();
        File.Exists(filePath).Should().BeTrue();

        //// Clean Up
        await db.asKeyValueSqlLiteRepo().ReleaseForCleanUp();
        File.Delete(filePath);
        File.Exists(filePath).Should().BeFalse();
    }

    [Fact]
    public async Task ConfirmTableDoesNotExistAndCanBeCreated()
    {
        var db = GetNewRepo();
        var filePath = db.asKeyValueSqlLiteRepo().DatabaseFileName;

        var opt = new KeyValueSqlLiteOptions() { ColumnPrefix = "col" };
        var defaultTableName = opt.DefaultTableName;

        ILogger<SchemaValidator> _SchemaLogger = new Mock<ILogger<SchemaValidator>>().Object;

        var verify = new SchemaValidator(_SchemaLogger);
        bool exist = await verify.TablesExists(defaultTableName, db.asKeyValueSqlLiteRepo().DbConn);
        exist.Should().BeFalse();

        bool tableCreated = await verify.CreateAllTables(opt, db.asKeyValueSqlLiteRepo().DbConn);
        tableCreated.Should().BeTrue();

        exist = await verify.TablesExists(defaultTableName, db.asKeyValueSqlLiteRepo().DbConn);
        exist.Should().BeTrue();

        var IsValid = await verify.ValidateSchema(opt, db.asKeyValueSqlLiteRepo().DbConn);
        IsValid.Should().BeTrue();
    }
}