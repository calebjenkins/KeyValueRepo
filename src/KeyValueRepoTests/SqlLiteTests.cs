using Calebs.Data.KeyValueRepo.SqlLite;

namespace KeyValueRepoTests;

public class SqlLiteTests : InMemeoryTests
{
    public override IKeyValueRepo GetNewRepo()
    {
        return new KeyValueSqlLiteRepo("Data Source=./../../../_Data/Db.db");
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
