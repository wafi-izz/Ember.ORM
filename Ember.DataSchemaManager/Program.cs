using Ember.DataSchemaManager.DataSchemas;
using Ember.DataSchemaManager.Dictionaries;
using System.Collections.ObjectModel;
using System.Reflection;

namespace Ember.DataSchemaManager;

internal class Program
{
    public Program()
    {
        DataSchema Schema = new DataSchema();
        ObservableCollection<Object> DataSchemaList = new ObservableCollection<Object>();

        List<Assembly> MigrationList = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(x => x.GetTypes())
            .Where(x => typeof(IMigratablesDictionary).IsAssignableFrom(x))
            .Where(x => x.Namespace!.StartsWith($"{nameof(Ember)}.{nameof(DataStructure)}.{nameof(Migrations)}"))
            .Where(x => x.IsInterface == false)
            .OrderBy(x => x.Namespace)
            .ToList();
        foreach (Assembly Migrate in MigrationList)
        {
            IMigratablesDictionary DBObject = (IMigratablesDictionary)Activator.CreateInstance(Migrate.GetType())!;
            DBObject.Schema = Schema;
            DBObject.Down();
            DBObject.Up();
            foreach (PropertyInfo Property in Migrate.GetType().GetProperties().Where(Property => Property.PropertyType.IsSubclassOf(typeof(DataSchema))).ToList())
            {
                DataSchemaList.Add(Property.GetValue(DBObject)!);
            }
        }
        List<String> TranscribedSchema = new List<string>();
        foreach (var DataSchema in DataSchemaList)
        {
            TranscribedSchema.Add(new Transcriber((DataSchema)DataSchema, SqlTypeEnum.SqlServer).Transcribe());
        }
        var tt = GlobalDataSchema.MyPostgreDBObject;
        return TranscribedSchema.ToString()!; //"a generated database script ... hopefully"
    }
}
