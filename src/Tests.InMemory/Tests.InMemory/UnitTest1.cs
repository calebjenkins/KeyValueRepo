namespace Tests.InMemory;

public class UnitTest1
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
}

public record Person (string First, string Last, int Id);
