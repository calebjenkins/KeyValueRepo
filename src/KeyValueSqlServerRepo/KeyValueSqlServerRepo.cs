using System.Data.SqlClient;

namespace Calebs.Data.KeyValueRepo.SqlServer;

public class KeyValueSqlServerRepo : IKeyValueRepo
{
    //private var _db = new SqlClient();

    public T? Get<T>(string key) where T : class
    {
        throw new NotImplementedException();
    }

    public IList<T>? GetAll<T>() where T : class
    {
        throw new NotImplementedException();
    }

    public void Update<T>(string key, T value) where T : class
    {
        throw new NotImplementedException();
    }
}
