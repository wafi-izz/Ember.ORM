using Ember.DataSchemaManager.DataSchemas;
using System.Numerics;

namespace Ember.DataStructure.Database;

public static class GlobalDataSchema
{
    public static DataSchema MSSQLDB = new DataSchema("Main1", DatabaseProviderEnum.SqlServer); 
    public static DataSchema PostgreDB = new DataSchema("Helper2", DatabaseProviderEnum.PostgreSql);
    public static DataSchema MySqlDB = new DataSchema("Side3",DatabaseProviderEnum.MySql);
    public static DataSchema SqliteDB = new DataSchema("Backup4",DatabaseProviderEnum.Sqlite);
}