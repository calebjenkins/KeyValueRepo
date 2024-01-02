
namespace KeyValueRepo.Benchmarks;

[Config(typeof(Config))]
public class SQLiteBenchmarks : BaseBenchmarks
{
    [GlobalSetup]
    public void SetUpSQLiteBenchmarks()
    {
        var opt = new KeyValueSqlLiteOptions() { ColumnPrefix = "col" };
        var vLogger = new Mock<ILogger<SchemaValidator>>().Object;
        var kvLogger = new Mock<ILogger<KeyValueSqLiteRepo>>().Object;
        var validator = new SchemaValidator(vLogger);

         Repo = new KeyValueSqLiteRepo(kvLogger, validator, opt);
    }

    [GlobalCleanup]
    public async Task CleanUp()
    {
        var pathToFile = Repo?.AsKeyValueSqlLiteRepo().DatabaseFileName;

        if (pathToFile.IsNotNullOrEmpty() && Repo != null)
        {
            await Repo.AsKeyValueSqlLiteRepo().ReleaseForCleanUp();
            File.Delete(pathToFile.ValueOrEmpty());
        }

    }
}
