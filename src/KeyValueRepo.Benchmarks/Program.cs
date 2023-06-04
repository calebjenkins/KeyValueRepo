namespace KeyValueRepo.Benchmarks;

[Config(typeof(BenchmarkConfig))]
public class Program
{
    private static void Main(string[] args)
    {
        BenchmarkRunner.Run<InMemoryBenchmarks>();
        BenchmarkRunner.Run<SQLiteBenchmarks>();
    }
}