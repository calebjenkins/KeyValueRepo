

namespace KeyValueRepo.Benchmarks;

public record Person(string Id, string First, string Last)
{
    public static IList<Person> TestPeople()
    {
        return new List<Person>()
        {
            new Person("001", "Nick", "Burkhart"),
            new Person("002", "Kelly", "Burkhart"),
            new Person("003", "Monroe", "")
        };
    }
}
public record Location(int Id, string Name, int Distance, bool HaveBeenTo)
{
    public static IList<Location> TestPlaces()
    {
        return new List<Location>()
        {
            new Location(1, "Pensacola", 687, true),
            new Location(2, "Telavive", 6525, true),
            new Location(3, "Greece", 7824, false),
            new Location(4, "London", 5530, false)
        };
    }
}
