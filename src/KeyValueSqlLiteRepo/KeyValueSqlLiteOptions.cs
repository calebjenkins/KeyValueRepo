namespace Calebs.Data.KeyValueRepo.SqlLite;

public class KeyValueSqlLiteOptions
{
    public string ConnectionString { get; set; } = "Data Source=./KeyValueDatabase.db";
    public bool ValidateSchemaOnStartUp { get; set; } = true;
    public bool CreateTableIfMissing { get; set; } = true;
    public string DefaultTableName { get; set; } = "KeyValueRepo";
    public string ColumnPrefix { get; set; } = string.Empty;
    public string KeyColumnName { get; set; } = "Key";
    public string ValueColumnName { get; set; } = "Value";
    public string TypeValueColumnName { get; set; } = "Type";

    public bool UseAuditFields { get; set; } = true;
    public string CreateByColumnName { get; set; } = "CreatedBy";
    public string CreateOnColumnName { get; set; } = "CreatedOn";
    public string UpdatedByColumnName { get; set; } = "UpdatedBy";
    public string UpdatedOnColumnName { get; set; } = "UpdatedOn";

    /// <summary>
    /// Keys - Type that should be mapped
    /// Value - Name of Table to map to
    /// </summary>
    public Dictionary<string, string> NonDefaultTableMapping { get; set; } = new Dictionary<string, string>();
}

public static class KeyValueSqliteOptionsExtensions
{
    public static IList<string> AllColumnsWithPrefix(this KeyValueSqlLiteOptions Opt)
    {
        var result = new List<string>()
        {
            Opt.ColumnPrefix + Opt.KeyColumnName,
            Opt.ColumnPrefix + Opt.TypeValueColumnName,
            Opt.ColumnPrefix + Opt.ValueColumnName
        };

        if(Opt.UseAuditFields)
        {
            result.Add(Opt.ColumnPrefix + Opt.CreateByColumnName);
            result.Add(Opt.ColumnPrefix + Opt.CreateOnColumnName);
            result.Add(Opt.ColumnPrefix + Opt.UpdatedByColumnName);
            result.Add(Opt.ColumnPrefix + Opt.UpdatedOnColumnName);
        }

        return result;

    }

    public static IList<string> AllTables(this KeyValueSqlLiteOptions Opt)
    {
        var result = new List<string>()
        {
            Opt.DefaultTableName
        };

        foreach(string name in Opt.NonDefaultTableMapping.Values)
        {
            result.Add(name);
        }

        return result;
    }
}
