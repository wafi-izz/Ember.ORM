using Ember.DataSchemaManager.DataSchemas;
using System.Numerics;

namespace Ember.DataStructure.Database;

public static class GlobalDataSchema
{
    public static DataSchema PostgreDB = new DataSchema("Main1", DatabaseProviderEnum.PostgreSql);
    public static DataSchema MSSQLDB = new DataSchema("Helper2",DatabaseProviderEnum.SqlServer); 
    public static DataSchema MySqlDB = new DataSchema("Side3",DatabaseProviderEnum.Sqlite);
    public static DataSchema SqliteDB = new DataSchema("Backup4",DatabaseProviderEnum.Sqlite);
}