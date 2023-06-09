
namespace KeyValueRepo.Benchmarks;

public class Config : ManualConfig
{
    public Config()
    {
        WithOptions(ConfigOptions.JoinSummary);
        var baseJob = Job.Default;
        AddExporter(JsonExporter.FullCompressed);
    }
}
