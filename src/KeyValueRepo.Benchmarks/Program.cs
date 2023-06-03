namespace KeyValueRepo.Benchmarks;


public class Program
{
    private static void Main(string[] args)
    {
        BenchmarkRunner.Run<InMemoryBenchmarks>();
        BenchmarkRunner.Run<SQLiteBenchmarks>();
    }
}