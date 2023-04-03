namespace Tests.InMemory;

public class InMemoryTests
{
    private readonly IKeyValueRepo _repo = new KeyValueInMemory();

    [Fact]
    public void shouldReturnNullOnUnknownTypes()
    {
        var result = _repo.Get<ExampleModel>("123");
        result.Should().BeNull();
    }

    [Fact]
    public void shouldAddNewTypesAndAddValue()
    {
        var id = Guid.NewGuid().ToString();
        var ex1 = new ExampleModel() { First = "Dave", Last = "Smith" };

        _repo.Update<ExampleModel>(id, ex1);

        var exOut = _repo.Get<ExampleModel>(id);
        exOut?.First.Should().Be("Dave");
        exOut?.Last.Should().Be("Smith");

    }
}

public class ExampleModel
{
    public string First { get; set; } = string.Empty;
    public string Last { get; set; } = string.Empty;
}