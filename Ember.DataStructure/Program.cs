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
        DataSchema Schema = new DataSchema();
        ObservableCollection<Object> DataSchemaList = new ObservableCollection<Object>();

        List<Type> MigrationList = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(x => x.GetTypes())
            .Where(x => typeof(IMigratablesDictionary).IsAssignableFrom(x))
            .Where(x => x.Namespace!.StartsWith($"{nameof(Ember)}.{nameof(DataStructure)}.{nameof(Migrations)}"))
            .Where(x => x.IsInterface == false)
            .OrderBy(x => x.Namespace)
            .ToList();
        foreach (Type Migration in MigrationList)
        {
            IMigratablesDictionary DBObject = (IMigratablesDictionary)Activator.CreateInstance(Migration)!;
            DBObject.Schema = Schema;
            DBObject.Down();
            DBObject.Up();
            foreach (PropertyInfo Property in Migration.GetProperties().Where(Property => Property.PropertyType.IsSubclassOf(typeof(DataSchema))).ToList())
            {
                DataSchemaList.Add(Property.GetValue(DBObject)!);
            }
        }
        List<String> TranscribedSchema = new List<String>();
        foreach (var DataSchema in DataSchemaList)
        {
            TranscribedSchema.Add(new Transcriber((DataSchema)DataSchema, SqlTypeEnum.SqlServer).Transcribe());
        }
        var tt = GlobalDataSchema.MyPostgreDBObject;
        GeneratedSchema = TranscribedSchema; //"a generated database script ... Hopefully"
    }
}
