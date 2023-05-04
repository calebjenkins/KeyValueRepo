namespace KeyValueTests;

public class InMemoryTests
{
    public virtual IKeyValueRepo GetNewInstanceOfRepoForTests()
    {
        return new KeyValueInMemory();
    }

    [Fact]
    public async Task InMemoryShouldHoldValues()
    {
        IKeyValueRepo repo = GetNewInstanceOfRepoForTests();
        var p = new Person("Test", "Last", 1);

        repo.Update(p.Id.ToString(), p);

        var p2 = await repo.Get<Person>("1");
        p2.Should().NotBeNull();
        p2?.First.Should().Be("Test");

        var p3 = p2 with { First = "Test2" };
        await repo.Update(p3.Id.ToString(), p3);

        var p4 = await repo.Get<Person>(1);
        p4?.First.Should().Be("Test2");  
    }

    [Fact]
    public async Task ShouldReturnVoidForUnknownTypes()
    {
        IKeyValueRepo repo = GetNewInstanceOfRepoForTests();
        var p = repo.Get<Person>("1");

        p.Should().BeNull();
    }

    [Fact]
    public async Task MissingIdShouldReturnNull()
    {
        IKeyValueRepo repo = GetNewInstanceOfRepoForTests();
        var p1 = new Person("Kelly", "Burkhardt", 1);
        await repo.Update(p1.Id, p1);

        var p2 = await repo.Get<Person>(1);
        p2.Should().NotBeNull();

        var p3 = await repo.Get<Person>("2");
        p3.Should().BeNull();
    }

    [Fact]
    public async Task GetAllShouldReturnAllInstances()
    {
        IKeyValueRepo repo = GetNewInstanceOfRepoForTests();
        var p1 = new Person("Kelly", "Burkhardt", 1);
        var p2 = new Person("Drew", "Wu", 2);
        var p3 = new Person("Monroe", "", 3);

        await repo.Update(p1.Id, p1);
        await repo.Update(p2.Id, p2);
        await repo.Update(p3.Id.ToString(), p3);

        var people = await repo.GetAll<Person>();
        people.Count.Should().Be(3);

        var l1 = new Location("1", "123 Main", "Dallas");
        var l2 = new Location("2", "456 Front St.", "Tulsa");

        await repo.Update(l1.Id, l1);
        await repo.Update(l2.Id, l2);

        var p4 = new Person("Nick", "Burkhardt", 4);
        await repo.Update(p4.Id.ToString(), p4);

        people = await repo.GetAll<Person>();
        var locals = await repo.GetAll<Location>();

        people.Count().Should().Be(4);
        locals.Count().Should().Be(2);
    }
}

public record Person (string First, string Last, int Id);
public record Location (string Id, string Street, string City);
