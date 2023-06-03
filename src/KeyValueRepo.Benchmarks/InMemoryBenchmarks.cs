

using Microsoft.Diagnostics.Tracing.Parsers.Clr;

namespace KeyValueRepo.Benchmarks;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class InMemoryBenchmarks
{
    //private static IKeyValueRepo GetRepo() => new KeyValueInMemory();
    private static IKeyValueRepo repo = new KeyValueInMemory();
   

    private static IList<Person> People = new List<Person>()
    {
        new Person("001", "Nick", "Burkhart"),
        new Person("002", "Kelly", "Burkhart"),
        new Person("003", "Monroe", "")
    };
    private static IList<Location> Places = new List<Location>()
    {
        new Location(1, "Pensacola", 687, true),
        new Location(2, "Telavive", 6525, true),
        new Location(3, "Greece", 7824, false),
        new Location(4, "London", 5530, false)
    };

    [Benchmark]
    public void ThreeWrites_OneReadOne()
    {
      //  var repo = GetRepo();

        repo.Update(People[0].Id, People[0]);
        repo.Update(People[1].Id, People[1]);
        repo.Update(People[2].Id, People[2]);

        var p1 = repo.Get<Person>("002");
    }
    [Benchmark]
    public void ThreeWrites_OneReadAll()
    {
        // var repo = GetRepo();

        repo.Update(People[0].Id, People[0]);
        repo.Update(People[1].Id, People[1]);
        repo.Update(People[2].Id, People[2]);

        var pA = repo.GetAll<Person>();
    }

    [Benchmark]
    public void FiveWrites_TwoReads()
    {
       // var repo = GetRepo();

        repo.Update(People[0].Id, People[0]);
        repo.Update(People[1].Id, People[1]);
        repo.Update(People[2].Id, People[2]);

        repo.Update(Places[0].Id, Places[0]);
        repo.Update(Places[1].Id, Places[1]);
        

        var p1 = repo.Get<Person>("003");
        var l1 = repo.Get<Location>(1);
    }

    [Benchmark]
    public void FiveWrites_OneReadAll()
    {
        // var repo = GetRepo();

        repo.Update(People[0].Id, People[0]);
        repo.Update(Places[0].Id, Places[0]);

        repo.Update(People[1].Id, People[1]);
        repo.Update(Places[1].Id, Places[1]);

        repo.Update(People[2].Id, People[2]);

        
        var pA = repo.GetAll<Person>();
    }
}
