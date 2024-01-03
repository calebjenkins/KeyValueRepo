
public class MetaObject<T> where T : class
{
    public string? CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public string? UpdatedBy { get; init; }
    public DateTime UpdatedOn { get; init; }
    public T? Value { get; set; }
}