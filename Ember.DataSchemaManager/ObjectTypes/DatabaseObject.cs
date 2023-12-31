
namespace Ember.DataSchemaManager.ObjectTypes;

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
