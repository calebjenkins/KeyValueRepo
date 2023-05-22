namespace KeyValueRepoTests;

public class SqlLiteTests : InMemeoryTests
{
    ILogger<KeyValueSqlLiteRepo> _logger = new Moq.Mock<ILogger<KeyValueSqlLiteRepo>>().Object;

    public override IKeyValueRepo GetNewRepo()
    {
        var opt = new KeyValueSqlLiteOptions() { ConnectionString = "Data Source=./../../../_Data/Db.db" };
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

        var exists = File.Exists(filePath);
        exists.Should().BeTrue();

        if (exists)
        {
            await db.asKeyValueSqlLiteRepo().ReleaseForCleanUp();
            File.Delete(filePath);
            File.Exists(filePath).Should().BeFalse();
        }

        var valid = await db.asKeyValueSqlLiteRepo().ValidateSchema();
        File.Exists(filePath).Should().BeTrue();

        // Clean Up
        // await db.asKeyValueSqlLiteRepo().ReleaseForCleanUp();
        // File.Delete(filePath);
        // File.Exists(filePath).Should().BeFalse();
    }
}