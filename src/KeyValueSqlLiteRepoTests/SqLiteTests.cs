
using System.Diagnostics;
using System.Security.Principal;

namespace KeyValueSqlLiteRepoTests;


[Collection ("RepoTests")]
public class SqLiteTests : KeyValueBaseTests
{
    ILogger<KeyValueSqLiteRepo> _logger = new Mock<ILogger<KeyValueSqLiteRepo>>().Object;
    ILogger<SchemaValidator> _schemaLogger = new Mock<ILogger<SchemaValidator>>().Object;
    ITestOutputHelper _out;
    IKeyValueRepo repo;

    public SqLiteTests(ITestOutputHelper output)
    {
        _out = output ?? throw new ArgumentNullException(nameof(output));

        var TEST_Name = "test name";
        IIdentity ident = new GenericIdentity(TEST_Name);
        IPrincipal princ = new GenericPrincipal(ident, null);
        Thread.CurrentPrincipal = princ;

        repo = GetNewRepo();

    }

    [Fact]
    public override async Task Dispose()
    {
        // clean up
        await repo.AsKeyValueSqlLiteRepo().RemoveAll<Person>();
        await repo.AsKeyValueSqlLiteRepo().RemoveAll<Location>();
        await base.Dispose();
    }

    public IKeyValueRepo GetNewRepo(KeyValueSqlLiteOptions options)
    {
        var validator = new SchemaValidator(_schemaLogger);
        return new KeyValueSqLiteRepo(_logger, validator, options);
    }

    public IKeyValueRepo GetNewRepo(string path)
    {
        var opt = new KeyValueSqlLiteOptions()
        {
            ConnectionString = path,
            ColumnPrefix = "col"
        };

        return GetNewRepo(opt);
    }
    public override IKeyValueRepo GetNewRepo()
    {
        return GetNewRepo("Data Source=./Db.db");
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
        var result = await db.AsKeyValueSqlLiteRepo().ValidateSchema();
        result.Should().BeTrue();
    }

    [Fact]
    public void FileNameIsAvailable()
    {
        var db = GetNewRepo();
        var filepath = db.AsKeyValueSqlLiteRepo().DatabaseFileName;
        filepath.Should().Contain("Db.db");
    }

    [Fact]
    public async Task CorrectFileGetsCreated()
    {
        var db = GetNewRepo();
        var filePath = db.AsKeyValueSqlLiteRepo().DatabaseFileName;

        // Clear File
       // await removeDbFileIfExists(db.AsKeyValueSqlLiteRepo());

        // Create a new File
        var valid = await db.AsKeyValueSqlLiteRepo().ValidateSchema();
        File.Exists(filePath).Should().BeTrue();

        // Clean Up
       // await removeDbFileIfExists(db.AsKeyValueSqlLiteRepo());
    }

    private async Task removeDbFileIfExists(KeyValueSqLiteRepo Repo)
    {
        var filePath = Repo.DatabaseFileName;
        Debug.WriteLine($"Database path: {filePath}");

        var exists = File.Exists(filePath);

        if (exists)
        {
            await Repo.ReleaseForCleanUp();
            File.Delete(filePath);
            File.Exists(filePath).Should().BeFalse();
        }
    }

    [Fact]
    public async Task ConfirmTableDoesNotExistAndCanBeCreated()
    {
        var tmpData = Guid.NewGuid().ToString().Substring(0, 5);
        var db = GetNewRepo($"Data Source=./{tmpData}.db");
        var filePath = db.AsKeyValueSqlLiteRepo().DatabaseFileName;

        // Reset DB
        await removeDbFileIfExists(db.AsKeyValueSqlLiteRepo());

        var opt = new KeyValueSqlLiteOptions() { ColumnPrefix = "col" };
        var defaultTableName = opt.DefaultTableName;

        ILogger<SchemaValidator> _SchemaLogger = new Mock<ILogger<SchemaValidator>>().Object;

        var verify = new SchemaValidator(_SchemaLogger);
        bool exist = await verify.TablesExists(defaultTableName, db.AsKeyValueSqlLiteRepo().DbConn);
        exist.Should().BeFalse();

        bool tableCreated = await verify.CreateAllTables(opt, db.AsKeyValueSqlLiteRepo().DbConn);
        tableCreated.Should().BeTrue();

        exist = await verify.TablesExists(defaultTableName, db.AsKeyValueSqlLiteRepo().DbConn);
        exist.Should().BeTrue();

        var IsValid = await verify.ValidateSchema(opt, db.AsKeyValueSqlLiteRepo().DbConn);
        IsValid.HasError.Should().BeFalse();
        IsValid.Messages.Count.Should().Be(7);

        // Reset DB
        await removeDbFileIfExists(db.AsKeyValueSqlLiteRepo());
    }

