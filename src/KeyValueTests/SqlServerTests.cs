using Calebs.Data.KeyValueRepo.SqlServer;

namespace Tests.InMemory;

public class SqlServerTests : InMemoryTests
{
    public override IKeyValueRepo GetNewInstanceOfRepoForTests()
    {
        return new KeyValueSqlServerRepo();
    }
}
