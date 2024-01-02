
public interface IKeyValueRepo
{
    Task Update<T>(string key, T value) where T : class;
    Task<T?> Get<T>(string key) where T : class;
    Task<IList<T>> GetAll<T>() where T : class;
    Task<MetaObject<T>?> GetMeta<T>(string key) where T : class;
    Task<IList<MetaObject<T>>?> GetHistory<T>(string key) where T : class;
}