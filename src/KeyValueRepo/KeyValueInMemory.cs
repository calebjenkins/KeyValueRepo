namespace Calebs.Data.KeyValueRepo;

public class KeyValueInMemory : IKeyValueRepo
{
    // Note a lot of "await Task.Run(".. etc in here, since this is InMemory, Tasks/Await not really needed, but the interface is designed for networked databases where that is generally preffered. 
    // Using in Memory JSON formatting (string)
    Dictionary<string, Dictionary<string, string>> _data = new Dictionary<string, Dictionary<string, string>>();

    public async Task<T?> Get<T>(string key) where T : class
    {
        string typeKey = typeof(T).ToString();

        if (_data.ContainsKey(typeKey))
        {
            if(_data[typeKey].ContainsKey(key))
            {
                T? data = _data[typeKey][key].FromJson<T>();
                return await Task.FromResult<T?>(data);
            }
        }

        return null;
    }

    public async Task<IList<T>> GetAll<T>() where T : class
    {
        string typeKey = typeof(T).ToString();
        List<T> list = new List<T>();

        if (_data.ContainsKey(typeKey))
        {
            var keys = _data[typeKey].Keys;
            foreach (var key in keys)
            {
                if (_data[typeKey].ContainsKey(key))
                {
                    T? data = _data[typeKey][key].FromJson<T>();
                    list.Add(data);
                }
            }
        }

        return await Task.FromResult<IList<T>>(list);
    }

    public async Task Update<T>(string key, T value) where T : class
    {
        string typeKey = typeof(T).ToString();

        // Create type dictionary if does now exist
        if (!_data.ContainsKey(typeKey))
        {
            var newDict = new Dictionary<string, string>();
            newDict.Add(key, value.ToJson());

            await Task.Run(() =>
            {
                _data.Add(typeKey, newDict);
            });
        }

        // upsert value - create if does not exist, update if does
        if (!_data[typeKey].ContainsKey(key))
        {
            await Task.Run(() =>
            {
                _data[typeKey].Add(key, value.ToJson());
            });
        }
        else
        {
            await Task.Run(() =>
            {
                _data[typeKey][key] = value.ToJson();
            });
        }
    }
}