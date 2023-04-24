namespace Calebs.Data.KeyValueRepo;

public interface IKeyValueRepo
{
    T? Get<T>(string key) where T : class;
    T? Get<T>(int key) where T : class => Get<T>(key.ToString());
    IList<T>? GetAll<T>() where T : class;

    void Update<T>(string key, T value) where T : class;
    void Update<T>(int key, T value) where T : class => Update<T>(key.ToString(), value);
}