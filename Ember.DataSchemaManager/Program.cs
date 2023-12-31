using Ember.DataSchemaManager.DataSchemas;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ember.DataSchemaManager;

internal class Program
{
    public Program()
    {
        DataSchema Schema = new DataSchema();
        ObservableCollection<Object> DataSchemaList = new ObservableCollection<Object>();

        List<Assembly> MigrationTypes = AppDomain.CurrentDomain.GetAssemblies()
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
