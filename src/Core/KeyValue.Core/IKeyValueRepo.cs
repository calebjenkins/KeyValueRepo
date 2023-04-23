﻿namespace Calebs.Data.KeyValues;

public interface IKeyValueRepo
{
    T? Get<T>(string key) where T : class;
    IList<T>? GetAll<T>() where T : class;

    void Update<T>(string key, T value) where T : class;
}