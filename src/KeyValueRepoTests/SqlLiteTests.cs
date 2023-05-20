using Calebs.Data.KeyValueRepo.SqlLite;
using Castle.Core.Logging;
using Microsoft.Extensions.Logging;

namespace KeyValueRepoTests;

public class SqlLiteTests : InMemeoryTests
{
    ILogger<KeyValueSqlLiteRepo> _logger = new Moq.Mock<ILogger<KeyValueSqlLiteRepo>>().Object;

    public override IKeyValueRepo GetNewRepo()
    {
        var opt = new KeyValueSqlLiteOptions() { ConnectionString= "Data Source=./../../../_Data/Db.db" };
        return new KeyValueSqlLiteRepo(opt, _logger);
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
        var result = await ((KeyValueSqlLiteRepo) db).ValidateSchema();
        result.Should().BeTrue();
    }
}
