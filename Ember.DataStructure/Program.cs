using Ember.DataSchemaManager.DataSchemas;
using Ember.DataSchemaManager.Dictionaries;
using Ember.DataStructure.Database;
using System.Reflection;
using System.Collections.ObjectModel;
using Ember.Transcription;

namespace Ember.DataStructure;

public class Init
{
    public List<String> GeneratedSchema { get; set; }
    public Init()
    {
        List<Type> MigrationList = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(x => x.GetTypes())
            .Where(x => typeof(IMigratablesDictionary).IsAssignableFrom(x))
            .Where(x => x.Namespace!.StartsWith($"{nameof(Ember)}.{nameof(DataStructure)}.{nameof(Migrations)}"))
            .Where(x => x.IsInterface == false)
            .OrderBy(x => x.Namespace)
            .ToList();
        MigrationList.ForEach(Migration =>
        {
            var DBObject = (IMigratablesDictionary)Activator.CreateInstance(Migration)!;
            DBObject.Down();
            DBObject.Up();
        });
        //foreach (PropertyInfo Property in typeof(GlobalDataSchema).GetProperties().Where(Property => Property.PropertyType.IsSubclassOf(typeof(DataSchema))).ToList())
        ObservableCollection<DataSchema> DataSchemaList = [.. typeof(GlobalDataSchema).GetFields(BindingFlags.Public | BindingFlags.Static).Where(x => typeof(DataSchema).IsAssignableFrom(x.FieldType)).Select(x => (DataSchema)x.GetValue(null)!)];
        GeneratedSchema = [.. DataSchemaList.Select(x => new Transcriber(x).Transcribe())]; //"a generated database script ... Hopefully"
    }
}
