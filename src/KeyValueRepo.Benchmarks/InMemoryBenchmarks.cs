
namespace KeyValueRepo.Benchmarks;


[Config(typeof(Config))]
public class InMemoryBenchmarks : BaseBenchmarks
{
    public InMemoryBenchmarks()
    {
        Repo = new KeyValueInMemory();
    }

    private class Config : ManualConfig
    {
        public Config()
        {
            WithOptions(ConfigOptions.JoinSummary);
            var baseJob = Job.Default;

            // AddJob(baseJob.
        }
    }
}
