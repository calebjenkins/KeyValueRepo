namespace Calebs.Data.KeyValueRepo;

public class KeyValueInMemory : IKeyValueRepo
{
    // Using in Memeory JSON formating (string)
    Dictionary<string, Dictionary<string, string>> _data = new Dictionary<string, Dictionary<string, string>>();

    public T? Get<T>(string key) where T : class
    {
        string typeKey = typeof(T).ToString();

        if (_data.ContainsKey(typeKey))
        {
            if(_data[typeKey].ContainsKey(key))
            {
                T? data = _data[typeKey][key].FromJson<T>();
                return data;
            }
        }

        return null;
    }

    public IList<T> GetAll<T>() where T : class
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

        return list;
    }

    public void Update<T>(string key, T value) where T : class
    {
        string typeKey = typeof(T).ToString();

        // Create type dictionary if does now exist
        if (!_data.ContainsKey(typeKey))
        {
            var newDict = new Dictionary<string, string>();
            newDict.Add(key, value.ToJson());

            _data.Add(typeKey, newDict);
        }

        // upsert value - create if does not exist, update if does
        if (!_data[typeKey].ContainsKey(key))
        {
            _data[typeKey].Add(key, value.ToJson());
        }
        else
        {
            _data[typeKey][key] = value.ToJson();
        }
    }
}