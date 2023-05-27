namespace Calebs.Data.KeyValueRepo.SqlLite;

public static class KeyValueSqliteOptionsExtensions
{
    public static IList<string> AllColumnsWithPrefix(this KeyValueSqlLiteOptions Opt)
    {
        return new List<string>()
        {
            Opt.ColumnPrefix + Opt.KeyColumnName,
            Opt.ColumnPrefix + Opt.TypeValueColumnName,
            Opt.ColumnPrefix + Opt.ValueColumnName,
            Opt.ColumnPrefix + Opt.CreateByColumnName,
            Opt.ColumnPrefix + Opt.CreateOnColumnName,
            Opt.ColumnPrefix + Opt.UpdatedByColumnName,
            Opt.ColumnPrefix + Opt.UpdatedOnColumnName
        };
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
