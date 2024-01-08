

using System.Security.Principal;

namespace Calebs.Data.KeyValueRepoTests;

[Collection("RepoTests")]
public class InMemoryTests
{
    public virtual IKeyValueRepo GetNewRepo()
    {
        return new KeyValueInMemory();
    }

    private async Task<IKeyValueRepo> getRepoWithRecords(Person p, string TestName)
    {
        var people = new List<Person>() { p };
        return await getRepoWithRecords(people, TestName);
    }

    private async Task<IKeyValueRepo> getRepoWithRecords(IList<Person> People, string TestName)
    {
        IKeyValueRepo repo = GetNewRepo();

        var TEST_Name = TestName;
        IIdentity ident = new GenericIdentity(TEST_Name);
        IPrincipal princ = new GenericPrincipal(ident, null);
        Thread.CurrentPrincipal = princ;

        foreach (Person p in People)
        {
            await repo.Update(p.Id, p);
        }

        return repo;
    }

    [Fact]
    public async Task Update_PopulatesMetaInfo()
    {
        var p = new Person("Test1", "Last", 1);
        var TEST_Name = "test name";

        IKeyValueRepo repo = await getRepoWithRecords(p, TEST_Name);

        var result = await repo.GetMeta<Person>(p.Id);
        result?.CreatedBy.Should().Be(TEST_Name);
        result?.UpdatedBy.Should().Be(TEST_Name);

        result?.Value?.Last.Should().Be(p.Last);
        result?.Value?.First.Should().Be(p.First);
        result?.Value?.Id.Should().Be(p.Id);
    }

    [Fact]
    public async Task GetMetaAll_ReturnsAllForType()
    {
        var people = new List<Person>()
        {
            new Person("Test1", "Last", 1),
            new Person("Test2", "Last2", 2),
            new Person("Test3", "Last3", 3)
        };
        var TEST_Name = "test name";

        IKeyValueRepo repo = await getRepoWithRecords(people, TEST_Name);


        var results = await repo.GetMetaAll<Person>();
        results.Count.Should().BeGreaterThan(2);
    }

    [Fact]
    public async Task GetHistory_ShouldReturnMetaList()
    {
        var p = new Person("Test1", "Last", 1);
        var TEST_Name = "test name";

        IKeyValueRepo repo = await getRepoWithRecords(p, TEST_Name);

        var results = await repo.GetHistory<Person>(p.Id);
        results?.Count.Should().BeGreaterThan(0);
        results?.First()?.Value?.First.Should().Be(p.First);
        results?.First()?.Value?.Last.Should().Be(p.Last);
        results?.First()?.CreatedBy.Should().Be(TEST_Name);
        results?.First()?.UpdatedBy.Should().Be(TEST_Name);
    }

    [Fact]
    public async Task RepoShouldHoldValues()
    {
        IKeyValueRepo repo = GetNewRepo();
        var p = new Person("Test", "Last", 1);

        await repo.Update(p.Id.ToString(), p);

        var p2 = await repo.Get<Person>("1");
        p2.Should().NotBeNull();
        p2?.First.Should().Be(p.First);
        p2?.Last.Should().Be(p.Last);

        var p3 = p2 with { First = "Test2" };
        await repo.Update(p3.Id.ToString(), p3);

        var p4 = await repo.Get<Person>(1);
        p4?.First.Should().Be(p3.First);
    }

    [Fact]
    public async Task Get_ShouldReturnVoidForUnknownTypes()
    {
        IKeyValueRepo repo = GetNewRepo();
        var p = await repo.Get<UnusedType>("1");

        p.Should().BeNull();
    }

    [Fact]
    public async Task GetMeta_ShouldReturnVoidForUnknownTypes()
    {
        IKeyValueRepo repo = GetNewRepo();
        var p = await repo.GetMeta<UnusedType>("1");

        p.Should().BeNull();
    }