    [Fact]
    public void GetHistory_ShouldReturnOneItemWhenHistoryFalse()
    {

    }

    [Fact]
    public async Task GetHistory_ShouldReturnAllHistoryWhenHistoryTrue()
    {
        var rnd = getRandomId();

        var opt = new KeyValueSqlLiteOptions()
        {
            ConnectionString = $"Data Source=./{rnd}_WithHistory.db",
            TrackHistory = true,
            ValidateSchemaOnStartUp = true
        };
        IKeyValueRepo db = GetNewRepo(opt);

        await db.Update<Person>(1, new Person("Test", "Test_First", 1));
        await db.Update<Person>(1, new Person("Test", "Test_Second", 1));
        await db.Update<Person>(1, new Person("Test", "Test_Last", 1));

        var p1 = await db.Get<Person>(1);
        p1?.Should().NotBeNull();
        p1?.First.Should().Be("Test");

        var pHist = await db.GetHistory<Person>(1);
        pHist?.Count.Should().Be(3);

        var path = db.AsKeyValueSqlLiteRepo().DatabaseFileName;
        await db.AsKeyValueSqlLiteRepo().ReleaseForCleanUp();

        File.Delete(path);
    }

    [Fact]
    public async Task Validator_TrackHistory_ShouldThrowErrorIfNotConfiguredForTrackHistory()
    {
        var rnd = getRandomId();
        var connString = $"Data Source=./{rnd}_WithHistory.db";
        var expectedError = false;

        var opt = new KeyValueSqlLiteOptions()
        {
            ConnectionString = connString,
            TrackHistory = true,
            ValidateSchemaOnStartUp = true
        };
        IKeyValueRepo dbWithTrackHistory = GetNewRepo(opt);
        dbWithTrackHistory.Should().NotBeNull();

        opt.TrackHistory = false; // mismatch
        IKeyValueRepo? dbWithNoTrackHistory = null;

        try
        {
            dbWithNoTrackHistory = GetNewRepo(opt);
        }
        catch(InvalidOperationException ex)
        {
            Assert.True(ex != null, "Expected Error - forced mismatch error");
            expectedError = true;
        }
        catch (Exception ex)
        {
            Assert.False(ex != null, "Should not be here - unexpected exception");
        }
        finally
        {
            if(dbWithNoTrackHistory != null)
                await dbWithNoTrackHistory.AsKeyValueSqlLiteRepo().ReleaseForCleanUp();
        }

        var path = dbWithTrackHistory.AsKeyValueSqlLiteRepo().DatabaseFileName;
        await dbWithTrackHistory.AsKeyValueSqlLiteRepo().ReleaseForCleanUp();
        
        File.Delete(path);

        Assert.True(expectedError);
    }

    [Fact]
    public async Task Validator_TrackHistoryFalse_ShouldThrowErrorIfConfiguredForTrackHistory()
    {
        var rnd = getRandomId();
        var connString = $"Data Source=./{rnd}_WithHistory.db";
        var expectedError = false;

        var opt = new KeyValueSqlLiteOptions()
        {
            ConnectionString = connString,
            TrackHistory = false,
            ValidateSchemaOnStartUp = true
        };
        IKeyValueRepo dbWithNotTrackHistory = GetNewRepo(opt);
        dbWithNotTrackHistory.Should().NotBeNull();

        opt.TrackHistory = true; // mismatch
        IKeyValueRepo? dbWithTrackHistory = null;

        try
        {
            dbWithTrackHistory = GetNewRepo(opt);
        }
        catch (InvalidOperationException ex)
        {
            Assert.True(ex != null, "Expected Error - forced mismatch error");
            expectedError = true;
        }
        catch (Exception ex)
        {
            Assert.False(ex != null, "Should not be here - unexpected exception");
        }
        finally
        {
            if (dbWithTrackHistory != null)
                await dbWithTrackHistory.AsKeyValueSqlLiteRepo().ReleaseForCleanUp();
        }

        var path = dbWithNotTrackHistory.AsKeyValueSqlLiteRepo().DatabaseFileName;
        await dbWithNotTrackHistory.AsKeyValueSqlLiteRepo().ReleaseForCleanUp();

        File.Delete(path);

        Assert.True(expectedError);
    }
}