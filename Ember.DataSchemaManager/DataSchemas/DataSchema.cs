using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Ember.DataSchemaManager.DataSchemas.TableSchema;

namespace Ember.DataSchemaManager.DataSchemas;

public enum DatabaseProviderType
{
    SQL,
    SqlServer,
    PostgreSql,
    MySql,
    Sqlite,
}
//TODO: find a way to make exceptions whether with try and catch or something else.
public class DataSchema
{
    public TableSchema TableSchema { get; set; }
    //Database Informations
    public String ConnectionName { get; set; }
    public String? DatabaseScript { get; set; }
    public String DatabaseVersion { get; set; } = "1.0"; //TODO: a global variable to control versions. maybe it should be across all DBs
    public DatabaseProviderType DatabaseProvider { get; set; }
    public DataSchema(String ConnectionName)
    {
        if (ConfigurationManager.ConnectionStrings[ConnectionName] == null) throw new ArgumentException("not found Connection String");
        if (!Enum.IsDefined(typeof(DatabaseProviderType), ConfigurationManager.ConnectionStrings[ConnectionName].ProviderName)) throw new ArgumentException("not found Provider Name");
        this.ConnectionName = ConnectionName;
        this.DatabaseProvider = (DatabaseProviderType)Enum.Parse(typeof(DatabaseProviderType), ConfigurationManager.ConnectionStrings[ConnectionName].ProviderName, true);
        this.TableSchema = new TableSchema();
    }
    public void Create(String TableName, TableBluePrintCallBack TableBluePrint)
    {
        TableSchema.Create(TableName, TableBluePrint);
    }
    public void Alter(String TableName,String TableRename, TableBluePrintCallBack TableBluePrint)
    {
        TableSchema.Alter(TableName,TableRename, TableBluePrint);
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
