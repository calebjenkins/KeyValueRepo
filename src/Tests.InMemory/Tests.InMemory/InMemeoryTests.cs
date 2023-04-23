namespace Tests.InMemory;

public class InMemeoryTests
{
    [Fact]
    public void InMemoryShouldHoldValues()
    {
        IKeyValueRepo repo = new KeyValueInMemory();
        var p = new Person("Test", "Last", 1);

        repo.Update(p.Id.ToString(), p);

        var p2 = repo.Get<Person>("1");
        p2.Should().NotBeNull();
        p2.First.Should().Be("Test");
        
    }

    [Fact]
    public void ShouldReturnVoidForUnknownTypes()
    {
        IKeyValueRepo repo = new KeyValueInMemory();
        var p = repo.Get<Person>("1");

        p.Should().BeNull();
    }
}

public record Person (string First, string Last, int Id);
public class Location
{
    public string Id { get; set; }
    public string Street {get;set;}
    public string City { get;set;}
}
