﻿namespace Calebs.Data.KeyValueRepo.SqlLite;

public class KeyValueSqlLiteOptions
{
    public string ConnectionString { get; set; } = "Data Source=./KeyValueDatabase.db";
    public bool ValidateSchemaOnStartUp { get; set; } = true;
    public bool TrackHistory { get; set; } = false;

    public string DefaultTableName { get; set; } = "KeyValueRepo";
    public string ColumnPrefix { get; set; } = string.Empty;
    public string KeyColumnName { get; set; } = "Key";
    public string TypeColumnName { get; set; } = "Type";
    public string ValueColumnName { get; set; } = "Value";

    public string CreateByColumnName { get; set; } = "CreatedBy";
    public string CreateOnColumnName { get; set; } = "CreatedOn";
    public string UpdatedByColumnName { get; set; } = "UpdatedBy";
    public string UpdatedOnColumnName { get; set; } = "UpdatedOn";
}
