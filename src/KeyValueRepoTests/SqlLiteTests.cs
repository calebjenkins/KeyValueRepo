using Calebs.Data.KeyValueRepo.SqlLite;

namespace KeyValueRepoTests;

public class SqlLiteTests : InMemeoryTests
{
    public override IKeyValueRepo GetNewRepo()
    {
        return new KeyValueSqlLiteRepo("");
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
        var db = new KeyValueSqlLiteRepo("");
        var result = await db.ValidateSchema();
        result.Should().BeTrue();

    }
}
