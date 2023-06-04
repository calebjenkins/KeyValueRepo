using BenchmarkDotNet.Attributes;
using Calebs.Extensions;
using Microsoft.Extensions.Logging;

namespace KeyValueRepo.Benchmarks;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class SQLiteBenchmarks : KeyValueBaseBenchmarks
{
    [GlobalSetup (Target = nameof(SQLiteBenchmarks))]
    public override void SetUp()
    {
        var opt = new KeyValueSqlLiteOptions() { ColumnPrefix = "col" };
        var vLogger = new Mock<ILogger<SchemaValidator>>().Object;
        var kvLogger = new Mock<ILogger<KeyValueSqLiteRepo>>().Object;
        var validator = new SchemaValidator(vLogger);

        Repo = new KeyValueSqLiteRepo(kvLogger, validator, opt);
    }

    [GlobalCleanup(Target = nameof(SQLiteBenchmarks))]
    public async Task CleanUp()
    {
        var pathToFile = Repo?.AsKeyValueSqlLiteRepo().DatabaseFileName;

        if (pathToFile.IsNotNullOrEmpty())
        {
            await Repo.AsKeyValueSqlLiteRepo().ReleaseForCleanUp();
            File.Delete(pathToFile.ValueOrEmpty());
        }

    }
}
