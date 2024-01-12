using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Ember.DataSchemaManager.DataSchemas.TableSchema;

namespace Ember.DataSchemaManager.DataSchemas;

public enum DatabaseProviderEnum
{
    SQL,
    SqlServer,
    PostgreSql,
    MySql,
    Sqlite,
}
public class DataSchema
{
    public TableSchema TableSchema { get; set; }
    //Database Informations
    public String DatabaseName { get; set; }
    public String DatabaseScript { get; set; }
    public String DatabaseVersion { get; set; } = "1.0"; // TODO : a global variable to control versions. maybe it should be across all DBs
    public DatabaseProviderEnum DatabaseProvider { get; set; }
    // ...
    public DataSchema(String DatabaseName,DatabaseProviderEnum DatabaseProvider)
    {
        this.DatabaseName = DatabaseName;
        this.DatabaseProvider = DatabaseProvider;
        this.TableSchema = new TableSchema();
    }
    public void Create(String TableName, TableBluePrintCallBack TableBluePrint)
    {
        TableSchema.Create(TableName, TableBluePrint);
    }
    public void DropTable(String TableName)
    {
        TableSchema.Drop(TableName);
    }
    public Boolean HasTable(String TableName)
    {
        return TableSchema.HasTable(TableName);
    }
}
