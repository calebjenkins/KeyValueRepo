using Calebs.Data.KeyValueRepo.SqlServer;

namespace KeyValueTests;

public class SqlServerOptionsTests
{
    [Fact]
    public void defaultValues()
    {
        KeyValueSqlServerOptions options = new KeyValueSqlServerOptions();
        
        options.ConnString.Should().BeEmpty();
        options.DefaultTableName.Should().Be("KeyValueRepo");
        options.ColumnPrefix.Should().BeEmpty();
        options.ValidateDataSchemaOnStart.Should().BeTrue();
        
        options.CreateTableIfMissing.Should().BeTrue();
        options.KeyValueColumnName.Should().Be("Key");
        options.ValueColumnName.Should().Be("Value");
        options.TypeValueColumnName.Should().Be("Type");

        options.UseAuditFields.Should().BeTrue();
        options.CreatedByColumnName.Should().Be("CreatedBy");
        options.CreateOnColumnName.Should().Be("CreatedOn");
        options.UpdatedByColumnName.Should().Be("UpdatedBy");
        options.UpdateOnColumnName.Should().Be("UpdatedOn");

        options.NonDefaultTableMapping.Count.Should().Be(0);
    }

    [Fact]
    public void OptionsHoldNewValues()
    {
        KeyValueSqlServerOptions options = new KeyValueSqlServerOptions()
        {
            ConnString = "Hello",
            DefaultTableName = "TableName",
            ColumnPrefix = "Prefix",
            ValidateDataSchemaOnStart = false,
            CreateTableIfMissing = false,
            KeyValueColumnName = "kColumn",
            ValueColumnName = "vColumn",
            TypeValueColumnName = "tColumn",
            UseAuditFields = false,
            CreatedByColumnName = "cbColumn",
            CreateOnColumnName = "coColumn",
            UpdatedByColumnName = "ubColumn",
            UpdateOnColumnName = "uoColumn"
        };
        options.NonDefaultTableMapping.Add("typeName", "typeColumn");


        options.ConnString.Should().Be("Hello");
        options.DefaultTableName.Should().Be("TableName");
        options.ColumnPrefix.Should().Be("Prefix");
        options.ValidateDataSchemaOnStart.Should().BeFalse();

        options.CreateTableIfMissing.Should().BeFalse();
        options.KeyValueColumnName.Should().Be("kColumn");
        options.ValueColumnName.Should().Be("vColumn");
        options.TypeValueColumnName.Should().Be("tColumn");

        options.UseAuditFields.Should().BeFalse();
        options.CreatedByColumnName.Should().Be("cbColumn");
        options.CreateOnColumnName.Should().Be("coColumn");
        options.UpdatedByColumnName.Should().Be("ubColumn");
        options.UpdateOnColumnName.Should().Be("uoColumn");

        options.NonDefaultTableMapping.Count.Should().Be(1);
        options.NonDefaultTableMapping["typeName"].Should().Be("typeColumn");
    }
}