    [Fact]
    public async Task Get_ShouldReturnNullForMissingIds()
    {
        IKeyValueRepo repo = GetNewRepo();
        var p1 = new Person("Kelly", "Burkhardt", 1);
        await repo.Update(p1.Id, p1);

        var p2 = await repo.Get<Person>(1);
        p2.Should().NotBeNull();

        var nope = await repo.Get<UnusedType>(1);
        nope.Should().BeNull();

        var randomId = Guid.NewGuid().ToString().Substring(0, 8);
        var p3 = await repo.Get<Person>(randomId);
        p3.Should().BeNull();
    }

    [Fact]
    public async Task GetMeta_ShouldReturnNullForMissingIds()
    {
        IKeyValueRepo repo = GetNewRepo();
        var p1 = new Person("Kelly", "Burkhardt", 1);
        await repo.Update(p1.Id, p1);

        var p2 = await repo.GetMeta<Person>(1);
        p2.Should().NotBeNull();

        var nope = await repo.GetMeta<UnusedType>(1);
        nope.Should().BeNull();

        var randomId = Guid.NewGuid().ToString().Substring(0, 8);
        var p3 = await repo.GetMeta<Person>(randomId);
        p3.Should().BeNull();
    }

    [Fact]
    public async Task GetAll_ShouldReturnAllInstances()
    {
        IKeyValueRepo repo = GetNewRepo();
        var p1 = new Person("Kelly", "Burkhardt", 1);
        var p2 = new Person("Drew", "Wu", 2);
        var p3 = new Person("Monroe", "", 3);

        await repo.Update(p1.Id, p1);
        await repo.Update(p2.Id, p2);
        await repo.Update(p3.Id.ToString(), p3);

        var people = await repo.GetAll<Person>();
        people.Count.Should().BeGreaterThanOrEqualTo(3); // Repo holds 4th record from previous test runs
                                                         // Either need to fully reset DB each time or maybe add a Remove / RemoveAll methods

        var l1 = new Location("1", "123 Main", "Dallas");
        var l2 = new Location("2", "456 Front St.", "Tulsa");

        await repo.Update(l1.Id, l1);
        await repo.Update(l2.Id, l2);

        var p4 = new Person("Nick", "Burkhardt", 4);
        await repo.Update(p4.Id.ToString(), p4);

        people = await repo.GetAll<Person>();
        var locals = await repo.GetAll<Location>();

        people.Count().Should().BeGreaterThanOrEqualTo(4);
        locals.Count().Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task GetMetaAll_ShouldReturnAllInstances()
    {
        IKeyValueRepo repo = GetNewRepo();
        var p1 = new Person("Kelly", "Burkhardt", 1);
        var p2 = new Person("Drew", "Wu", 2);
        var p3 = new Person("Monroe", "", 3);

        await repo.Update(p1.Id, p1);
        await repo.Update(p2.Id, p2);
        await repo.Update(p3.Id.ToString(), p3);

        var people = await repo.GetMetaAll<Person>();
        people.Count.Should().BeGreaterThanOrEqualTo(3);
        // Repo holds records from previous test runs
        // Either need to fully reset DB each time or maybe add a Remove / RemoveAll methods

        var l1 = new Location("1", "123 Main", "Dallas");
        var l2 = new Location("2", "456 Front St.", "Tulsa");

        await repo.Update(l1.Id, l1);
        await repo.Update(l2.Id, l2);

        var p4 = new Person("Nick", "Burkhardt", 4);
        await repo.Update(p4.Id.ToString(), p4);

        people = await repo.GetMetaAll<Person>();
        var locals = await repo.GetMetaAll<Location>();

        people.Count().Should().BeGreaterThanOrEqualTo(4);
        locals.Count().Should().BeGreaterThanOrEqualTo(2);
    }
}

public record Person(string First, string Last, int Id);
public record Location(string Id, string Street, string City);
public record UnusedType();
