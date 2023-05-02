namespace Calebs.Data.KeyValueRepo.SqlServer;

public class KeyValueSqlServerOptions
{
    public string ConnString { get; set; }
    public bool CreateTableIfMissing { get; set; } = true;
    public string DefaultTableName { get; set; } = "KeyValueRepo";
    public string KeyValueColumnName { get; set; } = "Key";
    public string ValueColumnName { get; set; } = "Value";
    public string TypeValueColumnName { get;set; } = "Type";
    
    public bool UseAuditFields { get; set; } = true;
    public string CreatedByColumnName { get; set; } = "CreatedBy";
    public string CreateOnColumnName { get; set; } = "CreatedOn";
    public string UpdatedByColumnName { get; set; } = "UpdatedBy";
    public string UpdateOnColumnName { get; set; } = "UpdatedOn";
}
