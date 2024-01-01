using Ember.DataSchemaManager.DataSchemas;
using System.Numerics;

namespace Ember.DataStructure.Database;

public static class GlobalDataSchema
{
    public static DataSchema MyPostgreDB = new DataSchema();
    public static DataSchema MyMSSQLDB = new DataSchema(); 
    public static DataSchema MySqliteDB = new DataSchema();
}