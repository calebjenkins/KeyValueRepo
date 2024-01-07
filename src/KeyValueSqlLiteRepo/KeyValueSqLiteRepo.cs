
using Newtonsoft.Json.Linq;
using System;

namespace Calebs.Data.KeyValueRepo.SqlLite;

public class KeyValueSqLiteRepo : IKeyValueRepo
{ 
    private ILogger<KeyValueSqLiteRepo> _logger;
    private KeyValueSqlLiteOptions _options;
    private SchemaValidator _validator;

    private SqliteConnection _db;

    public KeyValueSqLiteRepo(ILogger<KeyValueSqLiteRepo> Logger, SchemaValidator Validator, KeyValueSqlLiteOptions? Options = null)
    {
        _logger = Logger ?? throw new ArgumentNullException(nameof(Logger));
        _options = Options ?? new KeyValueSqlLiteOptions();
        _validator = Validator?? throw new ArgumentNullException(nameof(Validator));

        _db = new SqliteConnection(_options.ConnectionString);

        if (_options.ValidateSchemaOnStartUp)
        {
            var validate =  ValidateSchema();
            validate.Wait();

            bool isValid = validate.Result;
            if (!isValid)
            {
                _logger.LogError( $"KeyValueRepo ({_options.DefaultTableName}) schema could not be validated - please see log for additional information." );
            }
        }
    }

    public SqliteConnection DbConn { get { return _db; } }
    public string DatabaseFileName => Path.GetFullPath(_db.DataSource);


    public async Task<bool> ValidateSchema()
    {
        _logger.LogInformation("Validating KeyValueRepo Database Schema");

        await _db.OpenAsync();

        var result = await _validator.ValidateSchema(_options, _db);

        _db.Close();

        var resultMsg = (result.HasError) ? "there were Errors. Enable Debug to log the errors" : "Everything looked good";

        _logger.LogInformation($"Finished Validating Schmema - {resultMsg}");
        _logger.LogDebug($"Schema validation results: {result.Messages.ToString()}");
        
        return (!result.HasError);
    }
    public async Task ReleaseForCleanUp()
    {
        SqliteConnection.ClearPool(_db);
        await _db.CloseAsync();
        await _db.DisposeAsync();
        SqliteConnection.ClearAllPools();

        GC.Collect();
        GC.WaitForPendingFinalizers();
        Thread.Sleep(1000);
        return;
    }

    public async Task<MetaObject<T>?> GetMeta<T>(string key) where T : class
    {
        return await selectMetaRecord<T>(key);
    }
    public async Task<T?> Get<T>(string key) where T : class
    {
        var result = await GetMeta<T>(key);
        return result?.Value;
    }
    public async Task<IList<T>> GetAll<T>() where T : class
    {
        var list = await selectAllRecords<T>();
        return list;
    }
    public async Task<IList<MetaObject<T>>> GetHistory<T>(string key) where T : class
    {
        var valueType = typeof(T).ToString();

        try
        {
            _db.ConfirmOpen();

            var results = new List<MetaObject<T>>();
            var command = commandForSelectOneIncludeHistory(key, valueType, ref _db, _options);

            using var reader = await command.ExecuteReaderAsync();
            while (reader.Read())
            {
                var keyRecord = reader.GetString(0);  // unused - useful for debugging
                var typeRecord = reader.GetString(1); // unused - useful for debugging
                var valueRecord = reader.GetString(2);
                var createdBy = reader.GetString(3);
                var createdOn = reader.GetString(4);
                var updatedBy = reader.GetString(5);
                var updatedOn = reader.GetString(6);

                var cDt = DateTime.Parse(createdOn);
                var uDt = DateTime.Parse(updatedOn);

                var mo = new MetaObject<T>()
                {
                    Value = valueRecord.FromJson<T>(),
                    CreatedBy = createdBy,
                    CreatedOn = cDt,
                    UpdatedBy = updatedBy,
                    UpdatedOn = uDt
                };
                results.Add(mo);
            }

            return results;
        }
        catch (SqliteException ex)
        {
            _logger.LogError("Error in Select Record: {0}", ex.Message);
            throw;
        }
    }
    public async Task<IList<MetaObject<T>>> GetMetaAll<T>() where T : class
    {
        return await selectAllMetaRecords<T>();
    }
    public async Task<bool> RemoveAll<T>() where T : class
    {
        var valueType = typeof(T).ToString();
        try
        {
            _db.ConfirmOpen();
            var command = commandForClearAll(valueType, _db, _options);
            var result = await command.ExecuteNonQueryAsync();
            return true;
        }
        catch (SqliteException ex)
        {
            _logger.LogError("Error validating Default Table: {0}", ex.Message);
            return false;
        }

    }

