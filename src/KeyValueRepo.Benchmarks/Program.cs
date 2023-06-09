namespace KeyValueRepo.Benchmarks;


public class Program
{
    //private static void Main(string[] args)
    //{
    //    BenchmarkRunner.Run<InMemoryBenchmarks>();
    //    BenchmarkRunner.Run<SQLiteBenchmarks>();
    //}
    public static void Main(string[] args) =>
    BenchmarkSwitcher.FromAssemblies(new[] { typeof(Program).Assembly }).Run(args);
}