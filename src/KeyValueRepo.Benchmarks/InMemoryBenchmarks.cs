
namespace KeyValueRepo.Benchmarks;


[Config(typeof(Config))]
public class InMemoryBenchmarks : BaseBenchmarks
{
    public InMemoryBenchmarks()
    {
        Repo = new KeyValueInMemory();
    }
}