    public async Task Update<T>(string key, T value) where T : class
    {
        var valueType = typeof(T).ToString();
        var user = getCurrentUser();

        // Insert or upsert?

        if(_options.TrackHistory)
        {
            // Insert - if we're tracking history, we always to Inserts
            var result = await insertRecord(key, valueType, value.ToJson(), user);
        }
        else
        {
            // upsert - this will do an insert or update if there is a PK conflict
            var result = await updateRecord(key, valueType, value.ToJson(), user);
        }
        
    }

#pragma warning disable CS8603 // Possible null reference return.
    private string getCurrentUser()
    {
        string? user =  Thread.CurrentPrincipal?.Identity?.Name;
        return user.IsNotNullOrEmpty() ? user : string.Empty;
    }
#pragma warning restore CS8603 // Possible null reference return.

    private async Task<bool> insertRecord(string key, string valueType, string value, string user)
    {
        try
        {
            _db.ConfirmOpen();

            var command = commandForInsert(key, valueType, value, user, _db, _options);
            var result = await command.ExecuteNonQueryAsync();
            return (result > 0)? true : false;
        }
        catch (SqliteException ex)
        {
            _logger.LogError("Error validating Default Table: {0}", ex.Message);
            return false;
        }
    }
    private async Task<bool> updateRecord(string key, string valueType, string value, string user)
    {
        try
        {
            _db.ConfirmOpen();

            // var command = commandForUpcert(key, valueType, value, user, _db, _options);
            var command = upcertCommand(key, valueType, value, user, _db, _options);
            var result = await command.ExecuteNonQueryAsync();
            return (result > 0) ? true : false;
        }
        catch (SqliteException ex)
        {
            _logger.LogError("Error validating Default Table: {0}", ex.Message);
            return false;
        }
    }

    private async Task<MetaObject<T>?> selectMetaRecord<T>(string key) where T : class
    {
        var valueType = typeof(T).ToString();
        MetaObject<T>? result = null; 

        try
        {
            _db.ConfirmOpen();

            var command = commandForSelectOne(key, valueType, ref _db, _options);
            using var reader = await command.ExecuteReaderAsync();
            while (reader.Read())
            {
                var keyRecord = reader.GetString(0);  // unused - useful for debugging
                var typeRecord = reader.GetString(1); // unused - useful for debugging
                var valueRecord = reader.GetString(2);
                var createdBy = reader.GetString(3);
                var createdOn = reader.GetDateTime(4);
                var updatedBy = reader.GetString(5);
                var updatedOn = reader.GetDateTime(6);

                // DateTime cDt = DateTime.Parse(createdOn);
                // DateTime uDt = DateTime.Parse(updatedOn);


                result = new MetaObject<T>()
                {
                    Value = valueRecord.FromJson<T>(),
                    CreatedBy = createdBy,
                    CreatedOn = createdOn,
                    UpdatedBy = updatedBy,
                    UpdatedOn = updatedOn
                };
            }
            return result;
        }
        catch (SqliteException ex)
        {
            _logger.LogError("Error in Select Record: {0}", ex.Message);
            throw;
        }
    }

    private async Task<string> selectRecord(string key, string valueType)
    {
        var Value = string.Empty;
        try
        {
            _db.ConfirmOpen();

            var command = commandForSelectOne(key, valueType, ref _db, _options);
            using var reader = await command.ExecuteReaderAsync();
            while (reader.Read())
            {
                /*
                 SELECT {Opt.ColumnPrefix + Opt.KeyColumnName},
                            {Opt.ColumnPrefix + Opt.TypeValueColumnName},
                            {Opt.ColumnPrefix + Opt.ValueColumnName},
                            {Opt.ColumnPrefix + Opt.CreateByColumnName},
                            {Opt.ColumnPrefix + Opt.CreateOnColumnName},
                            {Opt.ColumnPrefix + Opt.UpdatedByColumnName},
                            {Opt.ColumnPrefix + Opt.UpdatedOnColumnName}
                 */
                var v1 = reader.GetString(0);
                var v2 = reader.GetString(1);
                Value = reader.GetString(2);
                var v3 = reader.GetString(3);
               
            }
            return Value;
        }
        catch (SqliteException ex)
        {
            _logger.LogError("Error in Select Record: {0}", ex.Message);
            throw;
        }
    }

