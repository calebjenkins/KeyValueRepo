﻿using System.Diagnostics;

namespace KeyValueRepoTests;

public class SqLiteTests : InMemeoryTests
{
    ILogger<KeyValueSqLiteRepo> _logger = new Mock<ILogger<KeyValueSqLiteRepo>>().Object;
    ILogger<SchemaValidator> _schemaLogger = new Mock<ILogger<SchemaValidator>>().Object;

    public override IKeyValueRepo GetNewRepo()
    {
        var opt = new KeyValueSqlLiteOptions() { ConnectionString = "Data Source=./Db.db" };
        var validator = new SchemaValidator(_schemaLogger);
        return new KeyValueSqLiteRepo(_logger, validator, opt);
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
        filepath.Should().Contain("_Data\\Db.db");
    }

    [Fact]
    public void DefaultFileNameShouldBeProvided()
    {
        ILogger<KeyValueSqLiteRepo> _logger = new Mock<ILogger<KeyValueSqLiteRepo>>().Object;
        ILogger<SchemaValidator> _loggerValidator = new Mock<ILogger<SchemaValidator>>().Object;
        SchemaValidator _validator = new SchemaValidator(_loggerValidator);

        var db = new KeyValueSqLiteRepo(_logger, _validator);
        var filePath = db.DatabaseFileName;
        filePath.Should().Contain("KeyValueDatabase.db");
    }

    [Fact]
    public async Task CorrectFileGetsCreated()
    {
        var db = GetNewRepo();
        var filePath = db.AsKeyValueSqlLiteRepo().DatabaseFileName;

        // Clear File
        await removeDbFileIfExists(db.AsKeyValueSqlLiteRepo());

        // Create a new File
        var valid = await db.AsKeyValueSqlLiteRepo().ValidateSchema();
        File.Exists(filePath).Should().BeTrue();

        // Clean Up
        await removeDbFileIfExists(db.AsKeyValueSqlLiteRepo());
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
        var db = GetNewRepo();
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
    }
}