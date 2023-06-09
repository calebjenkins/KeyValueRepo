
namespace KeyValueRepo.Benchmarks;

[KeepBenchmarkFiles]
[MemoryDiagnoser]
[RankColumn]
public abstract class BaseBenchmarks
{
    public IKeyValueRepo? Repo { get; set; }
    private IList<Person> People = Person.TestPeople();
    private IList<Location> Places = Location.TestPlaces();


    [Benchmark]
    public void ThreeWrites_OneReadOne()
    {
        Repo?.Update(People[0].Id, People[0]);
        Repo?.Update(People[1].Id, People[1]);
        Repo?.Update(People[2].Id, People[2]);

        var p1 = Repo?.Get<Person>("002");
    }
    [Benchmark]
    public void ThreeWrites_OneReadAll()
    {
        Repo?.Update(People[0].Id, People[0]);
        Repo?.Update(People[1].Id, People[1]);
        Repo?.Update(People[2].Id, People[2]);

        var pA = Repo?.GetAll<Person>();
    }

    [Benchmark]
    public void FiveWrites_TwoReads()
    {
        Repo?.Update(People[0].Id, People[0]);
        Repo?.Update(People[1].Id, People[1]);
        Repo?.Update(People[2].Id, People[2]);

        Repo?.Update(Places[0].Id, Places[0]);
        Repo?.Update(Places[1].Id, Places[1]);
        

        var p1 = Repo?.Get<Person>("003");
        var l1 = Repo?.Get<Location>(1);
    }

    [Benchmark]
    public void FiveWrites_OneReadAll()
    {
        Repo?.Update(People[0].Id, People[0]);
        Repo?.Update(Places[0].Id, Places[0]);

        Repo?.Update(People[1].Id, People[1]);
        Repo?.Update(Places[1].Id, Places[1]);

        Repo?.Update(People[2].Id, People[2]);
   
        var pA = Repo?.GetAll<Person>();
    }
}