    private async Task<IList<T>> selectAllRecords<T>() where T: class
    {
        var valueType = typeof(T).ToString();

        IList<T> Values = new List<T>();
        try
        {
            _db.ConfirmOpen();

            var command = commandForSelectAll(valueType, ref _db, _options);
            using var reader = await command.ExecuteReaderAsync();
            while (reader.Read())
            {
                T itm = reader.GetString(2).FromJson<T>();
                Values.Add(itm);
            }

            return Values;
        }
        catch (SqliteException ex)
        {
            _logger.LogError("Error validating Default Table: {0}", ex.Message);
            throw;
        }
    }
    private async Task<IList<MetaObject<T>>> selectAllMetaRecords<T>() where T : class
    {
        var valueType = typeof(T).ToString();
        var Values = new List<MetaObject<T>>();

        try
        {
            _db.ConfirmOpen();

            var command = commandForSelectAll(valueType, ref _db, _options);
            using var reader = await command.ExecuteReaderAsync();
            while (reader.Read())
            {
                MetaObject<T> mo = new MetaObject<T>()
                {
                    Value = reader.GetString(2).FromJson<T>(),
                    CreatedBy = reader.GetString(3),
                    CreatedOn = reader.GetDateTime(4),
                    UpdatedBy = reader.GetString(5),
                    UpdatedOn = reader.GetDateTime(6)
                };

                Values.Add(mo);
            }

            return Values;
        }
        catch (SqliteException ex)
        {
            _logger.LogError("Error validating Default Table: {0}", ex.Message);
            throw;
        }
    }

    private SqliteCommand commandForSelectOne(string key, string valueType, ref SqliteConnection Db, KeyValueSqlLiteOptions Opt)
    {
        var cmd = Db.CreateCommand();
        cmd.Parameters.AddWithValue("$Key_Value", key);
        cmd.Parameters.AddWithValue("$ValueType_Value", valueType);

        var historySql = (!Opt.TrackHistory) ? string.Empty : $@"AND {Opt.ColumnPrefix + Opt.UpdatedOnColumnName} = (SELECT MAX({Opt.ColumnPrefix + Opt.UpdatedOnColumnName})
                                            FROM { Opt.DefaultTableName }
                                            WHERE {Opt.ColumnPrefix + Opt.KeyColumnName} = $Key_Value
                                             AND {Opt.ColumnPrefix + Opt.TypeColumnName} = $ValueType_Value)";

        var sql = $@"SELECT {Opt.ColumnPrefix + Opt.KeyColumnName},
                            {Opt.ColumnPrefix + Opt.TypeColumnName},
                            {Opt.ColumnPrefix + Opt.ValueColumnName},
                            {Opt.ColumnPrefix + Opt.CreateByColumnName},
                            datetime({Opt.ColumnPrefix + Opt.CreateOnColumnName}, 'unixepoch'),
                            {Opt.ColumnPrefix + Opt.UpdatedByColumnName},
                            datetime({Opt.ColumnPrefix + Opt.UpdatedOnColumnName}, 'unixepoch')
                            
                    FROM { Opt.DefaultTableName }

                    WHERE {Opt.ColumnPrefix + Opt.KeyColumnName} = $Key_Value
                      AND {Opt.ColumnPrefix + Opt.TypeColumnName} = $ValueType_Value
                      {historySql} ;";

