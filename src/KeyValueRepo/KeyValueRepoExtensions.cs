namespace Calebs.Data.KeyValueRepo;

public static class KeyValueRepoExtensions
{
    public static Task<MetaObject<T>?> GetMeta<T>(this IKeyValueRepo repo, int key) where T : class => repo.GetMeta<T>(key.ToString());
    public static Task<T?> Get<T>(this IKeyValueRepo repo, int key) where T : class => repo.Get<T>(key.ToString());
    public static Task<IList<MetaObject<T>>?> GetHistory<T>(this IKeyValueRepo repo, int key) where T : class => repo.GetHistory<T>(key.ToString());
    public static Task Update<T>(this IKeyValueRepo repo, int key, T value) where T : class => repo.Update<T>(key.ToString(), value);
}
