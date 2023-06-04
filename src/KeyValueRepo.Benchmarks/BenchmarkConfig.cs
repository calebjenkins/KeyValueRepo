
using BenchmarkDotNet.Configs;

namespace KeyValueRepo.Benchmarks;

public class BenchmarkConfig : ManualConfig
{
    public BenchmarkConfig()
    {
        WithOptions(ConfigOptions.JoinSummary);
    }
}
