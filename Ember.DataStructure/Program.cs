using Ember.DataStructure.Base;
using Ember.DataStructure.Base.DatabaseObjects;
using Ember.Transcription;
using Ember.DataSchemaManager.DataSchemas;
using Ember.DataStructure.Database;
using Ember.DataStructure.Migrations.M_02_Tables;
using System.Reflection;
using System.Net.Quic;
using System.Collections.ObjectModel;

namespace Ember.DataStructure;

public class Init
{
    public Init()
    {
        var t = 3;
    }
    public static String Main()
    {
        DatabaseObjectScrapper Scrapper = new DatabaseObjectScrapper();
        DataSchema Schema = new DataSchema();

        var MigrationTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(x => x.GetTypes())
            .Where(x => typeof(IMigratablesDictionary).IsAssignableFrom(x))
            .Where(x => x.Namespace!.StartsWith($"{nameof(Ember)}.{nameof(DataStructure)}.{nameof(Migrations)}"))
            .Where(x => x.IsInterface == false)
            .OrderBy(x => x.Namespace)
            .ToList();
        foreach (Type type in MigrationTypes)
        {
            IMigratablesDictionary Migration = (IMigratablesDictionary)Activator.CreateInstance(type)!;
            var t = type.GetType().Name;
            Migration.Schema = Schema;
            Migration.Down();
            Migration.Up();
        }
        Transcriber Transcriber = new Transcriber(Schema, SqlTypeEnum.SqlServer);
        String GeneratedScript = Transcriber.Transcribe();
        return GeneratedScript; //"a generated database script ... hopefully"
    }

    public static String Second()
    {
        DatabaseObjectScrapper Scrapper = new DatabaseObjectScrapper();
        DataSchema Schema = new DataSchema();
        ObservableCollection<Object> DataSchemaList = new ObservableCollection<Object>();

        var MigrationTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(x => x.GetTypes())
            .Where(x => typeof(IMigratablesDictionary).IsAssignableFrom(x))
            .Where(x => x.Namespace!.StartsWith($"{nameof(Ember)}.{nameof(DataStructure)}.{nameof(Migrations)}"))
            .Where(x => x.IsInterface == false)
            .OrderBy(x => x.Namespace)
            .ToList();
        foreach (Type type in MigrationTypes)
        {
            IMigratablesDictionary DBObject = (IMigratablesDictionary)Activator.CreateInstance(type)!;
            DBObject.Schema = Schema;
            DBObject.Down();
            DBObject.Up();
            foreach (PropertyInfo Property in type.GetProperties().Where(Property => Property.PropertyType.IsSubclassOf(typeof(DataSchema))).ToList())
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
