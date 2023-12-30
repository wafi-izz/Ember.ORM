using Ember.DataSchemaManager.DataSchemas;
using Ember.DataStructure.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ember.DataStructure.Database;

public class D_01_Database : DataSchema
{
    public String DatabaseName { get; set; }
    public String DatabaseVersion { get; set; } = "1.0";
    public String DatabaseType { get; set; }

    public D_01_Database()
    {

    }    
}

public class D_02_PostgreDB : DataSchema
{
    public String DatabaseName { get; set; }
    public String DatabaseVersion { get; set; } = "1.0";
    public String DatabaseType { get; set; }

    public D_02_PostgreDB()
    {

    }    
}

public static class GlobalDataSchema
{
    public static DataSchema MyPostgreDBObject = new DataSchema(); // Pass All The Required Parameters.
    public static DataSchema MyMSSQLDBObject = new DataSchema(); // Pass All The Required Parameters.
    public static DataSchema MySqliteDBObject = new DataSchema(); // Pass All The Required Parameters.
}

