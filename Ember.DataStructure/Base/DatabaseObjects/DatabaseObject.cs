using Ember.DataSchemaManager.DataSchemas;
using System.Collections.ObjectModel;
using System.Reflection;

namespace Ember.DataStructure.Base.DatabaseObjects;

public class DatabaseObject 
{
    public DatabaseObject()
    {

    }
    public string ExtractObjectName(string ObjectClassName)
    {
        return ObjectClassName.Substring(ObjectClassName.LastIndexOf("_") + 1);
    }
}
