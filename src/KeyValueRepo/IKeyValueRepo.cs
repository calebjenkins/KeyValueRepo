namespace Calebs.Data.KeyValueRepo;

public interface IKeyValueRepo
{
    Task<T?> Get<T>(string key) where T : class;
    Task<T?> Get<T>(int key) where T : class => Get<T>(key.ToString());
    Task<IList<T>> GetAll<T>() where T : class;

    Task Update<T>(string key, T value) where T : class;
    Task Update<T>(int key, T value) where T : class => Update<T>(key.ToString(), value);
}