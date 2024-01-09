
using System.Security.Principal;

namespace KeyValueSqlLiteRepoTests;


public abstract class KeyValueBaseTests
{
    [Fact]
    public virtual async Task Dispose()
    {
        var repo = GetNewRepo();
        // clean up
        await repo.RemoveAll<Person>();
        await repo.RemoveAll<Location>();
    }

    public virtual IKeyValueRepo GetNewRepo()
    {
        return new KeyValueInMemory();
    }

    internal string getRandomId()
    {
        return Guid.NewGuid().ToString().Substring(0, 8);
    }

    internal void ApplyNameToThread(string UserName)
    {
        IIdentity ident = new GenericIdentity(UserName);
        IPrincipal princ = new GenericPrincipal(ident, null);
        Thread.CurrentPrincipal = princ;
    }

    private async Task<IKeyValueRepo> getRepoWithRecords(Person p, string TestName = "TestName")
    {
        var people = new List<Person>() { p };
        return await getRepoWithRecords(people, TestName);
    }

    private async Task<IKeyValueRepo> getRepoWithRecords(IList<Person> People, string TestName = "TestName")
    {
        ApplyNameToThread(TestName);

        IKeyValueRepo repo = GetNewRepo();
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
        result?.CreatedOn.Should<DateTime>().NotBeNull();
        result?.UpdatedOn.Should<DateTime>().NotBeNull();

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
        results?.First()?.CreatedOn.Should<DateTime>().NotBeNull();
        results?.First()?.UpdatedBy.Should().Be(TEST_Name);
        results?.First()?.UpdatedOn.Should<DateTime>().NotBeNull();
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

    [Fact]
    public async Task Remove_ShouldRemoveOnlySpecifiedRecord()
    {
        var p1 = new Person("Kelly", "Burkhardt", 1);
        var p2 = new Person("Drew", "Wu", 2);
        var p3 = new Person("Monroe", "", 3);
        var people = new List<Person>() { p1, p2, p3 };
        var TEST_USER = "test name";
        var repo = await getRepoWithRecords(people, TEST_USER);

        var allPeople = await repo.GetAll<Person>();

        var peopleCount = allPeople.Count();
        peopleCount.Should().BeGreaterThan(2);
        var drew = await repo.Get<Person>(p2.Id);
        drew.Should().NotBeNull();

        await repo.Remove<Person>(p2.Id);
        var nullDrew = await repo.Get<Person>(p2.Id);
        nullDrew.Should().BeNull();

        var newP1 = await repo.Get<Person>(p1.Id);
        newP1.Should().NotBeNull();
        newP1.Should().BeEquivalentTo(p1);

        var newP3 = await repo.Get<Person>(p3.Id);
        newP3.Should().NotBeNull();
        newP3.Should().BeEquivalentTo(p3);
    }

    [Fact]
    public async Task Remove_MissingIdShouldNotThrowAnError()
    {
        var p1 = new Person("Kelly", "Burkhardt", 1);
        var p2 = new Person("Drew", "Wu", 2);
        var p3 = new Person("Monroe", "", 3);
        var people = new List<Person>() { p1, p2, p3 };
        var repo = await getRepoWithRecords(people);

        try
        {
            await repo.Remove<Person>(4);
            true.Should().BeTrue();
        }
        catch (Exception ex)
        {
            ex.Should().BeNull();
        }
    }

    [Fact]
    public async Task RemoveAll_ShouldRemoveAllSpecifiedTypes()
    {
        var repo = GetNewRepo();

        await repo.Update(1, new Person("Kelly", "Burkhardt", 1));
        await repo.Update("1", new Location("1", "123 Main", "Portland"));
        await repo.Update("2", new Location("2", "123 Main", "Fort Worth"));
        await repo.Update(2, new Person("Drew", "Wu", 2));
        await repo.Update(3, new Person("Monroe", "", 3));
        await repo.Update("3", new Location("3", "123 Main", "Dallas"));
        await repo.Update("4", new Location("4", "123 Main Street", "Salem"));

        var people = await repo.GetAll<Person>();
        people.Count.Should().BeGreaterThan(2);

        var places = await repo.GetAll<Location>();
        places.Count.Should().BeGreaterThan(3);

        await repo.RemoveAll<Person>();

        people = await repo.GetAll<Person>();
        people.Count.Should().Be(0);

        places = await repo.GetAll<Location>();
        places.Count.Should().BeGreaterThan(3);
    }

    [Fact]
    public async Task RemoveAll_MissingTypeShouldNotThrowAnError()
    {
        var repo = GetNewRepo();

        await repo.Update(1, new Person("Kelly", "Burkhardt", 1));
        await repo.Update("1", new Location("1", "123 Main", "Portland"));
        await repo.Update("2", new Location("2", "123 Main", "Fort Worth"));
        await repo.Update(2, new Person("Drew", "Wu", 2));
        await repo.Update(3, new Person("Monroe", "", 3));
        await repo.Update("3", new Location("3", "123 Main", "Dallas"));
        await repo.Update("4", new Location("4", "123 Main Street", "Salem"));

        var people = await repo.GetAll<Person>();
        people.Count.Should().BeGreaterThan(2);

        var places = await repo.GetAll<Location>();
        places.Count.Should().BeGreaterThan(3);

        try
        {
            await repo.RemoveAll<UnusedType>();
            true.Should().BeTrue();
        }
        catch (Exception ex)
        {
            ex.Should().BeNull();
        }
    }
}

public record Person (string First, string Last, int Id);
public record Location (string Id, string Street, string City);
public record UnusedType();