        cmd.CommandText = sql;
        return cmd;

    }
    private SqliteCommand commandForSelectOneIncludeHistory(string key, string valueType, ref SqliteConnection Db, KeyValueSqlLiteOptions Opt)
    {
        var cmd = Db.CreateCommand();
        cmd.Parameters.AddWithValue("$Key_Value", key);
        cmd.Parameters.AddWithValue("$Type_Value", valueType);

        var sql = $@"SELECT { Opt.ColumnPrefix + Opt.KeyColumnName },
                            { Opt.ColumnPrefix + Opt.TypeColumnName },
                            { Opt.ColumnPrefix + Opt.ValueColumnName },
                            { Opt.ColumnPrefix + Opt.CreateByColumnName },
                            datetime({Opt.ColumnPrefix + Opt.CreateOnColumnName}, 'unixepoch'),
                            {Opt.ColumnPrefix + Opt.UpdatedByColumnName},
                            datetime({Opt.ColumnPrefix + Opt.UpdatedOnColumnName}, 'unixepoch')

                    FROM { Opt.DefaultTableName }

                    WHERE { Opt.ColumnPrefix + Opt.KeyColumnName } = $Key_Value
                      AND { Opt.ColumnPrefix + Opt.TypeColumnName } = $Type_Value
                 ORDER BY { Opt.ColumnPrefix + Opt.UpdatedOnColumnName } ASC;";

        cmd.CommandText = sql;
        return cmd;

    }
    private SqliteCommand commandForSelectAll(string valueType, ref SqliteConnection Db, KeyValueSqlLiteOptions Opt)
    {
        var cmd = Db.CreateCommand();
        cmd.Parameters.AddWithValue("$ValueType_Value", valueType);

        //datetime($CreatedOn_Column, 'unixepoch'),
        var sql = $@"
            SELECT t.{Opt.ColumnPrefix + Opt.KeyColumnName},
                   t.{Opt.ColumnPrefix + Opt.TypeColumnName},
                   t.{Opt.ColumnPrefix + Opt.ValueColumnName}, 
                   t.{Opt.ColumnPrefix + Opt.CreateByColumnName},
                   datetime(t.{Opt.ColumnPrefix + Opt.CreateOnColumnName}, 'unixepoch'),
                   t.{Opt.ColumnPrefix + Opt.UpdatedByColumnName},
                   datetime(t.{Opt.ColumnPrefix + Opt.UpdatedOnColumnName}, 'unixepoch')
            FROM {Opt.DefaultTableName} as t
            JOIN (
                SELECT {Opt.ColumnPrefix + Opt.KeyColumnName},
                       MAX({Opt.ColumnPrefix + Opt.UpdatedOnColumnName}) AS MaxDate
                FROM {Opt.DefaultTableName}
                WHERE {Opt.ColumnPrefix + Opt.TypeColumnName} = $ValueType_Value
                GROUP BY {Opt.ColumnPrefix + Opt.KeyColumnName}
            ) as sub
            ON t.{Opt.ColumnPrefix + Opt.KeyColumnName} = sub.{Opt.ColumnPrefix + Opt.KeyColumnName}
            AND t.{Opt.ColumnPrefix + Opt.UpdatedOnColumnName} = sub.MaxDate; ";

        cmd.CommandText = sql;

        return cmd;

    }
    private SqliteCommand commandForInsert(string key, string valueType, string value, string User, SqliteConnection Db, KeyValueSqlLiteOptions Opt)
    {
        var utcNow = DateTime.UtcNow;
        var cmd = Db.CreateCommand();

        cmd.Parameters.AddWithValue("$Key_Value", key);
        cmd.Parameters.AddWithValue("$ValueType_Value", valueType);
        cmd.Parameters.AddWithValue("$Value_Value", value);

        cmd.Parameters.AddWithValue("$CreatedBy_Value", User);
        cmd.Parameters.AddWithValue("$CreatedOn_Value", utcNow);
        cmd.Parameters.AddWithValue("$UpdatedBy_Value", User);
        cmd.Parameters.AddWithValue("$UpdatedOn_Value", utcNow);

        var sql = $@"INSERT INTO { Opt.DefaultTableName }
                   (
                    {Opt.ColumnPrefix + Opt.KeyColumnName},
                    {Opt.ColumnPrefix + Opt.TypeColumnName},
                    {Opt.ColumnPrefix + Opt.TypeColumnName},
                    {Opt.ColumnPrefix + Opt.CreateByColumnName},
                    {Opt.ColumnPrefix + Opt.CreateOnColumnName},
                    {Opt.ColumnPrefix + Opt.UpdatedByColumnName},
                    {Opt.ColumnPrefix + Opt.UpdatedOnColumnName}
                   ) VALUES (
                     $Key_Value,
                     $ValueType_Value,
                     $Value_Value,
                     $CreatedBy_Value,
                     unixepoch($CreatedOn_Value),
                     $UpdatedBy_Value,
                     unixepoch($UpdatedOn_Value)
                   );";

        cmd.CommandText = sql;

        return cmd;

    }
    private SqliteCommand commandForUpcert(string key, string valueType, string value, string User, SqliteConnection Db, KeyValueSqlLiteOptions Opt)
    {
        var utcNow = DateTime.UtcNow;
        var cmd = Db.CreateCommand();
        
        cmd.Parameters.AddWithValue("$Key_Value", key);
        cmd.Parameters.AddWithValue("$Type_Value", valueType);
        cmd.Parameters.AddWithValue("$Value_Value", value);

        cmd.Parameters.AddWithValue("$UpdatedBy_Value", User);
        cmd.Parameters.AddWithValue("$UpdatedOn_Value", utcNow);

        var sql = $@"
                     UPDATE {Opt.DefaultTableName}
                        SET {Opt.ColumnPrefix + Opt.ValueColumnName} = $Value_Value,
                            {Opt.ColumnPrefix + Opt.UpdatedByColumnName} = $UpdatedBy_Value,
                            {Opt.ColumnPrefix + Opt.UpdatedOnColumnName} = unixepoch($UpdatedOn_Value)
                      WHERE {Opt.ColumnPrefix + Opt.KeyColumnName} = $Key_Value
                        AND {Opt.ColumnPrefix + Opt.TypeColumnName} = $Type_Value;

                     INSERT OR IGNORE INTO {Opt.DefaultTableName}
                            ( {Opt.ColumnPrefix + Opt.KeyColumnName},
                            {Opt.ColumnPrefix + Opt.TypeColumnName},
                            {Opt.ColumnPrefix + Opt.ValueColumnName},
                            {Opt.ColumnPrefix + Opt.CreateByColumnName},
                            {Opt.ColumnPrefix + Opt.CreateOnColumnName},
                            {Opt.ColumnPrefix + Opt.UpdatedByColumnName},
                            {Opt.ColumnPrefix + Opt.UpdatedOnColumnName})
                    VALUES ($Key_Value,
                            $Type_Value,
                            $Value_Value,
                            $UpdatedBy_Value,
                            unixepoch($UpdatedOn_Value),
                            $UpdatedBy_Value,
                            unixepoch($UpdatedOn_Value) );";
                
        cmd.CommandText = sql;
        Debug.WriteLine($"Upsert SQL: {sql}");

        return cmd;

    }
    private SqliteCommand upcertCommand(string key, string valueType, string value, string User, SqliteConnection Db, KeyValueSqlLiteOptions Opt)
    {
        var utcNow = DateTime.UtcNow;
        var cmd = Db.CreateCommand();

        cmd.Parameters.AddWithValue("$Key_Value", key);
        cmd.Parameters.AddWithValue("$Type_Value", valueType);
        cmd.Parameters.AddWithValue("$Value_Value", value);

        cmd.Parameters.AddWithValue("$UpdatedBy_Value", User);
        cmd.Parameters.AddWithValue("$UpdatedOn_Value", utcNow);

        var sql = $@"
            INSERT INTO {Opt.DefaultTableName} (
                {Opt.ColumnPrefix + Opt.KeyColumnName},
                {Opt.ColumnPrefix + Opt.TypeColumnName},
                {Opt.ColumnPrefix + Opt.ValueColumnName},
                {Opt.ColumnPrefix + Opt.CreateByColumnName},
                {Opt.ColumnPrefix + Opt.CreateOnColumnName},
                {Opt.ColumnPrefix + Opt.UpdatedByColumnName},
                {Opt.ColumnPrefix + Opt.UpdatedOnColumnName}
                ) VALUES (
                $Key_Value,
                $Type_Value,
                $Value_Value,
                $UpdatedBy_Value,
                unixepoch($UpdatedOn_Value),
                $UpdatedBy_Value,
                unixepoch($UpdatedOn_Value)
                )
             ON CONFLICT({Opt.ColumnPrefix + Opt.KeyColumnName},{Opt.ColumnPrefix + Opt.TypeColumnName})
                DO UPDATE SET
                    {Opt.ColumnPrefix + Opt.ValueColumnName} = $Value_Value,
                    {Opt.ColumnPrefix + Opt.UpdatedByColumnName} = $UpdatedBy_Value,
                    {Opt.ColumnPrefix + Opt.UpdatedOnColumnName} = unixepoch($UpdatedOn_Value)
                WHERE {Opt.ColumnPrefix + Opt.KeyColumnName} = $Key_Value
                AND {Opt.ColumnPrefix + Opt.TypeColumnName} = $Type_Value;";

        cmd.CommandText = sql;
        Debug.WriteLine($"Upsert SQL: {sql}");

        return cmd;
    }

    private SqliteCommand commandForClearAll(string valueType, SqliteConnection Db, KeyValueSqlLiteOptions Opt)
    {
        var cmd = Db.CreateCommand();

        cmd.Parameters.AddWithValue("$Type_Value", valueType);

        var sql = $@"DELETE 
                    FROM {Opt.DefaultTableName}
                    WHERE {Opt.ColumnPrefix + Opt.TypeColumnName} = $Type_Value;";

        cmd.CommandText = sql;
        return cmd;
    }
}
