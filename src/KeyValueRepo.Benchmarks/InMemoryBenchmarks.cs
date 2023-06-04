
namespace KeyValueRepo.Benchmarks;


[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class InMemoryBenchmarks: KeyValueBaseBenchmarks
{
    [GlobalSetup(Target = nameof(InMemoryBenchmarks))]
    public override void SetUp()
    {
        Repo = new KeyValueInMemory();
    }
}
