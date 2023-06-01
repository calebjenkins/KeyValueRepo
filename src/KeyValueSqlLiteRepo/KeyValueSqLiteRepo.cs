
using Calebs.Extensions;
using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;

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
        await _db.CloseAsync();
        await _db.DisposeAsync();
        //SqliteConnection.ClearPool(_db);
        SqliteConnection.ClearAllPools();

        GC.Collect();
        GC.WaitForPendingFinalizers();
        return;
    }

    public async Task<T?> Get<T>(string key) where T : class
    {
        var valueType = typeof(T).ToString();
        var value = await selectRecord(key, valueType);
        var returnValue = value.FromJson<T>();

        return returnValue;
    }

    public async Task<IList<T>> GetAll<T>() where T : class
    {
        if(_options.TrackHistory)
        {
            _logger.LogError("Cannot use GetAll when TrackHistory is enabled, Set TrackHistory to False in te config and try again.");
           // throw new InvalidOperationException("Cannot use GetAll when TrackHistory is enabled");
           return new List<T>();
        }

        var list = await selectAllRecords<T>();
        return list;
    }

    public async Task Update<T>(string key, T value) where T : class
    {
        var valueType = typeof(T).ToString();
        var user = getCurrentUser();

        // Insert or upsert?

        if(_options.TrackHistory)
        {
            // Insert
            var result = await insertRecord(key, valueType, value.ToJson(), user);
        }
        else
        {
            // upsert
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

            var command = getCommandForInsert(key, valueType, value, user, _db, _options);
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

            var command = getCommandForUpcert(key, valueType, value, user, _db, _options);
            var result = await command.ExecuteNonQueryAsync();
            return (result > 0) ? true : false;
        }
        catch (SqliteException ex)
        {
            _logger.LogError("Error validating Default Table: {0}", ex.Message);
            return false;
        }
    }
    private async Task<string> selectRecord(string key, string valueType)
    {
        var Value = string.Empty;
        try
        {
            _db.ConfirmOpen();

            var command = getCommandForSelectOne(key, valueType, _db, _options);
            using var reader = await command.ExecuteReaderAsync();
            while (reader.Read())
            {
                Value = reader.GetString(2);
               
            }
            return Value;
        }
        catch (SqliteException ex)
        {
            _logger.LogError("Error validating Default Table: {0}", ex.Message);
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

            var command = getCommandForSelectAll(valueType, _db, _options);
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

    private SqliteCommand getCommandForSelectOne(string key, string valueType, SqliteConnection Db, KeyValueSqlLiteOptions Opt, bool hasHistory = true)
    {
        var cmd = Db.CreateCommand();
        cmd.Parameters.AddWithValue("$Key_Column", Opt.ColumnPrefix + Opt.KeyColumnName);
        cmd.Parameters.AddWithValue("$Key_Value", key);

        cmd.Parameters.AddWithValue("$ValueType_Column", Opt.ColumnPrefix + Opt.TypeValueColumnName);
        cmd.Parameters.AddWithValue("$ValueType_Value", valueType);

        cmd.Parameters.AddWithValue("$Value_Column", Opt.ColumnPrefix + Opt.TypeValueColumnName);
        cmd.Parameters.AddWithValue("$CreatedBy_Column", Opt.ColumnPrefix + Opt.CreateByColumnName);
        cmd.Parameters.AddWithValue("$CreatedOn_Column", Opt.ColumnPrefix + Opt.CreateOnColumnName);
        cmd.Parameters.AddWithValue("$UpdatedBy_Column", Opt.ColumnPrefix + Opt.UpdatedByColumnName);
        cmd.Parameters.AddWithValue("$UpdatedOn_Column", Opt.ColumnPrefix + Opt.UpdatedOnColumnName);

        cmd.Parameters.AddWithValue("$table_name", Opt.DefaultTableName);

        var historySql = (!hasHistory) ? string.Empty : $@"AND $UpdatedOn_Column = (SELECT MAX($UpdatedOn_Column) FROM $table_name
                                            WHERE $Key_Column = $Key_Value
                                             AND $ValueType_Column = $ValueType_Value)";

        var sql = $@"SELECT $Key_Column,
                            $ValueType_Column,
                            $Value_Column,
                            $CreatedBy_Column,
                            $CreatedOn_Column,
                            $UpdatedBy_Column,
                            $UpdatedOn_Column

                    FROM $table_name;

                    WHERE $Key_Column = $Key_Value
                      AND $ValueType_Column = $ValueType_Value
                      {historySql} ;";

        cmd.CommandText = sql;
        return cmd;

    }

    private SqliteCommand getCommandForSelectOneIncludeHistory(string key, string valueType, SqliteConnection Db, KeyValueSqlLiteOptions Opt)
    {
        var cmd = Db.CreateCommand();
        cmd.Parameters.AddWithValue("$Key_Column", Opt.ColumnPrefix + Opt.KeyColumnName);
        cmd.Parameters.AddWithValue("$Key_Value", key);

        cmd.Parameters.AddWithValue("$ValueType_Column", Opt.ColumnPrefix + Opt.TypeValueColumnName);
        cmd.Parameters.AddWithValue("$ValueType_Value", valueType);

        cmd.Parameters.AddWithValue("$Value_Column", Opt.ColumnPrefix + Opt.TypeValueColumnName);
        cmd.Parameters.AddWithValue("$CreatedBy_Column", Opt.ColumnPrefix + Opt.CreateByColumnName);
        cmd.Parameters.AddWithValue("$CreatedOn_Column", Opt.ColumnPrefix + Opt.CreateOnColumnName);
        cmd.Parameters.AddWithValue("$UpdatedBy_Column", Opt.ColumnPrefix + Opt.UpdatedByColumnName);
        cmd.Parameters.AddWithValue("$UpdatedOn_Column", Opt.ColumnPrefix + Opt.UpdatedOnColumnName);

        cmd.Parameters.AddWithValue("$table_name", Opt.DefaultTableName);

        var sql = $@"SELECT $Key_Column,
                            $ValueType_Column,
                            $Value_Column,
                            $CreatedBy_Column,
                            $CreatedOn_Column,
                            $UpdatedBy_Column,
                            $UpdatedOn_Column

                    FROM $table_name;

                    WHERE $Key_Column = $Key_Value
                      AND $ValueType_Column = $ValueType_Value
                 ORDER BY $UpdatedOn_Column ASC;";

        cmd.CommandText = sql;
        return cmd;

    }

    private SqliteCommand getCommandForSelectAll(string valueType, SqliteConnection Db, KeyValueSqlLiteOptions Opt)
    {
        var cmd = Db.CreateCommand();
        cmd.Parameters.AddWithValue("$Key_Column", Opt.ColumnPrefix + Opt.KeyColumnName);
        cmd.Parameters.AddWithValue("$ValueType_Column", Opt.ColumnPrefix + Opt.TypeValueColumnName);
        cmd.Parameters.AddWithValue("$ValueType_Value", valueType);

        cmd.Parameters.AddWithValue("$Value_Column", Opt.ColumnPrefix + Opt.TypeValueColumnName);
        cmd.Parameters.AddWithValue("$CreatedBy_Column", Opt.ColumnPrefix + Opt.CreateByColumnName);
        cmd.Parameters.AddWithValue("$CreatedOn_Column", Opt.ColumnPrefix + Opt.CreateOnColumnName);
        cmd.Parameters.AddWithValue("$UpdatedBy_Column", Opt.ColumnPrefix + Opt.UpdatedByColumnName);
        cmd.Parameters.AddWithValue("$UpdatedOn_Column", Opt.ColumnPrefix + Opt.UpdatedOnColumnName);

        cmd.Parameters.AddWithValue("$table_name", Opt.DefaultTableName);

        var sql = $@"
            SELECT t.$Key_Column, t.$ValueType_Column, t.$ValueColumn, 
                   t.$CreatedBy_Column, t.$CreatedOn_Column, t.$UpdatedBy_Column, t.$UpdatedOn_Column
            FROM $table_name as t
            JOIN (
                SELECT $Key_Column, MAX($UpdatedOn_Column) AS MaxDate
                FROM [$table_name]
                WHERE $ValueType_Column = $ValueType_Value
                GROUP BY $Key_Column
            ) as sub
            ON t.$Key_Column = sub.$Key_Column
            AND t.$UpdatedOn_Column = sub.MaxDate;
";

        cmd.CommandText = sql;

        return cmd;

    }

    private SqliteCommand getCommandForInsert(string key, string valueType, string value, string User, SqliteConnection Db, KeyValueSqlLiteOptions Opt)
    {
        var utcNow = DateTime.UtcNow;
        var cmd = Db.CreateCommand();
        cmd.Parameters.AddWithValue("$Key_Column", Opt.ColumnPrefix + Opt.KeyColumnName);
        cmd.Parameters.AddWithValue("$Key_Value", key);

        cmd.Parameters.AddWithValue("$ValueType_Column", Opt.ColumnPrefix + Opt.TypeValueColumnName);
        cmd.Parameters.AddWithValue("$ValueType_Value", valueType);

        cmd.Parameters.AddWithValue("$Value_Column", Opt.ColumnPrefix + Opt.TypeValueColumnName);
        cmd.Parameters.AddWithValue("$Value_Value", value);

        cmd.Parameters.AddWithValue("$CreatedBy_Column", Opt.ColumnPrefix + Opt.CreateByColumnName);
        cmd.Parameters.AddWithValue("$CreatedBy_Value", User);

        cmd.Parameters.AddWithValue("$CreatedOn_Column", Opt.ColumnPrefix + Opt.CreateOnColumnName);
        cmd.Parameters.AddWithValue("$CreatedOn_Value", utcNow);

        cmd.Parameters.AddWithValue("$UpdatedBy_Column", Opt.ColumnPrefix + Opt.UpdatedByColumnName);
        cmd.Parameters.AddWithValue("$UpdatedBy_Value", User);

        cmd.Parameters.AddWithValue("$UpdatedOn_Column", Opt.ColumnPrefix + Opt.UpdatedOnColumnName);
        cmd.Parameters.AddWithValue("$UpdatedOn_Value", utcNow);


        cmd.Parameters.AddWithValue("$table_name", Opt.DefaultTableName);


        // INSERT INTO table_name (column1, column2, column3, ...)
        // VALUES(value1, value2, value3, ...);
        var sql = $@"INSERT INTO $table_name
                            ( $Key_Column,
                            $ValueType_Column,
                            $Value_Column,
                            $CreatedBy_Column,
                            $CreatedOn_Column,
                            $UpdatedBy_Column,
                            $UpdatedOn_Column)
                    VALUES ($Key_Value,
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

    private SqliteCommand getCommandForUpcert(string key, string valueType, string value, string User, SqliteConnection Db, KeyValueSqlLiteOptions Opt)
    {
        var utcNow = DateTime.UtcNow;
        var cmd = Db.CreateCommand();
        cmd.Parameters.AddWithValue("$Key_Column", Opt.ColumnPrefix + Opt.KeyColumnName);
        cmd.Parameters.AddWithValue("$Key_Value", key);

        cmd.Parameters.AddWithValue("$ValueType_Column", Opt.ColumnPrefix + Opt.TypeValueColumnName);
        cmd.Parameters.AddWithValue("$ValueType_Value", valueType);

        cmd.Parameters.AddWithValue("$Value_Column", Opt.ColumnPrefix + Opt.TypeValueColumnName);
        cmd.Parameters.AddWithValue("$Value_Value", value);

        cmd.Parameters.AddWithValue("$UpdatedBy_Column", Opt.ColumnPrefix + Opt.UpdatedByColumnName);
        cmd.Parameters.AddWithValue("$UpdatedBy_Value", User);

        cmd.Parameters.AddWithValue("$UpdatedOn_Column", Opt.ColumnPrefix + Opt.UpdatedOnColumnName);
        cmd.Parameters.AddWithValue("$UpdatedOn_Value", utcNow);


        cmd.Parameters.AddWithValue("$table_name", Opt.DefaultTableName);


        //  UPDATE table_name
        //  SET column1 = value1, column2 = value2...., columnN = valueN
        //  WHERE[condition];
        var sql = $@"
                BEGIN
                     UPDATE $table_name
                        SET $Value_Column = $Value_Value,
                            $UpdatedBy_Column = $UpdatedBy_Value,
                            $UpdatedOn_Column = unixepoch($UpdatedOn_Value)
                      WHERE $Key_Column = $Key_Value
                        AND $ValueType_Column = $ValueType_Value; Go;
                    int changed = changes();
               IIF(changed < 1, 
                     INSERT INTO $table_name
                            ( $Key_Column,
                            $ValueType_Column,
                            $Value_Column,
                            $CreatedBy_Column,
                            $CreatedOn_Column,
                            $UpdatedBy_Column,
                            $UpdatedOn_Column)
                    VALUES ($Key_Value,
                            $ValueType_Value,
                            $Value_Value,
                            $CreatedBy_Value,
                            unixepoch($CreatedOn_Value),
                            $UpdatedBy_Value,
                            unixepoch($UpdatedOn_Value)
                             )
                        , );
                END
";
                

        cmd.CommandText = sql;

        return cmd;

    }



}
