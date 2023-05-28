
using System;
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

    public Task<T?> Get<T>(string key) where T : class
    {
        throw new NotImplementedException();
    }

    public Task<IList<T>> GetAll<T>() where T : class
    {
        throw new NotImplementedException();
    }

    public Task Update<T>(string key, T value) where T : class
    {
        throw new NotImplementedException();
    }

    private async Task<bool> insertRecord(string key, string valueType, string value, string user)
    {
        return false;
    }
    private async Task<bool> selectRecord(string key, string valueType)
    { 
        // Only one table ever used - right now
        var tableName = _options.DefaultTableName;

        var sql = "";
  
        try
        {
            _db.ConfirmOpen();
            using var connection = _db;
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("$table_name", tableName);

            using var reader = await command.ExecuteReaderAsync();
            while (reader.Read())
            {
                var name = reader.GetString(0);
               
            }
            return false;
        }
        catch (SqliteException ex)
        {
            _logger.LogError("Error validating Default Table: {0}", ex.Message);
            throw;
        }
    }

    private SqliteCommand getCommandForSelect(string key, string valueType, SqliteConnection Db, KeyValueSqlLiteOptions Opt)
    {
        var cmd = Db.CreateCommand();
        cmd.Parameters.AddWithValue("$Key_Column", Opt.ColumnPrefix + Opt.KeyColumnName);
        cmd.Parameters.AddWithValue("$Key_Value", key);

        var sql = $@"SELECT $Key_Column,
                            {Opt.ColumnPrefix + Opt.TypeValueColumnName},
                            {Opt.ColumnPrefix + Opt.ValueColumnName},
                            {Opt.ColumnPrefix + Opt.CreateByColumnName},
                            {Opt.ColumnPrefix + Opt.CreateOnColumnName},
                            {Opt.ColumnPrefix + Opt.UpdatedByColumnName},
                            {Opt.ColumnPrefix + Opt.UpdatedOnColumnName}

                    FROM $table_name;

                    WHERE $Key_Column = $Key_Value
                      AND {_options.ColumnPrefix + _options.TypeValueColumnName} = {valueType}
                      AND {_options.ColumnPrefix + _options.UpdatedOnColumnName} = (SELECT MAX({_options.ColumnPrefix + _options.UpdatedOnColumnName}) FROM yourTable WHERE orderNo = [data].orderNo));";

        return cmd;

    }
}