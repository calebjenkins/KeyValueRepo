
/// <summary>
///  Recomend Running IKeyValueRepo as Singleton
/// </summary>
public class KeyValueInMemory : IKeyValueRepo
{
    // Note a lot of "await Task.Run(".. etc in here, since this is InMemory, Tasks/Await not really needed, but the interface is designed for networked databases where that is generally preffered. 
    // Using in Memeory JSON formating (string)
    // Recomend Running IKeyValueRepo as Singleton
    Dictionary<string, Dictionary<string, string>> _data = new Dictionary<string, Dictionary<string, string>>();

    public async Task<T?> Get<T>(string key) where T : class
    {
        string typeKey = typeof(T).ToString();

        if (_data.ContainsKey(typeKey))
        {
            if(_data[typeKey].ContainsKey(key))
            {
                var data = _data[typeKey][key].FromJson<MetaObject<T>>()?.Value;
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
                    var data = _data[typeKey][key].FromJson<MetaObject<T>>().Value;
                    if (data != null)
                    {
                        list.Add(data);
                    }
                }
            }
        }

        return await Task.FromResult<IList<T>>(list);
    }

    public async Task<IList<MetaObject<T>>> GetMetaAll<T>() where T : class
    {
        string typeKey = typeof(T).ToString();
        List<MetaObject<T>> list = new List<MetaObject<T>>();

        if (_data.ContainsKey(typeKey))
        {
            var keys = _data[typeKey].Keys;
            foreach (var key in keys)
            {
                if (_data[typeKey].ContainsKey(key))
                {
                    var data = _data[typeKey][key].FromJson<MetaObject<T>>();
                    if (data != null)
                    {
                        list.Add(data);
                    }
                }
            }
        }

        return await Task.FromResult<IList<MetaObject<T>>>(list);
    }

    public async Task<IList<MetaObject<T>>> GetHistory<T>(string key) where T : class
    {
        string typeKey = typeof(T).ToString();

        var list = new List<MetaObject<T>>();

        if (_data.ContainsKey(typeKey))
        {
            var keys = _data[typeKey].Keys;
            foreach (var k in keys)
            {
                if (_data[typeKey].ContainsKey(k))
                {
                    var data = _data[typeKey][k].FromJson<MetaObject<T>>();
                    list.Add(data);
                }
            }
        }

        return await Task.FromResult<IList<MetaObject<T>>>(list);
    }

    public async Task<MetaObject<T>?> GetMeta<T>(string key) where T : class
    {
        string typeKey = typeof(T).ToString();

        if (_data.ContainsKey(typeKey))
        {
            if (_data[typeKey].ContainsKey(key))
            {
                var data = _data[typeKey][key].FromJson<MetaObject<T>>();
                return await Task.FromResult(data);
            }
        }
        return null;
    }

    public async Task Remove<T>(string key) where T : class
    {
        string typeKey = typeof(T).ToString();
        if (_data.ContainsKey(typeKey) && _data[typeKey].ContainsKey(key))
        {
            await Task.Run(()=> _data[typeKey].Remove(key));
        }
    }

    public async Task RemoveAll<T>() where T : class
    {
        string typeKey = typeof(T).ToString();
        if (_data.ContainsKey(typeKey))
        {
            await Task.Run(() => _data.Remove(typeKey));
        }
    }

    public async Task Update<T>(string key, T value) where T : class
    {
        var ident = Thread.CurrentPrincipal?.Identity?.Name ?? "";
        var now = DateTime.Now;

        string typeKey = typeof(T).ToString();
        var mo = new MetaObject<T>()
        {
            Value = value,
            CreatedBy = ident,
            CreatedOn = now,
            UpdatedBy = ident,
            UpdatedOn = now
        };

        // Create type dictionary if does not exist
        if (!_data.ContainsKey(typeKey))
        {
            var newDict = new Dictionary<string, string>();
            //newDict.Add(key, value.ToJson());
            newDict.Add(key, mo.ToJson());

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
                // _data[typeKey].Add(key, value.ToJson());
                _data[typeKey].Add(key, mo.ToJson());
            });
        }
        else
        {
            await Task.Run(() =>
            {
                var existingData = _data[typeKey][key].FromJson<MetaObject<T>>();
                existingData.CreatedBy = mo.CreatedBy;
                existingData.CreatedOn = mo.CreatedOn;
                existingData.Value = mo.Value;

                _data[typeKey][key] = existingData.ToJson();
            });
        }
    }
}